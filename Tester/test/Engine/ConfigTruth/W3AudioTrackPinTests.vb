Option Strict On
Option Explicit On
Option Infer On

' W3AudioTrackPinTests.vb — W3 reconciliation pin for the SeparateTrack
' plumbing: config → CaptureSettings → MapSessionConfig → LiveMux args.
'
' Background (W3 audio forensics): a videocheck run with audio.json
' {SystemEnabled:true, MicEnabled:true, AudioTrackMode:1} produced an
' amix MP4 (one 2ch audio stream) instead of separate tracks. Static
' reading of every hop says "correct" — this module measures the REAL
' runtime values at each hop and reports which layer lies.
'
' Modes (env W3_PIN_DIR = config dir to load; default = self-written temp):
'   HOP  — write the exact forensics config set, load + map, dump values.
'   ARGS — start a real LiveMuxSession against a stub ffmpeg.exe that
'          records its command line, feed both pipes, stop, dump the args
'          (proves which BuildArgs branch executed and what the mux saw).
'
' Run: dotnet <dll> -- w3pin        (from the ConfigTruth runner args gate)

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Namespace Engine.ConfigTruth

    Friend Module W3AudioTrackPinTests

        Private ReadOnly _pass As New List(Of String)()
        Private ReadOnly _fail As New List(Of String)()

        Private Sub Check(cond As Boolean, what As String, detail As String)
            Dim line = $"{If(cond, "OK  ", "LIE ")} {what} :: {detail}"
            Console.WriteLine("  " & line)
            If cond Then _pass.Add(line) Else _fail.Add(line)
        End Sub

        ''' <summary>Write the exact forensics config set (audio.json says
        ''' SeparateTrack) into an isolated dir.</summary>
        Private Function WriteForensicsDir() As String
            Dim dir = Path.Combine(Path.GetTempPath(), "w3-pin-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(dir)
            File.WriteAllText(Path.Combine(dir, "video.json"),
                "{""Encoder"":""h264_nvenc"",""ActivePreset"":""CUSTOM"",""Current"":{""fps"":60,""bitrate"":20000,""encoder_preset"":4,""use_native_resolution"":true,""width"":0,""height"":0}}")
            File.WriteAllText(Path.Combine(dir, "audio.json"),
                "{""SystemEnabled"":true,""MicEnabled"":true,""SystemVolume"":1.0,""MicVolume"":1.0,""AudioTrackMode"":1}")
            File.WriteAllText(Path.Combine(dir, "engine.json"),
                "{""ConfigVersion"":3,""CaptureMethod"":""ddagrab"",""PixelFormat"":""nv12"",""Preset"":"""",""RateControl"":""cbr"",""UseNativeResolution"":true,""CustomWidth"":0,""CustomHeight"":0}")
            Return dir
        End Function

        Public Sub RunPin()
            Console.WriteLine(" ══ W3 SeparateTrack pin ══")
            Dim dir = Environment.GetEnvironmentVariable("W3_PIN_DIR")
            Dim isolated As Boolean = String.IsNullOrWhiteSpace(dir)
            Dim owned As String = Nothing
            If isolated Then
                dir = WriteForensicsDir()
                owned = dir
            End If
            Try
                Console.WriteLine($"  OverlayConfig.IsAvailable = {OverlayConfig.IsAvailable}")
                Console.WriteLine($"  OverlayConfig.ConfigPath  = {OverlayConfig.ConfigPath}")

                Dim settings = CaptureSettings.Load(Path.Combine(dir, "engine.json"))
                Console.WriteLine($"  [hop 1] CaptureSettings: SystemAudioCapture={settings.SystemAudioCapture} " &
                                  $"MicCapture={settings.MicCapture} MicDeviceId=""{settings.MicDeviceId}"" " &
                                  $"AudioTrackMode={settings.AudioTrackMode}")
                Check(settings.SystemAudioCapture, "hop1 SystemAudioCapture", "audio.json SystemEnabled:true")
                Check(settings.MicCapture, "hop1 MicCapture", "audio.json MicEnabled:true")
                Check(settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack,
                      "hop1 AudioTrackMode=SeparateTrack", $"got {settings.AudioTrackMode}")

                Dim cfg = NextRecordingConfig.MapSessionConfig(settings, "w3-pin-out.mp4", "ffmpeg", Nothing)
                Console.WriteLine($"  [hop 2] SessionConfig:   MicEnabled={cfg.MicEnabled} " &
                                  $"MicSeparateTracks={cfg.MicSeparateTracks}")
                Check(cfg.MicEnabled, "hop2 MicEnabled", "must follow MicCapture")
                Check(cfg.MicSeparateTracks, "hop2 MicSeparateTracks", "must follow AudioTrackMode=SeparateTrack")
            Finally
                If owned IsNot Nothing Then
                    Try : Directory.Delete(owned, True) : Catch : End Try
                End If
            End Try
        End Sub

        ''' <summary>Prints the summary and returns the lie count (process
        ''' exit-code contract for the w3pin gate).</summary>
        Public Function ReportSummary() As Integer
            Console.WriteLine($"  pin summary: pass={_pass.Count} lie={_fail.Count}")
            For Each f In _fail
                Console.WriteLine("    !! " & f)
            Next
            Return _fail.Count
        End Function

    End Module

End Namespace
