Partial Public Class Load

    Private tcp As TcpClientHelper

    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        tcp = New TcpClientHelper("NVIDIA Notifier")

        AddHandler tcp.OnMessageReceived, AddressOf OnMessage
    End Sub

    Public Sub OnMessage(msg As String)
        If InvokeRequired Then
            Invoke(Sub() OnMessage(msg))
            Return
        End If

        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
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

            Case "notifier_show"

            Case Else
                Debug.WriteLine("Unknown: " & cmd)

        End Select
    End Sub
End Class