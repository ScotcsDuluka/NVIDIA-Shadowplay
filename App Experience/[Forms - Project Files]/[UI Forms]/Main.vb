
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

        If IO.File.Exists("Use_Overlay") Then
            Use_Overlay.IsOn = True
        Else
            Use_Overlay.IsOn = False
        End If
        OpenApp()
        Timer1.Start()
    End Sub

    Private Sub RadioButton1_Click(sender As Object, e As EventArgs) Handles RadioButton1.Click
        Application.Exit()
    End Sub

    Private Sub IF_APP_Tick(sender As Object, e As EventArgs) Handles IF_APP.Tick
        Dim ShadowPlay As Boolean = Process.GetProcessesByName("NVIDIA ShadowPlay").Length > 0
        Dim Ready_Use As String = Path.Combine(Application.StartupPath, "Ready")

        Dim isReady As Boolean = File.Exists(Ready_Use)

        overlay_game.Checked = ShadowPlay AndAlso isReady
        overlay_game.Text = If(Not ShadowPlay OrElse isReady, "IN-GAME OVERLAY", "Loading...")
        openoverlay.Visible = ShadowPlay AndAlso isReady


        Dim API As Boolean = Process.GetProcessesByName("NVIDIA Notifier").Length > 0

        If API Then
            API_OVERLAY.Checked = CheckState.Checked
        Else
            API_OVERLAY.Checked = CheckState.Unchecked
            File.Delete(Ready_Use)
        End If

        Dim NVAPIC As Boolean = Process.GetProcessesByName("NVIDIA API").Length > 0

        If NVAPIC Then
            NVAPI.Checked = CheckState.Checked
        Else
            NVAPI.Checked = CheckState.Unchecked
        End If


        Dim overlayPath As String = Path.Combine(Application.StartupPath, "Use_Overlay")

        If Use_Overlay.IsOn Then
            If Not File.Exists(overlayPath) Then
                File.Create(overlayPath).Dispose()
            End If
            overlay_text.ForeColor = Color.White
        Else
            overlay_text.ForeColor = Color.DimGray
            If File.Exists(overlayPath) Then File.Delete(overlayPath)
        End If
    End Sub

    Public Sub OpenApp()
        Dim exePath As String = Application.StartupPath & "\NVIDIA API.exe"
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
        If File.Exists("Use_Overlay") Then
            File.Delete("Use_Overlay")
        End If
        Dim apps As String() = {
        "NVIDIA Notifier.exe",
        "NVIDIA ShadowPlay.exe",
        "NVIDIA API.exe"
    }

        For Each app In apps
            Dim processName As String = Path.GetFileNameWithoutExtension(app)
            Dim running = Process.GetProcessesByName(processName)
            For Each proc In running
                Try
                    proc.Kill()
                Catch ex As Exception
                    File.AppendAllText("kill_error.log", $"{proc.ProcessName} : {ex.Message}" & Environment.NewLine)
                End Try
            Next
        Next
        Application.Exit()
    End Sub

    Private Sub openoverlay_Click(sender As Object, e As EventArgs) Handles openoverlay.Click
        tcp.Send("open_overlay")
    End Sub
End Class
