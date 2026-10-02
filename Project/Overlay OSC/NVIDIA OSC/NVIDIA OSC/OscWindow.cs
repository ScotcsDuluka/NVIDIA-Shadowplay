// OscWindow.cs — หน้าต่าง overlay WPF โปร่งใส per-pixel (เลียน Share.exe)
//
// การเรนเดอร์: CefSharp OSR → WPF AllowsTransparency window พื้น Transparent
//   ส่วนที่หน้าเว็บไม่ได้วาด = alpha 0 = มองทะลุ + คลิกทะลุ (WPF ทำเอง)
//
// วงจร:
//   Alt+Z → nvsphelper → :59002 (หน้า toggle ผ่าน socket.io ของ node)
//                          → :59003 (host ซ่อน/โชว์หน้าต่าง)
//   host ซ่อน → ยิง success เข้า QUERY_OSC_REGISTER_CLOSE_EVENT (เหมือนของแท้)
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;

public sealed class OscWindow : Window
{
    readonly OscConfig _cfg;
    readonly ChromiumWebBrowser _browser;
    HttpListener _toggle;
    HttpListener _pages;

    public OscWindow(OscConfig cfg)
    {
        _cfg = cfg;

        // หน้าต่างกระจก: ไม่มีกรอบ โปร่งใสจริง บนสุด ไม่โผล่ taskbar
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        Topmost = cfg.TopMost;
        ShowInTaskbar = false;
        ShowActivated = false;
        ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.Manual;

        // เต็ม monitor หลัก
        var scr = SystemParameters.WorkArea; // ใช้ work area ก่อน (taskbar เผื่อไว้)
        Left = 0; Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;

        // หน้ามาจาก http://localhost:3000 (origin ที่หน้าคาดหวัง — แก้ CORS)
        // หน้ายิง API ตรงไป node :59001 (patch LOCALHOST_PORT + node มี CORS *)
        StartPageServer();

        _browser = new ChromiumWebBrowser
        {
            Background = Brushes.Transparent,   // OSR: ส่วนที่หน้าไม่วาด = โปร่งทะลุหน้าต่าง WPF
        };
        _browser.Load("http://localhost:" + PageServerPort + "/index.html");

        // วาล์วประหยัด CPU (เหมือน OSD scale factor ของ Share แท้):
        // เรนเดอร์ที่ resolution ต่ำกว่าจอ (renderScale) แล้วขยายเต็มหน้าต่างด้วย transform
        var scale = Math.Clamp(_cfg.RenderScale, 0.25, 1.0);
        _browser.Width = Math.Round(Width * scale);
        _browser.Height = Math.Round(Height * scale);
        _browser.LayoutTransform = new ScaleTransform(1.0 / scale, 1.0 / scale);

        Content = _browser;

        _browser.FrameLoadEnd += (s, e) =>
        {
            if (!e.Frame.IsMain) return;
            e.Frame.ExecuteJavaScriptAsync(CefQueryShim);
            Log("[cef] frame loaded — cefQuery shim injected");
        };

        _browser.JavascriptMessageReceived += OnPageQuery;
        _browser.ConsoleMessage += (s, e) =>
        {
            if (e.Level is LogSeverity.Error or LogSeverity.Warning)
                Log("[page:" + e.Level + "] " + e.Message);
        };
        _browser.LoadError += (s, e) => Log("[loaderror] " + e.ErrorCode + " " + e.FailedUrl);

        Loaded += (s, e) =>
        {
            if (!cfg.ShowOnBoot) Hide();
            StartToggleListener();
            Log("=== NVIDIA OSC (ของเรา, WPF OSR alpha) boot — " + _cfg.Page + " ===");
        };
    }

    // window.cefQuery shim
    const string CefQueryShim = @"
(function(){
  if (window.cefQuery) return;
  window.__cefQuerySuccess = null;
  window.__cefQueryFailure = null;
  window.cefQuery = function(q){
    try{
      window.__cefQuerySuccess = q && q.onSuccess ? q.onSuccess : null;
      window.__cefQueryFailure = q && q.onFailure ? q.onFailure : null;
      CefSharp.PostMessage(JSON.stringify({ __cefQuery: true, request: (q && q.request) ? q.request : '' }));
    }catch(e){
      if (window.__cefQueryFailure) window.__cefQueryFailure(-1, String(e));
    }
  };
})();";

    void OnPageQuery(object sender, JavascriptMessageReceivedEventArgs e)
    {
        try
        {
            using var doc = JsonDocument.Parse(e.Message.ToString()!);
            var r = doc.RootElement;
            if (!r.TryGetProperty("__cefQuery", out var flag) || !flag.GetBoolean()) return;
            var request = r.TryGetProperty("request", out var req) ? req.GetString() : "";

            string command = "";
            try
            {
                using var rq = JsonDocument.Parse(request ?? "");
                if (rq.RootElement.TryGetProperty("command", out var c)) command = c.GetString() ?? "";
            }
            catch { }

            Log("[cefQuery] " + command);

            switch (command)
            {
                case "QUERY_FULLSCREEN_STATE":
                    ReplySuccess("{'fullscreen':false,'hdractive':false,'borderlessMode':false}");
                    break;
                case "QUERY_OSC_REGISTER_CLOSE_EVENT":
                    _closeRegistered = true;
                    break;
                case "QUERY_WIN_CLOSE_OSC":
                    ReplySuccess("{}");
                    Dispatcher.BeginInvoke(() => { Hide(); Log("[window] HIDE (page requested)"); });
                    break;
                case "QUERY_WIN_OPEN_OSC":
                    ReplySuccess("{}");
                    break;
                case "QUERY_HTTPSERVER_START":
                    ExecuteOnUi("if(window.__cefQueryFailure) window.__cefQueryFailure(-1,'not supported')");
                    break;
                default:
                    ReplySuccess("{}");
                    break;
            }
        }
        catch (Exception ex) { Log("[cefQuery] ERR " + ex.Message); }
    }

