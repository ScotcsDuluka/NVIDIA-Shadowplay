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
        _snipSessionId += 1   
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

        KillSnipHostAfterDelay(_snipSessionId)

        ShowNotifier("notificationScreenshotSavedToGallery")
        If OverlayOpen = True Then
            ShowMainPanel()
        End If
    End Sub

    Private _snipSessionId As Integer = 0

    Private Sub KillSnipHostAfterDelay(sessionId As Integer)
        Task.Run(Async Function()
                     Await Task.Delay(1000) 
                     If sessionId = _snipSessionId AndAlso Not _snipPending Then
                         KillSnipHostProcesses()
                     End If
                 End Function)
    End Sub

    Private Sub KillSnipHostProcesses()
        Dim hostNames As String() = {"SnippingTool", "ScreenClippingHost", "ScreenSketch"}

        For Each hostName As String In hostNames
            Dim hosts As Process() = Process.GetProcessesByName(hostName)
            For Each p As Process In hosts
                Try
                    If Not p.HasExited Then
                        p.Kill(entireProcessTree:=True)
                        p.WaitForExit(2000)
                    End If
                Catch
                    
                Finally
                    p.Dispose()
                End Try
            Next
        Next
    End Sub

End Class
