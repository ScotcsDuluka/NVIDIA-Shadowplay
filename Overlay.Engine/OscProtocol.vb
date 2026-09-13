' OscProtocol.vb — PURE functions mapping between the osc web world and the
' hub TCP world. No I/O, no timers: every member is deterministic so the
' test suite can link this file and assert exact strings (the repo's
' ConfigTruth philosophy: test the REAL production source, zero drift).
'
' Ground truth: Overlay.Engine/PROTOCOL-MATRIX.md (evidence-cited).
' The pipe rule: hub broadcasts verbatim and every parser takes parts(1)
' only, so '|' inside a value never survives. These builders therefore
' NEVER emit '|' in values, and the parsers treat multi-pipe tails as
' best-effort leftovers of senders that still do.

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Text

Public NotInheritable Class OscProtocol

    Public Const AppName As String = "NVIDIA Share"
    Public Const EngineAppName As String = "NVIDIA Engine"

    ' ── hub wire line builders (TcpClientHelper.Send produces
    '    "[Send] <app>|<cmd>[:<value>]"; these return cmd/value pairs the
    '    tests assert on, and the exact full line via BuildLine) ──

    Public Shared Function BuildLine(cmd As String, Optional value As String = "") As String
        If String.IsNullOrEmpty(value) Then
            Return "[Send] " & AppName & "|" & cmd
        End If
        Return "[Send] " & AppName & "|" & cmd & ":" & value
    End Function

    Public Shared Function RecordStartValue(outputPath As String) As String
        Return outputPath
    End Function

    ''' <summary>PREWARM_FFMPEG value: PATH ONLY. The Forms overlay appends
    '     "|<encoder>" but the engine-side parser splits on the first pipe
    '     (Engine [Engine] Client.vb:147 → parts(1)), so the suffix never
    '     arrives there — sending it would be protocol theater.</summary>
    Public Shared Function PrewarmValue(ffmpegPath As String) As String
        Return ffmpegPath
    End Function

    Public Shared Function RecordOutputPath(savePath As String, now As DateTime) As String
        Dim dir As String = savePath
        If String.IsNullOrWhiteSpace(dir) Then
            dir = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NVIDIA ShadowPlay", "videos")
        End If
        Return IO.Path.Combine(dir, "Record_" & now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture) & ".mp4")
    End Function

    ' ── hub wire line parser (mirrors Base.OnMessage / OnTcpMessage,
    '    hardened) ──

    Public Class HubMessage
        Public Sender As String
        Public Cmd As String
        Public Value As String
        Public FullValue As String   ' everything after the first ':' in
                                     ' parts(1) INCLUDING any later |segments
    End Class

    ''' <summary>Parses a received line the way the hub relayed it.
    '     Returns Nothing when the line cannot carry a command.</summary>
    Public Shared Function ParseHubLine(msg As String) As HubMessage
        If String.IsNullOrEmpty(msg) Then Return Nothing
        Dim pipeIdx As Integer = msg.IndexOf("|"c)
        If pipeIdx < 0 Then Return Nothing

        Dim result As New HubMessage()
        Dim senderSeg As String = msg.Substring(0, pipeIdx).Trim()
        result.Sender = senderSeg.Replace("[Send] ", "").Replace("[Receive] ", "").Trim()

        ' value = the SECOND segment only (all live parsers do this) — but
        ' keep the tail so defensive consumers can recover '|'-separated
        ' fields when the SENDER is a process that still packs them.
        Dim data As String = msg.Substring(pipeIdx + 1)
        Dim tail As String = ""
        Dim secondPipe As Integer = data.IndexOf("|"c)
        If secondPipe >= 0 Then
            tail = data.Substring(secondPipe + 1)
            data = data.Substring(0, secondPipe)
        End If

        Dim colonIdx As Integer = data.IndexOf(":"c)
        If colonIdx >= 0 Then
            result.Cmd = data.Substring(0, colonIdx)
            result.Value = data.Substring(colonIdx + 1)
        Else
            result.Cmd = data
            result.Value = ""
        End If
        result.FullValue = If(tail.Length > 0, result.Value & "|" & tail, result.Value)
        Return result
    End Function

    ' ── engine_response value helpers ──
    ' Value shape: <cmd>,<status>[,<data>][,req=<id>]; data itself may pack
    ' '|' fields that usually got truncated in transit (pipe rule).

    Public Class EngineResponse
        Public Cmd As String
        Public Status As String
        Public Data As String
        Public RequestId As String
        Public DataFields As List(Of String) ' data split on '|' (may be short)
    End Class

    Public Shared Function ParseEngineResponse(value As String) As EngineResponse
        If String.IsNullOrEmpty(value) Then Return Nothing
        Dim parts As String() = value.Split(","c)
        If parts.Length < 2 Then Return Nothing
        Dim r As New EngineResponse
        r.Cmd = parts(0).Trim()
        r.Status = parts(1).Trim()
        r.DataFields = New List(Of String)
        If parts.Length >= 3 Then
            Dim rest As String = value.Substring(value.IndexOf(","c, value.IndexOf(","c) + 1) + 1)
            ' strip trailing req= token
            Dim reqIdx As Integer = rest.LastIndexOf(",req=", StringComparison.Ordinal)
            If reqIdx >= 0 Then
                r.RequestId = rest.Substring(reqIdx + 5)
                rest = rest.Substring(0, reqIdx)
            End If
            r.Data = rest
            r.DataFields.AddRange(rest.Split("|"c))
        End If
        Return r
    End Function

    ''' <summary>Status payload contract: <state>[|<sec>[|<path>]] — returns
    '     the state word only when the tail was truncated in transit.</summary>
    Public Shared Function StatusStateFromData(data As String) As String
        If String.IsNullOrEmpty(data) Then Return ""
        Dim p As String() = data.Split("|"c)
        Return p(0).Trim()
    End Function

    Public Shared Function StatusElapsedFromData(data As String) As Integer
        Dim p As String() = If(data, "").Split("|"c)
        Dim sec As Integer = -1
        If p.Length >= 2 Then Integer.TryParse(p(1).Trim(), sec)
        Return sec
    End Function

    ' ── reconcile logic (mirrors Overlay HandleEngineStateChanged,
    '    Base [Overlay] Client.vb:233-252) ──

    Public Shared Function ShouldShowRecording(stateName As String, currentlyRecording As Boolean) As Boolean
        Select Case stateName
            Case "Recording" : Return True
            Case "Idle", "HasError" : Return False
            Case "Stopping" : Return currentlyRecording ' Stopping keeps state
            Case Else : Return currentlyRecording
        End Select
    End Function

    ' ── displayRect hit-testing (GFE click-through model) ──
    ' The page sends displayRects (client coords) for regions that must
    ' accept mouse input; everything else must be click-through.

    Public Shared Function PointInAnyRect(x As Integer, y As Integer, rects As IEnumerable(Of Rectangle)) As Boolean
        If rects Is Nothing Then Return False
        For Each r As Rectangle In rects
            If x >= r.Left AndAlso x < r.Right AndAlso y >= r.Top AndAlso y < r.Bottom Then Return True
        Next
        Return False
    End Function

    ''' <summary>Parses the QUERY_OSC_SET_DISPLAY_RECTS request's
    '     displayRects field: JSON array of [x,y,w,h] or {x,y,width,height} —
    '     tolerant to both shapes the bundle may emit.</summary>
    Public Shared Function ParseDisplayRects(jsonArray As System.Text.Json.JsonElement) As List(Of Rectangle)
        Dim list As New List(Of Rectangle)
        If jsonArray.ValueKind <> System.Text.Json.JsonValueKind.Array Then Return list
        For Each el As System.Text.Json.JsonElement In jsonArray.EnumerateArray()
            Try
                If el.ValueKind = System.Text.Json.JsonValueKind.Array AndAlso el.GetArrayLength() >= 4 Then
                    Dim a As System.Text.Json.JsonElement() = EnumerableToArray(el)
                    list.Add(New Rectangle(CInt(a(0).GetDouble()), CInt(a(1).GetDouble()), CInt(a(2).GetDouble()), CInt(a(3).GetDouble())))
                ElseIf el.ValueKind = System.Text.Json.JsonValueKind.Object Then
                    list.Add(New Rectangle(GetInt(el, "x"), GetInt(el, "y"), GetInt(el, "width", "w"), GetInt(el, "height", "h")))
                End If
            Catch
                ' skip malformed entry
            End Try
        Next
        Return list
    End Function

    Private Shared Function EnumerableToArray(el As System.Text.Json.JsonElement) As System.Text.Json.JsonElement()
        Dim items As New List(Of System.Text.Json.JsonElement)
        For Each e As System.Text.Json.JsonElement In el.EnumerateArray()
            items.Add(e)
        Next
        Return items.ToArray()
    End Function

    Private Shared Function GetInt(obj As System.Text.Json.JsonElement, ParamArray names As String()) As Integer
        For Each n As String In names
            Dim v As System.Text.Json.JsonElement
            If obj.TryGetProperty(n, v) AndAlso v.ValueKind = System.Text.Json.JsonValueKind.Number Then
                Return CInt(v.GetDouble())
            End If
        Next
        Return 0
    End Function

End Class
