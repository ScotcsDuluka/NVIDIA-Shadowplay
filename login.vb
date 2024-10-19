Imports System.Net
Imports Newtonsoft.Json
Imports System.Drawing
Imports System.IO

Public Class Login
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
        Dim inputUser = txtUsername.Text
        Dim inputPassword = txtPassword.Text

        Dim login As New Login

        ' ตรวจสอบการเข้าสู่ระบบ
        If login.ValidateUser(inputUser, inputPassword) Then
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ("")  ' แสดงเครื่องหมายถูก
            Notifier.text_n.Text = ("Login successful! Welcome " & inputUser)
            My.Computer.FileSystem.DeleteDirectory("C:\ProgramData\NVIDIA Corporation\ShadowPlay1\", FileIO.DeleteDirectoryOption.DeleteAllContents)
            My.Computer.FileSystem.CreateDirectory("C:\ProgramData\NVIDIA Corporation\ShadowPlay1\")
            File.Create("C:\ProgramData\NVIDIA Corporation\ShadowPlay1\" & inputUser).Dispose()
            Me.Close()
        Else
            Notifier.Show()
            Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, 40)
            Notifier.icon_n.Text = ("")  ' แสดงเครื่องหมายผิด
            Notifier.text_n.Text = ("Login failed! Incorrect username or password.")
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' ปิดฟอร์มเมื่อกดปุ่ม Button1
        Me.Close()
    End Sub

    Private Sub Login_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' ยกเลิกการปิดฟอร์มเมื่อไม่ใช่ปุ่ม Button1
        If Not DirectCast(sender, Login).ActiveControl Is Button1 AndAlso
       Not DirectCast(sender, Login).ActiveControl Is btnLogin Then
            e.Cancel = True
        End If
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
