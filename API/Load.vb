Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

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
        ' กำหนด path หลังจาก form load แล้ว
        iconFontPath = Path.Combine(Application.StartupPath, "Languages", "_icon.ttf")

        ' ติดตั้งฟอนต์ครั้งเดียวตอน load
        FontManager.InstallFont(iconFontPath)

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

    Private Sub Load_APP_Disposed(sender As Object, e As EventArgs) Handles Load_APP.Tick
        HideFromAltTab()
        HandleAppsSmart()
        ' ❌ เอา FontManager.InstallFont ออกจากนี้
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

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        FontManager.UninstallFont(iconFontPath)
    End Sub
End Class