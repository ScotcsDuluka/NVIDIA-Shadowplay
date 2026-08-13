Public Class Base

    ''' <summary>Shared TCP helper — accessible from all forms (Sub_Record, Video Capture, etc.)</summary>
    Public Shared tcp As TcpClientHelper

    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tcp = New TcpClientHelper("NVIDIA Overlay")

        AddHandler tcp.OnMessageReceived, AddressOf OnMessage
    End Sub

    ' Dispose TCP on form close
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

            Case "engine_ready"
                ' ✅ P1.6: Engine just connected to the Hub (or reconnected).
                ' Re-send PREWARM_FFMPEG so Engine picks up our ffmpeg.exe path.
                ' Problem: Overlay sends PREWARM on its Load event, but Engine
                ' often connects LATER → the original PREWARM is lost. Engine
                ' then can't find ffmpeg → RECORD_START fails. By re-sending
                ' PREWARM here, we ensure Engine always gets the path.
                Debug.WriteLine("[Overlay] received engine_ready, re-sending PREWARM_FFMPEG")
                Try
                    Dim ffmpegPath As String = AppSettings.Instance.Paths.FFmpegPath
                    If Not String.IsNullOrEmpty(ffmpegPath) Then
                        Dim encoderName As String = AppSettings.Instance.Recording.Encoder
                        tcp.Send("PREWARM_FFMPEG", ffmpegPath & "|" & encoderName)
                    End If
                Catch ex As Exception
                    Debug.WriteLine("[Overlay] engine_ready re-send failed: " & ex.Message)
                End Try

            Case "engine_response"
                ' ✅ P1.6: log Engine responses for debugging. Format:
                '   engine_response:<command>,<status>[,<data>][,req=<reqId>]
                Debug.WriteLine($"[Overlay] engine_response: {value}")

            Case Else
                Debug.WriteLine("Unknown: " & cmd)

        End Select
    End Sub
End Class
