' ==============================================================================
' Base_Gallery.vb - Gallery Form for NVIDIA Shadowplay Application
' Organized and restructured for better maintainability
' ==============================================================================

' --- IMPORTS (Organized by namespace hierarchy) ---
Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices

Public Class Base_Gallery

#Region "============================================================================ NATIVE METHODS"

    ' Window Style Constants
    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80      ' ToolWindow style (hidden from Alt+Tab)
    Private Const WS_EX_APPWINDOW As Integer = &H40000    ' Show in Task Switcher

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub

#End Region

#Region "============================================================================ FIELDS"

    ' Context Menu for Image Operations
    Private WithEvents contextMenu As New ContextMenuStrip()
    Private currentImagePath As String = ""

    ' Image Display Settings
    Private Const ThumbnailWidth As Integer = 225
    Private Const ThumbnailHeight As Integer = 155
    Private Const SettingsPanelWidth As Integer = 1010
    Private Const SettingsPanelHeight As Integer = 600

    ' Supported Image Extensions
    Private ReadOnly SupportedImageExtensions As String() = {".jpg", ".jpeg", ".png", ".bmp"}

#End Region

#Region "============================================================================ FORM INITIALIZATION"

    Public Sub InitForm()
        ' Reserved for future initialization logic
    End Sub

    Private Async Sub Gallery_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        InitializeUI()
        Await Task.Delay(1000)
        LoadImagesFromPath()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        FlowLayoutPanel1.AutoScroll = True
    End Sub

    Private Sub InitializeUI()
        Base.Main_menu_list.Visible = False
        settings_1.Size = New Size(SettingsPanelWidth, SettingsPanelHeight)
    End Sub

#End Region

#Region "============================================================================ IMAGE LOADING"
    Private Function CropTo16by9(img As Image) As Image
        Dim targetWidth As Integer = img.Width
        Dim targetHeight As Integer = CInt(targetWidth * 9 / 16)

        If targetHeight > img.Height Then
            targetHeight = img.Height
            targetWidth = CInt(targetHeight * 16 / 9)
        End If

        Dim x As Integer = (img.Width - targetWidth) \ 2
        Dim y As Integer = (img.Height - targetHeight) \ 2

        Dim bmp As New Bitmap(targetWidth, targetHeight)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.DrawImage(img, 0, 0, New Rectangle(x, y, targetWidth, targetHeight), GraphicsUnit.Pixel)
        End Using
        Return bmp
    End Function
    Private Sub LoadImagesFromPath()
        Dim folderPath As String = txtFilePath.Text

        If Directory.Exists(folderPath) Then
            LoadImages(folderPath)
        Else
            ShowNotifier("Please select a valid save path for capture.", "", False)
        End If
    End Sub
    Private Function GetImageFiles(folderPath As String) As IEnumerable(Of String)
        Return Directory.GetFiles(folderPath, "*.*").Where(Function(f)
                                                               Dim extension As String = Path.GetExtension(f).ToLower()
                                                               Return SupportedImageExtensions.Contains(extension)
                                                           End Function)
    End Function
    Private Sub LoadImages(folderPath As String)
        FlowLayoutPanel1.Controls.Clear()

        Try
            Dim imageFiles As IEnumerable(Of String) = GetImageFiles(folderPath)

            ' โหลดภาพแบบ async
            Task.Run(Sub()
                         For Each file As String In imageFiles
                             Try
                                 ' โหลดรูปจากไฟล์
                                 Using fs As New FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)
                                     Using img As New Bitmap(fs)
                                         Dim croppedImg As Image = CropTo16by9(img)

                                         ' สร้าง PictureBox
                                         Dim picBox As New PictureBox() With {
                                         .Image = New Bitmap(croppedImg),
                                         .SizeMode = PictureBoxSizeMode.Zoom,
                                         .Width = ThumbnailWidth,
                                         .Height = ThumbnailHeight,
                                         .BorderStyle = BorderStyle.FixedSingle
                                     }

                                         ' เพิ่มเข้า FlowLayoutPanel ใน UI thread
                                         If FlowLayoutPanel1.InvokeRequired Then
                                             FlowLayoutPanel1.Invoke(Sub() FlowLayoutPanel1.Controls.Add(picBox))
                                         Else
                                             FlowLayoutPanel1.Controls.Add(picBox)
                                         End If
                                     End Using
                                 End Using
                             Catch ex As Exception
                                 Debug.WriteLine("Error loading image: " & ex.Message)
                             End Try
                         Next
                     End Sub)

        Catch ex As Exception
            MessageBox.Show("ไม่สามารถดึงข้อมูลรูปภาพได้: " & ex.Message)
        End Try
    End Sub

    Private Async Sub AddImageThumbnail(filePath As String)
        Dim bmp As Bitmap = Nothing

        Try
            ' โหลดรูปแบบ async โดยใช้ MemoryStream
            Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                bmp = New Bitmap(fs)
            End Using
        Catch ex As Exception
            Debug.WriteLine("Error loading image: " & ex.Message)
            Return
        End Try

        ' สร้าง PictureBox
        Dim picBox As New PictureBox() With {
        .Image = bmp,
        .SizeMode = PictureBoxSizeMode.Zoom,
        .Width = ThumbnailWidth,
        .Height = ThumbnailHeight,
        .BorderStyle = BorderStyle.FixedSingle
    }

        ' เพิ่มเข้า FlowLayoutPanel ใน UI thread
        If FlowLayoutPanel1.InvokeRequired Then
            FlowLayoutPanel1.Invoke(Sub() FlowLayoutPanel1.Controls.Add(picBox))
        Else
            FlowLayoutPanel1.Controls.Add(picBox)
        End If
    End Sub

