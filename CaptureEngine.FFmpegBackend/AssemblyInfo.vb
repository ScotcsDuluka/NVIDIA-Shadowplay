Option Strict On
Option Explicit On

Imports System.Runtime.CompilerServices

' Expose Friend (internal) members of CaptureEngine.FFmpegBackend to the
' test assembly so tests can observe internal state without exposing it
' on the public API surface.
<Assembly: InternalsVisibleTo("CaptureEngine.FFmpegTests")>
