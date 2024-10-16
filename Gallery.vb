Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Gallery_1
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

    Private Sub Gallery_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        Base.action.Visible = False
        settings_1.Size = New Size(1010, 600) ' ขนาดที่ต้องการAlignPanelToTop()


        ' ดึง Path จาก TextBox (txtFilePath)
        Dim folderPath As String = txtFilePath.Text

        ' ตรวจสอบว่ามี Path นั้นหรือไม่
        If Directory.Exists(folderPath) Then
            LoadImages(folderPath)
        Else
            ShowNotifier("Please select a valid save path for capture.", "", False)
        End If








    End Sub
    ' สร้าง ContextMenuStrip
    Private WithEvents contextMenu As New ContextMenuStrip()
    Private currentImagePath As String = ""
    Private Sub LoadImages(ByVal folderPath As String)
        FlowLayoutPanel1.Controls.Clear() ' ล้าง Control ใน FlowLayoutPanel

        Try
            ' ดึงไฟล์ทั้งหมดในโฟลเดอร์ที่เลือก
            Dim files() As String = Directory.GetFiles(folderPath, "*.*").Where(Function(f) f.EndsWith(".jpg") Or f.EndsWith(".png") Or f.EndsWith(".bmp")).ToArray()

            ' แสดงแต่ละไฟล์ใน FlowLayoutPanel
            For Each file As String In files
                Dim picBox As New PictureBox()
                picBox.Image = Image.FromFile(file) ' โหลดรูปภาพ
                picBox.SizeMode = PictureBoxSizeMode.Zoom ' ตั้งค่าการแสดงผลให้ย่อโดยรักษาสัดส่วน
                picBox.Width = 225 ' กำหนดความกว้างสูงสุด
                picBox.Height = 155 ' กำหนดความสูงสูงสุด
                picBox.BorderStyle = BorderStyle.FixedSingle ' กำหนดเส้นขอบ


                FlowLayoutPanel1.Controls.Add(picBox) ' เพิ่ม PictureBox ลงใน FlowLayoutPanel
            Next
        Catch ex As Exception
            MessageBox.Show("ไม่สามารถดึงข้อมูลรูปภาพได้: " & ex.Message)
        End Try
    End Sub
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' ตั้งค่า FlowLayoutPanel
        FlowLayoutPanel1.AutoScroll = True ' เปิดการเลื่อน
    End Sub
























    Private Sub save_sc_Click(sender As Object, e As EventArgs) Handles save_sc.Click
        Dim folderDlg As New FolderBrowserDialog With {
            .Description = "Select the folder to save the capture."
        }

        If folderDlg.ShowDialog() = DialogResult.OK Then
            txtFilePath.Text = folderDlg.SelectedPath
            My.Settings.SavePath = txtFilePath.Text
            My.Settings.Save() ' บันทึกค่า Settings
        End If
    End Sub

    Private Sub ShowNotifier(message As String, icon As String, Optional isValidPath As Boolean = True)
        Notifier.Show()
        Notifier.icon_n.Font = New Font(Notifier.icon_n.Font.FontFamily, If(isValidPath, 50, 40))
        Notifier.icon_n.ForeColor = Color.White
        Notifier.icon_n.Text = icon
        Notifier.text_n.Text = message
    End Sub

    Private Sub HandleCaptureFolder()
        Dim folderPath As String = txtFilePath.Text.Trim()

        If Directory.Exists(folderPath) Then
            ShowNotifier("Location capture has been saved", "", False)
            WindowState = FormWindowState.Minimized
            Opacity = 0
            Hide()
            Base.Show()
            Base.action.Visible = True
            Base.alt_z.Start()
        Else
            ShowNotifier("Please select a valid save path for capture.", "", False)
        End If
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click

        HandleCaptureFolder()
    End Sub

    Private Sub bg_fn_Click(sender As Object, e As EventArgs) Handles bg_fn.Click

        HandleCaptureFolder()
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Label2.Click
        Dim folderPath = txtFilePath.Text.Trim
        If Directory.Exists(folderPath) Then
            Process.Start("explorer.exe", folderPath)
            HandleCaptureFolder()
            Notifier.text_n.Text = "Folders open : " & folderPath
        Else
            ShowNotifier("Please select a valid save path for capture.", "", False)
        End If
    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click
        Label2_Click(sender, e) ' Reuse the same logic
    End Sub

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles Label6.Click
        ' ดึง Path จาก TextBox (txtFilePath)
        Dim folderPath As String = txtFilePath.Text

        ' ตรวจสอบว่ามี Path นั้นหรือไม่
        If Directory.Exists(folderPath) Then
            LoadImages(folderPath)
        Else
            MessageBox.Show("ไม่พบโฟลเดอร์นี้ กรุณาตรวจสอบ Path อีกครั้ง")
        End If
    End Sub

    Private Sub PictureBox3_Click(sender As Object, e As EventArgs) Handles PictureBox3.Click
        ' ดึง Path จาก TextBox (txtFilePath)
        Dim folderPath As String = txtFilePath.Text

        ' ตรวจสอบว่ามี Path นั้นหรือไม่
        If Directory.Exists(folderPath) Then
            LoadImages(folderPath)
        Else
            MessageBox.Show("ไม่พบโฟลเดอร์นี้ กรุณาตรวจสอบ Path อีกครั้ง")
        End If
    End Sub

End Class
