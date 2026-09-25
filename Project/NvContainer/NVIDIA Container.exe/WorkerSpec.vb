Imports System

' One supervised process entry from the config file.
Friend Class WorkerSpec

    Public Property Name As String = ""
    Public Property Exe As String = ""
    Public Property Args As String = ""
    Public Property WorkingDirectory As String = ""
    Public Property Enabled As Boolean = True
    Public Property MaxRestarts As Integer = 10
    Public Property RestartBackoffSeconds As Integer = 5
    ' When true, Start() first looks for an already-running instance of Exe
    ' (matched by full image path) and adopts it instead of spawning a
    ' duplicate — the ownership handover path (task-spawned -> container-owned).
    Public Property AdoptExisting As Boolean = True

End Class
