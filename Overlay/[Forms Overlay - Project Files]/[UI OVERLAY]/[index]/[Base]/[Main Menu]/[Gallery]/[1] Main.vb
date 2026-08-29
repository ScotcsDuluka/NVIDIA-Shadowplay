Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports Notifier_API

Public Class Base_Gallery
    Inherits System.Windows.Forms.Form

    Public Sub InitForm()
    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
    End Sub

    Private Sub Gallery_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        Base.AlignPanelToTop()
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles Saved_l10n.Click, bg_fn.Click
        Hide()
        Base.IF_OpenShare = True
        Base.ShowMainPanel()
        Base.shadowplay.Visible = True
    End Sub

    Private Sub Openloaction_l10n_Click(sender As Object, e As EventArgs) Handles Openloaction_l10n.Click
          Base.IF_OpenShare = True
        If Directory.Exists(txtFilePath.Text) Then
            Process.Start("explorer.exe", txtFilePath.Text)
            Hide()
            Base.HideAllControls()
            Base.IF_OpenShare = True
        Else
            MessageBox.Show("foldererror")
        End If
    End Sub

    Private Sub save_sc_Click(sender As Object, e As EventArgs) Handles save_sc.Click
        Dim folderDlg As New FolderBrowserDialog With {
        .Description = "Select the folder to save the capture."
    }

        If folderDlg.ShowDialog = DialogResult.OK Then
            txtFilePath.Text = folderDlg.SelectedPath

            AppSettings.Instance.Paths.GalleryPath = txtFilePath.Text
            AppSettings.Instance.Save()
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        settings_1.Location = New Point((Me.ClientSize.Width - settings_1.Width) / 2, 160)
    End Sub
End Class