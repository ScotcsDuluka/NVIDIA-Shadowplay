' OscHostForm.vb — M1 skeleton: the real window mechanics (WebView2 host,
' transparency, displayRect click-through, tray) are wired in the next
' milestone steps; this stub only gives Program.vb something to run so the
' project builds and the controller server / TCP client can be verified
' headlessly.

Imports System
Imports System.Windows.Forms

Public Class OscHostForm
    Inherits Form

    Public Sub New()
        Text = "NVIDIA Overlay Engine"
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Opacity = 0
    End Sub

End Class
