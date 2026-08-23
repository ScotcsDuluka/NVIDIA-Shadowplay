Option Strict On
Option Explicit On
Option Infer On

' LiveMuxSession.vb — OBS-style live mux: NVENC H.264 + WASAPI PCM piped
' DIRECTLY into one FFmpeg process while recording.
'
' OWNER directive (2026-08-23): record video+audio together, both follow the
' wall clock, both end together — the OBS model. This replaces the
' temp-H.264 + WAV-sidecar + wrap + later-mux pipeline whose intermediate
' files could not carry timestamps (the root of every sync bug we chased).
'
' Architecture:
'   ffmpeg.exe (spawned at record start, ONE process)
'     input 0: \\.\pipe\sp_v_xxx   raw H.264 (CFR paced by CaptureSession)
'     input 1: \\.\pipe\sp_a_xxx   raw PCM s16le (gap-filled wall clock)
'    [input 2: \\.\pipe\sp_m_xxx   mic PCM]
'     output : fragmented MP4 (crash-safe) → +faststart remux at stop
'
' Timeline alignment (the SyncMath model, applied at FEED time):
'   video t0 = first encoded frame (session-start frame fix guarantees t≈0).
'   audio started at t_a; if audio ran BEFORE video t0 → DISCARD that head
'   (old -ss skip); if audio's first data lands AFTER t0 → PAD silence.
'   From then on both pipes advance at wall-clock rate → container-aligned
'   BY CONSTRUCTION. Residual = WASAPI delivery latency (constant, small).
'
' Pipes use the exact pattern proven by the legacy AudioPipe on this very
' machine (named pipe, 1MB buffer, WaitForConnection, FFmpeg as client).

Imports System.Collections.Concurrent
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.IO.Pipes
Imports System.Threading

