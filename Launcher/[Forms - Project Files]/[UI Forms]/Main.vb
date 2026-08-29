
Imports System.IO
Imports System.Net.Sockets
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.Logging
Partial Public Class NVIDIA_Shadowplay_Helper


    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(
        hwnd As IntPtr,
        dwAttribute As Integer,
        ByRef pvAttribute As Integer,
        cbAttribute As Integer
    ) As Integer
    End Function

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmExtendFrameIntoClientArea(
        hwnd As IntPtr,
        ByRef pMarInset As MARGINS
    ) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MARGINS
        Public leftWidth As Integer
        Public rightWidth As Integer
        Public topHeight As Integer
        Public bottomHeight As Integer
    End Structure

    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)

        Dim attrValue As Integer = 2
        DwmSetWindowAttribute(Me.Handle, 2, attrValue, 4)

        Dim margins As New MARGINS With {
            .leftWidth = 1,
            .rightWidth = 1,
            .topHeight = 1,
            .bottomHeight = 1
        }

        DwmExtendFrameIntoClientArea(Me.Handle, margins)
    End Sub


    <DllImport("user32.dll")>
    Private Shared Function ReleaseCapture() As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As Integer, lParam As Integer) As Integer
    End Function

    Private Const WM_NCLBUTTONDOWN As Integer = &HA1
    Private Const HTCAPTION As Integer = 2
    Private Sub BOX_LOGO_MouseDown(sender As Object, e As MouseEventArgs) Handles BOX_LOGO.MouseDown
        If e.Button = MouseButtons.Left Then
            ReleaseCapture()
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0)
        End If
    End Sub
    Private Sub NVIDIA_Shadowplay_Helper_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim fontExists As Boolean = FontHelper.CheckAndInstallUserFont("nvgcshare.ttf")
        If Not fontExists Then
            Return
        End If

        ' Single-source config: the toggle state lives in config.json
        ' Overlay.UseOverlayEnabled (was: the Flags\Use_Overlay marker file;
        ' reading the CWD-relative name used to silently reset the overlay).
        Use_Overlay.IsOn = AppConfigShared.ReadBool("Overlay", "UseOverlayEnabled", False)
        OpenApp()
        Timer1.Start()
    End Sub

    Private Sub RadioButton1_Click(sender As Object, e As EventArgs) Handles RadioButton1.Click
        Application.Exit()
    End Sub

    Private Sub IF_APP_Tick(sender As Object, e As EventArgs) Handles IF_APP.Tick
        Dim ShadowPlay As Boolean = Process.GetProcessesByName("NVIDIA ShadowPlay").Length > 0
        Dim Ready_Use As String = AppLayout.P("Flags", "Ready")
        Dim isReady As Boolean = File.Exists(Ready_Use)

        ' ── NVIDIA ShadowPlay + Overlay API ──
        Dim overlayActive As Boolean = ShadowPlay AndAlso isReady
        overlay_game.Checked = overlayActive
        overlay_game.Text = If(Not ShadowPlay OrElse isReady, "O V E R L A Y  A P I", "L o a d i n g . . .")
        openoverlay.Visible = overlayActive
        NvStatusDot_OVERLAYAPI.Status = If(ShadowPlay AndAlso Not isReady,
            NvStatusDot.DotStatus.Loading,
            If(overlayActive, NvStatusDot.DotStatus.Running, NvStatusDot.DotStatus.Stopped))

        ' ── NVIDIA Notifier ──
        Dim notifierRunning As Boolean = Process.GetProcessesByName("NVIDIA Notifier").Length > 0
        API_OVERLAY.Checked = notifierRunning
        NvStatusDot_NVNOTIFIER.Status = If(notifierRunning,
            NvStatusDot.DotStatus.Running, NvStatusDot.DotStatus.Stopped)
        If Not notifierRunning Then AppLayout.DeleteFileIfExists(Ready_Use)

        ' ── NVIDIA API ──
        Dim apiRunning As Boolean = Process.GetProcessesByName("NVIDIA API").Length > 0
        NVAPI.Checked = apiRunning
        NvStatusDot_NVAPI.Status = If(apiRunning,
            NvStatusDot.DotStatus.Running, NvStatusDot.DotStatus.Stopped)

        ' ── Overlay toggle (config.json Overlay.UseOverlayEnabled) ──
        ' Writes only on mismatch: parse-patch-write of the CURRENT file, so
        ' sections owned by other processes survive; the API hub enforces the
        ' value every second (start/keep-alive vs kill the overlay stack).
        Dim overlayEnabledStored As Boolean = AppConfigShared.ReadBool("Overlay", "UseOverlayEnabled", False)
        If Use_Overlay.IsOn Then
            If Not overlayEnabledStored Then
                AppConfigShared.WriteBool("Overlay", "UseOverlayEnabled", True)
            End If
            overlay_text.ForeColor = Color.White
        Else
            If overlayEnabledStored Then
                AppConfigShared.WriteBool("Overlay", "UseOverlayEnabled", False)
            End If
            overlay_text.ForeColor = Color.DimGray
        End If
    End Sub

    Public Sub OpenApp()
        ' ExePath: Application\<name> in the staged tree, layout root in a dev
        ' bin\ (where the API exe builds flat — Application\ does not exist).
        Dim exePath As String = AppLayout.ExePath("NVIDIA API.exe")
        Try
            Process.Start(exePath)

        Catch ex As Exception
        End Try
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Opacity = 0

        Me.SetStyle(ControlStyles.ResizeRedraw, True)
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Me.Opacity < 1 Then
            Me.Opacity += 0.2
        Else
            Timer1.Stop()
        End If
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub Null_OVERLAY_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.Click
        ' Single-source config: clear the overlay switch in config.json
        ' (was: delete the Flags\Use_Overlay marker — the CWD-relative name
        ' missed it and the overlay survived the kill).
        AppConfigShared.WriteBool("Overlay", "UseOverlayEnabled", False)
        Dim apps = {
        "NVIDIA Notifier.exe",
        "NVIDIA ShadowPlay.exe",
        "NVIDIA API.exe",
        "NVIDIA Capture.exe"
    }

        For Each app In apps
            Dim processName = Path.GetFileNameWithoutExtension(app)
            Dim running = Process.GetProcessesByName(processName)
            For Each proc In running
                Try
                    proc.Kill()
                Catch ex As Exception
                    ' ROOT-FIXED LAYOUT: error log belongs in <root>\Logs\.
                    Dim logsDir As String = AppLayout.P("Logs")
                    If Not Directory.Exists(logsDir) Then Directory.CreateDirectory(logsDir)
                    File.AppendAllText(IO.Path.Combine(logsDir, "kill_error.log"),
                        $"{proc.ProcessName} : {ex.Message}" & Environment.NewLine)
                End Try
            Next
        Next
        Application.Exit()
    End Sub

    Private Sub NvButton1_Click(sender As Object, e As EventArgs) Handles openoverlay.Click
        tcp.Send("open_overlay")
    End Sub
End Class
