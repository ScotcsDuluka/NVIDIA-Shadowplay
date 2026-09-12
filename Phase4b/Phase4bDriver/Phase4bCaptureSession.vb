Option Strict On
Option Explicit On
Option Infer On

' Phase4bCaptureSession.vb — EXPERIMENT-ONLY fork of production
' CaptureSession.vb (snapshot of HEAD cca2b2d, file dated 2026-09-11).
'
' ★ ห้ามแก้ production: the real CaptureEngine.Recording\CaptureSession.vb
' is NOT modified. This fork exists so the exact production timing logic can
' run A/B with (a) a parameterized timeline delay and (b) interface-typed
' backends (real NVENC/Ddagrab on the NVIDIA machine, synthetic/canned on
' the Intel causality machine).
'
' Documented deltas from production CaptureSession.vb (everything else is
' copied verbatim, CFR loop included):
'   D1. Class renamed Phase4bCaptureSession; backends typed to
'       IPhase4bVideoBackend / IEncoderBackend instead of the concrete
'       DdagrabBackend / NvencEncoderBackend (same members, delegation).
'   D2. The hardcoded 10ms timeline delay
'           _timelineStartTicks = Stopwatch.GetTimestamp() + Stopwatch.Frequency \ 10
'       becomes ctor parameter `_timelineDelayTicks`
'       (production arm = Stopwatch.Frequency \ 10, no-delay arm = 0).
'   D3. t0-mode "loop": re-arms the common timeline immediately before the
'       CFR loop starts (producers + mux are then genuinely armed before
'       T0 — the design intent stated by the production log line
'       "all producers armed before T0"). t0-mode "arm" = production order.
'   D4. Audio removed (video-only session): Phase 4B's focus chain is
'       Capture → Queue → CFR → NVENC → Packet → Mux → MP4 PTS; the audio
'       stage is out of scope. The shared AudioEngine wiring, the legacy
'       `If False` migration blocks, and mic sidecar are dropped; LiveMux
'       runs video-only (sysRate=0/micRate=0), BeginTimelines(0,0) inline.
'   D5. DeferredVideoFrameDisposer (Friend) replaced by the local
'       Phase4bDeferredFrameDisposer with the same contract.
'   D6. WasapiPositionCapture.StopwatchTicksTo100ns replaced by the local
'       TicksTo100ns with the identical overflow-safe formula.

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Handoff
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.FFmpegBackend
Imports Phase4b

