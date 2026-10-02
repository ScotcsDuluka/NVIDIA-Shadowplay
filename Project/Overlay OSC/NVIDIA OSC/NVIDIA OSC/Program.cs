// Program.cs — NVIDIA OSC (ของเรา): WPF + CefSharp OSR host เลียน NVIDIA Share.exe
using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;

static class Program
{
    const string CONFIG = @"C:\My Project\NVIDIA-Shadowplay\Project\NvConfig\nvidia-osc.json";

    [STAThread]
    static void Main()
    {
        bool created;
        var single = new System.Threading.Mutex(true, @"Local\Duluka.NVIDIA OSC", out created);
        if (!created) return;
        GC.KeepAlive(single);

        var cfg = OscConfig.Load(CONFIG);

        var settings = new CefSettings
        {
            CachePath = cfg.CachePath,
            LogSeverity = LogSeverity.Warning,
            RemoteDebuggingPort = cfg.DebugPort,
            // เหมือน Share แท้ (log แท้: "offscreen_window"): OSR + clear โปร่งใส
            WindowlessRenderingEnabled = true,
            BackgroundColor = 0,
        };
        Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

        var app = new Application { ShutdownMode = ShutdownMode.OnMainWindowClose };
        app.Run(new OscWindow(cfg));
    }
}

// ---- config (Project\NvConfig\nvidia-osc.json) ----
public sealed class OscConfig
{
    public string Page { get; set; } = @"C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA OSC\osc\index.html";
    public string CachePath { get; set; } = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA OSC\CefCache";
    public string LogFile { get; set; } = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Logs\NVIDIA OSC.log";
    public int TogglePort { get; set; } = 59003;
    public int DebugPort { get; set; } = 0;
    public bool ShowOnBoot { get; set; } = false;
    public bool TopMost { get; set; } = true;
    public double RenderScale { get; set; } = 0.75;   // เรนเดอร์ต่ำกว่าจอแล้วขยาย (เหมือน OSD scale ของแท้)
    public int FrameRate { get; set; } = 20;

    public static OscConfig Load(string path)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var r = doc.RootElement;
            var c = new OscConfig();
            if (r.TryGetProperty("page", out var v)) c.Page = v.GetString();
            if (r.TryGetProperty("cef", out var cef) && cef.TryGetProperty("cachePath", out v)) c.CachePath = v.GetString();
            if (r.TryGetProperty("logFile", out v)) c.LogFile = v.GetString();
            if (r.TryGetProperty("toggle", out v)) c.TogglePort = v.GetInt32();
            if (r.TryGetProperty("debugPort", out v)) c.DebugPort = v.GetInt32();
            if (r.TryGetProperty("window", out var w))
            {
                if (w.TryGetProperty("showOnBoot", out v)) c.ShowOnBoot = v.GetBoolean();
                if (w.TryGetProperty("topMost", out v)) c.TopMost = v.GetBoolean();
                if (w.TryGetProperty("renderScale", out v)) c.RenderScale = v.GetDouble();
                if (w.TryGetProperty("frameRate", out v)) c.FrameRate = v.GetInt32();
            }
            return c;
        }
        catch { return new OscConfig(); }
    }
}
