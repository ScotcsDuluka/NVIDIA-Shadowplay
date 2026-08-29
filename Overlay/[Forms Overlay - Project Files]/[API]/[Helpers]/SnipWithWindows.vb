Imports System.IO

Partial Public Class Base

    Private OverlayOpen As Boolean = False

    Private Async Sub SnipWithWindows()
        OverlayOpen = (Me.Opacity <> 0)
        HideAllControls()
        For Each s In shas
            If s IsNot Nothing Then s.Hide()
        Next
        Await Task.Delay(250)

        _snipPending = True
        AddClipboardFormatListener(Me.Handle)
        HookKeyboard()
        Process.Start(New ProcessStartInfo("ms-screenclip:") With {.UseShellExecute = True})
    End Sub

    Private Sub SnipFinished()
        _snipPending = False
        RemoveClipboardFormatListener(Me.Handle)
        UnhookKeyboard()

        If Not Clipboard.ContainsImage() Then Return

        Dim filePath As String = Base_Gallery.txtFilePath.Text
        Using img As Image = Clipboard.GetImage()
            Dim fileName As String = Path.Combine(filePath,
            "Shadowplay Screenshot " & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".png")
            img.Save(fileName, System.Drawing.Imaging.ImageFormat.Png)
        End Using

        ShowNotifier("notificationScreenshotSavedToGallery")
        If OverlayOpen = True Then
            ShowMainPanel()
        End If
    End Sub

End Class
