// CheckPlugin.cs — plugin แรกของระบบ: ตรวจทุกหมวด + fix actions
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NvPlugins
{
    public class CheckPlugin : INvPlugin
    {
        public INvHost Host;
        public List<CheckItem> Items = new();
        public Dictionary<string, List<CheckItem>> ByCat = new();
        public static string Stage = "";   // GUI อ่านสด — บอกว่าค้างขั้นไหน

        public string GetInfo() => "CheckPlugin v1 — File/Registry/Server/Port/DLL/Connections";

        public void Init(INvHost host) { Host = host; }
        public void Start() { }
        public void Stop() { }

        private CheckItem Add(string cat, string name, St st, string exp, string act, Action fix = null, string fixLabel = "", string note = "")
        {
            var it = new CheckItem { Category = cat, Name = name, Status = st, Expected = exp, Actual = act, Fix = fix, FixLabel = fixLabel, Note = note };
            Items.Add(it);
            if (!ByCat.TryGetValue(cat, out var l)) ByCat[cat] = l = new List<CheckItem>();
            l.Add(it);
            return it;
        }

        // ================= RUN: ตรวจทุกหมวด =================
        public void RunAll()
        {
            Items.Clear(); ByCat.Clear();
            Stage = "files"; CheckFiles();
            Stage = "registry"; CheckRegistry();
            Stage = "server"; CheckServer();
            Stage = "ports"; CheckPorts();
            Stage = "dll"; CheckDlls();
            Stage = "connections"; CheckConnections();
            Stage = "done";
        }

        // ---------------- 1. FILE ----------------
        void CheckFiles()
        {
            const string C = "File";

            // shims ใน System32
            foreach (var (src, dst) in new[] {
                (P.PAYLOAD + @"\System32Shims\nvspcap64.dll", @"C:\Windows\System32\nvspcap64.dll"),
                (P.PAYLOAD + @"\System32Shims\SysWOW64\nvspcap.dll", @"C:\Windows\SysWOW64\nvspcap.dll"),
            })
            {
                bool ok = U.FileOk(dst, 1000000);
                Add(C, "shim: " + Path.GetFileName(dst), ok ? St.OK : St.FAIL,
                    "มีไฟล์ (≥1MB)", ok ? File.Exists(dst) ? new FileInfo(dst).Length + " bytes" : "ไม่มี" : "ไม่มี",
                    () => { Directory.CreateDirectory(Path.GetDirectoryName(dst)); File.Copy(src, dst, true); },
                    "copy จาก payload");
            }
            Add(C, "nvaudcap64v.dll (System32)", U.FileOk(@"C:\Windows\System32\nvaudcap64v.dll") ? St.OK : St.WARN,
                "มีไฟล์", File.Exists(@"C:\Windows\System32\nvaudcap64v.dll") ? "มี" : "ไม่มี");

            // hardlinks / byte-identical ใน GFE dir
            foreach (var n in new[] { "NVIDIA Share.exe", "MessageBus.dll", "libprotobuf.dll", "Poco.dll", "PocoInitializer.dll" })
            {
                string b = P.BUILD + (n == "NVIDIA Share.exe" ? @"\Share\" + n : @"\NvContainer\" + n);
                string g = P.GFE + @"\" + n;
                bool same = U.BytesEqual(b, g);
                Add(C, "hardlink: GFE\\" + n, same ? St.OK : St.FAIL,
                    "byte = build", U.BytesEqual(g, g) && File.Exists(g) ? (File.Exists(b) ? (same ? "เหมือน build" : "ต่างจาก build!") : "build ไม่มี") : "ไม่มีไฟล์",
                    () => { File.Delete(g); HardLink.Create(g, b); },
                    "สร้าง hardlink จาก build");
            }

            // PF anchors
            Add(C, "anchor: PF\\ShadowPlay\\nvsphelper64.exe",
                U.FileOk(P.PF + @"\ShadowPlay\nvsphelper64.exe") ? St.OK : St.WARN,
                "มีไฟล์ (container spawn ใช้)", File.Exists(P.PF + @"\ShadowPlay\nvsphelper64.exe") ? "มี" : "ไม่มี");
            Add(C, "PF\\NvContainer\\nvcontainer.exe (genuine)",
                File.Exists(P.PF + @"\NvContainer\nvcontainer.exe") ? St.OK : St.FAIL,
                "มีไฟล์", File.Exists(P.PF + @"\NvContainer\nvcontainer.exe") ? "มี" : "ไม่มี");

            // build + payload
            Add(C, "build\\Share\\NVIDIA Share.exe", File.Exists(P.BUILD + @"\Share\NVIDIA Share.exe") ? St.OK : St.FAIL, "มีไฟล์", File.Exists(P.BUILD + @"\Share\NVIDIA Share.exe") ? "มี" : "ไม่มี");
            Add(C, "build\\NvNode\\NVIDIA Web Helper.exe", File.Exists(P.BUILD + @"\NvNode\NVIDIA Web Helper.exe") ? St.OK : St.FAIL, "มีไฟล์", File.Exists(P.BUILD + @"\NvNode\NVIDIA Web Helper.exe") ? "มี" : "ไม่มี");
            Add(C, "build\\ShadowPlay\\nvsphelper64.exe", File.Exists(P.BUILD + @"\ShadowPlay\nvsphelper64.exe") ? St.OK : St.FAIL, "มีไฟล์", File.Exists(P.BUILD + @"\ShadowPlay\nvsphelper64.exe") ? "มี" : "ไม่มี");
            Add(C, "payload (ไว้ใช้ fix)", Directory.Exists(P.PAYLOAD + @"\System32Shims") ? St.OK : St.FAIL, "มี System32Shims", Directory.Exists(P.PAYLOAD + @"\System32Shims") ? "มี" : "ไม่มี");
        }

        // ---------------- 2. REGISTRY ----------------
        void CheckRegistry()
        {
            const string C = "Registry";
            string buildShare = P.BUILD + @"\Share\NVIDIA Share.exe";
            string pfNvc = P.PF + @"\NvContainer";

            // service ImagePath = PF
            string ip = U.RegGet(P.SVC, "ImagePath") ?? "";
            Add(C, "service ImagePath = PF", ip.StartsWith("\"" + P.PF, StringComparison.OrdinalIgnoreCase) ? St.OK : St.FAIL,
                P.PF + "\\nvcontainer.exe...", ip.Length > 90 ? ip.Substring(0, 90) + "..." : ip,
                () =>
                {
                    U.RegSetExpand(P.SVC, "ImagePath", "\"" + pfNvc + "\\nvcontainer.exe\" -s NvContainerLocalSystem -a -f \"C:\\ProgramData\\NVIDIA\\NvContainerLocalSystem.log\" -l 3 -d \"" + pfNvc + "\\plugins\\LocalSystem\" -r -p 30000 -st \"" + pfNvc + "\\NvContainerTelemetryApi.dll\" -ert");
                    RestartService();
                }, "set PF + restart service");

            // watchdog SPUser = PF
            string wdC = U.RegGet(P.WD + @"\SPUserX64", "Container") ?? "";
            Add(C, "Watchdog SPUserX64 Container = PF", wdC.Equals(P.PF + @"\NvContainer\nvcontainer.exe", StringComparison.OrdinalIgnoreCase) ? St.OK : St.FAIL,
                pfNvc + "\\nvcontainer.exe", wdC,
                () => { U.RegSet(P.WD + @"\SPUserX64", "Container", pfNvc + "\\nvcontainer.exe"); U.RegSet(P.WD + "\\SPUserX64", "Folder", pfNvc + "\\plugins\\SPUser"); U.RegSet(P.WD + "\\SPUserX64", "Parameters", "-f \"C:\\ProgramData\\NVIDIA\\NvContainerUser%dSPUser.log\" -d \"" + pfNvc + "\\plugins\\SPUser\" -r -l 3 -p 30000"); }, "set PF values");

            // FullPath 2 views
            foreach (var (view, key) in new[] { ("64-bit", @"HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience"), ("WOW64", @"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience") })
            {
                string fp = U.RegGet(key, "FullPath") ?? "";
                Add(C, "FullPath (" + view + ") = build Share", fp.Equals(buildShare, StringComparison.OrdinalIgnoreCase) ? St.OK : St.FAIL,
                    buildShare, fp.Length == 0 ? "(ว่าง)" : fp,
                    () => { U.RegSet(key, "FullPath", buildShare); U.RegSetDword(key, "Installed", 1); }, "set build Share path");
            }

            // Global\NvNode
            int port = U.RegGetDword(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", "port");
            int sec = U.RegGetDword(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", "disableSecurity");
            Add(C, "NvNode port = 59001", port == 59001 ? St.OK : St.FAIL, "59001", port.ToString(),
                () => U.RegSetDword(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", "port", 59001), "set 59001");
            Add(C, "NvNode disableSecurity = 1", sec == 1 ? St.OK : St.FAIL, "1", sec.ToString(),
                () => U.RegSetDword(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", "disableSecurity", 1), "set 1");

            // NVSPCAPS
            int sp = U.RegGetDword(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS", "IsShadowPlayEnabled");
            Add(C, "NVSPCAPS IsShadowPlayEnabled = 1", sp == 1 ? St.OK : St.WARN, "1", sp < 0 ? "(ไม่มี)" : sp.ToString(),
                () => U.RegSetDword(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS", "IsShadowPlayEnabled", 1), "set 1");

            // FailureActions ล้างแล้ว (ไม่มีค่า = ปลอดภัยเหมือนกัน)
            string fa = U.RegGet(P.SVC, "FailureActions") ?? "";
            bool faZero = fa.Length == 0 || fa.Trim('0').Length == 0;
            Add(C, "FailureActions = ล้างแล้ว", faZero ? St.OK : St.WARN, "all zeros หรือไม่มี", fa.Length > 40 ? fa.Substring(0, 40) + "..." : (fa.Length == 0 ? "(ไม่มี)" : fa),
                () => U.RegSetBinaryZeros(P.SVC, "FailureActions"), "ล้าง zeros");
        }

        // ---------------- 3. SERVER ----------------
        public int SpUserPid = 0; public string SpUserPath = "";
        void CheckServer()
        {
            const string C = "Server";
            var map = U.ProcMap();

            var svc = System.ServiceProcess.ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "NvContainerLocalSystem");
            Add(C, "service NvContainerLocalSystem", svc != null && svc.Status == System.ServiceProcess.ServiceControllerStatus.Running ? St.OK : St.FAIL,
                "Running", svc?.Status.ToString() ?? "ไม่พบ service",
                () => RestartService(), "restart service");

            // containers — SPUser ต้องจาก PF (WMI cmdline อ่านได้ทุก process — WMI ค้าง = ยอม WARN ไม่ค้าง GUI)
            var nvcontainers = Process.GetProcessesByName("nvcontainer").Concat(Process.GetProcessesByName("NvContainer")).ToList();
            var spUsers = map.Where(kv => kv.Value.cmd.IndexOf("SPUser", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            var fromPf = spUsers.Where(kv => (kv.Value.exe + " " + kv.Value.cmd).IndexOf(P.PF, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            SpUserPid = fromPf.Count > 0 ? (int)fromPf[0].Key : (int)(spUsers.Count > 0 ? spUsers[0].Key : 0);
            SpUserPath = SpUserPid != 0 ? (map[SpUserPid].exe.Length > 0 ? map[SpUserPid].exe : "cmdline ชี้ PF") : "";
            if (nvcontainers.Count == 0 && map.Count == 0)
                Add(C, "SPUser container จาก PF", St.WARN, P.PF + @"\NvContainer\nvcontainer.exe", "WMI ไม่ตอบ (watchdog 5 วิ) — กด CHECK ใหม่ / restart app");
            else
                Add(C, "SPUser container จาก PF", fromPf.Count > 0 ? St.OK : (nvcontainers.Count > 0 ? St.WARN : St.FAIL),
                    P.PF + @"\NvContainer\nvcontainer.exe",
                    spUsers.Count == 0 ? "ไม่มี nvcontainer รันเลย" : ("PID " + SpUserPid + " → " + SpUserPath),
                    () => { foreach (var kv in spUsers) KillPid(kv.Key); }, "kill build ตัว (watchdog เกิดใหม่จาก PF)");

            // node / helper / Share — ตรวจด้วย "ชื่อ process" (path โดนซ่อนถ้า elevated)
            var nodeP = Process.GetProcessesByName("NVIDIA Web Helper");
            var nodePath = nodeP.Length > 0 && map.TryGetValue(nodeP[0].Id, out var nm) && nm.exe.Length > 0 ? nm.exe : "";
            Add(C, "node (NVIDIA Web Helper)", nodeP.Length > 0 ? St.OK : St.FAIL,
                "รัน (build)", nodeP.Length > 0 ? "PID " + string.Join(",", nodeP.Select(p => p.Id)) + (nodePath.Length > 0 ? " → " + nodePath : " (path ซ่อน)·build ตามสูตร") : "ไม่รัน",
                () => StartBuildExe(@"NvNode\NVIDIA Web Helper.exe", @"NvNode"), "สตาร์ตจาก build");

            int nCode = U.NodeHttp();
            Add(C, "node REST :59001 openshare", nCode == 200 ? St.OK : St.FAIL, "200", nCode == 0 ? "no response" : nCode.ToString(),
                () => StartBuildExe(@"NvNode\NVIDIA Web Helper.exe", @"NvNode"), "สตาร์ต node");

            var shares = Process.GetProcessesByName("NVIDIA Share").ToList();
            Add(C, "NVIDIA Share.exe", shares.Count > 0 ? St.OK : St.FAIL, "≥1 (×2 = attach mode)", shares.Count + " ตัว (PID " + string.Join(",", shares.Select(p => p.Id)) + ")",
                () => StartBuildExe(@"Share\NVIDIA Share.exe", @"Share"), "สตาร์ตจาก build");

            var helper = Process.GetProcessesByName("nvsphelper64");
            Add(C, "nvsphelper64", helper.Length > 0 ? St.OK : St.FAIL, "รัน (หลัง node พร้อม)", helper.Length > 0 ? "PID " + helper[0].Id : "ไม่รัน",
                () => StartBuildExe(@"ShadowPlay\nvsphelper64.exe", @"ShadowPlay"), "สตาร์ตจาก build");
        }

        // ---------------- 4. PORT ----------------
        void CheckPorts()
        {
            const string C = "Port";
            Add(C, ":59001 (node)", U.PortListening(59001) ? St.OK : St.FAIL, "LISTENING", U.PortListening(59001) ? "LISTENING" : "ปิด",
                () => StartBuildExe(@"NvNode\NVIDIA Web Helper.exe", @"NvNode"), "สตาร์ต node");
            bool l2 = U.PortListening(59002);
            Add(C, ":59002 (fire — optional)", l2 ? St.OK : St.WARN, "LISTENING (โหมด hook/DulukaPort)", l2 ? "LISTENING" : "ปิด");
        }

        // ---------------- 5. DLL ----------------
        void CheckDlls()
        {
            const string C = "DLL";
            Add(C, "nvspcap64.dll (System32) PE", NvPlugins.DllProbe.HasPeHeader(@"C:\Windows\System32\nvspcap64.dll") ? St.OK : St.FAIL,
                "PE header ปกติ", NvPlugins.DllProbe.Describe(@"C:\Windows\System32\nvspcap64.dll"),
                () => File.Copy(P.PAYLOAD + @"\System32Shims\nvspcap64.dll", @"C:\Windows\System32\nvspcap64.dll", true), "copy จาก payload");
            Add(C, "MessageBus.dll โหลดได้ (GFE dir)", NvPlugins.DllProbe.CanLoad(P.GFE + @"\MessageBus.dll") ? St.OK : St.FAIL,
                "LoadLibrary OK", NvPlugins.DllProbe.Describe(P.GFE + @"\MessageBus.dll"),
                () => File.Copy(P.BUILD + @"\NvContainer\MessageBus.dll", P.GFE + @"\MessageBus.dll", true), "copy จาก build\\NvContainer");
            Add(C, "NvShadowPlayAPINode.node (PF x86)", File.Exists(@"C:\Program Files (x86)\NVIDIA Corporation\NvNode\NvShadowPlayAPINode.node") ? St.OK : St.WARN,
                "มีไฟล์", File.Exists(@"C:\Program Files (x86)\NVIDIA Corporation\NvNode\NvShadowPlayAPINode.node") ? "มี" : "ไม่มี");
        }

        // ---------------- 6. CONNECTIONS (tree) ----------------
        public List<(string from, string to, string via)> Edges = new();
        public List<(string name, St st, string detail)> Nodes = new();

        void CheckConnections()
        {
            const string C = "Connection";
            Edges.Clear(); Nodes.Clear();
            string log = "";
            try { if (File.Exists(P.CCLOG)) { using var fs = new FileStream(P.CCLOG, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); fs.Seek(-Math.Min(fs.Length, 2000000), SeekOrigin.End); using var r = new StreamReader(fs); log = r.ReadToEnd(); } } catch { }

            var re = new Regex(@"\[R\]\[(?<tag>[A-Za-z0-9]+):(?<pid>\d+):[^\]]+\] IpcCommonInterface::HandleJoinMessage: MessageBus (?<what>joined|left) - System: (?<sys>[^,]+), Module: (?<mod>[^\r\n]+)");
            var latest = new Dictionary<string, (string what, string line)>();
            foreach (Match m in re.Matches(log))
                latest[m.Groups["tag"].Value + ":" + m.Groups["pid"].Value + "|" + m.Groups["sys"].Value + ":" + m.Groups["mod"].Value] = (m.Groups["what"].Value, m.Value);

            var joined = new List<(string tag, int pid, string sys, string mod)>();
            foreach (var kv in latest)
            {
                if (kv.Value.what != "joined") continue;
                var mm = Regex.Match(kv.Key, @"(?<tag>[A-Za-z0-9]+):(?<pid>\d+)\|(?<sys>[^:]+):(?<mod>.+)");
                if (!mm.Success) continue;
                joined.Add((mm.Groups["tag"].Value, int.Parse(mm.Groups["pid"].Value), mm.Groups["sys"].Value, mm.Groups["mod"].Value));
            }

            bool HasJoin(string tag, int pid, string sys, string modPrefix) =>
                joined.Any(j => j.tag == tag && (pid == 0 || j.pid == pid) && j.sys == sys && j.mod.StartsWith(modPrefix, StringComparison.OrdinalIgnoreCase));

            // --- ตรวจด้วยชื่อ process (แม่นกว่า log) + ใช้ log ยืนยัน "การเชื่อม" ---
            var helperP = Process.GetProcessesByName("nvsphelper64").FirstOrDefault();
            bool helperJoin = helperP != null && HasJoin("HELPR", helperP.Id, "Hotkey", "HotkeyPlugin");
            Nodes.Add(("nvsphelper64 " + (helperP != null ? "(PID " + helperP.Id + ")" : ""), helperP != null ? St.OK : St.FAIL,
                helperP == null ? "ไม่รัน" : helperJoin ? "join Hotkey:HotkeyPlugin ✓ (จับ Alt+Z พร้อม)" : "รันอยู่ — แต่ยังไม่เห็น join ใน log ล่าสุด"));
            if (helperP != null) Edges.Add(("nvsphelper64", "MessageBus System:Hotkey", helperJoin ? "RegisterHotKey Alt+Z ✓" : "ยังไม่ join"));

            var nodeP = Process.GetProcessesByName("NVIDIA Web Helper").FirstOrDefault();
            bool nodeHotkey = nodeP != null && HasJoin("NODJS", nodeP.Id, "Hotkey", "Node");
            bool nodeSp = nodeP != null && HasJoin("NODJS", nodeP.Id, "Shadowplay", "ShadowplayApi");
            Nodes.Add(("NVIDIA Web Helper (node)", nodeP != null ? St.OK : St.FAIL,
                nodeP == null ? "ไม่รัน" : (nodeHotkey ? "join Hotkey:Node ✓ (receiver armed)" : "รันอยู่ — receiver ยังไม่ armed (กด /Launch)")));
            if (nodeP != null)
            {
                Edges.Add(("node receiver", "MessageBus System:Hotkey", nodeHotkey ? "รับ Alt+Z broadcast ✓" : "ยังไม่ armed"));
                Edges.Add(("node SP API", "MessageBus System:Shadowplay", nodeSp ? "join ✓" : "ยังไม่ join"));
            }

            var shares = Process.GetProcessesByName("NVIDIA Share");
            var shareJoin = shares.Any(sh => HasJoin("SHARE", sh.Id, "Shadowplay", "ShadowplayApi"));
            Nodes.Add(("NVIDIA Share.exe ×" + shares.Length, shares.Length > 0 ? St.OK : St.FAIL,
                shares.Length == 0 ? "ไม่รัน" : shareJoin ? "join Shadowplay:ShadowplayApi ✓ (page host)" : "รันอยู่ — ยังไม่เห็น join"));
            if (shares.Length > 0) Edges.Add(("Share (OSC host)", "MessageBus System:Shadowplay", shareJoin ? "เชื่อมแล้ว ✓" : "ยังไม่ join"));

            var spUserPids = U.ProcMap().Where(kv => kv.Value.cmd.IndexOf("SPUser", StringComparison.OrdinalIgnoreCase) >= 0).Select(kv => (int)kv.Key).ToList();
            var svj = spUserPids.Any(p => HasJoin("CNTNR", p, "Shadowplay", "ShadowplayServer"));
            Nodes.Add(("SPUser container (ShadowplayServer) ×" + spUserPids.Count, spUserPids.Count > 0 && svj ? St.OK : spUserPids.Count > 0 ? St.WARN : St.FAIL,
                spUserPids.Count == 0 ? "ไม่รัน" : svj ? "join Shadowplay:ShadowplayServer ✓" : "รันอยู่ — ยังไม่เห็น join"));
            if (spUserPids.Count > 0) Edges.Add(("SPUser container", "MessageBus System:Shadowplay", svj ? "ShadowplayServer ✓" : "ยังไม่ join"));

            bool l90 = U.PortListening(59001);
            Edges.Add(("node HTTP", ":59001", l90 ? "LISTENING (page เชื่อมผ่านนี้)" : "ปิด!"));
            Nodes.Add(("HTTP :59001 (node ↔ page)", l90 ? St.OK : St.FAIL, l90 ? "LISTENING" : "ปิด"));

            foreach (var n in Nodes) Add(C, n.name, n.st, "", n.detail);
        }

        // ================= FIX ALL =================
        public int FixAll(Action<string> log)
        {
            int done = 0;
            foreach (var it in Items.Where(i => i.Status != St.OK && i.Fix != null))
            {
                try { log("FIX: " + it.Name + " → " + it.FixLabel); it.Fix(); done++; }
                catch (Exception ex) { log("  FIX FAILED: " + ex.Message); }
            }
            return done;
        }

        // ================= helpers =================
        public static void RestartService()
        {
            U.RegSetBinaryZeros(P.SVC, "FailureActions");
            Cmd("sc.exe stop NvContainerLocalSystem", 4000);
            System.Threading.Thread.Sleep(5000);
            Cmd("sc.exe start NvContainerLocalSystem", 3000);
            System.Threading.Thread.Sleep(12000);
            var sc = System.ServiceProcess.ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "NvContainerLocalSystem");
            if (sc == null || sc.Status != System.ServiceProcess.ServiceControllerStatus.Running)
            {
                Cmd("sc.exe start NvContainerLocalSystem", 3000);   // รอบสอง (pattern ที่รู้)
                System.Threading.Thread.Sleep(12000);
            }
        }

        public static void StartBuildExe(string rel, string workRel)
        {
            var p = new ProcessStartInfo(P.BUILD + "\\" + rel)
            {
                WorkingDirectory = P.BUILD + "\\" + workRel,
                UseShellExecute = true
            };
            Process.Start(p);
        }

        static void KillPid(long pid) { try { Process.GetProcessById((int)pid).Kill(); } catch { } }

        public static void Cmd(string exe, int waitMs)
        {
            var psi = new ProcessStartInfo("cmd.exe", "/c " + exe) { CreateNoWindow = true, UseShellExecute = false };
            using var p = Process.Start(psi);
            p?.WaitForExit(waitMs);
        }
    }

    // Hardlink P/Invoke
    public static class HardLink
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        public static extern bool CreateHardLinkW(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
        public static void Create(string link, string target)
        {
            if (!CreateHardLinkW(link, target, IntPtr.Zero))
                throw new Exception("CreateHardLink failed: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
        }
    }

    // DLL probe (ปลอดภัย — ไม่รัน code ของ dll)
    public static class DllProbe
    {
        public static bool HasPeHeader(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                using var fs = File.OpenRead(path);
                Span<byte> b = stackalloc byte[2];
                if (fs.Read(b) != 2) return false;
                return b[0] == 'M' && b[1] == 'Z';
            }
            catch { return false; }
        }
        public static string Describe(string path)
        {
            try
            {
                if (!File.Exists(path)) return "ไม่มีไฟล์";
                var fi = new FileInfo(path);
                return fi.Length + " bytes";
            }
            catch { return "?"; }
        }
        public static bool CanLoad(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                var h = LoadLibraryExW(path, IntPtr.Zero, 0x1 /* DONT_RESOLVE_DLL_REFERENCES */);
                if (h == IntPtr.Zero) return false;
                FreeLibrary(h);
                return true;
            }
            catch { return false; }
        }
        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        static extern IntPtr LoadLibraryExW(string name, IntPtr handle, uint flags);
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr h);
    }
}
