using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace Duluka.Server.Manager;

/// <summary>
/// Local operator control panel for Duluka.Server — deliberately simple:
/// Start/Stop the server process, a live log console, a data-folder shortcut
/// and a small stats strip read straight from the SQLite file (read-only).
/// Dark palette matches the Overlay's Connect surface (#2E3439 family).
/// </summary>
public sealed class MainForm : Form
{
    private const string AdminUrl = "http://127.0.0.1:5115/admin";
    private const int MaxLogLines = 4000;

    // Overlay dark palette
    private static readonly Color FormBg = Color.FromArgb(34, 39, 43);
    private static readonly Color PanelBg = Color.FromArgb(46, 52, 57);
    private static readonly Color Panel2 = Color.FromArgb(52, 59, 65);
    private static readonly Color Border = Color.FromArgb(61, 69, 76);
    private static readonly Color LogBg = Color.FromArgb(28, 33, 37);
    private static readonly Color TextCol = Color.FromArgb(236, 239, 241);
    private static readonly Color Muted = Color.FromArgb(154, 164, 171);
    private static readonly Color Mint = Color.FromArgb(63, 222, 158);
    private static readonly Color Danger = Color.FromArgb(240, 97, 109);

    private Process? _server;
    private DateTime? _startedAt;
    private string? _serverDir;

    private readonly Label _statusDot = new();
    private readonly Label _statusText = new();
    private readonly Button _btStart = new();
    private readonly Button _btStop = new();
    private readonly Button _btFolder = new();
    private readonly Button _btAdmin = new();
    private Label _lbAccounts = new();
    private Label _lbSessions = new();
    private Label _lbDevices = new();
    private Label _lbLinks = new();
    private readonly TextBox _log = new();
    private readonly CheckBox _cbAutoScroll = new();
    private readonly System.Windows.Forms.Timer _tick = new();

    public MainForm()
    {
        Text = "Duluka Server Manager";
        BackColor = FormBg;
        ForeColor = TextCol;
        Font = new Font("Segoe UI", 9.75f);
        MinimumSize = new Size(760, 520);
        Size = new Size(880, 600);
        StartPosition = FormStartPosition.CenterScreen;

        BuildTopBar();
        BuildLog();
        BuildBottomBar();

        _tick.Interval = 2000;
        _tick.Tick += (_, _) => { UpdateUptime(); RefreshStats(); };
        _tick.Start();

        Shown += (_, _) => { LocateServer(); RefreshStats(); };
        FormClosing += OnFormClosing;
    }

    // ── UI construction ─────────────────────────────────────────────────────

