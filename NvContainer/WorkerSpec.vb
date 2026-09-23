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

End Class
