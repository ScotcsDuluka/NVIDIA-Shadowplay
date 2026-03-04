Imports System.Diagnostics
Imports System.IO
Imports System.Net.Http
Imports System.Reflection
Imports System.Threading.Tasks
Imports Newtonsoft.Json.Linq
Imports System.Drawing

Module UpdateChecker

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
                    File.Create(Path.Combine(dataFolder, "update_available-api")).Dispose()
                    Base_Notifier.Show()
                    With Base_Notifier
                        .icon_n.Font = New Font(.icon_n.Font.FontFamily, If(isValidPath, 50, 40))
                        .icon_n.ForeColor = Color.White
                        .icon_n.Text = ""
                        .text_n.Text = LangHelper.GetText("l10n.notificationUploadURLCopied")
                    End With
                Else
                    File.Create(Path.Combine(dataFolder, "version_latest-api")).Dispose()
                End If
            End Using

        Catch ex As Exception
            ' เกิด Error
            Dim dataFolder As String = Path.Combine(Application.StartupPath, "NVIDIA_Shadowplay_Data")
            If Not Directory.Exists(dataFolder) Then Directory.CreateDirectory(dataFolder)
            File.Create(Path.Combine(dataFolder, "notificationErrorGeneral-api")).Dispose()
        End Try
        Base.ch.Enabled = True
    End Function

End Module