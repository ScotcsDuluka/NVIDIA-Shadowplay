Imports System.IO
Imports Newtonsoft.Json.Linq

''' <summary>
''' Loads notifier_obs.json (sits next to the Notifier exe). Falls back to
''' sensible defaults if the file is missing or malformed — the Notifier
''' should never crash because of a config typo.
''' </summary>
Public Class ObsConfig

    Public Property Enabled As Boolean = True
    Public Property Host As String = "127.0.0.1"
    Public Property Port As Integer = 4455
    Public Property Password As String = ""

    Public Property ForwardRecordStateChanged As Boolean = True
    Public Property ForwardReplayBufferStateChanged As Boolean = True
    Public Property ForwardScreenshotSaved As Boolean = True

    Public Const FileName As String = "notifier_obs.json"

    Public Shared Function Load() As ObsConfig
        Dim cfg As New ObsConfig()
        Dim path = Path.Combine(Application.StartupPath, FileName)
        If Not File.Exists(path) Then Return cfg

        Try
            Dim json As String
            Using fs As New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using sr As New StreamReader(fs)
                    json = sr.ReadToEnd()
                End Using
            End Using

            Dim root = JObject.Parse(json)
            cfg.Enabled = root("enabled")?.Value(Of Boolean)() ?? True
            cfg.Host = If(root("host")?.Value(Of String)(), "127.0.0.1")
            cfg.Port = If(root("port")?.Value(Of Integer)(), 4455)
            cfg.Password = If(root("password")?.Value(Of String)(), "")

            Dim forward = root("forward")
            If forward IsNot Nothing Then
                cfg.ForwardRecordStateChanged = If(forward("record_state_changed")?.Value(Of Boolean)(), True)
                cfg.ForwardReplayBufferStateChanged = If(forward("replay_buffer_state_changed")?.Value(Of Boolean)(), True)
                cfg.ForwardScreenshotSaved = If(forward("screenshot_saved")?.Value(Of Boolean)(), True)
            End If

            Debug.WriteLine($"[ObsConfig] Loaded {path}  enabled={cfg.Enabled}  host={cfg.Host}:{cfg.Port}")
        Catch ex As Exception
            Debug.WriteLine($"[ObsConfig] Failed to load {path}: {ex.Message}  (using defaults)")
        End Try

        Return cfg
    End Function

    Public Function ShouldForward(eventType As String) As Boolean
        Select Case eventType
            Case "RecordStateChanged" : Return ForwardRecordStateChanged
            Case "ReplayBufferStateChanged" : Return ForwardReplayBufferStateChanged
            Case "ScreenshotSaved" : Return ForwardScreenshotSaved
            Case Else : Return False
        End Select
    End Function

End Class
