Option Strict On
Option Explicit On
Option Infer On

' GetEngineModeTests.vb — C/2: engine_mode vocabulary authority contract.
'
' The Overlay writer (AppSettings.NormalizeEngineMode) accepts
'   duluka | ddagrab | native  → "Duluka"
'   ffmpeg | legacy            → "FFmpeg"
' and the project's own regime table (HANDOFF §5) documents
' "duluka/ddagrab/native" as the Duluka engine_mode set. The Engine reader
' (OverlayConfig.GetEngineMode) MUST therefore accept the same vocabulary —
' a config.json written or hand-edited with engine_mode "native" must
' dispatch the Duluka (RecordingEngine) path, not silently fall through to
' the api_capture inference and run the legacy FFmpeg engine while the
' Overlay UI claims Duluka.
'
' Deterministic: writes config.json fixtures into <exeDir>\Config (the
' harness pattern from CT4ConfigTruthTests), no hardware / ffmpeg / UI.

Imports System
Imports System.IO
Imports Engine.ConfigTruth.Tests

Friend Module GetEngineModeTests

    Private Function ModeConfig(engineMode As String, apiCapture As String) As String
        ' Shape mirrors what the Overlay's AppSettings.Save writes: nested
        ' Recording section WITH a current{} block (the TryParseNestedRecording
        ' gate — without it the nested branch is skipped entirely) plus the
        ' flat APICapture field.
        Dim sb As New Text.StringBuilder()
        sb.AppendLine("{")
        sb.AppendLine("  ""Recording"": {")
        If engineMode IsNot Nothing Then
            sb.AppendLine("    ""engine_mode"": """ & engineMode & """,")
        End If
        If apiCapture IsNot Nothing Then
            sb.AppendLine("    ""APICapture"": """ & apiCapture & """,")
        End If
        sb.AppendLine("    ""current"": { ""fps"": 60, ""bitrate"": 17000 }")
        sb.AppendLine("  }")
        sb.AppendLine("}")
        Return sb.ToString()
    End Function

    Private Sub WriteModeConfig(engineMode As String, apiCapture As String)
        Dim cfgDir As String = Path.Combine(AppLayout.Dir, "Config")
        Directory.CreateDirectory(cfgDir)
        File.WriteAllText(Path.Combine(cfgDir, "config.json"), ModeConfig(engineMode, apiCapture))
        ' The unified config dir is cached process-wide — invalidate so
        ' every case re-resolves deterministically.
        OverlayConfig.ResetResolvedPath()
    End Sub

    Public Sub RunAll()
        Console.WriteLine(" ──── GetEngineMode vocabulary (C/2) ────")

        ' THE regression: "native" is an Overlay-accepted Duluka alias.
        TestRunner.RunTest("C2-GEM1 nested engine_mode='native' → ddagrab",
           Sub()
               WriteModeConfig("native", Nothing)
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ddagrab",
                   $"expected ddagrab (Overlay NormalizeEngineMode maps native→Duluka) but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        TestRunner.RunTest("C2-GEM2 flat EngineMode='native' → ddagrab",
           Sub()
               ' Flat shape (legacy AppConfig field), same vocabulary rule.
               Dim json As String = "{""Recording"": { ""EngineMode"": ""native"" }}"
               Dim cfgDir As String = Path.Combine(AppLayout.Dir, "Config")
               Directory.CreateDirectory(cfgDir)
               File.WriteAllText(Path.Combine(cfgDir, "config.json"), json)
               OverlayConfig.ResetResolvedPath()
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ddagrab",
                   $"expected ddagrab for flat EngineMode=native but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        ' Pin the pre-existing accepted vocabulary (no behavior change).
        TestRunner.RunTest("C2-GEM3 nested engine_mode='duluka' → ddagrab",
           Sub()
               WriteModeConfig("duluka", Nothing)
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ddagrab",
                   $"expected ddagrab but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        TestRunner.RunTest("C2-GEM4 nested engine_mode='ddagrab' → ddagrab",
           Sub()
               WriteModeConfig("ddagrab", Nothing)
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ddagrab",
                   $"expected ddagrab but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        TestRunner.RunTest("C2-GEM5 nested engine_mode='legacy' → ffmpeg",
           Sub()
               WriteModeConfig("legacy", Nothing)
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ffmpeg",
                   $"expected ffmpeg but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        TestRunner.RunTest("C2-GEM6 nested engine_mode='ffmpeg' → ffmpeg",
           Sub()
               WriteModeConfig("ffmpeg", Nothing)
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ffmpeg",
                   $"expected ffmpeg but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        ' Fail-closed pins: unknown vocabulary falls back to api_capture
        ' inference — ddagrab only when explicitly requested.
        TestRunner.RunTest("C2-GEM7 unknown engine_mode + APICapture=ddagrab → ddagrab",
           Sub()
               WriteModeConfig("quantum", "ddagrab")
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ddagrab",
                   $"expected ddagrab via api_capture inference but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        TestRunner.RunTest("C2-GEM8 unknown engine_mode, no APICapture → ffmpeg",
           Sub()
               WriteModeConfig("quantum", Nothing)
               TestRunner.Assert(OverlayConfig.GetEngineMode() = "ffmpeg",
                   $"expected ffmpeg (fail-closed) but got '{OverlayConfig.GetEngineMode()}'")
           End Sub)

        ' Cleanup: remove the fixture so later suites resolve their own.
        Try
            File.Delete(Path.Combine(AppLayout.Dir, "Config", "config.json"))
            OverlayConfig.ResetResolvedPath()
        Catch
        End Try
    End Sub

End Module
