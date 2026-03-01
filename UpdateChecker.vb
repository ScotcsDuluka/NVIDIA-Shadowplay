Imports System.Net.Http
Imports Newtonsoft.Json.Linq
Imports System.Threading.Tasks
Imports System.IO

Module UpdateChecker
    Async Function CheckForUpdateAsync() As Task
        Try
            Dim url As String = "https://drive.google.com/uc?id=1N_F7SS8v--_b7GVkTJJ3hpkeD8085GT9"

            Using client As New HttpClient()
                ' ตั้ง Timeout 5 วินาที
                client.Timeout = TimeSpan.FromSeconds(5)

                ' โหลด JSON แบบ Async
                Dim jsonString As String = Await client.GetStringAsync(url)

                ' แปลง JSON
                Dim json As JObject = JObject.Parse(jsonString)

                Dim latestVersion As String = json("version").ToString()
                Dim downloadUrl As String = json("download_url").ToString()
                Dim versionStats As String = json("version_stats").ToString()
                Dim currentVersion As String = Application.ProductVersion

                If currentVersion <> latestVersion Then
                    If downloadUrl <> "" Then
                        Process.Start(New ProcessStartInfo(downloadUrl) With {.UseShellExecute = True})
                    End If
                    File.Create(Application.StartupPath & "\NVIDIA_Shadowplay_Data\update_available-api").Dispose()
                Else
                    File.Create(Application.StartupPath & "\NVIDIA_Shadowplay_Data\version_latest-api").Dispose()
                End If
            End Using

        Catch ex As TaskCanceledException
            File.Create(Application.StartupPath & "\NVIDIA_Shadowplay_Data\notificationErrorGeneral-api").Dispose()
        Catch ex As HttpRequestException
            File.Create(Application.StartupPath & "\NVIDIA_Shadowplay_Data\notificationErrorGeneral-api").Dispose()
        Catch ex As Exception
            File.Create(Application.StartupPath & "\NVIDIA_Shadowplay_Data\notificationErrorGeneral-api").Dispose()
        End Try
        Base.ch.Enabled = True
    End Function
End Module