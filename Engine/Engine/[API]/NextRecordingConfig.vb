Option Strict On
Option Explicit On
Option Infer On

' NextRecordingConfig.vb
'
' PHASE 0 — CONFIG TRUTH (audit task 23 → fix wave task 24).
'
' Single seam for "what the NEXT recording will use". The SessionConfig
' mapping used to live inline in HandleRecordingStart
' (RecordingEngineHost.vb:214-228 pre-fix) where it read UI_Engine._settings
' — a CaptureSettings loaded ONCE at form init (UI_Engine.vb:616, called
' from :85). Result (audit verdict): a setting changed after process start
' never reached the new-engine path — "UI setting = persisted config =
' effective runtime config" did not hold.
'
' FIX-1 wires HandleRecordingStart through THIS class:
'   LoadEffectiveSettings() = fresh reload from disk (the same fresh-reload
'   the legacy path has always done — HandleEngineRecordStart,
'   UI_Engine.vb:369-371). Inside CaptureSettings.Load the unified Overlay
'   config.json WINS (CaptureSettings.vb:101-116), so the reloaded object
'   IS the effective config.
'
' MapSessionConfig is a VERBATIM move of the pre-fix inline mapping — the
' fix changes WHERE the settings come from, NOT the mapping semantics
' (field-for-field identical, including the Nothing-settings fallbacks).
'
' Compiled by BOTH the Engine project (Windows, root namespace
' NVIDIA_Capture) and Engine.ConfigTruth.Tests (Linux, linked source) —
' all cross-type references are therefore unqualified/global, and the file
' must stay free of WinForms dependencies.

Imports System.Diagnostics
Imports CaptureEngine.Recording

Public NotInheritable Class NextRecordingConfig

    ''' <summary>
    ''' Engine's config path anchor — the SAME expression UI_Engine uses at
    ''' form load (UI_Engine.vb:76: _configPath = AppLayout.P("Config",
    ''' "engine.json")). CaptureSettings.Load derives the legacy-tier base
    ''' dir from it; the unified Overlay config.json is resolved
    ''' independently by OverlayConfig.ConfigDir.
    ''' </summary>
    Public Shared Function EngineConfigPath() As String
        Return AppLayout.P("Config", "engine.json")
    End Function

    ''' <summary>
    ''' FIX-1: fresh reload of the persisted config. Unified Overlay
    ''' config.json wins inside CaptureSettings.Load (engine.json → unified
    ''' WINS + early return → legacy video/audio.json tier only when no
    ''' config.json exists anywhere). Returns a NEW CaptureSettings every
    ''' call — callers must not cache it past one recording start.
    ''' </summary>
    Public Shared Function LoadEffectiveSettings(Optional configPath As String = Nothing) As CaptureSettings
        Return CaptureSettings.Load(If(String.IsNullOrEmpty(configPath), EngineConfigPath(), configPath))
    End Function

    ''' <summary>
    ''' The SessionConfig the NEXT recording runs with: effective settings
    ''' (fresh reload) mapped verbatim. This is the CT-4 contract entry —
    ''' Engine.ConfigTruth.Tests executes exactly this composition.
    ''' </summary>
    Public Shared Function BuildSessionConfig(outputPath As String,
                                              ffmpegPath As String,
                                              onProcessStarted As Action(Of Process),
                                              Optional configPath As String = Nothing) As SessionConfig
        Return MapSessionConfig(LoadEffectiveSettings(configPath), outputPath, ffmpegPath, onProcessStarted)
    End Function

    ''' <summary>
    ''' VERBATIM move of the SessionConfig mapping that was inline in
    ''' HandleRecordingStart (RecordingEngineHost.vb:214-228 pre-fix).
    ''' DurationSeconds=3600 ("no fixed duration — stop via command") and
    ''' every Nothing-settings fallback are preserved exactly; only the
    ''' host hook became a parameter so the mapping is testable without a
    ''' WinForms form.
    ''' </summary>
    Public Shared Function MapSessionConfig(settings As CaptureSettings,
                                            outputPath As String,
                                            ffmpegPath As String,
                                            onProcessStarted As Action(Of Process)) As SessionConfig
        Return New SessionConfig() With {
            .OutputPath = outputPath,
            .DurationSeconds = 3600,          ' no fixed duration — stop via command
            .FFmpegPath = ffmpegPath,
            .AudioEnabled = (settings Is Nothing) OrElse settings.SystemAudioCapture,
            .SystemVolume = If(settings IsNot Nothing, settings.SystemAudioVolume, 1.0F),
            .MicEnabled = (settings IsNot Nothing) AndAlso settings.MicCapture,
            .MicVolume = If(settings IsNot Nothing, settings.MicVolume, 1.0F),
            .MicDeviceId = If(settings IsNot Nothing, settings.MicDeviceId, ""),
            .MicDeviceName = If(settings IsNot Nothing, settings.MicDeviceName, ""),
            .MicSeparateTracks = (settings IsNot Nothing) AndAlso
                                 settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack,
            .AudioClockMode = If(settings IsNot Nothing, settings.AudioClockMode, "Legacy"),
            .OnProcessStarted = onProcessStarted
        }
    End Function

End Class
