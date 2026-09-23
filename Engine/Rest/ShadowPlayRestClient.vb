' ShadowPlayRestClient.vb — the engine's REST command channel (Phase 3).
'
' REPLACES the TCP :5001 hub commands with the SAME REST API the osc page
' uses on the backend (:59001) — one wire, one source of truth:
'
'   old (TCP :5001 hub)            new (REST :59001, same API as osc page)
'   ─────────────────────────      ────────────────────────────────────────
'   RECORD_START:<path>      <-    POST /ShadowPlay/v.1.0/Record/Enable {status:true}
'   RECORD_STOP              <-    POST /ShadowPlay/v.1.0/Record/Enable {status:false}
'   (polls /state)           <-    GET  /ShadowPlay/v.1.0/Record/Enable|Running
'   (hub push to page)       ->    POST /ShadowPlay/v.1.0/Record/Running {running:...}
'   INSTANTREPLAY_*          <-    POST /ShadowPlay/v.1.0/InstantReplay/Enable {status:...}
'
' How the engine uses it (drop-in, dependency-free shared source):
'
'   Dim rest As New ShadowPlayRestClient()                       ' :59001 default
'   rest.WaitForBackend(TimeSpan.FromSeconds(30))                ' boot gate
'   Dim poller As New RestCommandPoller(rest)
'   AddHandler poller.RecordStartRequested, Sub(path) ...        ' = RECORD_START
'   AddHandler poller.RecordStopRequested,  Sub()     ...        ' = RECORD_STOP
'   poller.Start()
'   ...
'   reporter.PublishRecordRunning(True)   ' live truth for the osc page
'
' Edge detection: the poller watches Enable flips (the osc page's POSTs),
' so the engine never needs a listener port of its own — the backend is
' the single authority. Poll failures are logged and retried (the backend
' restarting must never take the engine down — standalone mode rule).

Option Strict On
Option Explicit On
Option Infer On

Imports System.Diagnostics
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Threading

