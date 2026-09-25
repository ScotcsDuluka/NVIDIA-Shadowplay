Imports System.IO
Imports System.Threading

Partial Public Class NVIDIA_Shadowplay_Helper

    Private tcp As TcpClientHelper

    Private Sub Base_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' ★ FIX (startup): the ctor no longer connects — do it in the background
        ' so the UI thread never blocks inside TcpClient.Connect.
        tcp = New TcpClientHelper("NVIDIA  APP")

        AddHandler tcp.OnMessageReceived, AddressOf OnMessage

        tcp.ConnectAsync()

        ' ★ FIX: removed dead code — "If tcp IsNot Nothing Then Exit Sub" always
        ' fired (a ctor either returns an object or throws), so the infinite
        ' Sleep(500) wait for the "NVIDIA API" process below never ran. Left in
        ' place it was a landmine: unguarding it would freeze the UI thread.
    End Sub

    Public Sub OnMessage(msg As String)
        If InvokeRequired Then
            ' ★ FIX: BeginInvoke (async) — blocking Invoke from the socket read
            ' thread stalled the reader behind whatever the UI was doing.
            If Not IsHandleCreated Then Return
            Try
                BeginInvoke(Sub() OnMessage(msg))
            Catch
                ' Form closing mid-dispatch — drop it.
            End Try
            Return
        End If

        If Not msg.Contains("|") Then Exit Sub

        Dim parts = msg.Split("|"c)
        If parts.Length < 2 Then Exit Sub ' ★ FIX: guard malformed frames
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

            Case Else
                Debug.WriteLine("Unknown: " & cmd)

        End Select
    End Sub

End Class