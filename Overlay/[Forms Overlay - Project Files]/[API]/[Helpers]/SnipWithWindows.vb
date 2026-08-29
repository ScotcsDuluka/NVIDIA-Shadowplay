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
        _snipSessionId += 1   ' รหัสเซสชัน snip ครั้งนี้ (ใช้กัน kill ทับเซสชันใหม่)
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

        ' ภาพลงไฟล์ปลอดภัยแล้ว → เก็บโปรเซส Snipping Tool ทิ้ง
        ' (Windows ไม่ปิดมันเองหลัง snip → ค้างใน Task Manager)
        KillSnipHostAfterDelay(_snipSessionId)

        ShowNotifier("notificationScreenshotSavedToGallery")
        If OverlayOpen = True Then
            ShowMainPanel()
        End If
    End Sub

    ' ─── Snip host cleanup ───────────────────────────────────────────────
    ' ms-screenclip: เปิด Snipping Tool ของ Windows (Win11: SnippingTool.exe,
    ' Win10: ScreenSketch.exe / ScreenClippingHost.exe) ซึ่งเป็น packaged app
    ' ที่ Windows "ไม่ปิดโปรเซสเอง" หลัง snip จบ → ค้างใน Task Manager
    ' เราจึงต้องปิดเองเมื่อเซสชันจบ (snip สำเร็จ / ESC / ปิดแอป)
    Private _snipSessionId As Integer = 0

    ''' <summary>เรียกหลังเซสชัน snip จบแล้วเท่านั้น บน background thread —
    ''' ใช้ session-id กัน kill ทับ snip ที่ผู้ใช้เปิดใหม่ภายในเสี้ยววินาที</summary>
    Private Sub KillSnipHostAfterDelay(sessionId As Integer)
        Task.Run(Async Function()
                     Await Task.Delay(1000) ' ให้ host เขียน clipboard + fade-out จบก่อน
                     If sessionId = _snipSessionId AndAlso Not _snipPending Then
                         KillSnipHostProcesses()
                     End If
                 End Function)
    End Sub

    ''' <summary>ปิดโปรเซส host ของ Windows Snipping Tool ทั้งหมด (ไม่ block caller นาน)</summary>
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
                    ' ปิดไม่สำเร็จ (มันปิดไปเองแล้ว / access denied) → ข้าม
                Finally
                    p.Dispose()
                End Try
            Next
        Next
    End Sub

End Class
