' OscHostForm.vb — the overlay window: a fullscreen borderless WinForms
' host for WebView2 running the osc web app, plus the tray fallback.
'
' Window model (mirrors GFE's OSC, adapted to WinForms):
'   - the window ALWAYS covers the primary screen (TopMost, no taskbar,
'     no Alt-Tab via WS_EX_TOOLWINDOW)
'   - when the menu is closed: Opacity=0 and WM_NCHITTEST answers
'     HTTRANSPARENT everywhere → the game never notices it
'   - when open: Opacity=1; click-through outside the page's displayRects
'     (the page reports its interactive regions via QUERY_OSC_SET_DISPLAY_RECTS)
'
' Hotkeys: NONE in M1 (ownership stays with the Forms overlay; see
' PROTOCOL-MATRIX.md / hotkey arbitration note). Toggle paths: the hub's
' open_overlay message, the tray menu, or the page itself.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Public Class OscHostForm
    Inherits Form

    Private Const WM_NCHITTEST As Integer = &H84
    Private Const HTCLIENT As Integer = 1
    Private Const HTTRANSPARENT As Integer = -1
    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000
    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000
    Private Const LWA_ALPHA As Integer = 2
    Private Const HWND_BOTTOM As Integer = 1
    Private Const HWND_TOPMOST As Integer = -1
    Private Const WS_EX_NOACTIVATE As Integer = &H8000000
    Private Const SWP_SHOWWINDOW As Integer = &H40
    Private Const SWP_NOMOVE As Integer = 2
    Private Const SWP_NOSIZE As Integer = 1
    Private Const SWP_NOACTIVATE As Integer = &H10

    <DllImport("user32.dll")>
    Private Shared Function SetWindowPos(hWnd As IntPtr, after As IntPtr, x As Integer, y As Integer, w As Integer, h As Integer, flags As Integer) As Boolean
    End Function

    Private WithEvents _webView As Microsoft.Web.WebView2.WinForms.WebView2
    Private _cdpCapture As HookCdpCapture
    Private _inputReader As HookInputReader
    Private _server As OscControllerServer
    Private _client As OscEngineClient
    Private _bridge As CefQueryBridge
    Private _storage As SharedStorageStore
    Private _hotkeys As OscHotkeys
    Private _applier As OscHotkeyApplier
    Private Const ToggleHotkeyId As Integer = 1
    Private _oscReady As Boolean
    Private _overlayOpen As Boolean
    Private _userOpen As Boolean
    Private _menuInputEnabled As Boolean
    Private _webviewReady As Boolean
    Private _closingForExit As Boolean

    ' latest engine truth for /state + the page
    Private _recording As Boolean
    Private _replayBuffering As Boolean
    Private _elapsedSec As Integer

    Public Sub New()
        Text = "NVIDIA Share"
        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Dim bounds As System.Drawing.Rectangle = Screen.PrimaryScreen.Bounds
        Location = bounds.Location
        Size = bounds.Size
        TopMost = True

        ' never in taskbar/Alt-Tab; layered+alpha255 so the WS_EX_TRANSPARENT
        ' click-through toggle (SetOverlayOpen) composites correctly; boot
        ' state = closed → start click-through
        Dim ex As Integer = CInt(GetWindowLong(Handle, GWL_EXSTYLE))
        SetWindowLong(Handle, GWL_EXSTYLE, New IntPtr((ex Or WS_EX_TOOLWINDOW Or WS_EX_LAYERED Or WS_EX_TRANSPARENT) And Not WS_EX_APPWINDOW))
        SetLayeredWindowAttributes(Handle, 0, 255, LWA_ALPHA)

        ' The form is NEVER auto-shown (Program.vb pumps an empty
        ' ApplicationContext; the tray icon carries the UI). While hidden
        ' its rect STAYS at the primary-screen bounds set above: Chromium
        ' derives window.screen from the monitor at the HWND rect, and a
        ' -32000 park made it snap to the portrait side monitor (measured:
        ' screen=1080x1920) which laid the osc menu out for the wrong
        ' display. SetOverlayOpen(True) does the first Show().
        Start()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        ' init runs in the ctor (Start) — the form may never be shown
    End Sub

    ' (ParkOffScreen/MoveOnScreen removed: moving the window breaks the
    '  WebView2 alpha surface — see the ctor comment.)

    ' ── lifecycle ──────────────────────────────────────────────

    Private Sub Start()
        ' this overlay is built ON TOP of an installed GFE — the machine
        ' must have GFE (its recorder service + driver context are what the
        ' FULL MODE capability answers describe).
        '   GFE installed → we launch NVIDIA Share for it; the REAL
        '   ShadowPlay overlay owns Alt+Z as the daily driver; OUR overlay
        '   stays reachable via tray/hub.
        '   No GFE → ours registers Alt+Z itself (limited feature set).
        If NvShadowPlayRecorder.IsAvailable() Then
            Log("GFE detected — real ShadowPlay overlay owns Alt+Z (ours = tray/hub)")
            EnsureRealShareRunning()
        Else
            Log("GFE not installed — our overlay takes Alt+Z (limited features)")
            _hotkeys = New OscHotkeys(AppConfigShared.ReadString("Hotkeys", "ToggleOverlay", "Alt+Z"), ToggleHotkeyId)
            AddHandler _hotkeys.LogLine, Sub(m) Log(m)
            _hotkeys.TryRegister(Handle)
        End If

        ' (tray icon removed — owner directive "ปิด Tray"; the ApplicationContext
        '  pump keeps the process alive and Alt+Z remains the toggle)
        StartStack()
    End Sub

    ''' <summary>Component management: the installed real overlay
    '     (NVIDIA Share.exe) is part of OUR stack — launch it when absent,
    '     exactly like the supervisor manages NVIDIA Capture.exe.</summary>
    Private Sub EnsureRealShareRunning()
        Try
            If IO.File.Exists(NvShadowPlayRecorder.GfeShareExe) AndAlso
               Process.GetProcessesByName("NVIDIA Share").Length = 0 Then
                Process.Start(New ProcessStartInfo With {
                    .FileName = NvShadowPlayRecorder.GfeShareExe,
                    .UseShellExecute = True})
                Log("NVIDIA Share.exe launched (real overlay component)")
            End If
        Catch ex As Exception
            Log("real overlay launch failed: " & ex.Message)
        End Try
    End Sub

    Private Sub StartStack()
        Try
            _storage = New SharedStorageStore()

            ' engine supervision: reuse the SAME supervisor source the Forms
            ' overlay uses (linked, not forked) so NVIDIA Capture.exe comes
            ' up for the Record tile even without the Forms overlay.
            EngineProcessSupervisor.EnsureEngineRunning()

            Dim oscRoot As String = ResolveOscRoot()
            If String.IsNullOrEmpty(oscRoot) Then
                Log("osc folder not found next to exe — cannot start")
                Return
            End If

            _server = New OscControllerServer(oscRoot)
            AddHandler _server.LogLine, Sub(m) Log(m)
            AddHandler _server.RestRequested, Sub(m, p) Log("REST " & m & " " & p)
            AddHandler _server.ClientEventReceived, AddressOf OnSocketEvent
            AddHandler _server.UiReady, Sub()
                                            _oscReady = True
                                            Log("osc uiReady — overlay UI is up")
                                        End Sub
            _server.StateProvider = AddressOf BuildStateJson
            _server.RecordPathsProvider = AddressOf BuildRecordPathsJson
            _server.RecordEnabledHandler = AddressOf OnRecordEnableRequested
            _server.LanguageProvider = Function() "{""language"":""en-US""}"
            AddHandler _server.ScreenshotRequested, Sub() BeginInvoke(Sub() CaptureScreenshotNow())
            _server.Start()

            _bridge = New CefQueryBridge(_storage)
            _bridge.Configure(_server.Port, _server.Secret)
            AddHandler _bridge.LogLine, Sub(m) Log(m)
            AddHandler _bridge.OpenOsc, Sub(input) BeginInvoke(Sub() SetOverlayOpen(True, pushToPage:=False, userInitiated:=False))
            AddHandler _bridge.CloseOsc, Sub() BeginInvoke(Sub() SetOverlayOpen(False, pushToPage:=False))

            _client = New OscEngineClient()
            AddHandler _client.LogLine, Sub(m) Log(m)
            AddHandler _client.OpenOverlayRequested, Sub() BeginInvoke(Sub() ToggleOverlay())
            AddHandler _client.EngineReady, Sub() Log("engine announced ready")
            AddHandler _client.EngineStateChanged, AddressOf OnEngineStateChanged
            AddHandler _client.RecordingProgress, Sub(sec, fields) BeginInvoke(Sub() _elapsedSec = sec)
            AddHandler _client.RecordingSaved, Sub(path) BeginInvoke(Sub() OnRecordingSaved(path))
            AddHandler _client.RecordingError, Sub(msg) BeginInvoke(Sub() OnRecordingError(msg))
            AddHandler _client.RecordStartedConfirmed, Sub() BeginInvoke(Sub()
                                                                            _recording = True
                                                                            PushNotification("Recording started", "RECORD_START accepted by engine")
                                                                        End Sub)
            AddHandler _client.RecordFailed, Sub(reason) BeginInvoke(Sub()
                                                                         Log("record failed: " & reason)
                                                                         PushNotification("Recording failed", reason)
                                                                     End Sub)
            _client.Connect()

            ' apply the page-saved hotkey bindings as REAL global hotkeys
            ' (Keyboard-shortcuts page → settings store → RegisterHotKey);
            ' re-applied on every save via HotkeySaved
            _applier = New OscHotkeyApplier()
            AddHandler _applier.LogLine, Sub(m) Log(m)
            AddHandler _applier.ActionTriggered, AddressOf OnHotkeyAction
            AddHandler _server.HotkeySaved, Sub(name) BeginInvoke(Sub() _applier.Reapply(Handle))
            AddHandler _server.HookInput, Sub(body) BeginInvoke(Sub() OnHookInput(body))
            _applier.Reapply(Handle)

            ' in-game hook bridge: write port+secret next to every CONSENTED
            ' whitelisted game (hook-whitelist.json) so the in-game mod can
            ' poll the controller (MiSide = first whitelisted game)
            Try
                Dim wlPath As String = AppLayout.P("Data", "hook-whitelist.json")
                If File.Exists(wlPath) Then
                    Dim wl As System.Text.Json.Nodes.JsonObject =
                        System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(wlPath)).AsObject()
                    If wl("games") IsNot Nothing Then
                        For Each g As System.Text.Json.Nodes.JsonNode In wl("games").AsArray()
                            Dim obj As System.Text.Json.Nodes.JsonObject = TryCast(g, System.Text.Json.Nodes.JsonObject)
                            If obj Is Nothing OrElse obj("consent")?.GetValue(Of Boolean)() <> True Then Continue For
                            Dim gdir As String = obj("path")?.ToString()
                            If gdir IsNot Nothing AndAlso Directory.Exists(gdir) Then
                                Log("hook game whitelisted: " & obj("name")?.ToString() & " (" & gdir & ")")
                                ' bridgeFile=true → write NvidiaShareHook.json next
                                ' to the exe (C# mods). Native Present-hook games
                                ' keep bridgeFile absent = ZERO files in the
                                ' game folder (owner rule: "ห้ามลงไฟล์ในเกม") —
                                ' they read port+secret from the MMF header
                                ' (+28/+32) instead. File write is OPT-IN only.
                                Dim wantBridge As Boolean = False
                                If obj("bridgeFile") IsNot Nothing Then wantBridge = obj("bridgeFile").GetValue(Of Boolean)()
                                If wantBridge Then
                                    File.WriteAllText(IO.Path.Combine(gdir, "NvidiaShareHook.json"),
                                        "{""port"":" & _server.Port.ToString() & ",""secret"":""" & _server.Secret & """}")
                                    Log("hook bridge written: " & gdir)
                                End If
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                Log("hook bridge failed: " & ex.Message)
            End Try

            InitWebView(oscRoot)
        Catch ex As Exception
            Log("StartStack failed: " & ex.Message)
        End Try
    End Sub

    Private Function ResolveOscRoot() As String
        ' 1) repo working copy (has BOTH the legacy bundle at root and the
        '    NEW UI under next\) — preferred in dev
        ' 2) staged product tree Overlay\osc\ (sweep copies it there)
        ' 3) exe-relative fallbacks
        Dim candidates As String() = {
            IO.Path.Combine(Application.StartupPath, "..", "..", "..", "..", "Overlay", "osc"),
            IO.Path.Combine(Application.StartupPath, "..", "osc"),
            IO.Path.Combine(Application.StartupPath, "osc"),
            IO.Path.Combine(Application.StartupPath, "..", "..", "osc")
        }
        For Each c As String In candidates
            Dim full As String = IO.Path.GetFullPath(c)
            If File.Exists(IO.Path.Combine(full, "index.html")) Then Return full
        Next
        Return Nothing
    End Function

    ' ── WebView2 ───────────────────────────────────────────────

    ' Surface alpha WORKS (desktop shows through — verified via wallpaper
    ' visible in every open screenshot). The earlier "white" was NOBODY
    ' painting a backdrop: real GFE's host paints the dim layer, the osc
    ' page paints nothing. Ours is the host-toggled #oscengine-backdrop div
    ' (see PolyfillSource + SetOverlayOpen). Never toggle this control's
    ' Visible: hide→show recreated the composition (white second-open).
    Private Sub InitWebView(oscRoot As String)
        ' REAL-HOST MODEL (from NVIDIA Share's own debug.log:
        ' "window size: 1680 1050" = the OSC window is the FULL monitor,
        ' and offscreen_window.cpp sets a zoom for the FHD canvas).
        ' zoom = min(W/1920, H/1080): the 1920x1080 canvas scales down by
        ' a visible percentage to fit (1680x1050 = 87.5%, 4K = 200%); the
        ' window stays fullscreen so the dim covers everything and the
        ' page's responsive CSS owns the leftover space — no letterbox
        ' strip, no Dock tricks (the real host has none).
        Dim scaleZoom As Double = Math.Min(Screen.PrimaryScreen.Bounds.Width / 1920.0,
                                           Screen.PrimaryScreen.Bounds.Height / 1080.0)
        _webView = New Microsoft.Web.WebView2.WinForms.WebView2 With {
            .Dock = DockStyle.Fill,
            .DefaultBackgroundColor = System.Drawing.Color.Transparent,
            .ZoomFactor = CSng(scaleZoom),
            .Visible = True
        }
        Controls.Add(_webView)
        BackColor = System.Drawing.Color.FromArgb(14, 14, 14)

        Dim udf As String = AppLayout.P("Data", "WebView2-osc")
        Try
            If Not Directory.Exists(IO.Path.GetDirectoryName(udf)) Then
                Directory.CreateDirectory(IO.Path.GetDirectoryName(udf))
            End If
        Catch
            udf = IO.Path.Combine(Application.StartupPath, "WebView2UDF")
        End Try
        InitWebViewAsync(oscRoot, udf)
    End Sub

    ''' <summary>Fully awaited init chain. MUST NOT block: WebView2 tasks
    '     resume on the UI thread — a .Wait() here deadlocks the form with
    '     the page stuck on about:blank (measured).</summary>
    Private Async Sub InitWebViewAsync(oscRoot As String, udf As String)
        Try
            Dim env As Microsoft.Web.WebView2.Core.CoreWebView2Environment =
                Await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateAsync(
                    Nothing, udf, New Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions() With {
                        .AdditionalBrowserArguments =
                            "--disable-backgrounding-occluded-windows --disable-renderer-backgrounding " &
                            "--remote-debugging-port=9224 " &
                            Environment.GetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS")
                    })
            Await _webView.EnsureCoreWebView2Async(env)

            Dim core As Microsoft.Web.WebView2.Core.CoreWebView2 = _webView.CoreWebView2
            core.Settings.AreDefaultContextMenusEnabled = False
            core.Settings.AreDevToolsEnabled = Environment.GetEnvironmentVariable("OSCENGINE_DEVTOOLS") = "1"
            AddHandler core.WebMessageReceived, AddressOf OnWebMessage
            AddHandler core.ProcessFailed, Sub(s, args) Log("webview process failed: " & args.ProcessFailedKind.ToString())

            ' polyfill BEFORE any page script: vendor.js's cefService must
            ' find window.cefQuery on first use.
            Await core.AddScriptToExecuteOnDocumentCreatedAsync(CefQueryBridge.PolyfillSource())
            ' IMPORTANT: the page's own API base is "http://localhost:<port>"
            ' (its LOCALHOST_ADDR constant). Serving the page from the SAME
            ' host keeps every REST/socket call same-origin — otherwise each
            ' call sends a CORS preflight OPTIONS that carries no auth
            ' cookie and gets rejected (measured).
            ' PRODUCTION UI = the ORIGINAL NVIDIA osc bundle (owner
            ' directive: "osc ของ NVIDIA" — the next/ drawer UI was removed
            ' and must NOT come back). The legacy entry /index.html is the
            ' real tiles menu; same-origin serving keeps every REST/socket
            ' call un-preflighted.
            core.Navigate("http://localhost:" & _server.Port.ToString() & "/index.html")
            _webviewReady = True
            Log("webview navigated to http://localhost:" & _server.Port.ToString() & "/index.html (NVIDIA osc)")
            ' mount the menu view NOW through the page's own openOSC (hidden
            ' while closed) so even the very first Alt+Z is a class flip
            Try
                Await core.ExecuteScriptAsync("window.__oscOpen && window.__oscOpen();")
            Catch
            End Try

            ' frame pump: stream the osc page render to hooked games
            HookCdpCapture.ControllerPort = _server.Port
            HookCdpCapture.ControllerSecret = _server.Secret
            _cdpCapture = New HookCdpCapture("9224")
            _cdpCapture.Start()
            ' WGC GPU source: 60fps frames straight off the compositor —
            ' the CDP loop idles while WGC is live (fallback otherwise)
            Try
                NvShareEngine.WgcFrameSource.FrameCallback = AddressOf HookCdpCapture.PublishPixels
                NvShareEngine.WgcFrameSource.GateProbe = Function() HookCdpCapture.CaptureEnabled
                NvShareEngine.WgcFrameSource.ActiveChanged = Sub(a)
                                                   HookCdpCapture.ExternalCapture = If(a, 1, 0)
                                                   Log("wgc " & If(a, "ACTIVE — gpu frames", "inactive — CDP fallback"))
                                               End Sub
                NvShareEngine.WgcFrameSource.Start(Me.Handle)   ' top-level only: CreateForWindow rejects child windows
            Catch ex2 As Exception
                Log("wgc start failed: " & ex2.Message)
            End Try
            ' in-game hook input: shared-memory ring (no HTTP — the game's
            ' online-fix layer intercepts WinHTTP inside the game process)
            _inputReader = New HookInputReader()
            AddHandler _inputReader.Input, Sub(body) BeginInvoke(Sub() OnHookInput(body))
            _inputReader.Start()
            ' watch whitelisted games → auto-inject the in-game hook DLL
            HookAutoInject.Start()
        Catch ex As Exception
            Log("WebView2 init failed: " & ex.Message)
        End Try
    End Sub

    Private Sub OnWebMessage(sender As Object, e As Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs)
        Log("webmsg: " & e.WebMessageAsJson.Substring(0, Math.Min(160, e.WebMessageAsJson.Length)))
        Dim reply As String = _bridge.HandleWebMessage(e.WebMessageAsJson)
        If reply IsNot Nothing Then
            _webView.CoreWebView2.PostWebMessageAsJson(reply)
            Log("webmsg reply: " & reply.Substring(0, Math.Min(160, reply.Length)))
        End If
    End Sub

    ' ── toggle / visibility ────────────────────────────────────

    ''' <summary>Instant state setter (owner directives: "WebView เปิดเต็มตลอด
    '     ห้ามย่อ", "hotkey ช้ามาก", "ปิดแล้วไป main-menu"). The window is
    '     ALWAYS fullscreen on the primary screen — open/close flips only:
    '     input (WndProc: closed = HTTRANSPARENT ทั้งจอ / open = HTCLIENT),
    '     the page route (direct hash navigation = instant, no socket
    '     round-trip), and the backdrop class. NO parking, NO window moves —
    '     moving the fullscreen window was what made everything flicker.</summary>
    Private Sub SetOverlayOpen(open As Boolean, pushToPage As Boolean, Optional userInitiated As Boolean = True)
        If Not _webviewReady Then
            Log("overlay state ignored — webview not ready")
            Return
        End If
        If _overlayOpen = open Then Return
        _overlayOpen = open
        _userOpen = If(open, userInitiated, False)
        HookCdpCapture.CaptureEnabled = If(open, 1, 0)
        TopMost = True

        ' IN-GAME MODE: the injected DLL is alive → drive the overlay purely
        ' through the shared-memory header (overlayVisible @ +16) and NEVER
        ' touch the window — no Show, no Activate, no style change — so the
        ' exclusive-fullscreen game keeps focus (owner bug: Alt+Z alt-tabbed
        ' the game out).
        Dim inGame As Boolean = False
        Try : inGame = HookFramePump.HookLive() : Catch : End Try
        If inGame Then
            ' DISPLAY = the injected DLL draws the frame inside the game's
            ' own Present. This window stays OUT OF SIGHT: bottom z-order +
            ' click-through, NEVER topmost (topmost = flat gray over all).
            Dim styleIn As Integer = CInt(GetWindowLong(Handle, GWL_EXSTYLE))
            SetWindowLong(Handle, GWL_EXSTYLE, New IntPtr(styleIn Or WS_EX_TRANSPARENT))
            If Not Visible Then Show()
            Dim pr As System.Drawing.Rectangle = Screen.PrimaryScreen.Bounds
            Location = pr.Location
            SetWindowPos(Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE Or SWP_NOSIZE Or SWP_NOACTIVATE)
            Try
                _webView.CoreWebView2.ExecuteScriptAsync(
                    "document.documentElement.classList.toggle('oscengine-open'," &
                    open.ToString().ToLowerInvariant() & ");" &
                    If(open, "window.__oscOpen && window.__oscOpen();",
                        "window.__oscClose && window.__oscClose();"))
            Catch ex As Exception
                Log("route/backdrop toggle failed: " & ex.Message)
            End Try
            Log(If(open, "overlay open (in-game)", "overlay closed (in-game)"))
            Return
        End If

        ' GFE semantics: menu open = the WHOLE window accepts input;
        ' closed = the whole window is click-through (WS_EX_TRANSPARENT —
        ' HTTRANSPARENT alone never passes hits to OTHER processes).
        ' Page-requested opens (screenshot/recording toasts) show the view
        ' but KEEP click-through — the screen must not be taken over.
        _menuInputEnabled = _overlayOpen AndAlso _userOpen
        Dim style As Integer = CInt(GetWindowLong(Handle, GWL_EXSTYLE))
        If _menuInputEnabled Then
            SetWindowLong(Handle, GWL_EXSTYLE, New IntPtr(style And Not WS_EX_TRANSPARENT))
            If Not Visible Then Show()   ' first open: the form was never auto-shown
            Activate()
        Else
            SetWindowLong(Handle, GWL_EXSTYLE, New IntPtr(style Or WS_EX_TRANSPARENT))
        End If
        ' THREE visual states —
        '   user open (Alt+Z)  : oscengine-open  = dim + menu (openOSC)
        '   page open (toasts) : oscengine-toast = light dim, NO menu,
        '                        click-through KEPT (screenshot previews)
        '   closed             : neither = fully hidden / click-through
        Try
            Dim cls As String = ""
            If open AndAlso userInitiated Then
                cls = "oscengine-open"
            ElseIf open Then
                cls = "oscengine-toast"
            End If
            Dim script As String =
                "document.documentElement.classList.remove('oscengine-open','oscengine-toast');"
            If cls.Length > 0 Then
                script &= "document.documentElement.classList.add('" & cls & "');"
            End If
            If open AndAlso userInitiated Then
                script &= "window.__oscOpen && window.__oscOpen();"
            End If
            If Not open Then
                script &= "window.__oscClose && window.__oscClose();"
            End If
            _webView.CoreWebView2.ExecuteScriptAsync(script)
            ' publish visibility to the in-game hook mod
            OscControllerServer.SetHookOverlayState(open)
        Catch ex As Exception
            Log("route/backdrop toggle failed: " & ex.Message)
        End Try
        Log(If(open, If(userInitiated, "overlay open", "overlay toast"), "overlay closed"))
    End Sub

    ''' <summary>Input from the in-game hook mod → synthetic DOM events on
    '     the page. Coordinates arrive in game pixels; the page viewport is
    '     (game / zoom) CSS px where zoom = min(W/1920,H/1080).
    '     type:"toggle" flips the overlay for remote control (bisect/debug).</summary>
    Private Sub OnHookInput(bodyJson As String)
        If Not _webviewReady Then Return
        Try
            Dim root As System.Text.Json.JsonElement = System.Text.Json.JsonDocument.Parse(bodyJson).RootElement
            Dim typ As String = root.GetProperty("type").GetString()
            If typ = "toggle" Then
                BeginInvoke(Sub() SetOverlayOpen(Not _overlayOpen, pushToPage:=False))
                Return
            End If
            Dim x As Double = 0, y As Double = 0
            Dim xN As System.Text.Json.JsonElement
            Dim yN As System.Text.Json.JsonElement
            If root.TryGetProperty("x", xN) Then x = xN.GetDouble()
            If root.TryGetProperty("y", yN) Then y = yN.GetDouble()
            Dim zoom As Double = Math.Min(
                Screen.PrimaryScreen.Bounds.Width / 1920.0,
                Screen.PrimaryScreen.Bounds.Height / 1080.0)
            If zoom <= 0 Then zoom = 1
            Dim cx As Integer = CInt(x / zoom)
            Dim cy As Integer = CInt(y / zoom)
            Dim js As String = ""
            Select Case typ
                Case "mousemove"
                    js = "(function(){var el=document.elementFromPoint(" & cx & "," & cy & ");if(!el)return;el.dispatchEvent(new MouseEvent('mousemove',{clientX:" & cx & ",clientY:" & cy & ",bubbles:true}));})()"
                Case "mousedown", "mouseup"
                    Dim btn As Integer = 0
                    Dim bN As System.Text.Json.JsonElement
                    If root.TryGetProperty("button", bN) Then btn = bN.GetInt32()
                    ' Angular binds (click)/(pointerdown) — a bare synthetic
                    ' mousedown/mouseup never synthesizes a click event, so
                    ' tiles ignored our taps. Dispatch the FULL sequence.
                    Dim phase As String = If(typ = "mousedown", "down", "up")
                    Dim seq As String =
                        "el.dispatchEvent(new PointerEvent('pointer" & phase & "',o));" &
                        "el.dispatchEvent(new MouseEvent('" & typ & "',o));"
                    If typ = "mouseup" Then seq &= "el.dispatchEvent(new MouseEvent('click',o));"
                    js = "(function(){var el=document.elementFromPoint(" & cx & "," & cy & ");if(!el)return;" &
                        "var o={clientX:" & cx & ",clientY:" & cy & ",button:" & btn & ",bubbles:true,cancelable:true};" &
                        seq & "})()"
                Case "keydown", "keyup"
                    Dim vkN As System.Text.Json.JsonElement
                    If Not root.TryGetProperty("vk", vkN) Then Exit Select
                    Dim vk As Integer = vkN.GetInt32()
                    Dim shift As Boolean = False, ctrl As Boolean = False
                    Dim sN As System.Text.Json.JsonElement
                    If root.TryGetProperty("shift", sN) Then shift = sN.GetInt32() = 1
                    If root.TryGetProperty("ctrl", sN) Then ctrl = sN.GetInt32() = 1
                    Dim key As String = VkToJsKey(vk, shift)
                    If key Is Nothing Then Exit Select
                    js = "(function(){document.dispatchEvent(new KeyboardEvent('" & typ &
                         "',{key:""" & key & """,keyCode:" & vk & ",which:" & vk &
                         ",shiftKey:" & shift.ToString().ToLowerInvariant() &
                         ",ctrlKey:" & ctrl.ToString().ToLowerInvariant() &
                         ",bubbles:true}));})()"
            End Select
            ' CDP Input.* goes through Chromium's REAL input pipeline —
            ' synthetic DOM events can't trigger CSS :hover or set focus,
            ' CDP input does both natively. DOM events are the FALLBACK
            ' only (CDP down), never combined (would double-fire clicks).
            Dim cdp As String = ""
            Select Case typ
                Case "mousemove"
                    cdp = """method"":""Input.dispatchMouseEvent"",""params"":{""type"":""mouseMoved"",""x"":" & cx & ",""y"":" & cy & "}"
                Case "mousedown"
                    cdp = """method"":""Input.dispatchMouseEvent"",""params"":{""type"":""mousePressed"",""x"":" & cx & ",""y"":" & cy & ",""button"":""left"",""buttons"":1,""clickCount"":1}"
                Case "mouseup"
                    cdp = """method"":""Input.dispatchMouseEvent"",""params"":{""type"":""mouseReleased"",""x"":" & cx & ",""y"":" & cy & ",""button"":""left"",""buttons"":0,""clickCount"":1}"
                Case "keydown", "keyup"
                    Dim vkN As System.Text.Json.JsonElement
                    If root.TryGetProperty("vk", vkN) Then
                        Dim vk As Integer = vkN.GetInt32()
                        Dim shift As Boolean = False
                        Dim sN As System.Text.Json.JsonElement
                        If root.TryGetProperty("shift", sN) Then shift = sN.GetInt32() = 1
                        Dim keyName As String = VkToJsKey(vk, shift)
                        If keyName IsNot Nothing AndAlso keyName.Length = 1 Then
                            Dim txtParam As String = If(typ = "keydown",
                                ",""text"":""" & keyName & """", "")
                            cdp = """method"":""Input.dispatchKeyEvent"",""params"":{""type"":""" &
                                If(typ = "keydown", "keyDown", "keyUp") & """,""key"":""" & keyName &
                                """,""windowsVirtualKeyCode"":" & vk & txtParam & "}"
                        ElseIf keyName IsNot Nothing Then
                            cdp = """method"":""Input.dispatchKeyEvent"",""params"":{""type"":""" &
                                If(typ = "keydown", "keyDown", "keyUp") & """,""key"":""" & keyName &
                                """,""windowsVirtualKeyCode"":" & vk & "}"
                        End If
                    End If
            End Select
            If cdp.Length > 0 Then
                If Not HookCdpCapture.SendCdpInput(cdp) AndAlso js.Length > 0 Then
                    _webView.CoreWebView2.ExecuteScriptAsync(js)   ' fallback
                End If
            ElseIf js.Length > 0 Then
                _webView.CoreWebView2.ExecuteScriptAsync(js)
            End If
        Catch ex As Exception
            Log("hook input failed: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Virtual-key → JS KeyboardEvent.key name. Returns Nothing
    '     for keys we deliberately do not forward (LWin/RWin, IME, mouse).</summary>
    Private Shared Function VkToJsKey(vk As Integer, shift As Boolean) As String
        If vk >= &H41 AndAlso vk <= &H5A Then
            Dim c As Char = ChrW(vk)
            Return If(shift, c, Char.ToLowerInvariant(c))
        End If
        If vk >= &H30 AndAlso vk <= &H39 Then Return ChrW(vk).ToString()
        If vk >= &H70 AndAlso vk <= &H87 Then Return "F" & (vk - &H6F).ToString()
        If vk >= &H60 AndAlso vk <= &H69 Then Return "Numpad" & ChrW(&H30 + (vk - &H60))
        Select Case vk
            Case &H8 : Return "Backspace"
            Case &H9 : Return "Tab"
            Case &HD : Return "Enter"
            Case &H10, &HA0, &HA1 : Return "Shift"
            Case &H11, &HA2, &HA3 : Return "Control"
            Case &H12, &HA4, &HA5 : Return "Alt"
            Case &H13 : Return "Pause"
            Case &H14 : Return "CapsLock"
            Case &H1B : Return "Escape"
            Case &H20 : Return " "
            Case &H21 : Return "PageUp"
            Case &H22 : Return "PageDown"
            Case &H23 : Return "End"
            Case &H24 : Return "Home"
            Case &H25 : Return "ArrowLeft"
            Case &H26 : Return "ArrowUp"
            Case &H27 : Return "ArrowRight"
            Case &H28 : Return "ArrowDown"
            Case &H2C : Return "PrintScreen"
            Case &H2D : Return "Insert"
            Case &H2E : Return "Delete"
            Case &HBA : Return ";"
            Case &HBB : Return "="
            Case &HBC : Return ","
            Case &HBD : Return "-"
            Case &HBE : Return "."
            Case &HBF : Return "/"
            Case &HC0 : Return "`"
            Case &HDB : Return "["
            Case &HDC : Return "\"
            Case &HDD : Return "]"
            Case &HDE : Return "'"
            Case Else : Return Nothing
        End Select
    End Function

    Private Sub ToggleOverlay(Optional forceOpen As Boolean = False)
        ' debounce: rapid auto-repeat must not machine-gun toggles — 150ms
        ' still filters key auto-repeat but lets fast pressing through
        Dim now As Integer = Environment.TickCount
        If Math.Abs(now - _lastToggleTick) < 150 Then Return
        _lastToggleTick = now
        ' in-game mode: the HEADER is the ground truth (the form's state can
        ' desync from restarts) — toggle against what the DLL actually draws
        If HookFramePump.HookLive() Then
            Dim vis As Boolean = HookFramePump.OverlayHeaderVisible()
            SetOverlayOpen(Not vis, pushToPage:=False)
            Return
        End If
        If forceOpen Then
            SetOverlayOpen(True, pushToPage:=True)
        Else
            SetOverlayOpen(Not _overlayOpen, pushToPage:=True)
        End If
    End Sub

    Private _lastToggleTick As Integer = -1000

    Private Sub ExitApplication()
        _closingForExit = True
        Close()
    End Sub

    ' ── engine events → page ───────────────────────────────────

    ''' <summary>Real actions behind the page-saved hotkey bindings.
    '     Screenshot captures for real; Record drives the engine; the
    '     Ansel/Mods bindings open their preview pages; everything else
    '     answers with a toast so the binding is provably live.</summary>
    Private Sub OnHotkeyAction(name As String)
        Log("action hotkey fired: " & name)
        Select Case name
            Case "Screenshot"
                CaptureScreenshotNow()
            Case "RecordToggle"
                If _recording Then
                    _client.SendRecordStop()
                Else
                    Dim savePath As String = AppConfigShared.ReadString("Paths", "SavePath", "")
                    _client.SendRecordStart(OscProtocol.RecordOutputPath(savePath, DateTime.Now))
                End If
            Case "NvCameraUI", "ModsUI"
                Dim route As String = If(name = "NvCameraUI", "nvcamera", "mods")
                SetOverlayOpen(True, pushToPage:=False)
                Try
                    _webView.CoreWebView2.ExecuteScriptAsync("location.hash='#/base/" & route & "';")
                Catch ex As Exception
                    Log("hotkey nav failed: " & ex.Message)
                End Try
            Case Else
                PushNotification(name, "hotkey received (preview)")
        End Select
    End Sub

    Private Sub OnEngineStateChanged(stateName As String)
        _recording = OscProtocol.ShouldShowRecording(stateName, _recording)
    End Sub

    Private Sub OnRecordingSaved(filePath As String)
        _recording = False
        ' GFE contract: recordingSaved + result 0 + file → the page updates
        ' its gallery/history and shows the saved toast
        PushNotificationPayload("recordingSaved", filePath)
    End Sub

    Private Sub OnRecordingError(message As String)
        _recording = False
        PushNotification("Recording failed", message)
    End Sub

    Private Sub PushNotification(title As String, message As String)
        Try
            _server.PushEvent("/ShadowPlay/v.1.0/Notification",
                "{""title"":" & OscWire.JsonString(title) & ",""message"":" & OscWire.JsonString(message) & "}")
        Catch ex As Exception
            Log("push notification failed: " & ex.Message)
        End Try
    End Sub

    Private Sub OnRecordEnableRequested(enable As Boolean)
        If enable Then
            Dim savePath As String = AppConfigShared.ReadString("Paths", "SavePath", "")
            _client.SendRecordStart(OscProtocol.RecordOutputPath(savePath, DateTime.Now))
        Else
            _client.SendRecordStop()
        End If
    End Sub

    Private Function BuildStateJson() As String
        Return "{""record"":" & _recording.ToString().ToLowerInvariant() &
               ",""instantReplay"":" & _replayBuffering.ToString().ToLowerInvariant() &
               ",""broadcast"":false,""elapsedSec"":" & _elapsedSec.ToString() & "}"
    End Function

    ' ── real screenshot (GFE /Screenshot/Capture contract) ─────

    ''' <summary>Captures the primary screen to a PNG under
    '     Pictures\NVIDIA ShadowPlay\Screenshots\ and pushes the GFE
    '     notification contract {notification:"screenshot", result:0, file}.
    '     result:0 == the page's OSC_SUCCESS.</summary>
    Private Sub CaptureScreenshotNow()
        Try
            Dim dir As String = IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "NVIDIA ShadowPlay", "Screenshots")
            Directory.CreateDirectory(dir)
            Dim file As String = IO.Path.Combine(dir,
                "Screenshot_" & DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") & ".png")

            ' NO hide/show here (owner: the fullscreen flash felt like the
            ' whole screen collapsing). The overlay is a layered transparent
            ' surface — CopyFromScreen composites it out naturally, and the
            ' window must never move or toggle visibility while capturing.
            Dim bounds As System.Drawing.Rectangle = Screen.PrimaryScreen.Bounds
            Using bmp As New System.Drawing.Bitmap(bounds.Width, bounds.Height)
                Using g As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bmp)
                    g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size)
                End Using
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png)
            End Using

            Log("screenshot saved: " & file)
            PushNotificationPayload("screenshot", file)
        Catch ex As Exception
            Log("screenshot failed: " & ex.Message)
        End Try
    End Sub

    ''' <summary>GFE notification payload on /ShadowPlay/v.1.0/Notification:
    '     {notification:"screenshot"|"recordingSaved", result:0(=OSC_SUCCESS), file}.</summary>
    Private Sub PushNotificationPayload(notificationType As String, file As String)
        Try
            _server.PushEvent("/ShadowPlay/v.1.0/Notification",
                "{""notification"":" & OscWire.JsonString(notificationType) &
                ",""result"":0,""file"":" & OscWire.JsonString(file) & "}")
        Catch ex As Exception
            Log("push notification failed: " & ex.Message)
        End Try
    End Sub

    Private Function BuildRecordPathsJson() As String
        Dim p As String = AppConfigShared.ReadString("Paths", "SavePath", "")
        If String.IsNullOrEmpty(p) Then
            p = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NVIDIA ShadowPlay", "videos")
        End If
        Return "{""savePath"":" & OscWire.JsonString(p) & "}"
    End Function

    ' ── socket events from the page ────────────────────────────

    Private Sub OnSocketEvent(channel As String, payloadJson As String)
        Log("page event " & channel & " " & If(payloadJson, "<null>"))
        Select Case channel
            Case "/ShadowPlay/v.1.0/Hotkey"
                ' M1: page hotkey surface is informational; record/replay go
                ' through REST + tray. Logged for the M2 mapping.
            Case Else
                ' other channels are informational in M1
        End Select
    End Sub

    ' ── click-through ──────────────────────────────────────────

    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = OscHotkeys.HotkeyMsg Then
            ' WM_HOTKEY: id 1 = the Alt+Z toggle (OscHotkeys); the rest are
            ' the applier's page-saved action bindings
            Dim hkId As Integer = m.WParam.ToInt32()
            If hkId = ToggleHotkeyId AndAlso _hotkeys IsNot Nothing AndAlso _hotkeys.IsRegistered Then
                ToggleOverlay()
                Return
            End If
            If _applier IsNot Nothing Then
                _applier.HandleHotkey(hkId)
                Return
            End If
        End If
        If m.Msg = WM_NCHITTEST Then
            ' interactive ONLY when the USER opened the overlay (Alt+Z) —
            ' page-requested opens (toasts) and the closed state stay
            ' click-through
            If Not _menuInputEnabled OrElse Not _webviewReady Then
                m.Result = CType(HTTRANSPARENT, IntPtr)
                Return
            End If
            ' Menu open → the PAGE owns all input (GFE open-OSC semantics:
            ' QUERY_WIN_OPEN_OSC enableInput). NEVER branch on the page's
            ' displayRects here — toasts/transitions re-report rect sets
            ' that don't cover the menu, which made the whole screen go
            ' click-dead until a toggle (owner-reported). The page handles
            ' its own hit-testing while open.
            m.Result = CType(HTCLIENT, IntPtr)
            Return
        End If
        MyBase.WndProc(m)
    End Sub

    ' ── shutdown (no orphan rule) ──────────────────────────────

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' a recording session must survive a UI close — same contract as
        ' the Forms overlay: nothing here touches NVIDIA Capture.exe.
        Try
            If _hotkeys IsNot Nothing Then
                _hotkeys.Unregister(Handle)
                _hotkeys.Dispose()
            End If
        Catch
        End Try
        Try
            If _applier IsNot Nothing Then _applier.UnregisterAll()
        Catch
        End Try
        Try
            EngineProcessSupervisor.Shutdown()
        Catch
        End Try
        Try
            If _server IsNot Nothing Then _server.StopServer()
        Catch
        End Try
        Try
            If _client IsNot Nothing Then _client.Dispose()
        Catch
        End Try
        ' WebView2: the WinForms control tears its browser process down with
        ' the form; StopServer above ends the long-polls first so msedgewebview2
        ' never lingers on a held request.
        If _webView IsNot Nothing Then
            Try
                _webView.Dispose()
            Catch
            End Try
            _webView = Nothing
        End If
        MyBase.OnFormClosing(e)
        If Not _closingForExit Then
            Environment.Exit(0)   ' hidden overlay window: never linger
        End If
    End Sub

    Private Shared ReadOnly _logLock As New Object()

    Private Sub Log(message As String)
        Debug.WriteLine("[OscEngine] " & message)
        Try
            Dim path As String = AppLayout.P("Logs", "overlay-engine.log")
            Dim dir As String = IO.Path.GetDirectoryName(path)
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
            ' concurrent appends (listener thread + UI thread + server
            ' threads) silently drop lines without this lock
            SyncLock _logLock
                File.AppendAllText(path, DateTime.Now.ToString("HH:mm:ss.fff") & " " & message & Environment.NewLine)
            End SyncLock
        Catch
            ' logging must never take the overlay down
        End Try
    End Sub

    ' ── interop ────────────────────────────────────────────────

    <DllImport("user32.dll", EntryPoint:="GetWindowLongW")>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    <DllImport("user32.dll", EntryPoint:="SetWindowLongW")>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As Integer
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetLayeredWindowAttributes(hWnd As IntPtr, crKey As UInteger, bAlpha As Byte, dwFlags As Integer) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function CreateRectRgn(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SetWindowRgn(hWnd As IntPtr, hRgn As IntPtr, bRedraw As Boolean) As Integer
    End Function

End Class
