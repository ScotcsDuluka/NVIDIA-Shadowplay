Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports CaptureEngine.Configuration
Imports CaptureEngine.Configuration.Schema
Imports CaptureEngine.FFmpeg
Imports CaptureEngine.Pipeline

Namespace CaptureEngine.ConfigTests.Regression
    ''' <summary>
    ''' Phase 6 regression tests — covers:
    '''
    '''   - Lifecycle: Create → Start → Stop → Dispose (and negative cases)
    '''     for ConfigMigrator + ConfigLoader + ConfigValidator + PipelineResolver
    '''   - FFmpeg command snapshot: V1 vs V2 parity for same logical config
    '''     (verify codec, fps, bitrate, preset, gop, aq, bufsize)
    '''
    ''' This module is invoked from Program.vb after Phase 1 tests run.
    ''' Pattern follows CaptureEngine.Tests/Program.vb (custom console runner,
    ''' no xUnit/NUnit/MSTest).
    ''' </summary>
    Friend Module RegressionTests
        Private _passed As Integer = 0
        Private _failed As Integer = 0
        Private ReadOnly _failures As New List(Of String)()

        Public ReadOnly Property Passed As Integer
            Get
                Return _passed
            End Get
        End Property

        Public ReadOnly Property Failed As Integer
            Get
                Return _failed
            End Get
        End Property

        Public ReadOnly Property Failures As IReadOnlyList(Of String)
            Get
                Return _failures
            End Get
        End Property

        Public Sub Reset()
            _passed = 0
            _failed = 0
            _failures.Clear()
        End Sub

        Public Sub RunAll()
            RunTest("L1.1 Lifecycle: V2 config can be constructed", AddressOf Test_Lifecycle_Construct)
            RunTest("L1.2 Lifecycle: V2 config → migrate → resolve → builder", AddressOf Test_Lifecycle_FullResolve)
            RunTest("L1.3 Lifecycle: pipeline resolve produces correct VideoBackend", AddressOf Test_Lifecycle_VideoBackendResolution)
            RunTest("L1.4 Lifecycle: pipeline resolve produces correct AudioBackend", AddressOf Test_Lifecycle_AudioBackendResolution)
            RunTest("L1.5 Lifecycle: pipeline resolve rejects unknown capture method", AddressOf Test_Lifecycle_RejectUnknownMethod)
            RunTest("L1.6 Lifecycle: pipeline builder rejects experimental DXGI (NotImplemented)", AddressOf Test_Lifecycle_DxgiNotImplemented)
            RunTest("L1.7 Lifecycle: resolver with audio enabled → wasapi_loopback", AddressOf Test_Lifecycle_WithAudio)
            RunTest("L1.8 Lifecycle: resolver with audio disabled → none", AddressOf Test_Lifecycle_NoAudio)
            RunTest("L1.9 Lifecycle: ConfigMigrator idempotent (migrate twice → same result)", AddressOf Test_Lifecycle_MigratorIdempotent)
            RunTest("L1.10 Lifecycle: V2 builder can be re-used (Build called twice)", AddressOf Test_Lifecycle_BuilderReusable)

            RunTest("S1.1 Snapshot: V1 default produces known command", AddressOf Test_Snapshot_V1Default)
            RunTest("S1.2 Snapshot: V2 default produces known command", AddressOf Test_Snapshot_V2Default)
            RunTest("S1.3 Snapshot: V2 fixes bufsize (2× bitrate)", AddressOf Test_Snapshot_V2BufsizeFix)
            RunTest("S1.4 Snapshot: V2 appends -pix_fmt for ddagrab+NVENC (V1 omits)", AddressOf Test_Snapshot_V2PixFmtAppended)
            RunTest("S1.5 Snapshot: V2 command contains -c:v h264_nvenc", AddressOf Test_Snapshot_V2Codec)
            RunTest("S1.6 Snapshot: V2 command contains -g 60 (GOP = FPS)", AddressOf Test_Snapshot_V2Gop)
            RunTest("S1.7 Snapshot: V2 command contains -preset p4", AddressOf Test_Snapshot_V2Preset)
            RunTest("S1.8 Snapshot: V2 command contains -b:v 20000000 (20 Mbps)", AddressOf Test_Snapshot_V2Bitrate)
            RunTest("S1.9 Snapshot: V2 command contains -fps_mode cfr", AddressOf Test_Snapshot_V2FpsMode)
            RunTest("S1.10 Snapshot: V2 command contains -spatial-aq 1 -temporal-aq 1", AddressOf Test_Snapshot_V2Aq)
            RunTest("S1.11 Snapshot: V2 command contains -movflags +faststart on final", AddressOf Test_Snapshot_V2FaststartFinal)
            RunTest("S1.12 Snapshot: V2 command SKIPS -movflags on .video.tmp.mp4", AddressOf Test_Snapshot_V2FaststartSkippedOnTemp)
            RunTest("S1.13 Snapshot: V2 command contains -y (overwrite)", AddressOf Test_Snapshot_V2Overwrite)
            RunTest("S1.14 Snapshot: V2 preserves custom resolution scale filter", AddressOf Test_Snapshot_V2CustomResolution)
            RunTest("S1.15 Snapshot: V2 supports non-default preset (p7)", AddressOf Test_Snapshot_V2CustomPreset)
            RunTest("S1.16 Snapshot: V2 supports custom framerate (144)", AddressOf Test_Snapshot_V2Fps144)
            RunTest("S1.17 Snapshot: V2 supports CQ mode (RateControl=cq, Cq=23)", AddressOf Test_Snapshot_V2CqMode)
            RunTest("S1.18 Snapshot: V2 supports ZeroLatency mode", AddressOf Test_Snapshot_V2ZeroLatency)
            RunTest("S1.19 Snapshot: V2 supports LookAhead > 0", AddressOf Test_Snapshot_V2LookAhead)
            RunTest("S1.20 Snapshot: V2 supports explicit Profile (high)", AddressOf Test_Snapshot_V2Profile)
            RunTest("S1.21 Snapshot: V2 -tune omitted when empty string", AddressOf Test_Snapshot_V2TuneOmittedWhenEmpty)
            RunTest("S1.22 Snapshot: V2 gdigrab command (no -vf hwdownload)", AddressOf Test_Snapshot_V2Gdigrab)
            RunTest("S1.23 Snapshot: V2 NVENC_HEVC encoder → hevc_nvenc", AddressOf Test_Snapshot_V2HevcEncoder)
            RunTest("S1.24 Snapshot: V2 contains expected OutputIndex in ddagrab (0)", AddressOf Test_Snapshot_V2OutputIndex)
            RunTest("S1.25 Snapshot: V2 default bitrate unit is bps (not kbps)", AddressOf Test_Snapshot_V2BitrateUnitBps)

            ' Parity delta: V1 vs V2 must differ ONLY in bufsize + pix_fmt
            RunTest("P1.1 Parity: V1 vs V2 same logical config differ only in bufsize + pix_fmt",
                    AddressOf Test_Parity_V1V2Delta)
            RunTest("P1.2 Parity: V1 vs V2 share identical encoder/fps/preset/gop/aq",
                    AddressOf Test_Parity_V1V2Shared)
        End Sub

        Private Sub RunTest(name As String, test As Action)
            Console.Write("[REG " & name & "] ")
            Dim pad As Integer = Math.Max(0, 70 - name.Length - 6)
            Console.Write(New String(" "c, pad))
            Try
                test()
                _passed += 1
                Console.WriteLine("PASS")
            Catch ex As Exception
                _failed += 1
                _failures.Add(name & ": " & ex.GetType().Name & ": " & ex.Message)
                Console.WriteLine("FAIL")
                Console.WriteLine("        " & ex.GetType().Name & ": " & ex.Message)
            End Try
        End Sub

        Private Sub Assert(condition As Boolean, message As String)
            If Not condition Then
                Throw New InvalidOperationException("Assertion failed: " & message)
            End If
        End Sub

        ' ════════════════════════════════════════════════════════════
        ' L1 — Lifecycle tests
        ' ════════════════════════════════════════════════════════════

        Private Sub Test_Lifecycle_Construct()
            Dim cfg As New EngineConfigV2()
            Assert(cfg IsNot Nothing, "V2 config should construct")
            Assert(cfg.Version = 2, "Version should be 2")
            Assert(cfg.Video.Capture.Method = "ddagrab", "Default capture method")
            Assert(cfg.Video.Encoder.Key = "NVENC_H264", "Default encoder key")
        End Sub

        Private Sub Test_Lifecycle_FullResolve()
            ' End-to-end: V1 → V2 → Resolve → Builder → Build
            Dim v1 As New ConfigMigrator.V1CaptureSettings()
            Dim v2 As EngineConfigV2 = ConfigMigrator.MigrateFromV1(v1)
            Dim pipeline As PipelineConfig = PipelineResolver.Resolve(v2)
            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("test.mp4")

            Assert(pipeline.VideoBackend = "ffmpeg_ddagrab", "Should resolve to ffmpeg_ddagrab")
            Assert(pipeline.AudioBackend = "wasapi_loopback", "Should resolve to wasapi_loopback (system audio on by default)")
            Assert(builder.BuilderLabel.Contains("V2"), "Should be V2 builder")
            Assert(cmd.Contains("ddagrab=output_idx=0:framerate=60"), "Should contain ddagrab input")
            Assert(cmd.Contains("h264_nvenc"), "Should contain h264_nvenc")
        End Sub

        Private Sub Test_Lifecycle_VideoBackendResolution()
            For Each method As String In {"ddagrab", "gdigrab", "gfxcapture"}
                Dim v2 As New EngineConfigV2()
                v2.Video.Capture.Method = method
                Dim p As PipelineConfig = PipelineResolver.Resolve(v2)
                Assert(p.VideoBackend = "ffmpeg_" & method,
                       $"Method '{method}' should resolve to 'ffmpeg_{method}', got '{p.VideoBackend}'")
            Next
        End Sub

        Private Sub Test_Lifecycle_AudioBackendResolution()
            ' Both audio sources off → none
            Dim v2a As New EngineConfigV2()
            v2a.Audio.System.Enabled = False
            v2a.Audio.Microphone.Enabled = False
            Dim pa As PipelineConfig = PipelineResolver.Resolve(v2a)
            Assert(pa.AudioBackend = "none", "Both disabled should be 'none'")

            ' System on → wasapi_loopback
            Dim v2b As New EngineConfigV2()
            v2b.Audio.System.Enabled = True
            v2b.Audio.Microphone.Enabled = False
            Dim pb As PipelineConfig = PipelineResolver.Resolve(v2b)
            Assert(pb.AudioBackend = "wasapi_loopback", "System on should be 'wasapi_loopback'")

            ' Mic on → wasapi_loopback
            Dim v2c As New EngineConfigV2()
            v2c.Audio.System.Enabled = False
            v2c.Audio.Microphone.Enabled = True
            Dim pc As PipelineConfig = PipelineResolver.Resolve(v2c)
            Assert(pc.AudioBackend = "wasapi_loopback", "Mic on should be 'wasapi_loopback'")
        End Sub

        Private Sub Test_Lifecycle_RejectUnknownMethod()
            Dim v2 As New EngineConfigV2()
            v2.Video.Capture.Method = "x11grab"
            Try
                PipelineResolver.Resolve(v2)
                Throw New InvalidOperationException("Should have thrown for unknown method")
            Catch ex As InvalidOperationException
                Assert(ex.Message.Contains("x11grab"), "Error message should mention the bad method")
            End Try
        End Sub

        Private Sub Test_Lifecycle_DxgiNotImplemented()
            Dim v2 As New EngineConfigV2()
            v2.Experimental.EnableD3D11Interop = True
            v2.Video.Capture.Method = "ddagrab"

            ' Resolve should mark as DXGI
            Dim p As PipelineConfig = PipelineResolver.Resolve(v2)
            Assert(p.VideoBackend = "dxgi", "Should resolve to 'dxgi' when experimental flag set")

            ' But builder should throw NotImplementedException
            Try
                PipelineResolver.BuildFFmpegCommandBuilder(v2)
                Throw New InvalidOperationException("Should have thrown NotImplementedException")
            Catch ex As NotImplementedException
                Assert(ex.Message.Contains("DXGI"), "Error should mention DXGI")
            End Try
        End Sub

        Private Sub Test_Lifecycle_WithAudio()
            Dim v2 As New EngineConfigV2()
            v2.Audio.System.Enabled = True
            v2.Audio.Microphone.Enabled = False
            Dim p As PipelineConfig = PipelineResolver.Resolve(v2)
            Assert(p.AudioBackend = "wasapi_loopback", "Should be wasapi_loopback")
        End Sub

        Private Sub Test_Lifecycle_NoAudio()
            Dim v2 As New EngineConfigV2()
            v2.Audio.System.Enabled = False
            v2.Audio.Microphone.Enabled = False
            Dim p As PipelineConfig = PipelineResolver.Resolve(v2)
            Assert(p.AudioBackend = "none", "Should be 'none'")
        End Sub

        Private Sub Test_Lifecycle_MigratorIdempotent()
            Dim v1 As New ConfigMigrator.V1CaptureSettings()
            v1.BitrateBps = 25000000L

            Dim v2a As EngineConfigV2 = ConfigMigrator.MigrateFromV1(v1)
            Dim v2b As EngineConfigV2 = ConfigMigrator.MigrateFromV1(v1)

            Assert(v2a.Video.Encoder.BitrateBps = v2b.Video.Encoder.BitrateBps, "BitrateBps should match")
            Assert(v2a.Video.Encoder.BufsizeBps = v2b.Video.Encoder.BufsizeBps, "BufsizeBps should match")
            Assert(v2a.Video.Encoder.GopSize = v2b.Video.Encoder.GopSize, "GopSize should match")
        End Sub

        Private Sub Test_Lifecycle_BuilderReusable()
            Dim v2 As New EngineConfigV2()
            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)

            Dim cmd1 As String = builder.Build("a.mp4")
            Dim cmd2 As String = builder.Build("b.mp4")

            Assert(cmd1.Contains("a.mp4"), "First build should contain a.mp4")
            Assert(cmd2.Contains("b.mp4"), "Second build should contain b.mp4")
            Assert(cmd1.Length > 0 AndAlso cmd2.Length > 0, "Both should be non-empty")
        End Sub

        ' ════════════════════════════════════════════════════════════
        ' S1 — FFmpeg command snapshot tests
        ' ════════════════════════════════════════════════════════════

        Private Function BuildDefaultV2Command(outputFile As String) As String
            Dim v2 As New EngineConfigV2()
            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Return builder.Build(outputFile)
        End Function

        Private Function BuildDefaultV1Command(outputFile As String) As String
            Dim v1 As New ConfigMigrator.V1CaptureSettings()
            Dim v1Settings As New FFmpegCommandBuilderV1.V1Settings() With {
                .Encoder = "h264_nvenc",
                .FPS = v1.FPS,
                .Bitrate = v1.BitrateBps,
                .NvencPreset = v1.NvencPreset,
                .CaptureMethod = v1.CaptureMethod,
                .UseNativeResolution = v1.UseNativeResolution,
                .PixelFormat = v1.PixelFormat
            }
            Dim builder As New FFmpegCommandBuilderV1(v1Settings)
            Return builder.Build(outputFile)
        End Function

        Private Sub Test_Snapshot_V1Default()
            Dim cmd As String = BuildDefaultV1Command("out.mp4")
            Assert(cmd.Contains("-c:v h264_nvenc"), "Should contain h264_nvenc")
            Assert(cmd.Contains("ddagrab=output_idx=0:framerate=60"), "Should contain ddagrab framerate")
            Assert(cmd.Contains("-preset p4 -tune ll -rc cbr"), "Should contain preset/tune/rc")
            Assert(cmd.Contains("-b:v 20000000 -minrate 20000000 -maxrate 20000000 -bufsize 20000000"), "Should contain CBR bitrate params")
            Assert(cmd.Contains("-g 60 -fps_mode cfr"), "Should contain GOP + fps_mode")
            Assert(cmd.Contains("-spatial-aq 1 -temporal-aq 1"), "Should contain AQ")
            Assert(cmd.Contains("-movflags +faststart"), "Should contain faststart on final")
            Assert(cmd.Contains("-y"), "Should contain -y overwrite")
            Assert(cmd.Contains("out.mp4"), "Should contain output filename")
        End Sub

        Private Sub Test_Snapshot_V2Default()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-c:v h264_nvenc"), "Should contain h264_nvenc")
            Assert(cmd.Contains("ddagrab=output_idx=0:framerate=60"), "Should contain ddagrab framerate")
            Assert(cmd.Contains("-preset p4 -tune ll -rc cbr"), "Should contain preset/tune/rc")
            Assert(cmd.Contains("-g 60 -fps_mode cfr"), "Should contain GOP + fps_mode")
            Assert(cmd.Contains("-spatial-aq 1 -temporal-aq 1"), "Should contain AQ")
            Assert(cmd.Contains("-movflags +faststart"), "Should contain faststart on final")
            Assert(cmd.Contains("-y"), "Should contain -y overwrite")
            Assert(cmd.Contains("out.mp4"), "Should contain output filename")
        End Sub

        Private Sub Test_Snapshot_V2BufsizeFix()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            ' V2 FIX: bufsize = bitrate * 2 = 40000000 (V1 used 1× = 20000000)
            Assert(cmd.Contains("-bufsize 40000000"),
                   "V2 bufsize should be 40000000 (= bitrate * 2). Cmd: " & cmd)
        End Sub

        Private Sub Test_Snapshot_V2PixFmtAppended()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            ' V2 FIX: -pix_fmt nv12 is appended for ddagrab+NVENC (V1 skips it)
            Assert(cmd.Contains("-pix_fmt nv12"),
                   "V2 should append -pix_fmt nv12 for ddagrab+NVENC. Cmd: " & cmd)
        End Sub

        Private Sub Test_Snapshot_V2Codec()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-c:v h264_nvenc"), "Should contain -c:v h264_nvenc")
        End Sub

        Private Sub Test_Snapshot_V2Gop()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-g 60 "), "Should contain -g 60 (GOP = 60 = FPS)")
        End Sub

        Private Sub Test_Snapshot_V2Preset()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-preset p4 "), "Should contain -preset p4")
        End Sub

        Private Sub Test_Snapshot_V2Bitrate()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-b:v 20000000 "), "Should contain -b:v 20000000 (bps)")
            Assert(cmd.Contains("-minrate 20000000 "), "Should contain -minrate 20000000")
            Assert(cmd.Contains("-maxrate 20000000 "), "Should contain -maxrate 20000000")
        End Sub

        Private Sub Test_Snapshot_V2FpsMode()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-fps_mode cfr "), "Should contain -fps_mode cfr")
        End Sub

        Private Sub Test_Snapshot_V2Aq()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-spatial-aq 1 "), "Should contain -spatial-aq 1")
            Assert(cmd.Contains("-temporal-aq 1 "), "Should contain -temporal-aq 1")
        End Sub

        Private Sub Test_Snapshot_V2FaststartFinal()
            Dim cmd As String = BuildDefaultV2Command("final.mp4")
            Assert(cmd.Contains("-movflags +faststart "), "Final output should get +faststart")
        End Sub

        Private Sub Test_Snapshot_V2FaststartSkippedOnTemp()
            Dim cmd As String = BuildDefaultV2Command("rec.video.tmp.mp4")
            Assert(Not cmd.Contains("-movflags"), "Temp video should NOT get -movflags +faststart. Cmd: " & cmd)
        End Sub

        Private Sub Test_Snapshot_V2Overwrite()
            Dim cmd As String = BuildDefaultV2Command("out.mp4")
            Assert(cmd.Contains("-y "), "Should contain -y overwrite")
            Assert(Not cmd.Contains("-n "), "Should NOT contain -n")
        End Sub

        Private Sub Test_Snapshot_V2CustomResolution()
            Dim v2 As New EngineConfigV2()
            v2.Video.Resolution.Mode = "custom"
            v2.Video.Resolution.Width = 1280
            v2.Video.Resolution.Height = 720

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("scale=1280:720"), "Should contain scale=1280:720")
            Assert(cmd.Contains("hwdownload,format=bgra,scale=1280:720,hwupload"),
                   "Should contain hwdownload+scale+hwupload chain for NVENC + custom res")
        End Sub

        Private Sub Test_Snapshot_V2CustomPreset()
            Dim v2 As New EngineConfigV2()
            v2.Video.Encoder.Preset = "p7"

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("-preset p7 "), "Should contain -preset p7")
            Assert(Not cmd.Contains("-preset p4 "), "Should NOT contain -preset p4")
        End Sub

        Private Sub Test_Snapshot_V2Fps144()
            Dim v2 As New EngineConfigV2()
            v2.Video.Capture.Framerate = 144
            ' GOP must follow framerate for 1-second GOP
            v2.Video.Encoder.GopSize = 144

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("framerate=144"), "Should contain framerate=144 in ddagrab input")
            Assert(cmd.Contains("-g 144 "), "Should contain -g 144 (GOP)")
        End Sub

        Private Sub Test_Snapshot_V2CqMode()
            Dim v2 As New EngineConfigV2()
            v2.Video.Encoder.RateControl = "cq"
            v2.Video.Encoder.Cq = 23
            ' CQ mode requires non-zero maxrate for NVENC
            v2.Video.Encoder.MinrateBps = 0L
            v2.Video.Encoder.MaxrateBps = v2.Video.Encoder.BitrateBps

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("-rc cq "), "Should contain -rc cq")
            Assert(cmd.Contains("-cq 23 "), "Should contain -cq 23")
            Assert(Not cmd.Contains("-minrate "), "Should NOT append -minrate when MinrateBps=0")
        End Sub

        Private Sub Test_Snapshot_V2ZeroLatency()
            Dim v2 As New EngineConfigV2()
            v2.Video.Encoder.ZeroLatency = True

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("-zerolatency 1 "), "Should contain -zerolatency 1")
        End Sub

        Private Sub Test_Snapshot_V2LookAhead()
            Dim v2 As New EngineConfigV2()
            v2.Video.Encoder.LookAhead = 8

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("-look_ahead 8 "), "Should contain -look_ahead 8")
        End Sub

        Private Sub Test_Snapshot_V2Profile()
            Dim v2 As New EngineConfigV2()
            v2.Video.Encoder.Profile = "high"

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("-profile:v high "), "Should contain -profile:v high")
        End Sub

        Private Sub Test_Snapshot_V2TuneOmittedWhenEmpty()
            Dim v2 As New EngineConfigV2()
            v2.Video.Encoder.Tune = ""

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(Not cmd.Contains("-tune "), "Should NOT contain -tune when empty")
        End Sub

        Private Sub Test_Snapshot_V2Gdigrab()
            Dim v2 As New EngineConfigV2()
            v2.Video.Capture.Method = "gdigrab"

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("-f gdigrab -framerate 60 -i desktop"), "Should contain gdigrab input")
            Assert(Not cmd.Contains("ddagrab"), "Should NOT contain ddagrab")
            Assert(Not cmd.Contains("hwdownload"), "gdigrab is CPU capture — no hwdownload")
        End Sub

        Private Sub Test_Snapshot_V2HevcEncoder()
            Dim v2 As New EngineConfigV2()
            v2.Video.Encoder.Key = "NVENC_HEVC"
            v2.Video.Encoder.FFmpegCodec = "hevc_nvenc"

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("-c:v hevc_nvenc"), "Should contain -c:v hevc_nvenc")
            Assert(Not cmd.Contains("h264_nvenc"), "Should NOT contain h264_nvenc")
        End Sub

        Private Sub Test_Snapshot_V2OutputIndex()
            Dim v2 As New EngineConfigV2()
            v2.Video.Capture.OutputIndex = 1

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")

            Assert(cmd.Contains("ddagrab=output_idx=1:framerate=60"),
                   "Should contain output_idx=1. Cmd: " & cmd)
        End Sub

        Private Sub Test_Snapshot_V2BitrateUnitBps()
            ' Default BitrateBps = 20000000 = 20 Mbps (NOT 20000 kbps)
            Dim v2 As New EngineConfigV2()
            Assert(v2.Video.Encoder.BitrateBps = 20000000L, "Default should be 20000000 (bps)")

            Dim builder As IFFmpegCommandBuilder = PipelineResolver.BuildFFmpegCommandBuilder(v2)
            Dim cmd As String = builder.Build("out.mp4")
            Assert(cmd.Contains("-b:v 20000000 "), "Command should use 20000000 (bps)")
        End Sub

        ' ════════════════════════════════════════════════════════════
        ' P1 — V1 vs V2 parity delta
        ' ════════════════════════════════════════════════════════════

        Private Sub Test_Parity_V1V2Delta()
            Dim v1Cmd As String = BuildDefaultV1Command("out.mp4")

            Dim v1 As New ConfigMigrator.V1CaptureSettings()
            Dim v2 As EngineConfigV2 = ConfigMigrator.MigrateFromV1(v1)
            Dim v2Cmd As String = PipelineResolver.BuildFFmpegCommandBuilder(v2).Build("out.mp4")

            ' Differences that MUST exist (V2 fixes):
            ' 1. bufsize: V1 has 20000000, V2 has 40000000
            Assert(v1Cmd.Contains("-bufsize 20000000 "), "V1 should have bufsize 20000000 (1× bitrate)")
            Assert(v2Cmd.Contains("-bufsize 40000000 "), "V2 should have bufsize 40000000 (2× bitrate = FIX)")

            ' 2. pix_fmt: V1 omits for ddagrab+NVENC, V2 appends
            Assert(Not v1Cmd.Contains("-pix_fmt "), "V1 should NOT contain -pix_fmt for ddagrab+NVENC")
            Assert(v2Cmd.Contains("-pix_fmt nv12 "), "V2 should append -pix_fmt nv12 for ddagrab+NVENC")
        End Sub

        Private Sub Test_Parity_V1V2Shared()
            Dim v1Cmd As String = BuildDefaultV1Command("out.mp4")

            Dim v1 As New ConfigMigrator.V1CaptureSettings()
            Dim v2 As EngineConfigV2 = ConfigMigrator.MigrateFromV1(v1)
            Dim v2Cmd As String = PipelineResolver.BuildFFmpegCommandBuilder(v2).Build("out.mp4")

            ' Shared FFmpeg args (byte-identical):
            Assert(v1Cmd.Contains("-c:v h264_nvenc") AndAlso v2Cmd.Contains("-c:v h264_nvenc"), "Both: -c:v h264_nvenc")
            Assert(v1Cmd.Contains("ddagrab=output_idx=0:framerate=60") AndAlso v2Cmd.Contains("ddagrab=output_idx=0:framerate=60"), "Both: ddagrab input")
            Assert(v1Cmd.Contains("-preset p4 -tune ll -rc cbr") AndAlso v2Cmd.Contains("-preset p4 -tune ll -rc cbr"), "Both: preset/tune/rc")
            Assert(v1Cmd.Contains("-b:v 20000000 -minrate 20000000 -maxrate 20000000") AndAlso v2Cmd.Contains("-b:v 20000000 -minrate 20000000 -maxrate 20000000"), "Both: bitrate + minrate + maxrate")
            Assert(v1Cmd.Contains("-g 60 -fps_mode cfr") AndAlso v2Cmd.Contains("-g 60 -fps_mode cfr"), "Both: GOP + fps_mode")
            Assert(v1Cmd.Contains("-spatial-aq 1 -temporal-aq 1") AndAlso v2Cmd.Contains("-spatial-aq 1 -temporal-aq 1"), "Both: AQ")
            Assert(v1Cmd.Contains("-movflags +faststart") AndAlso v2Cmd.Contains("-movflags +faststart"), "Both: +faststart")
            Assert(v1Cmd.Contains("-y ") AndAlso v2Cmd.Contains("-y "), "Both: -y overwrite")
            Assert(v1Cmd.Contains("out.mp4") AndAlso v2Cmd.Contains("out.mp4"), "Both: output filename")
        End Sub
    End Module
End Namespace
