Option Strict On
Option Explicit On
Option Infer On

' SoakInfra.vb — shared plumbing for the C/3 real-hardware soak:
' the production backend stack, the session runner, the media validator,
' and the failure recorder. Written in deliberately conservative VB
' (top-level classes, no import aliases, no single-line Try) after the
' first draft tripped a vbc declaration-parsing cascade.

Imports System
Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.Recording
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports SharpGen.Runtime
Imports Vortice.Direct3D
Imports Vortice.Direct3D11
Imports Vortice.DXGI

Friend Class SoakFailures
    Friend Shared ReadOnly List As New List(Of String)()
    Friend Shared ReadOnly SyncRoot As New Object()

    Friend Shared Sub Record(scope As String, message As String)
        SyncLock SyncRoot
            List.Add("[" & DateTime.Now.ToString("HH:mm:ss.fff") & "] " & scope & ": " & message)
        End SyncLock
        SyncLock SoakConsole.SyncRoot
            Console.WriteLine("      FAIL " & scope & ": " & message)
        End SyncLock
    End Sub
End Class

Friend Class SoakConsole
    Friend Shared ReadOnly SyncRoot As New Object()
End Class

Friend Class SoakAssertionException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class

Friend Class SoakBackendContext
    Implements IVideoBackendContext

    Private ReadOnly _log As EngineLogger

    Public Sub New(log As EngineLogger)
        _log = log
    End Sub

    Public ReadOnly Property Logger As EngineLogger Implements IVideoBackendContext.Logger
        Get
            Return _log
        End Get
    End Property

    Public ReadOnly Property BackendKind As VideoBackendKind Implements IVideoBackendContext.BackendKind
        Get
            Return VideoBackendKind.Ddagrab
        End Get
    End Property
End Class

''' <summary>Real production stack: DdagrabBackend + NvencEncoderBackend,
''' both fully initialized on the NVIDIA adapter.</summary>
Friend Class SoakStack
    Implements IDisposable

    Public ReadOnly Capture As DdagrabBackend
    Public ReadOnly Encoder As NvencEncoderBackend

    Public Sub New()
        Dim log As New EngineLogger("soak", EngineLogger.LogLevel.Warning)
        Capture = New DdagrabBackend(log)
        Capture.Initialize(New SoakBackendContext(log))

        Dim cfg As New EncoderConfig()
        cfg.CodecKey = "NVENC_H264"
        cfg.BitrateBps = 4000000L
        cfg.MinrateBps = 4000000L
        cfg.MaxrateBps = 4000000L
        cfg.BufsizeBps = 8000000L
        cfg.GopSize = 60
        cfg.RateControl = "cbr"
        cfg.Preset = "p4"
        cfg.FrameRateFps = 30
        cfg.ExpectedWidth = Capture.OutputWidth
        cfg.ExpectedHeight = Capture.OutputHeight
        cfg.EncodeWidth = Capture.OutputWidth
        cfg.EncodeHeight = Capture.OutputHeight

        Encoder = New NvencEncoderBackend(log)
        Encoder.Initialize(cfg)
        SoakCounters.NoteEncoderCreated()
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Try
            If Encoder IsNot Nothing Then Encoder.Dispose()
        Catch
        End Try
        Try
            If Capture IsNot Nothing Then Capture.Dispose()
        Catch
        End Try
    End Sub
End Class

Friend Class SoakCounters
    Friend Shared EncoderInstancesCreated As Integer = 0

    Friend Shared Sub NoteEncoderCreated()
        EncoderInstancesCreated += 1
    End Sub
End Class

''' <summary>Sink keeping only the latest frame (older disposed); used to
''' hand real captured frames to the encoder in Scenario C/E.</summary>
Friend Class LatestSink
    Implements IVideoFrameSink

    Private ReadOnly _sync As New Object()
    Private _latest As IVideoFrame

    Public Received As Integer = 0

    Public Function TryPush(result As FrameAcquisitionResult) As PushOutcome Implements IVideoFrameSink.TryPush
        Dim old As IVideoFrame = Nothing
        SyncLock _sync
            old = _latest
            _latest = result.Frame
            Received += 1
        End SyncLock
        If old IsNot Nothing Then
            Try
                old.Dispose()
            Catch
            End Try
        End If
        Return PushOutcome.Pushed
    End Function

    Public Function TakeLatest() As IVideoFrame
        SyncLock _sync
            Dim f As IVideoFrame = _latest
            _latest = Nothing
            Return f
        End SyncLock
    End Function

    Public Sub DisposeLatest()
        Dim f As IVideoFrame = TakeLatest()
        If f IsNot Nothing Then
            Try
                f.Dispose()
            Catch
            End Try
        End If
    End Sub