Namespace CaptureEngine.FFmpegBackend

    ''' <summary>Statistics of a completed live-mux run.</summary>
    Public NotInheritable Class LiveMuxResult
        Public Property Succeeded As Boolean
        Public Property FFmpegExitCode As Integer
        Public Property VideoBytesFed As Long
        Public Property SystemBytesFed As Long
        Public Property MicBytesFed As Long
        Public Property DroppedBytes As Long
        Public Property UsedFaststartRemux As Boolean
        Public Property ErrorMessage As String = ""

        Public Overrides Function ToString() As String
            Return $"LiveMux: ok={Succeeded} exit={FFmpegExitCode} " &
                   $"v={VideoBytesFed:N0}B a={SystemBytesFed:N0}B mic={MicBytesFed:N0}B " &
                   $"dropped={DroppedBytes:N0}B faststart={UsedFaststartRemux}" &
                   If(String.IsNullOrEmpty(ErrorMessage), "", " err=" & ErrorMessage)
        End Function
    End Class

    Public NotInheritable Class LiveMuxSession
        Implements IDisposable

        Private Const PipeBufferBytes As Integer = 1024 * 1024
        Private Const ConnectTimeoutMs As Integer = 15000
        Private Const QueueChunkCap As Integer = 512   ' ★ raised: silence bursts (pre-roll up to 60s) + probe delay headroom

        Private ReadOnly _ffmpegPath As String
        Private ReadOnly _finalPath As String
        Private ReadOnly _fragPath As String
        Private ReadOnly _videoFps As Integer
        Private ReadOnly _separateTracks As Boolean
        Private ReadOnly _sysVolume As Single
        Private ReadOnly _micVolume As Single
        Private ReadOnly _log As Action(Of String)

        ' stream configs (Nothing = stream absent)
        Private ReadOnly _sysRate As Integer
        Private ReadOnly _sysChannels As Integer
        Private ReadOnly _micRate As Integer
        Private ReadOnly _micChannels As Integer

        Private _proc As Process
        Private _video As PipeFeed
        Private _audio As PipeFeed
        Private _mic As PipeFeed
        Private _stderrTask As Task(Of String)
        Private _started As Boolean
        Private _disposed As Boolean

        Public Sub New(ffmpegPath As String,
                       finalOutputPath As String,
                       videoFps As Integer,
                       systemSampleRate As Integer,
                       systemChannels As Integer,
                       micSampleRate As Integer,
                       micChannels As Integer,
                       separateTracks As Boolean,
                       systemVolume As Single,
                       micVolume As Single,
                       Optional log As Action(Of String) = Nothing)
            _ffmpegPath = ffmpegPath
            _finalPath = finalOutputPath
            _fragPath = finalOutputPath & ".frag.mp4"
            _videoFps = Math.Max(1, videoFps)
            _sysRate = systemSampleRate
            _sysChannels = systemChannels
            _micRate = micSampleRate
            _micChannels = micChannels
            _separateTracks = separateTracks
            _sysVolume = systemVolume
            _micVolume = micVolume
            _log = log
        End Sub

        Private Sub Log(msg As String)
            _log?.Invoke(msg)
        End Sub

        ' ─── lifecycle ──────────────────────────────────────────────

        ''' <summary>
        ''' Create pipes, start listeners, spawn FFmpeg. Returns False (with
        ''' reason logged) if the process cannot start. FFmpeg blocks reading
        ''' until data arrives — safe to start before capture begins.
        ''' </summary>
        Public Function Start() As Boolean
            Dim id As String = Guid.NewGuid().ToString("N").Substring(0, 10)
            Try
                _video = New PipeFeed("sp_v_" & id, waitForTimeline:=False)
                _audio = New PipeFeed("sp_a_" & id, waitForTimeline:=True)
                _mic = If(_micRate > 0, New PipeFeed("sp_m_" & id, waitForTimeline:=True), Nothing)

                _video.StartListening()
                _audio.StartListening()
                _mic?.StartListening()

                Dim psi As New ProcessStartInfo With {
                    .FileName = _ffmpegPath,
                    .Arguments = BuildArgs(),
                    .UseShellExecute = False,
                    .RedirectStandardError = True,
                    .RedirectStandardOutput = True,
                    .CreateNoWindow = True
                }
                _proc = Process.Start(psi)
                _stderrTask = _proc.StandardError.ReadToEndAsync()

                _video.StartWriter()
                _audio.StartWriter()
                _mic?.StartWriter()

                _started = True
                Log($"[live-mux] started ffmpeg pid {_proc.Id} @ {_videoFps}fps, " &
                    $"sys {_sysRate}Hz/{_sysChannels}ch, mic {If(_mic IsNot Nothing, _micRate & "Hz/" & _micChannels & "ch", "off")}")
                Return True
            Catch ex As Exception
                Log("[live-mux] start FAILED: " & ex.Message)
                Return False
            End Try
        End Function

        Private Function BuildArgs() As String
            Dim sb As New Text.StringBuilder()
            sb.Append("-y -hide_banner -loglevel warning ")

            ' input 0: raw H.264 CFR
            sb.Append($"-f h264 -framerate {_videoFps} -i ""\\.\pipe\{_video.Name}"" ")

            ' input 1: system audio raw PCM
            sb.Append($"-f s16le -ar {_sysRate} -ac {_sysChannels} -i ""\\.\pipe\{_audio.Name}"" ")

            ' input 2: mic raw PCM
            If _mic IsNot Nothing Then
                sb.Append($"-f s16le -ar {_micRate} -ac {_micChannels} -i ""\\.\pipe\{_mic.Name}"" ")
            End If

            If _mic IsNot Nothing AndAlso Not _separateTracks Then
                ' amix single track (mirrors MuxCoordinator's mixed mode)
                Dim sysFilter As String = BuildOneFilter(_sysVolume, True)
                Dim micFilter As String = BuildOneFilter(_micVolume, True)
                sb.Append($"-filter_complex ""[1:a]{sysFilter}[a0];[2:a]{micFilter}[a1];" &
                          "[a0][a1]amix=inputs=2:duration=longest:normalize=0[aout]"" ")
                sb.Append("-map 0:v -map ""[aout]"" ")
            ElseIf _mic IsNot Nothing Then
                sb.Append("-map 0:v -map 1:a -map 2:a ")
            Else
                sb.Append("-map 0:v -map 1:a ")
            End If

            sb.Append("-c:v copy ")
            If _mic IsNot Nothing AndAlso _separateTracks Then
                sb.Append("-c:a:0 aac -b:a:0 320k -ar:a:0 48000 ")
                sb.Append("-c:a:1 aac -b:a:1 320k -ar:a:1 48000 ")
            Else
                sb.Append("-c:a aac -b:a 320k -ar 48000 ")
            End If

            ' fragmented MP4: recording survives a crash mid-session
            sb.Append("-movflags +frag_keyframe+empty_moov+default_base_moof ")
            sb.Append($"""{_fragPath}""")
            Return sb.ToString()
        End Function

        Private Shared Function BuildOneFilter(volume As Single, includeApad As Boolean) As String
            Dim parts As New List(Of String)()
            If Math.Abs(volume - 1.0F) > 0.001F Then
                Dim v As Single = Math.Max(0.0F, Math.Min(2.0F, volume))
                parts.Add($"volume={v.ToString("0.000", CultureInfo.InvariantCulture)}")
            End If
            parts.Add("aresample=async=1:first_pts=0")
            If includeApad Then parts.Add("apad")
            Return String.Join(",", parts)
        End Function

        ' ─── timeline + feeding ─────────────────────────────────────

        ''' <summary>Video t0 established (first encoded frame). Offsets use the SyncMath model.</summary>
        Public Sub BeginTimelines(systemOffsetSec As Double, micOffsetSec As Double)
            ' offset > 0 → audio started BEFORE video → discard head
            ' offset < 0 → audio data begins AFTER video → pad silence
            Dim sysBytesPerSec As Long = CLng(_sysRate) * _sysChannels * 2L
            Dim micBytesPerSec As Long = CLng(_micRate) * _micChannels * 2L

            Dim sysDiscard As Long = CLng(Math.Max(0, systemOffsetSec) * sysBytesPerSec)
            Dim sysPad As Long = CLng(Math.Max(0, -systemOffsetSec) * sysBytesPerSec)
            _audio.BeginTimeline(sysDiscard, sysPad)
            Log($"[live-mux] sys timeline: offset={systemOffsetSec:0.000}s → discard {sysDiscard:N0}B / pad {sysPad:N0}B")

            If _mic IsNot Nothing Then
                Dim micDiscard As Long = CLng(Math.Max(0, micOffsetSec) * micBytesPerSec)
                Dim micPad As Long = CLng(Math.Max(0, -micOffsetSec) * micBytesPerSec)
                _mic.BeginTimeline(micDiscard, micPad)
                Log($"[live-mux] mic timeline: offset={micOffsetSec:0.000}s → discard {micDiscard:N0}B / pad {micPad:N0}B")
            End If
        End Sub

        Public Sub FeedVideo(payload As Byte(), length As Integer)
            If _started Then _video.Feed(payload, length)
        End Sub

        Public Sub FeedSystemAudio(pcm As Byte(), length As Integer)
            If _started Then _audio.Feed(pcm, length)
        End Sub

        Public Sub FeedMicAudio(pcm As Byte(), length As Integer)
            If _started Then _mic?.Feed(pcm, length)
        End Sub

        ' ─── stop ───────────────────────────────────────────────────

        ''' <summary>
        ''' Drain queues, close pipes (EOF), wait for FFmpeg to finalize the
        ''' fragment, then remux to +faststart. Never throws.
        ''' </summary>
        Public Function [Stop](timeoutMs As Integer) As LiveMuxResult
            Dim res As New LiveMuxResult()
            If Not _started Then
                res.ErrorMessage = "not started"
                Return res
            End If
            Try
                _video.RequestStopAndDrain(timeoutMs)
                _audio.RequestStopAndDrain(timeoutMs)
                _mic?.RequestStopAndDrain(timeoutMs)

                If Not _proc.WaitForExit(timeoutMs) Then
                    Try : _proc.Kill() : Catch : End Try
                    res.ErrorMessage = "ffmpeg finalize timeout"
                End If
                res.FFmpegExitCode = _proc.ExitCode

                Dim stderrTail As String = ""
                Try
                    If _stderrTask.Wait(2000) Then
                        Dim lines As String() = _stderrTask.Result.Split(New Char() {ControlChars.Lf}, StringSplitOptions.RemoveEmptyEntries)
                        stderrTail = String.Join(" | ", lines.Skip(Math.Max(0, lines.Length - 3)))
                    End If
                Catch
                End Try

                res.VideoBytesFed = _video.BytesWritten
                res.SystemBytesFed = _audio.BytesWritten
                res.MicBytesFed = If(_mic IsNot Nothing, _mic.BytesWritten, 0)
                res.DroppedBytes = _video.DroppedBytes + _audio.DroppedBytes + If(_mic IsNot Nothing, _mic.DroppedBytes, 0)

                If res.FFmpegExitCode = 0 AndAlso File.Exists(_fragPath) Then
                    ' +faststart remux (stream copy — cheap)
                    Dim remuxOk As Boolean = RunRemux()
                    res.UsedFaststartRemux = remuxOk
                    If remuxOk Then
                        Try : File.Delete(_fragPath) : Catch : End Try
                        res.Succeeded = File.Exists(_finalPath)
                    Else
                        ' salvage: fragmented file is still playable
                        Try
                            If File.Exists(_finalPath) Then File.Delete(_finalPath)
                            File.Move(_fragPath, _finalPath)
                        Catch
                        End Try
                        res.Succeeded = File.Exists(_finalPath)
                        res.ErrorMessage &= " faststart-remux failed (kept fragmented file)"
                    End If
                Else
                    res.ErrorMessage &= " ffmpeg exit=" & res.FFmpegExitCode & " " & stderrTail
                End If

                Log("[live-mux] " & res.ToString())
            Catch ex As Exception
                res.ErrorMessage &= " stop error: " & ex.Message
                Log("[live-mux] stop error: " & ex.Message)
            End Try
            Return res
        End Function

        Private Function RunRemux() As Boolean
            Try
                Dim psi As New ProcessStartInfo With {
                    .FileName = _ffmpegPath,
                    .Arguments = $"-y -hide_banner -loglevel error -i ""{_fragPath}"" -c copy -movflags +faststart ""{_finalPath}""",
                    .UseShellExecute = False,
                    .RedirectStandardError = True,
                    .CreateNoWindow = True
                }
                Using p As Process = Process.Start(psi)
                    Dim errTask = p.StandardError.ReadToEndAsync()
                    If Not p.WaitForExit(30000) Then
                        Try : p.Kill() : Catch : End Try
                        Return False
                    End If
                    errTask.Wait(1000)
                    Return p.ExitCode = 0 AndAlso File.Exists(_finalPath)
                End Using
            Catch
                Return False
            End Try
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            Try : _video?.Dispose() : Catch : End Try
            Try : _audio?.Dispose() : Catch : End Try
            Try : _mic?.Dispose() : Catch : End Try
            Try : _proc?.Dispose() : Catch : End Try
        End Sub

        ' ═════════════════════════════════════════════════════════════
        ' PipeFeed: named-pipe server + bounded queue + writer thread
        ' ═════════════════════════════════════════════════════════════
        Private NotInheritable Class PipeFeed
            Implements IDisposable

            Friend ReadOnly Name As String
            Private ReadOnly _pipe As NamedPipeServerStream
            Private ReadOnly _queue As New ConcurrentQueue(Of Byte())()
            Private ReadOnly _signal As New AutoResetEvent(False)
            Private ReadOnly _stopWriter As New ManualResetEvent(False)
            Private ReadOnly _timelineStarted As New ManualResetEvent(False)
            Private ReadOnly _connectedEvent As New ManualResetEvent(False)
            Private ReadOnly _sync As New Object()

            Private _writer As Thread
            Private _pipeBroken As Boolean
            Private _bytesWritten As Long
            Private _droppedBytes As Long
            Private _discardBytes As Long
            Private _padBytes As Long

            Friend Sub New(pipeName As String, waitForTimeline As Boolean)
                Name = pipeName
                _waitForTimeline = waitForTimeline
                _pipe = New NamedPipeServerStream(pipeName, PipeDirection.Out, 1,
                                                  PipeTransmissionMode.Byte,
                                                  PipeOptions.Asynchronous,
                                                  PipeBufferBytes, PipeBufferBytes)
            End Sub

            Private ReadOnly _waitForTimeline As Boolean

            Friend ReadOnly Property BytesWritten As Long
                Get
                    Return Interlocked.Read(_bytesWritten)
                End Get
            End Property

            Friend ReadOnly Property DroppedBytes As Long
                Get
                    Return Interlocked.Read(_droppedBytes)
                End Get
            End Property

            ''' <summary>Wait for the FFmpeg client to connect (background).</summary>
            Friend Sub StartListening()
                Task.Run(Sub()
                             Try
                                 _pipe.WaitForConnection()
                             Catch
                             End Try
                             _connectedEvent.Set()
                         End Sub)
            End Sub

            Friend Sub StartWriter()
                _writer = New Thread(AddressOf WriterLoop) With {
                    .IsBackground = True,
                    .Name = "LiveMux_" & Name
                }
                _writer.Start()
            End Sub

            ''' <summary>
            ''' t0 established. discardBytes = skip that much REAL data from the
            ''' queue head (audio ran before video). padBytes = write that much
            ''' silence FIRST (audio data begins after video t0).
            ''' </summary>
            Friend Sub BeginTimeline(discardBytes As Long, padBytes As Long)
                SyncLock _sync
                    _discardBytes = Math.Max(0, discardBytes)
                    _padBytes = Math.Max(0, padBytes)
                End SyncLock
                _timelineStarted.Set()
                _signal.Set()
            End Sub

            Friend Sub Feed(data As Byte(), count As Integer)
                If count <= 0 OrElse count > data.Length Then Return
                Dim copy(count - 1) As Byte
                Array.Copy(data, copy, count)
                While _queue.Count >= QueueChunkCap
                    Dim dropped As Byte() = Nothing
                    If _queue.TryDequeue(dropped) Then
                        Interlocked.Add(_droppedBytes, dropped.Length)
                    Else
                        Exit While
                    End If
                End While
                _queue.Enqueue(copy)
                _signal.Set()
            End Sub

            Private Sub WriterLoop()
                Try
                    ' 1. wait for FFmpeg to connect — on the REAL connection event,
                    '    NOT a blind sleep. Owner run 21:15 (a=0B, exit -22):
                    '    the old blind 15s sleep deadlocked with ffmpeg's SERIAL
                    '    input opening. ffmpeg opens input 0 (video pipe) and blocks
                    '    probing until video data arrives; the video writer was also
                    '    blind-sleeping, so no data for 15s; ffmpeg therefore opened
                    '    the AUDIO pipe only at ~15.1s — but the audio writer checked
                    '    IsConnected ONCE at 15.0s, saw nothing, and exited forever
                    '    (queue silently held everything; 254 chunks < cap so not even
                    '    drops were counted). Waiting on the event fixes both sides:
                    '    video writes the moment ffmpeg connects → probe finishes
                    '    fast → ffmpeg opens the audio pipe promptly → the audio
                    '    writer is still waiting on its event and proceeds.
                    Dim connectWait() As WaitHandle = {_connectedEvent, _stopWriter}
                    WaitHandle.WaitAny(connectWait, 30000)
                    If Not _pipe.IsConnected Then
                        DrainDiscardOnly()
                        Return
                    End If

                    ' 2. AUDIO pipes wait for the timeline (video t0) — the
                    '    discard/pad alignment needs to know t0 first.
                    '    VIDEO pipe does NOT wait: it IS the timeline, and waiting
                    '    here caused the 21:36 failure — during the 5s blind wait
                    '    the queue overflowed (256 chunks) and drop-oldest threw
                    '    away the FIRST encoded frames including SPS/PPS, leaving
                    '    ffmpeg with 'non-existing PPS 0 referenced' and a dead
                    '    output. Video starts writing the moment it connects.
                    If _waitForTimeline Then
                        WaitHandle.WaitAny(New WaitHandle() {_timelineStarted, _stopWriter}, 5000)
                    End If

                    ' 3. initial alignment: pad silence first, then discard head
                    WritePadIfAny()
                    DiscardHeadIfAny()

                    ' 4. steady state: drain as chunks arrive
                    While True
                        WaitHandle.WaitAny(New WaitHandle() {_signal, _stopWriter}, 200)
                        DrainQueue()
                        If _stopWriter.WaitOne(0) Then Exit While
                    End While

                    ' 5. final drain (stop requested)
                    DrainQueue()
                Catch
                End Try
            End Sub

            Private Sub DrainQueue()
                Dim chunk As Byte() = Nothing
                While _queue.TryDequeue(chunk)
                    If _pipeBroken Then
                        Interlocked.Add(_droppedBytes, chunk.Length)
                        Continue While
                    End If
                    Try
                        _pipe.Write(chunk, 0, chunk.Length)
                        Interlocked.Add(_bytesWritten, chunk.Length)
                    Catch
                        _pipeBroken = True
                        Interlocked.Add(_droppedBytes, chunk.Length)
                    End Try
                End While
            End Sub

            Private Sub DrainDiscardOnly()
                Dim chunk As Byte() = Nothing
                While _queue.TryDequeue(chunk)
                    Interlocked.Add(_droppedBytes, chunk.Length)
                End While
            End Sub

            Private Sub WritePadIfAny()
                Dim pad As Long
                SyncLock _sync
                    pad = _padBytes
                    _padBytes = 0
                End SyncLock
                If pad <= 0 OrElse _pipeBroken Then Return
                Dim zeros(65535) As Byte
                Dim off As Long = 0
                While off < pad
                    Dim n As Integer = CInt(Math.Min(pad - off, zeros.Length))
                    Try
                        _pipe.Write(zeros, 0, n)
                        Interlocked.Add(_bytesWritten, n)
                    Catch
                        _pipeBroken = True
                        Return
                    End Try
                    off += n
                End While
            End Sub

            Private Sub DiscardHeadIfAny()
                Dim disc As Long
                SyncLock _sync
                    disc = _discardBytes
                End SyncLock
                If disc <= 0 Then Return
                Dim chunk As Byte() = Nothing
                While disc > 0 AndAlso _queue.TryDequeue(chunk)
                    If chunk.Length <= disc Then
                        disc -= chunk.Length
                        Interlocked.Add(_droppedBytes, chunk.Length)
                    Else
                        ' partial chunk: write the tail, drop the head
                        Dim skip As Integer = CInt(disc)
                        Dim keep As Byte() = New Byte(chunk.Length - skip - 1) {}
                        Array.Copy(chunk, skip, keep, 0, keep.Length)
                        If Not _pipeBroken Then
                            Try
                                _pipe.Write(keep, 0, keep.Length)
                                Interlocked.Add(_bytesWritten, keep.Length)
                            Catch
                                _pipeBroken = True
                            End Try
                        End If
                        disc = 0
                    End If
                End While
                SyncLock _sync
                    _discardBytes = Math.Max(0, disc)   ' not enough data yet; discard rest later? keep simple: stop discarding
                    _discardBytes = 0
                End SyncLock
            End Sub

            ''' <summary>Signal stop, drain, then dispose the pipe → FFmpeg reads EOF.</summary>
            Friend Sub RequestStopAndDrain(timeoutMs As Integer)
                _stopWriter.Set()
                _signal.Set()
                _timelineStarted.Set()
                If _writer IsNot Nothing AndAlso Not _writer.Join(timeoutMs) Then
                    ' writer stuck on a blocked pipe write — force close
                    Try : _pipe.Dispose() : Catch : End Try
                    Try : _writer.Join(2000) : Catch : End Try
                End If
                Try : _pipe.Dispose() : Catch : End Try
            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                _stopWriter.Set()
                _signal.Set()
                Try : _pipe.Dispose() : Catch : End Try
                Try : _signal.Dispose() : Catch : End Try
                Try : _stopWriter.Dispose() : Catch : End Try
                Try : _timelineStarted.Dispose() : Catch : End Try
                Try : _connectedEvent.Dispose() : Catch : End Try
            End Sub
        End Class

    End Class

End Namespace
