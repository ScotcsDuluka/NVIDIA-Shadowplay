' Overlay close behavior:
'   Alt+F4 / X -> hide (never kill the overlay or the engine behind it)
'   ESC        -> hide
' Real exit stays with the Logo button (Application.Exit) and
' Application.Restart — both pass through the CloseReason gate untouched.
'
' Written as partials on purpose: NO shared base class, so the WinForms
' designer keeps loading plain System.Windows.Forms.Form.

Imports System.Windows.Forms

Friend Module OverlayGuard

    ' False while a sub-page (Settings / Gallery / Recordings) is active.
    ' Same gate every hotkey handler in Sub_Hotkey.vb uses.
    Public ReadOnly Property Engaged As Boolean
        Get
            Return Base.IF_OpenShare
        End Get
    End Property

    Public Sub InterceptClose(f As Form, e As FormClosingEventArgs)
        If e.CloseReason <> CloseReason.UserClosing Then Return
        e.Cancel = True             ' the form never actually dies
        If Not Engaged Then Return
        HideOverlay(f)
    End Sub

    ' True = ESC consumed (overlay hidden). False = caller falls back to
    ' normal key handling.
    Public Function EscapeHide(f As Form, keyData As Keys) As Boolean
        If keyData <> Keys.Escape Then Return False
        If Not Engaged Then Return False
        HideOverlay(f)
        Return True
    End Function

    ' Canonical dismiss: hide the focused form, then Base.HideAllControls()
    ' (resets state, opacities and the Base/Background/Background_Top trio),
    ' and clear the Game Filter flag so the next hotkey toggle starts clean.
    Public Sub HideOverlay(f As Form)
        If Not Engaged Then Return
        f.Hide()
        Base.isFunctionActive_f3 = False
        Base.HideAllControls()
    End Sub

End Module

' ---- [1] Main Menu ----
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

' ---- [2] Background Top ----
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

' ---- [3] Background ----
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

' ---- [2] Overlay Hub ----
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

' ---- [1] Connect ----
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

' ---- [0] Settings ----
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

' ---- Gallery ----
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

' ---- Game Filter ----
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

' ---- Game Filter Sub ----
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

' ---- Privacy Control ----
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

' ---- Keyboard Shortcut ----
' ESC during hotkey capture means "cancel capture" — KeyDown owns it.
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
