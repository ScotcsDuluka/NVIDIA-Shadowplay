Imports System
Imports System.IO
Imports System.Text

Friend Module ShimLog
    Private ReadOnly Sync As New Object()
    Private _path As String = ""

    Public Sub Init(path As String)
        SyncLock Sync
            _path = path
            Try
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path))
                File.AppendAllText(path, "==== NvShim session " &
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & " ====" & Environment.NewLine,
                                    Encoding.UTF8)
            Catch
            End Try
        End SyncLock
    End Sub

    Public Sub Log(message As String)
        Dim line As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "] " & message
        SyncLock Sync
            Console.WriteLine(line)
            If _path <> "" Then
                Try
                    File.AppendAllText(_path, line & Environment.NewLine, Encoding.UTF8)
                Catch
                End Try
            End If
        End SyncLock
    End Sub
End Module
