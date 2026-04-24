Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Class API_RUN
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    Private iconFontPath As String

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        Dim newStyle As Integer = (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
        SetWindowLong(Me.Handle, GWL_EXSTYLE, newStyle)
    End Sub

    Private Sub API_RUN_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetupLogCopy()

        SetStartup(True)
    End Sub

    Public Sub SetStartup(enable As Boolean)
        Dim appName As String = "NVIDIA API"
        Dim appPath As String = Application.ExecutablePath

        Using key As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\Run", True)
            If enable Then
                key.SetValue(appName, """" & appPath & """")
            Else
                If key.GetValue(appName) IsNot Nothing Then
                    key.DeleteValue(appName)
                End If
            End If
        End Using
    End Sub
    Private Shared Sub KillProcess(processName As String)
        Try
            ' เอา .exe ออกถ้ามี
            Dim name As String = processName.Replace(".exe", "")
            For Each proc As Process In Process.GetProcessesByName(name)
                proc.Kill()
            Next
        Catch ex As Exception
            Debug.WriteLine("Error killing process " & processName & ": " & ex.Message)
        End Try
    End Sub

    Private Sub Load_APP_Disposed(sender As Object, e As EventArgs) Handles Load_APP.Tick

        HandleAppsSmart()
        Dim fontExists As Boolean = FontHelper.CheckAndInstallUserFont("nvgcshare.ttf")
        If Not fontExists Then
            KillProcess("NVIDIA Notifier.exe")
            KillProcess("NVIDIA ShadowPlay.exe")
        End If
    End Sub

    Public Sub HandleAppsSmart()
        Dim apps As String() = {
            "NVIDIA Notifier.exe",
            "NVIDIA ShadowPlay.exe",
            "ffmpeg.exe"
        }

        Dim overlayExists As Boolean = File.Exists(Path.Combine(Application.StartupPath, "Use_Overlay"))

        For Each app In apps
            Dim exePath As String = Path.Combine(Application.StartupPath, app)
            Dim processName As String = Path.GetFileNameWithoutExtension(app)

            Dim running = Process.GetProcessesByName(processName)

            If overlayExists Then
                If running.Length = 0 AndAlso File.Exists(exePath) Then
                    Try
                        Process.Start(exePath)
                    Catch ex As Exception
                        Console.WriteLine("Cannot start " & exePath & ": " & ex.Message)
                    End Try
                End If
            Else
                For Each p In running
                    Try
                        p.Kill()
                        p.WaitForExit()
                    Catch ex As Exception
                        Console.WriteLine("Cannot kill " & processName & ": " & ex.Message)
                    End Try
                Next
            End If
        Next
    End Sub

    Private Sub API_RUN_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.WindowState = FormWindowState.Minimized
        End If
    End Sub













    Private Sub SetupLogCopy()
        ' Context Menu
        Dim menu As New ContextMenuStrip()

        ' Copy Selected
        menu.Items.Add("Copy Selected", Nothing, Sub(s, e) CopyLog(False))
        menu.Items.Add("Copy All", Nothing, Sub(s, e) CopyLog(True))
        menu.Items.Add(New ToolStripSeparator())
        menu.Items.Add("Clear", Nothing, Sub(s, e) lstLog.Items.Clear())

        lstLog.ContextMenuStrip = menu

        ' Keyboard Shortcut
        AddHandler lstLog.KeyDown, Sub(s, e)
                                       If e.Control AndAlso e.KeyCode = Keys.C Then CopyLog(False)
                                       If e.Control AndAlso e.KeyCode = Keys.A Then SelectAllLog()
                                   End Sub
    End Sub


    Private Sub CopyLog(copyAll As Boolean)
        Dim items = If(copyAll, lstLog.Items.Cast(Of Object)(),
                                 lstLog.SelectedItems.Cast(Of Object)())

        If items.Count = 0 Then Return

        Dim text = String.Join(Environment.NewLine, items.Select(Function(x) x.ToString()))
        Clipboard.SetText(text)
    End Sub


    Private Sub SelectAllLog()
        For i = 0 To lstLog.Items.Count - 1
            lstLog.SetSelected(i, True)
        Next
    End Sub








End Class