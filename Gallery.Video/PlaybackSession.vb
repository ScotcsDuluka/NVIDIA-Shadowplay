Option Strict On
Option Explicit On
Option Infer On

' PlaybackSession.vb — the Gallery playback orchestrator (design doc §3.3–§3.7).
'
' OWNER-MANDATED PROPERTIES, and how this file delivers them:
'   1. Nine-state lifecycle (PlaybackState) — transitions follow the static
'      truth table in PlaybackStateMachine (unit-pinned in StateMachineTests).
'   2. UI thread NEVER blocks — Open/Play/Pause/Seek/Stop validate + mutate
'      state synchronously (microseconds) and hand the heavy work to ONE
'      serialized orchestration path (SemaphoreSlim). No orchestration ever
'      runs on the caller's thread. WaitForState/WaitForEof are test seams,
'      not UI APIs.
'   3. Bounded memory — video queue bounded (FrameQueue), render-side hold
'      ≤ 1 frame, audio ring bounded (AudioPcmBuffer). Pause PARKS the decode
'      worker (kills the ffmpeg subprocess): a paused session holds zero
'      subprocesses and a bounded number of stale frames that the next
'      generation flush disposes. No path can grow memory over time.
'   4. Single dispose-once guard (Interlocked) — after Dispose, every API is
'      a safe no-op/reject and no further events fire (render thread joined
'      BEFORE the Disposed transition is published).
'   5. One timestamp domain (QPC 100-ns ticks) — PlaybackClock is the wall
'      clock; when audio renders, its sample position (same 100-ns domain)
'      is the master; late/early windows are ±1.5 frame intervals (§3.4).
'   6. Seek protocol (§3.7) — generation token bump, worker kill, queue
'      flush by generation, respawn with input-side -ss at target, first
'      frame present + clock re-anchor. Failure → advisory SeekFailed fault
'      event and the session RECOVERS to Paused at the old position (§7);
'      only a failed recovery escalates to terminal Faulted.
'   7. EOF is not a fault (§7) — decode end + drained queues → hold last
'      frame, clock freezes, state → Paused, EosReached fires.
'
' DEVIATION NOTE (documented in the design doc §11 status table):
'   Design §3.4 described pause as "drain to a render-side hold ≤1 frame".
'   Implementation parks the DECODER instead (kill subprocess on pause).
'   Same invariant (bounded memory, no growth), fewer moving parts, and the
'   paused session costs zero subprocess CPU. Doc updated to match.

Imports System
Imports System.Globalization
Imports System.IO
Imports System.Threading

