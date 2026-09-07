Option Strict On
Option Explicit On
Option Infer On

' TestShims.vb — TEST-ONLY compile shims. Never shipped, never referenced
' by production projects (this file lives only in the test project folder,
' so the Engine's own glob never sees it).
'
' Why this exists:
'   The linked production files are compiled by the ENGINE project with
'   WinForms project-level imports (NVIDIA Capture.vbproj imports
'   System.Windows.Forms). On Linux there is no WinForms runtime or
'   reference assembly — but the ONLY WinForms symbol the linked files
'   touch is Screen.PrimaryScreen inside CaptureSettings.GetCaptureResolution
'   (CaptureSettings.vb:462-467), which the CT-4 contract never calls.
'
' Why the shim is NOT in namespace System.Windows.Forms:
'   Declaring a source namespace under System.* makes the VB compiler
'   resolve framework types (AssemblyInfo attributes) against the source
'   namespace first — BC31424 failures at build time. A neutral namespace
'   + a matching <Import Include=...> gives bare `Screen` in
'   CaptureSettings.vb the exact same resolution result with zero
'   framework-namespace interference. On Windows the Engine keeps the real
'   Screen because the shim is not part of its compilation.

Namespace Shims
    ''' <summary>Compile shim for the ONE WinForms symbol the linked
    ''' production files reference. NOT functional — tests must never
    ''' exercise CaptureSettings.GetCaptureResolution().</summary>
    Public Class Screen
        Public Shared ReadOnly Property PrimaryScreen As New Screen()

        Public ReadOnly Property Bounds As System.Drawing.Rectangle
            Get
                Return New System.Drawing.Rectangle(0, 0, 0, 0)
            End Get
        End Property
    End Class
End Namespace
