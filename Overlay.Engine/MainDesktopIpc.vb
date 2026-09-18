Imports System
Imports System.IO
Imports System.IO.Pipes
Imports System.Security.Principal
Imports System.Text.Json
Imports System.Threading

''' <summary>
''' Small authenticated local control channel between the Main supervisor and
''' its Desktop child. The pipe is intentionally line-delimited JSON so a
''' crashed child cannot leave a partially written binary protocol behind.
''' </summary>
Public NotInheritable Class MainDesktopIpc
    Private Sub New()
    End Sub

    Public Const PipePrefix As String = "NVIDIA_Share_Control_"

    Public Shared Function PipeName() As String
        Try
            Dim sid = WindowsIdentity.GetCurrent().User
            If sid IsNot Nothing Then Return PipePrefix & sid.Value.Replace("-", "_")
        Catch
        End Try
        Return PipePrefix & Environment.UserName
    End Function

    Public Shared Function CreateToken() As String
        Return Guid.NewGuid().ToString("N")
    End Function

    Public Shared Function Encode(command As String, token As String,
                                   Optional reason As String = Nothing,
                                   Optional pid As Integer = 0) As String
        Return JsonSerializer.Serialize(New With {
            .command = command,
            .token = token,
            .reason = reason,
            .pid = pid,
            .utc = DateTime.UtcNow.ToString("O")
        })
    End Function

    Public Shared Function IsAuthorized(line As String, token As String) As Boolean
        Try
            Using doc = JsonDocument.Parse(line)
                Dim supplied As String = Nothing
                Dim tokenElement As JsonElement
                If Not doc.RootElement.TryGetProperty("token", tokenElement) Then Return False
                If tokenElement.ValueKind <> JsonValueKind.String Then Return False
                supplied = tokenElement.GetString()
                Return String.Equals(supplied, token, StringComparison.Ordinal)
            End Using
        Catch
            Return False
        End Try
    End Function

    Public Shared Function ReadCommand(line As String) As String
        Try
            Using doc = JsonDocument.Parse(line)
                Dim value As JsonElement
                If doc.RootElement.TryGetProperty("command", value) Then
                    Return value.GetString()
                End If
            End Using
        Catch
        End Try
        Return Nothing
    End Function

    Public Shared Function ReadReason(line As String) As String
        Try
            Using doc = JsonDocument.Parse(line)
                Dim value As JsonElement
                If doc.RootElement.TryGetProperty("reason", value) AndAlso
                   value.ValueKind = JsonValueKind.String Then
                    Return value.GetString()
                End If
            End Using
        Catch
        End Try
        Return Nothing
    End Function

    Public Shared Sub RunServer(token As String, stopping As Func(Of Boolean),
                                received As Action(Of String, String))
        Dim thread As New Thread(
            Sub()
                While Not stopping()
                    Try
                        Using pipe As New NamedPipeServerStream(
                            PipeName(), PipeDirection.InOut, 1,
                            PipeTransmissionMode.Byte, PipeOptions.None)
                            pipe.WaitForConnection()
                            Using reader As New StreamReader(pipe)
                                While pipe.IsConnected AndAlso Not stopping()
                                    Dim line = reader.ReadLine()
                                    If line Is Nothing Then Exit While
                                    If IsAuthorized(line, token) Then
                                        received(ReadCommand(line), ReadReason(line))
                                    End If
                                End While
                            End Using
                        End Using
                    Catch
                        If Not stopping() Then Thread.Sleep(250)
                    End Try
                End While
            End Sub) With {.IsBackground = True, .Name = "MainDesktopIpcServer"}
        thread.Start()
    End Sub

    Public Shared Sub RunClient(token As String, stopping As Func(Of Boolean),
                                pid As Integer, status As Func(Of String))
        Dim thread As New Thread(
            Sub()
                While Not stopping()
                    Try
                        Using pipe As New NamedPipeClientStream(
                            ".", PipeName(), PipeDirection.Out, PipeOptions.None)
                            pipe.Connect(3000)
                            Using writer As New StreamWriter(pipe) With {.AutoFlush = True}
                                writer.WriteLine(Encode("hello", token, pid:=pid))
                                While Not stopping()
                                    writer.WriteLine(Encode("heartbeat", token,
                                        reason:=status(), pid:=pid))
                                    Thread.Sleep(2000)
                                End While
                            End Using
                        End Using
                    Catch
                        If Not stopping() Then Thread.Sleep(1000)
                    End Try
                End While
            End Sub) With {.IsBackground = True, .Name = "DesktopMainIpcClient"}
        thread.Start()
    End Sub
End Class
