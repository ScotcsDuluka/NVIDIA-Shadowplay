Public Class BlockClose
    Inherits System.Windows.Forms.Form

    Public Shared AllowClose As Boolean = False

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)

        If Not AllowClose Then
            e.Cancel = True
        End If

        MyBase.OnFormClosing(e)
    End Sub

End Class