#End Region

#Region "============================================================================ NOTIFIER SYSTEM"

    Private Sub ShowNotifier(message As String, icon As String, Optional isValidPath As Boolean = True)
        Base_Notifier.Show()

        With Base_Notifier
            .icon_n.Font = New Font(.icon_n.Font.FontFamily, If(isValidPath, 50, 40))
            .icon_n.ForeColor = Color.White
            .icon_n.Text = icon
            .text_n.Text = message
        End With
    End Sub

#End Region

#Region "============================================================================ FOLDER OPERATIONS"

    Private Sub HandleCaptureFolder()
        Dim folderPath As String = txtFilePath.Text.Trim()

        If Directory.Exists(folderPath) Then
            ShowNotifier("Location capture has been saved", "", False)
            CloseGalleryAndReturnToBase()
        Else
            ShowNotifier("Please select a valid save path for capture.", "", False)
        End If
    End Sub

    Private Sub CloseGalleryAndReturnToBase()
        WindowState = FormWindowState.Minimized
        Opacity = 0
        Hide()
        Base.Show()
        Base.Main_menu_list.Visible = True
        Base.ALT_Z.Start()
    End Sub

    Private Sub OpenFolderInExplorer(folderPath As String)
        If Directory.Exists(folderPath) Then
            Process.Start("explorer.exe", folderPath)
            HandleCaptureFolder()
            Base_Notifier.text_n.Text = "Folders open : " & folderPath
        Else
            ShowNotifier("Please select a valid save path for capture.", "", False)
        End If
    End Sub

#End Region



#Region "============================================================================ EVENT HANDLERS - GALLERY ACTIONS"

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles Saved_l10n.Click
        HandleCaptureFolder()
    End Sub

    Private Sub bg_fn_Click(sender As Object, e As EventArgs) Handles bg_fn.Click
        HandleCaptureFolder()
    End Sub

    Private Sub Label2_Click(sender As Object, e As EventArgs) Handles Openloaction_l10n.Click
        OpenFolderInExplorer(txtFilePath.Text.Trim())
    End Sub

    Private Sub PictureBox5_Click(sender As Object, e As EventArgs) Handles PictureBox5.Click
        OpenFolderInExplorer(txtFilePath.Text.Trim())
    End Sub

#End Region

#Region "============================================================================ EVENT HANDLERS - RELOAD IMAGES"

    Private Sub Label6_Click(sender As Object, e As EventArgs) Handles Load_l10n.Click
        ReloadImages()
    End Sub

    Private Sub PictureBox3_Click(sender As Object, e As EventArgs) Handles PictureBox3.Click
        ReloadImages()
    End Sub

    Private Sub ReloadImages()
        Dim folderPath As String = txtFilePath.Text

        If Directory.Exists(folderPath) Then
            LoadImages(folderPath)
        Else
            MessageBox.Show("ไม่พบโฟลเดอร์นี้ กรุณาตรวจสอบ Path อีกครั้ง")
        End If
    End Sub

    Private Sub LoactionSaved_l10n_Click(sender As Object, e As EventArgs) Handles LoactionSaved_l10n.Click

    End Sub


#End Region

End Class