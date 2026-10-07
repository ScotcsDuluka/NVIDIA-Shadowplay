// NvHotkeyHelper.cs — NVIDIA ShadowPlay Helper (ของเรา 100%) — แบบของแท้
// บทบาท (เหมือน nvsphelper64.exe แท้): เป็น "ตัวจับ hotkey ระดับ OS" ตัวเดียวของระบบ
//   • อ่านคีย์ที่ผู้ใช้ปรับจาก NvNode\shadowplay-settings.json (หน้า OSC settings เขียน)
//   • RegisterHotKey ทุกตัวที่มี modifier (Alt+F1, Alt+F9, Ctrl+Alt+R …)
//   • กดแล้วยิง :59002/?hk=<Name> → node shim → socket /ShadowPlay/v.1.0/Hotkey → หน้าทำงาน
//   • FileSystemWatcher ตามไฟล์ settings — แก้คีย์ใน OSC แล้วมีผลทันที (ไม่ต้องรีสตาร์ท)
//   • กด Alt+Z = OpenShare (toggle overlay) — ยิง :59003 คู่ด้วย (โหมด genuine ผ่านได้)
// config: Project\NvConfig\nvsphelper.json
//
// WinExe: ห้ามแตะ Console ทุกชนิด (handle invalid = crash) — log ลงไฟล์เท่านั้น
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;

