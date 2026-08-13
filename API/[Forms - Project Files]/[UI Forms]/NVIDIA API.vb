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

    ' ✅ m2 FIX: removed dead 'Private iconFontPath As String' — never assigned, never read.

    ' ═══════════════════════════════════════════
    '  ซ่อน/แสดง จาก Alt-Tab
    ' ═══════════════════════════════════════════
    Private isHiddenFromAltTab As Boolean = False

    Private Sub ToggleAltTabVisibility(hide As Boolean)
        isHiddenFromAltTab = hide
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)

        If hide Then
            ' ซ่อนจาก Alt-Tab
            Dim newStyle As Integer = (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
            SetWindowLong(Me.Handle, GWL_EXSTYLE, newStyle)
        Else
            ' แสดงใน Alt-Tab
            Dim newStyle As Integer = (style And Not WS_EX_TOOLWINDOW) Or WS_EX_APPWINDOW
            SetWindowLong(Me.Handle, GWL_EXSTYLE, newStyle)
        End If
    End Sub
    Private Sub API_RUN_Load(sender As Object, e As EventArgs) Handles Me.Load
        SetupLogCopy()
        Dim overlayExists As Boolean = File.Exists(Path.Combine(Application.StartupPath, "Dev"))
        If overlayExists Then
            ToggleAltTabVisibility(False)
            Opacity = 1
            Me.WindowState = FormWindowState.Normal
        Else
            ToggleAltTabVisibility(True)
            Me.WindowState = FormWindowState.Minimized
            Me.Hide()
            Opacity = 0
        End If
        SetStartup(True)
        notifyIcon.Icon = Me.Icon
        notifyIcon.Text = "NVIDIA API Server"
        notifyIcon.Visible = True

        ' Menu เวลาคลิกขวา
        Dim contextMenu As New ContextMenuStrip()

        Dim miShow As New ToolStripMenuItem("Show")
        miShow.Font = New Font("Consolas", 9)
        AddHandler miShow.Click, AddressOf Tray_Show
        contextMenu.Items.Add(miShow)

        contextMenu.Items.Add(New ToolStripSeparator())

        Dim miExit As New ToolStripMenuItem("Exit")
        miExit.Font = New Font("Consolas", 9)
        AddHandler miExit.Click, AddressOf Tray_Exit
        contextMenu.Items.Add(miExit)

        notifyIcon.ContextMenuStrip = contextMenu

        ' Double-click → แสดง Form
        AddHandler notifyIcon.DoubleClick, AddressOf Tray_Show
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
    ' ✅ m1 FIX: removed dead KillProcess — FONTS.vb has its own identical
    ' copy that is actually called. This one was never referenced.

    ' ✅ FIX: flag to stop the font-install bootloop. Old code: Load_APP.Tick ran every 1s;
    ' if nvgcshare.ttf was missing from both %LOCALAPPDATA%\...\Fonts AND appdir, FontHelper
    ' returned False and the supervisor immediately killed Notifier + ShadowPlay → next tick
    ' supervisor restarted them → next tick FontHelper killed them again → endless loop, CPU spike.
    Private _fontInstallFailedOnce As Boolean = False

    Private Sub Load_APP_Disposed(sender As Object, e As EventArgs) Handles Load_APP.Tick

        HandleAppsSmart()
        If Not _fontInstallFailedOnce Then
            Dim fontExists As Boolean = FontHelper.CheckAndInstallUserFont("nvgcshare.ttf")
            If Not fontExists Then
                ' Install failed once → don't try again every 1s. Don't kill Notifier/ShadowPlay either —
                ' they'll fall back to a default font. User can re-trigger by restarting the app.
                _fontInstallFailedOnce = True
                Log("[Warn] NVIDIA API", "font_install_failed_disabling_retry")
            End If
        End If
    End Sub

    Public Sub HandleAppsSmart()
        ' ✅ FIX: removed ffmpeg.exe from the supervised list. ffmpeg is a CLI tool spawned
        ' by the Engine on demand; it is NOT a long-running background process and should
        ' not be "kept alive" or killed by the API Hub. Old behavior: when Use_Overlay marker
        ' was absent, the API killed every ffmpeg.exe on the machine — including ones launched
        ' by unrelated software (OBS, HandBrake, streamers, video editors).
        Dim apps As String() = {
            "NVIDIA Notifier.exe",
            "NVIDIA ShadowPlay.exe",
            "NVIDIA Capture.exe"
        }

        Dim overlayExists As Boolean = File.Exists(Path.Combine(Application.StartupPath, "Use_Overlay"))

        For Each app In apps
            Dim exePath As String = Path.Combine(Application.StartupPath, app)
            Dim processName As String = Path.GetFileNameWithoutExtension(app)

            ' ✅ M4 FIX: dispose every Process object. HandleAppsSmart runs every 1s
            ' and Process.GetProcessesByName returns Process objects that hold
            ' SafeProcessHandle. Without disposal, handles accumulate over hours
            ' until GC finalizes them — slow leak.
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
                    Finally
                        Try : p.Dispose() : Catch : End Try
                    End Try
                Next
            End If

            ' Dispose the rest (the ones we didn't kill).
            For Each p In running
                Try : p.Dispose() : Catch : End Try
            Next
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






    ' ═══════════════════════════════════════════
    '  Minimize → Tray
    ' ═══════════════════════════════════════════
    Private Sub API_RUN_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Hide()
            notifyIcon.Visible = True
            notifyIcon.BalloonTipTitle = "Server"
            notifyIcon.BalloonTipText = "Server is running in background"
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info
            notifyIcon.ShowBalloonTip(2000)
            ToggleAltTabVisibility(True)
            Me.Opacity = 0
        End If
    End Sub

    ' ═══════════════════════════════════════════
    '  Tray Actions
    ' ═══════════════════════════════════════════
    Private Sub Tray_Show(sender As Object, e As EventArgs)
        Me.Show()
        Me.Opacity = 1
        Me.WindowState = FormWindowState.Normal
        Me.Activate()
        ToggleAltTabVisibility(False)
    End Sub


    Private Sub Tray_Exit(sender As Object, e As EventArgs)
        ' ✅ M5 FIX: set _isShuttingDown = True FIRST so the StartServer accept
        ' loop and HeartbeatMonitor know we're shutting down. Without this,
        ' listener.Stop() throws SocketException in AcceptTcpClientAsync which
        ' gets logged as a spurious 'accept_failed_' error even though it's
        ' a clean exit.
        _isShuttingDown = True

        ' ✅ P2.6: cancel heartbeat before closing clients (was missing).
        If _heartbeatCts IsNot Nothing Then
            Try : _heartbeatCts.Cancel() : Catch : End Try
        End If

        ' ปิดทุก Client
        SyncLock clientsLock
            For Each c In clients
                Try : c.Client.Close() : Catch : End Try
            Next
            clients.Clear()
        End SyncLock

        ' หยุด Server
        Try
            listener.Stop()
        Catch : End Try

        notifyIcon.Visible = False
        Application.Exit()
    End Sub

End Class