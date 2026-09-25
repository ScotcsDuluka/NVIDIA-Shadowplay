Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text.Json

Friend NotInheritable Class ShimRole
    Public Property Name As String = ""
    Public Property ExePath As String = ""
    Public Property Args As String = ""
End Class

Friend NotInheritable Class ShimConfig
    Private ReadOnly _roles As New Dictionary(Of String, ShimRole)(StringComparer.OrdinalIgnoreCase)

    Public Shared Function Load(path As String, baseDir As String) As ShimConfig
        Dim config As New ShimConfig()
        If Not File.Exists(path) Then
            config.SeedDefaults()
            Try
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path))
                File.WriteAllText(path, config.DefaultJson())
                ShimLog.Log("default config self-seeded: " & path)
            Catch ex As Exception
                ShimLog.Log("config self-seed failed: " & ex.Message)
            End Try
            config.ResolvePaths(baseDir)
            Return config
        End If

        Try
            Using doc As JsonDocument = JsonDocument.Parse(File.ReadAllText(path))
                Dim roles As JsonElement
                If doc.RootElement.TryGetProperty("roles", roles) AndAlso roles.ValueKind = JsonValueKind.Object Then
                    For Each propertyItem As JsonProperty In roles.EnumerateObject()
                        Dim spec As New ShimRole With {.Name = propertyItem.Name}
                        Dim exe As JsonElement
                        Dim arg As JsonElement
                        If propertyItem.Value.TryGetProperty("exe", exe) AndAlso exe.ValueKind = JsonValueKind.String Then
                            spec.ExePath = exe.GetString()
                        End If
                        If propertyItem.Value.TryGetProperty("args", arg) AndAlso arg.ValueKind = JsonValueKind.String Then
                            spec.Args = arg.GetString()
                        End If
                        If spec.ExePath <> "" Then config._roles(spec.Name) = spec
                    Next
                End If
            End Using
        Catch ex As Exception
            ShimLog.Log("config parse failed: " & ex.Message)
        End Try

        If config._roles.Count = 0 Then config.SeedDefaults()
        config.ResolvePaths(baseDir)
        Return config
    End Function

    Public Function FindRole(name As String) As ShimRole
        Dim role As ShimRole = Nothing
        If _roles.TryGetValue(name, role) Then Return role
        Return Nothing
    End Function

    Public Function RoleNames() As IEnumerable(Of String)
        Return _roles.Keys
    End Function

    Private Sub SeedDefaults()
        _roles.Clear()
        Add("coordinator", "Coordinator\NVIDIA Share.exe")
        Add("winform", "WinForm\NVIDIA ShadowPlay.exe")
        Add("webview", "WebView\NVIDIA Share.exe")
        Add("hook", "Hook\NVIDIA Share.exe")
    End Sub

    Private Sub Add(name As String, exe As String)
        _roles(name) = New ShimRole With {.Name = name, .ExePath = exe, .Args = ""}
    End Sub

    Private Sub ResolvePaths(baseDir As String)
        For Each role As ShimRole In _roles.Values
            If Not Path.IsPathRooted(role.ExePath) Then role.ExePath = Path.Combine(baseDir, role.ExePath)
        Next
    End Sub

    Private Function DefaultJson() As String
        Return "{""roles"": {" & Environment.NewLine &
               "  ""coordinator"": { ""exe"": ""Coordinator\\NVIDIA Share.exe"", ""args"": """" }," & Environment.NewLine &
               "  ""winform"": { ""exe"": ""WinForm\\NVIDIA Share.exe"", ""args"": """" }," & Environment.NewLine &
               "  ""webview"": { ""exe"": ""WebView\\NVIDIA Share.exe"", ""args"": """" }," & Environment.NewLine &
               "  ""hook"": { ""exe"": ""Hook\\NVIDIA Share.exe"", ""args"": """" }" & Environment.NewLine &
               "}}" & Environment.NewLine
    End Function
End Class
