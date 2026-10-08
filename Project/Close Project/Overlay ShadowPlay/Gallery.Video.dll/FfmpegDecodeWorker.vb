Option Strict On
Option Explicit On
Option Infer On

' FfmpegDecodeWorker.vb — subprocess decode (design doc §3.2/§3.3).
'
' ONE worker instance PER GENERATION (open or seek):
'   video:  ffmpeg -nostdin -v info -ss <t> -i <file> -map 0:v:0
'                     -vf showinfo -fps_mode passthrough
'                     -f rawvideo -pix_fmt bgra pipe:1
'   audio:  ffmpeg -nostdin -v error -ss <t> -i <file> -map 0:a:0
'                     -f s16le -ar <rate> -ac <ch> pipe:1
'
' WHY -fps_mode passthrough (2026-09-12, measured W1): the rawvideo muxer's
' default vsync (cfr) DUPLICATES frames AFTER the filtergraph whenever the
' µs-quantized source cadence drifts under the target rate (240fps file:
' 966 stdout frames vs 961 showinfo lines). Duplicate frames carry no
' showinfo line, so TakePtsTicks blocked the stdout reader for its full 2s
' deadline per duplicate (5×2s = 10s per 4s file) and broke the 1:1
' frame↔PTS pairing. passthrough keeps stdout frames == showinfo lines
' (961/961 measured) and raised worker delivery 47.9 → 99.4 fps.
' WHY subprocess (repo precedent, no FFmpeg library bindings exist anywhere —
' see design doc §2): matches FFmpegProcessHost/LiveMuxSession discipline.
' WHY showinfo: rawvideo carries no timestamps; showinfo prints pts_time per
' frame on stderr (parsed like FFmpegStderrParser does). With -ss BEFORE -i
' output timestamps start ≈ 0 at the seek target, so
'   frame absolute PTS = seekBaseTicks + pts_time.
'
' Lifecycle: Start() spawns + launches reader threads; RequestStop() kills
' both processes (entire tree) and joins; EOF is reported via events. All
' pipes drained on dedicated threads (FFmpegLocator 64KB-stderr lesson).
' Thread-safe: queue writes are the only external mutations.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Threading

