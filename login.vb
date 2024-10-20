Imports System.Net
Imports Newtonsoft.Json
Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices

Public Class Login
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80 ' สถานะสำหรับ ToolWindow (ไม่แสดงใน Alt+Tab)
    Private Const WS_EX_APPWINDOW As Integer = &H40000 ' สถานะสำหรับการแสดงใน Task Switcher
    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub

    Private Const JSON_URL As String = "https://drive.google.com/uc?export=download&id=1tcsQ6tunfe2YbAJ1J0sWBzYj5k3G-aVD"

    ' ฟังก์ชันสำหรับตรวจสอบการเข้าสู่ระบบ
    Public Function ValidateUser(username As String, password As String) As Boolean
        Dim users As List(Of User) = GetUsers()

        For Each user As User In users
            If user.Username = username AndAlso user.Password = password Then
                Return True
            End If
        Next

        Return False
    End Function

    ' ฟังก์ชันดาวน์โหลดและอ่านข้อมูลผู้ใช้จาก JSON
    Private Function GetUsers() As List(Of User)
        Try
            Using client As New WebClient()
                Dim json As String = client.DownloadString(JSON_URL)
                Dim userInfo As UserList = JsonConvert.DeserializeObject(Of UserList)(json)
                Return userInfo.Users
            End Using
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message)
            Return New List(Of User)()
        End Try
    End Function

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        If txtUsername.Text = "" Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ("")  ' แสดงเครื่องหมายผิด
            Notifier.text_n.Text = ("Login failed! Incorrect username or password.")
            Return
        Else

        End If
        If txtPassword.Text = "" Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ("")  ' แสดงเครื่องหมายผิด
            Notifier.text_n.Text = ("Login failed! Incorrect username or password.")
            Return
        Else

        End If


        Dim inputUser = txtUsername.Text
        Dim inputPassword = txtPassword.Text

        Dim login As New login

        ' ตรวจสอบการเข้าสู่ระบบ
        If login.ValidateUser(inputUser, inputPassword) Then
            My.Settings.User = inputUser
            My.Settings.Save()
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ("")  ' แสดงเครื่องหมายถูก
            Notifier.text_n.Text = ("Login successful! Welcome " & inputUser)
            Me.Close()
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ("")  ' แสดงเครื่องหมายผิด
            Notifier.text_n.Text = ("Login failed! Incorrect username or password.")
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Base.Home_settings.Visible = True
        Base.settings_all.Visible = True
        Base.ch.Visible = True
        Base.ch_bg.Visible = True
        Base.action_fn.Visible = True
        Base.bg_fn.Visible = True
        Base.alt_z.Start()
        Load.Stop()
        ' ปิดฟอร์มเมื่อกดปุ่ม Button1
        Me.Close()
    End Sub

    Private Sub Load_Tick(sender As Object, e As EventArgs) Handles Load.Tick
        TopMost = True
        HideFromAltTab()
    End Sub

    Private Sub login_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub login_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Base.Home_settings.Visible = True
        Base.settings_all.Visible = True
        Base.ch.Visible = True
        Base.ch_bg.Visible = True
        Base.action_fn.Visible = True
        Base.bg_fn.Visible = True
        Base.alt_z.Start()
        Load.Stop()
    End Sub

    Private Sub login_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        Base.Home_settings.Visible = True
        Base.settings_all.Visible = True
        Base.ch.Visible = True
        Base.ch_bg.Visible = True
        Base.action_fn.Visible = True
        Base.bg_fn.Visible = True
        Base.alt_z.Start()
        Load.Stop()
    End Sub
End Class

' คลาสสำหรับเก็บข้อมูลผู้ใช้
Public Class UserList
    Public Property Users As List(Of User)
End Class
' คลาสสำหรับเก็บข้อมูลผู้ใช้แต่ละคน
Public Class User
    Public Property Username As String
    Public Property Password As String
End Class
