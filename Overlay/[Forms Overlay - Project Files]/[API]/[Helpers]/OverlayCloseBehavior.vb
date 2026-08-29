' T5 (OWNER order): pressing Alt+F4 / X on an overlay form must NOT close it —
' the form hides instead, so the overlay (and the capture engine behind it)
' keeps running. ESC also hides the overlay.
'
' Design notes (evidence-based):
'  - Implemented as per-class PARTIALS in this single file. NO shared base class
'    is reintroduced — the WinForms designer only needs to load the base class,
'    which is now System.Windows.Forms.Form for all of these forms (T4).
'  - Only CloseReason.UserClosing is intercepted. The real quit paths stay
'    intact: Logo_Click (Background Top) -> Application.Exit(), and
'    Main_Top_Click -> Application.Restart(). Those arrive with
'    CloseReason.ApplicationExitCall / MdiFormClosing etc. and pass through.
'  - OnFormClosing is cancelled BEFORE MyBase.OnFormClosing raises the
'    FormClosing event, so a mere "hide" never triggers close-time side
'    effects (e.g. Base_FormClosing unregisters hotkeys,
'    Base_TestFormClosing shuts down the engine supervisor).
'  - ESC uses ProcessCmdKey, which works regardless of KeyPreview and of which
'    control has focus inside the form. FACT: keys only reach a window that
'    owns keyboard focus — ESC hides an overlay only while it is the active
'    window.
'  - Base_KeySet exception: during hotkey capture, ESC means "cancel capture"
'    (Base_KeySet_KeyDown). ESC only hides that form when NOT capturing.
'  - T6 (OWNER order): hiding goes through Base.HideAllControls() — the
'    canonical dismiss protocol (sub-form first, then overlay state/opacities/
'    main trio reset) — NOT a bare Form.Hide().

Imports System.Windows.Forms

Friend Module OverlayCloseBehaviorGuard

    ''' <summary>Cancel a user-initiated close (Alt+F4 / X) and hide instead.
    ''' All other CloseReasons pass through untouched so the app can really exit.</summary>
    Public Sub HideInsteadOfClose(f As Form, e As FormClosingEventArgs)
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            HideOverlay(f)
        End If
    End Sub

    ''' <summary>Hide the whole overlay using the canonical protocol the codebase
    ''' already uses (Settings / Gallery / TCP open_overlay): hide the focused
    ''' sub-form first, then Base.HideAllControls() which resets overlay state
    ''' (isFunctionActive), opacities and hides the main trio
    ''' (Base + Base_Background + Base_Background_Top). Also clears the Game
    ''' Filter toggle flag (isFunctionActive_f3), mirroring the open_overlay
    ''' handler, so the next hotkey toggle starts from a clean state.</summary>
    ''' <remarks>Gated by IF_OpenShare: when the overlay is disengaged (a
    ''' sub-page like Settings / Gallery / Recordings is active, the flag is
    ''' False) this does NOTHING — the exact same guard every hotkey handler
    ''' in Sub_Hotkey.vb uses ("If IF_OpenShare = False Then Return").</remarks>
    Public Sub HideOverlay(f As Form)
        If Base.IF_OpenShare = False Then Return
        f.Hide()
        Base.isFunctionActive_f3 = False
        Base.HideAllControls()
    End Sub

    ''' <summary>True when the main Share panel is the active surface.
    ''' Single authority for reading Base.IF_OpenShare — cross-class reads go
    ''' through here so no partial relies on cross-class name resolution.
    ''' When False, a sub-page (Settings / Gallery / Recordings) is active and
    ''' every hotkey handler in Sub_Hotkey.vb returns early; hide does too.</summary>
    Public Function IsOverlayEngaged() As Boolean
        Return Base.IF_OpenShare
    End Function

    ''' <summary>True for a bare ESC press (no Ctrl/Alt/Shift modifiers).</summary>
    Public Function IsEscape(keyData As Keys) As Boolean
        Return keyData = Keys.Escape
    End Function

End Module

' ===== [1] Main Menu (main form) — ESC / Alt+F4 => hide =====
Partial Class Base

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== [2] Background Top =====
Partial Class Base_Background_Top

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== [3] Background =====
Partial Class Base_Background

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== [2] Overlay Hub =====
Partial Class Base_Overlay_Hub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== [1] Connect =====
Partial Class Base_Connect

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== [0] Settings - UI Main =====
Partial Class Base_Settings

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== Gallery / [1] Main =====
Partial Class Base_Gallery

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== Game Filter / [1] Settings UI Main =====
Partial Class Base_Game_Filter

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== Game Filter / [2] Sub =====
Partial Class Base_Game_Filter_Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== [7] Privacy Control =====
Partial Class Base_Privacy_Control

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class

' ===== [4] Keyboard Shortcut — special case =====
' While a hotkey is being captured (_captureActionKey non-empty), ESC is
' "cancel capture" and must reach Base_KeySet_KeyDown untouched.
Partial Class Base_KeySet

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        OverlayCloseBehaviorGuard.HideInsteadOfClose(Me, e)
        If e.Cancel Then Return
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If OverlayCloseBehaviorGuard.IsEscape(keyData) AndAlso String.IsNullOrEmpty(_captureActionKey) Then
            If OverlayCloseBehaviorGuard.IsOverlayEngaged() = False Then Return MyBase.ProcessCmdKey(msg, keyData)
            OverlayCloseBehaviorGuard.HideOverlay(Me)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

End Class