Public NotInheritable Class ShadowPlayRestClient

    Private Const DefaultBaseUrl As String = "http://127.0.0.1:59001"
    ' Liveness probe: the deployed Duluka backend serves an OPEN /health
    ' BEFORE the auth middleware ({"status":"ok"}). The authed
    ' /Backend/v.1.0/health shape belongs to the newer dev backend, not the
    ' deployed Phase 1B host -- watch /health for the boot gate.
    Private Const HealthPath As String = "/health"
    ' M1 host cookie: the same value the osc bundle carries in commonHeaders
    ' and the deployed backend pins in config/default.json. Every request
    ' carries it (header form), same scheme as the osc page.
    Private Const DefaultSecret As String = "eb6eb0702ec25f9aeb0b0f8f79d06d5b"
    Private Shared ReadOnly _secret As String = If(Environment.GetEnvironmentVariable("DULUKA_BACKEND_SECRET"), DefaultSecret)
    Private Const RequestTimeoutMs As Integer = 3000

    Private ReadOnly _baseUrl As String

    Public Sub New()
        _baseUrl = DefaultBaseUrl
    End Sub

    Public Sub New(baseUrl As String)
        _baseUrl = If(String.IsNullOrWhiteSpace(baseUrl), DefaultBaseUrl, baseUrl.TrimEnd("/"c))
    End Sub

    Public ReadOnly Property BaseUrl As String
        Get
            Return _baseUrl
        End Get
    End Property

    ' ── JSON surface (tiny local helpers — no dependencies) ─────────
    Public Function GetJson(path As String) As String
        Return Request("GET", path, Nothing)
    End Function

    Public Function PostJson(path As String, jsonBody As String) As String
        Return Request("POST", path, If(jsonBody, "{}"))
    End Function

    Public Shared Function JsonBoolField(json As String, field As String) As Boolean
        If String.IsNullOrEmpty(json) Then Return False
        ' matches "field":true / "field":false (compact JSON bodies)
        Dim marker As String = """" & field & """:"
        Dim idx As Integer = json.IndexOf(marker, StringComparison.Ordinal)
        If idx < 0 Then Return False
        Dim tail As String = json.Substring(idx + marker.Length).TrimStart()
        Return tail.StartsWith("true", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Shared Function JsonStringField(json As String, field As String) As String
        If String.IsNullOrEmpty(json) Then Return Nothing
        Dim marker As String = """" & field & """:"
        Dim idx As Integer = json.IndexOf(marker, StringComparison.Ordinal)
        If idx < 0 Then Return Nothing
        Dim tail As String = json.Substring(idx + marker.Length).TrimStart()
        If Not tail.StartsWith("""") Then Return Nothing
        Dim sb As New StringBuilder()
        Dim i As Integer = 1
        While i < tail.Length
            Dim c As Char = tail(i)
            If c = "\"c AndAlso i + 1 < tail.Length Then
                Dim n As Char = tail(i + 1)
                If n = "\"c Then
                    sb.Append("\"c)
                ElseIf n = "/"c Then
                    sb.Append("/"c)
                ElseIf n = "n"c Then
                    sb.Append(ControlChars.Lf)
                ElseIf n = "r"c Then
                    sb.Append(ControlChars.Cr)
                ElseIf n = "t"c Then
                    sb.Append(ControlChars.Tab)
                End If
                i += 2
            ElseIf c = """"c Then
                Exit While
            Else
                sb.Append(c)
                i += 1
            End If
        End While
        Return sb.ToString()
    End Function

    ' ── boot gate ───────────────────────────────────────────────────
    Public Function IsBackendUp() As Boolean
        Try
            Dim body As String = Request("GET", HealthPath, Nothing)
            Return body IsNot Nothing AndAlso (body.IndexOf("""ok"":true", StringComparison.Ordinal) >= 0 OrElse body.IndexOf("""status"":""ok""", StringComparison.Ordinal) >= 0)
        Catch
            Return False
        End Try
    End Function

    Public Function WaitForBackend(timeout As TimeSpan) As Boolean
        Dim deadline As DateTime = DateTime.UtcNow + timeout
        While DateTime.UtcNow < deadline
            If IsBackendUp() Then Return True
            Thread.Sleep(1000)
        End While
        Return IsBackendUp()
    End Function

    ' ── typed command/state helpers (the osc-page API verbatim) ─────
    Public Function GetRecordEnabled() As Boolean
        Return JsonBoolField(GetJson("/ShadowPlay/v.1.0/Record/Enable"), "status")
    End Function

    Public Function GetInstantReplayEnabled() As Boolean
        Return JsonBoolField(GetJson("/ShadowPlay/v.1.0/InstantReplay/Enable"), "status")
    End Function

    Public Function GetRecordSavePath() As String
        Dim videos As String = JsonStringField(GetJson("/ShadowPlay/v.1.0/RecordPaths"), "videos")
        If String.IsNullOrWhiteSpace(videos) Then
            videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
        End If
        Return videos
    End Function

    ' ── engine-plane state surface (the DEPLOYED backend's proven loop) ──
    ' GET /Record/Enable only flips true AFTER the engine's own confirm
    ' (recordState=Recording), so the capture engine watches the state
    ' machine string instead: Idle|Starting|Recording|Stopping|Saved|HasError.
    Public Function GetRecordStateName() As String
        Return JsonStringField(GetJson("/Duluka/v.1.0/State"), "recordState")
    End Function

    ' Actual confirmations on the Duluka ingestion surface: the backend runs
    ' the PROVEN state machine (Starting -> Recording only via actual.confirm)
    ' and emits the socket pushes the osc page trusts -- desired changes never
    ' push, actuals do. Invalid transitions are REJECTED (never guessed).
    Public Function ConfirmRecordStarted() As String
        Return PostJson("/Duluka/v.1.0/Actual", "{""record"":""Recording""}")
    End Function

    Public Function ConfirmRecordStopped() As String
        Return PostJson("/Duluka/v.1.0/Actual", "{""record"":""Idle""}")
    End Function

    Public Function ReportRecordError(reason As String) As String
        Dim r As String = If(reason, "").Replace("\"c, "/"c).Replace(""""c, "'"c)
        Return PostJson("/Duluka/v.1.0/Actual", "{""record"":""Error"",""reason"":""" & r & """}")
    End Function

    Public Sub PublishRecordRunning(running As Boolean)
        Try
            PostJson("/ShadowPlay/v.1.0/Record/Running", "{""running"":" & If(running, "true", "false") & "}")
        Catch
            ' publish failures are never fatal
        End Try
    End Sub

    Public Sub PublishInstantReplayRunning(running As Boolean)
        Try
            PostJson("/ShadowPlay/v.1.0/InstantReplay/Running", "{""running"":" & If(running, "true", "false") & "}")
        Catch
        End Try
    End Sub

    ' ── transport ───────────────────────────────────────────────────
    Private Function Request(method As String, path As String, body As String) As String
        Dim req As HttpWebRequest = CType(WebRequest.Create(_baseUrl & path), HttpWebRequest)
        req.Method = method
        req.Timeout = RequestTimeoutMs
        req.ReadWriteTimeout = RequestTimeoutMs
        req.ContentType = "application/json"
        req.Headers("X_LOCAL_SECURITY_COOKIE") = _secret
        If body IsNot Nothing Then
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(body)
            req.ContentLength = bytes.Length
            Using s As IO.Stream = req.GetRequestStream()
                s.Write(bytes, 0, bytes.Length)
            End Using
        End If
        Using resp As WebResponse = req.GetResponse()
            Using reader As New StreamReader(resp.GetResponseStream(), Encoding.UTF8)
                Return reader.ReadToEnd()
            End Using
        End Using
    End Function

End Class

' ── poller: Enable edge detection -> engine events ──────────────────
Public NotInheritable Class RestCommandPoller

    Private Const PollIntervalMs As Integer = 1000
    Private Const ErrorRetryMs As Integer = 3000

    Private ReadOnly _rest As ShadowPlayRestClient
    Private ReadOnly _thread As Thread
    Private _running As Boolean = False
    Private _lastRecordState As String = Nothing
    Private _lastInstantReplayEnabled As Boolean = False
    Private _primed As Boolean = False

    ' = RECORD_START:<path> / RECORD_STOP from the old hub contract
    Public Event RecordStartRequested(savePath As String)
    Public Event RecordStopRequested()
    Public Event InstantReplayStartRequested(savePath As String)
    Public Event InstantReplayStopRequested()

    Public Sub New(restClient As ShadowPlayRestClient)
        _rest = restClient
        _thread = New Thread(AddressOf PollLoop) With {.Name = "RestCommandPoller", .IsBackground = True}
    End Sub

    Public Sub Start()
        If _running Then Return
        _running = True
        _thread.Start()
    End Sub

    Public Sub [Stop]()
        _running = False
    End Sub

    Private Sub PollLoop()
        While _running
            Try
                Dim st As String = _rest.GetRecordStateName()
                Dim ir As Boolean = _rest.GetInstantReplayEnabled()

                If Not _primed Then
                    ' first cycle adopts current state (no spurious START on
                    ' boot) -- but a Starting/Stopping leftover from a previous
                    ' session is PENDING WORK, not neutral state: recover it.
                    _lastRecordState = st
                    _lastInstantReplayEnabled = ir
                    _primed = True
                    If st = "Starting" Then
                        RaiseEvent RecordStartRequested(_rest.GetRecordSavePath())
                    ElseIf st = "Stopping" Then
                        RaiseEvent RecordStopRequested()
                    End If
                Else
                    If st = "Starting" AndAlso _lastRecordState <> "Starting" Then
                        RaiseEvent RecordStartRequested(_rest.GetRecordSavePath())
                    ElseIf st = "Stopping" AndAlso _lastRecordState <> "Stopping" Then
                        RaiseEvent RecordStopRequested()
                    End If
                    If ir AndAlso Not _lastInstantReplayEnabled Then
                        RaiseEvent InstantReplayStartRequested(_rest.GetRecordSavePath())
                    ElseIf Not ir AndAlso _lastInstantReplayEnabled Then
                        RaiseEvent InstantReplayStopRequested()
                    End If
                    _lastRecordState = st
                    _lastInstantReplayEnabled = ir
                End If

                Thread.Sleep(PollIntervalMs)
            Catch ex As Exception
                ' backend restart / network blip — never fatal, retry slow
                Debug.WriteLine($"[RestCommandPoller] poll error: {ex.Message}")
                Thread.Sleep(ErrorRetryMs)
            End Try
        End While
    End Sub

End Class
