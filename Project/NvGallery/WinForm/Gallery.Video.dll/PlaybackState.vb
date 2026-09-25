Option Strict On
Option Explicit On
Option Infer On

' PlaybackState.vb
'
' Lifecycle states of a Gallery playback session (owner-mandated set).
'
' CONTRACT (docs/GALLERY-VIDEO-PLAYBACK-DESIGN-2026-09-10.md §3.5):
'
'   Created ──Open()──► Opening ──ok──► Playing ⇄ Paused
'                          │fail           │    ▲   ▲   │
'                          ▼               │    │   └───┴─ Seek(t) ──► Seeking
'                       Faulted ◄──fault──┘    │                        │
'                          ▲                   └── Stop() ──► Stopping ──► Stopped
'                          └────────────────────── any state ──fault─────────┘
'
'   Dispose() valid from ANY state → Disposed (idempotent, one-shot).
'
' Rules enforced by PlaybackSession (and unit-tested here):
'   - Play:     Paused|Stopped → Playing; Playing → no-op; else rejected.
'   - Pause:    Playing → Paused; Paused → no-op; else rejected.
'   - Seek:     Playing|Paused → Seeking; else rejected (incl. Disposed).
'   - Stop:     any non-terminal (not Disposed) → Stopping → Stopped; idempotent.
'   - Fault:    any non-terminal → Faulted (reason carried in GalleryVideoFault).
'   - Terminal states: Stopped, Faulted, Disposed. A Faulted session is
'     recoverable only via Stop() → new Open() on a fresh session.

Namespace Gallery.Video

    Public Enum PlaybackState
        ''' <summary>Fresh session, nothing opened yet.</summary>
        Created
        ''' <summary>Probe + decoder spawn in flight (bounded by OpenTimeoutMs).</summary>
        Opening
        ''' <summary>Present loop running, clock advancing.</summary>
        Playing
        ''' <summary>Clock frozen at pause PTS; last frame held (not black).</summary>
        Paused
        ''' <summary>Seek in flight: decoders re-spawning at target, queues flushed.</summary>
        Seeking
        ''' <summary>Draining subprocesses/queues after Stop().</summary>
        Stopping
        ''' <summary>Clean terminal state after Stop.</summary>
        Stopped
        ''' <summary>Terminal fault state; reason in GalleryVideoFault. Recoverable via Stop().</summary>
        Faulted
        ''' <summary>Final; every API call returns fault; no callbacks, no threads.</summary>
        Disposed
    End Enum

    ''' <summary>Static transition truth — one authority for session + tests.</summary>
    Public Module PlaybackStateMachine

        Public ReadOnly TerminalStates As PlaybackState() = {
            PlaybackState.Stopped, PlaybackState.Faulted, PlaybackState.Disposed}

        ''' <summary>True when the command is acceptable in the given state.</summary>
        Public Function CanPlay(s As PlaybackState) As Boolean
            Return s = PlaybackState.Paused OrElse s = PlaybackState.Stopped OrElse s = PlaybackState.Playing
        End Function

        Public Function CanPause(s As PlaybackState) As Boolean
            Return s = PlaybackState.Playing OrElse s = PlaybackState.Paused
        End Function

        Public Function CanSeek(s As PlaybackState) As Boolean
            Return s = PlaybackState.Playing OrElse s = PlaybackState.Paused
        End Function

        ''' <summary>Stop is legal from every state except Disposed (idempotent).</summary>
        Public Function CanStop(s As PlaybackState) As Boolean
            Return s <> PlaybackState.Disposed
        End Function

        Public Function IsTerminal(s As PlaybackState) As Boolean
            Return TerminalStates.Contains(s)
        End Function

    End Module

End Namespace
