// OscModes.cs — ตัวเลือกฝั่ง OSC: Genuine NVIDIA / Custom (NVIDIA-free)
// หลักการ: OSC มีชีวิตด้วย "ใครเลี้ยง" — Genuine = container แท้ · Custom = shim node ตอบ Custom Data ล้วน
// ทั้งสองโหมดใช้ .exe NVIDIA แท้ (Share.exe / Web Helper.exe / node.exe)
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace NvPlugins
{
    public static class OscModes
    {
        public const string ShimDir   = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NvNode";
        public const string ShimHost  = ShimDir + @"\NVIDIA Web Helper.exe";
        public const string ShareExe  = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA Share\NVIDIA Share.exe";
        public const string FireUrl   = @"http://127.0.0.1:59002/?hk=OpenShare";
        public const string LaunchUrl = @"http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch";

        static void KillNamed(string name, Action<string> log)
        {
            foreach (var p in Process.GetProcessesByName(name))
            { try { log("[KILL] " + name + " PID " + p.Id); p.Kill(); } catch { } }
        }

        static void Post(string url)
        {
            var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            req.Method = "POST"; req.ContentType = "application/json"; req.Timeout = 8000; req.Proxy = null;
            var b = System.Text.Encoding.UTF8.GetBytes("{}");
            req.ContentLength = b.Length; req.GetRequestStream().Write(b, 0, b.Length);
            using var rsp = (System.Net.HttpWebResponse)req.GetResponse();
        }

        // ---------- BOOT GENUINE (ตัวแม่ = nvcontainer service) ----------
        public static void BootGenuine(Action<string> log)
        {
            log("stop Custom side (shim node / DulukaPort) — kill เฉพาะตัวใต้ ShimDir");
            foreach (var kv in U.ProcMap())
            {
                var exe = kv.Value.exe;
                if (exe.Length == 0) continue;
                var name = Path.GetFileName(exe);
                bool underShim = exe.StartsWith(ShimDir, StringComparison.OrdinalIgnoreCase);
                if (name.Equals("node.exe", StringComparison.OrdinalIgnoreCase) && underShim) { log("[KILL] shim node PID " + kv.Key); try { Process.GetProcessById((int)kv.Key).Kill(); } catch { } }
                else if (name.Equals("NVIDIA Web Helper.exe", StringComparison.OrdinalIgnoreCase) && underShim) { log("[KILL] shim host PID " + kv.Key); try { Process.GetProcessById((int)kv.Key).Kill(); } catch { } }
            }
            OscHotkey.Stop(log);
            Thread.Sleep(2000);

            log("boot genuine stack (service → Share → attach → node → helper)");
            Deploy.BootStack(log);
            Thread.Sleep(2000);
            log("genuine mode ready — Alt+Z เปิด OSC (หรือกด OPEN OSC)");
        }

        // ---------- BOOT CUSTOM (ตัวแม่ = shim node host — ไม่แตะ NVIDIA ระบบใด) ----------
        public static void BootCustom(Action<string> log)
        {
            if (!File.Exists(ShimHost)) { log("ERROR: ไม่พบ shim host: " + ShimHost); return; }

            log("stop Genuine side (node / helper / Share / service / containers)");
            KillNamed("NVIDIA Share", log);
            KillNamed("nvsphelper64", log);
            KillNamed("NVIDIA Web Helper", log);
            CheckPlugin.Cmd("sc.exe stop NvContainerLocalSystem", 4000);
            Thread.Sleep(4000);
            KillNamed("nvcontainer", log);
            Thread.Sleep(1500);

            log("start OscHotkey (pairing MMF + event + Alt+Z — ในแอป)");
            OscHotkey.Start(log);
            Thread.Sleep(1500);

            log("spawn mother — shim node host (ตอบ Custom Data ทุก route)");
            Process.Start(new ProcessStartInfo(ShimHost) { WorkingDirectory = ShimDir, UseShellExecute = true });
            for (int i = 0; i < 30 && !U.PortListening(59001); i++) Thread.Sleep(1000);
            log(U.PortListening(59001) ? "  node :59001 ✓ (shim)" : "  node :59001 ยังไม่ตอบ!");

            log("spawn Share.exe (genuine — nv-osc=true)");
            Process.Start(new ProcessStartInfo(ShareExe) { WorkingDirectory = Path.GetDirectoryName(ShareExe), UseShellExecute = true });
            Thread.Sleep(8000);

            log("custom mode ready — กด OPEN OSC (Alt+Z ก็ได้ ผ่าน OscHotkey)");
        }

        // ---------- OPEN OSC (สั่งเปิด — เดาโหมดจากพอร์ต) ----------
        public static void OpenOsc(Action<string> log)
        {
            if (U.PortListening(59002))
            {
                log("custom mode: fire " + FireUrl);
                Post(FireUrl);
                log("  overlayToggle ส่งแล้ว — หน้าเปิดเอง");
            }
            else
            {
                log("genuine mode: POST /Launch");
                Post(LaunchUrl);
                log("  ส่งแล้ว — กด Alt+Z เพื่อเปิด overlay");
            }
        }
    }

    // ---------- OscHotkey: DulukaPort รวมเข้าแอป (ไม่มี exe แยก) ----------
    // 1) ถือ pairing MMF + event ให้ Share.exe เจอ node
    // 2) จับ Alt+Z → fire :59002 → shim → WindowState overlayToggle
    public static class OscHotkey
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateEventW(IntPtr attrs, bool manual, bool init, string name);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateFileMappingW(IntPtr hFile, IntPtr attrs, uint prot, uint maxHi, uint maxLo, string name);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(IntPtr h, uint access, uint hi, uint lo, uint n);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterHotKey(IntPtr h, int id, uint mods, uint vk);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnregisterHotKey(IntPtr h, int id);
        [DllImport("user32.dll")]
        static extern int GetMessageW(out MSG m, IntPtr h, uint a, uint b);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool PostThreadMessageW(uint threadId, uint msg, IntPtr w, IntPtr l);

        [StructLayout(LayoutKind.Sequential)]
        struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int ptX; public int ptY; }

        const uint WM_HOTKEY = 0x0312, WM_QUIT = 0x0012;
        const uint MOD_ALT = 0x1, VK_Z = 0x5A;
        const uint PAGE_READWRITE = 0x04, FILE_MAP_ALL = 0x0F;
        const string EVENT_NAME = @"Global\{1E6C4F0F-8ABF-4709-AAAD-0341CF2D44A7}";
        const string MAP_NAME = "{8BA1E16C-FC54-4595-9782-E370A5FBE8DA}";
        const string FIRE_URL = @"http://127.0.0.1:59002/?hk=OpenShare";
        const string NODE_JSON = "{\"port\":59001,\"secret\":\"0CA906D2784F0D14E399874F5C5ED4A1\"}";

        static Thread _thread;
        static uint _threadId;
        static volatile bool _running;
        static Action<string> _log = _ => { };

        public static void Start(Action<string> log)
        {
            if (_running) { log("hotkey host รันอยู่แล้ว"); return; }
            _log = log;
            CreateEventW(IntPtr.Zero, false, false, EVENT_NAME);
            var map = CreateFileMappingW(new IntPtr(-1), IntPtr.Zero, PAGE_READWRITE, 0, 4096, MAP_NAME);
            if (map != IntPtr.Zero)
            {
                var view = MapViewOfFile(map, FILE_MAP_ALL, 0, 0, 4096);
                if (view != IntPtr.Zero)
                {
                    var buf = new byte[4096];
                    var j = System.Text.Encoding.ASCII.GetBytes(NODE_JSON);
                    Array.Copy(j, buf, j.Length);
                    Marshal.Copy(buf, 0, view, 4096);
                    log("pairing MMF เขียนแล้ว (port 59001 + secret)");
                }
            }
            _running = true;
            _thread = new Thread(MsgLoop) { IsBackground = true };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
            log("Alt+Z hotkey host เริ่มแล้ว (ในแอป — ไม่ต้องมี DulukaPort.exe)");
        }

        public static void Stop(Action<string> log)
        {
            if (!_running) return;
            _running = false;
            if (_threadId != 0) PostThreadMessageW(_threadId, WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            log("hotkey host หยุดแล้ว");
        }

        static void MsgLoop()
        {
            _threadId = (uint)AppDomain.GetCurrentThreadId();
            if (!RegisterHotKey(IntPtr.Zero, 1, MOD_ALT, VK_Z))
            { _log("[hotkey] RegisterHotKey Alt+Z FAILED err=" + Marshal.GetLastWin32Error()); return; }
            var m = new MSG();
            while (_running && GetMessageW(out m, IntPtr.Zero, 0, 0) > 0)
            {
                if (m.message == WM_HOTKEY && m.wParam.ToInt32() == 1)
                {
                    _log("[hotkey] Alt+Z → fire OpenShare");
                    try { Post(FIRE_URL); } catch { }
                }
            }
            UnregisterHotKey(IntPtr.Zero, 1);
        }

        static void Post(string url)
        {
            var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            req.Method = "POST"; req.ContentType = "application/json"; req.Timeout = 5000; req.Proxy = null;
            var bs = System.Text.Encoding.UTF8.GetBytes("{}");
            req.ContentLength = bs.Length;
            using (var s = req.GetRequestStream()) s.Write(bs, 0, bs.Length);
            using var r = (System.Net.HttpWebResponse)req.GetResponse();
        }
    }
}
