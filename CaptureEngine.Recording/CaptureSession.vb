Option Strict On
Option Explicit On
Option Infer On

' CaptureSession.vb
'
' Per-session resource owner. Composes:
'   - BoundedVideoFrameSink (frame queue)
'   - System audio → WavSidecarWriter (bounded queue + writer thread)
'   - Raw H.264 file (native NVENC packets)
'   - FFmpeg wrap (raw H.264 → temp MP4 @ display refresh rate)
'   - MuxCoordinator (video + audio → final MP4, A/V sync offsets)
'
' Lifecycle (per session):
'   1. Start audio sidecar (WASAPI loopback → WavSidecarWriter queue)
'   2. Start video capture (DdagrabBackend.Start(sink)) + encoder
'   3. Capture/encode loop: sink.Take → encoder.Encode → write H.264 to file
'   4. Stop video → drain sink → stop encoder
'   5. Stop audio → event-driven WAV finalize (accounting invariant)
'   6. FFmpeg wrap: H.264 → temp MP4 at DISPLAY REFRESH RATE
'   7. ffprobe temp MP4 → exact video duration
'   8. FFmpeg mux with SyncMath offsets → final MP4
'   9. Verify MP4 streams → SessionResult
'
' Phase 12b audio model (AUDIO hard-blocker rework):
'   PRODUCER  : WASAPI DataAvailable callback → copy → enqueue. NO disk I/O
'               on the callback thread (proven AudioFileWriter lesson).
'   CONSUMER  : WavSidecarWriter dedicated thread drains bounded queue.
'   ACCOUNTING: enqueued = written + dropped (residual must be 0).
'   SYNC      : systemStartTicks captured at StartRecording() CALL time
'               (NOT first callback — avoids the historical -10s bug);
'               videoStartTicks at first frame taken from the sink;
'               offset = (videoStart - systemStart)/freq clamped [-2,+5]
'               (proven legacy model, extracted into SyncMath).
'   FINALIZE  : RecordingStopped event → ManualResetEvent wait (no Sleep).
'
' Ownership:
'   - BORROWS _capture + _encoder from RecordingEngine (does NOT dispose)
'   - OWNS sink + sidecar writer + audio capture + H.264 file + temp files

Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports NAudio.Wave
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports CaptureEngine.Video.Handoff
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.FFmpegBackend

