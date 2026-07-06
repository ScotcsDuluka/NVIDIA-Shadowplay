Imports System.Diagnostics

Namespace CaptureCore

    ''' <summary>
    ''' ★ Phase 1 refactor: Centralized capture limits.
    '''
    ''' WHY THIS EXISTS:
    '''   Before this file, the same logical constants were duplicated across
    '''   three places with INCONSISTENT values:
    '''
    '''     - ScreenRecorder.MIN_BITRATE    = 500
    '''     - Base_RecordingsSet.MIN_BITRATE_GLOBAL = 500   (same intent)
    '''     - ScreenRecorder.MAX_BITRATE    = 300000  (recorder hard cap)
    '''     - Base_RecordingsSet.MAX_BITRATE_GLOBAL = 150000  (UI input cap)  ← DIFFERENT
    '''     - ScreenRecorder.MAX_FRAMERATE  = 240  (recorder hard cap)
    '''     - Base_RecordingsSet.MAX_FPS_GLOBAL = 800  (UI input cap)  ← DIFFERENT
    '''
    '''   The two MAX_* values are intentionally different:
    '''     - RECORDER hard cap = absolute ceiling the FFmpeg pipeline accepts.
    '''     - UI input cap      = what the settings UI allows the user to type.
    '''   UI can allow typing 800 fps, but the recorder will clamp at 240.
    '''   That behavior is preserved here as two distinct constants.
    '''
    ''' MIGRATION POLICY:
    '''   - Old constants in ScreenRecorder / Base_RecordingsSet are kept as
    '''     backward-compatible aliases (= CaptureLimits.XXX). No external
    '''     caller breaks.
    '''   - New code should reference CaptureLimits directly.
    ''' </summary>
    Public Module CaptureLimits

#Region "Bitrate (kbps)"
        ''' <summary>Absolute minimum bitrate the recorder will accept.</summary>
        Public Const MIN_BITRATE As Integer = 500

        ''' <summary>Absolute maximum bitrate the recorder will accept (FFmpeg hard cap).</summary>
        Public Const MAX_BITRATE_RECORDER As Integer = 300000

        ''' <summary>Maximum bitrate the settings UI will allow the user to enter.</summary>
        Public Const MAX_BITRATE_UI As Integer = 150000

        ''' <summary>Default bitrate when no user preference is loaded.</summary>
        Public Const DEFAULT_BITRATE As Integer = 8000
#End Region

#Region "Framerate (fps)"
        ''' <summary>Minimum framerate the recorder will accept.</summary>
        Public Const MIN_FRAMERATE As Integer = 1

        ''' <summary>Absolute maximum framerate the recorder will accept (FFmpeg hard cap).</summary>
        Public Const MAX_FRAMERATE_RECORDER As Integer = 240

        ''' <summary>Maximum framerate the settings UI will allow the user to enter.</summary>
        Public Const MAX_FRAMERATE_UI As Integer = 800

        ''' <summary>Default framerate when no user preference is loaded.</summary>
        Public Const DEFAULT_FRAMERATE As Integer = 60
#End Region

#Region "Encoder Preset (1=slowest/high quality, 7=fastest/lower quality)"
        Public Const MIN_ENCODER_PRESET As Integer = 1
        Public Const MAX_ENCODER_PRESET As Integer = 7
        Public Const DEFAULT_ENCODER_PRESET As Integer = 4
#End Region

#Region "Replay Buffer Duration (seconds)"
        Public Const MIN_REPLAY_DURATION As Integer = 15
        Public Const MAX_REPLAY_DURATION As Integer = 1200
        Public Const DEFAULT_REPLAY_DURATION As Integer = 60
#End Region

#Region "Replay Buffer Capacity"
        ''' <summary>Maximum number of segment files the buffer will keep.</summary>
        Public Const BUFFER_MAX_SEGMENTS As Integer = 2400

        ''' <summary>Maximum buffer duration in seconds (matches MAX_REPLAY_DURATION).</summary>
        Public Const BUFFER_MAX_DURATION As Integer = 1200
#End Region

#Region "Resolution (UI-level bounds only — recorder itself has no hard cap)"
        Public Const MIN_RESOLUTION_WIDTH As Integer = 320
        Public Const MAX_RESOLUTION_WIDTH As Integer = 7680
        Public Const MIN_RESOLUTION_HEIGHT As Integer = 240
        Public Const MAX_RESOLUTION_HEIGHT As Integer = 4320
#End Region

#Region "UI Cooldown"
        ''' <summary>
        ''' Minimum interval between user-initiated actions (record toggle,
        ''' replay toggle, save replay). Used by both the recorder engine
        ''' AND the UI layer as defense-in-depth against hotkey spam.
        ''' </summary>
        Public Const ACTION_COOLDOWN_MS As Integer = 500
#End Region

    End Module

End Namespace