Namespace Gallery.Video

    ''' <summary>Decode configuration for one generation.</summary>
    Public NotInheritable Class DecodeGenerationConfig
        Public Property FilePath As String = ""
        Public Property FfmpegExe As String = ""
        Public Property Generation As Long
        ''' <summary>Seek base in seconds (0 = open from start). Input-side -ss.</summary>
        Public Property SeekSeconds As Double = 0.0
        Public Property VideoEnabled As Boolean = True
        Public Property AudioEnabled As Boolean = False
        Public Property FrameWidth As Integer
        Public Property FrameHeight As Integer
        Public Property FrameRate As Double = 30.0
        Public Property AudioSampleRate As Integer = 48000
        Public Property AudioChannels As Integer = 2
        Public Property VideoQueue As FrameQueue
        Public Property AudioBuffer As AudioPcmBuffer
        ''' <summary>Process kill patience (Join patience mirrors repo L12 style).</summary>
        Public Property StopTimeoutMs As Integer = 3000
    End Class

    Public NotInheritable Class FfmpegDecodeWorker
        Implements IDisposable

        Private ReadOnly _cfg As DecodeGenerationConfig
        Private _videoProc As Process
        Private _audioProc As Process

        ' ★ W2 audio pacing: 1 ms timer quantum for the pump's sleeps (see
        ' AudioStdoutLoop) — same winmm pattern as CaptureSession's CFR fix.
        <System.Runtime.InteropServices.DllImport("winmm.dll")>
        Private Shared Function timeBeginPeriod(period As UInteger) As Integer
        End Function
        <System.Runtime.InteropServices.DllImport("winmm.dll")>
        Private Shared Function timeEndPeriod(period As UInteger) As Integer
        End Function
        Private _threads As New List(Of Thread)()
        Private _stopState As Integer = 0          ' 0 running, 1 stop requested
        Private _videoEofState As Integer = 0      ' 0 not eof
        Private _audioEofState As Integer = 0
        Private _faulted As GalleryVideoFault = Nothing

        ' PTS plumbing: showinfo (stderr) → ordered pts queue → frame assembler
        Private ReadOnly _ptsLock As New Object()
        Private ReadOnly _ptsQueue As New Queue(Of Long)() ' 100-ns ticks, relative to seek base
        Private _lastPtsTicks As Long = -1
        Private _frameSeq As Long = 0

        ' Metrics (design doc §6 — measured, reported, never hidden)
        Private _framesDecoded As Long
        Private _ptsWaitTimeouts As Long
        Private _audioBytesDecoded As Long

        Public Sub New(cfg As DecodeGenerationConfig)
            If cfg Is Nothing Then Throw New ArgumentNullException(NameOf(cfg))
            If cfg.VideoQueue Is Nothing Then Throw New ArgumentNullException(NameOf(cfg.VideoQueue))
            _cfg = cfg
        End Sub

        Public ReadOnly Property Generation As Long
            Get
                Return _cfg.Generation
            End Get
        End Property

        ''' <summary>Video ffmpeg PID — diagnostics + decode-liveness test seam
        ''' (F4 stall tests must target the EXACT process, never a name-based
        ''' lookup). -1 before spawn, after exit, or when unavailable.</summary>
        Public ReadOnly Property VideoProcessId As Integer
            Get
                Dim p = _videoProc
                If p Is Nothing Then Return -1
                Try
                    If p.HasExited Then Return -1
                    Return p.Id
                Catch
                    Return -1
                End Try
            End Get
        End Property

        Public ReadOnly Property VideoEof As Boolean
            Get
                Return Volatile.Read(_videoEofState) <> 0
            End Get
        End Property

        Public ReadOnly Property AudioEof As Boolean
            Get
                Return _cfg.AudioEnabled = False OrElse Volatile.Read(_audioEofState) <> 0
            End Get
        End Property

        Public ReadOnly Property FramesDecoded As Long
            Get
                Return Volatile.Read(_framesDecoded)
            End Get
        End Property

        Public ReadOnly Property AudioBytesDecoded As Long
            Get
                Return Volatile.Read(_audioBytesDecoded)
            End Get
        End Property

        Public ReadOnly Property Fault As GalleryVideoFault
            Get
                Return _faulted
            End Get
        End Property

        ''' <summary>Fired when the VIDEO stream reaches EOF (clean end).</summary>
        Public Event VideoEofReached(sender As FfmpegDecodeWorker)

        ''' <summary>Fired on fatal worker error (process spawn fail / immediate exit).</summary>
        Public Event FaultDetected(sender As FfmpegDecodeWorker, fault As GalleryVideoFault)

        ' ---- spawn ----

        Public Sub Start()
            Dim gen = _cfg.Generation
            Dim seekStr = _cfg.SeekSeconds.ToString("0.###", CultureInfo.InvariantCulture)

            If _cfg.VideoEnabled Then
                Dim frameBytes = _cfg.FrameWidth * _cfg.FrameHeight * 4
                Dim vArgs = $"-nostdin -hide_banner -v info -ss {seekStr} " &
                            $"-i ""{_cfg.FilePath}"" -map 0:v:0 -vf showinfo -fps_mode passthrough " &
                            $"-f rawvideo -pix_fmt bgra pipe:1"
                _videoProc = Spawn(_cfg.FfmpegExe, vArgs)
                If _videoProc Is Nothing Then
                    FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.BackendMissing,
                                                   $"video decode spawn failed (gen {gen})"))
                    Return
                End If
                StartThread(AddressOf VideoStdoutLoop, $"gv-dec-v-{gen}")
                StartThread(AddressOf VideoStderrLoop, $"gv-pts-{gen}")
            End If

            If _cfg.AudioEnabled Then
                Dim aArgs = $"-nostdin -hide_banner -v error -ss {seekStr} " &
                            $"-i ""{_cfg.FilePath}"" -map 0:a:0 " &
                            $"-f s16le -ar {_cfg.AudioSampleRate} -ac {_cfg.AudioChannels} pipe:1"
                _audioProc = Spawn(_cfg.FfmpegExe, aArgs)
                If _audioProc Is Nothing Then
                    FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.BackendMissing,
                                                   $"audio decode spawn failed (gen {gen})"))
                    Return
                End If
                StartThread(AddressOf AudioStdoutLoop, $"gv-dec-a-{gen}")
            End If
        End Sub

        Private Function Spawn(exe As String, args As String) As Process
            Try
                Dim psi As New ProcessStartInfo With {
                    .FileName = exe,
                    .Arguments = args,
                    .UseShellExecute = False,
                    .CreateNoWindow = True,
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .RedirectStandardInput = True
                }
                Return Process.Start(psi)
            Catch
                ' Fault reporting belongs to FaultOut (single raiser) — a bare
                ' Nothing return keeps the FaultDetected event contract intact.
                Return Nothing
            End Try
        End Function

        Private Sub StartThread(entry As ThreadStart, name As String)
            Dim t As New Thread(entry) With {.IsBackground = True, .Name = name}
            _threads.Add(t)
            t.Start()
        End Sub

        ' ---- video stdout: raw frames ----

        Private Sub VideoStdoutLoop()
            Dim frameBytes = _cfg.FrameWidth * _cfg.FrameHeight * 4
            Dim buf(frameBytes - 1) As Byte
            Dim filled = 0

            Try
                Using stdout = _videoProc.StandardOutput.BaseStream
                    While Volatile.Read(_stopState) = 0
                        Dim need = frameBytes - filled
                        Dim read = stdout.Read(buf, filled, need)
                        If read <= 0 Then
                            ' EOF (or process died): partial frame discarded.
                            Exit While
                        End If
                        filled += read

                        If filled = frameBytes Then
                            filled = 0
                            OnCompleteFrame(buf)
                        End If
                    End While
                End Using
            Catch ex As Exception
                If Volatile.Read(_stopState) = 0 Then
                    FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.InternalError,
                                                   "video stdout read", ex.GetType().Name))
                End If
            Finally
                If Interlocked.CompareExchange(_videoEofState, 1, 0) = 0 Then
                    RaiseEvent VideoEofReached(Me)
                End If
                ' Completed-but-not-EOF exit with a half frame and zero frames
                ' overall is the corrupt-file signature — surface it once.
                If Volatile.Read(_stopState) = 0 AndAlso Volatile.Read(_framesDecoded) = 0 Then
                    FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.CorruptFile,
                                                   "video stream produced 0 complete frames"))
                End If
            End Try
        End Sub

        Private Sub OnCompleteFrame(buf As Byte())
            Dim ptsTicks = TakePtsTicks()
            ' Absolute presentation time = seek base + showinfo-relative pts
            ' (with -ss BEFORE -i, output pts starts ≈ 0 at the seek target —
            ' design doc §3.6). start_time≠0 handling is a measured follow-up;
            ' product outputs carry start_time=0 (HANDOFF §3 evidence).
            Dim absolutePts = PlaybackClock.SecondsToTicks(_cfg.SeekSeconds) + ptsTicks

            Dim pixels(_cfg.FrameWidth * _cfg.FrameHeight * 4 - 1) As Byte
            Array.Copy(buf, pixels, pixels.Length)

            Dim frame = New PlaybackFrame(pixels, _cfg.FrameWidth, _cfg.FrameHeight,
                                          absolutePts, _cfg.Generation, _frameSeq)
            _frameSeq += 1L
            _framesDecoded += 1

            ' Ownership transfers to the queue. False = closed/stale/timeout —
            ' the queue already disposed the frame (one-dispose invariant).
            _cfg.VideoQueue.TryEnqueue(frame, 5000)
        End Sub

        ' ---- video stderr: showinfo pts ----

        Private Sub VideoStderrLoop()
            Try
                Using stderr = _videoProc.StandardError
                    While True
                        Dim line As String = stderr.ReadLine()
                        If line Is Nothing Then Exit While
                        If Volatile.Read(_stopState) <> 0 Then Exit While
                        ParseShowinfoLine(line)
                    End While
                End Using
            Catch
                ' Drained or killed — normal on stop.
            End Try
        End Sub

        ''' <summary>Extract pts_time from a showinfo line into the pts queue.
        ''' showinfo format: "...] n:   0 pts:...  pts_time:0.041667 ..."</summary>
        Friend Sub ParseShowinfoLine(line As String)
            If line Is Nothing Then Return
            Dim idx = line.IndexOf("pts_time:", StringComparison.Ordinal)
            If idx < 0 Then Return
            Dim startIdx = idx + "pts_time:".Length
            Dim endIdx = startIdx
            While endIdx < line.Length AndAlso (Char.IsDigit(line(endIdx)) OrElse line(endIdx) = "."c OrElse line(endIdx) = "-"c)
                endIdx += 1
            End While

            Dim secsStr = line.Substring(startIdx, endIdx - startIdx)
            Dim secs As Double
            If Double.TryParse(secsStr, NumberStyles.Float, CultureInfo.InvariantCulture, secs) Then
                Dim ticks = PlaybackClock.SecondsToTicks(secs)
                SyncLock _ptsLock
                    _ptsQueue.Enqueue(ticks)
                    Monitor.Pulse(_ptsLock)
                End SyncLock
            End If
        End Sub

        ''' <summary>Pop one pts (relative to seek base) for the next completed
        ''' frame. showinfo usually lands first; bounded wait keeps ordering
        ''' deterministic. Fallback: extrapolate from the previous frame —
        ''' counted, never silent (§6 measured-results culture).</summary>
        Private Function TakePtsTicks() As Long
            Dim deadline = DateTime.UtcNow.AddSeconds(2)
            SyncLock _ptsLock
                While _ptsQueue.Count = 0
                    If Volatile.Read(_stopState) <> 0 Then
                        Return If(_lastPtsTicks < 0, 0L, _lastPtsTicks)
                    End If
                    Dim remaining = CInt((deadline - DateTime.UtcNow).TotalMilliseconds)
                    If remaining <= 0 Then
                        _ptsWaitTimeouts += 1
                        Exit While
                    End If
                    Monitor.Wait(_ptsLock, Math.Min(remaining, 50))
                End While

                If _ptsQueue.Count > 0 Then
                    _lastPtsTicks = _ptsQueue.Dequeue()
                    Return _lastPtsTicks
                End If

                ' Extrapolate: previous + frame interval (counted fallback).
                Dim interval = PlaybackClock.SecondsToTicks(1.0 / Math.Max(1.0, _cfg.FrameRate))
                _lastPtsTicks = If(_lastPtsTicks < 0, 0L, _lastPtsTicks + interval)
                Return _lastPtsTicks
            End SyncLock
        End Function

        ' ---- audio stdout: raw s16le ----

        Private Sub AudioStdoutLoop()
            ' ★ W2 audio backpressure: the audio ffmpeg decodes the whole file
            ' far faster than real time; an unpaced pump makes the drop-oldest
            ' ring discard UNPLAYED content and the device starve after the
            ' first ring (measured: ring→0, underruns=37, counter frozen).
            ' Three rules keep content continuous: SMALL chunks (~25 ms — a
            ' 64 KB read = 682 ms of audio lands past the 200 ms ring and
            ' drop-oldest eats the middle), a 1 ms timer quantum (Thread.Sleep
            ' honours the ~15.6 ms system quantum otherwise — the capture
            ' session's CFR PACING FIX lesson), and a consumer-reference
            ' throttle (writer stays ≤200 ms of audio ahead of what the device
            ' actually consumed — BytesWritten − BytesRead).
            Dim bytesPerSec = Math.Max(1, _cfg.AudioSampleRate * Math.Max(1, _cfg.AudioChannels) * 2)
            Dim buf(Math.Max(1920, bytesPerSec \ 40) - 1) As Byte   ' ~25 ms
            Dim bytesWritten As Long = 0
            timeBeginPeriod(1UI)
            Try
                Using stdout = _audioProc.StandardOutput.BaseStream
                    While Volatile.Read(_stopState) = 0
                        Dim read = stdout.Read(buf, 0, buf.Length)
                        If read <= 0 Then Exit While
                        Dim aheadBytes = _cfg.AudioBuffer.BytesWritten - _cfg.AudioBuffer.BytesRead
                        If aheadBytes + read > bytesPerSec \ 5 Then
                            Dim behindMs = ((aheadBytes + read - bytesPerSec \ 5) * 1000.0) / bytesPerSec
                            Dim waitMs = CInt(Math.Min(behindMs, 20.0))
                            If waitMs > 0 Then Thread.Sleep(waitMs)
                        End If
                        If Not _cfg.AudioBuffer.Write(buf, 0, read) Then Exit While
                        bytesWritten += read
                        _audioBytesDecoded += read
                    End While
                End Using
            Catch
                ' Killed mid-read on stop/seek — normal.
            Finally
                Interlocked.CompareExchange(_audioEofState, 1, 0)
                timeEndPeriod(1UI)
            End Try
        End Sub

        ' ---- stop ----

        Public Sub RequestStop()
            If Interlocked.CompareExchange(_stopState, 1, 0) <> 0 Then Return

            WakePtsWaiters()
            KillProcess(_videoProc)
            KillProcess(_audioProc)

            ' Join readers (bounded — threads exit on _stopState or pipe close).
            For Each t In _threads
                Try
                    If t.IsAlive Then t.Join(_cfg.StopTimeoutMs)
                Catch
                End Try
            Next

            DisposeProcess(_videoProc)
            DisposeProcess(_audioProc)
            _videoProc = Nothing
            _audioProc = Nothing
        End Sub

        Private Sub WakePtsWaiters()
            SyncLock _ptsLock
                Monitor.PulseAll(_ptsLock)
            End SyncLock
        End Sub

        Private Sub KillProcess(p As Process)
            If p Is Nothing Then Return
            Try
                If Not p.HasExited Then p.Kill(entireProcessTree:=True)
            Catch
            End Try
        End Sub

        Private Sub DisposeProcess(ByRef p As Process)
            If p Is Nothing Then Return
            Try
                p.Dispose()
            Catch
            End Try
            p = Nothing
        End Sub

        Private Sub FaultOut(fault As GalleryVideoFault)
            If _faulted IsNot Nothing Then Return
            _faulted = fault
            WakePtsWaiters()
            RaiseEvent FaultDetected(Me, fault)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            RequestStop()
        End Sub

    End Class

End Namespace
