' Program.vb — NVIDIA Overlay Engine entry point.
'
' The Overlay Engine is the SECOND overlay host (M1): a transparent
' per-screen window hosting the NVIDIA GFE "osc" web app in WebView2,
' driven over the same loopback TCP hub protocol as the Forms overlay.
' It runs IN PARALLEL with NVIDIA ShadowPlay.exe (the Forms overlay) and
' never competes for global state:
'   - own single-instance mutex (Global\NVIDIA_Shadowplay_OscEngine)
'   - registers NO global hotkeys (HotkeyService ownership stays with the
'     Forms overlay; toggling comes from the TCP hub "open_overlay" or the
'     tray menu)
'   - no changes to any other process's behavior
'
' M1 scope: osc boot + cefQuery handshake + controller server + TCP
' RECORD_START/STOP + engine_get_status rehydration. Replay, notifications
' parity, settings UI and hotkey ownership arbitration are M2.

Imports System
Imports System.IO
Imports System.Threading

Public Class Program

    <STAThread()>
    Public Shared Sub Main(args As String())
        ' Single instance — name deliberately different from the Forms
        ' overlay's Global\NVIDIA_Shadowplay_Overlay_SingleInstance mutex.
        Dim created As Boolean = False
        Using mutex As New Mutex(True, "Global\NVIDIA_Shadowplay_OscEngine_SingleInstance", created)
            If Not created Then
                Return
            End If

            Try
                ' Root-fixed layout: assembly resolver + CWD to layout root.
                ' Must run before any type touches AppLayout paths.
                AppLayout.Initialize()
            Catch ex As Exception
                ' Dev runs outside the staged tree still work: AppLayout falls
                ' back to the exe dir. Never block startup on layout errors.
                Trace.WriteLine($"[OscEngine] AppLayout.Initialize: {ex.Message}")
            End Try

            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            Application.Run(New OscHostForm())
        End Using
    End Sub

End Class
