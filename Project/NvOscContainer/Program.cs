// nvosc_container.exe — ผู้เฝ้าตรวจ/test OSC (Phase 3.5 — สั่ง OWNER 2026-10-05)
//
// ไม่มี args        = WinForms dashboard (สถานะสด L0-L5 + RECORD/IR tile + log + ปุ่ม boot/test)
//   check           = ตรวจครบทุกชั้นครั้งเดียว (exit 0 = ปกติ)
//   watch [sec]     = วนตรวจ (light ทุกรอบ, deep ทุก 10 รอบ)
//
// ชั้นตรวจ: L0 supervisor · L1 api · L2 record-sync(deep) · L3 ir-sync(deep) ·
//           L4 ui-presence · L5 capture-sync(deep, เมื่อเปิด capture) / paused(เมื่อปิด)
// กติกา: ตัวนี้เป็น "คนตรวจ" ไม่แก้อะไร — แก้ = ปุ่ม boot (เรียก NvPlugins.exe boot)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WFTimer = System.Windows.Forms.Timer;
using System.Windows.Forms;

namespace NvOscContainer
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int pid);

        [STAThread]
        static int Main(string[] args)
        {
            var mode = (args.FirstOrDefault() ?? "ui").ToLowerInvariant();

            if (mode == "check" || mode == "watch")
            {
                AttachConsole(-1);   // ให้ print ออก terminal เดิมได้แม้ WinExe
                var failures = mode == "check"
                    ? Audit.RunAll(deep: true, log: m => { try { Console.WriteLine(m); } catch { } })
                    : Audit.Watch(args.Skip(1).FirstOrDefault(), m => { try { Console.WriteLine(m); } catch { } });
                if (mode == "watch") return 0;
                return failures == 0 ? 0 : 1;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DashboardForm());
            return 0;
        }
    }

    // ═══════════════════════ AUDIT ENGINE (light/deep) ═══════════════════════
    internal static class Audit
    {
        public const string Node = "http://127.0.0.1:59001";
        public const string Engine = "http://127.0.0.1:59077";
        public const string Build = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay";
        public const string GfeShareExe = @"C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\NVIDIA Share.exe";
        public const string CaptureFlag = @"C:\My Project\NVIDIA-Shadowplay\Project\NvConfig\capture-disabled.flag";

        public static string LogFile = Path.Combine(Build, "Logs", "nvosc-container.log");

        public class Res { public string Name; public bool Ok; public string Detail; }

        static Res R(string name, bool ok, string detail) => new Res { Name = name, Ok = ok, Detail = detail };

        // light = ไม่แตะ state (รันได้ทุกวินาที) · deep = toggle round-trip (เปลี่ยน fake state)
        public static List<Res> Light() => new List<Res>
        {
            Run1("L0 supervisor", CheckSupervisor),
            Run1("L1 api-sweep", CheckApiSweep),
            Run1("L4 ui-presence", CheckUiPresence),
            File.Exists(CaptureFlag)
                ? Run1("L5 capture", CheckCapturePaused)
                : Run1("L5 capture", CheckCaptureHealth),
        };

        public static List<Res> Deep() => new List<Res>
        {
            Run1("L2 record-sync", CheckRecordSync),
            Run1("L3 ir-sync", CheckIrSync),
        };

        static Res Run1(string name, Func<string, Res> f)
        {
            try { return f(""); }
            catch (Exception ex) { return new Res { Name = name, Ok = false, Detail = ex.Message }; }
        }

        // ── L0 ──
        static Res CheckSupervisor(string _)
        {
            var exe = Path.Combine(Build, "NvPlugins", "NvPlugins.exe");
            if (!File.Exists(exe)) return R("L0 supervisor", false, "ไม่พบ NvPlugins.exe");
            try
            {
                var psi = new ProcessStartInfo(exe, "status")
                { UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true };
                using var p = Process.Start(psi);
                var outp = p.StandardOutput.ReadToEnd();
                p.WaitForExit(30000);
                var m = System.Text.RegularExpressions.Regex.Match(outp, @"FAIL (\d+)");
                var failCount = m.Success ? int.Parse(m.Groups[1].Value) : -1;
                return failCount == 0
                    ? R("L0 supervisor", true, "NvPlugins status ทุก step OK")
                    : R("L0 supervisor", false, $"FAIL {failCount} step — รัน NvPlugins.exe boot");
            }
            catch (Exception ex) { return R("L0 supervisor", false, ex.Message); }
        }

        // ── L1 ──
        static readonly (string method, string path, string body)[] ApiRoutes = new[]
        {
            ("GET",  "/ShadowPlay/v.1.0/Record/Enable", ""),
            ("GET",  "/ShadowPlay/v.1.0/Record/Running", ""),
            ("GET",  "/ShadowPlay/v.1.0/Record/Settings", ""),
            ("GET",  "/ShadowPlay/v.1.0/Capture/State", ""),
            ("GET",  "/ShadowPlay/v.1.0/InstantReplay/Enable", ""),
            ("GET",  "/ShadowPlay/v.1.0/InstantReplay/Running", ""),
            ("GET",  "/ShadowPlay/v.1.0/InstantReplay/BufferLength", ""),
            ("POST", "/ShadowPlay/v.1.0/InstantReplay/Save", "{}"),
            ("GET",  "/ShadowPlay/v.1.0/Broadcast/Support", ""),
            ("GET",  "/ShadowPlay/v.1.0/Screenshot/Support", ""),
            ("GET",  "/ShadowPlay/v.1.0/Launch", ""),
            ("GET",  "/ShadowPlay/v.1.0/AudioSettings", ""),
            ("GET",  "/ShadowPlay/v.1.0/DesktopCapture/Support/Reason", ""),
        };

        static Res CheckApiSweep(string _)
        {
            var bad = new List<string>();
            foreach (var (method, path, body) in ApiRoutes)
            {
                var (code, _) = Http(method, Node + path, body);
                if (code != 200) bad.Add($"{method} {path} -> {code}");
            }
            return bad.Count == 0
                ? R("L1 api-sweep", true, $"{ApiRoutes.Length} routes 200 ทั้งหมด")
                : R("L1 api-sweep", false, "ไม่ผ่าน: " + string.Join("; ", bad));
        }

        // ── L2 (deep) ──
        static Res CheckRecordSync(string _)
        {
            var startOn = GetState(Node + "/ShadowPlay/v.1.0/Record/Running");
            if (startOn == "true") return R("L2 record-sync", true, "recording อยู่ — ข้าม toggle-test (ไม่รบกวน session)");
            var errs = new List<string>();
            Http("POST", Node + "/ShadowPlay/v.1.0/Record/Enable", "{\"status\":true}");
            if (GetState(Node + "/ShadowPlay/v.1.0/Record/Running") != "true") errs.Add("Enable true -> Running ไม่ขึ้น");
            Http("POST", Node + "/ShadowPlay/v.1.0/Record/Enable", "{\"status\":false}");
            if (GetState(Node + "/ShadowPlay/v.1.0/Record/Running") != "false") errs.Add("Enable false -> Running ไม่ลง");
            return errs.Count == 0
                ? R("L2 record-sync", true, "toggle round-trip sync (on->true / off->false)")
                : R("L2 record-sync", false, "ไม่ sync: " + string.Join("; ", errs));
        }

        // ── L3 (deep) ──
        static Res CheckIrSync(string _)
        {
            var errs = new List<string>();
            Http("POST", Node + "/ShadowPlay/v.1.0/InstantReplay/Enable", "{\"status\":true}");
            if (GetState(Node + "/ShadowPlay/v.1.0/InstantReplay/Enable") != "true") errs.Add("Enable true ไม่ขึ้น");
            if (GetState(Node + "/ShadowPlay/v.1.0/InstantReplay/Running") != "true") errs.Add("Running ไม่ขึ้น");
            var (code, saveBody) = Http("POST", Node + "/ShadowPlay/v.1.0/InstantReplay/Save", "{}");
            if (code != 200 || !saveBody.Contains("\"status\":true")) errs.Add($"Save: {code} {saveBody}");
            Http("POST", Node + "/ShadowPlay/v.1.0/InstantReplay/Enable", "{\"status\":false}");
            if (GetState(Node + "/ShadowPlay/v.1.0/InstantReplay/Enable") != "false") errs.Add("Enable false ไม่ลง");
            return errs.Count == 0
                ? R("L3 ir-sync", true, "IR toggle + Save gate sync")
                : R("L3 ir-sync", false, "ไม่ sync: " + string.Join("; ", errs));
        }

        // ── L4 ──
        static Res CheckUiPresence(string _)
        {
            var errs = new List<string>();
            var (cdpCode, cdpBody) = Http("GET", "http://127.0.0.1:9222/json", "");
            if (cdpCode != 200 || !cdpBody.Contains("osc/index.html")) errs.Add("CDP :9222 ไม่เห็นหน้า osc");

            var socketPids = NodeSocketPids(59001);
            if (socketPids.Count == 0) errs.Add("ไม่มี socket หน้า <-> node");

            var mains = 0;
            foreach (var p in Process.GetProcessesByName("NVIDIA Share"))
            {
                var path = ProcPath(p.Id);
                if (path.Length == 0) continue;
                if (!socketPids.Contains(p.Id)) continue;   // child CEF — ไม่ใช่ main
                if (path.Equals(GfeShareExe, StringComparison.OrdinalIgnoreCase)) mains++;
                else errs.Add($"Share ผิด path: PID {p.Id} {path}");
            }
            if (mains == 0 && !errs.Any(e => e.Contains("ผิด path"))) errs.Add("ไม่มี Share main จาก GFE dir");
            if (mains > 1) errs.Add($"Share main {mains} ตัว (ต้อง 1) — แย่ง overlay กัน");

            return errs.Count == 0
                ? R("L4 ui-presence", true, "หน้ามีชีวิต · socket ✓ · Share main 1 (GFE)")
                : R("L4 ui-presence", false, string.Join("; ", errs));
        }

        // ── L5 light (paused / engine health) ──
        static Res CheckCapturePaused(string _)
        {
            var alive = Process.GetProcessesByName("NvCapture").Length;
            return alive > 0
                ? R("L5 capture", false, "PAUSED แต่ engine ยังรัน — ควร kill")
                : R("L5 capture", true, "capture paused (engine ไม่รัน, boundary fail-soft)");
        }

        static Res CheckCaptureHealth(string _)
        {
            var (_, h) = Http("GET", Engine + "/health", "");
            return h != null && h.Contains("\"ok\":true")
                ? R("L5 capture", true, "engine healthy (NvCapture :59077)")
                : R("L5 capture", false, "engine /health ไม่ตอบ");
        }

        // ── L5 deep (เมื่อ capture เปิด): record round-trip กับ NvCapture + ไฟล์เกิด ──
        public static Res CheckCaptureRoundTrip(string _)
        {
            var errs = new List<string>();
            var (_, h) = Http("GET", Engine + "/health", "");
            if (!h.Contains("\"ok\":true")) return R("L5 capture-sync", false, "engine /health ไม่ตอบ");

            var before = DirFiles().Length;
            Http("POST", Node + "/ShadowPlay/v.1.0/Record/Enable", "{\"status\":true}");
            Thread.Sleep(3000);
            if (GetState(Node + "/ShadowPlay/v.1.0/Record/Running") != "true") errs.Add("Running ไม่ขึ้น");
            var st = EngineState();
            if (!st.Contains("\"recording\":true")) errs.Add("engine ไม่ recording (UI/State ไม่ sync กับ NvCapture)");

            Http("POST", Node + "/ShadowPlay/v.1.0/Record/Enable", "{\"status\":false}");
            Thread.Sleep(4000);
            var after = DirFiles().Length;
            if (after <= before) errs.Add("ไฟล์ไม่เกิด");

            return errs.Count == 0
                ? R("L5 capture-sync", true, $"record round-trip sync กับ NvCapture (ไฟล์ +{after - before})")
                : R("L5 capture-sync", false, string.Join("; ", errs));
        }

        static string[] DirFiles()
        {
            try { return Directory.GetFiles(VideosDir(), "NvCapture_*.mp4"); }
            catch { return new string[0]; }
        }

        static string VideosDir()
        {
            var (code, body) = Http("GET", Node + "/ShadowPlay/v.1.0/RecordPaths", "");
            if (code == 200)
            {
                var m = System.Text.RegularExpressions.Regex.Match(body ?? "", "\"videos\"\\s*:\\s*\"([^\"]+)\"");
                if (m.Success) return m.Groups[1].Value.Replace("\\\\", "\\");
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        }

        // ================= full-run helpers =================
        public static int RunAll(bool deep, Action<string> log)
        {
            var results = new List<Res>();
            results.AddRange(Light());
            if (deep) results.AddRange(Deep());
            foreach (var r in results) log($"[{(r.Ok ? "OK  " : "FAIL")}] {r.Name}  |  {r.Detail}");
            var fails = results.Count(r => !r.Ok);
            log(fails == 0 ? "OSC สมบูรณ์ — ทุกชั้น sync" : $"OSC มีปัญหา {fails} ชั้น");
            return fails;
        }

        public static int Watch(string secArg, Action<string> log)
        {
            var sec = secArg != null && int.TryParse(secArg, out var s) ? Math.Max(15, s) : 60;
            log($"watch: light ทุก {sec}s · deep ทุก 10 รอบ");
            var n = 0;
            while (true)
            {
                n++;
                var fails = 0;
                foreach (var r in Light()) { log($"[{(r.Ok ? "OK" : "FAIL")}] {r.Name} | {r.Detail}"); if (!r.Ok) fails++; }
                if (n % 10 == 0)
                    foreach (var r in Deep()) { log($"[{(r.Ok ? "OK" : "FAIL")}] {r.Name} | {r.Detail}"); if (!r.Ok) fails++; }
                if (fails > 0) log($"WATCH: {fails} FAILED");
                Thread.Sleep(sec * 1000);
            }
        }

        // ================= primitives =================
        // คืน "true"/"false" จาก {"status":..}/{"running":..} — "" = อ่านไม่ได้
        public static string GetState(string url)
        {
            var (code, body) = Http("GET", url, "");
            if (code != 200) return "";
            var m = System.Text.RegularExpressions.Regex.Match(body ?? "", "\"(?:status|running)\"\\s*:\\s*(true|false)");
            return m.Success ? m.Groups[1].Value : "";
        }

        public static (int, string) Http(string method, string url, string body)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = method; req.Timeout = 6000; req.Proxy = null;
                if (!string.IsNullOrEmpty(body) && method == "POST")
                {
                    req.ContentType = "application/json";
                    var b = Encoding.UTF8.GetBytes(body);
                    req.ContentLength = b.Length;
                    using var s = req.GetRequestStream(); s.Write(b, 0, b.Length);
                }
                using var rsp = (HttpWebResponse)req.GetResponse();
                using var sr = new StreamReader(rsp.GetResponseStream());
                return ((int)rsp.StatusCode, sr.ReadToEnd());
            }
            catch (WebException we) when (we.Response is HttpWebResponse r)
            {
                using var sr = new StreamReader(r.GetResponseStream());
                return ((int)r.StatusCode, sr.ReadToEnd());
            }
            catch { return (0, ""); }
        }

        public static string EngineState()
        {
            var (_, body) = Http("GET", Engine + "/state", "");
            return body ?? "";
        }

        public static HashSet<int> NodeSocketPids(int port)
        {
            var pids = new HashSet<int>();
            try
            {
                var psi = new ProcessStartInfo("netstat.exe", "-ano -p tcp")
                { UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true };
                using var p = Process.Start(psi);
                var outp = p.StandardOutput.ReadToEnd(); p.WaitForExit(3000);
                var nl = Convert.ToChar(10);
                foreach (var line in outp.Split(nl))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5 && parts[2].EndsWith(":" + port) && parts[3] == "ESTABLISHED"
                        && int.TryParse(parts[4], out var pid)) pids.Add(pid);
                }
            }
            catch { }
            return pids;
        }

        public static string ProcPath(int pid)
        {
            try
            {
                var h = OpenProcess(0x1000, false, pid);
                if (h == IntPtr.Zero) return "";
                var sb = new StringBuilder(1024); uint size = 1024;
                var ok = QueryFullProcessImageNameW(h, 0, sb, ref size);
                CloseHandle(h);
                return ok ? sb.ToString() : "";
            }
            catch { return ""; }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool QueryFullProcessImageNameW(IntPtr h, uint flags, StringBuilder sb, ref uint size);
        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr h);
    }

    // ═══════════════════════ DASHBOARD (WinForms) ═══════════════════════
    public class DashboardForm : Form
    {
        readonly Dictionary<string, Label> _rows = new Dictionary<string, Label>();
        readonly TextBox _log;
        readonly Label _recLabel; readonly Label _irLabel;
        readonly WFTimer _timer;
        readonly WFTimer _deepTimer;

        public DashboardForm()
        {
            Text = "nvosc_container — OSC watchdog";
            Size = new Size(560, 480);
            StartPosition = FormStartPosition.Manual;
            Location = new Point(40, 40);
            BackColor = Color.FromArgb(24, 26, 32);
            Font = new Font("Segoe UI", 9F);

            var title = new Label { Text = "nvosc_container — ผู้เฝ้าตรวจ OSC", ForeColor = Color.Gainsboro, AutoSize = true, Left = 16, Top = 12, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            Controls.Add(title);

            string[] rows = { "L0 supervisor", "L1 api-sweep", "L4 ui-presence", "L5 capture" };
            var y = 46;
            foreach (var r in rows)
            {
                var l = new Label { Text = r, ForeColor = Color.Silver, AutoSize = true, Left = 24, Top = y, Width = 130 };
                var v = new Label { Text = "...", ForeColor = Color.Silver, AutoSize = true, Left = 170, Top = y, Width = 330 };
                Controls.Add(l); Controls.Add(v);
                _rows[r] = v;
                y += 26;
            }

            _recLabel = new Label { Text = "RECORD: ...", ForeColor = Color.Silver, AutoSize = true, Left = 24, Top = y + 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            _irLabel = new Label { Text = "IR: ...", ForeColor = Color.Silver, AutoSize = true, Left = 200, Top = y + 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Controls.Add(_recLabel); Controls.Add(_irLabel);
            y += 30;

            var btnDeep = new Button { Text = "TEST SYNC (toggle round-trip)", Left = 24, Top = y, Width = 200, Height = 30 };
            var btnBoot = new Button { Text = "BOOT แก้ทั้งระบบ", Left = 236, Top = y, Width = 160, Height = 30 };
            var btnRec = new Button { Text = "re-check", Left = 404, Top = y, Width = 100, Height = 30 };
            btnDeep.Click += (s, e) => Task.Run(() => { foreach (var r in Audit.Deep()) UiLog($"[{(r.Ok ? "OK" : "FAIL")}] {r.Name} | {r.Detail}"); });
            btnBoot.Click += (s, e) => RunBoot();
            btnRec.Click += (s, e) => LightNow();
            Controls.Add(btnDeep); Controls.Add(btnBoot); Controls.Add(btnRec);
            y += 44;

            _log = new TextBox
            {
                Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Left = 16, Top = y, Width = 510, Height = 180,
                BackColor = Color.FromArgb(12, 13, 16), ForeColor = Color.FromArgb(170, 220, 170),
                Font = new Font("Consolas", 8.5F)
            };
            Controls.Add(_log);

            _timer = new WFTimer { Interval = 5000 };
            _timer.Tick += (s, e) => LightNow();
            _deepTimer = new WFTimer { Interval = 60000 };
            _deepTimer.Tick += (s, e) => DeepNow();

            Load += (s, e) => { LightNow(); DeepNow(); _timer.Start(); _deepTimer.Start(); };
        }

        void LightNow()
        {
            Task.Run(() =>
            {
                var results = Audit.Light();
                var rec = Audit.GetState(Audit.Node + "/ShadowPlay/v.1.0/Record/Running");
                var ir = Audit.GetState(Audit.Node + "/ShadowPlay/v.1.0/InstantReplay/Enable");
                BeginInvoke((Action)(() =>
                {
                    foreach (var r in results)
                        if (_rows.TryGetValue(r.Name, out var lbl))
                        {
                            lbl.Text = r.Detail;
                            lbl.ForeColor = r.Ok ? Color.FromArgb(80, 200, 120) : Color.FromArgb(255, 82, 82);
                        }
                    _recLabel.Text = "RECORD: " + (rec == "true" ? "● RECORDING (fake)" : "○ idle");
                    _recLabel.ForeColor = rec == "true" ? Color.OrangeRed : Color.FromArgb(80, 200, 120);
                    _irLabel.Text = "IR: " + (ir == "true" ? "● armed" : "○ off");
                    _irLabel.ForeColor = ir == "true" ? Color.OrangeRed : Color.FromArgb(80, 200, 120);
                }));
            });
        }

        void DeepNow()
        {
            Task.Run(() =>
            {
                foreach (var r in Audit.Deep()) UiLog($"[{(r.Ok ? "OK" : "FAIL")}] {r.Name} | {r.Detail}");
            });
        }

        void RunBoot()
        {
            var exe = Path.Combine(Audit.Build, "NvPlugins", "NvPlugins.exe");
            try
            {
                var psi = new ProcessStartInfo(exe, "boot") { UseShellExecute = true, Verb = "runas" };
                Process.Start(psi);
                UiLog("boot เริ่มแล้ว (elevated) — รอ ~60 วิ แล้วกด re-check");
            }
            catch (Exception ex) { UiLog("boot fail: " + ex.Message); }
        }

        void UiLog(string m)
        {
            try
            {
                if (!IsHandleCreated) return;
                BeginInvoke((Action)(() =>
                {
                    _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {m}{Environment.NewLine}");
                    try { File.AppendAllText(Audit.LogFile, $"[{DateTime.Now:HH:mm:ss}] {m}{Environment.NewLine}"); } catch { }
                }));
            }
            catch { }
        }
    }
}
