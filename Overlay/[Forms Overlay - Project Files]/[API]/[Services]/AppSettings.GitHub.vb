



Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text.Json

Partial Public Class AppSettings
#Region "GitHub Account (login / avatar)"
    
    
    
    Public Sub SaveGitHubUser(username As String, avatarUrl As String, token As String)
        GitHubUser.Username = username
        GitHubUser.AvatarUrl = avatarUrl
        GitHubUser.IsLoggedIn = True
        GitHubUser.LastLogin = DateTime.Now
        GitHubToken = token
        Save()
        Debug.WriteLine($"SaveGitHubUser: Saved user '{username}' to config.json")
    End Sub

    
    
    
    Public Sub ClearGitHubUser()
        GitHubUser.Username = ""
        GitHubUser.AvatarUrl = ""
        GitHubUser.IsLoggedIn = False
        GitHubUser.LastLogin = DateTime.MinValue
        GitHubToken = ""
        Save()
        Debug.WriteLine("ClearGitHubUser: User logged out")
    End Sub

    
    
    
    Public Async Function LoadGitHubUser() As Task
        Try
            If String.IsNullOrEmpty(GitHubToken) Then
                Debug.WriteLine("LoadGitHubUser: No token, skipping")
                Return
            End If

            
            Using ping As New Net.NetworkInformation.Ping()
                Dim reply = Await ping.SendPingAsync("api.github.com", 2000)
                If reply.Status <> Net.NetworkInformation.IPStatus.Success Then
                    Debug.WriteLine("LoadGitHubUser: No internet connection")
                    Throw New Exception("No Internet")
                End If
            End Using

            
            Using client As New HttpClient()
                client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", GitHubToken)
                client.DefaultRequestHeaders.UserAgent.ParseAdd("VBApp")

                Dim json = Await client.GetStringAsync("https://api.github.com/user")
                Dim doc = JsonDocument.Parse(json)

                GitHubUser.Username = doc.RootElement.GetProperty("login").GetString()
                GitHubUser.AvatarUrl = doc.RootElement.GetProperty("avatar_url").GetString()
                GitHubUser.IsLoggedIn = True

                Debug.WriteLine($"LoadGitHubUser: Loaded user '{GitHubUser.Username}' from API")
            End Using

            
            Save()

        Catch ex As Exception
            Debug.WriteLine($"LoadGitHubUser Error: {ex.Message}")
            
        End Try
    End Function

    
    
    
    Public Async Function LoadGitHubAvatar(pb As PictureBox) As Task
        If String.IsNullOrEmpty(GitHubUser.AvatarUrl) Then
            Debug.WriteLine("LoadGitHubAvatar: No avatar URL")
            Return
        End If

        Try
            
            Dim avatarPath As String = AppLayout.P("avatar.png")
            If File.Exists(avatarPath) Then
                Try
                    
                    Dim bytes As Byte() = File.ReadAllBytes(avatarPath)
                    Using ms As New MemoryStream(bytes)
                        pb.BackgroundImage = New Bitmap(ms)
                    End Using
                    Debug.WriteLine("LoadGitHubAvatar: Loaded from cache")
                    Return
                Catch
                    
                End Try
            End If

            
            Using client As New HttpClient()
                Dim bytes() As Byte = Await client.GetByteArrayAsync(GitHubUser.AvatarUrl)

                
                Using ms As New MemoryStream(bytes)
                    pb.BackgroundImage = New Bitmap(ms)
                End Using

                
                Try
                    File.WriteAllBytes(avatarPath, bytes)
                    Debug.WriteLine("LoadGitHubAvatar: Saved to cache")
                Catch ex As Exception
                    Debug.WriteLine($"LoadGitHubAvatar: Failed to save cache - {ex.Message}")
                End Try
            End Using

            Debug.WriteLine("LoadGitHubAvatar: Loaded from URL")

        Catch ex As Exception
            Debug.WriteLine($"LoadGitHubAvatar Error: {ex.Message}")
            
        End Try
    End Function

    
    
    
    Public Sub SaveAvatarFromBytes(avatarBytes As Byte())
        Try
            Dim avatarPath As String = AppLayout.P("avatar.png")
            File.WriteAllBytes(avatarPath, avatarBytes)
            Debug.WriteLine($"SaveAvatarFromBytes: Saved to {avatarPath}")
        Catch ex As Exception
            Debug.WriteLine($"SaveAvatarFromBytes Error: {ex.Message}")
        End Try
    End Sub

    
    
    
    Public ReadOnly Property IsGitHubLoggedIn As Boolean
        Get
            Return GitHubUser.IsLoggedIn AndAlso Not String.IsNullOrEmpty(GitHubToken)
        End Get
    End Property
#End Region

End Class

