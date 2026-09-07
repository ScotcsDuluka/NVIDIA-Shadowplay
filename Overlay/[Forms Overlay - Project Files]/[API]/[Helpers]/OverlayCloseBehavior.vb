

Imports System.Windows.Forms

Friend Module OverlayGuard

    Public ReadOnly Property Engaged As Boolean
        Get
            Return Base.IF_OpenShare
        End Get
    End Property

    Public Sub InterceptClose(f As Form, e As FormClosingEventArgs)
        If e.CloseReason <> CloseReason.UserClosing Then Return
        e.Cancel = True             
        If Not Engaged Then Return
        HideOverlay(f)
    End Sub

    Public Function EscapeHide(f As Form, keyData As Keys) As Boolean
        If keyData <> Keys.Escape Then Return False
        If Not Engaged Then Return False
        HideOverlay(f)
        Return True
    End Function

    Public Sub HideOverlay(f As Form)
        If Not Engaged Then Return
        f.Hide()
        Base.isFunctionActive_f3 = False
        Base.HideAllControls()
    End Sub

End Module

Partial Class Base

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Background_Top

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Background

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Overlay_Hub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Connect

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Settings

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Gallery

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Game_Filter

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Game_Filter_Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_Privacy_Control

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

Partial Class Base_KeySet

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayGuard.InterceptClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If String.IsNullOrEmpty(_captureActionKey) AndAlso OverlayGuard.EscapeHide(Me, keyData) Then Return True
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
