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

    Private WithEvents _webView As Microsoft.Web.WebView2.WinForms.WebView2
    Private _server As OscControllerServer
    Private _client As OscEngineClient
    Private _bridge As CefQueryBridge
    Private _storage As SharedStorageStore
    Private _tray As NotifyIcon
    Private _hotkeys As OscHotkeys
    Private _applier As OscHotkeyApplier
    Private Const ToggleHotkeyId As Integer = 1
    Private _oscReady As Boolean
    Private _overlayOpen As Boolean
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

        ' never in taskbar/Alt-Tab
        Dim ex As Integer = CInt(GetWindowLong(Handle, GWL_EXSTYLE))
        SetWindowLong(Handle, GWL_EXSTYLE, New IntPtr((ex Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW))

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

        SetupTray()
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

    Private Sub SetupTray()
        _tray = New NotifyIcon With {
            .Icon = SystemIcons.Application,
            .Text = "NVIDIA Share (osc)",
            .Visible = True
        }
        Dim menu As New ContextMenuStrip()
        menu.Items.Add("Toggle OUR overlay" & If(_hotkeys?.IsRegistered, " (" & _hotkeys.BindingText & ")", ""), Nothing, Sub() ToggleOverlay())
        If Process.GetProcessesByName("NVIDIA Share").Length > 0 Then
            menu.Items.Add("Toggle GFE overlay (real ShadowPlay)", Nothing,
                Sub() NvShadowPlayRecorder.InjectAltZ())
        End If
        menu.Items.Add(New ToolStripSeparator())
        menu.Items.Add("Exit", Nothing, Sub() ExitApplication())
        _tray.ContextMenuStrip = menu
        AddHandler _tray.DoubleClick, Sub() ToggleOverlay()
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
            AddHandler _bridge.OpenOsc, Sub(input) BeginInvoke(Sub() SetOverlayOpen(True, pushToPage:=False))
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
            _applier.Reapply(Handle)

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
                        .AdditionalBrowserArguments = Environment.GetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS")
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

    ''' <summary>Idempotent state setter (closed-loop — see the push block).
    '     The window NEVER moves while open... it DOES park when closed
    '     (a topmost idle fullscreen window renders as a gray veil).</summary>
    Private Async Sub SetOverlayOpen(open As Boolean, pushToPage As Boolean)
        If Not _webviewReady Then
            Log("overlay state ignored — webview not ready")
            Return
        End If
        If _overlayOpen = open Then Return
        _overlayOpen = open
        TopMost = True
        ' GFE semantics: menu open = the WHOLE window accepts input
        ' (QUERY_WIN_OPEN_OSC enableInput) — displayRects are for the
        ' closed/OSD state. Without this, an empty rect list made every
        ' click fall through and the menu was unclickable (owner-reported).
        _menuInputEnabled = open
        If open Then
            If Not Visible Then Show()   ' first open: the form was never auto-shown
            Dim bounds As System.Drawing.Rectangle = Screen.PrimaryScreen.Bounds
            Location = bounds.Location
            Size = bounds.Size
            Activate()
        Else
            ' park just BELOW the primary screen: invisible like a -32000
            ' park, but MonitorFromWindow still resolves to the PRIMARY —
            ' at -32000 it snapped to the portrait side monitor and
            ' Chromium re-derived a wrong window.screen (menu mislaid out).
            Dim b As System.Drawing.Rectangle = Screen.PrimaryScreen.Bounds
            Location = New System.Drawing.Point(b.X, b.Bottom + 40)
        End If
        ' host-owned backdrop follows OUR state (real GFE: the host paints
        ' the dim behind the menu; the page paints nothing)
        Try
            _webView.CoreWebView2.ExecuteScriptAsync(
                "document.documentElement.classList.toggle('oscengine-open'," &
                open.ToString().ToLowerInvariant() & ")")
        Catch ex As Exception
            Log("backdrop toggle failed: " & ex.Message)
        End Try
        If pushToPage Then
            ' CLOSED-LOOP control with retry: the page exposes NO stateful
            ' "open menu" channel (DisplayOscState only routes
            ' preferences/gallery), so we read location.hash, push the right
            ' frame, and RE-READ until the page actually converged — a one-
            ' shot read races the ui-router transition (measured: dismiss at
            ' .406, hash still main-menu at .444 → phantom state flip).
            Try
                For attempt As Integer = 1 To 4
                    Dim raw As String = Await _webView.CoreWebView2.ExecuteScriptAsync("location.hash")
                    Dim pageOpen As Boolean = If(raw IsNot Nothing, raw.Contains("main-menu"), False)
                    If pageOpen = open Then
                        Log("page converged (" & If(pageOpen, "open", "closed") & ") on attempt " & attempt.ToString())
                        Exit For
                    End If
                    If attempt > 1 Then
                        Await Task.Delay(350)
                        raw = Await _webView.CoreWebView2.ExecuteScriptAsync("location.hash")
                        pageOpen = If(raw IsNot Nothing, raw.Contains("main-menu"), False)
                        If pageOpen = open Then
                            Log("page converged (" & If(pageOpen, "open", "closed") & ") after settle, attempt " & attempt.ToString())
                            Exit For
                        End If
                    End If
                    If open Then
                        _server.PushEvent("/ShadowPlay/v.1.0/WindowState", "{""windowMsg"":""overlayToggle""}")
                        Log("push overlayToggle (page reports closed), attempt " & attempt.ToString())
                    Else
                        _server.PushEvent("/ShadowPlay/v.1.0/WindowState", "{""windowMsg"":""dismiss""}")
                        Log("push dismiss (page reports open), attempt " & attempt.ToString())
                    End If
                    Await Task.Delay(400)
                Next
            Catch ex As Exception
                Log("closed-loop push failed: " & ex.Message)
            End Try
        End If
        Log(If(open, "overlay open", "overlay closed"))
    End Sub

    Private Sub ToggleOverlay(Optional forceOpen As Boolean = False)
        ' debounce: key auto-repeat (holding Alt+Z fires WM_HOTKEY every
        ' ~30ms) must not machine-gun toggles — 500ms = one toggle per press
        Dim now As Integer = Environment.TickCount
        If Math.Abs(now - _lastToggleTick) < 500 Then Return
        _lastToggleTick = now
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

            ' hide our own window while capturing (real GFE does the same)
            Dim wasVisible As Boolean = Visible
            If wasVisible Then Visible = False
            Threading.Thread.Sleep(120)

            Dim bounds As System.Drawing.Rectangle = Screen.PrimaryScreen.Bounds
            Using bmp As New System.Drawing.Bitmap(bounds.Width, bounds.Height)
                Using g As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(bmp)
                    g.CopyFromScreen(bounds.Location, System.Drawing.Point.Empty, bounds.Size)
                End Using
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png)
            End Using
            If wasVisible Then Visible = True

            Log("screenshot saved: " & file)
            PushNotificationPayload("screenshot", file)
        Catch ex As Exception
            Log("screenshot failed: " & ex.Message)
            If Not Visible Then Visible = _overlayOpen
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
            If Not _overlayOpen OrElse Not _webviewReady Then
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
            If _tray IsNot Nothing Then
                _tray.Visible = False
                _tray.Dispose()
            End If
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

End Class
