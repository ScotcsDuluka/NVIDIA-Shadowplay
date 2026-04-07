Public Class NoCloseForm
    Inherits Form

    Public Shared AllowClose As Boolean = True

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)

        If Not AllowClose Then
            e.Cancel = True
        End If

        MyBase.OnFormClosing(e)
    End Sub

End Class