Namespace CaptureEngine.Recording

    Public NotInheritable Class CaptureSession
        Implements IDisposable

        Private ReadOnly _capture As DdagrabBackend
        Private ReadOnly _encoder As NvencEncoderBackend
        Private ReadOnly _config As SessionConfig
        Private ReadOnly _logger As EngineLogger
        Private _stopSignal As Boolean = False
        Private _disposed As Boolean = False

        ' Sync timeline (Stopwatch ticks)
        Private _systemStartTicks As Long = 0
        Private _videoStartTicks As Long = 0
        ' ★ M1: mic has its own independent timeline (device clock/cadence
        ' differ from system loopback — never share or derive one from other)
        Private _micStartTicks As Long = 0

        ' ★ OBS-style live mux (owner directive): one FFmpeg process receives
        ' video+audio through named pipes while recording — no temp files,
        ' no post-hoc wrap/mux, wall-clock-true by construction.
        Private _liveMux As LiveMuxSession

        ' ★ OBS trick: render endless silence to the loopback device while
        ' recording so WASAPI delivers callbacks CONTINUOUSLY (silence included).
        ' Kills gap-fill/steering/noise-at-silence at the source.
        Private _silenceKeepAlive As SilenceKeepAlive

        ' ★ AudioTap instances (single gap-fill engine for both tracks)
        Private _sysTap As AudioTap
        Private _micTap As AudioTap

        Private Sub SysPipeFeed(data As Byte(), count As Integer)
            _liveMux?.FeedSystemAudio(data, count)
        End Sub

        Private Sub MicPipeFeed(data As Byte(), count As Integer)
            _liveMux?.FeedMicAudio(data, count)
        End Sub

        ' Dual sink: one stream to live-mux pipe (+ optional WAV evidence sidecar
        ' — disabled by default per audit #7; the pipe is the real output).
        Private Class AudioTapSinkDual
            Implements IAudioTapSink

            Private ReadOnly _wav As WavSidecarWriter
            Private ReadOnly _pipe As Action(Of Byte(), Integer)

            Public Sub New(wav As WavSidecarWriter, pipe As Action(Of Byte(), Integer))
                _wav = wav          ' may be Nothing (evidence sidecar disabled)
                _pipe = pipe
            End Sub

            Public Sub Write(data As Byte(), count As Integer) Implements IAudioTapSink.Write
                _wav?.EnqueueChunk(data, count)
                _pipe?.Invoke(data, count)
            End Sub
        End Class

        Public Sub New(capture As DdagrabBackend,
                      encoder As NvencEncoderBackend,
                      config As SessionConfig,
                      logger As EngineLogger)
            _capture = capture
            _encoder = encoder
            _config = config
            _logger = logger
        End Sub

        Public Function Run() As SessionResult
            Dim result As New SessionResult() With {
                .OutputPath = _config.OutputPath,
                .RequestedDurationSec = _config.DurationSeconds
            }

            ' ─── Temp file paths ─────────────────────────────────────────
            Dim tempWav As String = Path.ChangeExtension(_config.OutputPath, ".tmp.wav")
            Dim tempMicWav As String = Path.ChangeExtension(_config.OutputPath, ".tmp.mic.wav")

            ' ─── Resources ───────────────────────────────────────────────
            Dim sink As BoundedVideoFrameSink = Nothing
            Dim audioCapture As WasapiLoopbackCapture = Nothing
            Dim wavStoppedEvent As New ManualResetEvent(False)
            Dim wavWriter As WavSidecarWriter = Nothing
            Dim wavReport As WavFinalizeReport = Nothing
            Dim waveFormat As WaveFormat = Nothing

            ' ★ M1: mic sidecar — mirror of the system sidecar, fully independent
            ' (own device, own queue, own writer, own accounting, own timeline)
            Dim micCapture As NAudio.CoreAudioApi.WasapiCapture = Nothing
            Dim micStoppedEvent As New ManualResetEvent(False)
            Dim micWriter As WavSidecarWriter = Nothing
            Dim micWaveFormat As WaveFormat = Nothing
            Dim micReport As WavFinalizeReport = Nothing

            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim duration As TimeSpan = TimeSpan.FromSeconds(_config.DurationSeconds)

            Try
                ' ─── 1. Create sink ──────────────────────────────────────
                sink = New BoundedVideoFrameSink(4, BoundedHandoffPolicy.DropOldest, _logger)

                ' ─── 2. Start audio sidecar ─────────────────────────────
                If _config.AudioEnabled Then
                    _logger.Info("[session] Starting audio sidecar...")
                    audioCapture = New WasapiLoopbackCapture()
                    waveFormat = audioCapture.WaveFormat
                    _logger.Info($"[session] Audio: {waveFormat.Channels}ch {waveFormat.SampleRate}Hz {waveFormat.BitsPerSample}bit {waveFormat.Encoding}")

                    ' WavSidecarWriter is PCM (16/24/32). WASAPI loopback on
                    ' shared mode is typically IeeeFloat 32-bit → convert to
                    ' PCM16 here so the sidecar stays a plain PCM WAV.
                    Dim srcFloat As Boolean = (waveFormat.Encoding = WaveFormatEncoding.IeeeFloat)
                    Dim sidecarBits As Integer = If(srcFloat, 32, waveFormat.BitsPerSample)
                    ' NOTE: 32-bit int PCM is NOT the same as float — WavSidecarWriter
                    ' takes raw frames; for float sources we convert below and keep 16-bit.
                    If srcFloat Then sidecarBits = 16

                    If _config.EvidenceSidecar Then
                        wavWriter = New WavSidecarWriter(tempWav, waveFormat.Channels, waveFormat.SampleRate, sidecarBits)
                        wavWriter.Start()
                    Else
                        _logger.Info("[session] evidence WAV sidecar disabled (live-mux is the output)")
                    End If

                    ' ★ WASAPI loopback gap-filling (real-bug fix, owner evidence
                    ' 2026-08-23 18:42: 16.28s session but WAV had only 3.54s —
                    '  loopback delivers callbacks ONLY while sound is actually
                    '  playing; silence = NO data at all. A naive byte-concat WAV
                    ' is 3.5s of sound taped together — the mux then places that
                    ' audio at the file start: 'music before I played any'.
                    '  Fix (same model as legacy AudioPipe pre-roll/gap logic):
                    '  track the last filled timestamp; when a callback arrives
                    '  after a gap, insert silence for the elapsed gap FIRST,
                    '  then the real audio. WAV duration == wall-clock duration,
                    '  so mux offsets hold for the entire file.
                    ' ★ AudioTap consolidation (GLM/6 audit): ONE gap-fill engine for
                    ' both tracks (previously duplicated logic). The tap receives
                    ' CONVERTED pcm16 bytes, feeds WAV sidecar + live-mux pipe with
                    ' one wall-clock timeline, measures the device's first-callback
                    ' warm-up, and closes the tail at stop. Lead = the systemic
                    ' audio-lead calibration (shifts the stream earlier by the
                    ' device-latency bias).
                    Dim sysTapBits As Integer = If(srcFloat, 16, waveFormat.BitsPerSample)
                    _sysTap = New AudioTap(
                        "sys",
                        waveFormat.SampleRate,
                        waveFormat.Channels,
                        sysTapBits,
                        New AudioTapSinkDual(wavWriter, AddressOf SysPipeFeed),
                        0.0,
                        Sub(m) _logger.Info("[session] " & m))

                    AddHandler audioCapture.DataAvailable, Sub(s, e)
                                                                If e.BytesRecorded > 0 Then
                                                                    If srcFloat Then
                                                                        Dim pcm As Byte() = ConvertFloatToPcm16(e.Buffer, e.BytesRecorded)
                                                                        _sysTap.Feed(pcm, pcm.Length)
                                                                    Else
                                                                        _sysTap.Feed(e.Buffer, e.BytesRecorded)
                                                                    End If
                                                                End If
                                                                If _stopSignal OrElse sw.Elapsed >= duration Then
                                                                    Try : audioCapture.StopRecording() : Catch : End Try
                                                                End If
                                                            End Sub

                    AddHandler audioCapture.RecordingStopped, Sub(s, e) wavStoppedEvent.Set()

                    ' ★ Keep-alive BEFORE capture start: rendering silence now
                    ' means the loopback's FIRST callback arrives within ~10ms
                    ' (the mixer is already active) — the 5.7s warm-up gap and
                    ' its pre-roll reconstruction never happen.
                    If _config.SilenceKeepAlive Then
                        _silenceKeepAlive = New SilenceKeepAlive()
                        _logger.Info($"[session] silence keep-alive: {If(_silenceKeepAlive.IsActive, "ACTIVE on " & _silenceKeepAlive.DeviceName, "unavailable — tap gap-fill remains active")} (OBS-style continuous loopback)")
                    Else
                        _logger.Info("[session] silence keep-alive disabled by config — AudioTap gap-fill active (proven path)")
                    End If

                    ' PROVEN MODEL: capture the StartRecording CALL time —
                    ' NOT the first-callback time (historical -10s offset bug).
                    _sysTap.MarkStart()
                    audioCapture.StartRecording()
                    _systemStartTicks = Stopwatch.GetTimestamp()
                    _logger.Info("[session] Audio sidecar started (bounded queue + writer thread)")
                Else
                    _logger.Info("[session] Audio DISABLED by config — video-only session")
                End If

                ' ★ M1: Start MIC sidecar (independent #2) ──────────────────
                ' Mirror of the system sidecar. Hard rules (GPT standing design):
                '   - own WasapiCapture (device-selected), own WavSidecarWriter
                '   - own start timestamp (micStartTicks) — never derived from
                '     system ticks; the two devices have independent clocks
                '   - failure to open the mic must NOT kill the session — the
                '     track is dropped and logged (system audio continues)
                If _config.MicEnabled Then
                    Try
                        _logger.Info("[session] Starting mic sidecar...")
                        Dim micDevice As NAudio.CoreAudioApi.MMDevice = FindMicDevice(_config.MicDeviceId, _config.MicDeviceName)
                        micCapture = If(micDevice IsNot Nothing, New NAudio.CoreAudioApi.WasapiCapture(micDevice), New NAudio.CoreAudioApi.WasapiCapture())
                        micWaveFormat = micCapture.WaveFormat
                        _logger.Info($"[session] Mic: {micWaveFormat.Channels}ch {micWaveFormat.SampleRate}Hz {micWaveFormat.BitsPerSample}bit {micWaveFormat.Encoding}")

                        Dim micSrcFloat As Boolean = (micWaveFormat.Encoding = WaveFormatEncoding.IeeeFloat)
                        Dim micBits As Integer = If(micSrcFloat, 16, micWaveFormat.BitsPerSample)

                        If _config.EvidenceSidecar Then
                            micWriter = New WavSidecarWriter(tempMicWav, micWaveFormat.Channels, micWaveFormat.SampleRate, micBits)
                            micWriter.Start()
                        End If

                        ' ★ AUDIT FIX consolidated via AudioTap (same engine as system)
                        _micTap = New AudioTap(
                            "mic",
                            micWaveFormat.SampleRate,
                            micWaveFormat.Channels,
                            micBits,
                            New AudioTapSinkDual(micWriter, AddressOf MicPipeFeed),
                            0.0,
                            Sub(m) _logger.Info("[session] " & m))

                        AddHandler micCapture.DataAvailable, Sub(s, e)
                                                                  If e.BytesRecorded > 0 Then
                                                                      If micSrcFloat Then
                                                                          Dim pcm As Byte() = ConvertFloatToPcm16(e.Buffer, e.BytesRecorded)
                                                                          _micTap.Feed(pcm, pcm.Length)
                                                                      Else
                                                                          _micTap.Feed(e.Buffer, e.BytesRecorded)
                                                                      End If
                                                                  End If
                                                                  If _stopSignal OrElse sw.Elapsed >= duration Then
                                                                      Try : micCapture.StopRecording() : Catch : End Try
                                                                  End If
                                                              End Sub

                        AddHandler micCapture.RecordingStopped, Sub(s, e) micStoppedEvent.Set()

                        _micTap.MarkStart()
                        micCapture.StartRecording()
                        _micStartTicks = Stopwatch.GetTimestamp()
                        _logger.Info("[session] Mic sidecar started (independent writer)")
                    Catch ex As Exception
                        ' Mic is a second track — its failure degrades, never kills.
                        _logger.Warning($"[session] Mic sidecar FAILED to start (track dropped): {ex.Message}")
                        Try : micCapture?.Dispose() : Catch : End Try
                        micCapture = Nothing
                        Try : micWriter?.Dispose() : Catch : End Try
                        micWriter = Nothing
                        _micStartTicks = 0
                    End Try
                End If

                ' ─── 2b. ★ OBS-style LIVE MUX: one FFmpeg, both streams, wall clock ──
                ' OWNER directive: record video+audio together, both follow the
                ' clock, both end together (replaces temp-H.264 + WAV + wrap + mux).
                Dim refreshRate As Integer = _capture.OutputRefreshRate
                If refreshRate <= 0 Then refreshRate = 60

                Dim micRate As Integer = If(micWaveFormat IsNot Nothing, micWaveFormat.SampleRate, 0)
                Dim micCh As Integer = If(micWaveFormat IsNot Nothing, micWaveFormat.Channels, 0)
                Dim sysRate As Integer = If(waveFormat IsNot Nothing, waveFormat.SampleRate, 48000)
                Dim sysCh As Integer = If(waveFormat IsNot Nothing, waveFormat.Channels, 2)

                _liveMux = New LiveMuxSession(
                    _config.FFmpegPath,
                    _config.OutputPath,
                    refreshRate,
                    sysRate, sysCh,
                    micRate, micCh,
                    _config.MicSeparateTracks,
                    _config.SystemVolume,
                    _config.MicVolume,
                    Sub(m) _logger.Info(m))
                If Not _liveMux.Start() Then
                    Throw New Exception("LiveMux failed to start (ffmpeg) — session aborted")
                End If

                ' ─── 3. Start video capture + encoder ─────────────────────
                _logger.Info("[session] Starting video capture...")
                _encoder.Start()
                _capture.Start(sink)

                ' ─── 4. (video bytes now stream into the live-mux pipe) ──

                ' ─── 5. CFR-paced capture/encode loop ────────────────
                ' ★ AUDIT FIX (owner: 'video still looks weird'). Full-path audit
                ' from zero found the core defect: DXGI Desktop Duplication only
                ' delivers frames when the screen CHANGES (0..refreshRate fps —
                ' inherently VFR). Raw H.264 cannot carry timestamps, so a
                ' constant-rate wrap of a VFR stream SMEARS content: a 10s static
                ' scene (≈5 frames) + 20s of motion (≈1500 frames) becomes
                ' 1505 uniformly-spaced frames — static compressed to ~0.1s,
                ' motion stretched to ~30s. No -r choice can fix that.
                '
                ' Fix = CFR pacing (what real ShadowPlay / OBS CFR / the legacy
                ' engine's DuplicateFrames do): pace encoding at a CONSTANT rate
                ' (display refresh). Each tick encodes the FRESHEST captured
                ' frame; when the screen was static (no new frame), re-encode the
                ' last frame → a tiny P-frame duplicate. Result: true CFR stream,
                ' exact wall-clock timing at every point, static scenes simply
                ' repeat frames like normal video. Re-encoding is safe: the
                ' encoder CopyResources the frame texture into its own texture
                ' on every Encode call.
                Dim targetFps As Integer = _capture.OutputRefreshRate
                If targetFps <= 0 Then targetFps = 60
                Dim tickIntervalTicks As Long = CLng(Stopwatch.Frequency / targetFps)
                Dim nextTick As Long = 0                     ' 0 = t0 not established yet
                Dim pendingFrame As IVideoFrame = Nothing     ' freshest captured, not yet displayed
                Dim pendingSeq As Long = -1
                Dim lastFrame As IVideoFrame = Nothing        ' last displayed (for duplicates)

                _logger.Info($"[session] Recording for {_config.DurationSeconds}s @ CFR {targetFps}fps...")

                Do While sw.Elapsed < duration AndAlso Not _stopSignal
                    ' Refresh 'pending' with the freshest frame (drop older).
                    Dim far As FrameAcquisitionResult
                    While sink.TryTake(far)
                        result.FramesCaptured += 1
                        Dim f As IVideoFrame = far.Frame
                        If f Is Nothing Then Continue While
                        If far.Sequence > pendingSeq Then
                            pendingFrame?.Dispose()
                            pendingFrame = f
                            pendingSeq = far.Sequence
                        Else
                            f.Dispose()   ' out-of-order older frame
                        End If
                    End While

                    ' t0 = first REAL captured frame (video timeline origin).
                    If _videoStartTicks = 0 Then
                        If pendingFrame Is Nothing Then
                            Thread.Sleep(1)
                            Continue Do
                        End If
                        _videoStartTicks = Stopwatch.GetTimestamp()
                        nextTick = _videoStartTicks

                        ' ★ OBS-model alignment: SyncMath offsets applied at FEED time.
                        ' (Owner run 20:58 showed a=0B + exit -22: this call was lost
                        ' in a merge — the audio PipeFeed waited forever for the
                        ' timeline event, nothing was ever written, ffmpeg died.)
                        ' ★ SELF-AUDIT FIX (regression in 342f808): I removed the
                        ' SyncMath offset here believing it double-compensated with
                        ' the tap's pre-roll. Sign analysis over the whole chain
                        ' proves the OPPOSITE — the two steps are complementary:
                        '   tap pre-roll builds the stream from AUDIO START
                        '   (wall-clock true, device-latency lead applied inside)
                        '   this discard trims the head to VIDEO t0 so both pipes
                        '   share the same origin (video pipe t0 = first frame).
                        ' Without it the audio runs EARLY by the variable
                        ' audio-start→video-t0 delta (0.05-0.5s per session logs)
                        ' — the residual 'not stable' the owner kept reporting.
                        ' Division of labor: AudioTap applies the device-latency
                        ' LEAD (its ctor param); the pipe applies the ORIGIN
                        ' OFFSET (SyncMath). One value each, no overlap.
                        ' ★ Lead lives HERE now (single place): the tap builds a pure
                        ' wall-clock stream (lead 0); the pipe origin offset carries
                        ' BOTH the audio-start→video-t0 trim AND the systemic
                        ' device-latency lead.
                        Dim sysOff As Double = SyncMath.ComputeAudioOffsetSec(
                            _videoStartTicks, _systemStartTicks, Stopwatch.Frequency) + SyncMath.SystemAudioLeadSec
                        Dim micOff As Double = SyncMath.ComputeAudioOffsetSec(
                            _videoStartTicks, _micStartTicks, Stopwatch.Frequency) + SyncMath.MicAudioLeadSec
                        _logger.Info($"[session] timeline origin: sys offset={sysOff:0.000}s mic offset={micOff:0.000}s (t0 trim + lead {SyncMath.SystemAudioLeadSec:0.000}s)")
                        _liveMux?.BeginTimelines(sysOff, micOff)
                    End If

                    Dim nowTicks As Long = Stopwatch.GetTimestamp()
                    If nowTicks >= nextTick Then
                        ' ── Tick: encode freshest frame, or duplicate the last ──
                        Dim encodeFrame As IVideoFrame = If(pendingFrame, lastFrame)
                        Dim isDuplicate As Boolean = (pendingFrame Is Nothing AndAlso lastFrame IsNot Nothing)

                        If encodeFrame IsNot Nothing Then
                            Dim packet As EncodedPacket = Nothing
                            Try
                                If _encoder.Encode(encodeFrame, packet) AndAlso packet IsNot Nothing Then
                                    _liveMux?.FeedVideo(packet.Payload, packet.PayloadLength)
                                    result.TotalVideoBytes += packet.PayloadLength
                                    result.FramesEncoded += 1
                                    packet.Dispose()
                                End If
                            Catch ex As Exception
                                result.NvencErrors += 1
                                _logger.Error($"[session] Encode error: {ex.Message}")
                            End Try

                            ' Ownership handoff: pending → last (old last disposed).
                            If pendingFrame IsNot Nothing Then
                                lastFrame?.Dispose()
                                lastFrame = pendingFrame
                                pendingFrame = Nothing
                                pendingSeq = -1
                            ElseIf isDuplicate Then
                                result.FramesDuplicated += 1
                            End If
                        End If

                        ' Advance the schedule. If we fell behind badly (encode
                        ' stall), skip missed ticks instead of burst-catch-up
                        ' (bursting would recreate the smear locally).
                        nextTick += tickIntervalTicks
                        If Stopwatch.GetTimestamp() >= nextTick + tickIntervalTicks Then
                            nextTick = Stopwatch.GetTimestamp()
                        End If
                    Else
                        ' Sleep toward the next tick (keep ~2ms spin margin for
                        ' Stop() responsiveness and tick precision).
                        ' ★ AUDIT FIX: at 240fps (4.17ms interval) waitMs is
                        ' ALWAYS < 2.5 → Thread.Sleep never fires → a full CPU
                        ' core spins at 100%. Use SpinWait for the sub-2.5ms
                        ' remainder instead of a hot loop.
                        Dim waitMs As Double = (nextTick - nowTicks) * 1000.0 / Stopwatch.Frequency
                        If waitMs > 2.5 Then
                            Thread.Sleep(CInt(waitMs - 2.0))
                        ElseIf waitMs > 0.2 Then
                            Thread.SpinWait(50)   ' ~sub-ms yield, keeps core free-ish
                        End If
                    End If
                Loop

                ' ─── 6. Stop video → final fresh frame → stop encoder ──
                _logger.Info("[session] Stopping video capture...")
                _capture.Stop()

                ' Drain: keep only the FRESHEST leftover frame (stale ones just
                ' dispose — the CFR stream already displayed newer states).
                Dim farDrain As FrameAcquisitionResult
                While sink.TryTake(farDrain)
                    result.FramesCaptured += 1
                    Dim f As IVideoFrame = farDrain.Frame
                    If f Is Nothing Then Continue While
                    If farDrain.Sequence > pendingSeq Then
                        pendingFrame?.Dispose()
                        pendingFrame = f
                        pendingSeq = farDrain.Sequence
                    Else
                        f.Dispose()
                    End If
                End While

                ' Encode the final fresh frame (if any) as the last frame.
                If pendingFrame IsNot Nothing Then
                    Dim packetF As EncodedPacket = Nothing
                    Try
                        If _encoder.Encode(pendingFrame, packetF) AndAlso packetF IsNot Nothing Then
                            _liveMux?.FeedVideo(packetF.Payload, packetF.PayloadLength)
                            result.TotalVideoBytes += packetF.PayloadLength
                            result.FramesEncoded += 1
                            packetF.Dispose()
                        End If
                    Catch
                        result.NvencErrors += 1
                    End Try
                    pendingFrame.Dispose()
                    pendingFrame = Nothing
                End If

                pendingFrame = Nothing   ' nothing left to dispose
                lastFrame?.Dispose()
                lastFrame = Nothing

                _logger.Info("[session] Stopping encoder...")
                _encoder.Stop()

                ' ─── 8. Stop audio → event-driven WAV finalize ──────────
                If audioCapture IsNot Nothing Then
                    _logger.Info("[session] Stopping audio sidecar...")
                    _stopSignal = True
                    Try : audioCapture.StopRecording() : Catch : End Try

                    ' Event-driven wait — replaces the old Thread.Sleep(500)
                    ' race. WASAPI fires RecordingStopped exactly once.
                    If Not wavStoppedEvent.WaitOne(3000) Then
                        _logger.Warning("[session] RecordingStopped event timeout (3s) — finalizing anyway")
                    End If

                    ' Stop the keep-alive FIRST (stop rendering silence), then
                    ' finalize the tap (the tail-pad still covers the last ~ms).
                    Try
                        If _silenceKeepAlive IsNot Nothing Then
                            _silenceKeepAlive.Dispose()
                            _silenceKeepAlive = Nothing
                        End If
                    Catch
                    End Try

                    ' ★ AudioTap closes the timeline: tail-pad from the last
                    ' callback to now (covers 'music stopped before stop was
                    ' pressed'), or the FULL span if the device never delivered
                    ' (fully silent session — else ffmpeg gets zero packets).
                    ' Warm-up + silence totals are logged as calibration evidence.
                    _sysTap?.FinalizeToNow()

                    If wavWriter IsNot Nothing Then
                        wavReport = wavWriter.Complete(5000)
                        _logger.Info("[session] " & wavReport.ToString())

                        result.AudioBytes = wavReport.BytesWritten
                        result.AudioDroppedBytes = wavReport.BytesDropped
                        result.AudioAccountingOk = wavReport.AccountingOk
                        result.AudioSamples = If(waveFormat IsNot Nothing,
                                                 wavReport.BytesWritten \ (waveFormat.Channels * waveFormat.BitsPerSample \ 8),
                                                 0)
                    Else
                        ' Evidence disabled: report the tap totals instead.
                        result.AudioBytes = If(_sysTap IsNot Nothing, _sysTap.DataBytes, 0)
                        result.AudioAccountingOk = True
                    End If
                End If

                ' ★ M1: stop + finalize mic sidecar (independent #2) ──────
                If micCapture IsNot Nothing Then
                    _logger.Info("[session] Stopping mic sidecar...")
                    Try : micCapture.StopRecording() : Catch : End Try

                    If Not micStoppedEvent.WaitOne(3000) Then
                        _logger.Warning("[session] Mic RecordingStopped event timeout (3s) — finalizing anyway")
                    End If

                    _micTap?.FinalizeToNow()

                    If micWriter IsNot Nothing Then
                        micReport = micWriter.Complete(5000)
                        _logger.Info("[session] Mic " & micReport.ToString())

                        result.MicBytes = micReport.BytesWritten
                        result.MicDroppedBytes = micReport.BytesDropped
                        result.MicAccountingOk = micReport.AccountingOk
                        result.MicSamples = If(micWaveFormat IsNot Nothing,
                                               micReport.BytesWritten \ (micWaveFormat.Channels * micWaveFormat.BitsPerSample \ 8),
                                               0)
                    Else
                        result.MicBytes = If(_micTap IsNot Nothing, _micTap.DataBytes, 0)
                        result.MicAccountingOk = True
                    End If
                End If

                result.ActualDurationSec = sw.Elapsed.TotalSeconds

                ' ★ OBS-model: finalize the LIVE MUX (drain pipes → FFmpeg EOF
                ' → finalize fragmented MP4 → +faststart remux). The declared CFR
                ' rate and the wall-clock audio feed already define the timeline;
                ' no wrap step, no post-hoc offsets.
                result.WrapFps = refreshRate
                Dim liveRes As LiveMuxResult = _liveMux.[Stop](30000)
                _logger.Info("[session] " & liveRes.ToString())

                ' ─── 10. Probe final MP4 duration (evidence) ─────────────
                Dim probePsi As New ProcessStartInfo With {
                    .FileName = _config.FFmpegPath,
                    .Arguments = "-hide_banner -i """ & _config.OutputPath & """",
                    .UseShellExecute = False,
                    .RedirectStandardError = True,
                    .CreateNoWindow = True
                }
                Try
                    Using probeProc As Process = Process.Start(probePsi)
                        Dim probeErr As String = probeProc.StandardError.ReadToEnd()
                        probeProc.WaitForExit(5000)
                        Dim m As System.Text.RegularExpressions.Match =
                            System.Text.RegularExpressions.Regex.Match(probeErr, "Duration:\s*(\d+):(\d+):(\d+\.?\d*)")
                        If m.Success Then
                            result.MuxVideoDurationSec =
                                CInt(m.Groups(1).Value) * 3600 + CInt(m.Groups(2).Value) * 60 + CDbl(m.Groups(3).Value)
                        End If
                    End Using
                Catch
                End Try

                ' Evidence: system audio offset that the live feed applied
                Dim systemOffset As Double = SyncMath.ComputeAudioOffsetSec(
                    _videoStartTicks, _systemStartTicks, Stopwatch.Frequency)
                result.SystemOffsetSec = systemOffset
                Dim micOffset As Double = SyncMath.ComputeAudioOffsetSec(
                    _videoStartTicks, _micStartTicks, Stopwatch.Frequency)
                result.MicOffsetSec = micOffset

                ' Clean the evidence WAV on success (keep on failure for debug)
                If liveRes.Succeeded Then
                    Try : File.Delete(tempWav) : Catch : End Try
                End If


                ' ─── 13. Verify MP4 ─────────────────────────────────────
                Dim fi As New FileInfo(_config.OutputPath)
                result.FileExists = fi.Exists
                result.FileSize = If(fi.Exists, fi.Length, 0)

                If fi.Exists Then
                    Dim verifyPsi As New ProcessStartInfo With {
                        .FileName = _config.FFmpegPath,
                        .Arguments = $"-hide_banner -i ""{_config.OutputPath}""",
                        .UseShellExecute = False,
                        .RedirectStandardError = True,
                        .CreateNoWindow = True
                    }
                    Try
                        Using verifyProc As Process = Process.Start(verifyPsi)
                            Try : _config.OnProcessStarted?.Invoke(verifyProc) : Catch : End Try
                            Dim stderr As String = verifyProc.StandardError.ReadToEnd()
                            verifyProc.WaitForExit(5000)
                            result.VideoStreamFound = stderr.Contains("Stream #") AndAlso stderr.Contains("Video:")
                            result.AudioStreamFound = stderr.Contains("Stream #") AndAlso stderr.Contains("Audio:")
                        End Using
                    Catch
                    End Try
                End If

                _logger.Info($"[session] Result: pass={result.Pass}, frames={result.FramesEncoded} " &
                             $"(captured={result.FramesCaptured}, dup={result.FramesDuplicated}), " &
                             $"video_bytes={result.TotalVideoBytes}, audio_bytes={result.AudioBytes}, " &
                             $"dropped={result.AudioDroppedBytes}, offset={result.SystemOffsetSec:0.000}s, " &
                             $"cfrFps={result.WrapFps:0.###}, duration={result.MuxVideoDurationSec:0.000}s, " &
                             $"file_size={result.FileSize}")

            Catch ex As Exception
                result.ErrorMessage = ex.Message
                _logger.Error($"[session] Failed: {ex.Message}", ex)
            Finally
                Try : _silenceKeepAlive?.Dispose() : Catch : End Try
                Try : _liveMux?.Dispose() : Catch : End Try
                Try : wavWriter?.Dispose() : Catch : End Try
                Try : audioCapture?.Dispose() : Catch : End Try
                ' ★ M1: mic sidecar cleanup mirrors the system sidecar
                Try : micWriter?.Dispose() : Catch : End Try
                Try : micCapture?.Dispose() : Catch : End Try
                Try : sink?.Dispose() : Catch : End Try
                Try : wavStoppedEvent.Dispose() : Catch : End Try
                Try : micStoppedEvent.Dispose() : Catch : End Try
            End Try

            Return result
        End Function

        ''' <summary>
        ''' ★ M1: resolve the microphone MMDevice (id first, then FriendlyName).
        ''' Ported from the PROVEN legacy AudioFileWriter.FindMicDevice — same
        ''' resolution order (exact ID match → exact FriendlyName match → Nothing).
        ''' Nothing = caller falls back to the default capture device.
        ''' </summary>
        Private Shared Function FindMicDevice(deviceId As String, deviceName As String) As NAudio.CoreAudioApi.MMDevice
            Using devEnum As New NAudio.CoreAudioApi.MMDeviceEnumerator()
                Dim devices As NAudio.CoreAudioApi.MMDeviceCollection =
                    devEnum.EnumerateAudioEndPoints(NAudio.CoreAudioApi.DataFlow.Capture,
                                                    NAudio.CoreAudioApi.DeviceState.Active)

                If Not String.IsNullOrEmpty(deviceId) Then
                    For Each dev As NAudio.CoreAudioApi.MMDevice In devices
                        If dev.ID = deviceId Then Return dev
                    Next
                End If

                If Not String.IsNullOrEmpty(deviceName) Then
                    For Each dev As NAudio.CoreAudioApi.MMDevice In devices
                        If String.Equals(dev.FriendlyName, deviceName, StringComparison.Ordinal) Then Return dev
                    Next
                End If

                Return Nothing
            End Using
        End Function

        ''' <summary>
        ''' Convert IEEE-float32 interleaved PCM into signed 16-bit PCM.
        ''' Allocation per chunk is acceptable: chunks are ~10ms and the copy
        ''' already happens once (buffer ownership rule).
        ''' </summary>
        Private Shared Function ConvertFloatToPcm16(buffer As Byte(), bytesRecorded As Integer) As Byte()
            Dim sampleCount As Integer = bytesRecorded \ 4
            Dim out(sampleCount * 2 - 1) As Byte
            For i As Integer = 0 To sampleCount - 1
                Dim f As Single = BitConverter.ToSingle(buffer, i * 4)
                If Single.IsNaN(f) OrElse Single.IsInfinity(f) Then f = 0.0F
                If f > 1.0F Then f = 1.0F
                If f < -1.0F Then f = -1.0F
                Dim v As Integer = CInt(Math.Round(f * 32767.0F))
                If v > 32767 Then v = 32767
                If v < -32768 Then v = -32768
                Dim u As UShort = CUShort(v And &HFFFFI)
                out(i * 2) = CByte(u And &HFFUI)
                out(i * 2 + 1) = CByte((u >> 8) And &HFFUI)
            Next
            Return out
        End Function

        Public Sub [Stop]()
            _stopSignal = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            _stopSignal = True
            ' NOTE: does NOT dispose _capture or _encoder — those are owned by RecordingEngine
        End Sub

    End Class

End Namespace
