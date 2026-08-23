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
            Dim tempH264 As String = Path.ChangeExtension(_config.OutputPath, ".tmp.h264")
            Dim tempVideoMp4 As String = Path.ChangeExtension(_config.OutputPath, ".tmp.video.mp4")
            Dim tempWav As String = Path.ChangeExtension(_config.OutputPath, ".tmp.wav")
            Dim tempMicWav As String = Path.ChangeExtension(_config.OutputPath, ".tmp.mic.wav")

            ' ─── Resources ───────────────────────────────────────────────
            Dim sink As BoundedVideoFrameSink = Nothing
            Dim audioCapture As WasapiLoopbackCapture = Nothing
            Dim wavStoppedEvent As New ManualResetEvent(False)
            Dim wavWriter As WavSidecarWriter = Nothing
            Dim wavReport As WavFinalizeReport = Nothing
            Dim videoFile As FileStream = Nothing
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

                    wavWriter = New WavSidecarWriter(tempWav, waveFormat.Channels, waveFormat.SampleRate, sidecarBits)
                    wavWriter.Start()

                    AddHandler audioCapture.DataAvailable, Sub(s, e)
                                                                If e.BytesRecorded > 0 Then
                                                                    If srcFloat Then
                                                                        Dim pcm As Byte() = ConvertFloatToPcm16(e.Buffer, e.BytesRecorded)
                                                                        wavWriter.EnqueueChunk(pcm, pcm.Length)
                                                                    Else
                                                                        wavWriter.EnqueueChunk(e.Buffer, e.BytesRecorded)
                                                                    End If
                                                                End If
                                                                If _stopSignal OrElse sw.Elapsed >= duration Then
                                                                    Try : audioCapture.StopRecording() : Catch : End Try
                                                                End If
                                                            End Sub

                    AddHandler audioCapture.RecordingStopped, Sub(s, e) wavStoppedEvent.Set()

                    ' PROVEN MODEL: capture the StartRecording CALL time —
                    ' NOT the first-callback time (historical -10s offset bug).
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

                        micWriter = New WavSidecarWriter(tempMicWav, micWaveFormat.Channels, micWaveFormat.SampleRate, micBits)
                        micWriter.Start()

                        AddHandler micCapture.DataAvailable, Sub(s, e)
                                                                  If e.BytesRecorded > 0 Then
                                                                      If micSrcFloat Then
                                                                          Dim pcm As Byte() = ConvertFloatToPcm16(e.Buffer, e.BytesRecorded)
                                                                          micWriter.EnqueueChunk(pcm, pcm.Length)
                                                                      Else
                                                                          micWriter.EnqueueChunk(e.Buffer, e.BytesRecorded)
                                                                      End If
                                                                  End If
                                                                  If _stopSignal OrElse sw.Elapsed >= duration Then
                                                                      Try : micCapture.StopRecording() : Catch : End Try
                                                                  End If
                                                              End Sub

                        AddHandler micCapture.RecordingStopped, Sub(s, e) micStoppedEvent.Set()

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

                ' ─── 3. Start video capture + encoder ─────────────────────
                _logger.Info("[session] Starting video capture...")
                _encoder.Start()
                _capture.Start(sink)

                ' ─── 4. Open video output file ───────────────────────────
                videoFile = New FileStream(tempH264, FileMode.Create, FileAccess.Write)
                _logger.Info($"[session] Writing H.264 to: {tempH264}")

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
                                    videoFile.Write(packet.Payload, 0, packet.PayloadLength)
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
                        Dim waitMs As Double = (nextTick - nowTicks) * 1000.0 / Stopwatch.Frequency
                        If waitMs > 2.5 Then Thread.Sleep(CInt(waitMs - 2.0))
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
                            videoFile.Write(packetF.Payload, 0, packetF.PayloadLength)
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

                ' ★ Sync fix (real-world drift): timestamp the END of frame
                ' delivery — after the final frame, before encoder teardown.
                ' Together with _videoStartTicks (first frame) this gives the
                ' true wall-clock span the encoded frames represent.
                Dim captureEndTicks As Long = Stopwatch.GetTimestamp()

                _logger.Info("[session] Stopping encoder...")
                _encoder.Stop()

                ' ─── 7. Close video file ────────────────────────────────
                videoFile.Flush()
                videoFile.Dispose()
                videoFile = Nothing

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

                    wavReport = wavWriter.Complete(5000)
                    _logger.Info("[session] " & wavReport.ToString())

                    result.AudioBytes = wavReport.BytesWritten
                    result.AudioDroppedBytes = wavReport.BytesDropped
                    result.AudioAccountingOk = wavReport.AccountingOk
                    result.AudioSamples = If(waveFormat IsNot Nothing,
                                             wavReport.BytesWritten \ (waveFormat.Channels * waveFormat.BitsPerSample \ 8),
                                             0)
                End If

                ' ★ M1: stop + finalize mic sidecar (independent #2) ──────
                If micCapture IsNot Nothing Then
                    _logger.Info("[session] Stopping mic sidecar...")
                    Try : micCapture.StopRecording() : Catch : End Try

                    If Not micStoppedEvent.WaitOne(3000) Then
                        _logger.Warning("[session] Mic RecordingStopped event timeout (3s) — finalizing anyway")
                    End If

                    micReport = micWriter.Complete(5000)
                    _logger.Info("[session] Mic " & micReport.ToString())

                    result.MicBytes = micReport.BytesWritten
                    result.MicDroppedBytes = micReport.BytesDropped
                    result.MicAccountingOk = micReport.AccountingOk
                    result.MicSamples = If(micWaveFormat IsNot Nothing,
                                           micReport.BytesWritten \ (micWaveFormat.Channels * micWaveFormat.BitsPerSample \ 8),
                                           0)
                End If

                result.ActualDurationSec = sw.Elapsed.TotalSeconds

                ' ─── 9. Wrap raw H.264 into MP4 ─────────────────────
                ' ★ REAL-WORLD SYNC FIX (owner test: recorded real music, lyrics
                ' desynced). Raw H.264 carries NO timestamps — the wrap's -r
                ' DEFINES playback speed. Old behavior (-r display refresh rate,
                ' decision 20932aa) assumed capture paced at exactly the display
                ' rate. Evidence says otherwise (12b sessions: 63-75fps on 75Hz;
                ' desktop-activity-dependent). Wrapping variable capture at a
                ' fixed 75 time-compresses the video: e.g. 68fps avg → ~9% fast
                ' → ~2.8s drift over 30s. Audio (wall-clock sidecar) cannot match.
                '
                ' Fix: wrap at the MEASURED average fps (framesEncoded / span).
                ' That preserves the wall-clock duration the audio also followed,
                ' so A/V stay aligned for the whole file. Display rate stays as
                ' fallback when the measurement is unavailable/absurd.
                ' (Long-term: per-frame PTS in a VFR container — Instant Replay
                ' research workstream; do-not-break rule says minimal fix now.)
                Dim refreshRate As Integer = _capture.OutputRefreshRate
                If refreshRate <= 0 Then refreshRate = 60 ' defensive fallback only

                Dim captureSpanSec As Double = 0.0
                Dim wrapFps As Double = refreshRate
                If _videoStartTicks > 0 AndAlso captureEndTicks > _videoStartTicks AndAlso result.FramesEncoded > 1 Then
                    captureSpanSec = (captureEndTicks - _videoStartTicks) / Stopwatch.Frequency
                    Dim measured As Double = result.FramesEncoded / captureSpanSec
                    If measured >= 1.0 AndAlso measured <= 500.0 Then
                        wrapFps = measured
                    End If
                    If refreshRate > 0 AndAlso Math.Abs(measured - refreshRate) / refreshRate > 0.05 Then
                        _logger.Warning($"[session] measured avg {measured:0.0}fps differs from display {refreshRate}Hz " &
                                        $"— old wrap would have drifted {((measured / refreshRate) - 1.0) * 100.0:0.0}% over time")
                    End If
                End If
                result.WrapFps = wrapFps
                Dim fpsArg As String = wrapFps.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)

                _logger.Info($"[session] Wrapping H.264 into MP4 @ {fpsArg}fps " &
                             $"(measured: {result.FramesEncoded} frames / {captureSpanSec:0.000}s; display: {refreshRate}Hz)")
                Dim wrapArgs As String = $"-y -hide_banner -f h264 -r {fpsArg} -i ""{tempH264}"" -c:v copy ""{tempVideoMp4}"""
                _logger.Info($"[session] Wrap command: {_config.FFmpegPath} {wrapArgs}")
                Dim wrapPsi As New ProcessStartInfo With {
                    .FileName = _config.FFmpegPath,
                    .Arguments = wrapArgs,
                    .UseShellExecute = False,
                    .RedirectStandardError = True,
                    .CreateNoWindow = True
                }
                Try
                    Using wrapProc As Process = Process.Start(wrapPsi)
                        ' Phase 12b: no-orphan-FFmpeg — assign to host job object.
                        Try : _config.OnProcessStarted?.Invoke(wrapProc) : Catch : End Try

                        Dim wrapStderr As String = wrapProc.StandardError.ReadToEnd()
                        If Not wrapProc.WaitForExit(30000) Then
                            Try : wrapProc.Kill() : Catch : End Try
                            _logger.Error("[session] H.264 wrap TIMEOUT (30s) — killed")
                        End If
                        If wrapProc.ExitCode <> 0 Then
                            _logger.Error($"[session] H.264 wrap failed: exit code {wrapProc.ExitCode}" &
                                          Environment.NewLine & "FFmpeg stderr:" & Environment.NewLine & wrapStderr)
                        Else
                            _logger.Info("[session] H.264 wrap succeeded")
                        End If
                    End Using
                Catch ex As Exception
                    _logger.Error($"[session] H.264 wrap threw: {ex.Message}")
                End Try

                ' ─── 10. Probe wrapped MP4 for exact duration ───────────
                ' The mux -t must use the CONTAINER duration (frame-accurate),
                ' not the wall-clock session time (which includes encoder
                ' spin-up and stop latency).
                Dim mux As New MuxCoordinator() With {
                    .FFmpegPath = _config.FFmpegPath,
                    .TempVideoPath = tempVideoMp4,
                    .TempSystemWavPath = tempWav,
                    .OutputPath = _config.OutputPath,
                    .HasSystemAudio = (_config.AudioEnabled AndAlso wavReport IsNot Nothing AndAlso wavReport.BytesWritten > 0),
                    .SystemVolume = _config.SystemVolume,
                    .TempMicWavPath = tempMicWav,
                    .HasMicAudio = (micReport IsNot Nothing AndAlso micReport.BytesWritten > 0),
                    .MicVolume = _config.MicVolume,
                    .SeparateTracks = _config.MicSeparateTracks,
                    .OnProcessStarted = _config.OnProcessStarted
                } ' M1: mic track wiring — MuxCoordinator already supports TempMicWavPath/HasMicAudio/MicOffsetSec/MicVolume

                Dim probedDuration As Double = mux.ProbeVideoDuration()
                If probedDuration > 0.001 Then
                    mux.VideoDurationSec = probedDuration
                    _logger.Info($"[session] ffprobe video duration: {probedDuration:0.000}s (container-accurate)")
                Else
                    mux.VideoDurationSec = result.ActualDurationSec
                    _logger.Warning($"[session] ffprobe failed — falling back to wall-clock {result.ActualDurationSec:0.000}s")
                End If
                result.MuxVideoDurationSec = mux.VideoDurationSec

                ' ─── 11. A/V sync offset (proven model) ──────────────────
                Dim systemOffset As Double = SyncMath.ComputeAudioOffsetSec(
                    _videoStartTicks, _systemStartTicks, Stopwatch.Frequency)
                mux.SystemOffsetSec = systemOffset
                result.SystemOffsetSec = systemOffset
                _logger.Info($"[session] Sync: videoStart={_videoStartTicks} sysStart={_systemStartTicks} " &
                             $"offset={systemOffset:0.000}s ({If(systemOffset > 0.001, "skip audio head", If(systemOffset < -0.001, "delay audio", "aligned"))})")

                ' ★ M1: mic offset — SAME proven model, mic's OWN timeline ──
                Dim micOffset As Double = SyncMath.ComputeAudioOffsetSec(
                    _videoStartTicks, _micStartTicks, Stopwatch.Frequency)
                mux.MicOffsetSec = micOffset
                result.MicOffsetSec = micOffset
                _logger.Info($"[session] Mic sync: micStart={_micStartTicks} " &
                             $"offset={micOffset:0.000}s ({If(micOffset > 0.001, "skip mic head", If(micOffset < -0.001, "delay mic", "aligned"))})")

                ' ─── 12. Mux ─────────────────────────────────────────────
                _logger.Info("[session] Muxing video + audio → MP4...")
                Dim muxOk As Boolean = mux.Run()
                If muxOk Then
                    _logger.Info("[session] Mux succeeded — cleaning temp files")
                    mux.CleanupTempFiles()
                    Try : File.Delete(tempH264) : Catch : End Try
                Else
                    _logger.Error("[session] Mux FAILED — keeping temp files for debugging")
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
                             $"wrapFps={result.WrapFps:0.000}, muxDur={result.MuxVideoDurationSec:0.000}s, " &
                             $"file_size={result.FileSize}")

            Catch ex As Exception
                result.ErrorMessage = ex.Message
                _logger.Error($"[session] Failed: {ex.Message}", ex)
            Finally
                Try : videoFile?.Dispose() : Catch : End Try
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
