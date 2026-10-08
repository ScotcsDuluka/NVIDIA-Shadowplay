// Program.cs — เข้าโปรแกรม: ไม่มี args = GUI · "check" = headless รายงาน · "fix" = headless ซ่อม
using System;
using System.Linq;
using System.Windows.Forms;

namespace NvPlugins
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            System.Net.ServicePointManager.Expect100Continue = true;
            var arg = args.FirstOrDefault()?.ToLowerInvariant() ?? "";

            if (arg == "serve")
            {
                var srv = new DownloadServer { Port = 15246, StoreDir = @"C:\My Project\NVIDIA-Plugins\Plugins" };
                srv.Log += m => Console.WriteLine("[server] " + m);
                srv.Start();
                Console.WriteLine("Download API on " + srv.LocalUrl + " — " + srv.FileCount + " files." + (Console.IsInputRedirected ? " (headless — runs until killed)" : " Press ENTER to stop."));
                if (Console.IsInputRedirected) System.Threading.Thread.Sleep(-1);
                else Console.ReadLine();
                srv.Stop();
                return 0;
            }

            if (arg == "deploy")
            {
                string server = args.Length > 1 ? args[1] : "http://127.0.0.1:15246";
                if (!U.IsAdmin()) { Console.WriteLine("[!] deploy requires admin (writes System32 / Program Files)"); return 3; }
                Console.WriteLine("=== Download Genuine Set from " + server + " ===");
                Deploy.RunFromServer(server, m => Console.WriteLine("[deploy] " + m), m => Console.WriteLine("[   " + m + "   ]"));
                Console.WriteLine("=== done ===");
                return 0;
            }

            if (arg == "uninstall")
            {
                if (!U.IsAdmin()) { Console.WriteLine("[!] uninstall ต้อง admin"); return 3; }
                Console.WriteLine("ถอดชุดที่ deploy (หยุด stack + ลบไฟล์ + เก็บ registry)...");
                Deploy.UninstallAll(m => Console.WriteLine(m));
                Console.WriteLine("=== uninstalled — ติดตั้งกลับ = NvPlugins.exe → Download Genuine Set ===");
                return 0;
            }

            if (arg == "bootgenuine")
            {
                if (!U.IsAdmin()) { Console.WriteLine("[!] bootgenuine ต้อง admin (แตะ service / Program Files)"); return 3; }
                Console.WriteLine("=== BOOT GENUINE (NvPlugins Supervisor — §21) ===");
                var rc = Supervisor.Boot(m => Console.WriteLine("[boot] " + m));
                Console.WriteLine(rc == 0 ? "=== genuine mode ready — Alt+X เปิด OSC ===" : "=== boot ไม่ผ่านครบ — ดูรายงานด้านบน ===");
                return rc;
            }

            if (arg == "boot")   // alias หลักของ Phase 2
            {
                if (!U.IsAdmin()) { Console.WriteLine("[!] boot ต้อง admin (แตะ service / Program Files / kill process)"); return 3; }
                var rc = Supervisor.Boot(m => Console.WriteLine(m));
                return rc;
            }

            if (arg == "status") // §21 transparent — verify-only ทุก step ไม่แตะอะไร
            {
                return Supervisor.Status(m => Console.WriteLine(m));
            }

            if (arg == "check" || arg == "fix")
            {
                if (!U.IsAdmin()) { Console.WriteLine("[!] ไม่ใช่ admin — ตรวจได้บางหมวด แต่ FIX ต้อง admin"); }
                var cp = new CheckPlugin();
                cp.RunAll();
                foreach (var g in cp.Items.GroupBy(i => i.Category))
                {
                    Console.WriteLine("== " + g.Key + " ==");
                    foreach (var i in g)
                        Console.WriteLine($"  [{(i.Status == St.OK ? "✓" : i.Status == St.FAIL ? "✗" : "▲")}] {i.Name}  |  {i.Actual}");
                }
                int fail = cp.Items.Count(i => i.Status == St.FAIL);
                Console.WriteLine($"\nสรุป: OK {cp.Items.Count(i => i.Status == St.OK)} · FAIL {fail} · WARN {cp.Items.Count(i => i.Status == St.WARN)}");
                if (arg == "fix")
                {
                    if (!U.IsAdmin()) { Console.WriteLine("[!] FIX ต้อง admin — คลิกขวา run as administrator"); return 3; }
                    int n = cp.FixAll(m => Console.WriteLine(m));
                    Console.WriteLine("ซ่อมสำเร็จ " + n + " รายการ — ตรวจซ้ำ:");
                    cp.RunAll();
                    Console.WriteLine($"FAIL คงเหลือ: {cp.Items.Count(i => i.Status == St.FAIL)}");
                    return 0;
                }
                return fail == 0 ? 0 : 1;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }
    }
}
