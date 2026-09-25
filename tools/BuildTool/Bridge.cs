using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace BuildTool;

public class VersionCfg
{
    public bool hardcore { get; set; }
    public string version { get; set; } = "3.41";
    public string company { get; set; } = "Duluka Corporation";
    public string authors { get; set; } = "ScotcsDuluka";
    public string product { get; set; } = "NVIDIA ShadowPlay";
    public string copyright { get; set; } = "Copyright (C) 2026 Duluka Corporation";
}

/// <summary>Host object ที่ JS เรียกผ่าน window.chrome.webview.hostObjects.async.host</summary>
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDual)]
public class Bridge
{
    private readonly string _root;
    private readonly string _buildDir;
    private readonly string _staged;
    private readonly string _script;
    private readonly object _lock = new();
    private readonly List<string> _log = new();
    private Process _proc;
    private volatile bool _running;
    private int _exit = -1;

    public Bridge(string root)
    {
        _root = root;
        _buildDir = Path.Combine(root, "Build", "Build-Config");
        _staged = Path.Combine(root, "Build", "NVIDIA ShadowPlay");
        _script = Path.Combine(root, "Scripts", "build-dev.ps1");
        EnsureDefaults();
    }

    // ─── Dashboard ───

    public string GetStatus()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(new
            {
                buildNo = ReadCounter(),
                commit = GitShort(),
                running = _running,
                exit = _running ? (int?)null : _exit,
                procs = new[]
                {
                    new { name = "NvContainer",  up = UpOurs("NvContainer") },
                    new { name = "nvsphelper64", up = UpOurs("nvsphelper64") },
                    new { name = "NvBackend",    up = UpOurs("NvBackend") },
                    new { name = "NvShadowPlay", up = UpOurs("NvShadowPlay") },
                    new { name = "NvNotifier",   up = UpOurs("NvNotifier") },
                }
            });
        }
    }

    // ─── Build ───

    public string StartBuild(bool clean)
    {
        lock (_lock)
        {
            if (_running)
                return JsonSerializer.Serialize(new { ok = false, error = "build is already running" });
            if (!File.Exists(_script))
                return JsonSerializer.Serialize(new { ok = false, error = "Scripts\\build-dev.ps1 not found" });

            _log.Clear();
            _running = true;
            _exit = -1;

            var no = ReadCounter() + 1;
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + _script + "\"" + (clean ? " -Clean" : ""),
                    WorkingDirectory = _root,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };
            _proc.OutputDataReceived += (_, e) => { if (e.Data != null) Log(e.Data); };
            _proc.ErrorDataReceived += (_, e) => { if (e.Data != null) Log(e.Data); };
            _proc.Exited += (_, _) =>
            {
                lock (_lock)
                {
                    _running = false;
                    _exit = _proc.ExitCode;
                    if (_exit == 0)
                        WriteCounter(ReadCounter() + 1);
                    Log("[Build.exe] BUILD OK - counter -> " + ReadCounter());
                }
            };
            _proc.Start();
            _proc.BeginOutputReadLine();
            _proc.BeginErrorReadLine();
        }
        return JsonSerializer.Serialize(new { ok = true, no = ReadCounter() });
    }

    public string GetLog(int since)
    {
        lock (_lock)
        {
            var lines = new List<string>();
            for (int i = since; i < _log.Count && lines.Count < 400; i++)
                lines.Add(_log[i]);
            return JsonSerializer.Serialize(new
            {
                running = _running,
                exit = _exit,
                total = _log.Count,
                lines,
            });
        }
    }

    // ─── Version (Hardcore) ───

    public string GetVersionConfig()
    {
        var c = ReadVersionConfig();
        var sample = c.hardcore
            ? c.version + "." + ReadCounter() + ".61"
            : "3.41." + ReadCounter() + ".61 (unlock)";
        return JsonSerializer.Serialize(new
        {
            c.hardcore,
            c.version,
            c.company,
            c.authors,
            c.product,
            c.copyright,
            buildNo = ReadCounter(),
            sample,
        });
    }

    public string SaveVersionConfig(string json)
    {
        try
        {
            var cfg = JsonSerializer.Deserialize<VersionCfg>(json) ?? new VersionCfg();
            if (string.IsNullOrWhiteSpace(cfg.version)) cfg.version = "3.41";
            Directory.CreateDirectory(_buildDir);
            File.WriteAllText(Path.Combine(_buildDir, "version.json"),
                JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
            RegenVersionProps(cfg);
            return JsonSerializer.Serialize(new { ok = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { ok = false, error = ex.Message });
        }
    }

    // ─── Preview ───

    public string GetPreview()
    {
        var files = new List<object>();
        long total = 0;
        int exeCount = 0;

        if (Directory.Exists(_staged))
        {
            foreach (var f in Directory.EnumerateFiles(_staged, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var fi = new FileInfo(f);
                    total += fi.Length;
                    if (fi.Extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) && exeCount < 60)
                    {
                        var vi = FileVersionInfo.GetVersionInfo(f);
                        exeCount++;
                        files.Add(new
                        {
                            name = fi.Name,
                            rel = f.Substring(_staged.Length + 1),
                            ver = string.IsNullOrEmpty(vi.FileVersion) ? "-" : vi.FileVersion,
                            product = string.IsNullOrEmpty(vi.ProductName) ? "-" : vi.ProductName,
                            kb = (int)(fi.Length / 1024),
                        });
                    }
                }
                catch { }
            }
        }

        return JsonSerializer.Serialize(new
        {
            exists = Directory.Exists(_staged),
            totalMB = Math.Round(total / 1048576.0, 1),
            files,
        });
    }

    public string LaunchApp()
    {
        var exe = Path.Combine(_staged, "Launcher.exe");
        if (!File.Exists(exe)) return JsonSerializer.Serialize(new { ok = false, error = "Launcher.exe missing" });
        Process.Start(new ProcessStartInfo { FileName = exe, WorkingDirectory = Path.GetDirectoryName(exe) });
        return JsonSerializer.Serialize(new { ok = true });
    }

    public string OpenFolder()
    {
        if (Directory.Exists(_staged)) Process.Start("explorer.exe", _staged);
        return JsonSerializer.Serialize(new { ok = true });
    }

    // ─── internals ───

    private void Log(string line)
    {
        lock (_lock) _log.Add($"[{DateTime.Now:HH:mm:ss}] {line}");
    }

    private string VersionJsonPath => Path.Combine(_buildDir, "version.json");
    private string VersionPropsPath => Path.Combine(_buildDir, "version.props");

    private VersionCfg ReadVersionConfig()
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(VersionJsonPath));
            var r = doc.RootElement;
            var c = new VersionCfg();
            if (r.TryGetProperty("hardcore", out var h)) c.hardcore = h.ValueKind == JsonValueKind.True;
            if (r.TryGetProperty("version", out var v) && !string.IsNullOrWhiteSpace(v.GetString())) c.version = v.GetString();
            if (r.TryGetProperty("company", out var co) && !string.IsNullOrWhiteSpace(co.GetString())) c.company = co.GetString();
            if (r.TryGetProperty("authors", out var au) && !string.IsNullOrWhiteSpace(au.GetString())) c.authors = au.GetString();
            if (r.TryGetProperty("product", out var pr) && !string.IsNullOrWhiteSpace(pr.GetString())) c.product = pr.GetString();
            if (r.TryGetProperty("copyright", out var cr) && !string.IsNullOrWhiteSpace(cr.GetString())) c.copyright = cr.GetString();
            return c;
        }
        catch
        {
            return new VersionCfg();
        }
    }

    private void EnsureDefaults()
    {
        try
        {
            Directory.CreateDirectory(_buildDir);
            if (!File.Exists(VersionJsonPath))
                File.WriteAllText(VersionJsonPath,
                    JsonSerializer.Serialize(ReadVersionConfig(), new JsonSerializerOptions { WriteIndented = true }));
            RegenVersionProps(ReadVersionConfig());
        }
        catch { }
    }

    private void RegenVersionProps(VersionCfg c)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<Project>");
        sb.AppendLine("  <!-- GENERATED by Build.exe - edit via Build.exe Version page only -->");
        sb.AppendLine("  <PropertyGroup>");
        sb.AppendLine($"    <Company>{XmlEsc(c.company)}</Company>");
        sb.AppendLine($"    <Authors>{XmlEsc(c.authors)}</Authors>");
        sb.AppendLine($"    <Product>{XmlEsc(c.product)}</Product>");
        sb.AppendLine($"    <Copyright>{XmlEsc(c.copyright)}</Copyright>");
        if (c.hardcore)
            sb.AppendLine($"    <HardcoreVersion>{XmlEsc(c.version)}</HardcoreVersion>");
        sb.AppendLine("  </PropertyGroup>");
        sb.AppendLine("</Project>");
        Directory.CreateDirectory(_buildDir);
        File.WriteAllText(VersionPropsPath, sb.ToString());
    }

    private static string XmlEsc(string s) => s
        .Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");

    private int ReadCounter()
    {
        try
        {
            var p = Path.Combine(_buildDir, "build.txt");
            if (int.TryParse(File.ReadAllText(p).Trim(), out var n)) return n;
        }
        catch { }
        return 0;
    }

    private void WriteCounter(int n)
    {
        Directory.CreateDirectory(_buildDir);
        File.WriteAllText(Path.Combine(_buildDir, "build.txt"), n.ToString());
    }

    private string GitShort()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --short HEAD",
                WorkingDirectory = _root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            var outp = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();
            return outp.Length > 0 ? outp : "unknown";
        }
        catch { return "unknown"; }
    }

    private bool UpOurs(string name)
    {
        foreach (var p in Process.GetProcessesByName(name))
        {
            try
            {
                var path = p.MainModule?.FileName;
                if (path != null && path.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch { }
        }
        return false;
    }
}
