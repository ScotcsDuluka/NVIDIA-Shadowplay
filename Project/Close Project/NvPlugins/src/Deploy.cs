// Deploy.cs — client อัตโนมัติ: manifest → download → place → registry → boot
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NvPlugins
{
    public class ManifestFile { public string Rel = ""; public long Size; }
    public class ManifestRegistry { public string Key = "", Name = "", Type = "", Value = ""; }
    public class Manifest
    {
        public string Set = ""; public int Version;
        public List<ManifestFile> Files = new();
        public List<ManifestRegistry> Registry = new();
    }

    public static class Deploy
    {
        // ---- ปลายทาง hardcoded (ทุกเครื่องเหมือนกัน) ----
        public static string DestinationOf(string rel)
        {
            var parts = rel.Split('/');
            var cat = parts[0]; var name = string.Join("\\", parts.Skip(1));
            return cat switch
            {
                "System32"   => Path.Combine(@"C:\Windows\System32", name),
                "SysWOW64"   => Path.Combine(@"C:\Windows\SysWOW64", name),
                "GFE"        => Path.Combine(@"C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience", name),
                "ShadowPlay" => Path.Combine(@"C:\Program Files\NVIDIA Corporation\ShadowPlay", name),
                "NvContainer"=> Path.Combine(@"C:\Program Files\NVIDIA Corporation\NvContainer", name),
                "NvNode"     => Path.Combine(@"C:\Program Files (x86)\NVIDIA Corporation\NvNode", name),
                _            => Path.Combine(@"C:\Program Files\NVIDIA Corporation\NvPlugins", rel.Replace('/', '\\')),
            };
        }

        public static Manifest GetManifest(string serverBase)
        {
            using var wc = new System.Net.WebClient { Proxy = null, Encoding = System.Text.Encoding.UTF8 };
            var json = wc.DownloadString(serverBase + "/api/manifest");
            var m = new Manifest();
            m.Set = JsonStr(json, "set");
            // files
            var filesBlock = Between(json, "\"files\":[", "]");
            foreach (var obj in SplitObjects(filesBlock))
            {
                m.Files.Add(new ManifestFile { Rel = JsonStr(obj, "rel"), Size = JsonLong(obj, "size") });
            }
            // registry (ค่ามาตรฐานที่ client ตั้งหลังวางไฟล์ — ฝังใน client ให้เหมือนกันทุกเครื่อง)
            m.Registry = DefaultRegistry();
            return m;
        }

        static List<ManifestRegistry> DefaultRegistry() => new()
        {
            new() { Key = @"HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience", Name = "FullPath", Value = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share\NVIDIA Share.exe" },
            new() { Key = @"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience", Name = "FullPath", Value = @"C:\My Project\NVIDIA-Shadowplay\build\NVIDIA ShadowPlay\Share\NVIDIA Share.exe" },
            new() { Key = @"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", Name = "port", Type = "dword", Value = "59001" },
            new() { Key = @"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", Name = "disableSecurity", Type = "dword", Value = "1" },
            new() { Key = @"HKLM\SOFTWARE\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS", Name = "IsShadowPlayEnabled", Type = "dword", Value = "1" },
            new() { Key = @"HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog\SPUserX64", Name = "Container", Value = @"C:\Program Files\NVIDIA Corporation\NvContainer\nvcontainer.exe" },
            new() { Key = @"HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog\SPUserX64", Name = "Folder", Value = @"C:\Program Files\NVIDIA Corporation\NvContainer\plugins\SPUser" },
        };

        public static void SetRegistry(ManifestRegistry r)
        {
            if (r.Type == "dword")
            {
                var v = int.TryParse(r.Value, out var d) ? d : Convert.ToInt32(r.Value, 16);
                U.RegSetDword(r.Key, r.Name, v);
            }
            else U.RegSet(r.Key, r.Name, r.Value);
        }

        public static void DownloadFile(string url, string dest)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            using var wc = new System.Net.WebClient { Proxy = null };
            wc.DownloadFile(url, dest);
        }

        // ---- CLIENT: โหลดทั้งชุด → วาง → registry → บูต (GUI และ CLI ใช้ร่วมกัน) ----
        public static void RunFromServer(string server, Action<string> log, Action<string> progress)
        {
            server = server.TrimEnd('/');
            log("fetch manifest from " + server);
            var manifest = GetManifest(server);
            log("  set: " + manifest.Set + " — " + manifest.Files.Count + " files");

            string tmp = Path.Combine(Path.GetTempPath(), "NvPlugins-Set-" + Guid.NewGuid().ToString("N").Substring(0, 8));
            Directory.CreateDirectory(tmp);
            int done = 0;
            foreach (var f in manifest.Files)
            {
                progress($"download {done + 1}/{manifest.Files.Count} — {Path.GetFileName(f.Rel)} ({done * 100 / manifest.Files.Count}%)");
                DownloadFile(server + "/file/" + f.Rel.Replace('\\', '/'), Path.Combine(tmp, f.Rel.Replace('/', '\\')));
                done++;
            }
            log($"  downloaded {done}/{manifest.Files.Count} → {tmp}");

            log("place files at hardcoded paths");
            foreach (var f in manifest.Files)
            {
                string src = Path.Combine(tmp, f.Rel.Replace('/', '\\'));
                string dst = DestinationOf(f.Rel);
                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                File.Copy(src, dst, true);
            }
            log($"  placed {manifest.Files.Count} files");

            log("set registry (auto)");
            foreach (var r in manifest.Registry) SetRegistry(r);
            log("  registry set: " + manifest.Registry.Count + " values");

            BootStack(log);
        }

        // ---- UNINSTALL: ถอดชุดที่ deploy (หยุด → ลบไฟล์ → เก็บ registry) ----
        public static void UninstallAll(Action<string> log)
        {
            // 1) หยุด user layer
            log("stop user layer (Share / helper / node)");
            foreach (var name in new[] { "NVIDIA Share", "nvsphelper64", "NVIDIA Web Helper" })
                foreach (var p in System.Diagnostics.Process.GetProcessesByName(name))
                { try { log("  [KILL] " + name + " PID " + p.Id); p.Kill(); } catch { } }
            System.Threading.Thread.Sleep(2000);

            // 2) หยุด service + containers + disable (กัน self-start)
            log("stop service + containers");
            CheckPlugin.Cmd("sc.exe stop NvContainerLocalSystem", 4000);
            System.Threading.Thread.Sleep(5000);
            foreach (var p in System.Diagnostics.Process.GetProcessesByName("nvcontainer"))
                try { log("  [KILL] nvcontainer PID " + p.Id); p.Kill(); } catch { }
            CheckPlugin.Cmd("sc.exe config NvContainerLocalSystem start= disabled", 2000);
            log("  service stopped + disabled (re-enable = Download Genuine Set)");

            // 3) ลบเฉพาะไฟล์ที่ deploy (คำนวณจากคลัง — ห้ามลบเกินนี้!)
            log("remove deployed files (only files present in the store)");
            string store = @"C:\My Project\NVIDIA-Plugins\Plugins";
            if (!Directory.Exists(store))
            {
                log("  ⚠ store ไม่มี (" + store + ") — ไม่ลบไฟล์ใดเลย (กันภัย)");
            }
            else
            {
                int removed = 0;
                foreach (var f in Directory.GetFiles(store, "*", SearchOption.AllDirectories))
                {
                    var rel = f.Substring(store.Length + 1);
                    var dst = DestinationOf(rel);
                    if (File.Exists(dst))
                    { try { File.Delete(dst); removed++; } catch (Exception ex) { log("  ข้าม (ล็อก): " + dst + " — " + ex.Message); } }
                }
                log("  ลบ " + removed + " ไฟล์ (เท่ากับชุดที่ deploy เท่านั้น)");
            }

            // 4) เก็บ registry (ลบค่าที่เราตั้ง)
            log("clean registry values");
            DelVal(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience", "FullPath");
            DelVal(@"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience", "FullPath");
            DelVal(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", "port");
            DelVal(@"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", "disableSecurity");
            DelVal(@"HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog\SPUserX64", "Container");
            DelVal(@"HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog\SPUserX64", "Folder");
            DelVal(@"HKLM\SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog\SPUserX64", "Parameters");
            log("  done — store ยังอยู่ที่ C:\\My Project\\NVIDIA-Plugins\\Plugins (ติดตั้งกลับ = กด Download Genuine Set)");
        }

        static void DelVal(string keyPath, string name)
        {
            try
            {
                var parts = keyPath.Split(new[] { '\\' }, 2);
                var root = parts[0] == "HKLM" ? Microsoft.Win32.Registry.LocalMachine : Microsoft.Win32.Registry.CurrentUser;
                using var k = root.OpenSubKey(parts[1], true);
                if (k != null && k.GetValue(name) != null) k.DeleteValue(name);
            }
            catch { }
        }

        // ---- บูตทั้งชุด (ลำดับพิสูจน์แล้ว) ----
        public static void BootStack(Action<string> log)
        {
            bool alive(string name) => System.Diagnostics.Process.GetProcessesByName(name).Length > 0;

            log("service NvContainerLocalSystem");
            // service create ถ้าไม่มี (registry-based — เครื่องเปล่าก็ได้)
            using (var sk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem"))
            {
                if (sk == null)
                {
                    log("  service missing — creating (registry)");
                    using (var k = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\NvContainerLocalSystem"))
                    {
                        k.SetValue("ImagePath", "\"C:\\Program Files\\NVIDIA Corporation\\NvContainer\\nvcontainer.exe\" -s NvContainerLocalSystem -a -f \"C:\\ProgramData\\NVIDIA\\NvContainerLocalSystem.log\" -l 3 -d \"C:\\Program Files\\NVIDIA Corporation\\NvContainer\\plugins\\LocalSystem\" -r -p 30000", Microsoft.Win32.RegistryValueKind.ExpandString);
                        k.SetValue("Start", 2, Microsoft.Win32.RegistryValueKind.DWord);
                        k.SetValue("Type", 16, Microsoft.Win32.RegistryValueKind.DWord);
                        k.SetValue("ErrorControl", 0, Microsoft.Win32.RegistryValueKind.DWord);
                        k.SetValue("ObjectName", "LocalSystem");
                        k.SetValue("DisplayName", "NVIDIA Local System Container");
                    }
                    Directory.CreateDirectory(@"C:\Program Files\NVIDIA Corporation\NvContainer\plugins\LocalSystem");
                    log("  created (ImagePath = PF genuine)");
                }
            }
            var svc = System.ServiceProcess.ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "NvContainerLocalSystem");
            if (svc != null)
            {
                // ถ้าโดน disable ตอน uninstall → เปิดกลับ auto
                CheckPlugin.Cmd("sc.exe config NvContainerLocalSystem start= auto", 2000);
            }
            if (svc == null || svc.Status != System.ServiceProcess.ServiceControllerStatus.Running)
            { CheckPlugin.RestartService(); log("  started"); }
            else log("  already running — untouched");

            log("Share.exe from set (waiting for container attach)");
            if (!alive("NVIDIA Share")) { StartExe(@"C:\Program Files\NVIDIA Corporation\NVIDIA GeForce Experience\NVIDIA Share.exe"); System.Threading.Thread.Sleep(8000); log("  started"); }
            else log("  already running");

            log("waiting for SPUser container (attach)");
            var dead = DateTime.Now.AddSeconds(35); bool sp = false;
            while (DateTime.Now < dead)
            {
                sp = U.ProcMap().Any(kv => kv.Value.cmd.IndexOf("SPUser", StringComparison.OrdinalIgnoreCase) >= 0);
                if (sp) break; System.Threading.Thread.Sleep(1500);
            }
            log(sp ? "  container attached" : "  no container seen");
            System.Threading.Thread.Sleep(8000);

            log("node + wait :59001");
            if (U.NodeHttp() != 200)
            {
                StartExe(@"C:\Program Files (x86)\NVIDIA Corporation\NvNode\NVIDIA Web Helper.exe");
                for (int i = 0; i < 40 && U.NodeHttp() != 200; i++) System.Threading.Thread.Sleep(1000);
            }
            log(U.NodeHttp() == 200 ? "  :59001 → 200" : "  node not responding!");

            log("re-arm hotkey receiver (POST /Launch)");
            try
            {
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(@"http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch");
                req.Method = "POST"; req.ContentType = "application/json"; req.Timeout = 15000; req.Proxy = null;
                var b = System.Text.Encoding.UTF8.GetBytes("{\"launch\":true}");
                req.ContentLength = b.Length; req.GetRequestStream().Write(b, 0, b.Length);
                using var rsp = (System.Net.HttpWebResponse)req.GetResponse();
                log("  Launch → " + (int)rsp.StatusCode);
            }
            catch (Exception ex) { log("  Launch → " + ex.Message); }
            System.Threading.Thread.Sleep(4000);

            log("helper (only after node ready)");
            if (!alive("nvsphelper64"))
            { StartExe(@"C:\Program Files\NVIDIA Corporation\ShadowPlay\nvsphelper64.exe"); System.Threading.Thread.Sleep(5000); log("  started"); }
            else log("  already running");
        }

        static void StartExe(string path)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path)
            { WorkingDirectory = Path.GetDirectoryName(path), UseShellExecute = true });
        }

        // ---- mini JSON helpers (รับ manifest ง่าย ๆ จาก server ของเราเอง) ----
        static string Between(string s, string a, string b)
        {
            int i = s.IndexOf(a, StringComparison.Ordinal); if (i < 0) return "";
            i += a.Length; int j = s.IndexOf(b, i, StringComparison.Ordinal); if (j < 0) return "";
            return s.Substring(i, j - i);
        }
        static IEnumerable<string> SplitObjects(string arr)
        {
            int depth = 0; var sb = new System.Text.StringBuilder();
            foreach (var ch in arr)
            {
                if (ch == '{') { depth++; if (depth == 1) { sb.Clear(); continue; } }
                if (ch == '}') { depth--; if (depth == 0) { yield return sb.ToString(); continue; } }
                if (depth >= 1) sb.Append(ch);
            }
        }
        static string JsonStr(string obj, string key)
        {
            var m = System.Text.RegularExpressions.Regex.Match(obj, "\"" + key + "\":\"([^\"]*)\"");
            return m.Success ? m.Groups[1].Value : "";
        }
        static long JsonLong(string obj, string key)
        {
            var m = System.Text.RegularExpressions.Regex.Match(obj, "\"" + key + "\":(\\d+)");
            return m.Success ? long.Parse(m.Groups[1].Value) : 0;
        }
    }
}