    bool _closeRegistered;

    void ReplySuccess(string payload)
    {
        var js = "if(window.__cefQuerySuccess) window.__cefQuerySuccess(" +
                 JsonSerializer.Serialize(payload) + ");";
        ExecuteOnUi(js);
    }

    void ExecuteOnUi(string js) => Dispatcher.BeginInvoke(() => _browser.ExecuteScriptAsync(js));

    // ---- toggle listener :59003 ----
    void StartToggleListener()
    {
        try
        {
            _toggle = new HttpListener();
            _toggle.Prefixes.Add("http://127.0.0.1:" + _cfg.TogglePort + "/");
            _toggle.Start();
            _toggle.BeginGetContext(OnToggle, null);
            Log("[toggle] listening on 127.0.0.1:" + _cfg.TogglePort);
        }
        catch (Exception ex) { Log("[toggle] FAIL " + ex.Message); }
    }

    void OnToggle(IAsyncResult ar)
    {
        HttpListenerContext ctx = null;
        try { ctx = _toggle.EndGetContext(ar); } catch { }
        try { _toggle.BeginGetContext(OnToggle, null); } catch { }
        if (ctx == null) return;

        var path = ctx.Request.Url!.AbsolutePath.ToLowerInvariant();
        try
        {
            ctx.Response.StatusCode = 200;
            var body = System.Text.Encoding.UTF8.GetBytes("ok");
            ctx.Response.OutputStream.Write(body, 0, body.Length);
        }
        catch { }

        Dispatcher.BeginInvoke(() =>
        {
            switch (path)
            {
                case "/toggle":
                    if (IsVisible) HideOsc(); else ShowOsc();
                    break;
                case "/show": ShowOsc(); break;
                case "/hide": HideOsc(); break;
            }
        });
    }

    void ShowOsc()
    {
        Show();
        Topmost = false; Topmost = _cfg.TopMost;
        Activate();                       // overlay แท้รับ focus ตอนเปิด
        try { _browser.GetBrowser().GetHost().WasHidden(false); } catch { }
        Log("[window] SHOW");
    }

    void HideOsc()
    {
        if (_closeRegistered)
            _browser.ExecuteScriptAsync(
                "if(window.__cefQuerySuccess) window.__cefQuerySuccess('close message');");
        Hide();
        try { _browser.GetBrowser().GetHost().WasHidden(true); } catch { }
        Log("[window] HIDE (+close event fired)");
    }

    // ---- page server :3000 ----
    const int PageServerPort = 3000;

    void StartPageServer()
    {
        try
        {
            _pages = new HttpListener();
            _pages.Prefixes.Add("http://localhost:" + PageServerPort + "/");
            _pages.Start();
            _pages.BeginGetContext(OnPageFile, null);
            Log("[pages] serving " + Path.GetDirectoryName(_cfg.Page) + " at http://localhost:" + PageServerPort + "/");
        }
        catch (Exception ex) { Log("[pages] FAIL " + ex.Message); }
    }

    void OnPageFile(IAsyncResult ar)
    {
        HttpListenerContext ctx = null;
        try { ctx = _pages.EndGetContext(ar); } catch { }
        try { _pages.BeginGetContext(OnPageFile, null); } catch { }
        if (ctx == null) return;

        try
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(_cfg.Page))!;
            var rel = ctx.Request.Url!.AbsolutePath.TrimStart('/').Replace('/', '\\');
            if (string.IsNullOrEmpty(rel)) rel = "index.html";
            var full = Path.GetFullPath(Path.Combine(dir, rel));
            if (!full.StartsWith(dir, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
            {
                ctx.Response.StatusCode = 404;
                ctx.Response.Close();
                return;
            }
            ctx.Response.ContentType = Mime(full);
            ctx.Response.AddHeader("Access-Control-Allow-Origin", "*");
            var bytes = File.ReadAllBytes(full);
            ctx.Response.ContentLength64 = bytes.Length;
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.Close();
        }
        catch (Exception ex)
        {
            Log("[pages] ERR " + ex.Message);
            try { ctx.Response.Close(); } catch { }
        }
    }

    static string Mime(string path) => path.ToLowerInvariant() switch
    {
        var p when p.EndsWith(".html") => "text/html",
        var p when p.EndsWith(".js") => "application/javascript",
        var p when p.EndsWith(".css") => "text/css",
        var p when p.EndsWith(".json") => "application/json",
        var p when p.EndsWith(".png") => "image/png",
        var p when p.EndsWith(".svg") => "image/svg+xml",
        var p when p.EndsWith(".woff2") => "font/woff2",
        var p when p.EndsWith(".ico") => "image/x-icon",
        _ => "application/octet-stream",
    };

    void Log(string m)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_cfg.LogFile)!);
            File.AppendAllText(_cfg.LogFile,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + m + Environment.NewLine);
        }
        catch { }
    }
}
