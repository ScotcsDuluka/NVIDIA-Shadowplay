Imports System.Runtime.InteropServices
Imports Microsoft.Web.WebView2.WinForms

Public Class www
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

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub TextBox1_Enter(sender As Object, e As EventArgs)
    End Sub

    'ตัวแปร Key การตรวจจับ
    <DllImport("user32.dll")>
    Private Shared Function GetAsyncKeyState(vKey As Integer) As Short
    End Function

    Private Const VK_ALT As Integer = &H12 ' Alt key
    Private Const VK_W As Integer = &H57 ' W key
    Private Const VK_Q As Integer = &H51 ' Q key
    Private Const VK_CAPITAL As Integer = &H14 ' Caps Lock key

    Private isFunctionActive As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัww
    Private isKeyPressed As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private isFunctionActivef As Boolean = False ' ตัวแปรสำหรับเปิด/ปิดฟังก์ชัww
    Private isKeyPressedf As Boolean = False ' เพื่อตรวจสอบว่าปุ่มถูกกดอยู่

    Private Sub key_Tick(sender As Object, e As EventArgs) Handles key.Tick
        HideFromAltTab()
        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_Q) And &H8000) <> 0 Then

            If Not isKeyPressedf Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActivef = Not isFunctionActivef ' สลับสถานะฟังก์ชัน
                If isFunctionActivef Then
                    url.Visible = True
                Else
                    url.Visible = False
                End If
                isKeyPressedf = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressedf = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If

        If Base.Opacity = 0 Then

        Else
            TopMost = True
        End If

        If (GetAsyncKeyState(VK_ALT) And &H8000) <> 0 AndAlso (GetAsyncKeyState(VK_CAPITAL) And &H8000) <> 0 Then


            If Not isKeyPressed Then ' ตรวจสอบว่าปุ่มไม่ถูกกดอยู่
                isFunctionActive = Not isFunctionActive ' สลับสถานะฟังก์ชัน
                If isFunctionActive Then
                    If Base.Opacity = 0 Then
                        Timer1.Stop()
                        Timer2.Start()
                        Bg.Timer1.Stop()
                        Bg.Timer2.Start()
                    Else
                        Timer1.Stop()
                        Timer2.Start()
                    End If
                Else
                    If Base.Opacity = 0 Then
                        Timer1.Start()
                        Timer2.Stop()
                        Bg.Timer1.Start()
                        Bg.Timer2.Stop()
                    Else
                        Timer1.Start()
                        Timer2.Stop()
                    End If

                End If
                isKeyPressed = True ' ตั้งค่าปุ่มให้ถูกกดอยู่
            End If
        Else
            isKeyPressed = False ' รีเซ็ตสถานะเมื่อปุ่มถูกปล่อย
        End If
    End Sub
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        HideFromAltTab()
        If Opacity = 0 Then
            Me.Hide()
            Timer1.Stop()
        Else
            Opacity -= 0.1 ' ลดความโปร่งใสลงทีละ 0.05
        End If
    End Sub
    Private Sub Timer2_Tick(sender As Object, e As EventArgs) Handles Timer2.Tick
        HideFromAltTab()
        If Opacity = 1 Then
            Timer2.Stop()
        Else
            Me.Opacity += 0.1 ' เพิ่มความโปร่งใสทีละ 0.05
            Me.Show()
        End If

    End Sub

    Private Sub www_Load(sender As Object, e As EventArgs) Handles Me.Load
        HideFromAltTab()
        key.Start()
        key.Interval = 1
    End Sub

    Private Sub urlTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles url.KeyDown
        ' ตรวจสอบถ้าผู้ใช้กด Enter
        If e.KeyCode = Keys.Enter Then
            ' โหลด URL จาก TextBox ลงใน WebView2
            Try
                WebView21.Source = New Uri(url.Text)
            Catch ex As UriFormatException
                MessageBox.Show("URL Erorr")
            End Try
        End If
    End Sub
End Class