static class NvShadowPlayHelper
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool RegisterHotKey(IntPtr h, int id, uint mods, uint vk);
    [DllImport("user32.dll")]
    static extern bool UnregisterHotKey(IntPtr h, int id);
    [DllImport("user32.dll")]
    static extern int GetMessageW(out MSG m, IntPtr h, uint a, uint b);
    [DllImport("user32.dll")]
    static extern int PeekMessageW(out MSG m, IntPtr h, uint a, uint b, uint remove);

    [StructLayout(LayoutKind.Sequential)]
    struct MSG { public IntPtr hwnd; public uint message; public IntPtr wParam; public IntPtr lParam; public uint time; public int ptX; public int ptY; }

    const uint WM_HOTKEY = 0x0312;
    const uint MOD_ALT = 0x1, MOD_CONTROL = 0x2, MOD_SHIFT = 0x4, MOD_WIN = 0x8;

    const string CONFIG = @"C:\My Project\NVIDIA-Shadowplay\Project\NvConfig\nvsphelper.json";
    const string SETTINGS = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Overlay OSC\NvNode\shadowplay-settings.json";

    static string _fireUrl = "http://127.0.0.1:59011/?hk={name}";
    static string _winUrl = "";   // ว่าง = ไม่ยิงคู่ (หน้าเป็นเจ้าของ UI ตามแท้)
    static string _logFile = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\NvContainer\Logs\nvsphelper.log";

    // ค่า default ตาม GFE แท้ (route-floor HOTKEYS) — ใช้เมื่อ settings ไม่มีชื่อนั้น
    static readonly Dictionary<string, int[]> Defaults = new Dictionary<string, int[]>
    {
        ["openshare"]            = new[] { 18, 90 },    // Alt+Z
        ["screenshot"]           = new[] { 18, 112 },   // Alt+F1
        ["recordtoggle"]         = new[] { 18, 120 },   // Alt+F9
        ["recordsave"]           = new[] { 18, 121 },   // Alt+F10
        ["dvrtoggle"]            = new[] { 16, 18, 121 }, // Ctrl+Alt+F10
        ["instantreplaytoggle"]  = new[] { 18, 117 },   // Alt+F7
        ["broadcasttoggle"]      = new[] { 18, 119 },   // Alt+F8
        ["broadcastpausetoggle"] = new[] { 16, 18, 119 },
        ["cameratoggle"]         = new[] { 18, 67 },    // Alt+C
        ["mictoggle"]            = new[] { 18, 77 },    // Alt+M
        ["fps"]                  = new[] { 18, 80 },    // Alt+P
        ["commentstoggle"]       = new[] { 18, 88 },
        ["modsui"]               = new[] { 18, 114 },   // Alt+F3
        ["modstoggle"]           = new[] { 16, 18, 114 },
        ["nvcameraui"]           = new[] { 18, 113 },
    };

    // ชื่อใน settings (lowercase) → ชื่อทางการที่หน้า/node ใช้ (we.HotkeyShortcuts)
    static readonly Dictionary<string, string> Canonical = new Dictionary<string, string>
    {
        ["openshare"] = "OpenShare", ["ptt"] = "PTT", ["fps"] = "FPS",
        ["screenshot"] = "Screenshot", ["recordsave"] = "RecordSave",
        ["recordtoggle"] = "RecordToggle", ["broadcasttoggle"] = "BroadcastToggle",
        ["broadcastpausetoggle"] = "BroadcastPauseToggle", ["cameratoggle"] = "CameraToggle",
        ["mictoggle"] = "MicToggle", ["dvrtoggle"] = "DVRToggle",
        ["instantreplaytoggle"] = "InstantReplayToggle", ["commentstoggle"] = "CommentsToggle",
        ["overlayaswitch"] = "OverlayASwitch", ["overlaybswitch"] = "OverlayBSwitch",
        ["overlaycswitch"] = "OverlayCSwitch", ["modsui"] = "ModsUI",
        ["modstoggle"] = "ModsToggle", ["modspreset1"] = "ModsPreset1",
        ["modspreset2"] = "ModsPreset2", ["modspreset3"] = "ModsPreset3",
        ["nvcameraui"] = "NvCameraUI", ["octooluitoggle"] = "OcToolUIToggle",
    };

    // id ที่ลงทะเบียนอยู่: id → (canonicalName, mods, vk)
    static readonly Dictionary<int, Tuple<string, uint, uint>> Registered = new Dictionary<int, Tuple<string, uint, uint>>();
    static int _nextId = 1;
    // RegisterHotKey(NULL,…) ผูกกับเธรดที่เรียก — re-register ต้องเกิดบนเธรดหลักเท่านั้น
    // (watcher แค่ปักธง หลักวนมาเห็นแล้วค่อยทำ — ไม่งั้นถอดของเดิมไม่ได้ = 1409 ค้าง)
    static volatile bool _reregister;
    // ★ retry: คีย์ที่ลงทะเบียนไม่ได้ตอนบูต (มีคนถือชั่วคราว เช่น grip ตอนปิดสายแท้)
    //   ลองซ้ำทุก 10 วิ จนได้ — ได้เมื่อไหร่ Alt+X กลับมาเอง
    static readonly Dictionary<string, Tuple<uint, uint>> FailedKeys = new Dictionary<string, Tuple<uint, uint>>();
    static long _lastRetry = -100000;
    // ★ upgrade-retry: คีย์ที่ได้ fallback (+Shift) ไว้ก่อน — ลองเอาคีย์หลักคืนทุก 10 วิ
    static readonly Dictionary<string, Tuple<uint, uint>> UpgradableKeys = new Dictionary<string, Tuple<uint, uint>>();
    static readonly Dictionary<string, long> _lastFire = new Dictionary<string, long>();

    static void LoadConfig()
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(CONFIG));
            var r = doc.RootElement;
            if (r.TryGetProperty("fireUrl", out var v)) _fireUrl = v.GetString();
            if (r.TryGetProperty("windowToggleUrl", out v)) _winUrl = v.GetString();
            if (r.TryGetProperty("logFile", out v)) _logFile = v.GetString();
        }
        catch { /* default ไว้แล้ว */ }
    }

    static void Log(string m)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_logFile));
            File.AppendAllText(_logFile,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + m + Environment.NewLine,
                Encoding.UTF8);
        }
        catch { }
    }

    // อ่านคีย์ทั้งหมด: settings ของผู้ใช้ก่อน → default มาเติมที่ขาด
    static Dictionary<string, int[]> LoadAllKeys()
    {
        var all = new Dictionary<string, int[]>(StringComparer.Ordinal);
        foreach (var kv in Defaults) all[kv.Key] = kv.Value;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(SETTINGS));
            if (doc.RootElement.TryGetProperty("hotkeys", out var hk) && hk.ValueKind == JsonValueKind.Object)
            {
                foreach (var p in hk.EnumerateObject())
                {
                    if (p.Value.ValueKind != JsonValueKind.Array) continue;
                    var keys = new List<int>();
                    foreach (var e in p.Value.EnumerateArray())
                        if (e.TryGetInt32(out int k)) keys.Add(k);
                    if (keys.Count > 0) all[p.Name.ToLowerInvariant()] = keys.ToArray();
                }
            }
        }
        catch { /* ยังไม่มีไฟล์ = ใช้ default */ }
        return all;
    }

    static void RegisterAll()
    {
        // ถอดของเดิมก่อน (re-register ทุกครั้งที่ settings เปลี่ยน) — เช็คผลลัพธ์ด้วย
        foreach (var kv in Registered)
        {
            if (!UnregisterHotKey(IntPtr.Zero, kv.Key))
                Log("[hk] unregister id " + kv.Key + " FAILED err=" + Marshal.GetLastWin32Error());
        }
        Registered.Clear();

        var all = LoadAllKeys();
        foreach (var kv in all)
        {
            var keys = kv.Value;
            // vk = ตัวสุดท้ายที่ไม่ใช่ modifier; mods = รวมจากตัวที่เหลือ
            uint mods = 0; uint vk = 0;
            for (int i = 0; i < keys.Length; i++)
            {
                switch (keys[i])
                {
                    case 16: case 160: case 161: mods |= MOD_SHIFT; break;
                    case 17: case 162: case 163: mods |= MOD_CONTROL; break;
                    case 18: case 164: case 165: mods |= MOD_ALT; break;
                    case 91: case 92: mods |= MOD_WIN; break;
                    default: vk = (uint)keys[i]; break;
                }
            }
            // ข้ามคีย์ล้วน (ไม่มี modifier) — RegisterHotKey กลืนทั้งระบบจะหนักไป
            // (PTT แบบคีย์เดี่ยวทำงานเฉพาะในหน้า OSC ตามของแท้)
            if (vk == 0 || mods == 0)
            {
                Log("[hk] ข้าม " + kv.Key + " (ต้องมี modifier) keys=" + string.Join(",", keys));
                continue;
            }
            int id = _nextId++;
            if (RegisterHotKey(IntPtr.Zero, id, mods, vk))
            {
                Registered[id] = Tuple.Create(kv.Key, mods, vk);
                UpgradableKeys.Remove(kv.Key);   // ★ settings ใหม่ลงสำเร็จ = เลิก upgrade คีย์นี้ (กันทับคีย์ที่ user เลือก)
                Log("[hk] " + kv.Key + " = " + string.Join(",", keys) + " (id " + id + ")");
            }
            else
            {
                // ★ coexistence: NVIDIA App แท้จดชุด default (Alt+Z, Alt+F9 ฯลฯ) ไปก่อน
                //   → เลื่อนคีย์ด้วย Shift พิเศษ (Alt+Shift+Z, Alt+Shift+F9 …) ให้ได้ทุกตัว
                uint shifted = mods | MOD_SHIFT;
                if ((mods & MOD_SHIFT) == 0 && RegisterHotKey(IntPtr.Zero, id, shifted, vk))
                {
                    Registered[id] = Tuple.Create(kv.Key, shifted, vk);
                    UpgradableKeys[kv.Key] = Tuple.Create(mods, vk);   // เก็บ mods หลัก (Alt) — upgrade จะได้ Alt+X จริง
                    Log("[hk] " + kv.Key + " ชนกับแท้ → fallback = +Shift (id " + id + ")");
                }
                else {
                    FailedKeys[kv.Key] = Tuple.Create(mods, vk);
                    Log("[hk] " + kv.Key + " register FAILED err=" + Marshal.GetLastWin32Error() + " (จะ retry ทุก 10 วิ)");
                }
            }
        }
        Log("[hk] รวมลงทะเบียน " + Registered.Count + " ตัว");
    }

    static void Main()
    {
        // dedup: หลาย instance จะแย่งกัน RegisterHotKey — เหลือตัวเดียว
        bool created;
        var single = new Mutex(true, @"Local\Duluka.nvsphelper", out created);
        if (!created) return;
        GC.KeepAlive(single);

        LoadConfig();
        RegisterAll();

        // ตามไฟล์ settings — ผู้ใช้แก้คีย์ใน OSC = มีผลทันที
        var watcher = new FileSystemWatcher(Path.GetDirectoryName(SETTINGS), Path.GetFileName(SETTINGS))
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        // ปักธงเท่านั้น — re-register ให้เธรดหลักทำ (hotkey ผูกเธรด)
        watcher.Changed += delegate { _reregister = true; };

        var m = new MSG();
        while (true)
        {
            // Peek (ไม่บล็อก) — เพื่อเช็คธง re-register สม่ำเสมอ
            while (PeekMessageW(out m, IntPtr.Zero, 0, 0, 1) != 0)
            {
                if (m.message != WM_HOTKEY) continue;
                int id = m.wParam.ToInt32();
                if (!Registered.TryGetValue(id, out var info)) continue;
                string name = info.Item1;
                string canonical = Canonical.TryGetValue(name, out var c) ? c : name;
                // ★ debounce: คีย์แช่ (auto-repeat) ยิงซ้ำใน <600ms — กลืนซ้ำ
                var nowS = Environment.TickCount64;
                if (_lastFire.TryGetValue(canonical, out var lastT) && nowS - lastT < 600) continue;
                _lastFire[canonical] = nowS;
                Log("[hotkey] " + name + " (" + canonical + ") -> fire");
                var n = canonical;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    // ★ 2026-10-07: fireUrl ว่าง = ข้าม (Alt+X/G ใช้ทางเดียว :59013/toggle —
                    //   เดิมยิงสองทาง node+host = หน้า toggle สองรอบ = เปิดแล้วปิดทันที)
                    if (_fireUrl.Length > 0) Post(_fireUrl.Replace("{name}", n));
                    if (name == "openshare" && _winUrl.Length > 0) Post(_winUrl);   // host เรา (โหมด genuine ผ่านได้)
                });
            }
            if ((FailedKeys.Count > 0 || UpgradableKeys.Count > 0) && Environment.TickCount64 - _lastRetry > 10000) {
                _lastRetry = Environment.TickCount64;
                // ★ upgrade: คีย์ที่ใช้ fallback — ถอด fallback ลองคีย์หลัก ถ้าไม่ได้ใส่ fallback กลับ
                var ups = new Dictionary<string, Tuple<uint, uint>>(UpgradableKeys);
                foreach (var kv in ups) {
                    var toRemove = new System.Collections.Generic.List<int>();
                    foreach (var reg in Registered) { if (reg.Value.Item1 == kv.Key) toRemove.Add(reg.Key); }
                    foreach (var ridOld in toRemove) {
                        UnregisterHotKey(IntPtr.Zero, ridOld);
                        Registered.Remove(ridOld);
                    }
                    int nid = _nextId++;
                    if (RegisterHotKey(IntPtr.Zero, nid, kv.Value.Item1, kv.Value.Item2)) {
                        Registered[nid] = Tuple.Create(kv.Key, kv.Value.Item1, kv.Value.Item2);
                        UpgradableKeys.Remove(kv.Key);
                        Log("[hk] upgrade สำเร็จ: " + kv.Key + " ได้คีย์หลักกลับ (id " + nid + ")");
                    } else {
                        int rid2 = _nextId++;
                        RegisterHotKey(IntPtr.Zero, rid2, kv.Value.Item1, kv.Value.Item2);
                        Registered[rid2] = Tuple.Create(kv.Key, kv.Value.Item1, kv.Value.Item2);
                        Log("[hk] upgrade ยังไม่ว่าง — ใช้ fallback ต่อ: " + kv.Key);
                    }
                }
                var retry = new Dictionary<string, Tuple<uint, uint>>(FailedKeys);
                foreach (var kv in retry) {
                    int rid = _nextId++;
                    if (RegisterHotKey(IntPtr.Zero, rid, kv.Value.Item1, kv.Value.Item2)) {
                        Registered[rid] = Tuple.Create(kv.Key, kv.Value.Item1, kv.Value.Item2);
                        FailedKeys.Remove(kv.Key);
                        Log("[hk] retry สำเร็จ: " + kv.Key + " = " + kv.Value.Item1 + "+" + kv.Value.Item2 + " (id " + rid + ")");
                    }
                }
            }
            if (_reregister)
            {
                _reregister = false;
                Log("[hk] settings เปลี่ยน — re-register (เธรดหลัก)");
                RegisterAll();
            }
            Thread.Sleep(80);
        }
    }

    static void Post(string url)
    {
        try
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST"; req.ContentType = "application/json"; req.Proxy = null;
            req.ContentLength = 2;
            req.Timeout = 3000;
            var bs = Encoding.UTF8.GetBytes("{}");
            using (var s = req.GetRequestStream()) s.Write(bs, 0, bs.Length);
            using var r = (HttpWebResponse)req.GetResponse();
            Log("[hotkey] " + url + " -> " + (int)r.StatusCode);
        }
        catch (Exception ex) { Log("[hotkey] " + url + " fail: " + ex.Message); }
    }
}
