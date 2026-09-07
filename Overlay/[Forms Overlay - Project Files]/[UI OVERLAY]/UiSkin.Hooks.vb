' =====================================================================
' UiSkin.Hooks — attaches the UiTheme engine to every form uniformly.
'
' Each override runs AFTER MyBase.OnLoad, so every Handles MyBase.Load
' handler has already finished wiring (hover wiring, layout, language).
' UiTheme.SkinForm is idempotent per form (guarded internally).
'
' Faded (static pages): settings/account/gallery pages — soft fade on
' every Show.  Not faded: the main Base overlay (its Opacity is
' choreographed by the app itself).  Not skinned at all: Mica backdrop
' forms (Base_Background / Base_Background_Top), shadow windows
' (sha1..sha4), the OAuth helper window and the debug UI.
' =====================================================================

' ---- Main overlay (no fade — opacity is choreographed by app logic) ----
Partial Public Class Base
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, False)
    End Sub
End Class

Partial Public Class Base_Settings
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Gallery
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

' ---- Settings pages ----

Partial Public Class Base_RecordingsSet
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_KeySet
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_AudioSet
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Notifications
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Overlay_Hub
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Privacy_Control
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Empty
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Game_Filter
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Game_Filter_Sub
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

' ---- Duluka Connect (account) pages ----

Partial Public Class Base_Connect
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Setup
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Create
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Login
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Signin
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Profile
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Providers
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Devices
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class

Partial Public Class Base_Connect_Security
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        UiTheme.SkinForm(Me, True)
    End Sub
End Class