Namespace CaptureEngine.Recording

    Public NotInheritable Class Phase4bCaptureSession
        Implements IDisposable

        ' ★ CFR PACING (production copy): 1ms timer resolution for the session.
        <DllImport("winmm.dll")>
        Private Shared Function timeBeginPeriod(period As UInteger) As Integer
        End Function

        <DllImport("winmm.dll")>
        Private Shared Function timeEndPeriod(period As UInteger) As Integer
        End Function

        Private ReadOnly _capture As IPhase4bVideoBackend
        Private ReadOnly _encoder As IEncoderBackend
        Private ReadOnly _config As SessionConfig
        Private ReadOnly _logger As EngineLogger
        Private ReadOnly _timelineDelayTicks As Long
        Private ReadOnly _armT0AtLoopEntry As Boolean
        Private _stopSignal As Boolean = False
        Private _stopRequestedTicks As Long = 0
        Private _disposed As Boolean = False

        ' Sync timeline (Stopwatch ticks)
        Private _systemStartTicks As Long = 0
        Private _videoStartTicks As Long = 0
        ' Single recording timeline origin shared by video and mux.
        Private _timelineStartTicks As Long = 0
        Private _timelineStartQpc100ns As Long = 0
        ' Device-mode video origin in the same 100ns QPC domain as WASAPI.
        Private _videoStartQpc100ns As Long = 0

        ' ★ OBS-style live mux (production copy).
        Private _liveMux As LiveMuxSession

        Public Sub New(capture As IPhase4bVideoBackend,
                       encoder As IEncoderBackend,
                       config As SessionConfig,
                       logger As EngineLogger,
                       timelineDelayTicks As Long,
                       armT0AtLoopEntry As Boolean)
            If capture Is Nothing Then Throw New ArgumentNullException(NameOf(capture))
            If encoder Is Nothing Then Throw New ArgumentNullException(NameOf(encoder))
            If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))
            If logger Is Nothing Then Throw New ArgumentNullException(NameOf(logger))
            _capture = capture
            _encoder = encoder
            _config = config
            _logger = logger
            _timelineDelayTicks = Math.Max(0L, timelineDelayTicks)
            _armT0AtLoopEntry = armT0AtLoopEntry
        End Sub

        Public Function Run() As SessionResult
            Dim result As New SessionResult() With {
                .OutputPath = _config.OutputPath,
                .RequestedDurationSec = _config.DurationSeconds,
                .AudioRequested = False   ' D4: video-only session
            }

            ' ─── Resources ───────────────────────────────────────────────
            Dim sink As BoundedVideoFrameSink = Nothing
            Dim frameDisposer As Phase4bDeferredFrameDisposer = Nothing

            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim duration As TimeSpan = TimeSpan.FromSeconds(_config.DurationSeconds)

            Dim captureRunning As Boolean = False
            Dim encoderRunning As Boolean = False

            Dim pendingFrame As IVideoFrame = Nothing
            Dim pendingSeq As Long = -1
            Dim lastFrame As IVideoFrame = Nothing

            ' One common recording clock. (Production line 279 — the 10ms
            ' delay becomes D2's parameter.)
            Dim armCallTicks As Long = Stopwatch.GetTimestamp()
            _timelineStartTicks = armCallTicks + _timelineDelayTicks
            _timelineStartQpc100ns = TicksTo100ns(_timelineStartTicks)
            _systemStartTicks = _timelineStartTicks
            _logger.Info($"[session] timeline arm call: ticks={armCallTicks} qpc100ns={TicksTo100ns(armCallTicks)} delayTicks={_timelineDelayTicks}")
            _logger.Info($"[session] common timeline armed: T0={_timelineStartQpc100ns} (all producers armed before T0)")

            Try
                timeBeginPeriod(1UI)
                _logger.Info("[session] timer resolution set to 1ms (CFR pacing)")
                sink = New BoundedVideoFrameSink(16, BoundedHandoffPolicy.DropOldest, _logger)
                frameDisposer = New Phase4bDeferredFrameDisposer()

                ' ─── 2b. ★ OBS-style LIVE MUX (video-only — D4) ─────────
                Dim targetFps As Integer = _config.TargetFps
                If targetFps <= 0 Then
                    targetFps = 60
                    _logger.Warning("[session] TargetFps missing/invalid — fallback 60fps")
                End If
                _logger.Info($"[session] video: fps source = config TargetFps = {targetFps} (display refresh {_capture.OutputRefreshRate}Hz is diagnostics-only)")
                _logger.Info($"[session] video: requested resolution = {If(_config.UseNativeResolution, "native", $"{_config.RequestedWidth}x{_config.RequestedHeight}")} → capture {_capture.OutputWidth}x{_capture.OutputHeight} → encode {_config.EncodeWidth}x{_config.EncodeHeight}")

                _liveMux = New LiveMuxSession(
                    _config.FFmpegPath,
                    _config.OutputPath,
                    targetFps,
                    0, 0,             ' system audio disabled (video-only experiment)
                    0, 0,             ' mic disabled
                    False,
                    1.0F, 1.0F,
                    Sub(m) _logger.Info(m),
                    _config.OnProcessStarted)
                If Not _liveMux.Start() Then
                    Throw New Exception("LiveMux failed to start (ffmpeg) — session aborted")
                End If
                _liveMux.BeginTimelines(0.0, 0.0)
                _logger.Info("[session] video-only session: mux timelines begin at offsets 0/0")

                ' ─── 3. Arm video capture + encoder BEFORE common T0 ────
                _logger.Info("[session] Arming video capture + encoder before common T0...")
                encoderRunning = True
                _encoder.Start()
                captureRunning = True
                _capture.Start(sink)

                _videoStartQpc100ns = _timelineStartQpc100ns
                _timelinesBegun = True
                _logger.Info("[session] Capture armed; common T0 committed to all timelines")

                ' ─── 5. CFR-paced capture/encode loop ────────────────
                Dim tickIntervalTicks As Long = CLng(Stopwatch.Frequency / targetFps)
                Dim nextTick As Long = _timelineStartTicks

                ' Phase 12c runtime telemetry.
                Dim selectedCount As Long = 0
                Dim selectedSeqLast As Long = -1
                Dim selectedSeqSkips As Long = 0
                Dim maxSelectedLag100ns As Long = 0
                Dim maxSourceGap100ns As Long = 0
                Dim totalEncodeTicks As Long = 0
                Dim maxEncodeTicks As Long = 0
                Dim maxTickLateTicks As Long = 0
                Dim lateTickCount As Long = 0
                Dim presentationTickCount As Long = 0
                Dim lastSelectedTs As Long = -1

                If _armT0AtLoopEntry Then
                    ' D3: re-arm T0 at loop entry — producers + mux are now
                    ' genuinely armed before T0 (the production comment's
                    ' stated design). The delay parameter still applies.
                    Dim rearmCallTicks As Long = Stopwatch.GetTimestamp()
                    _timelineStartTicks = rearmCallTicks + _timelineDelayTicks
                    _timelineStartQpc100ns = TicksTo100ns(_timelineStartTicks)
                    _videoStartQpc100ns = _timelineStartQpc100ns
                    nextTick = _timelineStartTicks
                    _logger.Info($"[session] RE-ARM call: ticks={rearmCallTicks} qpc100ns={TicksTo100ns(rearmCallTicks)} delayTicks={_timelineDelayTicks}")
                    _logger.Info($"[session] common timeline RE-ARMED at loop entry: T0={_timelineStartQpc100ns} (producers armed before T0 — design intent restored)")
                End If

                _logger.Info($"[session] Recording for {_config.DurationSeconds}s @ CFR {targetFps}fps...")

                Dim durationTicks As Long = CLng(duration.TotalSeconds * Stopwatch.Frequency)
                Dim encoderFaulted As Boolean = False

                ' D7 instrumentation: first successful encode→mux feed (the
                ' headline A/B metric) + per-tick selection lag stats.
                Dim firstFeedTick As Long = 0
                Dim firstFeedPresentationTick As Long = 0
                Dim firstFeedTargetQpc100ns As Long = 0
                Dim firstFeedSelectedTs As Long = 0
                Dim firstFeedIsDuplicate As Boolean = False
                Dim lagSamples As New List(Of Long)()
                Dim maxLagTickIndex As Long = 0
                Do While (Stopwatch.GetTimestamp() - _timelineStartTicks) < durationTicks AndAlso
                         Not Threading.Volatile.Read(_stopSignal) AndAlso
                         Not encoderFaulted
                    ' Pull only the next chronological source frame.
                    If pendingFrame Is Nothing Then
                        Dim far As FrameAcquisitionResult
                        If sink.TryTake(far) Then
                            result.FramesCaptured += 1
                            pendingFrame = far.Frame
                            pendingSeq = far.Sequence

                            ' ★ FORENSIC INSTRUMENTATION: Queue dequeue timing for first 4 frames
                            If pendingSeq <= 4 Then
                                Dim dequeueTick As Long = Stopwatch.GetTimestamp()
                                Dim dequeueQpc As Long = Stopwatch.GetTimestamp() ' Using Stopwatch as QPC equivalent
                                _logger.Info($"CaptureSession: FRAME {pendingSeq} QUEUE DEQUEUE:")
                                _logger.Info($"  dequeueTick={dequeueTick}")
                                _logger.Info($"  dequeueQpc={dequeueQpc}")
                                _logger.Info($"  frame.Diagnostics.CaptureTimeTicks={pendingFrame.Diagnostics.CaptureTimeTicks}")
                            End If
                        End If
                    End If

                    ' The recording timeline has one owner: common T0.
                    If _videoStartTicks = 0 AndAlso Stopwatch.GetTimestamp() >= _timelineStartTicks Then
                        _videoStartTicks = _timelineStartTicks
                        _videoStartQpc100ns = _timelineStartQpc100ns
                    End If

                    Dim nowTicks As Long = Stopwatch.GetTimestamp()
                    If nowTicks >= nextTick Then
                        ' ── CFR catch-up: emit every presentation tick that is already due. ──
                        Dim catchUpCount As Integer = 0
                        Do
                            If nextTick >= _timelineStartTicks + durationTicks Then Exit Do

                            Dim targetQpc100ns As Long = _timelineStartQpc100ns +
                                (CLng(Math.Max(0L, nextTick - _timelineStartTicks)) * 10000000L \ Stopwatch.Frequency)
                            Dim selectedFrame As IVideoFrame = Nothing
                            presentationTickCount += 1

                            ' ★ FORENSIC INSTRUMENTATION: CFR frame selection timing for first 4 presentation ticks
                            If presentationTickCount <= 4 Then
                                Dim cfrTick As Long = Stopwatch.GetTimestamp()
                                Dim cfrQpc As Long = Stopwatch.GetTimestamp() ' Using Stopwatch as QPC equivalent
                                _logger.Info($"CaptureSession: CFR TICK {presentationTickCount} SELECTION DEBUG:")
                                _logger.Info($"  targetQpc100ns={targetQpc100ns}")
                                _logger.Info($"  timelineStartQpc100ns={_timelineStartQpc100ns}")
                                _logger.Info($"  nextTick={nextTick}")
                                _logger.Info($"  _timelineStartTicks={_timelineStartTicks}")
                                _logger.Info($"  cfrTick={cfrTick}")
                                _logger.Info($"  cfrQpc={cfrQpc}")
                            End If

                            While pendingFrame IsNot Nothing AndAlso
                                  pendingFrame.Diagnostics.CaptureTimeTicks <= targetQpc100ns
                                ' Keep only the newest eligible source frame.
                                If selectedFrame IsNot Nothing Then
                                    frameDisposer.Enqueue(selectedFrame)
                                End If
                                Dim selectedSeq As Long = pendingSeq
                                If selectedSeqLast >= 0 AndAlso selectedSeq > selectedSeqLast + 1 Then selectedSeqSkips += selectedSeq - selectedSeqLast - 1
                                If selectedSeq >= 0 Then selectedSeqLast = selectedSeq
                                selectedCount += 1
                                Dim selectedTs As Long = pendingFrame.Diagnostics.CaptureTimeTicks
                                Dim selectedLag As Long = targetQpc100ns - selectedTs
                                lagSamples.Add(selectedLag)
                                If selectedLag > maxSelectedLag100ns Then
                                    maxSelectedLag100ns = selectedLag
                                    maxLagTickIndex = presentationTickCount
                                End If
                                If lastSelectedTs >= 0 AndAlso selectedTs >= lastSelectedTs Then
                                    Dim sourceGap As Long = selectedTs - lastSelectedTs
                                    If sourceGap > maxSourceGap100ns Then maxSourceGap100ns = sourceGap
                                End If
                                lastSelectedTs = selectedTs
                                selectedFrame = pendingFrame
                                pendingFrame = Nothing
                                pendingSeq = -1

                                Dim nextSource As FrameAcquisitionResult
                                If sink.TryTake(nextSource) Then
                                    result.FramesCaptured += 1
                                    pendingFrame = nextSource.Frame
                                    pendingSeq = nextSource.Sequence
                                End If

                                ' ★ FORENSIC INSTRUMENTATION: Selected frame timing for first 4 presentation ticks
                                If presentationTickCount <= 4 Then
                                    Dim selectedTick As Long = Stopwatch.GetTimestamp()
                                    Dim selectedQpc As Long = Stopwatch.GetTimestamp() ' Using Stopwatch as QPC equivalent
                                    _logger.Info($"CaptureSession: CFR TICK {presentationTickCount} SELECTED FRAME:")
                                    _logger.Info($"  selectedSeq={selectedSeq}")
                                    _logger.Info($"  selectedTs={selectedTs}")
                                    _logger.Info($"  selectedLag={selectedLag}ns")
                                    _logger.Info($"  selectedTick={selectedTick}")
                                    _logger.Info($"  selectedQpc={selectedQpc}")
                                End If
                            End While

                            If selectedFrame IsNot Nothing Then
                                If lastFrame IsNot Nothing Then
                                    frameDisposer.Enqueue(lastFrame)
                                End If
                                lastFrame = selectedFrame
                            End If

                            Dim encodeFrame As IVideoFrame = lastFrame

                            Dim isDuplicate As Boolean = (selectedFrame Is Nothing AndAlso lastFrame IsNot Nothing)
                            Dim encodeTicks As Long = 0
                            If encodeFrame IsNot Nothing Then
                                Dim packet As EncodedPacket = Nothing
                                Dim encodeStartTicks As Long = Stopwatch.GetTimestamp()
                                Try
                                    If _encoder.Encode(encodeFrame, packet) AndAlso packet IsNot Nothing Then
                                        ' ★ FORENSIC INSTRUMENTATION: Muxer feed timing for first 4 presentation ticks
                                        If presentationTickCount <= 4 Then
                                            Dim muxFeedTick As Long = Stopwatch.GetTimestamp()
                                            Dim muxFeedQpc As Long = Stopwatch.GetTimestamp() ' Using Stopwatch as QPC equivalent
                                            _logger.Info($"CaptureSession: CFR TICK {presentationTickCount} MUXER FEED:")
                                            _logger.Info($"  muxFeedTick={muxFeedTick}")
                                            _logger.Info($"  muxFeedQpc={muxFeedQpc}")
                                            _logger.Info($"  packet.Metadata.PresentationTimestampTicks={packet.Metadata.PresentationTimestampTicks}")
                                            _logger.Info($"  packet.Metadata.Sequence={packet.Metadata.Sequence}")
                                        End If

                                        _liveMux?.FeedVideo(packet.Payload, packet.PayloadLength)
                                        If firstFeedTick = 0 Then
                                            firstFeedTick = Stopwatch.GetTimestamp()
                                            firstFeedPresentationTick = presentationTickCount
                                            firstFeedTargetQpc100ns = targetQpc100ns
                                            firstFeedSelectedTs = If(encodeFrame IsNot Nothing, encodeFrame.Diagnostics.CaptureTimeTicks, 0)
                                            firstFeedIsDuplicate = isDuplicate
                                        End If
                                        result.TotalVideoBytes += packet.PayloadLength
                                        result.FramesEncoded += 1
                                        packet.Dispose()
                                    End If
                                Catch ex As Exception
                                    result.NvencErrors += 1
                                    _logger.Error($"[session] Encode error: {ex.Message}")
                                    If _encoder.CurrentState = EncoderState.Faulted Then
                                        _logger.Error("[session] Encoder Faulted — aborting CFR loop")
                                        encoderFaulted = True
                                        Exit Do
                                    End If
                                End Try

                                encodeTicks = Stopwatch.GetTimestamp() - encodeStartTicks
                                totalEncodeTicks += encodeTicks
                                If encodeTicks > maxEncodeTicks Then maxEncodeTicks = encodeTicks

                                If isDuplicate Then
                                    result.FramesDuplicated += 1
                                End If
                            End If

                            Dim tickDoneTicks As Long = Stopwatch.GetTimestamp()
                            Dim tickLateTicks As Long = tickDoneTicks - nextTick
                            If tickLateTicks > maxTickLateTicks Then maxTickLateTicks = tickLateTicks
                            If tickLateTicks > tickIntervalTicks Then lateTickCount += 1

                            nextTick += tickIntervalTicks
                            catchUpCount += 1
                            nowTicks = Stopwatch.GetTimestamp()
                        Loop While catchUpCount < 8 AndAlso
                                   nextTick <= nowTicks AndAlso
                                   Not Threading.Volatile.Read(_stopSignal)

                    Else
                        ' Sleep toward the next tick (production copy).
                        Dim waitMs As Double = (nextTick - nowTicks) * 1000.0 / Stopwatch.Frequency
                        If waitMs > 1.0 Then
                            Thread.Sleep(1)
                        ElseIf waitMs > 0.05 Then
                            Thread.SpinWait(200)
                        End If
                    End If
                Loop

                ' ─── 6. Stop video → final fresh frame → stop encoder ──
                Dim stopQpcTicks As Long = Interlocked.Read(_stopRequestedTicks)
                If stopQpcTicks <= 0 Then stopQpcTicks = Stopwatch.GetTimestamp()
                Dim stopElapsedSeconds As Double = Math.Min(duration.TotalSeconds, Math.Max(0.0, (stopQpcTicks - _timelineStartTicks) / CDbl(Stopwatch.Frequency)))

                _logger.Info($"[session] Stop snapshot: elapsed={stopElapsedSeconds:F3}s")
                _logger.Info("[session] Stopping video capture...")
                _capture.Stop()
                captureRunning = False
                _logger.Info($"[session] capture diagnostics: emitted={_capture.Diagnostics.EmittedFrames}, pushed={_capture.FramesPushed}, dropped={_capture.Diagnostics.DroppedFrames}, replaced={_capture.Diagnostics.ReplacedFrames}, noFrame={_capture.Diagnostics.NoFrameCount}, errors={_capture.Diagnostics.ErrorCount}, accessLost={_capture.AccessLostCount}, textures={_capture.TexturesCreated}/{_capture.TexturesDisposed}")

                Dim avgEncodeMs As Double = If(result.FramesEncoded > 0, totalEncodeTicks * 1000.0 / Stopwatch.Frequency / result.FramesEncoded, 0.0)
                Dim maxEncodeMs As Double = maxEncodeTicks * 1000.0 / Stopwatch.Frequency
                Dim maxSelectedLagMs As Double = maxSelectedLag100ns / 10000.0
                Dim maxSourceGapMs As Double = maxSourceGap100ns / 10000.0
                Dim maxTickLateMs As Double = maxTickLateTicks * 1000.0 / Stopwatch.Frequency
                _logger.Info($"[session] CFR telemetry: ticks={presentationTickCount}, selectedSources={selectedCount}, seqSkips={selectedSeqSkips}, maxSelectedLag={maxSelectedLagMs:0.###}ms (tick {maxLagTickIndex}), maxSourceGap={maxSourceGapMs:0.###}ms, avgEncode={avgEncodeMs:0.###}ms, maxEncode={maxEncodeMs:0.###}ms, lateTicks>{1000.0 * tickIntervalTicks / Stopwatch.Frequency:0.###}ms={lateTickCount}, maxTickLate={maxTickLateMs:0.###}ms")

                ' D7: the headline metric — when did the FIRST packet reach
                ' the mux, at which CFR tick, with which frame, relative to T0.
                If firstFeedTick > 0 Then
                    Dim feedMinusT0Ms As Double = (firstFeedTick - _timelineStartTicks) * 1000.0 / Stopwatch.Frequency
                    Dim contentMinusT0Ms As Double = (firstFeedSelectedTs - _timelineStartQpc100ns) / 10000.0
                    Dim lagStats As String = ""
                    If lagSamples.Count > 0 Then
                        Dim sorted As New List(Of Long)(lagSamples)
                        sorted.Sort()
                        Dim p50 As Double = sorted(sorted.Count \ 2) / 10000.0
                        Dim p95 As Double = sorted(CInt(Math.Min(sorted.Count - 1, Math.Floor(sorted.Count * 0.95)))) / 10000.0
                        lagStats = $", lagP50={p50:0.###}ms, lagP95={p95:0.###}ms"
                    End If
                    _logger.Info($"[session] FIRST ENCODE FEED: presentationTick={firstFeedPresentationTick} feedMinusT0Ms={feedMinusT0Ms:0.###} contentMinusT0Ms={contentMinusT0Ms:0.###} targetQpc100ns={firstFeedTargetQpc100ns} selectedCaptureQpc100ns={firstFeedSelectedTs} isDuplicate={firstFeedIsDuplicate}{lagStats}")
                Else
                    _logger.Info("[session] FIRST ENCODE FEED: none (no frame encoded during the CFR loop)")
                End If

                ' Drain: keep only the FRESHEST leftover frame.
                Dim farDrain As FrameAcquisitionResult
                While sink.TryTake(farDrain)
                    result.FramesCaptured += 1
                    Dim f As IVideoFrame = farDrain.Frame
                    If f Is Nothing Then Continue While
                    If farDrain.Sequence > pendingSeq Then
                        frameDisposer.Enqueue(pendingFrame)
                        pendingFrame = f
                        pendingSeq = farDrain.Sequence
                    Else
                        frameDisposer.Enqueue(f)
                    End If
                End While

                ' Encode the final fresh frame (if any), then tail-fill.
                Dim finalFrame As IVideoFrame = pendingFrame
                If finalFrame Is Nothing Then finalFrame = lastFrame

                If finalFrame IsNot Nothing Then
                    If pendingFrame IsNot Nothing Then
                        Dim packetF As EncodedPacket = Nothing
                        Try
                            If _encoder.Encode(pendingFrame, packetF) AndAlso packetF IsNot Nothing Then
                                _liveMux?.FeedVideo(packetF.Payload, packetF.PayloadLength)
                                result.TotalVideoBytes += packetF.PayloadLength
                                result.FramesEncoded += 1
                                packetF.Dispose()

                                frameDisposer.Enqueue(lastFrame)
                                lastFrame = pendingFrame
                                pendingFrame = Nothing
                                finalFrame = lastFrame
                            End If
                        Catch
                            result.NvencErrors += 1
                        End Try
                    End If

                    Dim actualSessionSeconds As Double = stopElapsedSeconds
                    Dim targetFrames As Long = CLng(Math.Ceiling(actualSessionSeconds * targetFps))
                    Dim fillBefore As Long = result.FramesEncoded
                    While result.FramesEncoded < targetFrames AndAlso finalFrame IsNot Nothing
                        Dim packetPad As EncodedPacket = Nothing
                        Try
                            If Not _encoder.Encode(finalFrame, packetPad) OrElse packetPad Is Nothing Then
                                result.NvencErrors += 1
                                Exit While
                            End If
                            _liveMux?.FeedVideo(packetPad.Payload, packetPad.PayloadLength)
                            result.TotalVideoBytes += packetPad.PayloadLength
                            result.FramesEncoded += 1
                            result.FramesDuplicated += 1
                            packetPad.Dispose()
                        Catch
                            result.NvencErrors += 1
                            Exit While
                        End Try
                    End While
                    If result.FramesEncoded > fillBefore Then
                        _logger.Info($"[session] CFR tail-fill: +{result.FramesEncoded - fillBefore} frames (target {targetFrames})")
                    End If
                End If
                If pendingFrame IsNot Nothing AndAlso pendingFrame IsNot lastFrame Then
                    frameDisposer.Enqueue(pendingFrame)
                End If
                frameDisposer.Enqueue(lastFrame)
                pendingFrame = Nothing
                lastFrame = Nothing
                frameDisposer.CompleteAndWait()

                _logger.Info("[session] GPU frame disposer drained; stopping encoder...")
                _encoder.Stop()
                encoderRunning = False

                _stopSignal = True

                result.ActualDurationSec = stopElapsedSeconds

                ' ★ OBS-model: finalize the LIVE MUX (production copy).
                result.WrapFps = targetFps
                Dim liveRes As LiveMuxResult = _liveMux.[Stop](30000)
                _logger.Info("[session] " & liveRes.ToString())

                result.MuxDroppedBytes = liveRes.DroppedBytes
                If liveRes.DroppedBytes > 0 Then
                    _logger.Warning($"[session] live-mux dropped {liveRes.DroppedBytes:N0}B — file is missing captured video (pass will report False)")
                End If

                ' ─── 10. Probe final MP4 duration (production copy) ──────
                Dim probePsi As New ProcessStartInfo With {
                    .FileName = _config.FFmpegPath,
                    .Arguments = "-hide_banner -i """ & _config.OutputPath & """",
                    .UseShellExecute = False,
                    .RedirectStandardError = True,
                    .CreateNoWindow = True
                }
                Try
                    Using probeProc As Process = Process.Start(probePsi)
                        Dim probeErrTask As System.Threading.Tasks.Task(Of String) = probeProc.StandardError.ReadToEndAsync()
                        If Not probeProc.WaitForExit(5000) Then
                            Try : probeProc.Kill() : probeProc.WaitForExit(2000) : Catch : End Try
                        End If
                        Dim probeErr As String = If(probeErrTask.Wait(1000), probeErrTask.Result, "")
                        Dim m As System.Text.RegularExpressions.Match =
                            System.Text.RegularExpressions.Regex.Match(probeErr, "Duration:\s*(\d+):(\d+):(\d+\.?\d*)")
                        If m.Success Then
                            result.MuxVideoDurationSec =
                                CInt(m.Groups(1).Value) * 3600 + CInt(m.Groups(2).Value) * 60 + CDbl(m.Groups(3).Value)
                        End If
                    End Using
                Catch
                End Try

                ' Video-only experiment: audio offsets are out of scope (D4).
                result.SystemOffsetSec = 0.0
                result.MicOffsetSec = 0.0

                ' ─── 13. Verify MP4 (production copy) ───────────────────
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
                            Dim verifyErrTask As System.Threading.Tasks.Task(Of String) = verifyProc.StandardError.ReadToEndAsync()
                            If Not verifyProc.WaitForExit(5000) Then
                                Try : verifyProc.Kill() : verifyProc.WaitForExit(2000) : Catch : End Try
                            End If
                            Dim stderr As String = If(verifyErrTask.Wait(1000), verifyErrTask.Result, "")
                            result.VideoStreamFound = stderr.Contains("Stream #") AndAlso stderr.Contains("Video:")
                            result.AudioStreamFound = stderr.Contains("Stream #") AndAlso stderr.Contains("Audio:")
                        End Using
                    Catch
                    End Try
                End If

                _logger.Info($"[session] Result: pass={result.Pass}, frames={result.FramesEncoded} " &
                             $"(captured={result.FramesCaptured}, dup={result.FramesDuplicated}), " &
                             $"video_bytes={result.TotalVideoBytes}, " &
                             $"cfrFps={result.WrapFps:0.###}, duration={result.MuxVideoDurationSec:0.000}s, " &
                             $"file_size={result.FileSize}")

            Catch ex As Exception
                result.ErrorMessage = ex.Message
                _logger.Error($"[session] Failed: {ex.Message}", ex)
            Finally
                Try : timeEndPeriod(1UI) : Catch : End Try
                If captureRunning Then
                    Try : _capture.Stop() : Catch ex As Exception
                        _logger.Warning("[session] capture stop during failure unwind: " & ex.Message)
                    End Try
                End If
                If encoderRunning Then
                    Try : _encoder.Stop() : Catch ex As Exception
                        _logger.Warning("[session] encoder stop during failure unwind: " & ex.Message)
                    End Try
                End If
                Try : pendingFrame?.Dispose() : Catch : End Try
                Try : lastFrame?.Dispose() : Catch : End Try
                Try : _liveMux?.Dispose() : Catch : End Try
                Try : frameDisposer?.Dispose() : Catch : End Try
                Try : sink?.Dispose() : Catch : End Try
            End Try

            Return result
        End Function

        ''' <summary>Production WasapiPositionCapture.StopwatchTicksTo100ns —
        ''' overflow-safe (D6): (ticks/freq)×1e7 + (ticks%freq)×1e7/freq.
        ''' NOTE VB precedence: `\` binds LOWER than `*`, so the parentheses
        ''' below are load-bearing (without them the product swallows the
        ''' division and the result wraps every second).</summary>
        Friend Shared Function TicksTo100ns(qpcTicks As Long) As Long
            Dim f As Long = Stopwatch.Frequency
            Return (qpcTicks \ f) * 10000000L + ((qpcTicks Mod f) * 10000000L) \ f
        End Function

        Public Sub [Stop]()
            Interlocked.CompareExchange(_stopRequestedTicks, Stopwatch.GetTimestamp(), 0)
            Threading.Volatile.Write(_stopSignal, True)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            _stopSignal = True
        End Sub

        ' D4: _timelinesBegun kept as a session field for the arm-order log.
        Private _timelinesBegun As Boolean = False

    End Class

End Namespace
