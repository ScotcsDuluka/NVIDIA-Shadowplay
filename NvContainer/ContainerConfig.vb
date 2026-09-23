Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json

' Typed view of Config\NvContainer.json. Missing file -> built-in defaults
' + self-seed the file so the operator has an editing starting point.
Friend Class ContainerConfig

    Public Property Port As Integer = 5050
    Public Property SecurityCookie As String = "eb6eb0702ec25f9aeb0b0f8f79d06d5b"
    Public Property Workers As New List(Of WorkerSpec)

    Public Shared Function Load(cfgPath As String, baseDir As String) As ContainerConfig
        Dim cfg As New ContainerConfig()

        If Not File.Exists(cfgPath) Then
            ContainerLog.Log("config file missing — built-in defaults in effect")
            Try
                Dim dir As String = Path.GetDirectoryName(cfgPath)
                If dir <> "" Then Directory.CreateDirectory(dir)
                File.WriteAllText(cfgPath, DefaultJson())
                ContainerLog.Log("default config self-seeded: " & cfgPath)
            Catch ex As Exception
                ContainerLog.Log("config self-seed failed: " & ex.Message)
            End Try
            Return cfg
        End If

        Try
            Using doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(cfgPath))
                Dim root As JsonElement = doc.RootElement
                Dim prop As JsonElement
                Dim n As Integer
                Dim s As String

                If root.TryGetProperty("port", prop) AndAlso prop.TryGetInt32(n) Then
                    If n > 0 AndAlso n < 65536 Then cfg.Port = n
                End If

                If root.TryGetProperty("securityCookie", prop) AndAlso
                   prop.ValueKind = JsonValueKind.String Then
                    s = prop.GetString()
                    If s <> "" Then cfg.SecurityCookie = s
                End If

                If root.TryGetProperty("workers", prop) AndAlso
                   prop.ValueKind = JsonValueKind.Array Then

                    For Each w As JsonElement In prop.EnumerateArray()
                        Dim spec As New WorkerSpec()

                        If w.TryGetProperty("name", prop) AndAlso prop.ValueKind = JsonValueKind.String Then
                            spec.Name = prop.GetString()
                        End If
                        If w.TryGetProperty("exe", prop) AndAlso prop.ValueKind = JsonValueKind.String Then
                            s = prop.GetString()
                            spec.Exe = If(Path.IsPathRooted(s), s, Path.Combine(baseDir, s))
                        End If
                        If w.TryGetProperty("args", prop) AndAlso prop.ValueKind = JsonValueKind.String Then
                            spec.Args = prop.GetString()
                        End If
                        If w.TryGetProperty("workingDirectory", prop) AndAlso prop.ValueKind = JsonValueKind.String Then
                            s = prop.GetString()
                            If s <> "" Then
                                spec.WorkingDirectory = If(Path.IsPathRooted(s), s, Path.Combine(baseDir, s))
                            End If
                        End If
                        If w.TryGetProperty("enabled", prop) AndAlso prop.ValueKind = JsonValueKind.True OrElse
                           w.TryGetProperty("enabled", prop) AndAlso prop.ValueKind = JsonValueKind.False Then
                            spec.Enabled = prop.GetBoolean()
                        End If
                        If w.TryGetProperty("maxRestarts", prop) AndAlso prop.TryGetInt32(n) Then
                            spec.MaxRestarts = n
                        End If
                        If w.TryGetProperty("restartBackoffSeconds", prop) AndAlso prop.TryGetInt32(n) Then
                            spec.RestartBackoffSeconds = n
                        End If

                        If spec.Name <> "" AndAlso spec.Exe <> "" Then
                            cfg.Workers.Add(spec)
                        Else
                            ContainerLog.Log("config: dropped worker entry with missing name/exe")
                        End If
                    Next
                End If
            End Using
        Catch ex As Exception
            ContainerLog.Log("config parse failed — built-in defaults in effect: " & ex.Message)
            Return New ContainerConfig()
        End Try

        Return cfg
    End Function

    Private Shared Function DefaultJson() As String
        Return "{""port"": 5050," & Environment.NewLine &
               " ""securityCookie"": ""eb6eb0702ec25f9aeb0b0f8f79d06d5b""," & Environment.NewLine &
               " ""workers"": []" & Environment.NewLine &
               "}"
    End Function

End Class
