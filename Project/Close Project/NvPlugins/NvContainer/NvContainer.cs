// NvContainer.cs — ตัวแม่ของระบบ (ของเราเอง ไม่ใช่ของ NVIDIA)
// หน้าที่: spawn shim backend (node.exe index.js ผ่าน host) + spawn Share.exe
//         + watchdog ทั้งสอง (ตายเกิดใหม่เอง) + ถือ pairing event/MMF ให้ทั้งระบบ
// รัน: NvContainer.exe  — ปิด = Ctrl+C (ลูกทั้งหมดถูก kill ตาม)
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

static class NvContainer
{
    // ---- paths (layout ใหม่: ทุกอยู่ใน build\NVIDIA ShadowPlay\) ----
    const string ROOT    = @"C:\My Project\NVIDIA-Shadowplay";
    const string HOST    = ROOT + @"\build\NVIDIA ShadowPlay\Overlay OSC\NvNode\NVIDIA Web Helper.exe";
    const string HOSTWD  = ROOT + @"\build\NVIDIA ShadowPlay\Overlay OSC\NvNode";
    const string SHARE   = ROOT + @"\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA Share\NVIDIA Share.exe";
    const string SHAREWD = ROOT + @"\build\NVIDIA ShadowPlay\Overlay OSC\NVIDIA Share";
    const string CONFIG  = ROOT + @"\Project\NvConfig";   // จุดรวม config ทุกอย่าง

    // ---- pairing kernel objects (เหมือนที่ NVIDIA node เขียนเอง) ----
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr CreateEventW(IntPtr attrs, bool manual, bool init, string name);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern IntPtr CreateFileMappingW(IntPtr hFile, IntPtr attrs, uint prot, uint maxHi, uint maxLo, string name);
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr MapViewOfFile(IntPtr h, uint access, uint hi, uint lo, uint n);

    const uint PAGE_READWRITE = 0x04, FILE_MAP_ALL = 0x0F;
    const string EVENT_NAME = @"Global\{1E6C4F0F-8ABF-4709-AAAD-0341CF2D44A7}";
    const string MAP_NAME = "{8BA1E16C-FC54-4595-9782-E370A5FBE8DA}";
    const string NODE_JSON = "{\"port\":59001,\"secret\":\"0CA906D2784F0D14E399874F5C5ED4A1\"}";

    static void HoldPairing()
    {
        // ตัวแม่ถือ event (liveness ของ launcher) + MMF (port+secret ให้ Share เจอ node)
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
                Log("[pairing] event + MMF held (port 59001 + secret)");
            }
        }
    }

    static Process Spawn(string exe, string wd)
    {
        var p = new ProcessStartInfo(exe) { WorkingDirectory = wd, UseShellExecute = true };
        return Process.Start(p);
    }

    static bool Alive(string name)
    {
        foreach (var p in Process.GetProcessesByName(name)) return true;
        return false;
    }

    static void Log(string m) { try { Console.WriteLine(m); } catch { } }

    static void Main()
    {
        // winexe: ไม่มี console — ห้ามแตะ Console.Title (handle invalid = crash)
        HoldPairing();

        // ⚠ ไม่ spawn node/host เอง — Share→launcher เป็นเจ้าของ node
        //    (แม่ spawn ซ้ำ = ชน :59001 = launcher ค้าง = Share พัง)
        Log("[spawn] NVIDIA Share.exe (หน้า OSC) — มันจะเลี้ยง node เองผ่าน launcher");
        Spawn(SHARE, SHAREWD);

        // watchdog
        while (true)
        {
            Thread.Sleep(15000);
            if (!Alive("NVIDIA Share"))
            {
                // เช็ด orphan ก่อน respawn: Share ตายแต่ host/node ไม่ตายตาม
                // → :59001 ถูกกิน → Share ใหม่ spawn node ชนพอร์ต = เปิดไม่ได้วนลูป
                foreach (var n in new[] { "nvnodejslauncher", "node", "NVIDIA Web Helper" })
                    foreach (var p in Process.GetProcessesByName(n))
                    { try { Log("[watchdog] เช็ด orphan " + n + " PID " + p.Id); p.Kill(); } catch { } }
                Thread.Sleep(2000);
                Log("[watchdog] Share หาย — เช็ด orphan แล้ว spawn ใหม่");
                Spawn(SHARE, SHAREWD);
            }
        }
    }
}
