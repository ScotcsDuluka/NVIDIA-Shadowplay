Imports System
Imports System.IO
Imports System.Text

' Console + file logger. File: Logs\NvContainer.log beside the exe.
Friend Module ContainerLog

    Private ReadOnly _lock As New Object()
    Private _path As String = ""

    Public Sub Init(path As String)
        SyncLock _lock
            _path = path
            Try
                File.AppendAllText(_path,
                    "==== NvContainer session " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & " ====" & vbCrLf,
                    Encoding.UTF8)
            Catch
            End Try
        End SyncLock
    End Sub

    Public Sub Log(msg As String)
        Dim line As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "] " & msg
        SyncLock _lock
            Console.WriteLine(line)
            If _path <> "" Then
                Try
                    File.AppendAllText(_path, line & vbCrLf, Encoding.UTF8)
                Catch
                End Try
            End If
        End SyncLock
    End Sub

End Module
