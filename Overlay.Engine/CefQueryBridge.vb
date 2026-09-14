' CefQueryBridge.vb — implements the host side of the CEF message router
' that the osc page expects (window.cefQuery). The polyfill is injected
' with AddScriptToExecuteOnDocumentCreated, so it exists BEFORE vendor.js
' runs and the crimson cefService finds a live bridge instead of logging
' "Cannot find cefQuery".
'
' Wire shape (both sides ours — defined to match the crimson wrapper's
' usage in vendor.js: cefQuery({request, persistent, onSuccess, onFailure})
' where request is a JSON STRING; persistent queries receive pushes on
' their onSuccess):
'   page → host : {__cef:1, id, request, persistent}
'   host → page : {__cefResponse:1, id, ok, response}   (response = string)
'
' The M1 command set and their payloads come from the analysis of
' app.js/vendor.js (see PROTOCOL-MATRIX.md). Unknown commands are FAILED
' with errorCode -1 so the calling service degrades cleanly instead of
' hanging.

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.Text

Public Class CefQueryBridge

    Private ReadOnly _storage As SharedStorageStore
    Private _port As Integer
    Private _secret As String

    Public Event LogLine(message As String)
    Public Event OpenOsc(enableInput As Boolean)
    Public Event CloseOsc()
    Public Event CloseRequested()   ' persistent close-event push target

    ' displayRects / painting state live here; the form reads them for
    ' click-through and visibility decisions.
    Public Property DisplayRects As List(Of System.Drawing.Rectangle)
    Public Property PaintingEnabled As Boolean

    ' sid of the persistent QUERY_OSC_REGISTER_CLOSE_EVENT query — pushing
    ' to it asks the page to close itself.
    Private _closeQueryId As Integer = -1

    Public Sub New(storage As SharedStorageStore)
        _storage = storage
        DisplayRects = New List(Of System.Drawing.Rectangle)
    End Sub

    Public Sub Configure(port As Integer, secret As String)
        _port = port
        _secret = secret
    End Sub

    ' ── page side ──────────────────────────────────────────────

    ''' <summary>Injected before any page script. Mirrors CEF's message
    '     router surface that vendor.js cefService relies on.</summary>
    Public Shared Function PolyfillSource() As String
        Dim sb As New StringBuilder()
        sb.Append("(function(){")
        sb.Append("if(window.cefQuery) return;")
        sb.Append("var seq=0, pending={};")
        ' request object shape returned by real cefQuery
        sb.Append("function mkHandle(id){return {id:id, cancel:function(){delete pending[id];}};}")
        sb.Append("window.cefQuery=function(o){")
        sb.Append("  var id=++seq; pending[id]={onSuccess:o.onSuccess||function(){}, onFailure:o.onFailure||function(){}};")
        sb.Append("  try{")
        sb.Append("    window.chrome.webview.postMessage({__cef:1, id:id, request:(typeof o.request==='string'?o.request:JSON.stringify(o.request||{})), persistent:!!o.persistent});")
        sb.Append("  }catch(e){")
        sb.Append("    (pending[id].onFailure||function(){})(-2, String(e)); delete pending[id]; return {id:id, cancel:function(){}};")
        sb.Append("  }")
        sb.Append("  return mkHandle(id);")
        sb.Append("};")
        sb.Append("window.cefQueryCancel=function(id){delete pending[id];};")
        sb.Append("window.__cefDeliver=function(msg){")
        sb.Append("  var p=pending[msg.id]; if(!p) return;")
        sb.Append("  if(msg.ok){ p.onSuccess(msg.response); if(!msg.persistent) delete pending[msg.id]; }")
        sb.Append("  else { p.onFailure(msg.errorCode===undefined?-1:msg.errorCode, msg.response||''); if(!msg.persistent) delete pending[msg.id]; }")
        sb.Append("};")
        ' host → page delivery: WebView2 posts the reply as a 'message' event
        ' on chrome.webview; route it into __cefDeliver.
        sb.Append("if(window.chrome&&window.chrome.webview&&window.chrome.webview.addEventListener){")
        sb.Append("  window.chrome.webview.addEventListener('message',function(e){ try{window.__cefDeliver(e.data);}catch(err){} });")
        sb.Append("}")
        ' Host-owned backdrop: in real GFE the CEF host paints the dark dim
        ' layer behind the menu — the page itself paints NOTHING (measured:
        ' zero fixed-background elements). Our host toggles the
        ' 'oscengine-open' class on <html>; the rule shows/hides the div.
        ' z-index:-1 = page-wide backdrop under every menu element.
        ' documentElement is NULL when document-created scripts run — the
        ' creation MUST wait for DOMContentLoaded (measured: a direct
        ' appendChild threw and the catch() swallowed it silently, so the
        ' div never existed and the backdrop never showed).
        sb.Append("try{window.__bdStatus='registered';var bdfn=function(){")
        sb.Append("try{if(document.getElementById('oscengine-backdrop')){window.__bdStatus='already';return;}")
        sb.Append("var st=document.createElement('style');st.id='oscengine-backdrop-css';")
        ' the second rule keeps the menu VIEWS mounted in the DOM while the
        ' overlay is closed (owner: hotkey toggle felt slow — an unload/reload
        ' on every Alt+Z was the cost). Open/close = one class flip = instant.
        ' THIRD state (oscengine-toast): the page shows a notification toast
        ' — .base visible but NO menu and a LIGHT dim, click-through kept.
        sb.Append("st.textContent='html.oscengine-open #oscengine-backdrop{display:block}html:not(.oscengine-open) .base{visibility:hidden!important}html.oscengine-toast #oscengine-backdrop{display:block;background:rgba(8,8,8,0.35)}html.oscengine-toast .base{visibility:visible!important}';")
        sb.Append("document.head.appendChild(st);")
        sb.Append("var bd=document.createElement('div');bd.id='oscengine-backdrop';")
        sb.Append("bd.style.cssText='position:fixed;left:0;top:0;width:100%;height:100%;background:rgba(8,8,8,0.35);z-index:-1;pointer-events:none;display:none';")
        sb.Append("document.body.appendChild(bd);window.__bdStatus='created';")
        sb.Append("}catch(e){window.__bdStatus='err:'+e.message;}};")
        sb.Append("if(document.body){bdfn();}else{document.addEventListener('DOMContentLoaded',bdfn);}}catch(e){window.__bdStatus='reg-err:'+e.message;}")
        ' FEATURE UNLOCK SHIM (owner directive "unlock ให้หมด"): the menu's
        ' Mods/Game-Filter tile is gated by nvCameraService.isOn(), which
        ' only turns true when the real NvBackend GFWSL feature service
        ' answers getAnselReady — a service our loopback host intentionally
        ' does not emulate (the real one needs NvContainer's trust chain).
        ' Patch the service methods after Angular bootstraps so the page
        ' renders ALL tiles; the actual filter engine stays with the
        ' "Game Filter เอง" milestone.
        sb.Append("try{window.__unlockStatus='reg';")
        sb.Append("var unlockFn=function(){")
        sb.Append("try{if(!window.angular) return false;")
        sb.Append("var inj=window.angular.element(document.body).injector(); if(!inj) return false;")
        sb.Append("var q=inj.get('$q'); var nv=inj.get('nvCameraService'); if(!nv||!nv.isOn) return false;")
        sb.Append("nv.isGfeAnselSupported=function(){return q.when(true);};")
        sb.Append("nv.isModsOn=function(){return true;};")
        sb.Append("nv.isOn=function(){return true;};")
        ' PREVIEW LAUNCHERS: the real launchers POST /NvCamera/v.1.1/*
        ' (driver-hooked capture — not available here) and die on 404.
        ' Redirect them to the page's own full-screen UIs so every screen
        ' is at least VIEWABLE (owner: "เปิด Preview ทุก UI ไปเลย").
        sb.Append("try{var st=inj.get('$state');")
        sb.Append("nv.launchUIForNvCamera=function(){st.go('nvcamera');};")
        sb.Append("nv.launchUIForMods=function(){st.go('mods');};")
        sb.Append("}catch(e){}")
        ' toggle through the page's OWN display service (openOSC/closeOSC —
        ' the same functions GFE's Alt+Z path uses) so state/hash stay in
        ' sync — raw hash edits from the host desynced ui-router (blank UI)
        sb.Append("try{var ds=inj.get('oscDisplayService');")
        sb.Append("window.__oscOpen=function(){ds.openOSC();};")
        sb.Append("window.__oscClose=function(){ds.closeOSC();};")
        sb.Append("}catch(e){}")
        sb.Append("window.__unlockStatus='done'; return true;")
        sb.Append("}catch(e){window.__unlockStatus='err:'+e.message; return false;}};")
        sb.Append("var uTries=0;")
        sb.Append("var uTimer=setInterval(function(){uTries++;")
        sb.Append("if(unlockFn()||uTries>150){clearInterval(uTimer);}} ,300);")
        sb.Append("}catch(e){}")
        sb.Append("})();")
        Return sb.ToString()
    End Function

    ' ── host side ──────────────────────────────────────────────

    ''' <summary>Handles a WebMessageReceived payload. Returns the response
    '     JSON to post back, or Nothing when nothing must be sent.</summary>
    Public Function HandleWebMessage(json As String) As String
        Try
            Dim el As System.Text.Json.JsonElement = System.Text.Json.JsonDocument.Parse(json).RootElement
            Dim isCef As System.Text.Json.JsonElement
            If el.ValueKind <> System.Text.Json.JsonValueKind.Object OrElse Not el.TryGetProperty("__cef", isCef) Then
                Return Nothing
            End If
            Dim id As Integer = GetInt(el, "id")
            Dim persistent As Boolean = GetBool(el, "persistent")
            Dim request As String = If(GetStr(el, "request"), "{}")

            Dim reqEl As System.Text.Json.JsonElement
            Try
                reqEl = System.Text.Json.JsonDocument.Parse(request).RootElement
            Catch ex As Exception
                Return FailResponse(id, persistent, -3, "bad request json")
            End Try
            If reqEl.ValueKind <> System.Text.Json.JsonValueKind.Object Then
                Return FailResponse(id, persistent, -3, "request not object")
            End If
            Dim cmd As String = GetStr(reqEl, "command")
            If String.IsNullOrEmpty(cmd) Then
                Return FailResponse(id, persistent, -3, "no command")
            End If
            Return Dispatch(id, persistent, cmd, reqEl)
        Catch ex As Exception
            Log("bridge parse error: " & ex.Message)
            Return Nothing
        End Try
    End Function

    Private Function Dispatch(id As Integer, persistent As Boolean, cmd As String, req As System.Text.Json.JsonElement) As String
        Select Case cmd
            Case "QUERY_WIN_NODE_INFO"
                ' boot-critical handshake: controller server port + auth secret
                Return OkResponse(id, persistent,
                    "{""port"":" & _port.ToString(CultureInfo.InvariantCulture) &
                    ",""secret"":""" & _secret & """}")

            Case "QUERY_FULLSCREEN_STATE"
                ' ALWAYS answer desktop (fullscreen=false): the in-game DLL
                ' draws the overlay into the game's frame itself — if the
                ' page learns "fullscreen game" it auto-dismisses the menu
                ' (measured: menu closed itself in Dungeons fullscreen).
                ' WinFullscreen probe kept for the log.
                Dim fs As Boolean = WinFullscreen.IsFullscreenActive()
                Dim payload As String = "{""fullscreen"":false,""hdractive"":false,""borderlessMode"":null}"
                RaiseEvent LogLine("fullscreen probe: " & WinFullscreen.LastFullscreenProbe &
                                   " → forced false (was " & If(fs, "TRUE", "false") & ")")
                Return OkResponse(id, persistent, payload)

            Case "QUERY_OSC_DISPLAY_IS_DESKTOP_MODE"
                ' We ARE the desktop (our WebView2 window shows over the
                ' desktop/borderless — no driver compositing) — "true" makes
                ' the osc render the FULL menu instead of the in-game
                ' compact sidebar.
                Return OkResponse(id, persistent, "true")

            Case "QUERY_OSC_SET_DISPLAY_RECTS"
                Dim rects As List(Of System.Drawing.Rectangle) = New List(Of System.Drawing.Rectangle)
                Dim rectsEl As System.Text.Json.JsonElement
                If req.TryGetProperty("displayRects", rectsEl) Then
                    rects = OscProtocol.ParseDisplayRects(rectsEl)
                End If
                DisplayRects = rects
                Log("displayRects=" & rects.Count.ToString())
                Return OkResponse(id, persistent, "true")

            Case "QUERY_OSC_SET_PAINTING"
                Dim enable As Boolean = GetBool(req, "enablePainting")
                PaintingEnabled = enable
                Log("painting=" & enable.ToString())
                Return OkResponse(id, persistent, "true")

            Case "QUERY_OSC_SET_EXPERIMENTAL"
                Return OkResponse(id, persistent, "true")

            Case "QUERY_OSC_REGISTER_CLOSE_EVENT"
                _closeQueryId = id
                ' persistent query: no immediate response body needed; push
                ' later via RequestCloseFromPage().
                Return If(persistent, Nothing, OkResponse(id, persistent, "true"))

            Case "QUERY_WIN_OPEN_OSC"
                RaiseEvent OpenOsc(GetBool(req, "enableInput"))
                Return OkResponse(id, persistent, "true")

            Case "QUERY_WIN_CLOSE_OSC"
                RaiseEvent CloseOsc()
                Return OkResponse(id, persistent, "true")

            Case "QUERY_READ_SHARED_STORAGE"
                Dim path As String = GetStr(req, "path")
                Return OkResponse(id, persistent, _storage.Read(path))

            Case "QUERY_WRITE_SHARED_STORAGE"
                _storage.Write(GetStr(req, "path"), GetStr(req, "data"))
                Return OkResponse(id, persistent, "true")

            Case "QUERY_LOAD_STRING_TABLE"
                ' page passes {stringTable:{userAgent, acceptLanguage?, onLoadError}}
                Return OkResponse(id, persistent, "true")

            Case "QUERY_WIN_COPY_TO_CLIPBOARD"
                Try
                    Dim text As String = GetStr(req, "clipBoardData")
                    If Not String.IsNullOrEmpty(text) Then
                        System.Windows.Forms.Clipboard.SetText(text)
                    End If
                Catch ex As Exception
                    Log("clipboard: " & ex.Message)
                End Try
                Return OkResponse(id, persistent, "true")

            Case "QUERY_HTTPSERVER_START"
                ' OAuth loopback capture — out of M1 scope. Fail fast so the
                ' login flow reports unavailable instead of hanging.
                Return FailResponse(id, persistent, -1, "oauth_not_implemented")

            Case Else
                Log("cefQuery not implemented: " & cmd)
                Return FailResponse(id, persistent, -1, "not_implemented")
        End Select
    End Function

    ''' <summary>Pushes a close request through the persistent
    '     QUERY_OSC_REGISTER_CLOSE_EVENT channel.</summary>
    Public Function RequestCloseFromPage() As String
        If _closeQueryId < 0 Then Return Nothing
        Return OkResponse(_closeQueryId, True, "true")
    End Function

    ' ── response builders (host → page JSON) ──

    Private Shared Function OkResponse(id As Integer, persistent As Boolean, response As String) As String
        Return "{""__cefResponse"":1,""id"":" & id.ToString(CultureInfo.InvariantCulture) &
               ",""ok"":true,""persistent"":" & If(persistent, "true", "false") &
               ",""response"":" & System.Text.Json.JsonSerializer.Serialize(response) & "}"
    End Function

    Private Shared Function FailResponse(id As Integer, persistent As Boolean, code As Integer, message As String) As String
        Return "{""__cefResponse"":1,""id"":" & id.ToString(CultureInfo.InvariantCulture) &
               ",""ok"":false,""persistent"":" & If(persistent, "true", "false") &
               ",""errorCode"":" & code.ToString(CultureInfo.InvariantCulture) &
               ",""response"":" & System.Text.Json.JsonSerializer.Serialize(message) & "}"
    End Function

    ' ── small JSON helpers ──

    Friend Shared Function GetInt(obj As System.Text.Json.JsonElement, name As String) As Integer
        Dim v As System.Text.Json.JsonElement
        If obj.TryGetProperty(name, v) AndAlso v.ValueKind = System.Text.Json.JsonValueKind.Number Then
            Return CInt(v.GetDouble())
        End If
        Return 0
    End Function

    Friend Shared Function GetBool(obj As System.Text.Json.JsonElement, name As String) As Boolean
        Dim v As System.Text.Json.JsonElement
        If obj.TryGetProperty(name, v) AndAlso v.ValueKind = System.Text.Json.JsonValueKind.True Then
            Return True
        End If
        Return False
    End Function

    Friend Shared Function GetStr(obj As System.Text.Json.JsonElement, name As String) As String
        Dim v As System.Text.Json.JsonElement
        If obj.TryGetProperty(name, v) AndAlso v.ValueKind = System.Text.Json.JsonValueKind.String Then
            Return v.GetString()
        End If
        Return Nothing
    End Function

    Private Sub Log(message As String)
        RaiseEvent LogLine(message)
        Debug.WriteLine("[OscEngine/cef] " & message)
    End Sub

End Class

''' <summary>Fullscreen probe for QUERY_FULLSCREEN_STATE. Foreground-window
'     bounds == the monitor bounds of that window ⇒ fullscreen (borderless
'     or exclusive; osc treats both the same way).</summary>
Public NotInheritable Class WinFullscreen

    Private Sub New()
    End Sub

    Public Declare Auto Function GetForegroundWindow Lib "user32" () As IntPtr
    Public Declare Auto Function GetWindowRect Lib "user32" (hWnd As IntPtr, ByRef rect As WinRect) As Boolean
    Public Declare Auto Function GetWindowThreadProcessId Lib "user32" (hWnd As IntPtr, ByRef pid As Integer) As Integer
    Public Declare Auto Function GetWindowLong Lib "user32" (hWnd As IntPtr, index As Integer) As Integer
    Public Declare Auto Function GetClassName Lib "user32" (hWnd As IntPtr, builder As Text.StringBuilder, count As Integer) As Integer

    Public Structure WinRect
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    Private Const GWL_STYLE As Integer = -16
    Private Const WS_CAPTION As Integer = &HC00000

    Public Event LogLine(message As String)

    ''' <summary>Decisive diagnostic: which window made us say "fullscreen".
    '     Logged once per state change via the bridge log.</summary>
    Public Shared LastFullscreenProbe As String = ""

    Public Shared Function IsFullscreenActive() As Boolean
        Try
            ' In-game mode renders through the injected Present hook while
            ' the game remains the foreground borderless window. Treating
            ' that window as a fullscreen transition makes osc immediately
            ' close the overlay after every in-game toggle.
            If HookFramePump.HookLive() Then
                LastFullscreenProbe = "in-game hook live"
                Return False
            End If
            Dim hwnd As IntPtr = GetForegroundWindow()
            If hwnd = IntPtr.Zero Then Return False
            ' OUR OWN overlay covers the whole monitor the moment it opens —
            ' reporting it as "fullscreen game" made the page auto-dismiss
            ' the menu ~0.5s after every open (measured 03:28). Only a
            ' FOREIGN process's window counts as the game.
            Dim pid As Integer = 0
            GetWindowThreadProcessId(hwnd, pid)
            If pid = Process.GetCurrentProcess().Id Then
                LastFullscreenProbe = "own-process pid=" & pid
                Return False
            End If
            ' A MAXIMIZED desktop app (Telegram etc.) also covers the monitor
            ' — its invisible resize borders overshoot by a few px — but it
            ' keeps WS_CAPTION. A borderless game window does not.
            Dim style As Integer = GetWindowLong(hwnd, GWL_STYLE)
            Dim r As WinRect
            If Not GetWindowRect(hwnd, r) Then Return False
            Dim screen As System.Drawing.Rectangle = System.Windows.Forms.Screen.FromHandle(hwnd).Bounds
            Dim covers As Boolean = r.Left <= screen.Left AndAlso r.Top <= screen.Top AndAlso
                                    r.Right >= screen.Right AndAlso r.Bottom >= screen.Bottom
            LastFullscreenProbe = "hwnd=0x" & hwnd.ToInt64().ToString("X") & " pid=" & pid &
                                  " style=0x" & style.ToString("X") &
                                  " rect=(" & r.Left & "," & r.Top & ")-(" & r.Right & "," & r.Bottom & ")" &
                                  " screen=" & screen.Width & "x" & screen.Height &
                                  " covers=" & covers.ToString()
            If (style And WS_CAPTION) <> 0 Then Return False
            Return covers
        Catch
            Return False
        End Try
    End Function

End Class
