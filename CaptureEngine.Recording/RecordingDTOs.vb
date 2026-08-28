Option Strict On
Option Explicit On
Option Infer On

' DTOs for CaptureEngine.Recording

Imports System.Diagnostics

Namespace CaptureEngine.Recording

    ''' <summary>
    ''' Recording engine state. Distinct from Foundation's EngineState
    ''' (which is the lifecycle state machine for CaptureEngine.vb).
    ''' </summary>
    Public Enum RecordingEngineState
        Created
        Initializing
        Idle
        Recording
        Stopping
        Faulted
        Disposed
    End Enum

    ''' <summary>
    ''' Configuration for a single recording session.
    ''' Per-session concerns only — codec/bitrate/GOP are process-lifetime
    ''' (persistent encoder owner) and live in EngineStartupConfig.
    ''' </summary>
    Public NotInheritable Class SessionConfig
        Public Property OutputPath As String = ""
        Public Property DurationSeconds As Integer = 30
        Public Property UseSharedHandle As Boolean = False

        ' FFmpeg path (from EngineConfigV2.Runtime.FFmpegPath or CLI override)
        Public Property FFmpegPath As String = ""

        ' ── Phase 12b: audio per-session options ──
        ''' <summary>Record system-audio loopback into the WAV sidecar.</summary>
        Public Property AudioEnabled As Boolean = True
        ''' <summary>System-audio volume applied at mux (0..2, 1 = unchanged).</summary>
        Public Property SystemVolume As Single = 1.0F

        ' ── M1: microphone second track (two independent WavSidecarWriter
        '    instances — per GPT standing design: separate queue/writer/
        '    accounting/start-timestamp per track; NO merged queues) ──
        ''' <summary>Record the microphone into a second WAV sidecar.</summary>
        Public Property MicEnabled As Boolean = False
        ''' <summary>Mic volume applied at mux (0..2, 1 = unchanged).</summary>
        Public Property MicVolume As Single = 1.0F
        ''' <summary>Preferred mic device id (NAudio MMDevice ID). Empty = default capture device.</summary>
        Public Property MicDeviceId As String = ""
        ''' <summary>Preferred mic device name (fallback when the id does not match).</summary>
        Public Property MicDeviceName As String = ""
        ''' <summary>
        ''' Mux mode when BOTH system + mic are present: True = two separate output
        ''' tracks (-map 0:v -map 1:a -map 2:a), False = amix single track.
        ''' Mirrors the legacy AudioTrackMode behavior.
        ''' </summary>
        Public Property MicSeparateTracks As Boolean = False

        ' ── GLM/6 audit #7: WAV sidecars are evidence/debug artifacts (the live
        '    mux is the real output). Three parallel disk writes per session is
        '    wasted I/O in steady state — default OFF now that live-mux is
        '    runtime-validated. Set True to bring back debug WAVs.
        Public Property EvidenceSidecar As Boolean = False

        ' ★ OBS keep-alive switch: renders endless silence to the loopback
        ' device so WASAPI delivers a continuous stream. Owner reported FULL-
        ' SCALE NOISE with it active on their machine (format mismatch between
        ' the WasapiOut render and the loopback capture, most likely) —
        ' default OFF until the root cause is proven. AudioTap gap-fill
        ' (proven stable in the 21:47 run) remains the primary mechanism.
        Public Property SilenceKeepAlive As Boolean = False

        ' ★ P13.3 clock-mode switch (docs/PHASE-13-SHADOWPLAY-CLOCK.md §4):
        '   "Legacy" = the proven AudioTap v2 (Stopwatch arrival gap-fill +
        '   SilenceKeepAlive switch as configured). "Device" = AudioTap v3 +
        '   WasapiPositionCapture: the system track's timeline comes from
        '   WASAPI qpcPosition stamps (hardware-measured gaps, exact QPC
        '   anchor at the mux). Unknown values normalize to "Legacy".
        '   The mic track stays on the legacy tap in both modes this phase.
        Public Property AudioClockMode As String = "Legacy"

        ' ── Phase 12b: process-lifecycle hook (no-orphan-FFmpeg criterion) ──
        ''' <summary>
        ''' Invoked right after CaptureSession spawns any child process
        ''' (H.264 wrap, verify). The host assigns the child to its
        ''' JobObjectGuard here. Additive — Nothing = previous behavior.
        ''' </summary>
        Public Property OnProcessStarted As Action(Of Process) = Nothing
    End Class

    ''' <summary>
    ''' Phase 12b: process-lifetime engine startup options — consumed once by
    ''' RecordingEngine.Initialize (the persistent NVENC session is expensive
    ''' to rebuild, so codec/bitrate/GOP cannot change per session).
    ''' Values mirror the proven legacy defaults when unset.
    ''' </summary>
    Public NotInheritable Class EngineStartupConfig
        Public Property CodecKey As String = "NVENC_H264"
        Public Property BitrateBps As Long = 20_000_000L
        Public Property GopSize As Integer = 60
        Public Property RateControl As String = "cbr"
        Public Property Preset As String = "p4"
    End Class

    ''' <summary>
    ''' Result of a recording session.
    ''' </summary>
    Public NotInheritable Class SessionResult
        Public Property OutputPath As String = ""
        Public Property RequestedDurationSec As Integer
        Public Property ActualDurationSec As Double
        Public Property FramesCaptured As Long
        Public Property FramesEncoded As Long
        ''' <summary>
        ''' ★ CFR pacing evidence: ticks where the screen was static and the
        ''' LAST frame was re-encoded (a duplicate P-frame). High values on idle
        ''' desktops are EXPECTED and correct (DXGI delivers no frames when the
        ''' screen does not change). dup ≈ 0 during full-motion capture.
        ''' </summary>
        Public Property FramesDuplicated As Long
        Public Property Drops As Long
        Public Property NvencErrors As Long
        Public Property TotalVideoBytes As Long
        Public Property AudioSamples As Long
        Public Property AudioBytes As Long
        Public Property VideoStreamFound As Boolean
        Public Property AudioStreamFound As Boolean
        Public Property FileExists As Boolean
        Public Property FileSize As Long
        Public Property ErrorMessage As String = ""

        ' ── Phase 12b: sync + accounting evidence ──
        ''' <summary>System-audio offset applied at mux (sec; &gt;0 = audio head skipped, &lt;0 = audio delayed).</summary>
        Public Property SystemOffsetSec As Double
        ''' <summary>Video duration used for mux -t (ffprobe of wrapped MP4, fallback wall-clock).</summary>
        Public Property MuxVideoDurationSec As Double
        ''' <summary>
        ''' ★ Sync-fix evidence: frame rate used for the raw-H.264→MP4 wrap.
        ''' Measured average (framesEncoded / capture span) — NOT the display rate.
        ''' If this differs from the display Hz by more than a few percent, the old
        ''' fixed-rate wrap would have time-compressed the video (progressive drift).
        ''' </summary>
        Public Property WrapFps As Double
        ''' <summary>WAV sidecar accounting is consistent (enqueued = written + dropped).</summary>
        Public Property AudioAccountingOk As Boolean
        ''' <summary>WAV sidecar bytes dropped under backpressure (0 = healthy run).</summary>
        Public Property AudioDroppedBytes As Long

        ' ── M1: mic track evidence (independent accounting) ──
        ''' <summary>Mic track bytes written to the mic sidecar WAV.</summary>
        Public Property MicBytes As Long
        ''' <summary>Mic sidecar dropped bytes (0 = healthy run).</summary>
        Public Property MicDroppedBytes As Long
        ''' <summary>Mic sidecar accounting invariant holds.</summary>
        Public Property MicAccountingOk As Boolean
        ''' <summary>Mic sample count (per the MIC device's own format).</summary>
        Public Property MicSamples As Long
        ''' <summary>Mic A/V offset applied at mux (own timeline, proven model).</summary>
        Public Property MicOffsetSec As Double

        Public ReadOnly Property Pass As Boolean
            Get
                Return FramesEncoded > 0 AndAlso
                       NvencErrors = 0 AndAlso
                       FileExists AndAlso
                       FileSize > 0 AndAlso
                       VideoStreamFound AndAlso
                       AudioStreamFound
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Engine status — safe to poll from any thread.
    ''' </summary>
    Public NotInheritable Class EngineStatus
        Public Property State As RecordingEngineState
        Public Property CurrentSessionId As String  ' Nothing if Idle
        Public Property FramesEncodedThisSession As Long
        Public Property LastSessionResult As SessionResult  ' Nothing if no session yet
    End Class

End Namespace
