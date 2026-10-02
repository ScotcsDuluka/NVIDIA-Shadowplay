// NvContainer.cs — ตัวแม่ของ Duluka System (ของเราเอง 100%) — config-driven
//
// สถาปัตยกรรม (2026-09-29, หลัง approve แผน):
//   โหมด ours (default):
//     แม่ → NVIDIA Web Helper.exe (เรา) → node :59001 (shims เรา)
//         → NVIDIA OSC.exe      (เรา — CEF host เลียน Share.exe)
//         → nvsphelper.exe      (เรา — Alt+Z)
//   โหมด genuine (--genuine หรือ mode ใน config):
//     แม่ → NVIDIA Share.exe แท้ (มันเลี้ยง node เองผ่าน launcher)
//
// config ทั้งระบบรวมที่ Project\NvConfig\nvcontainer.json
// log: build\NVIDIA ShadowPlay\NvContainer\Logs\NvContainer.log
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;

static class NvContainer
{
    // ---- config hub ----
    const string ROOT     = @"C:\My Project\NVIDIA-Shadowplay";
    const string CONFIG   = ROOT + @"\Project\NvConfig\nvcontainer.json";

    // fallback ถ้า config หาย (ทำให้แม่ไม่ตายเฉย ๆ)
    const string DEF_BUILD = ROOT + @"\build\NVIDIA ShadowPlay";

    static JsonElement C;          // ทั้งไฟล์ config
    static string BuildRoot;
    static string LogDir;
    static bool ModeOurs = true;

    static void LoadConfig()
    {
        try
        {
            C = JsonDocument.Parse(File.ReadAllText(CONFIG)).RootElement;
            BuildRoot = Str("buildRoot", DEF_BUILD);
            ModeOurs = Str("mode", "ours") != "genuine";
            LogDir = Path.Combine(BuildRoot, Str("logsDir", @"NvContainer\Logs"));
        }
        catch (Exception ex)
        {
            // config พัง = ใช้ค่า default ทั้งหมด แล้วทำงานต่อ
            BuildRoot = DEF_BUILD;
            LogDir = Path.Combine(DEF_BUILD, @"NvContainer\Logs");
            ModeOurs = true;
            Log("[config] อ่าน " + CONFIG + " ไม่ได้ (" + ex.Message + ") — ใช้ default");
        }
        // พอร์ต toggle ของ OSC host (ใช้เป็นหัวใจ liveness) — จาก nvidia-osc.json
        try
        {
            var osc = JsonDocument.Parse(File.ReadAllText(
                ROOT + @"\Project\NvConfig\nvidia-osc.json")).RootElement;
            OscTogglePort = osc.GetProperty("toggle").GetInt32();
        }
        catch { }
    }

    static string Str(string name, string def)
    {
        try { var v = C.GetProperty(name).GetString(); return string.IsNullOrEmpty(v) ? def : v; }
        catch { return def; }
    }

    static string Child(string name)
    {
        try { return Path.Combine(BuildRoot, C.GetProperty("children").GetProperty(name).GetString()); }
        catch { return null; }
    }

