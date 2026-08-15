Imports System.IO
Imports Newtonsoft.Json.Linq

Public Class ObsConfig

    Public Property Enabled As Boolean = True
    Public Property Host As String = "127.0.0.1"
    Public Property Port As Integer = 4455
    Public Property Password As String = ""

    Public Property ForwardRecordStateChanged As Boolean = True
    Public Property ForwardReplayBufferStateChanged As Boolean = True
    Public Property ForwardReplayBufferSaved As Boolean = True
    Public Property ForwardScreenshotSaved As Boolean = True

    Public Const FileName As String = "notifier_obs.json"

    Public Shared Function Load() As ObsConfig
        Dim cfg As New ObsConfig()
        Dim configPath As String = Path.Combine(Application.StartupPath, FileName)
        If Not File.Exists(configPath) Then Return cfg

        Try
            Dim json As String
            Using fs As New FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Using sr As New StreamReader(fs)
                    json = sr.ReadToEnd()
                End Using
            End Using

            Dim root As JObject = JObject.Parse(json)

            cfg.Enabled = ReadBool(root, "enabled", True)
            cfg.Host = ReadString(root, "host", "127.0.0.1")
            cfg.Port = ReadInt(root, "port", 4455)
            cfg.Password = ReadString(root, "password", "")

            Dim forward As JObject = TryCast(root("forward"), JObject)
            If forward IsNot Nothing Then
                cfg.ForwardRecordStateChanged = ReadBool(forward, "record_state_changed", True)
                cfg.ForwardReplayBufferStateChanged = ReadBool(forward, "replay_buffer_state_changed", True)
                cfg.ForwardReplayBufferSaved = ReadBool(forward, "replay_buffer_saved", True)
                cfg.ForwardScreenshotSaved = ReadBool(forward, "screenshot_saved", True)
            End If

            Debug.WriteLine("[ObsConfig] Loaded " & configPath &
                            "  enabled=" & cfg.Enabled.ToString() &
                            "  host=" & cfg.Host & ":" & cfg.Port.ToString())
        Catch ex As Exception
            Debug.WriteLine("[ObsConfig] Failed to load " & configPath &
                            ": " & ex.Message & "  (using defaults)")
        End Try

        Return cfg
    End Function

    Private Shared Function ReadBool(obj As JObject, key As String, defaultValue As Boolean) As Boolean
        If obj Is Nothing Then Return defaultValue
        Dim tok As JToken = obj(key)
        If tok Is Nothing Then Return defaultValue
        Try
            Return tok.Value(Of Boolean)()
        Catch
            Return defaultValue
        End Try
    End Function

    Private Shared Function ReadInt(obj As JObject, key As String, defaultValue As Integer) As Integer
        If obj Is Nothing Then Return defaultValue
        Dim tok As JToken = obj(key)
        If tok Is Nothing Then Return defaultValue
        Try
            Return tok.Value(Of Integer)()
        Catch
            Dim s As String = Nothing
            Try
                s = tok.Value(Of String)()
            Catch
            End Try
            Dim n As Integer
            If s IsNot Nothing AndAlso Integer.TryParse(s, n) Then Return n
            Return defaultValue
        End Try
    End Function

    Private Shared Function ReadString(obj As JObject, key As String, defaultValue As String) As String
        If obj Is Nothing Then Return defaultValue
        Dim tok As JToken = obj(key)
        If tok Is Nothing Then Return defaultValue
        Dim s As String = Nothing
        Try
            s = tok.Value(Of String)()
        Catch
        End Try
        If String.IsNullOrEmpty(s) Then Return defaultValue
        Return s
    End Function

    Public Function ShouldForward(eventType As String) As Boolean
        Select Case eventType
            Case "RecordStateChanged"
                Return ForwardRecordStateChanged
            Case "ReplayBufferStateChanged"
                Return ForwardReplayBufferStateChanged
            Case "ReplayBufferSaved"
                Return ForwardReplayBufferSaved
            Case "ScreenshotSaved"
                Return ForwardScreenshotSaved
            Case Else
                Return False
        End Select
    End Function

End Class
