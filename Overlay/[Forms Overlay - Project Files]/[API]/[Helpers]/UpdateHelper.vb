Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Reflection
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports System.Drawing

Module UpdateHelper

    Async Function CheckForUpdateAsync() As Task
        Try
            Dim url As String = "https://drive.google.com/uc?id=1N_F7SS8v--_b7GVkTJJ3hpkeD8085GT9"

            Using client As New HttpClient()
                client.Timeout = TimeSpan.FromSeconds(5)
                Dim jsonString As String = Await client.GetStringAsync(url)
                Dim json As JObject = JObject.Parse(jsonString)

                Dim latestVer As New Version(If(json("version") IsNot Nothing, json("version").ToString(), "0.0.0.0"))
                Dim downloadUrl As String = If(json("download_url") IsNot Nothing, json("download_url").ToString(), "")
                Dim versionStats As String = If(json("version_stats") IsNot Nothing, json("version_stats").ToString(), "")
                Dim currentVer As New Version(Assembly.GetExecutingAssembly().GetName().Version.ToString())


                Dim dataFolder As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data")
                If Not Directory.Exists(dataFolder) Then Directory.CreateDirectory(dataFolder)

                If currentVer < latestVer Then
                    If Not String.IsNullOrEmpty(downloadUrl) Then
                        Process.Start(New ProcessStartInfo(downloadUrl) With {.UseShellExecute = True})
                    End If
                    File.Create(Path.Combine(dataFolder, "l10n.update_available")).Dispose()

                Else
                    File.Create(Path.Combine(dataFolder, "l10n.version_latest")).Dispose()
                End If
            End Using

        Catch ex As Exception
            ' เกิด Error
            Dim dataFolder As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data")
            If Not Directory.Exists(dataFolder) Then Directory.CreateDirectory(dataFolder)
            File.Create(Path.Combine(dataFolder, "l10n.notificationErrorGeneral")).Dispose()
        End Try
        Base_Settings.ch.Enabled = True
    End Function

End Module