    // ---- pairing kernel objects ----
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr CreateEventW(IntPtr attrs, bool manual, bool init, string name);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr CreateFileMappingW(IntPtr hFile, IntPtr attrs, uint prot, uint maxHi, uint maxLo, string name);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr h, uint access, uint hi, uint lo, uint n);

    const uint PAGE_READWRITE = 0x04, FILE_MAP_ALL = 0x0F;

    static void HoldPairing()
    {
        string evName, mapName; int port; string secret;
        try
        {
            var p = C.GetProperty("pairing");
            evName = p.GetProperty("eventName").GetString();
            mapName = p.GetProperty("mapName").GetString();
            port = p.GetProperty("port").GetInt32();
            secret = p.GetProperty("secret").GetString();
        }
        catch
        {
            // ★ coexistence: ชื่อ MMF/event ต้องไม่ซ้ำกับ NVIDIA App แท้ (มันใช้ตัวเดิม)
            evName = @"Global\{1E6C4F0F-8ABF-4709-AAAD-0341CF2D44A7}-Duluka";
            mapName = "{8BA1E16C-FC54-4595-9782-E370A5FBE8DA}-Duluka";
            port = 59001; secret = "0CA906D2784F0D14E399874F5C5ED4A1";
        }
        CreateEventW(IntPtr.Zero, false, false, evName);
        var map = CreateFileMappingW(new IntPtr(-1), IntPtr.Zero, PAGE_READWRITE, 0, 4096, mapName);
        if (map == IntPtr.Zero) { Log("[pairing] MMF create failed err=" + Marshal.GetLastWin32Error()); return; }
        var view = MapViewOfFile(map, FILE_MAP_ALL, 0, 0, 4096);
        if (view == IntPtr.Zero) { Log("[pairing] map view failed err=" + Marshal.GetLastWin32Error()); return; }
        var json = Encoding.ASCII.GetBytes("{\"port\":" + port + ",\"secret\":\"" + secret + "\"}");
        var buf = new byte[4096];
        Array.Copy(json, buf, json.Length);
        Marshal.Copy(buf, 0, view, 4096);
        Log("[pairing] event + MMF held (port " + port + " + secret)");
    }

    // ไม่มี launcher แล้วในโหมด ours — แม่เขียนสัญญาณ wake ให้ node เอง
    static void WriteNvNodeInit()
    {
        try
        {
            string pathRaw = Str("nvnodeInitPath",
                "%LOCALAPPDATA%\\NVIDIA Corporation\\NvNode\\nvnode-init-duluka.json");
            string path = Environment.ExpandEnvironmentVariables(pathRaw);
            string secret = "0CA906D2784F0D14E399874F5C5ED4A1";
            int port = 59001;
            try
            {
                var p = C.GetProperty("pairing");
                port = p.GetProperty("port").GetInt32();
                secret = p.GetProperty("secret").GetString();
            } catch { }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path,
                "{\"port\":" + port + ",\"secret\":\"" + secret.ToLower() + "\"}",
                Encoding.ASCII);
            Log("[nvnode-init] เขียน " + path);
        }
        catch (Exception ex) { Log("[nvnode-init] FAIL: " + ex.Message); }
    }

    static Process Spawn(string exe, string wd) { return Spawn(exe, wd, null); }

    static Process Spawn(string exe, string wd, string args)
    {
        try
        {
            var psi = new ProcessStartInfo(exe)
            {
                Arguments = args ?? "",
                WorkingDirectory = wd,
                UseShellExecute = true,
            };
            // บอกลูกว่า config hub อยู่ไหน (env ธรรมดา ใช้ได้กับ UseShellExecute=false เท่านั้น
            // → ใช้ส่งผ่าน command line แทนไม่ได้เพราะลูกไม่รับ argv → set env ผ่าน registry ไม่คุ้ม
            // ลูกอ่าน Project\NvConfig\<ตัวเอง>.json เองตาม contract)
            return Process.Start(psi);
        }
        catch (Exception ex)
        {
            Log("[spawn] FAIL " + Path.GetFileName(exe) + ": " + ex.Message);
            return null;
        }
    }

    // ★ coexistence: NVIDIA App แท้มี process ชื่อเดียวกับของเรา (NVIDIA Web Helper.exe,
    //   nvcontainer) — เช็คชื่อเฉย ๆ จะหลอกว่า "ลูกเรายังมีชีวิต" ทั้งที่ตาย
    //   นับเฉพาะตัวที่ exe อยู่ใต้ tree ของเรา (build\NVIDIA ShadowPlay\)
    static string OurRoot()
    {
        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", ".."));
    }
    static bool UnderOurRoot(Process p)
    {
        try { return p.MainModule?.FileName != null &&
                   p.MainModule.FileName.StartsWith(OurRoot(), StringComparison.OrdinalIgnoreCase); }
        catch { return false; }   // access denied = ไม่ใช่ของเรา (ของแท้ก็ไม่อ่านได้)
    }
    static bool Alive(string name)
    {
        foreach (var p in Process.GetProcessesByName(name))
            if (UnderOurRoot(p)) return true;
        return false;
    }

    // liveness ของ OSC ด้วยพอร์ต toggle (:59003) — ชื่อ process ไม่พอ:
    // ซาก loader-stall ชื่อ "NVIDIA OSC" ค้างในระบบแล้วหลอกการเช็คชื่อ (เหตุการณ์ 05:50)
    static int OscTogglePort = 59003;
    static bool PortOpen(int port)
    {
        try
        {
            using var c = new TcpClient();
            if (c.ConnectAsync("127.0.0.1", port).Wait(700) && c.Connected) return true;
        }
        catch { }
        return false;
    }
    static bool OscAlive()
    {
        return PortOpen(OscTogglePort);
    }
    // OSC ที่กำลังบูต (มีชีวิต อายุ < 60 วิ แต่พอร์ตยังไม่เปิด) — watchdog ต้องรอ
    static bool YoungOscAlive()
    {
        foreach (var p in Process.GetProcessesByName("NVIDIA OSC"))
        {
            try
            {
                if (!UnderOurRoot(p)) continue;
                if (DateTime.Now - p.StartTime < TimeSpan.FromSeconds(60)) return true;
            }
            catch { }
        }
        return false;
    }

    static void Log(string m)
    {
        var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + m;
        try
        {
            Directory.CreateDirectory(LogDir);
            File.AppendAllText(Path.Combine(LogDir, "NvContainer.log"),
                line + Environment.NewLine, Encoding.UTF8);
        }
        catch { }
    }

    static void Main(string[] args)
    {
        bool created;
        var single = new Mutex(true, @"Local\Duluka.NvContainer.Mother", out created);
        if (!created) return;
        GC.KeepAlive(single);

        LoadConfig();

        // --genuine ใน argv ชนะ config
        foreach (var a in args) if (string.Equals(a, "--genuine", StringComparison.OrdinalIgnoreCase)) ModeOurs = false;

        // WinExe: ห้ามแตะ Console ทุกชนิด
        Log("=== NvContainer (ตัวแม่) boot — โหมด " + (ModeOurs ? "OURS (ของเราทั้งสาย)" : "GENUINE (Share แท้)") + " ===");

        // โหมดแท้: ไม่ยึด pairing MMF — ให้ NvNode ของ NVIDIA เป็นเจ้าของเอง
        // (แม่เราเขียนทับจะทำ Share แท้อ่านค่า pairing ผิด)
        if (ModeOurs) HoldPairing();

        if (ModeOurs)
        {
            WriteNvNodeInit();

            var host = Child("webHelperExe");
            var osc  = Child("oscExe");
            var hk   = Child("hotkeyExe");

            // 1) node backend ก่อน (หน้า OSC จะได้มี :59001 ตั้งแต่เปิด)
            if (host != null && File.Exists(host))
            {
                if (!Alive("NVIDIA Web Helper")) Spawn(host, Child("webHelperWd") ?? Path.GetDirectoryName(host));
            }
            else Log("[boot] ไม่เจอ NVIDIA Web Helper.exe (" + host + ")");

            // 2) nvsphelper (Alt+Z)
            if (hk != null && File.Exists(hk))
            {
                if (!Alive("nvsphelper")) Spawn(hk, Path.GetDirectoryName(hk));
            }
            else Log("[boot] ไม่เจอ nvsphelper.exe (" + hk + ")");

            // 3) หน้าต่าง OSC ของเรา
            if (osc != null && File.Exists(osc)) Spawn(osc, Child("oscWd") ?? Path.GetDirectoryName(osc));
            else Log("[boot] ไม่เจอ NVIDIA OSC.exe (" + osc + ") — ยังไม่ได้ build? (Phase 2)");

            WatchOurs();
        }
        else
        {
            var share = Child("genuineShareExe");
            if (share == null || !File.Exists(share)) { Log("[boot] ไม่เจอ NVIDIA Share.exe ที่ " + share); return; }
            Log("[spawn] NVIDIA Share.exe (แท้) — มันเลี้ยง node เองผ่าน launcher");
            if (!Alive("nvsphelper"))
            {
                var hk = Child("hotkeyExe");
                if (hk != null && File.Exists(hk)) Spawn(hk, Path.GetDirectoryName(hk));
            }
            Spawn(share, Child("genuineShareWd") ?? Path.GetDirectoryName(share));
            WatchGenuine(share);
        }
    }

    // ---- watchdog โหมด ours: ลูก 3 ตัวของเรา ----
    static void WatchOurs()
    {
        var host = Child("webHelperExe");
        var osc  = Child("oscExe");
        var hk   = Child("hotkeyExe");
        int interval = 15;
        try { interval = C.GetProperty("watchdog").GetProperty("intervalSeconds").GetInt32(); } catch { }
        var spills = 0;

        while (true)
        {
            // ★ แม่ห้ามตาย: exception ใด ๆ ในรอบเฝ้ายาม (process race, IO) = log แล้วรอบต่อไป
            try
            {
                Thread.Sleep(interval * 1000);

                if (hk != null && File.Exists(hk) && !Alive("nvsphelper")) Spawn(hk, Path.GetDirectoryName(hk));

                // host ตาย = node ตายตาม (job object) → :59001 ว่าง → spawn ใหม่ได้เลย
                if (host != null && File.Exists(host) && !Alive("NVIDIA Web Helper"))
                {
                    WipeOrphans();
                    spills++;
                    Log("[watchdog] Web Helper หาย (รอบที่ " + spills + ") — spawn ใหม่");
                    Spawn(host, Child("webHelperWd") ?? Path.GetDirectoryName(host));
                }

                // หน้าต่าง OSC: เช็คพอร์ต toggle — ตายจริง (พอร์ตปิด) ค่อยเปิดใหม่
                // ★ แต่บูต CEF+GPU ใช้เวลาได้เกิน 15 วิ (เมื่อ GPU ต้องแบ่งกับ NVIDIA App แท้)
                //   → ถ้า process ยังอยู่และอายุ < 60 วิ = กำลังบูต ห้ามฆ่า
                if (osc != null && File.Exists(osc) && !OscAlive() && !YoungOscAlive())
                {
                    // เช็ดซากที่ค้างก่อน (best-effort — ซากที่ฆ่าไม่ตายไม่ถือพอร์ต ไม่ขวาง)
                    foreach (var p in Process.GetProcessesByName("NVIDIA OSC"))
                    { try { p.Kill(); } catch { } }
                    Thread.Sleep(1000);
                    Log("[watchdog] NVIDIA OSC ตาย (:59003 ปิด) — spawn ใหม่");
                    Spawn(osc, Child("oscWd") ?? Path.GetDirectoryName(osc));
                }

                // capture engine (NvCapture.exe — ตัวอัด ตามสถาปัตยกรรม): headless --rest
                // อยู่ตลอด รอคำสั่ง /Record/Enable จากหน้า OSC (edge ผ่าน /Duluka plane)
                var cap = Child("captureExe");
                if (cap != null && File.Exists(cap) && !Alive("NvCapture"))
                {
                    Log("[watchdog] NvCapture (ตัวอัด) หาย — spawn ใหม่");
                    Spawn(cap, Child("captureWd") ?? Path.GetDirectoryName(cap), Child("captureArgs") ?? "--rest");
                }
            }
            catch (Exception ex)
            {
                try { Log("[watchdog] รอบยกเลิก: " + ex.GetType().Name + " — " + ex.Message); } catch { }
            }
        }
    }

    // ---- watchdog โหมด genuine (พฤติกรรมเดิม) ----
    static void WatchGenuine(string share)
    {
        var hk = Child("hotkeyExe");
        int interval = 15;
        try { interval = C.GetProperty("watchdog").GetProperty("intervalSeconds").GetInt32(); } catch { }
        var spills = 0;

        while (true)
        {
            Thread.Sleep(interval * 1000);
            if (hk != null && File.Exists(hk) && !Alive("nvsphelper")) Spawn(hk, Path.GetDirectoryName(hk));

            if (!Alive("NVIDIA Share"))
            {
                WipeOrphans();
                Thread.Sleep(2000);
                spills++;
                Log("[watchdog] Share หาย (รอบที่ " + spills + ") — เช็ด orphan แล้ว spawn ใหม่");
                Spawn(share, Child("genuineShareWd") ?? Path.GetDirectoryName(share));
            }
        }
    }

    static void WipeOrphans()
    {
        string[] names;
        try { names = System.Text.Json.JsonSerializer.Deserialize<string[]>(
                  C.GetProperty("watchdog").GetProperty("orphanWipeNames").GetRawText()); }
        catch { names = new[] { "nvnodejslauncher", "node", "NVIDIA Web Helper" }; }
        // ★ coexistence: เช็ดเฉพาะ orphan "ใต้ tree เรา" — ของแท้ (Program Files) ห้ามแตะ
        foreach (var n in names)
            foreach (var p in Process.GetProcessesByName(n))
            {
                if (!UnderOurRoot(p)) continue;
                try { Log("[watchdog] เช็ด orphan " + n + " PID " + p.Id); p.Kill(); }
                catch { }
            }
    }
}
