' OscControllerServer.vb — the loopback controller server the osc page
' talks to (same origin as the static app, so there is no CORS surface).
'
' Two protocols on one port:
'   1. Static osc files + REST subset  (cookie-excepted for static GETs)
'   2. engine.io v3 polling + socket.io v2  — implemented EXACTLY as
'      measured in the golden transcript
'      (Tester/test/Overlay/Overlay.OscEngine.Tests/golden/):
'        handshake GET → <len>:0{"sid",...}
'        server sends socket.io CONNECT "40" on the client's first poll
'          (socket.io v2 clients never send CONNECT on the default ns)
'        client POSTs its PING "1:2" every pingInterval → we queue PONG "3"
'        events pushed as <len>:42["<channel>",<json>] batches, several
'          packets per response are fine
'
' Heartbeat direction note (measured, not assumed): in engine.io v3 the
' CLIENT pings. The server only answers.

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Public Class OscControllerServer
    Implements IDisposable

    Public Const DefaultPingTimeoutMs As Integer = 60000

    ''' <summary>Env override for tests/e2e (OSCENGINE_PING_INTERVAL) —
    '     production default stays 25000, matching GFE cadence.</summary>
    Public Shared ReadOnly Property EffectivePingIntervalMs As Integer = GetEnvPingInterval()

    Private ReadOnly _port As Integer
    Private ReadOnly _secret As String
    Private ReadOnly _oscRoot As String
    Private ReadOnly _listener As HttpListener
    Private ReadOnly _cts As New CancellationTokenSource()
    Private ReadOnly _sessions As New ConcurrentDictionary(Of String, SioSession)
    Private _loopTask As Task

    Public Event LogLine(message As String)
    Public Event RestRequested(method As String, path As String)
    Public Event ClientEventReceived(channel As String, payloadJson As String)
    Public Event UiReady()

    ' ── host-provided REST state (the form wires these) ──
    Public Property StateProvider As Func(Of String)   ' GET /state → JSON
    Public Property RecordSettingsProvider As Func(Of String) ' GET /Record/Settings
    Public Property RecordPathsProvider As Func(Of String)    ' GET /RecordPaths
    Public Property RecordEnabledHandler As Action(Of Boolean) ' POST /Record/Enable
    Public Property LanguageProvider As Func(Of String)       ' GET /Language
    Public Event ScreenshotRequested()                        ' POST /Screenshot/Capture

    ' ── capability probes (zero-dependency) ──

    ''' <summary>Number of waveform-audio input (microphone) devices via
    '     winmm — no extra packages needed.</summary>
    Private Shared Function MicrophoneCount() As Integer
        Return NativeAudio.waveInGetNumDevs()
    End Function

    ''' <summary>True when a video-capture (camera) device class instance is
    '     present in the registry — no WMI/package needed.</summary>
    Private Shared Function WebcamPresent() As Boolean
        Try
            Dim root As String = "HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\DeviceClasses\{e5323777-f976-4f5b-9b55-b94699c46e44}"
            Dim key As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                "SYSTEM\CurrentControlSet\Control\DeviceClasses\{e5323777-f976-4f5b-9b55-b94699c46e44}")
            If key Is Nothing Then Return False
            Using key
                Return key.SubKeyCount > 0
            End Using
        Catch
            Return False
        End Try
    End Function

    Private Class NativeAudio
        <System.Runtime.InteropServices.DllImport("winmm.dll")>
        Public Shared Function waveInGetNumDevs() As Integer
        End Function

        <System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet:=System.Runtime.InteropServices.CharSet.Unicode)>
        Public Structure WaveInCaps
            Public wMid As UShort
            Public wPid As UShort
            Public vDriverVersion As UInteger
            <System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst:=32)>
            Public szPname As String
            Public dwFormats As UInteger
            Public wChannels As UShort
            Public wReserved1 As UShort
        End Structure

        <System.Runtime.InteropServices.DllImport("winmm.dll", CharSet:=System.Runtime.InteropServices.CharSet.Unicode)>
        Public Shared Function waveInGetDevCapsW(uDeviceID As IntPtr, ByRef caps As WaveInCaps, cbCaps As UInteger) As Integer
        End Function
    End Class

    ''' <summary>Product name of a waveform-input device ("" when absent) —
    '     the Audio settings page renders these in the per-mic rows.</summary>
    Private Shared Function MicrophoneName(index As Integer) As String
        Try
            If index < 0 OrElse index >= MicrophoneCount() Then Return ""
            Dim caps As New NativeAudio.WaveInCaps()
            Dim ok As Integer = NativeAudio.waveInGetDevCapsW(New IntPtr(index), caps, CUInt(System.Runtime.InteropServices.Marshal.SizeOf(GetType(NativeAudio.WaveInCaps))))
            If ok = 0 Then Return caps.szPname
        Catch
        End Try
        Return ""
    End Function

    Private Shared Function MicrophoneSettingsJson(index As Integer) As String
        Dim stored As JsonObject = GetSection("mic:" & index.ToString(CultureInfo.InvariantCulture))
        If stored.Count > 0 Then
            stored("index") = index
            If stored("name") Is Nothing Then stored("name") = MicrophoneName(index)
            Return stored.ToJsonString()
        End If
        Return "{""index"":" & index.ToString(CultureInfo.InvariantCulture) &
               ",""name"":""" & MicrophoneName(index) &
               """,""muted"":false,""volumePercent"":100,""boostPercent"":0}"
    End Function

    Private Class SioSession
        Public ReadOnly Lock As New Object()
        Public Pending As New List(Of String)
        Public Waiter As TaskCompletionSource(Of Boolean)
        Public Connected As Boolean
        Public LastSeen As DateTime = DateTime.UtcNow
    End Class

    Public Sub New(oscRoot As String, Optional requestedPort As Integer = 0)
        _oscRoot = IO.Path.GetFullPath(oscRoot)
        _secret = New Random().Next(100000, 999999).ToString(CultureInfo.InvariantCulture) &
                  Guid.NewGuid().ToString("N").Substring(0, 12)
        _port = If(requestedPort > 0, requestedPort, FindFreePort())
        _listener = New HttpListener()
        _listener.Prefixes.Add("http://127.0.0.1:" & _port.ToString(CultureInfo.InvariantCulture) & "/")
    End Sub

    Public ReadOnly Property Port As Integer
        Get
            Return _port
        End Get
    End Property

    Public ReadOnly Property Secret As String
        Get
            Return _secret
        End Get
    End Property

    Public ReadOnly Property ServerUrl As String
        Get
            Return "http://127.0.0.1:" & _port.ToString(CultureInfo.InvariantCulture)
        End Get
    End Property

    Private Shared Function GetEnvPingInterval() As Integer
        Dim text As String = Environment.GetEnvironmentVariable("OSCENGINE_PING_INTERVAL")
        Dim value As Integer
        If Not String.IsNullOrEmpty(text) AndAlso Integer.TryParse(text, value) AndAlso value >= 1000 Then
            Return value
        End If
        Return 25000 ' the GFE-cadence default
    End Function

    Private Shared Function FindFreePort() As Integer
        ' HttpListener cannot bind :0; take a port from the OS via a
        ' transient TcpListener on loopback, then use it immediately.
        For attempt As Integer = 1 To 10
            Try
                Dim l As New System.Net.Sockets.TcpListener(IPAddress.Loopback, 0)
                l.Start()
                Dim p As Integer = CType(l.LocalEndpoint, IPEndPoint).Port
                l.Stop()
                Return p
            Catch
            End Try
        Next
        Return 3000 ' osc's own LOCALHOST_PORT fallback
    End Function

    Public Sub Start()
        _listener.Start()
        _loopTask = Task.Run(AddressOf AcceptLoop, _cts.Token)
        RaiseEvent LogLine("controller server on " & ServerUrl & " secret=" & _secret & " (osc=" & _oscRoot & ")")
    End Sub

    Public Sub StopServer()
        Try : _cts.Cancel() : Catch : End Try
        Try : _listener.Stop() : Catch : End Try
        SyncLock _sessions
            For Each s As SioSession In _sessions.Values
                Try
                    If s.Waiter IsNot Nothing Then s.Waiter.TrySetResult(False)
                Catch
                End Try
            Next
            _sessions.Clear()
        End SyncLock
    End Sub

    ' ── accept loop ────────────────────────────────────────────

    Private Sub AcceptLoop()
        While Not _cts.IsCancellationRequested
            Dim ctx As HttpListenerContext = Nothing
            Try
                ctx = _listener.GetContext()
            Catch ex As Exception
                If _cts.IsCancellationRequested Then Exit While
                RaiseEvent LogLine("accept error: " & ex.Message)
                Continue While
            End Try
            Dim captured As HttpListenerContext = ctx
            Task.Run(Sub()
                         Try : HandleContext(captured) : Catch ex As Exception
                             RaiseEvent LogLine("handle error: " & ex.Message)
                         End Try
                     End Sub, _cts.Token)
        End While
    End Sub

    ' ── request routing ────────────────────────────────────────

    Private Sub HandleContext(ctx As HttpListenerContext)
        Dim req As HttpListenerRequest = ctx.Request
        Dim res As HttpListenerResponse = ctx.Response
        Dim rawPath As String = Uri.UnescapeDataString(req.Url.AbsolutePath)
        Dim method As String = req.HttpMethod

        Try
            ' CORS preflight: answered unconditionally (page is loopback;
            ' this only matters for dev tools / future cross-host tooling).
            If method = "OPTIONS" Then
                res.Headers("Access-Control-Allow-Origin") = "*"
                res.Headers("Access-Control-Allow-Headers") = req.Headers("Access-Control-Request-Headers")
                res.Headers("Access-Control-Allow-Methods") = "GET, POST, PUT, DELETE, OPTIONS"
                WriteJson(res, 200, "{}")
                Return
            End If

            If rawPath.StartsWith("/socket.io/", StringComparison.Ordinal) Then
                HandleSocketIo(req, res)
                Return
            End If

            If rawPath = "/favicon.ico" Then
                ' osc ships no favicon — 204 keeps the browser console clean
                res.StatusCode = 204
                Return
            End If

            ' static files: no cookie requirement (the page fetches l10n and
            ' assets without custom headers from various code paths)
            If method = "GET" AndAlso IsStaticPath(rawPath) Then
                If rawPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) OrElse rawPath = "/" Then
                    RaiseEvent LogLine("static GET " & rawPath)
                End If
                ServeStatic(rawPath, res)
                Return
            End If

            If Not HasCookie(req) Then
                RaiseEvent LogLine("401 " & method & " " & rawPath & " (missing/invalid cookie)")
                WriteJson(res, 401, "{}")
                Return
            End If

            RaiseEvent RestRequested(method, rawPath)

            ' ── FULL MODE: real-response catalog ──
            ' Every GET is answered from the captured REAL controller
            ' responses (146 endpoints, captured live from the installed
            ' GFE) before falling back to the specific handlers/{} — the
            ' osc page therefore sees the SAME answers the real host gives.
            If method = "GET" Then
                Dim catalogFile As String = FindCatalogResponse(rawPath)
                If catalogFile.Length > 0 Then
                    Try
                        res.ContentType = "application/json; charset=UTF-8"
                        WriteText(res, 200, File.ReadAllText(catalogFile))
                    Catch ex As Exception
                        WriteJson(res, 200, "{}")
                    End Try
                    Return
                End If
            End If

            Select Case rawPath
                Case "/uiReady"
                    If method = "POST" Then RaiseEvent UiReady()
                    WriteJson(res, 200, "{}")
                Case "/state"
                    WriteJson(res, 200, SafeJson(StateProvider, "{}"))
                Case "/ShadowPlay/v.1.0/Record/Settings"
                    RaiseEvent LogLine("Record/Settings " & method & " hit (case)")
                    If method = "POST" Then
                        Dim body As String = ReadBody(req)
                        StoreSection("recordSettings", body)
                        ApplyRecordSettingsToEngineConfig(body)
                        WriteJson(res, 200, "{}")
                    Else
                        WriteJson(res, 200, BuildRecordSettingsFromEngineConfig())
                    End If
                Case "/ShadowPlay/v.1.0/RecordPaths"
                    If method = "POST" Then
                        StoreSection("recordPaths", ReadBody(req))
                        WriteJson(res, 200, "{}")
                    Else
                        Dim stored As JsonObject = GetSection("recordPaths")
                        WriteJson(res, 200, If(stored.Count > 0, stored.ToJsonString(), SafeJson(RecordPathsProvider, "{}")))
                    End If
                Case "/ShadowPlay/v.1.0/Record/Enable"
                    If method = "POST" Then
                        Dim body As String = ReadBody(req)
                        Dim enable As Boolean = body.Trim().ToLowerInvariant().Contains("true")
                        RaiseEvent LogLine("Record/Enable → " & enable.ToString())
                        If RecordEnabledHandler IsNot Nothing Then RecordEnabledHandler.Invoke(enable)
                        WriteJson(res, 200, "{}")
                    Else
                        WriteJson(res, 200, "{}")
                    End If
                Case "/Language", "/Settings/v.1.0/Language"
                    ' GFE shape: {"language":"<locale>"} — an empty {} made the
                    ' settings service crash a digest on .indexOf (measured).
                    WriteJson(res, 200, SafeJson(LanguageProvider, "{""language"":""en-US""}"))
                Case "/support"
                    WriteJson(res, 200, "{}")
                Case "/ShadowPlay/v.1.0/Screenshot/Support"
                    ' the page gates the Screenshot panel on data.support
                    WriteJson(res, 200, "{""support"":true}")
                Case "/ShadowPlay/v.1.0/Screenshot/Capture"
                    If method = "POST" Then
                        RaiseEvent ScreenshotRequested()
                        WriteJson(res, 200, "{}")
                    Else
                        WriteJson(res, 200, "{}")
                    End If
                Case "/ShadowPlay/v.1.0/Microphone/Present"
                    ' data.present = microphone COUNT (number of mics)
                    WriteJson(res, 200, "{""present"":" & MicrophoneCount().ToString(CultureInfo.InvariantCulture) & "}")
                Case "/ShadowPlay/v.1.0/Webcam/Present"
                    ' data.present = boolean
                    WriteJson(res, 200, "{""present"":" & If(WebcamPresent(), "true", "false") & "}")
                Case "/ShadowPlay/v.1.0/Webcam/Enable"
                    If method = "POST" Then
                        WriteJson(res, 200, "{}")
                    Else
                        WriteJson(res, 200, "{""status"":false}")
                    End If
                Case "/ShadowPlay/v.1.0/Record/Running"
                    WriteJson(res, 200, SafeJson(StateProvider, "{""running"":false}"))
                Case "/ShadowPlay/v.1.0/InstantReplay/Running"
                    ' honest M2: replay is not wired yet
                    WriteJson(res, 200, "{""running"":false,""status"":""off""}")
                Case "/ShadowPlay/v.1.0/InstantReplay/Enable"
                    If method = "POST" Then
                        StoreSection("instantReplay", ReadBody(req))
                        WriteJson(res, 200, "{}")
                    Else
                        Dim stored As JsonObject = GetSection("instantReplay")
                        WriteJson(res, 200, If(stored.Count > 0, stored.ToJsonString(), "{""enabled"":false}"))
                    End If
                Case "/HardwareInformation/v.0.1"
                    ' Boot resolve + gfwslService need a REAL GPU list: the
                    ' octool min-spec check does findWhere(GPU,{IsPrimary:"1"})
                    ' and an empty GPU list crashed gfwsl (measured). The values
                    ' pass the bundled min-spec Bt={GPUArchitecture:352,
                    ' ExGPUArchImplementation:[8,7],OSBuildNumber:17134,
                    ' DDVersion:455}.
                    WriteJson(res, 200,
                        "{""GPU"":[{""IsPrimary"":""1"",""GPUArchitecture"":""352"",""GPUArchImplementation"":""0""}]," &
                        """OSBuildNumber"":26100,""DriverVersion"":""566.36""}")
                Case "/ShadowPlay/v.1.0/DesktopCapture/Support/Reason"
                    ' MUST be {"support":true} or a STRING unsupportReason — the
                    ' boot chain does unsupportReason.indexOf(...) when support
                    ' is falsy and a missing field crashes it (measured,
                    ' app.js:2:27389). M1 claims support; the feature itself is
                    ' not wired yet (Enable → {}).
                    WriteJson(res, 200, "{""support"":true}")
                Case Else
                    ' ── PREVIEW MODE (owner directive "เปิด Preview ทุก UI"):
                    ' dynamic-path handlers for the tiles/pages unlocked by the
                    ' shim — hotkey bindings (a missing one renders the tile
                    ' shortcut as "l10n.disabled"), broadcast, OSD indicators,
                    ' desktop capture, concurrency.
                    If rawPath.StartsWith("/ShadowPlay/v.1.0/Hotkey/", StringComparison.OrdinalIgnoreCase) Then
                        Dim hkName As String = rawPath.Substring("/ShadowPlay/v.1.0/Hotkey/".Length)
                        If method = "GET" Then
                            Dim stored As JsonObject = GetSection("hotkey:" & hkName.ToLowerInvariant())
                            If stored.Count > 0 Then
                                WriteJson(res, 200, stored.ToJsonString())
                            Else
                                WriteJson(res, 200, HotkeyPreviewJson(hkName))
                            End If
                            Return
                        End If
                        ' POST: the keyboard-shortcuts page saves {keys:[vk...]};
                        ' store verbatim — the page renders the label itself via
                        ' shortcutToStr (a response WITHOUT keys is what made
                        ' the Ansel/Screenshot tiles show "Disabled").
                        If Not hkName.Equals("monitor", StringComparison.OrdinalIgnoreCase) AndAlso
                           Not hkName.Equals("dynamictoggle", StringComparison.OrdinalIgnoreCase) Then
                            StoreSection("hotkey:" & hkName.ToLowerInvariant(), ReadBody(req))
                            RaiseEvent LogLine("hotkey saved: " & hkName)
                            RaiseEvent HotkeySaved(hkName)
                        End If
                        WriteJson(res, 200, "{}")
                        Return
                    End If
                    If rawPath = "/ShadowPlay/v.1.0/Capture/ProcessInfo/" & (UInteger.MaxValue).ToString(CultureInfo.InvariantCulture) _
                       OrElse rawPath.StartsWith("/ShadowPlay/v.1.0/Capture/ProcessInfo/", StringComparison.OrdinalIgnoreCase) Then
                        ' getLastAppID's active-process lookup: {profileName,
                        ' processID, cmsID} at top level (app.js normalizes
                        ' profileName with ||"")
                        WriteJson(res, 200, "{""profileName"":"""",""processID"":0,""cmsID"":0}")
                        Return
                    End If
                    Select Case rawPath
                        Case "/ShadowPlay/v.1.0/Broadcast/Provider"
                            If method = "POST" Then StoreSection("broadcastProvider", ReadBody(req))
                            Dim bp As JsonObject = GetSection("broadcastProvider")
                            WriteJson(res, 200, If(bp.Count > 0, bp.ToJsonString(), "{""provider"":""""}"))
                        Case "/ShadowPlay/v.1.0/Broadcast/Enable"
                            If method = "POST" Then StoreSection("broadcastEnable", ReadBody(req))
                            Dim be As JsonObject = GetSection("broadcastEnable")
                            WriteJson(res, 200, If(be.Count > 0, be.ToJsonString(), "{""enable"":false}"))
                        Case "/ShadowPlay/v.1.0/DesktopCapture/Enable"
                            If method = "POST" Then StoreSection("desktopCapture", ReadBody(req))
                            Dim dc As JsonObject = GetSection("desktopCapture")
                            WriteJson(res, 200, If(dc.Count > 0, dc.ToJsonString(), "{""enable"":false}"))
                        Case "/ShadowPlay/v.1.0/Audio"
                            If method = "POST" Then StoreSection("audio", ReadBody(req))
                            Dim au As JsonObject = GetSection("audio")
                            WriteJson(res, 200, If(au.Count > 0, au.ToJsonString(), "{""mode"":""off""}"))
                        Case "/ShadowPlay/v.1.0/AudioSettings"
                            If method = "POST" Then StoreSection("audioSettings", ReadBody(req))
                            Dim aus As JsonObject = GetSection("audioSettings")
                            WriteJson(res, 200, If(aus.Count > 0, aus.ToJsonString(),
                                "{""systemVolumePercent"":100,""separateTracks"":false}"))
                        Case "/ShadowPlay/v.1.0/Record/Concurrency/Broadcast",
                             "/ShadowPlay/v.1.0/Record/Concurrency/Gamestream"
                            WriteJson(res, 200, "{""supported"":true}")
                        Case Else
                            If rawPath.EndsWith("/Support", StringComparison.OrdinalIgnoreCase) AndAlso
                               rawPath.StartsWith("/ShadowPlay/v.1.0/Indicator/", StringComparison.OrdinalIgnoreCase) Then
                                ' OSD indicators (fps / record / viewer): the
                                ' HUD-layout page gates each indicator on support
                                WriteJson(res, 200, "{""support"":true}")
                                Return
                            End If
                            If rawPath.StartsWith("/ShadowPlay/v.1.0/Indicator/", StringComparison.OrdinalIgnoreCase) AndAlso
                               rawPath.EndsWith("/Settings", StringComparison.OrdinalIgnoreCase) Then
                                Dim indId As String = rawPath.Substring("/ShadowPlay/v.1.0/Indicator/".Length)
                                indId = indId.Substring(0, indId.Length - "/Settings".Length)
                                If method = "POST" Then StoreSection("indicator:" & indId.ToLowerInvariant(), ReadBody(req))
                                Dim ind As JsonObject = GetSection("indicator:" & indId.ToLowerInvariant())
                                WriteJson(res, 200, If(ind.Count > 0, ind.ToJsonString(), "{""enable"":false,""position"":""TopRight""}"))
                                Return
                            End If
                            If rawPath.StartsWith("/NvCamera/v.1.0/", StringComparison.OrdinalIgnoreCase) Then
                        ' Ansel / Photo-mode panel preview data. The REAL values
                        ' arrive from the driver hook through socket.io channel
                        ' '/NvCamera/v.1.0/Notifications' as {type:...} payloads
                        ' (NvCameraAPI.js EmitNotification → io.emit — the POSTs
                        ' themselves only trigger the native side). Emit the
                        ' same payloads from our store of truth: the screen.
                        If rawPath.StartsWith("/NvCamera/v.1.0/Capture/GetResolutions", StringComparison.OrdinalIgnoreCase) Then
                            Dim b As System.Drawing.Rectangle = System.Windows.Forms.Screen.PrimaryScreen.Bounds
                            PushEvent("/NvCamera/v.1.0/Notifications",
                                      "{""type"":""gameResolution"",""width"":" & b.Width & ",""height"":" & b.Height & "}")
                            PushEvent("/NvCamera/v.1.0/Notifications",
                                      "{""type"":""highResResolutions"",""resolutions"":[" &
                                      "{""width"":" & b.Width & ",""height"":" & b.Height & "}," &
                                      "{""width"":1920,""height"":1080}," &
                                      "{""width"":2560,""height"":1440}," &
                                      "{""width"":3840,""height"":2160}]}")
                            WriteJson(res, 200, "{}")
                            Return
                        End If
                        WriteJson(res, 200, "{}")
                        Return
                    End If
                    ' ── Audio device settings (real WinMM devices) ──
                    If rawPath = "/ShadowPlay/v.1.0/Microphone" AndAlso method = "GET" Then
                        Dim items As New StringBuilder("[")
                        For i As Integer = 0 To MicrophoneCount() - 1
                            If i > 0 Then items.Append(",")
                            items.Append(MicrophoneSettingsJson(i))
                        Next
                        items.Append("]")
                        WriteJson(res, 200, items.ToString())
                        Return
                    End If
                    If rawPath = "/ShadowPlay/v.1.0/Microphone/Settings" AndAlso method = "GET" Then
                        WriteJson(res, 200, MicrophoneSettingsJson(0))
                        Return
                    End If
                    If rawPath.StartsWith("/ShadowPlay/v.1.0/Microphone/", StringComparison.OrdinalIgnoreCase) Then
                        Dim tail As String = rawPath.Substring("/ShadowPlay/v.1.0/Microphone/".Length)
                        Dim parts As String() = tail.Split("/"c)
                        Dim micIndex As Integer
                        If parts.Length = 2 AndAlso parts(1).Equals("Settings", StringComparison.OrdinalIgnoreCase) AndAlso
                           Integer.TryParse(parts(0), micIndex) Then
                            If method = "POST" Then StoreSection("mic:" & micIndex.ToString(CultureInfo.InvariantCulture), ReadBody(req))
                            WriteJson(res, 200, MicrophoneSettingsJson(micIndex))
                            Return
                        End If
                        If tail.Equals("PTT", StringComparison.OrdinalIgnoreCase) Then
                            WriteJson(res, 200, "{""enabled"":false}")
                            Return
                        End If
                    End If

                    ' ── In-game hook bridge (whitelisted games, e.g. MiSide) ──
                    If rawPath = "/ShadowPlay/v.1.0/Hook/Poll" AndAlso method = "GET" Then
                        Dim hk As JsonObject = GetSection("hook")
                        If hk.Count > 0 Then
                            WriteJson(res, 200, hk.ToJsonString())
                        Else
                            WriteJson(res, 200, "{""marker"":true,""text"":""NVIDIA Share - hooked"",""overlayVisible"":false}")
                        End If
                        Return
                    End If
                    If rawPath = "/ShadowPlay/v.1.0/Hook/Report" AndAlso method = "POST" Then
                        Dim body As String = ReadBody(req)
                        StoreSection("hookReport", body)
                        RaiseEvent LogLine("hook report: " & body)
                        WriteJson(res, 200, "{}")
                        Return
                    End If
                    If rawPath = "/ShadowPlay/v.1.0/Hook/Input" AndAlso method = "POST" Then
                        Dim body As String = ReadBody(req)
                        RaiseEvent HookInput(body)
                        WriteJson(res, 200, "{}")
                        Return
                    End If

                    ' ── Video-capture option lists (Recordings settings page) ──
                    ' The option set mirrors the Forms overlay's Video Capture
                    ' page (native + the 8 common resolutions, 30-240 FPS,
                    ' kbps bitrate range) so the osc page offers the same set.
                    If rawPath.StartsWith("/ShadowPlay/v.1.0/Resolutions", StringComparison.OrdinalIgnoreCase) Then
                        Dim native As System.Drawing.Rectangle = System.Windows.Forms.Screen.PrimaryScreen.Bounds
                        Dim listJson As String = "{" &
                            """resolutions"":[" &
                            "{""name"":""" & native.Width & "x" & native.Height & """,""supported"":true}," &
                            "{""name"":""3840x2160"",""supported"":true}," &
                            "{""name"":""3440x1440"",""supported"":true}," &
                            "{""name"":""2560x1440"",""supported"":true}," &
                            "{""name"":""2560x1080"",""supported"":true}," &
                            "{""name"":""1920x1080"",""supported"":true}," &
                            "{""name"":""1600x900"",""supported"":true}," &
                            "{""name"":""1366x768"",""supported"":true}," &
                            "{""name"":""1280x720"",""supported"":true}]}"
                        WriteJson(res, 200, listJson)
                        Return
                    End If
                    If rawPath = "/ShadowPlay/v.1.0/FrameRates" OrElse rawPath = "/ShadowPlay/v.1.0/Framerates" Then
                        WriteJson(res, 200, "{""framerates"":[30,60,120,144,240]}")
                        Return
                    End If
                    If rawPath.StartsWith("/ShadowPlay/v.1.0/Framerates/", StringComparison.OrdinalIgnoreCase) Then
                        WriteJson(res, 200, "{""framerate"":60}")
                        Return
                    End If
                    If rawPath.StartsWith("/ShadowPlay/v.1.0/BitRates/", StringComparison.OrdinalIgnoreCase) Then
                        WriteJson(res, 200, "{""min"":5000,""max"":100000,""default"":17000}")
                        Return
                    End If

                    ' ── Instant Replay settings (Forms overlay: 15-1200s) ──
                    ' replayLengthSeconds maps to config.json Recording.
                    ' replay_duration which the engine consumes at replay start.
                    If rawPath = "/ShadowPlay/v.1.0/InstantReplay/Settings" Then
                        If method = "POST" Then
                            Dim body As String = ReadBody(req)
                            StoreSection("instantReplaySettings", body)
                            ApplyReplayLengthToEngineConfig(body)
                            WriteJson(res, 200, "{}")
                        Else
                            WriteJson(res, 200, BuildInstantReplaySettings())
                        End If
                        Return
                    End If

                    RaiseEvent LogLine("unknown REST " & method & " " & rawPath & " → {}")
                            If method = "GET" OrElse method = "POST" Then
                                WriteJson(res, 200, "{}")
                            Else
                                WriteJson(res, 405, "{}")
                            End If
                    End Select
            End Select
        Catch ex As HttpListenerException
            ' client vanished mid-response — routine for long polls
        Catch ex As Exception
            RaiseEvent LogLine("route error " & rawPath & ": " & ex.Message)
        Finally
            Try : res.Close() : Catch : End Try
        End Try
    End Sub

    Private Shared Function SafeJson(provider As Func(Of String), fallback As String) As String
        Try
            Dim s As String = If(provider IsNot Nothing, provider(), Nothing)
            Return If(String.IsNullOrEmpty(s), fallback, s)
        Catch
            Return fallback
        End Try
    End Function

    ''' <summary>Preview hotkey bindings for /ShadowPlay/v.1.0/Hotkey/{name}.
    '     The response shape is {keys:[vk...]} — the page computes the label
    '     itself (shortcutToStr) and shows "Disabled" when keys is missing
    '     (measured). Preview bindings follow the real GFE defaults.</summary>
    Private Shared Function HotkeyPreviewJson(hk As String) As String
        Dim keys As String = "[0]"
        Select Case hk
            Case "OverlayToggle", "OpenShare"
                keys = "[18,90]"
            Case "Screenshot"
                keys = "[18,112]"
            Case "NvCameraUI"
                keys = "[18,113]"
            Case "ModsUI"
                keys = "[18,114]"
            Case "ModsToggle"
                keys = "[16,18,114]"
            Case "ModsPreset1"
                keys = "[18,116]"
            Case "ModsPreset2"
                keys = "[18,117]"
            Case "ModsPreset3"
                keys = "[18,118]"
            Case "ModsPresetCycle"
                keys = "[18,115]"
            Case "BroadcastToggle"
                keys = "[18,119]"
            Case "BroadcastPauseToggle"
                keys = "[16,18,119]"
            Case "DVRToggle"
                keys = "[16,18,121]"
            Case "RecordToggle"
                keys = "[18,120]"
            Case "RecordSave"
                keys = "[18,121]"
            Case "CameraToggle"
                keys = "[18,67]"
            Case "MicToggle"
                keys = "[18,77]"
            Case "FPS"
                keys = "[18,80]"
            Case "PTT"
                keys = "[86]"
            Case "CommentsToggle"
                keys = "[18,88]"
            Case "OverlayASwitch"
                keys = "[18,65]"
            Case "OverlayBSwitch"
                keys = "[18,66]"
            Case "OverlayCSwitch"
                keys = "[16,18,65]"
            Case "pmocoverlay"
                keys = "[18,82]"
            Case "pmocoverlaycycle"
                keys = "[16,18,82]"
            Case "pmocresetaveragemetrics"
                keys = "[16,18,80]"
            Case "pmocloggingtoggle"
                keys = "[16,18,76]"
        End Select
        Return "{""keys"":" & keys & "}"
    End Function

    ' ── record settings ⇆ engine config.json ──────────────────
    ' The engine's OverlayConfig.ApplyUnifiedToCaptureSettings reads
    ' config.json → Recording.current.{fps, bitrate(kbps), width, height,
    ' use_native_resolution} at every recording start. The osc's
    ' Record/Settings page therefore reads/writes THAT section directly —
    ' the same place the Forms overlay writes — so saved values reach the
    ' real recorder without a second path.

    Private Shared ReadOnly RecordSettingsLock As New Object()

    Private Function BuildRecordSettingsFromEngineConfig() As String
        Dim fps As Integer = 60
        Dim bitrateKbps As Integer = 17000
        Dim width As Integer = 0
        Dim height As Integer = 0
        Try
            Dim path As String = AppConfigShared.ConfigPath()
            If File.Exists(path) Then
                SyncLock RecordSettingsLock
                    Dim root As JsonObject = JsonNode.Parse(File.ReadAllText(path)).AsObject()
                    Dim rec As JsonObject = TryCast(root("Recording"), JsonObject)
                    Dim cur As JsonObject = If(TryCast(rec?.Item("current"), JsonObject), New JsonObject())
                    If cur("fps") IsNot Nothing Then fps = CInt(cur("fps"))
                    If cur("bitrate") IsNot Nothing Then bitrateKbps = CInt(cur("bitrate"))
                    If cur("width") IsNot Nothing Then width = CInt(cur("width"))
                    If cur("height") IsNot Nothing Then height = CInt(cur("height"))
                End SyncLock
            End If
        Catch
        End Try
        If width <= 0 OrElse height <= 0 Then
            Dim b As System.Drawing.Rectangle = System.Windows.Forms.Screen.PrimaryScreen.Bounds
            width = b.Width
            height = b.Height
        End If
        Return "{""quality"":""custom"",""resolution"":""" & width & "x" & height &
               """,""framerate"":" & fps.ToString(CultureInfo.InvariantCulture) &
               ",""bitrateBps"":" & (bitrateKbps * 1000).ToString(CultureInfo.InvariantCulture) & "}"
    End Function

    Private Sub ApplyRecordSettingsToEngineConfig(body As String)
        Try
            Dim posted As JsonObject = TryCast(JsonNode.Parse(body), JsonObject)
            If posted Is Nothing Then Return
            Dim fps As Integer? = Nothing
            Dim bitrateKbps As Integer? = Nothing
            Dim width As Integer? = Nothing
            Dim height As Integer? = Nothing
            Dim native As Boolean? = Nothing
            If posted("framerate") IsNot Nothing Then fps = CInt(posted("framerate"))
            If posted("bitrateBps") IsNot Nothing Then bitrateKbps = CInt(Math.Max(1, CDbl(posted("bitrateBps").ToString()) / 1000.0))
            If posted("resolution") IsNot Nothing Then
                Dim res As String = posted("resolution").ToString().Trim()
                If res.Length > 0 AndAlso res.ToLowerInvariant() <> "native" Then
                    Dim parts As String() = res.Split("x"c, "X"c, "×"c)
                    Dim w As Integer, h As Integer
                    If parts.Length = 2 AndAlso Integer.TryParse(parts(0).Trim(), w) AndAlso Integer.TryParse(parts(1).Trim(), h) Then
                        width = w
                        height = h
                        native = False
                    End If
                Else
                    native = True
                End If
            End If
            Dim path As String = AppConfigShared.ConfigPath()
            Dim root As JsonObject
            If File.Exists(path) Then
                root = JsonNode.Parse(File.ReadAllText(path)).AsObject()
            Else
                root = New JsonObject()
            End If
            SyncLock RecordSettingsLock
                Dim rec As JsonObject = TryCast(root("Recording"), JsonObject)
                If rec Is Nothing Then
                    rec = New JsonObject()
                    root("Recording") = rec
                End If
                Dim cur As JsonObject = TryCast(rec("current"), JsonObject)
                If cur Is Nothing Then
                    cur = New JsonObject()
                    rec("current") = cur
                End If
                If fps.HasValue AndAlso fps.Value > 0 AndAlso fps.Value <= 240 Then cur("fps") = fps.Value
                If bitrateKbps.HasValue AndAlso bitrateKbps.Value > 0 Then cur("bitrate") = bitrateKbps.Value
                If native.HasValue Then cur("use_native_resolution") = native.Value
                If width.HasValue Then cur("width") = width.Value
                If height.HasValue Then cur("height") = height.Value
                Dim dir As String = IO.Path.GetDirectoryName(path)
                If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
                File.WriteAllText(path, root.ToJsonString(
                    New System.Text.Json.JsonSerializerOptions With {.WriteIndented = True}))
            End SyncLock
            RaiseEvent LogLine("record settings applied to engine config")
        Catch ex As Exception
            RaiseEvent LogLine("record settings apply failed: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Instant Replay settings: reads the saved section first and
    '     fills replayLengthSeconds from the engine config default
    '     (Forms overlay allows 15-1200 seconds).</summary>
    Private Function BuildInstantReplaySettings() As String
        Dim replaySec As Integer = 300
        Dim fps As Integer = 60
        Dim bitrateKbps As Integer = 17000
        Try
            Dim path As String = AppConfigShared.ConfigPath()
            If File.Exists(path) Then
                SyncLock RecordSettingsLock
                    Dim root As JsonObject = JsonNode.Parse(File.ReadAllText(path)).AsObject()
                    Dim rec As JsonObject = TryCast(root("Recording"), JsonObject)
                    If rec?.Item("replay_duration") IsNot Nothing Then replaySec = CInt(rec("replay_duration"))
                    Dim cur As JsonObject = TryCast(rec?.Item("current"), JsonObject)
                    If cur?.Item("fps") IsNot Nothing Then fps = CInt(cur("fps"))
                    If cur?.Item("bitrate") IsNot Nothing Then bitrateKbps = CInt(cur("bitrate"))
                End SyncLock
            End If
        Catch
        End Try
        Dim stored As JsonObject = GetSection("instantReplaySettings")
        If stored.Count > 0 Then Return stored.ToJsonString()
        Dim w As Integer = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width
        Dim h As Integer = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height
        Return "{""replayLengthSeconds"":" & replaySec.ToString(CultureInfo.InvariantCulture) &
               ",""quality"":""custom"",""resolution"":""" & w & "x" & h &
               """,""framerate"":" & fps.ToString(CultureInfo.InvariantCulture) &
               ",""bitrateBps"":" & (bitrateKbps * 1000).ToString(CultureInfo.InvariantCulture) & "}"
    End Function

    ''' <summary>Maps the osc replay payload to the engine's
    '     Recording.replay_duration (consumed by ApplyUnifiedToCaptureSettings
    '     — the Forms overlay allows 15-1200 seconds; clamp to that).</summary>
    Private Sub ApplyReplayLengthToEngineConfig(body As String)
        Try
            Dim posted As JsonObject = TryCast(JsonNode.Parse(body), JsonObject)
            If posted Is Nothing OrElse posted("replayLengthSeconds") Is Nothing Then Return
            Dim secs As Integer = CInt(posted("replayLengthSeconds"))
            secs = Math.Max(15, Math.Min(1200, secs))
            Dim path As String = AppConfigShared.ConfigPath()
            Dim root As JsonObject
            If File.Exists(path) Then
                root = JsonNode.Parse(File.ReadAllText(path)).AsObject()
            Else
                root = New JsonObject()
            End If
            SyncLock RecordSettingsLock
                Dim rec As JsonObject = TryCast(root("Recording"), JsonObject)
                If rec Is Nothing Then
                    rec = New JsonObject()
                    root("Recording") = rec
                End If
                rec("replay_duration") = secs
                Dim dir As String = IO.Path.GetDirectoryName(path)
                If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
                File.WriteAllText(path, root.ToJsonString(
                    New System.Text.Json.JsonSerializerOptions With {.WriteIndented = True}))
            End SyncLock
            RaiseEvent LogLine("replay_duration applied to engine config: " & secs.ToString(CultureInfo.InvariantCulture) & "s")
        Catch ex As Exception
            RaiseEvent LogLine("replay settings apply failed: " & ex.Message)
        End Try
    End Sub

    ' ── persistent settings store ─────────────────────────────
    ' The page is the shape authority: POST bodies are stored VERBATIM
    ' (merged per section) under Data\osc-settings.json and GETs return
    ' the stored section — every settings screen (keyboard shortcuts,
    ' recordings, audio, HUD layout, broadcast) survives restarts.
    ' Mirrors the real controller's split (index.js routes → the
    ' ShadowPlay API store) without the NvBackend native module.

    Private Shared ReadOnly _settingsLock As New Object()
    Private Shared _settingsNode As JsonObject
    Private Shared _settingsLoaded As Boolean

    Private Shared Function LoadSettings() As JsonObject
        If _settingsLoaded Then Return _settingsNode
        SyncLock _settingsLock
            If Not _settingsLoaded Then
                Try
                    Dim p As String = AppLayout.P("Data", "osc-settings.json")
                    If File.Exists(p) Then
                        _settingsNode = JsonNode.Parse(File.ReadAllText(p)).AsObject()
                    End If
                Catch
                End Try
                _settingsNode = If(_settingsNode, New JsonObject())
                _settingsLoaded = True
            End If
            Return _settingsNode
        End SyncLock
    End Function

    Private Shared Sub SaveSettings()
        Try
            Dim p As String = AppLayout.P("Data", "osc-settings.json")
            Dim dir As String = IO.Path.GetDirectoryName(p)
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
            SyncLock _settingsLock
                File.WriteAllText(p, _settingsNode.ToJsonString(
                    New System.Text.Json.JsonSerializerOptions With {.WriteIndented = True}))
            End SyncLock
        Catch
        End Try
    End Sub

    ''' <summary>Returns (creating on first use) a named section of the
    '     settings store. Sections are keyed by endpoint family; dynamic
    '     segments (per hotkey / per indicator id) live in their own key.</summary>
    Private Shared Function GetSection(name As String) As JsonObject
        Dim root As JsonObject = LoadSettings()
        SyncLock _settingsLock
            Dim s As JsonNode = root(name)
            If s Is Nothing OrElse TypeOf s Is JsonObject = False Then
                s = New JsonObject()
                root(name) = s
            End If
            Return CType(s, JsonObject)
        End SyncLock
    End Function

    ''' <summary>Merges a POSTed body over a section and persists. The body
    '     shape is whatever the page sent — stored verbatim.</summary>
    Private Shared Sub StoreSection(name As String, body As String)
        Try
            If String.IsNullOrWhiteSpace(body) Then Return
            Dim sec As JsonObject = GetSection(name)
            Dim incoming As JsonNode = JsonNode.Parse(body)
            If TypeOf incoming Is JsonObject Then
                SyncLock _settingsLock
                    For Each kv As KeyValuePair(Of String, JsonNode) In CType(incoming, JsonObject)
                        sec(kv.Key) = If(kv.Value IsNot Nothing, kv.Value.DeepClone(), Nothing)
                    Next
                End SyncLock
                SaveSettings()
            End If
        Catch
        End Try
    End Sub

    Private ReadOnly _responsesDir As String = IO.Path.Combine(AppContext.BaseDirectory, "responses")

    ''' <summary>Finds a captured real-response file for a path. Matches the
    '     exact file first, then the longest prefix (paths with per-id
    '     segments fall back to the generic capture). Returns "" when the
    '     catalog has no answer for this path. The catalog = REAL responses
    '     captured live from the installed GFE controller (FULL MODE).</summary>
    Private Function FindCatalogResponse(rawPath As String) As String
        Try
            Dim name As String = rawPath.TrimStart("/"c).Replace("/", "_")
            If name.Length = 0 Then Return ""
            Dim direct As String = IO.Path.Combine(_responsesDir, name & ".json")
            If File.Exists(direct) Then Return direct

            ' longest-prefix fallback: /Applications/v.1.0/123/state →
            ' Applications_v.1.0_123_state → ... → Applications_v.1.0
            Dim prefix As String = name
            While prefix.Contains("_"c)
                Dim cut As Integer = prefix.LastIndexOf("_"c)
                If cut <= 0 Then Exit While
                prefix = prefix.Substring(0, cut)
                Dim candidate As String = IO.Path.Combine(_responsesDir, prefix & ".json")
                If File.Exists(candidate) Then Return candidate
            End While
        Catch
        End Try
        Return ""
    End Function

    Private Function HasCookie(req As HttpListenerRequest) As Boolean
        Dim provided As String = req.Headers("X_LOCAL_SECURITY_COOKIE")
        If String.IsNullOrEmpty(provided) Then
            provided = req.QueryString("X_LOCAL_SECURITY_COOKIE")
        End If
        Return String.Equals(provided, _secret, StringComparison.Ordinal)
    End Function

    ' ── static files ───────────────────────────────────────────

    Private Shared Function IsStaticPath(rawPath As String) As Boolean
        If rawPath = "/" Then Return True
        Dim ext As String = IO.Path.GetExtension(rawPath)
        Select Case ext
            Case ".html", ".js", ".json", ".png", ".svg", ".gif", ".jpg", ".css",
                 ".woff", ".woff2", ".ttf", ".ico", ".map", ".txt"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Sub ServeStatic(rawPath As String, res As HttpListenerResponse)
        Dim rel As String = If(rawPath = "/", "index.html", rawPath.TrimStart("/"c))
        Dim full As String = IO.Path.GetFullPath(IO.Path.Combine(_oscRoot, rel))
        If Not full.StartsWith(_oscRoot, StringComparison.OrdinalIgnoreCase) OrElse Not File.Exists(full) Then
            WriteText(res, 404, "not found")
            Return
        End If
        Dim ext As String = IO.Path.GetExtension(full).ToLowerInvariant()
        Dim mime As String
        Select Case ext
            Case ".html" : mime = "text/html; charset=UTF-8"
            Case ".js" : mime = "application/javascript; charset=UTF-8"
            Case ".json" : mime = "application/json; charset=UTF-8"
            Case ".css" : mime = "text/css; charset=UTF-8"
            Case ".png" : mime = "image/png"
            Case ".svg" : mime = "image/svg+xml"
            Case ".gif" : mime = "image/gif"
            Case ".jpg" : mime = "image/jpeg"
            Case ".woff" : mime = "font/woff"
            Case ".woff2" : mime = "font/woff2"
            Case ".ttf" : mime = "font/ttf"
            Case ".ico" : mime = "image/x-icon"
            Case ".map" : mime = "application/json; charset=UTF-8"
            Case Else : mime = "application/octet-stream"
        End Select
        res.ContentType = mime
        res.ContentLength64 = New FileInfo(full).Length
        Using fs As FileStream = File.OpenRead(full)
            fs.CopyTo(res.OutputStream)
        End Using
    End Sub

    ' ── engine.io v3 polling + socket.io v2 (golden-verified) ──

    Private Sub HandleSocketIo(req As HttpListenerRequest, res As HttpListenerResponse)
        Dim q As NameValueCompat = New NameValueCompat(req.QueryString)
        Dim method As String = req.HttpMethod

        If Not HasCookie(req) Then
            ' GFE-equivalent servers reject unauthorized sockets outright.
            RaiseEvent LogLine("sio REJECT 401 " & method)
            WriteText(res, 401, "unauthorized")
            Return
        End If

        Dim sid As String = q.GetValue("sid")

        If String.IsNullOrEmpty(sid) Then
            ' handshake (GET only)
            If method <> "GET" Then
                RaiseEvent LogLine("sio REJECT bad-handshake " & method)
                WriteText(res, 400, "bad handshake")
                Return
            End If
            Dim newSid As String = Guid.NewGuid().ToString("N").Substring(0, 16)
            Dim sess As New SioSession()
            _sessions(newSid) = sess
            Dim open As String = OscWire.OpenPacket(newSid, EffectivePingIntervalMs, DefaultPingTimeoutMs)
            WritePolling(res, req, OscWire.EncodePayload(New String() {open}))
            RaiseEvent LogLine("engine.io handshake sid=" & newSid)
            Return
        End If

        Dim session As SioSession = Nothing
        If Not _sessions.TryGetValue(sid, session) Then
            RaiseEvent LogLine("sio REJECT unknown-sid " & method & " sid=" & sid.Substring(0, Math.Min(8, sid.Length)))
            WriteText(res, 400, "unknown sid")
            Return
        End If

        If method = "POST" Then
            Dim body As String = ReadBody(req)
            session.LastSeen = DateTime.UtcNow
            For Each packet As String In OscWire.DecodePayload(body)
                HandleClientPacket(session, packet)
            Next
            ' POST ack: the 1.x browser client feeds the POST response into
            ' its parser, so the body must decode to ZERO packets — an empty
            ' octet-stream body does exactly that for both client families.
            WriteBinary(res, 200, New Byte() {})
            Return
        End If

        ' GET = long-poll
        session.LastSeen = DateTime.UtcNow
        Dim deliver As String
        SyncLock session.Lock
            If Not session.Connected Then
                session.Connected = True
                deliver = OscWire.EncodePayload(New String() {OscWire.SocketConnectPacket()})
            ElseIf session.Pending.Count > 0 Then
                deliver = OscWire.EncodePayload(session.Pending.ToArray())
                session.Pending.Clear()
            Else
                deliver = Nothing
            End If
        End SyncLock

        If deliver IsNot Nothing Then
            WritePolling(res, req, deliver)
            Return
        End If

        ' hold the poll until PushEvent or timeout (then NOOP "1:6" —
        ' keeps the client's poll cycle alive, matches the golden server).
        Dim waiter As New TaskCompletionSource(Of Boolean)(TaskCreationOptions.RunContinuationsAsynchronously)
        Dim deliverBatch As String = Nothing
        SyncLock session.Lock
            ' re-check under lock: a push may have arrived meanwhile
            If session.Pending.Count > 0 Then
                deliverBatch = OscWire.EncodePayload(session.Pending.ToArray())
                session.Pending.Clear()
            Else
                session.Waiter = waiter
            End If
        End SyncLock
        If deliverBatch IsNot Nothing Then
            WriteText(res, 200, deliverBatch)
            Return
        End If

        Try
            Dim signaled As Boolean = waiter.Task.Wait(20000)
            SyncLock session.Lock
                session.Waiter = Nothing
                If session.Pending.Count > 0 Then
                    deliverBatch = OscWire.EncodePayload(session.Pending.ToArray())
                    session.Pending.Clear()
                End If
            End SyncLock
            If deliverBatch Is Nothing Then deliverBatch = OscWire.EngineNoop
            If signaled Then
                WritePolling(res, req, deliverBatch)
            Else
                ' timed out: noop delivered in the request's own framing
                If WantsText(req) Then
                    WriteText(res, 200, "1:" & OscWire.EngineNoop)
                Else
                    WriteBinary(res, 200, OscWire.EncodePayloadBinaryOne(OscWire.EngineNoop))
                End If
            End If
        Catch ex As Exception
            Try : WriteBinary(res, 200, New Byte() {}) : Catch : End Try
        End Try
    End Sub

    Private Sub HandleClientPacket(session As SioSession, packet As String)
        If String.IsNullOrEmpty(packet) Then Return
        If packet = OscWire.EnginePing Then
            ' engine.io v3: CLIENT pings, server PONGs on the next poll
            SyncLock session.Lock
                session.Pending.Add(OscWire.EnginePong)
                If session.Waiter IsNot Nothing Then
                    session.Waiter.TrySetResult(True)
                End If
            End SyncLock
            Return
        End If
        If packet = OscWire.SocketConnectPacket() Then
            ' v2 clients do not send this on the default ns; tolerate anyway
            SyncLock session.Lock
                session.Connected = True
            End SyncLock
            Return
        End If
        If packet.StartsWith("41", StringComparison.Ordinal) Then
            RaiseEvent LogLine("socket.io client DISCONNECT")
            Return
        End If
        Dim ev As OscWire.SocketEvent = OscWire.TryParseSocketEvent(packet)
        If ev IsNot Nothing Then
            RaiseEvent ClientEventReceived(ev.Channel, ev.PayloadJson)
        Else
            RaiseEvent LogLine("unhandled sio packet: " & packet.Substring(0, Math.Min(60, packet.Length)))
        End If
    End Sub

    ' ── host → page push ───────────────────────────────────────

    ''' <summary>A hotkey binding was saved by the page — the host re-applies
    '     its real RegisterHotKey bindings (OscHotkeyApplier).</summary>
    Public Event HotkeySaved(name As String)

    ''' <summary>Raw input event from the in-game hook mod (JSON:
    '     {type,x,y,button,key}) — the form dispatches it into the page.</summary>
    Public Event HookInput(bodyJson As String)

    ''' <summary>Overlay visibility published to the in-game hook mod's poll.
    '     Written by OscHostForm on every open/close state change.</summary>
    Public Shared Sub SetHookOverlayState(visible As Boolean)
        Try
            Dim sec As JsonObject = GetSection("hook")
            sec("overlayVisible") = visible
            SaveSettings()
        Catch
        End Try
    End Sub

    ''' <summary>Stored VK array for an action name from the settings store,
    '     falling back to the preview bindings. Consumed by OscHotkeyApplier
    '     so UI-saved bindings become REAL global hotkeys.</summary>
    Public Shared Function GetStoredHotkeyKeys(hk As String) As Integer()
        Dim out As New List(Of Integer)()
        Try
            Dim sec As JsonObject = GetSection("hotkey:" & hk.ToLowerInvariant())
            Dim arr As JsonArray = TryCast(sec("keys"), JsonArray)
            If arr IsNot Nothing Then
                For Each n As JsonNode In arr
                    out.Add(CInt(n))
                Next
                Return out.ToArray()
            End If
        Catch
        End Try
        Try
            Dim jn As JsonNode = JsonNode.Parse(HotkeyPreviewJson(hk))
            Dim arr2 As JsonArray = TryCast(jn("keys"), JsonArray)
            If arr2 IsNot Nothing Then
                For Each n As JsonNode In arr2
                    out.Add(CInt(n))
                Next
            End If
        Catch
        End Try
        Return out.ToArray()
    End Function

    Public Sub PushEvent(channel As String, payloadJson As String)
        Dim packet As String = OscWire.SocketEventPacket(channel, payloadJson)
        Dim delivered As Integer = 0
        For Each kv As KeyValuePair(Of String, SioSession) In _sessions
            Dim s As SioSession = kv.Value
            SyncLock s.Lock
                s.Pending.Add(packet)
                If s.Waiter IsNot Nothing Then
                    s.Waiter.TrySetResult(True)
                End If
            End SyncLock
            delivered += 1
        Next
        If delivered = 0 Then
            RaiseEvent LogLine("push queued (no page connected): " & channel)
        End If
    End Sub

    ' ── polling response modes ─────────────────────────────────

    ''' <summary>engine.io v3 polling responses come in TWO modes and the
    '     1.x browser client REQUIRES the right one (measured):
    '       b64=1 in the query → text/plain, "<len>:<packet>" framing.
    '       otherwise → application/octet-stream, binary framing
    '         (0x00 | ascii length | 0xFF | utf8) — with a text/plain body
    '         the client's onLoad feeds the literal "ok" into its parser and
    '         dies with parser error, producing the handshake storm.</summary>
    Private Shared Function WantsText(req As HttpListenerRequest) As Boolean
        Return req.QueryString("b64") = "1"
    End Function

    Private Sub WritePolling(res As HttpListenerResponse, req As HttpListenerRequest, textPayload As String)
        If WantsText(req) Then
            WriteText(res, 200, textPayload)
        Else
            Dim packets As List(Of String) = OscWire.DecodePayload(textPayload)
            Dim rebuilt As New List(Of String)
            rebuilt.AddRange(packets)
            WriteBinary(res, 200, OscWire.EncodePayloadBinary(rebuilt))
        End If
    End Sub

    Private Shared Sub WriteBinary(res As HttpListenerResponse, status As Integer, body As Byte())
        Try
            res.StatusCode = status
            res.ContentType = "application/octet-stream"
            res.ContentLength64 = body.Length
            If body.Length > 0 Then
                res.OutputStream.Write(body, 0, body.Length)
            End If
        Catch
            ' client already gone
        End Try
    End Sub

    ' ── plumbing ───────────────────────────────────────────────

    Private Shared Function ReadBody(req As HttpListenerRequest) As String
        If req.HasEntityBody Then
            Using sr As New StreamReader(req.InputStream, Encoding.UTF8)
                Return sr.ReadToEnd()
            End Using
        End If
        Return ""
    End Function

    Private Shared Sub WriteJson(res As HttpListenerResponse, status As Integer, body As String)
        res.StatusCode = status
        res.ContentType = "application/json; charset=UTF-8"
        WriteText(res, status, body)
    End Sub

    Private Shared Sub WriteText(res As HttpListenerResponse, status As Integer, body As String)
        Try
            res.StatusCode = status
            res.ContentType = If(res.ContentType, "text/plain; charset=UTF-8")
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(body)
            res.ContentLength64 = bytes.Length
            res.OutputStream.Write(bytes, 0, bytes.Length)
        Catch
            ' client already gone
        End Try
    End Sub

    ''' <summary>HttpListenerResponse.ContentType setter throws when headers
    '     were already sent; this shim guards the WriteText path.</summary>
    Private Class NameValueCompat
        Private ReadOnly _inner As System.Collections.Specialized.NameValueCollection
        Public Sub New(inner As System.Collections.Specialized.NameValueCollection)
            _inner = inner
        End Sub
        Public Function GetValue(name As String) As String
            Return _inner(name)
        End Function
    End Class

    Public Sub Dispose() Implements IDisposable.Dispose
        StopServer()
        Try : _cts.Dispose() : Catch : End Try
    End Sub

End Class
