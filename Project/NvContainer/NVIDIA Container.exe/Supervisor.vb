Imports System
Imports System.Collections.Generic

' Owns every configured worker process. Spawn on boot, restart policy with
' backoff, manual start/stop/restart from the TCP authority, aggregate status.
Friend Class Supervisor

    Private ReadOnly _workers As New Dictionary(Of String, WorkerProcess)

    Public Sub New(config As ContainerConfig)
        For Each spec As WorkerSpec In config.Workers
            If Not _workers.ContainsKey(spec.Name) Then
                _workers.Add(spec.Name, New WorkerProcess(spec))
            Else
                ContainerLog.Log("config: duplicate worker name '" & spec.Name & "' — first entry wins")
            End If
        Next
    End Sub

    Public Function Count() As Integer
        Return _workers.Count
    End Function

    Public Sub StartAll()
        For Each w As WorkerProcess In _workers.Values
            If w.Spec.Enabled Then
                w.Start("boot")
            Else
                ContainerLog.Log("worker '" & w.Name & "' disabled by config — not started")
            End If
        Next
    End Sub

    Public Sub StopAll()
        For Each w As WorkerProcess In _workers.Values
            w.Shutdown()
        Next
    End Sub

    Public Function Start(name As String) As Boolean
        Dim w As WorkerProcess = Find(name)
        If w Is Nothing Then Return False
        w.Start("authority")
        Return True
    End Function

    Public Function StopWorker(name As String) As Boolean
        Dim w As WorkerProcess = Find(name)
        If w Is Nothing Then Return False
        w.Shutdown()
        Return True
    End Function

    Public Function Restart(name As String) As Boolean
        Dim w As WorkerProcess = Find(name)
        If w Is Nothing Then Return False
        w.Shutdown()
        w.Start("authority-restart")
        Return True
    End Function

    Public Function HasWorker(name As String) As Boolean
        Return _workers.ContainsKey(name)
    End Function

    Public Function StatusList() As List(Of Dictionary(Of String, Object))
        Dim list As New List(Of Dictionary(Of String, Object))
        For Each w As WorkerProcess In _workers.Values
            list.Add(w.Status())
        Next
        Return list
    End Function

    Private Function Find(name As String) As WorkerProcess
        Dim w As WorkerProcess = Nothing
        If _workers.TryGetValue(name, w) Then Return w
        Return Nothing
    End Function

End Class