End Class


''' <summary>Sink that disposes every pushed frame immediately (normal / cancel / crash cycles).</summary>
Friend Class DisposingSink
    Implements IVideoFrameSink

    Public Function TryPush(result As FrameAcquisitionResult) As PushOutcome Implements IVideoFrameSink.TryPush
        If result.Frame IsNot Nothing Then
            Try
                result.Frame.Dispose()
            Catch
            End Try
        End If
        Return PushOutcome.Pushed
    End Function
End Class
''' <summary>Sink that slows every push (slow-stop generator).</summary>
Friend Class SlowSink
    Implements IVideoFrameSink

    Public SlowMs As Integer = 0

    Public Function TryPush(result As FrameAcquisitionResult) As PushOutcome Implements IVideoFrameSink.TryPush
        If SlowMs > 0 Then
            Thread.Sleep(SlowMs)
        End If
        If result.Frame IsNot Nothing Then
            Try
                result.Frame.Dispose()
            Catch
            End Try
        End If
        Return PushOutcome.Pushed
    End Function
End Class

''' <summary>Sink parking the worker until released (timeout generator).</summary>
Friend Class GatedSink
    Implements IVideoFrameSink

    Private ReadOnly _gate As ManualResetEvent
    Public Entered As Boolean = False

    Public Sub New(gate As ManualResetEvent)
        _gate = gate
    End Sub

    Public Function TryPush(result As FrameAcquisitionResult) As PushOutcome Implements IVideoFrameSink.TryPush
        Entered = True
        _gate.WaitOne()
        If result.Frame IsNot Nothing Then
            Try
                result.Frame.Dispose()
            Catch
            End Try
        End If
        Return PushOutcome.Pushed
    End Function
End Class

Friend Module SoakShared
    Friend FfmpegPath As String = ""
    Friend Sandbox As String = ""

    Friend Sub LogLine(message As String)
        SyncLock SoakConsole.SyncRoot
            Console.WriteLine(message)
        End SyncLock
    End Sub

    Friend Sub Assert(condition As Boolean, message As String)
        If Not condition Then
            Throw New SoakAssertionException(message)
        End If
    End Sub

    Friend Function CountFfmpeg() As Integer
        Return Process.GetProcessesByName("ffmpeg").Length
    End Function

    Friend Function WaitState(backend As DdagrabBackend, target As DdagrabBackend.DdagrabBackendState, timeoutMs As Integer) As Boolean
        Dim deadline As Long = DateTime.UtcNow.Ticks + timeoutMs * 10000L
        While DateTime.UtcNow.Ticks < deadline
            If backend.CurrentState = target Then
                Return True
            End If
            Thread.Sleep(25)
        End While
        Return False
    End Function

    Friend Function WaitActivity(backend As DdagrabBackend, sink As LatestSink, timeoutMs As Integer) As Boolean
        Dim deadline As Long = DateTime.UtcNow.Ticks + timeoutMs * 10000L
        While DateTime.UtcNow.Ticks < deadline
            If backend.Diagnostics.NoFrameCount > 0 OrElse
               backend.Diagnostics.EmittedFrames > 0 OrElse
               backend.Diagnostics.ErrorCount > 0 Then
                Return True
            End If
            If sink IsNot Nothing AndAlso sink.Received > 0 Then
                Return True
            End If
            Thread.Sleep(20)
        End While
        Return False
    End Function

    Friend Function MakeConfig(stack As SoakStack, outPath As String) As SessionConfig
        Dim cfg As New SessionConfig()
        cfg.OutputPath = outPath
        cfg.DurationSeconds = 3600
        cfg.TargetFps = 30
        cfg.FFmpegPath = FfmpegPath
        cfg.AudioEnabled = False
        cfg.MicEnabled = False
        cfg.UseNativeResolution = True
        cfg.EncodeWidth = stack.Capture.OutputWidth
        cfg.EncodeHeight = stack.Capture.OutputHeight
        Return cfg
    End Function

    ''' <summary>Run a CaptureSession and Stop() it after stopAfterMs.</summary>
    Friend Function RunStoppedSession(stack As SoakStack, outPath As String, stopAfterMs As Integer, scope As String) As SessionResult
        Dim session As New CaptureSession(stack.Capture, stack.Encoder,
                                          MakeConfig(stack, outPath),
                                          New EngineLogger("soak-session", EngineLogger.LogLevel.Warning))
        Dim runTask As Task(Of SessionResult) = Task.Run(Function() session.Run())
        Thread.Sleep(stopAfterMs)
        session.[Stop]()
        If Not runTask.Wait(45000) Then
            SoakFailures.Record(scope, "session Run did not finish within 45s of Stop")
            Throw New SoakAssertionException("session hang")
        End If
        Return runTask.Result
    End Function

    ''' <summary>Validate produced media: exists, size>0, decodable, video
    ''' stream present, duration>0. Video-only soak — audio absence is NOT a
    ''' failure (per the task protocol).</summary>
    Friend Sub ValidateMedia(path As String, scope As String)
        Assert(File.Exists(path), "media missing: " & path)
        Assert(New FileInfo(path).Length > 0, "media size 0: " & path)

        Dim psi As New ProcessStartInfo()
        psi.FileName = FfmpegPath
        psi.Arguments = "-hide_banner -i """ & path & """"
        psi.UseShellExecute = False
        psi.RedirectStandardError = True
        psi.CreateNoWindow = True
        Using p As Process = Process.Start(psi)
            Dim err As String = p.StandardError.ReadToEnd()
            If Not p.WaitForExit(10000) Then
                Try
                    p.Kill()
                Catch
                End Try
                SoakFailures.Record(scope, "media probe timeout")
                Return
            End If
            Dim dur As Double = 0.0
            Dim m As System.Text.RegularExpressions.Match =
                System.Text.RegularExpressions.Regex.Match(err, "Duration:\s*(\d+):(\d+):(\d+\.?\d*)")
            If m.Success Then
                dur = CInt(m.Groups(1).Value) * 3600 + CInt(m.Groups(2).Value) * 60 + CDbl(m.Groups(3).Value)
            End If
            Assert(dur > 0, "media duration 0 (" & path & ")")
            Assert(err.Contains("Video:"), "media has no video stream (" & path & ")")
        End Using
    End Sub

    ''' <summary>Create a REAL 64x64 BGRA texture on the NVIDIA adapter and
    ''' wrap it — the encoder's dimension-mismatch validation (a real NVENC
    ''' path: dims checked before any NVENC call) faults it deterministically.</summary>
    Friend Function MakeWrongDimsFrame() As IVideoFrame
        Dim factory As IDXGIFactory1 = DXGI.CreateDXGIFactory1(Of IDXGIFactory1)()
        Dim nvidia As IDXGIAdapter1 = Nothing
        Dim idx As Integer = 0
        While True
            Dim adapter As IDXGIAdapter1 = Nothing
            If Not factory.EnumAdapters1(CUInt(idx), adapter).Success Then
                Exit While
            End If
            If adapter.Description1.VendorId = &H10DEUI Then
                nvidia = adapter
                Exit While
            End If
            idx += 1
        End While
        Assert(nvidia IsNot Nothing, "no NVIDIA adapter for wrong-dims frame")

        Dim device As ID3D11Device = Nothing
        Dim context As ID3D11DeviceContext = Nothing
        D3D11.D3D11CreateDevice(nvidia, DriverType.Unknown,
                                DeviceCreationFlags.BgraSupport,
                                New FeatureLevel() {FeatureLevel.Level_11_0},
                                device, context).CheckError()
        Dim desc As New Texture2DDescription()
        desc.Width = 64UI
        desc.Height = 64UI
        desc.MipLevels = 1
        desc.ArraySize = 1
        desc.Format = Format.B8G8R8A8_UNorm
        desc.SampleDescription = New SampleDescription(1, 0)
        desc.Usage = ResourceUsage.Default
        desc.BindFlags = BindFlags.ShaderResource
        Dim tex As ID3D11Texture2D = device.CreateTexture2D(desc)
        Dim frame As New D3D11VideoFrame(tex, 64, 64, 0, 0, 0)
        context.Dispose()
        device.Dispose()
        Return frame
    End Function
End Module