    private void BuildTopBar()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 128, BackColor = PanelBg };
        Controls.Add(top);

        var statusRow = new FlowLayoutPanel
        {
            Location = new Point(16, 14), Size = new Size(top.Width - 32, 44),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = PanelBg, WrapContents = false,
        };

        _statusDot.Size = new Size(14, 14);
        _statusDot.Margin = new Padding(2, 12, 2, 0);
        SetDot(false);

        _statusText.AutoSize = true;
        _statusText.ForeColor = TextCol;
        _statusText.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        _statusText.Margin = new Padding(6, 8, 18, 0);
        _statusText.Text = "Stopped";

        StyleButton(_btStart, "Start", mint: true);
        _btStart.Click += (_, _) => StartServer();
        StyleButton(_btStop, "Stop");
        _btStop.Click += (_, _) => StopServer();
        _btStop.ForeColor = Danger;
        _btStop.Enabled = false;
        StyleButton(_btFolder, "Open data folder");
        _btFolder.Click += (_, _) => OpenDataFolder();
        StyleButton(_btAdmin, "Open admin console");
        _btAdmin.Click += (_, _) => OpenAdmin();

        statusRow.Controls.AddRange([_statusDot, _statusText, _btStart, _btStop, _btFolder, _btAdmin]);
        top.Controls.Add(statusRow);

        var statsRow = new FlowLayoutPanel
        {
            Location = new Point(16, 72), Size = new Size(top.Width - 32, 44),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            BackColor = PanelBg, WrapContents = false,
        };
        var (capAcc, valAcc) = StatLabel("ACCOUNTS");
        var (capSess, valSess) = StatLabel("ACTIVE SESSIONS");
        var (capDev, valDev) = StatLabel("ACTIVE DEVICES");
        var (capLink, valLink) = StatLabel("ACTIVE LINKS");
        _lbAccounts = valAcc;
        _lbSessions = valSess;
        _lbDevices = valDev;
        _lbLinks = valLink;
        statsRow.Controls.AddRange([capAcc, valAcc, capSess, valSess, capDev, valDev, capLink, valLink]);
        top.Controls.Add(statsRow);
    }

    private static (Label Caption, Label Value) StatLabel(string caption) =>
    (
        new Label
        {
            AutoSize = true, ForeColor = Muted,
            Font = new Font("Segoe UI", 8.5f),
            Text = caption, Margin = new Padding(0, 8, 4, 0),
        },
        new Label
        {
            AutoSize = true, ForeColor = Mint,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            Text = "—", Margin = new Padding(2, 4, 36, 0),
        }
    );

    private void BuildLog()
    {
        _log.Dock = DockStyle.Fill;
        _log.Multiline = true;
        _log.ReadOnly = true;
        _log.ScrollBars = ScrollBars.Vertical;
        _log.BackColor = LogBg;
        _log.ForeColor = TextCol;
        _log.Font = new Font("Consolas", 9.75f);
        _log.BorderStyle = BorderStyle.None;
        Controls.Add(_log);
        _log.BringToFront();
    }

    private void BuildBottomBar()
    {
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 44, BackColor = PanelBg };
        Controls.Add(bottom);
        bottom.BringToFront();

        _cbAutoScroll.Text = "Auto-scroll";
        _cbAutoScroll.Checked = true;
        _cbAutoScroll.AutoSize = true;
        _cbAutoScroll.ForeColor = Muted;
        _cbAutoScroll.Location = new Point(16, 12);

        var btClear = new Button();
        StyleButton(btClear, "Clear log");
        btClear.Location = new Point(120, 9);
        btClear.Click += (_, _) => _log.Clear();

        var btCopy = new Button();
        StyleButton(btCopy, "Copy log");
        btCopy.Location = new Point(215, 9);
        btCopy.Click += (_, _) =>
        {
            if (_log.Text.Length > 0) Clipboard.SetText(_log.Text);
        };

        bottom.Controls.Add(_cbAutoScroll);
        bottom.Controls.Add(btClear);
        bottom.Controls.Add(btCopy);
    }

    private void StyleButton(Button b, string text, bool mint = false)
    {
        b.Text = text;
        b.AutoSize = true;
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderColor = Border;
        b.FlatAppearance.MouseOverBackColor = mint
            ? Color.FromArgb(82, 232, 178) : Color.FromArgb(64, 72, 79);
        b.BackColor = mint ? Mint : Panel2;
        b.ForeColor = mint ? Color.FromArgb(20, 32, 26) : TextCol;
        b.Padding = new Padding(8, 4, 8, 4);
        b.Margin = new Padding(2, 6, 2, 0);
    }

    // ── server location ─────────────────────────────────────────────────────

    private void LocateServer()
    {
        _serverDir = FindServerDir();
        if (_serverDir is null)
        {
            AppendLog("[manager] Duluka.Server build not found next to the manager.\r\n");
            AppendLog("[manager] Build it first (dotnet build Duluka/Duluka.Server) or pick the DLL on next Start.\r\n");
        }
        else
        {
            AppendLog($"[manager] Server build: {_serverDir}\r\n");
        }
    }

    private string? FindServerDir()
    {
        // Repo layout: Duluka/Duluka.Server.Manager/bin/{Config}/net10.0-windows/
        //           → Duluka/Duluka.Server/bin/{Config}/net10.0/
        string? dir = AppContext.BaseDirectory;
        for (var dirInfo = new DirectoryInfo(dir);
             dirInfo is not null;
             dirInfo = dirInfo.Parent)
        {
            foreach (var config in new[] { "Debug", "Release" })
            {
                var candidate = Path.Combine(dirInfo.FullName, "Duluka.Server", "bin", config, "net10.0");
                if (IsServerDir(candidate)) return candidate;
            }
            var publish = Path.Combine(dirInfo.FullName, "publish");
            if (IsServerDir(publish)) return publish;
        }
        return null;
    }

    private static bool IsServerDir(string dir) =>
        File.Exists(Path.Combine(dir, "Duluka.Server.dll"));

    private string? ResolveServerDb()
    {
        var dir = _serverDir ?? FindServerDir();
        if (dir is null) return null;
        var path = Path.Combine(dir, "AppData", "DulukaAccount.db");
        return File.Exists(path) ? path : null;
    }

    // ── start / stop ────────────────────────────────────────────────────────

    private void StartServer()
    {
        if (_server is not null && !_server.HasExited) return;

        _serverDir ??= FindServerDir();
        if (_serverDir is null)
        {
            using var pick = new OpenFileDialog
            {
                Title = "Pick Duluka.Server.dll (or Duluka.Server.exe)",
                Filter = "Duluka.Server.dll|Duluka.Server.dll|Duluka.Server.exe|Duluka.Server.exe",
            };
            if (pick.ShowDialog(this) != DialogResult.OK) return;
            _serverDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(pick.FileName)));
            if (_serverDir is null || !IsServerDir(_serverDir))
                _serverDir = Path.GetDirectoryName(pick.FileName);
        }

        var exe = Path.Combine(_serverDir!, "Duluka.Server.exe");
        var psi = File.Exists(exe)
            ? new ProcessStartInfo { FileName = exe, UseShellExecute = false }
            : new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{Path.Combine(_serverDir!, "Duluka.Server.dll")}\"",
                UseShellExecute = false,
            };
        psi.WorkingDirectory = _serverDir!;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        try
        {
            _server = Process.Start(psi);
        }
        catch (Exception ex)
        {
            AppendLog($"[manager] Failed to start: {ex.Message}\r\n");
            MessageBox.Show(this, "Could not start the server process.\r\n\r\n" + ex.Message,
                "Duluka Server Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var proc = _server!;
        proc.OutputDataReceived += (_, e) => { if (e.Data is not null) AppendLog(e.Data + "\r\n"); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data is not null) AppendLog(e.Data + "\r\n"); };
        proc.EnableRaisingEvents = true;
        proc.Exited += (_, _) => BeginInvoke(() =>
        {
            if (ReferenceEquals(_server, proc)) ServerStopped();
        });
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        _startedAt = DateTime.UtcNow;
        SetDot(true);
        _statusText.Text = "Running";
        _btStart.Enabled = false;
        _btStop.Enabled = true;
        AppendLog("[manager] Server starting on http://127.0.0.1:5115 — admin console at " + AdminUrl + "\r\n");

        // The DB may not exist until first boot — re-read shortly after.
        Task.Delay(1500).ContinueWith(_ => BeginInvoke(RefreshStats));
        RefreshStats();
    }

    private void StopServer()
    {
        if (_server is null || _server.HasExited) { ServerStopped(); return; }
        try
        {
            // Dev-tool semantics: kill the whole tree. SQLite is crash-safe
            // (journal/WAL), and the server keeps no in-flight state beyond
            // one request — an operator Stop is not a graceful-drain surface.
            _server.Kill(entireProcessTree: true);
            AppendLog("[manager] Stop signal sent.\r\n");
        }
        catch (Exception ex)
        {
            AppendLog($"[manager] Stop failed: {ex.Message}\r\n");
        }
    }

    private void ServerStopped()
    {
        _server?.Dispose();
        _server = null;
        _startedAt = null;
        SetDot(false);
        _statusText.Text = "Stopped";
        _btStart.Enabled = true;
        _btStop.Enabled = false;
        AppendLog("[manager] Server stopped.\r\n");
        RefreshStats();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_server is null || _server.HasExited) return;
        var answer = MessageBox.Show(this,
            "The server is still running. Stop it and exit?",
            "Duluka Server Manager", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) { e.Cancel = true; return; }
        try { _server.Kill(entireProcessTree: true); } catch { /* best effort */ }
    }

    // ── stats + helpers ─────────────────────────────────────────────────────

    private void UpdateUptime()
    {
        if (_startedAt is null || _server is null || _server.HasExited) return;
        var up = DateTime.UtcNow - _startedAt.Value;
        _statusText.Text = $"Running · {up:hh\\:mm\\:ss}";
    }

    private void RefreshStats()
    {
        var dbPath = ResolveServerDb();
        int accounts = -1, sessions = -1, devices = -1, links = -1;
        if (dbPath is not null)
        {
            try
            {
                var csb = new SqliteConnectionStringBuilder { DataSource = dbPath, Mode = SqliteOpenMode.ReadOnly };
                using var conn = new SqliteConnection(csb.ToString());
                conn.Open();
                var now = DateTimeOffset.UtcNow.ToString("o");
                accounts = Count(conn, "SELECT COUNT(*) FROM DulukaAccount");
                sessions = Count(conn, "SELECT COUNT(*) FROM AccountSession WHERE RevokedAt IS NULL AND ExpiresAt > $now", ("$now", now));
                devices = Count(conn, "SELECT COUNT(*) FROM AccountDevice WHERE RevokedAt IS NULL");
                links = Count(conn, "SELECT COUNT(*) FROM AccountProviderLink WHERE Status='Active'");
            }
            catch { /* DB absent or mid-migration — show dashes */ }
        }
        _lbAccounts.Text = accounts >= 0 ? accounts.ToString() : "—";
        _lbSessions.Text = sessions >= 0 ? sessions.ToString() : "—";
        _lbDevices.Text = devices >= 0 ? devices.ToString() : "—";
        _lbLinks.Text = links >= 0 ? links.ToString() : "—";
    }

    private static int Count(SqliteConnection conn, string sql, params (string Name, object Value)[] ps)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in ps) cmd.Parameters.AddWithValue(name, value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private void OpenDataFolder()
    {
        var dbPath = ResolveServerDb();
        var dir = dbPath is not null ? Path.GetDirectoryName(dbPath)
                : (_serverDir is not null ? Path.Combine(_serverDir, "AppData") : null);
        if (dir is null || !Directory.Exists(dir))
        {
            MessageBox.Show(this, "The data folder does not exist yet — start the server once to create it.",
                "Duluka Server Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = dbPath is not null ? $"/select,\"{dbPath}\"" : $"\"{dir}\"",
            UseShellExecute = true,
        });
    }

    private void OpenAdmin()
    {
        Process.Start(new ProcessStartInfo { FileName = AdminUrl, UseShellExecute = true });
    }

    private void SetDot(bool running) =>
        _statusDot.BackColor = running ? Mint : Danger;

    private void AppendLog(string line)
    {
        if (IsDisposed || Disposing) return;
        if (_log.InvokeRequired)
        {
            try { BeginInvoke(() => AppendLog(line)); } catch (ObjectDisposedException) { }
            return;
        }
        _log.AppendText(line);
        if (_log.Lines.Length > MaxLogLines)
        {
            // AppendText keeps growing the buffer — trim from the top.
            var lines = _log.Lines;
            _log.Text = string.Join("\r\n", lines[^MaxLogLines..]);
        }
        if (_cbAutoScroll.Checked)
            _log.SelectionStart = _log.TextLength;
        _log.ScrollToCaret();
    }
}
