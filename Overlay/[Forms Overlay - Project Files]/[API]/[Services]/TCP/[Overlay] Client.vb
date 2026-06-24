Public Class Base

    Private tcp As TcpClientHelper

    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tcp = New TcpClientHelper("NVIDIA Overlay")

        AddHandler tcp.OnMessageReceived, AddressOf OnMessage
    End Sub

    ' แก้: Dispose TCP ตอน form ปิด
    Private Sub Base_TestFormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Try
            If tcp IsNot Nothing Then
                tcp.Disconnect()
                tcp.Dispose()
            End If
        Catch
        End Try

    End Sub

    Public Sub OnMessage(msg As String)
        If InvokeRequired Then
            Invoke(Sub() OnMessage(msg))
            Return
        End If

        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
        If parts.Length < 2 Then Exit Sub

        Dim data = parts(1)

        Dim colonIndex = data.IndexOf(":"c)
        Dim cmd, value As String
        If colonIndex >= 0 Then
            cmd = data.Substring(0, colonIndex)
            value = data.Substring(colonIndex + 1)
        Else
            cmd = data
            value = ""
        End If

        Select Case cmd

            Case "open_overlay"
                tcp.SendLog(cmd)
                If Settings_List.Visible Then Return
                If Base_Gallery.Visible Then Return

                isFunctionActive_f3 = False

                If shadowplay.Visible = True Then
                    HideAllControls()
                    shadowplay.Visible = False
                Else
                    ShowMainPanel()
                    shadowplay.Visible = True
                    Base_Game_Filter_Sub.Opacity = 0
                    Base_Game_Filter.Opacity = 0
                    Base_Game_Filter.Hide()
                    Base_Game_Filter_Sub.Hide()
                End If

            Case Else
                Debug.WriteLine("Unknown: " & cmd)

        End Select
    End Sub
End Class