Namespace Gallery.Video

    ''' <summary>Session configuration. Paths: the prototype REQUIRES explicit
    ''' binary paths (tests pass them; product wiring resolves via
    ''' FFmpegLocator against the product install dir — a wiring follow-up,
    ''' never an assumption inside the engine).</summary>
    Public NotInheritable Class PlaybackSessionOptions
        Public Property FfmpegExe As String = ""
        Public Property FfprobeExe As String = ""

        ''' <summary>Video frame queue capacity (default 3 ≈ 50 ms @60fps ≈ 25 MB
        ''' at 1080p BGRA — bounded by design, §3.4).</summary>
        Public Property VideoQueueCapacity As Integer = 3

        ''' <summary>Audio ring length in milliseconds (default ≈ 200 ms, §3.4).</summary>
        Public Property AudioRingMs As Integer = 200

        ''' <summary>Bounded waits: open (probe+spawn) and seek (first frame).</summary>
        Public Property OpenTimeoutMs As Integer = 15000
        Public Property SeekTimeoutMs As Integer = 5000

        ''' <summary>Decode audio when the probe finds an audio stream.</summary>
        Public Property AudioEnabled As Boolean = True

        ''' <summary>Target window handle. IntPtr.Zero → NullVideoSink (headless
        ''' proof path, Linux-runnable). Non-zero → D3D11VideoRenderer (Windows).</summary>
        Public Property RenderWindow As IntPtr = IntPtr.Zero

        ''' <summary>Test seam: inject a fake-frequency clock for deterministic
        ''' time math. Nothing → real QPC clock.</summary>
        Public Property Clock As PlaybackClock = Nothing

        ''' <summary>Render loop tick (presentation pacing granularity).</summary>
        Public Property RenderTickMs As Integer = 8
    End Class

    ''' <summary>Immediate result of a session command (UI never throws for
    ''' guard rejections — reasons are data, not exceptions).</summary>
    Public NotInheritable Class PlaybackCommandResult
        Public ReadOnly Property Accepted As Boolean
        Public ReadOnly Property Reason As String

        Private Sub New(accepted As Boolean, reason As String)
            Me.Accepted = accepted
            Me.Reason = reason
        End Sub

        Public Shared ReadOnly Ok As New PlaybackCommandResult(True, "")

        Public Shared Function Reject(reason As String) As PlaybackCommandResult
            Return New PlaybackCommandResult(False, reason)
        End Function

        Public Overrides Function ToString() As String
            Return If(Accepted, "OK", $"REJECTED: {Reason}")
        End Function
    End Class

    Public NotInheritable Class PlaybackSession
        Implements IDisposable

        ' ---- events (never fired after Dispose returns) ----

        Public Event StateChanged(sender As PlaybackSession, oldState As PlaybackState, newState As PlaybackState)
        Public Event FaultRaised(sender As PlaybackSession, fault As GalleryVideoFault)
        Public Event EosReached(sender As PlaybackSession)
        Public Event OpenCompleted(sender As PlaybackSession, ok As Boolean, fault As GalleryVideoFault)

        ' ---- core fields ----
        Private ReadOnly _opts As PlaybackSessionOptions
        Private ReadOnly _clock As PlaybackClock
        Private ReadOnly _lock As New Object()
        Private ReadOnly _holdLock As New Object()
        Private ReadOnly _orchestration As New SemaphoreSlim(1, 1)

        Private _state As PlaybackState = PlaybackState.Created
        Private _generation As Long = 0
        Private _fault As GalleryVideoFault = Nothing
        Private _media As MediaInfo = Nothing
        Private _path As String = ""
        Private _disposedState As Integer = 0

        ' pipeline pieces (created per generation; disposed on stop/fault/dispose)
        Private _worker As FfmpegDecodeWorker = Nothing
        Private _videoQueue As FrameQueue = Nothing
        Private _audioBuffer As AudioPcmBuffer = Nothing
        Private _sink As IVideoRenderSink = Nothing
        Private _audio As AudioRenderer = Nothing

        ' render loop
        Private _renderThread As Thread = Nothing
        Private _renderExitState As Integer = 0
        Private _decodeAlive As Boolean = False     ' False = worker parked/stopped (no EOF logic)
        Private _holdFrame As PlaybackFrame = Nothing
        Private _sinkLastGeneration As Long = -1
        Private _sinkLastSequence As Long = -1

        ' playback bookkeeping
        Private _fps As Double = 30.0
        Private _durationTicks As Long = 0
        Private _lastPresentedPts As Long = -1
        Private _pausedTicks As Long = 0            ' anchor for resume/post-EOF respawn
        Private _resumePaused As Boolean = False    ' seek return mode
        Private _playIntentQueued As Boolean = False
        Private _audioActive As Boolean = False
        Private _fpsFallbackUsed As Boolean = False

        ' metrics (measured, never hidden — §6)
        Private _framesPresented As Long
        Private _droppedLate As Long
        Private _staleGenerationsFlushed As Long
        Private _eofCount As Long
        Private _audioFallbackCount As Long

        ' eof waiter (test seam)
        Private ReadOnly _eofEvent As New ManualResetEventSlim(False)

        Public Sub New(Optional opts As PlaybackSessionOptions = Nothing)
            _opts = If(opts, New PlaybackSessionOptions())
            _clock = If(_opts.Clock, New PlaybackClock())
        End Sub

        ' ---- observability ----

        Public ReadOnly Property State As PlaybackState
            Get
                SyncLock _lock
                    Return _state
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property Generation As Long
            Get
                SyncLock _lock
                    Return _generation
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property Fault As GalleryVideoFault
            Get
                SyncLock _lock
                    Return _fault
                End SyncLock
            End Get
        End Property

        ''' <summary>Probe result of the opened file (Nothing before a successful
        ''' open; retained after Stop for Play-from-Stopped restart).</summary>
        Public ReadOnly Property Media As MediaInfo
            Get
                SyncLock _lock
                    Return _media
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property IsDisposed As Boolean
            Get
                Return Volatile.Read(_disposedState) <> 0
            End Get
        End Property

        Public ReadOnly Property FramesPresented As Long
            Get
                Return Volatile.Read(_framesPresented)
            End Get
        End Property

        Public ReadOnly Property DroppedLateFrames As Long
            Get
                Return Volatile.Read(_droppedLate)
            End Get
        End Property

        Public ReadOnly Property EofCount As Long
            Get
                Return Volatile.Read(_eofCount)
            End Get
        End Property

        ''' <summary>True when audio render was requested but no endpoint was
        ''' available → video-only mode (counted, never silent — §7 spirit).</summary>
        Public ReadOnly Property AudioFallbackToVideoOnly As Long
            Get
                Return Volatile.Read(_audioFallbackCount)
            End Get
        End Property

        Public ReadOnly Property FpsFallbackUsed As Boolean
            Get
                SyncLock _lock
                    Return _fpsFallbackUsed
                End SyncLock
            End Get
        End Property

        Public ReadOnly Property DurationTicks As Long
            Get
                SyncLock _lock
                    Return _durationTicks
                End SyncLock
            End Get
        End Property

        ''' <summary>PTS (100-ns ticks) of the most recently presented frame;
        ''' -1 before the first present. Seek tests pin this against the target.</summary>
        Public ReadOnly Property LastPresentedPtsTicks As Long
            Get
                Return Volatile.Read(_lastPresentedPts)
            End Get
        End Property

        ''' <summary>True while the presentation clock is frozen (paused/EOF).</summary>
        Public ReadOnly Property ClockFrozen As Boolean
            Get
                Return _clock.IsFrozen
            End Get
        End Property

        ''' <summary>Current presentation position (100-ns ticks, QPC domain):
        ''' master clock while Playing; last frozen/presented position otherwise.</summary>
        Public ReadOnly Property PositionTicks As Long
            Get
                Dim st = State
                If st = PlaybackState.Playing Then Return MasterTicks()
                SyncLock _lock
                    Return If(_lastPresentedPts >= 0, _lastPresentedPts, _pausedTicks)
                End SyncLock
            End Get
        End Property

        Private Function MasterTicks() As Long
            Dim a As AudioRenderer = Nothing
            Dim active As Boolean = False
            SyncLock _lock
                a = _audio
                active = _audioActive
            End SyncLock
            If active AndAlso a IsNot Nothing AndAlso a.IsPlaying Then
                Return a.AudioPositionTicks
            End If
            Return _clock.PresentationTicks
        End Function

        ' ---- test seams (polling keeps them off the UI contract) ----

        Public Function WaitForState(target As PlaybackState, timeoutMs As Integer) As Boolean
            Dim deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs)
            While DateTime.UtcNow < deadline
                If IsDisposed AndAlso target <> PlaybackState.Disposed Then Return False
                If State = target Then Return True
                Thread.Sleep(10)
            End While
            Return State = target
        End Function

        Public Function WaitForEof(timeoutMs As Integer) As Boolean
            Return _eofEvent.Wait(timeoutMs)
        End Function

        ' ================================================================
        '  Public command surface — all non-blocking (guard + hand-off)
        ' ================================================================

        ''' <summary>Open a media file. Valid from Created. Completes via
        ''' OpenCompleted + StateChanged (Opening → Playing on success,
        ''' → Faulted on failure). Auto-plays on success (§3.5).</summary>
        Public Function Open(path As String) As PlaybackCommandResult
            If IsDisposed Then Return PlaybackCommandResult.Reject("disposed")
            If String.IsNullOrWhiteSpace(path) Then Return PlaybackCommandResult.Reject("empty path")

            SyncLock _lock
                If _state <> PlaybackState.Created Then
                    Return PlaybackCommandResult.Reject($"Open requires Created (state={_state})")
                End If
                _state = PlaybackState.Opening
            End SyncLock
            FireState(PlaybackState.Created, PlaybackState.Opening)

            Dim p = path
            Task.Run(Sub() OpenCore(p))
            Return PlaybackCommandResult.Ok
        End Function

        ''' <summary>Play. Truth table: Playing → no-op; Paused → resume
        ''' (respawns the parked decoder at the paused position);
        ''' Stopped → restart from the beginning of the last media;
        ''' Opening → queue the intent (applied on open completion).</summary>
        Public Function Play() As PlaybackCommandResult
            If IsDisposed Then Return PlaybackCommandResult.Reject("disposed")

            Dim startRestart As Boolean = False
            SyncLock _lock
                Select Case _state
                    Case PlaybackState.Playing
                        Return PlaybackCommandResult.Ok ' no-op
                    Case PlaybackState.Paused
                        _state = PlaybackState.Playing
                    Case PlaybackState.Stopped
                        If _media Is Nothing Then Return PlaybackCommandResult.Reject("no media (nothing opened yet)")
                        _state = PlaybackState.Playing
                        startRestart = True
                    Case PlaybackState.Opening
                        _playIntentQueued = True
                        Return PlaybackCommandResult.Ok ' queued
                    Case Else
                        Return PlaybackCommandResult.Reject($"Play invalid in {_state}")
                End Select
            End SyncLock

            Dim old = If(startRestart, PlaybackState.Stopped, PlaybackState.Paused)
            FireState(old, PlaybackState.Playing)

            If startRestart Then
                Task.Run(Sub() RestartFromStoppedCore())
            Else
                Task.Run(Sub() ResumeFromPauseCore())
            End If
            Return PlaybackCommandResult.Ok
        End Function

        ''' <summary>Pause: freeze the clock at the current position and PARK the
        ''' decoder (bounded memory, zero subprocess cost while paused). The
        ''' last presented frame stays on screen (no black frame).</summary>
        Public Function Pause() As PlaybackCommandResult
            If IsDisposed Then Return PlaybackCommandResult.Reject("disposed")

            Dim parked As Boolean = False
            SyncLock _lock
                If _state = PlaybackState.Paused Then Return PlaybackCommandResult.Ok ' no-op
                If _state <> PlaybackState.Playing Then
                    Return PlaybackCommandResult.Reject($"Pause requires Playing (state={_state})")
                End If
                _state = PlaybackState.Paused
                _pausedTicks = MasterTicks()
                _clock.Freeze()
                _audio?.Pause()
                parked = _decodeAlive
                _decodeAlive = False
            End SyncLock
            FireState(PlaybackState.Playing, PlaybackState.Paused)

            If parked Then Task.Run(Sub() ParkDecoderCore())
            Return PlaybackCommandResult.Ok
        End Function

        ''' <summary>Seek (§3.7). Valid from Playing/Paused. Completion via
        ''' StateChanged (Seeking → previous mode) or FaultRaised (advisory
        ''' SeekFailed with recovery to Paused at the old position).</summary>
        Public Function Seek(seconds As Double) As PlaybackCommandResult
            If IsDisposed Then Return PlaybackCommandResult.Reject("disposed")

            Dim clamped As Double
            Dim originTicks As Long
            Dim prevState As PlaybackState
            SyncLock _lock
                If Not PlaybackStateMachine.CanSeek(_state) Then
                    Return PlaybackCommandResult.Reject($"Seek requires Playing/Paused (state={_state})")
                End If
                ' Clamp to [0, duration - 1.5 frames] when duration is known —
                ' a decode generation spawned exactly at EOF produces 0 frames
                ' (ffmpeg exits cleanly) which the worker reads as corrupt.
                clamped = Math.Max(0.0, seconds)
                If _durationTicks > 0 Then
                    Dim maxT = PlaybackClock.TicksToSeconds(
                        Math.Max(0L, _durationTicks - PlaybackClock.FrameWindowTicks(1.0 / _fps)))
                    clamped = Math.Min(clamped, maxT)
                End If
                originTicks = MasterTicks()
                _resumePaused = (_state = PlaybackState.Paused)
                prevState = _state
                _state = PlaybackState.Seeking
                _pausedTicks = PlaybackClock.SecondsToTicks(clamped)
            End SyncLock
            _seekOriginTicks = originTicks
            FireState(prevState, PlaybackState.Seeking)

            Task.Run(Sub() SeekCore(clamped))
            Return PlaybackCommandResult.Ok
        End Function

        Private _seekOriginTicks As Long

        ''' <summary>Stop: release the whole pipeline (subprocesses, queues,
        ''' renderer, audio), keep the probed media info (Play-from-Stopped
        ''' restart). Idempotent; legal from every state except Disposed.</summary>
        Public Function Stop_() As PlaybackCommandResult
            If IsDisposed Then Return PlaybackCommandResult.Reject("disposed")

            Dim prevState As PlaybackState
            SyncLock _lock
                If _state = PlaybackState.Stopped Then Return PlaybackCommandResult.Ok
                If _state = PlaybackState.Stopping Then Return PlaybackCommandResult.Ok ' in flight
                prevState = _state
                _state = PlaybackState.Stopping
                _playIntentQueued = False
            End SyncLock
            FireState(prevState, PlaybackState.Stopping)

            Task.Run(Sub() StopCore(PlaybackState.Stopped))
            Return PlaybackCommandResult.Ok
        End Function

        ''' <summary>VB-friendly alias (Stop is a reserved context keyword
        ''' inside method bodies; the runner/tests use Stop_).</summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.CompareExchange(_disposedState, 1, 0) <> 0 Then Return

            ' Synchronous bounded teardown: after Dispose returns there are NO
            ' live threads and NO further events (owner mandate: no stale
            ' callbacks past dispose).
            SyncLock _lock
                If _state = PlaybackState.Disposed Then Return
            End SyncLock

            Try
                TeardownPipeline()
            Catch
            End Try

            Dim old As PlaybackState
            SyncLock _lock
                old = _state
                _state = PlaybackState.Disposed
            End SyncLock
            FireState(old, PlaybackState.Disposed)
        End Sub

        ' ================================================================
        '  Orchestration cores (always on tasks, serialized by semaphore)
        ' ================================================================

        Private Sub OpenCore(path As String)
            If Not _orchestration.Wait(30000) Then
                FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.InternalError, "open orchestration timeout"))
                Return
            End If
            Try
                Dim st = State
                If st <> PlaybackState.Opening OrElse IsDisposed Then Return ' stopped meanwhile

                Try
                    ' ---- pre-spawn guards (§7 order: cheap checks first) ----
                    If Not File.Exists(path) Then
                        FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.FileMissing, path))
                        Return
                    End If
                    If String.IsNullOrWhiteSpace(_opts.FfprobeExe) OrElse String.IsNullOrWhiteSpace(_opts.FfmpegExe) Then
                        FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.BackendMissing,
                                                       "ffmpeg/ffprobe paths not configured"))
                        Return
                    End If
                    If Not FFmpegLocator.IsUsableFFmpeg(_opts.FfprobeExe) OrElse
                       Not FFmpegLocator.IsUsableFFmpeg(_opts.FfmpegExe) Then
                        FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.BackendMissing,
                                                       $"binaries unusable: {_opts.FfmpegExe}"))
                        Return
                    End If

                    ' ---- probe (metadata authority — never assume format) ----
                    Dim probe As MediaInfo
                    Try
                        probe = MediaProbe.Probe(path, _opts.FfprobeExe)
                    Catch ioex As MediaProbe.ProbeIOException
                        FaultOut(GalleryVideoFault.FromException(path, ioex)) ' FileLocked mapping
                        Return
                    End Try
                    If probe Is Nothing Then
                        FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.CorruptFile,
                                                       $"ffprobe rejected {path}"))
                        Return
                    End If
                    If Not probe.HasVideo Then
                        FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.NoVideoStream,
                                                       $"streams={probe.Streams.Count}"))
                        Return
                    End If

                    Dim v = probe.Video
                    Dim fps = MediaInfo.ParseFrameRate(v.AvgFrameRate)
                    If fps <= 0 Then
                        fps = 30.0
                        _fpsFallbackUsed = True ' counted, never silent (§6)
                    End If

                    ' ---- §7 format gate: probed codec/pixfmt must be in the
                    ' supported set BEFORE any pipeline allocation. The set is
                    ' deliberate: the product's own recordings are h264/yuv420p
                    ' (pinned from a real ShadowPlay file, design doc §5) —
                    ' HEVC/QSV variants stay gated out until probed-for-real.
                    If v.CodecName <> "h264" OrElse v.PixFmt <> "yuv420p" OrElse
                       v.Width <= 0 OrElse v.Height <= 0 Then
                        FaultOut(New GalleryVideoFault(
                            GalleryVideoFaultKind.UnsupportedFormat,
                            $"codec={v.CodecName} pix_fmt={v.PixFmt} {v.Width}x{v.Height} " &
                            $"(supported: h264/yuv420p)"))
                        Return
                    End If

                    ' ---- allocate pipeline ----
                    SyncLock _lock
                        _media = probe
                        _path = path
                        _fps = fps
                        _durationTicks = PlaybackClock.SecondsToTicks(Math.Max(0.0, probe.Format.DurationSec))
                        _videoQueue = New FrameQueue(_opts.VideoQueueCapacity)
                        _audioBuffer = New AudioPcmBuffer(AudioRingBytes())
                    End SyncLock

                    If Not BuildRenderAndAudio() Then Return ' faulted inside

                    ' ---- first decode generation ----
                    If Not SpawnGeneration(0.0) Then Return ' faulted inside

                    ' ---- go live ----
                    ' Anchor BEFORE going live: the render loop must never see
                    ' a clock that already ran past the first frame's PTS
                    ' (probe time would otherwise read as "late" and drop the
                    ' opening frames — §3.4 late rule).
                    If wasOpening0() Then _clock.AnchorAt(0L)

                    Dim wasOpening As Boolean
                    SyncLock _lock
                        wasOpening = (_state = PlaybackState.Opening)
                        _decodeAlive = True
                        If wasOpening Then _state = PlaybackState.Playing
                        _playIntentQueued = False
                    End SyncLock
                    StartRenderThreadIfNeeded()

                    If wasOpening Then
                        FireState(PlaybackState.Opening, PlaybackState.Playing)
                        RaiseEvent OpenCompleted(Me, True, Nothing)
                    End If
                Catch ex As Exception
                    FaultOut(GalleryVideoFault.FromException(path, ex))
                End Try
            Finally
                _orchestration.Release()
            End Try
        End Sub

        ''' <summary>Small helper: is this session still in the Opening state
        ''' (no stop/seek/dispose raced the open)? Lock-free snapshot.</summary>
        Private Function wasOpening0() As Boolean
            Return State = PlaybackState.Opening
        End Function

        Private Function AudioRingBytes() As Integer
            ' S16LE stereo-ish estimate is refined by the probe values at spawn;
            ' capacity only bounds the ring — exact rate/ch flow through it.
            Dim bytesPerSec = 48000 * 2 * 2
            If _media IsNot Nothing AndAlso _media.HasAudio Then
                bytesPerSec = Math.Max(8000, _media.Audio.SampleRate * Math.Max(1, _media.Audio.Channels) * 2)
            End If
            Return CInt(Math.Max(4096, bytesPerSec * Math.Max(20, _opts.AudioRingMs) \ 1000))
        End Function

        ''' <summary>Create the sink + audio renderer per configured options.
        ''' Returns False (fault published) when the configured renderer cannot
        ''' be created; audio absence DEGRADES to video-only (counted, §7).</summary>
        Private Function BuildRenderAndAudio() As Boolean
            Dim probe = _media
            Dim sink As IVideoRenderSink = Nothing

            If _opts.RenderWindow <> IntPtr.Zero Then
                ' Real present path — Windows/GPU only; TryCreate returns
                ' Nothing (never throws) off-hardware (honest gate, §9).
                sink = D3D11VideoRenderer.TryCreate(_opts.RenderWindow,
                                                    probe.Video.Width, probe.Video.Height)
                If sink Is Nothing Then
                    FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.RendererUnavailable,
                                                   "D3D11 renderer creation failed (no GPU/desktop?)"))
                    Return False
                End If
            Else
                sink = New NullVideoSink()
            End If

            Dim audio As AudioRenderer = Nothing
            Dim audioOn As Boolean = False
            If _opts.AudioEnabled AndAlso probe.HasAudio Then
                audio = AudioRenderer.TryCreate(_audioBuffer, probe.Audio.SampleRate, probe.Audio.Channels)
                If audio IsNot Nothing Then
                    audioOn = True
                Else
                    _audioFallbackCount += 1L ' video-only mode (no endpoint) — counted
                End If
            End If

            SyncLock _lock
                _sink = sink
                _audio = audio
                _audioActive = audioOn
            End SyncLock
            Return True
        End Function

        ''' <summary>Spawn one decode generation at `seekSeconds`. Publishes a
        ''' fault and returns False on spawn failure. Wires worker faults into
        ''' the session state machine (stale workers ignored).</summary>
        Private Function SpawnGeneration(seekSeconds As Double) As Boolean
            Dim probe = _media
            Dim v = probe.Video
            Dim gen As Long
            Dim queue As FrameQueue
            Dim ring As AudioPcmBuffer
            SyncLock _lock
                gen = _generation
                queue = _videoQueue
                ring = _audioBuffer
            End SyncLock

            Dim fps = _fps
            Dim cfg As New DecodeGenerationConfig With {
                .FilePath = _path,
                .FfmpegExe = _opts.FfmpegExe,
                .Generation = gen,
                .SeekSeconds = seekSeconds,
                .VideoEnabled = True,
                .AudioEnabled = _opts.AudioEnabled AndAlso probe.HasAudio AndAlso _audioBuffer IsNot Nothing,
                .FrameWidth = v.Width,
                .FrameHeight = v.Height,
                .FrameRate = fps,
                .AudioSampleRate = If(probe.HasAudio, probe.Audio.SampleRate, 48000),
                .AudioChannels = If(probe.HasAudio, probe.Audio.Channels, 2),
                .VideoQueue = queue,
                .AudioBuffer = ring,
                .StopTimeoutMs = 3000
            }

            Dim worker As New FfmpegDecodeWorker(cfg)
            Dim current As FfmpegDecodeWorker = worker
            AddHandler worker.FaultDetected,
                Sub(w, f)
                    ' Ignore stale workers (superseded generation).
                    Dim live As FfmpegDecodeWorker = Nothing
                    SyncLock _lock
                        live = _worker
                    End SyncLock
                    If live IsNot current Then Return
                    FaultOut(f, fromWorker:=True)
                End Sub

            worker.Start()

            ' Spawn verification: a spawn that dies immediately without a fault
            ' event (race window) is caught by the first-frame wait in callers.

            SyncLock _lock
                _worker = worker
            End SyncLock
            Return True
        End Function

        Private Sub ParkDecoderCore()
            If Not _orchestration.Wait(30000) Then Return
            Try
                Dim w As FfmpegDecodeWorker = Nothing
                SyncLock _lock
                    w = _worker
                    _worker = Nothing
                End SyncLock
                If w IsNot Nothing Then
                    Try : w.RequestStop() : Catch : End Try
                End If
            Finally
                _orchestration.Release()
            End Try
        End Sub

        ''' <summary>Resume from Paused: kill any live decoder (a paused-seek
        ''' leaves the seek generation RUNNING — resuming on top of it would
        ''' double-decode into the same queue), then respawn at the paused
        ''' position (new generation; queued stale frames flushed by floor).</summary>
        Private Sub ResumeFromPauseCore()
            If Not _orchestration.Wait(30000) Then Return
            Try
                If IsDisposed Then Return
                SyncLock _lock
                    If _state <> PlaybackState.Playing Then Return ' stopped/seeked meanwhile
                End SyncLock

                ' Kill the previous generation BEFORE spawning (single-decoder
                ' invariant: one session owns at most ONE live decode pair).
                Dim staleWorker As FfmpegDecodeWorker = Nothing
                SyncLock _lock
                    staleWorker = _worker
                    _worker = Nothing
                End SyncLock
                If staleWorker IsNot Nothing Then
                    Try : staleWorker.RequestStop() : Catch : End Try
                End If

                Dim pausedTicks As Long
                SyncLock _lock
                    pausedTicks = _pausedTicks
                End SyncLock

                Dim wasEofPaused As Boolean = _eofEvent.IsSet
                Dim seekSeconds = PlaybackClock.TicksToSeconds(pausedTicks)
                If wasEofPaused Then
                    ' Resume at EOF: step back one frame window so the decode
                    ' generation emits the final frames (a generation spawned
                    ' exactly at EOF emits 0 frames → false corrupt signature).
                    Dim frameSecs = 1.0 / Math.Max(1.0, _fps)
                    seekSeconds = Math.Max(0.0, seekSeconds - frameSecs)
                End If
                _eofEvent.Reset()

                BumpGenerationAndFlush()

                If Not SpawnGeneration(seekSeconds) Then Return

                SyncLock _lock
                    _decodeAlive = True
                End SyncLock
                _clock.AnchorAt(PlaybackClock.SecondsToTicks(seekSeconds))
                _audio?.Play()
                StartRenderThreadIfNeeded()
            Finally
                _orchestration.Release()
            End Try
        End Sub

        ''' <summary>Play-from-Stopped: rebuild the whole pipeline from the
        ''' retained probe at position 0 (fresh queues/sink/audio).</summary>
        Private Sub RestartFromStoppedCore()
            If Not _orchestration.Wait(30000) Then Return
            Try
                If IsDisposed Then Return
                SyncLock _lock
                    If _state <> PlaybackState.Playing Then Return ' stopped meanwhile
                End SyncLock

                TeardownPipeline(keepMedia:=True)
                _eofEvent.Reset()

                SyncLock _lock
                    _videoQueue = New FrameQueue(_opts.VideoQueueCapacity)
                    _audioBuffer = New AudioPcmBuffer(AudioRingBytes())
                End SyncLock

                If Not BuildRenderAndAudio() Then Return
                If Not SpawnGeneration(0.0) Then Return

                _clock.AnchorAt(0L)
                SyncLock _lock
                    _decodeAlive = True
                End SyncLock
                StartRenderThreadIfNeeded()
            Finally
                _orchestration.Release()
            End Try
        End Sub

        ''' <summary>Seek core (§3.7 steps 2–6). On success the first new frame
        ''' is presented (render thread handles both Playing and Paused modes
        ''' via the sequence gate), the clock re-anchors at the target, and the
        ''' previous mode resumes. On failure: advisory SeekFailed + recovery
        ''' to Paused at the seek origin (§7) — terminal Faulted only if the
        ''' recovery respawn also fails.</summary>
        Private Sub SeekCore(targetSeconds As Double)
            If Not _orchestration.Wait(30000) Then Return
            Try
                If IsDisposed Then Return
                SyncLock _lock
                    If _state <> PlaybackState.Seeking Then Return ' stopped/disposed meanwhile
                End SyncLock

                ' Kill current decode; all in-flight frames become garbage.
                Dim w As FfmpegDecodeWorker = Nothing
                SyncLock _lock
                    w = _worker
                    _worker = Nothing
                    _decodeAlive = False
                End SyncLock
                If w IsNot Nothing Then
                    Try : w.RequestStop() : Catch : End Try
                End If

                Dim newGen = BumpGenerationAndFlush()

                ' Respawn at target.
                _eofEvent.Reset()
                If Not SpawnGeneration(targetSeconds) Then
                    RecoverAfterSeekFailure("respawn at target failed")
                    Return
                End If

                ' Wait for the first frame of the new generation (bounded).
                Dim first As PlaybackFrame = Nothing
                Dim deadline = DateTime.UtcNow.AddMilliseconds(_opts.SeekTimeoutMs)
                While DateTime.UtcNow < deadline
                    If IsDisposed OrElse State <> PlaybackState.Seeking Then
                        If first IsNot Nothing Then first.Dispose()
                        Return ' session moved on (stop) — queue owns nothing here
                    End If
                    Dim f As PlaybackFrame = Nothing
                    If _videoQueue.TryDequeue(100, f) Then
                        SyncLock _lock
                            If f.Generation < _generation Then
                                f.Dispose() ' stale (rare: floor raced us) — keep waiting
                            Else
                                first = f
                                Exit While
                            End If
                        End SyncLock
                        Continue While
                    End If
                    Dim flt As GalleryVideoFault = Nothing
                    SyncLock _lock
                        flt = If(_worker?.Fault, Nothing)
                    End SyncLock
                    If flt IsNot Nothing Then Exit While
                End While

                If first Is Nothing Then
                    RecoverAfterSeekFailure($"no frame within {_opts.SeekTimeoutMs}ms at {targetSeconds:0.###}s")
                    Return
                End If

                ' Re-anchor: first frame PTS ≈ target (input-side -ss). Present
                ' decision is left to the render loop (sequence gate) so the
                ' D3D11 context stays single-threaded (§3.8).
                Dim anchor = Math.Max(first.PtsTicks, PlaybackClock.SecondsToTicks(targetSeconds))
                first.Dispose()

                Dim resumePaused As Boolean = _resumePaused
                SyncLock _lock
                    _decodeAlive = True
                End SyncLock
                _clock.AnchorAt(anchor)
                If resumePaused Then
                    SyncLock _lock
                        _state = PlaybackState.Paused
                        _clock.Freeze()
                    End SyncLock
                    FireState(PlaybackState.Seeking, PlaybackState.Paused)
                Else
                    _audio?.Play()
                    StartRenderThreadIfNeeded()
                    SyncLock _lock
                        _state = PlaybackState.Playing
                    End SyncLock
                    FireState(PlaybackState.Seeking, PlaybackState.Playing)
                End If
            Finally
                _orchestration.Release()
            End Try
        End Sub

        ''' <summary>§7 SeekFailed outcome: session stays usable at the OLD
        ''' position. Advisory fault event + respawn at the seek origin in
        ''' Paused mode. Escalates to Faulted only if the recovery also fails.</summary>
        Private Sub RecoverAfterSeekFailure(why As String)
            Dim origin = PlaybackClock.TicksToSeconds(_seekOriginTicks)
            RaiseEvent FaultRaised(Me, New GalleryVideoFault(GalleryVideoFaultKind.SeekFailed,
                                                             $"{why}; recovering at {origin:0.###}s"))

            BumpGenerationAndFlush()
            If Not SpawnGeneration(origin) Then
                FaultOut(New GalleryVideoFault(GalleryVideoFaultKind.SeekFailed,
                                               $"recovery respawn failed: {why}"))
                Return
            End If

            SyncLock _lock
                _decodeAlive = True
            End SyncLock
            _clock.AnchorAt(_seekOriginTicks)
            SyncLock _lock
                _state = PlaybackState.Paused
                _clock.Freeze()
            End SyncLock
            StartRenderThreadIfNeeded()
            FireState(PlaybackState.Seeking, PlaybackState.Paused)
        End Sub

        Private Sub StopCore(terminal As PlaybackState)
            If Not _orchestration.Wait(30000) Then
                ' Orchestration stuck (should never happen — bounded joins);
                ' force the terminal state anyway (never hang the UI).
            End If
            Try
                If IsDisposed AndAlso terminal <> PlaybackState.Disposed Then Return
                TeardownPipeline(keepMedia:=(terminal = PlaybackState.Stopped))

                Dim old As PlaybackState
                SyncLock _lock
                    old = _state
                    _state = terminal
                End SyncLock
                If old <> terminal Then FireState(old, terminal)
            Finally
                Try : _orchestration.Release() : Catch : End Try
            End Try
        End Sub

        ' ================================================================
        '  Render loop (the ONLY presenter — §3.3/§3.8)
        ' ================================================================

        Private Sub StartRenderThreadIfNeeded()
            SyncLock _lock
                If _renderThread IsNot Nothing AndAlso _renderThread.IsAlive Then Return
                Volatile.Write(_renderExitState, 0)
                Dim t As New Thread(AddressOf RenderLoop) With {
                    .IsBackground = True, .Name = "gv-render-" & _generation.ToString()}
                _renderThread = t
                t.Start()
            End SyncLock
        End Sub

        Private Sub RenderLoop()
            Dim tickMs = Math.Max(2, _opts.RenderTickMs)
            While Volatile.Read(_renderExitState) = 0 AndAlso Not IsDisposed

                Dim st As PlaybackState
                Dim gen As Long
                SyncLock _lock
                    st = _state
                    gen = _generation
                End SyncLock

                If st <> PlaybackState.Playing AndAlso st <> PlaybackState.Paused Then
                    Thread.Sleep(tickMs)
                    Continue While
                End If

                ' ---- fetch into the hold slot (≤1 frame — bounded, §3.4) ----
                Dim hold As PlaybackFrame = Nothing
                SyncLock _holdLock
                    hold = _holdFrame
                    If hold Is Nothing Then
                        Dim f As PlaybackFrame = Nothing
                        If _videoQueue IsNot Nothing AndAlso _videoQueue.TryDequeue(30, f) Then
                            _holdFrame = f
                            hold = f
                        End If
                    End If
                End SyncLock

                If hold Is Nothing Then
                    ' ---- EOF detection (decode alive + everything drained) ----
                    Dim w As FfmpegDecodeWorker = Nothing
                    Dim alive As Boolean = False
                    SyncLock _lock
                        w = _worker
                        alive = _decodeAlive
                    End SyncLock
                    If alive AndAlso w IsNot Nothing AndAlso w.VideoEof AndAlso
                       (_videoQueue Is Nothing OrElse _videoQueue.Count = 0) Then
                        Dim raiseEos As Boolean = False
                        SyncLock _lock
                            If _state = PlaybackState.Playing AndAlso _decodeAlive Then
                                _decodeAlive = False
                                _pausedTicks = MasterTicks()
                                _clock.Freeze()
                                _audio?.Pause()
                                _state = PlaybackState.Paused
                                _eofCount += 1L
                                raiseEos = True
                            End If
                        End SyncLock
                        If raiseEos Then
                            FireState(PlaybackState.Playing, PlaybackState.Paused)
                            ' Event BEFORE the waiter gate opens: anyone woken by
                            ' WaitForEof must already have seen EosReached (the
                            ' reverse order is a wake-before-notify race).
                            RaiseEvent EosReached(Me)
                            _eofEvent.Set()
                        End If
                    End If
                    Thread.Sleep(tickMs)
                    Continue While
                End If

                ' ---- stale generation guard (seek contract: never present) ----
                If hold.Generation < gen Then
                    SyncLock _holdLock
                        If _holdFrame Is hold Then _holdFrame = Nothing
                    End SyncLock
                    Interlocked.Increment(_staleGenerationsFlushed)
                    Try : hold.Dispose() : Catch : End Try
                    Continue While
                End If

                If st = PlaybackState.Paused Then
                    ' Paused: show only BRAND-NEW frames (post-seek target frame);
                    ' the sink already holds the freeze frame otherwise.
                    ' "New" is (generation, sequence)-ordered — a new generation
                    ' RESTARTS sequence at 0, so raw sequence comparison would
                    ' suppress every post-seek frame forever.
                    Dim lastGen = Volatile.Read(_sinkLastGeneration)
                    Dim lastSeq = Volatile.Read(_sinkLastSequence)
                    Dim isNew = hold.Generation > lastGen OrElse
                                (hold.Generation = lastGen AndAlso hold.Sequence > lastSeq)
                    If isNew Then
                        PresentFrame(hold)
                    End If
                    Thread.Sleep(tickMs)
                    Continue While
                End If

                ' ---- Playing: early / in-window / late decisions (§3.4/§3.6) ----
                Dim now = MasterTicks()
                Dim window = PlaybackClock.FrameWindowTicks(1.0 / Math.Max(1.0, _fps))
                Dim delta = hold.PtsTicks - now

                If delta < -window Then
                    SyncLock _holdLock
                        If _holdFrame Is hold Then _holdFrame = Nothing
                    End SyncLock
                    Interlocked.Increment(_droppedLate)
                    Try : hold.Dispose() : Catch : End Try
                    Continue While
                End If

                If delta > window Then
                    Thread.Sleep(tickMs) ' early — hold (bounded to this slot)
                    Continue While
                End If

                ' In window → present. Ownership: sink copies synchronously;
                ' frame disposed HERE (single dispose point, §2.1 discipline).
                PresentFrame(hold)
                SyncLock _holdLock
                    If _holdFrame Is hold Then _holdFrame = Nothing
                End SyncLock
                Try : hold.Dispose() : Catch : End Try
            End While

            ' Exit: drop the hold (stop/dispose path also flushes the queue).
            SyncLock _holdLock
                If _holdFrame IsNot Nothing Then
                    Try : _holdFrame.Dispose() : Catch : End Try
                    _holdFrame = Nothing
                End If
            End SyncLock
        End Sub

        Private Sub PresentFrame(frame As PlaybackFrame)
            Dim sink As IVideoRenderSink = Nothing
            SyncLock _lock
                sink = _sink
            End SyncLock
            If sink Is Nothing OrElse Not sink.IsAvailable Then Return
            Try
                sink.Present(frame)
                Volatile.Write(_sinkLastGeneration, frame.Generation)
                Volatile.Write(_sinkLastSequence, frame.Sequence)
                Volatile.Write(_lastPresentedPts, frame.PtsTicks)
                Interlocked.Increment(_framesPresented)
            Catch
                ' Sink-level failure must never kill the render loop; device
                ' loss is reported through the sink's IsAvailable (§3.8).
            End Try
        End Sub

        ' ================================================================
        '  Teardown / fault plumbing
        ' ================================================================

        ''' <summary>Release every live resource. `keepMedia` retains the probe
        ''' + path (Stop keeps them for Play-from-Stopped; Fault/Dispose too —
        ''' diagnostics remain dumpable, §3.5).</summary>
        Private Sub TeardownPipeline(Optional keepMedia As Boolean = True)
            Volatile.Write(_renderExitState, 1)

            Dim w As FfmpegDecodeWorker = Nothing
            SyncLock _lock
                w = _worker
                _worker = Nothing
                _decodeAlive = False
            End SyncLock
            If w IsNot Nothing Then
                Try : w.RequestStop() : Catch : End Try
            End If

            Dim rt As Thread = Nothing
            SyncLock _lock
                rt = _renderThread
            End SyncLock
            If rt IsNot Nothing AndAlso rt.IsAlive Then
                Try : rt.Join(2000) : Catch : End Try
            End If

            SyncLock _holdLock
                If _holdFrame IsNot Nothing Then
                    Try : _holdFrame.Dispose() : Catch : End Try
                    _holdFrame = Nothing
                End If
            End SyncLock

            Dim q As FrameQueue = Nothing
            Dim ring As AudioPcmBuffer = Nothing
            Dim audio As AudioRenderer = Nothing
            Dim sink As IVideoRenderSink = Nothing
            SyncLock _lock
                q = _videoQueue : _videoQueue = Nothing
                ring = _audioBuffer : _audioBuffer = Nothing
                audio = _audio : _audio = Nothing
                sink = _sink : _sink = Nothing
                _audioActive = False
                If Not keepMedia Then _media = Nothing
            End SyncLock

            If q IsNot Nothing Then Try : q.Dispose() : Catch : End Try
            If ring IsNot Nothing Then Try : ring.Dispose() : Catch : End Try
            If audio IsNot Nothing Then Try : audio.Dispose() : Catch : End Try
            If sink IsNot Nothing Then Try : sink.Dispose() : Catch : End Try
        End Sub

        ''' <summary>Bump the generation and raise the queue floor — every
        '  frame of every older generation becomes unreachable garbage.</summary>
        Private Function BumpGenerationAndFlush() As Long
            Dim newGen As Long
            SyncLock _lock
                _generation += 1L
                newGen = _generation
            End SyncLock
            If _videoQueue IsNot Nothing Then _videoQueue.Flush(newGen)
            If _audioBuffer IsNot Nothing Then _audioBuffer.Clear()
            Return newGen
        End Function

        ''' <summary>THE fault raiser. First fault wins; session becomes
        ''' Faulted (terminal, recoverable via Stop → fresh Open, §3.5). The
        ''' pipeline is torn down so nothing leaks behind the fault.
        ''' EXCEPTION — worker faults during Seeking do NOT escalate: §7
        ''' mandates seek failure recovers at the old position. SeekCore
        ''' observes the worker's fault and drives the recovery itself.</summary>
        Private Sub FaultOut(fault As GalleryVideoFault, Optional fromWorker As Boolean = False)
            If fault Is Nothing Then Return

            If fromWorker Then
                SyncLock _lock
                    If _state = PlaybackState.Seeking Then Return
                End SyncLock
            End If

            Dim fire As Boolean = False
            Dim old As PlaybackState = PlaybackState.Created

            SyncLock _lock
                If Volatile.Read(_disposedState) <> 0 Then Return
                If _fault IsNot Nothing Then Return ' first fault wins
                If PlaybackStateMachine.IsTerminal(_state) AndAlso _state <> PlaybackState.Stopped Then Return
                old = _state
                _fault = fault
                _state = PlaybackState.Faulted
                fire = True
            End SyncLock

            If fire Then
                Try
                    TeardownPipeline(keepMedia:=True)
                Catch
                End Try
                FireState(old, PlaybackState.Faulted)
                RaiseEvent FaultRaised(Me, fault)
                RaiseEvent OpenCompleted(Me, False, fault)
            End If
        End Sub

        ''' <summary>Fire StateChanged OUTSIDE the lock, guarded against a
        ''' disposed session (no stale callbacks, §3.5).</summary>
        Private Sub FireState(oldState As PlaybackState, newState As PlaybackState)
            If Volatile.Read(_disposedState) <> 0 Then Return
            RaiseEvent StateChanged(Me, oldState, newState)
        End Sub

    End Class

End Namespace
