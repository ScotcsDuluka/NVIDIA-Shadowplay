// Common.cs — paths, models, helpers ของ NvPlugins
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace NvPlugins
{
    public static class P
    {
        public const string ROOT    = @"C:\My Project\NVIDIA-Shadowplay";
        public const string BUILD   = ROOT + @"\build\NVIDIA ShadowPlay";
        public const string PF      = @"C:\Program Files\NVIDIA Corporation";
        public const string GFE     = PF + @"\NVIDIA GeForce Experience";
        public const string PAYLOAD = ROOT + @"\Project\OscProvision\Payload";
        public const string CCLOG   = @"C:\ProgramData\NVIDIA Corporation\ShadowPlay\CaptureCore.log";
        public const string SVC     = @"HKLM\SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem";
        public const string WD      = @"HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog";
        public const string NODE_URL = "http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare";
    }

    public enum St { OK, FAIL, WARN }

    public class CheckItem
    {
        public string Category = "";    // File / Registry / Server / Port / DLL / Connection
        public string Name = "";
        public St Status = St.WARN;
        public string Expected = "";
        public string Actual = "";
        public Action Fix = null;       // null = ซ่อมอัตโนมัติไม่ได้
        public string FixLabel = "";
        public string Note = "";
    }

    public static class U
    {
        public static bool IsAdmin()
        {
            using var wi = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(wi).IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool FileOk(string path, long? minSize = null)
        {
            if (!File.Exists(path)) return false;
            if (minSize.HasValue && new FileInfo(path).Length < minSize.Value) return false;
            return true;
        }

        public static bool BytesEqual(string a, string b)
        {
            if (!File.Exists(a) || !File.Exists(b)) return false;
            var fa = new FileInfo(a); var fb = new FileInfo(b);
            if (fa.Length != fb.Length) return false;
            using var ha = fa.OpenRead(); using var hb = fb.OpenRead();
            Span<byte> ba = stackalloc byte[8192]; Span<byte> bb = stackalloc byte[8192];
            int n;
            while ((n = ha.Read(ba)) > 0)
            {
                if (hb.Read(bb) != n) return false;
                for (int i = 0; i < n; i++) if (ba[i] != bb[i]) return false;
            }
            return true;
        }

        public static string RegGet(string keyPath, string valueName)
        {
            // keyPath: HKLM\SOFTWARE\... ; valueName ว่าง = (Default)
            var parts = keyPath.Split(new[] { '\\' }, 2);
            var root = parts[0] == "HKLM" ? Microsoft.Win32.Registry.LocalMachine
                     : parts[0] == "HKCU" ? Microsoft.Win32.Registry.CurrentUser
                     : throw new ArgumentException(keyPath);
            using var k = root.OpenSubKey(parts[1]);
            if (k == null) return null;
            var v = valueName.Length == 0 ? k.GetValue("") : k.GetValue(valueName);
            if (v is byte[] b) return Convert.ToHexString(b);
            return v == null ? null : v.ToString();
        }

        public static int RegGetDword(string keyPath, string valueName, int def = -1)
        {
            var s = RegGet(keyPath, valueName);
            if (s == null) return def;
            try
            {
                var o = Convert.ToInt64(s, s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? 16 : 10);
                return unchecked((int)o);
            }
            catch { return def; }
        }

        public static void RegSet(string keyPath, string valueName, string value)
        {
            var parts = keyPath.Split(new[] { '\\' }, 2);
            var root = parts[0] == "HKLM" ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser;
            using var k = root.CreateSubKey(parts[1], true);
            k.SetValue(valueName.Length == 0 ? "" : valueName, value, Microsoft.Win32.RegistryValueKind.String);
        }

        public static void RegSetExpand(string keyPath, string valueName, string value)
        {
            var parts = keyPath.Split(new[] { '\\' }, 2);
            using var k = (parts[0] == "HKLM" ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser).CreateSubKey(parts[1], true);
            k.SetValue(valueName, value, Microsoft.Win32.RegistryValueKind.ExpandString);
        }

        public static void RegSetDword(string keyPath, string valueName, int value)
        {
            var parts = keyPath.Split(new[] { '\\' }, 2);
            using var k = (parts[0] == "HKLM" ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser).CreateSubKey(parts[1], true);
            k.SetValue(valueName, value, Microsoft.Win32.RegistryValueKind.DWord);
        }

        public static void RegSetBinaryZeros(string keyPath, string valueName)
        {
            var parts = keyPath.Split(new[] { '\\' }, 2);
            using var k = (parts[0] == "HKLM" ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser).CreateSubKey(parts[1], true);
            var cur = k.GetValue(valueName) as byte[];
            k.SetValue(valueName, cur != null ? new byte[cur.Length] : new byte[28], Microsoft.Win32.RegistryValueKind.Binary);
        }

        public static bool PortListening(int port)
        {
            try
            {
                var ip = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
                foreach (var ep in ip.GetActiveTcpListeners())
                    if (ep.Port == port) return true;
            }
            catch { }
            return false;
        }

        public static int NodeHttp()   // 0 = fail, else status code
        {
            try
            {
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(P.NODE_URL);
                req.Method = "GET"; req.Timeout = 2500; req.Proxy = null;
                using var rsp = (System.Net.HttpWebResponse)req.GetResponse();
                return (int)rsp.StatusCode;
            }
            catch (System.Net.WebException we)
            {
                if (we.Response is System.Net.HttpWebResponse r) return (int)r.StatusCode;
                return 0;
            }
            catch { return 0; }
        }

        public static Dictionary<long, (string exe, string cmd)> ProcMap()
        {
            var map = new Dictionary<long, (string, string)>();
            // WMI ค้างได้ถ้า repository ยุ่ง — ใส่ watchdog 5 วิ (ตรวจ path ไม่ได้ก็ปล่อย ไม่ให้ GUI ตาย)
            var task = System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    using var searcher = new System.Management.ManagementObjectSearcher(
                        "SELECT ProcessId, ExecutablePath, CommandLine FROM Win32_Process");
                    foreach (var o in searcher.Get())
                    {
                        var pid = Convert.ToInt64(o["ProcessId"]);
                        var exe = o["ExecutablePath"] as string ?? "";
                        var cmd = o["CommandLine"] as string ?? "";
                        map[pid] = (exe, cmd);
                    }
                }
                catch { }
            });
            task.Wait(5000);
            return map;
        }
    }

    // ---- Plugin contract (ฝั่ง host) ----
    public interface INvHost
    {
        void Log(string msg);
        string ConfigDir();          // โฟลเดอร์ config ของ host
        string PayloadDir();
    }

    public interface INvPlugin
    {
        string GetInfo();            // "Name vX — description"
        void Init(INvHost host);
        void Start();
        void Stop();
    }
}
