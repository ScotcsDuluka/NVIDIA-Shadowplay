Option Strict On
Option Explicit On
Option Infer On

' Program.vb — Phase 12a-5b Integration Harness
'
' Minimal runtime gate harness for the architectural assumption:
'
'   Ddagrab's D3D11 device  →  creates staging texture  →  D3D11VideoFrame
'                                                              ↓
'                                                       (BORROW — sink owns)
'                                                              ↓
'   Encoder's D3D11 device (separate)  ←  CopyResource(frameTexture → encoderTexture)
'                                                              ↓
'                                                       NVENC encode
'                                                              ↓
'                                                       EncodedPacket
'
' The harness does NOT use RecordingEngine (12a-5 — deferred). It composes
' DdagrabBackend + BoundedVideoFrameSink + NvencEncoderBackend directly to
' prove the architectural assumption holds before orchestration is built.
'
' Two cases:
'   Case A: single run (Ddagrab → sink → encoder → dispose)
'   Case B: double run (run case A, dispose, run case A again)
'
'     Case B is the lifecycle contract gate — proves that resources are
'     properly released and a second Initialize() succeeds after a full
'     Dispose() of the first run.

Imports System.Diagnostics
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports CaptureEngine.Video.Handoff
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc

Module Program

    Private Const TestDurationSec As Integer = 5  ' short — just enough to prove the chain works
    Private Const SinkCapacity As Integer = 4
    Private Const HandoffPolicy As BoundedHandoffPolicy = BoundedHandoffPolicy.DropOldest

    Public Function Main(args As String()) As Integer
        Console.OutputEncoding = System.Text.Encoding.UTF8
        Console.WriteLine("============================================================")
        Console.WriteLine(" Phase 12a-5b — Integration Harness")
        Console.WriteLine(" Ddagrab → BoundedVideoFrameSink → NvencEncoderBackend")
        Console.WriteLine("============================================================")
        Console.WriteLine()

        Dim overallOk As Boolean = True

        ' ─── Case A: single run ─────────────────────────────────────────
        Console.WriteLine(">>> Case A: single run (5s)")
        Console.WriteLine()
        Dim caseA As RunResult = RunOnce("A")
        overallOk = overallOk AndAlso caseA.Pass
        PrintRunReport(caseA)
        Console.WriteLine()

        ' ─── Case B: second run after dispose ────────────────────────────
        Console.WriteLine(">>> Case B: second run (5s) after dispose")
        Console.WriteLine()
        Dim caseB As RunResult = RunOnce("B")
        overallOk = overallOk AndAlso caseB.Pass
        PrintRunReport(caseB)
        Console.WriteLine()

        ' ─── Final verdict ───────────────────────────────────────────────
        Console.WriteLine("============================================================")
        Console.WriteLine(" PHASE 12a-5b VERDICT")
        Console.WriteLine("============================================================")
        Console.WriteLine($"  Case A (single run):       {If(caseA.Pass, "PASS", "FAIL")}")
        Console.WriteLine($"  Case B (second run):       {If(caseB.Pass, "PASS", "FAIL")}")
        Console.WriteLine($"  Cross-device CopyResource: {If(caseA.Pass AndAlso caseB.Pass, "PROVEN", "UNPROVEN")}")
        Console.WriteLine($"  Resource lifecycle:       {If(caseA.Pass AndAlso caseB.Pass, "STABLE", "UNSTABLE")}")
        Console.WriteLine()
        Console.WriteLine($"  OVERALL: {If(overallOk, "PASS", "FAIL")}")
        Console.WriteLine("============================================================")

        Return If(overallOk, 0, 1)
    End Function

    ' ═══════════════════════════════════════════════════════════════════
    ' Single run: initialize Ddagrab + Encoder, capture 5s, encode all
    ' frames pulled from the sink, dispose.
    ' ═══════════════════════════════════════════════════════════════════

    Private Function RunOnce(caseId As String) As RunResult
        Dim result As New RunResult()
        result.CaseId = caseId

        Dim logger As New EngineLogger($"Harness.{caseId}", EngineLogger.LogLevel.Info, AddressOf Console.WriteLine)
        Dim encoderConfig As New EncoderConfig() With {
            .CodecKey = "NVENC_H264",
            .BitrateBps = 20_000_000L,
            .MinrateBps = 20_000_000L,
            .MaxrateBps = 20_000_000L,
            .BufsizeBps = 40_000_000L,
            .GopSize = 60,
            .RateControl = "cbr",
            .Preset = "p4"
        }

        Dim backend As DdagrabBackend = Nothing
        Dim sink As BoundedVideoFrameSink = Nothing
        Dim encoder As NvencEncoderBackend = Nothing

        Dim sw As Stopwatch = Stopwatch.StartNew()

        Try
            ' ─── Initialize D3D11 + DXGI duplication (persistent) ─────
            logger.Info($"[{caseId}] Initializing DdagrabBackend...")
            backend = New DdagrabBackend(logger)
            Dim ctx As New TestBackendContext(VideoBackendKind.Ddagrab, logger)
            backend.Initialize(ctx)
            logger.Info($"[{caseId}] DdagrabBackend state: {backend.CurrentState}")
            logger.Info($"[{caseId}] DdagrabBackend initialized — Output {backend.OutputWidth}x{backend.OutputHeight}")
            ' Configure encoder expected dimensions to match Ddagrab output.
            encoderConfig.ExpectedWidth = backend.OutputWidth
            encoderConfig.ExpectedHeight = backend.OutputHeight

            ' ─── Initialize NVENC encoder (separate D3D11 device!) ────
            logger.Info($"[{caseId}] Initializing NvencEncoderBackend...")
            encoder = New NvencEncoderBackend(logger)
            encoder.Initialize(encoderConfig)
            logger.Info($"[{caseId}] NvencEncoderBackend state: {encoder.CurrentState}")

            ' ─── Create sink + start backends ─────────────────────────
            sink = New BoundedVideoFrameSink(SinkCapacity, HandoffPolicy, logger)
            encoder.Start()
            backend.Start(sink)

            logger.Info($"[{caseId}] Running for {TestDurationSec}s...")
            Dim endTicks As Long = sw.Elapsed.Ticks + CLng(TestDurationSec) * Stopwatch.Frequency

            ' ─── Capture/encode loop ──────────────────────────────────
            Do While sw.Elapsed.Ticks < endTicks
                ' Pull frame from sink (non-blocking).
                Dim far As FrameAcquisitionResult
                If sink.TryTake(far) Then
                    result.FramesAcquired += 1
                    ' Encode the frame.
                    Dim packet As EncodedPacket = Nothing
                    Dim encodeOk As Boolean = False
                    Try
                        encodeOk = encoder.Encode(far.Frame, packet)
                    Catch ex As Exception
                        result.NvencErrors += 1
                        logger.Error($"[{caseId}] Encode threw: {ex.Message}", ex)
                    End Try
                    If encodeOk AndAlso packet IsNot Nothing Then
                        result.EncodedPackets += 1
                        result.EncodedBytes += CULng(packet.PayloadLength)
                        packet.Dispose()
                    ElseIf Not encodeOk Then
                        ' Encode returned False (backpressure / pipeline delay)
                        result.EncodeReturnedFalse += 1
                    End If
                    ' Frame is BORROWED by encoder (must NOT dispose inside Encode).
                    ' Encoder returned control → sink consumer owns frame now → dispose.
                    far.Frame?.Dispose()
                Else
                    ' Queue empty — short yield
                    Thread.Sleep(1)
                End If
            Loop

            ' ─── Stop backends ────────────────────────────────────────
            logger.Info($"[{caseId}] Stopping DdagrabBackend...")
            backend.Stop()
            logger.Info($"[{caseId}] Stopping NvencEncoderBackend...")
            encoder.Stop()

            ' Drain remaining frames in sink
            Dim far2 As FrameAcquisitionResult
            Do While sink.TryTake(far2)
                result.FramesAcquired += 1
                Dim packet2 As EncodedPacket = Nothing
                Try
                    If encoder.Encode(far2.Frame, packet2) AndAlso packet2 IsNot Nothing Then
                        result.EncodedPackets += 1
                        result.EncodedBytes += CULng(packet2.PayloadLength)
                        packet2.Dispose()
                    End If
                Catch ex As Exception
                    result.NvencErrors += 1
                End Try
                far2.Frame?.Dispose()
            Loop

            result.DurationSec = sw.Elapsed.TotalSeconds

            ' ─── Collect diagnostics ──────────────────────────────────
            result.FramesEmitted = backend.EmittedFrames
            result.FramesPushed = backend.FramesPushed
            result.FramesDroppedByBackend = backend.DroppedFrames
            result.FramesReplaced = backend.ReplacedFrames
            result.NoFrameCount = backend.NoFrameCount
            result.AccessLostCount = backend.AccessLostCount
            result.TexturesCreated = backend.TexturesCreated
            result.TexturesDisposed = backend.TexturesDisposed
            result.AchievedFps = If(result.DurationSec > 0,
                                    result.FramesAcquired / result.DurationSec, 0)

            ' After cleanup, verify leak invariant
            result.LeakInvariant = (result.TexturesCreated = result.TexturesDisposed)

            ' ─── Pass/Fail criteria ───────────────────────────────────
            result.Pass =
                result.FramesAcquired > 0 AndAlso
                result.EncodedPackets > 0 AndAlso
                result.EncodedBytes > 0 AndAlso
                result.NvencErrors = 0 AndAlso
                result.LeakInvariant

        Catch ex As Exception
            result.Pass = False
            result.ErrorMessage = ex.GetType().Name + ": " + ex.Message
            logger.Error($"[{caseId}] Harness failed: {ex.Message}", ex)
        Finally
            ' ─── Cleanup (reverse order) ──────────────────────────────
            Try : encoder?.Dispose() : Catch ex As Exception : logger.Warning($"encoder.Dispose: {ex.Message}") : End Try
            Try : backend?.Dispose() : Catch ex As Exception : logger.Warning($"backend.Dispose: {ex.Message}") : End Try
            Try : sink?.Dispose() : Catch ex As Exception : logger.Warning($"sink.Dispose: {ex.Message}") : End Try
        End Try

        Return result
    End Function

    Private Sub PrintRunReport(r As RunResult)
        Console.WriteLine("────────────────────────────────────────────────────────────")
        Console.WriteLine($" Case {r.CaseId} — Run Report")
        Console.WriteLine("────────────────────────────────────────────────────────────")
        Console.WriteLine($"  duration:              {r.DurationSec:F3} s")
        Console.WriteLine($"  frames_acquired:       {r.FramesAcquired}")
        Console.WriteLine($"  frames_emitted:        {r.FramesEmitted}")
        Console.WriteLine($"  frames_pushed:         {r.FramesPushed}")
        Console.WriteLine($"  frames_dropped:        {r.FramesDroppedByBackend}")
        Console.WriteLine($"  frames_replaced:       {r.FramesReplaced}")
        Console.WriteLine($"  no_frame_count:        {r.NoFrameCount}")
        Console.WriteLine($"  access_lost_count:     {r.AccessLostCount}")
        Console.WriteLine($"  achieved_fps:          {r.AchievedFps:F2}")
        Console.WriteLine()
        Console.WriteLine($"  textures_created:      {r.TexturesCreated}")
        Console.WriteLine($"  textures_disposed:      {r.TexturesDisposed}")
        Console.WriteLine($"  leak_invariant (cd==dd): {r.LeakInvariant}")
        Console.WriteLine()
        Console.WriteLine($"  encoded_packets:       {r.EncodedPackets}")
        Console.WriteLine($"  encoded_bytes:         {r.EncodedBytes}")
        Console.WriteLine($"  encode_returned_false: {r.EncodeReturnedFalse}")
        Console.WriteLine($"  nvenc_errors:          {r.NvencErrors}")
        Console.WriteLine()
        Console.WriteLine($"  pass:                   {r.Pass}")
        If Not String.IsNullOrEmpty(r.ErrorMessage) Then
            Console.WriteLine($"  error:                  {r.ErrorMessage}")
        End If
        Console.WriteLine("────────────────────────────────────────────────────────────")
    End Sub

    ' ═══════════════════════════════════════════════════════════════════
    ' Internal helpers
    ' ═══════════════════════════════════════════════════════════════════

    ''' <summary>
    ''' Minimal IVideoBackendContext for the harness. In production, this
    ''' is provided by the engine layer (RecordingEngine).
    ''' </summary>
    Private NotInheritable Class TestBackendContext
        Implements IVideoBackendContext

        Private ReadOnly _logger As EngineLogger
        Private ReadOnly _kind As VideoBackendKind

        Public Sub New(kind As VideoBackendKind, logger As EngineLogger)
            _kind = kind
            _logger = logger
        End Sub

        Public ReadOnly Property Logger As EngineLogger Implements IVideoBackendContext.Logger
            Get
                Return _logger
            End Get
        End Property

        Public ReadOnly Property BackendKind As VideoBackendKind Implements IVideoBackendContext.BackendKind
            Get
                Return _kind
            End Get
        End Property
    End Class

    Private Class RunResult
        Public CaseId As String
        Public DurationSec As Double
        Public FramesAcquired As Long
        Public FramesEmitted As Long
        Public FramesPushed As Long
        Public FramesDroppedByBackend As Long
        Public FramesReplaced As Long
        Public NoFrameCount As Long
        Public AccessLostCount As Long
        Public AchievedFps As Double
        Public TexturesCreated As Long
        Public TexturesDisposed As Long
        Public LeakInvariant As Boolean
        Public EncodedPackets As Long
        Public EncodedBytes As ULong
        Public EncodeReturnedFalse As Long
        Public NvencErrors As Long
        Public Pass As Boolean
        Public ErrorMessage As String = ""
    End Class

End Module
