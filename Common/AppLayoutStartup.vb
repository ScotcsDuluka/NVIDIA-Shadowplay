' AppLayoutStartup.vb — links AppLayout.Initialize() into the VB
' application framework's Startup event (fires BEFORE the main form is
' created). Shared file: compiled into every app project, where the
' project's RootNamespace makes `Namespace My` resolve to that app's
' <Root>.My.MyApplication partial.
'
' Startup order guarantee (VB application framework):
'   MyApplication.Startup  ->  OnCreateMainForm (form ctor / Load events)
' so every Application.StartupPath-derived path can safely rely on
' AppLayout.Dir by the time any form code runs.

Namespace My

    Partial Friend Class MyApplication

        Private Sub AppLayout_Initialize(sender As Object, e As ApplicationServices.StartupEventArgs) Handles Me.Startup
            AppLayout.Initialize()
        End Sub

    End Class

End Namespace
