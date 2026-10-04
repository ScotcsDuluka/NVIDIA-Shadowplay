// Supervisor.cs — Phase 2 (§21): รวมระบบบูตทั้งหมดเข้า NvPlugins ตัวเดียว
// สเปก OWNER 3 ข้อ: Modular (step เดี่ยว idempotent ตั้งชื่อชัด) · Transparent (status/log/manifest เปิดได้เสมอ)
//                   · Trustworthy (self-heal ทุกบูต + rollback journal + verify ไม่ผ่าน = หยุดรายงาน ไม่เดา)
// ลำดับ step = port ตรงจาก start-osc.ps1 โหมด genuine (พิสูจน์แล้ว §20.14) — ห้ามสองใจกับลำดับที่ผ่าน
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace NvPlugins
{
    public class StepResult
    {
        public string Name = "";
        public St Status = St.WARN;
        public string Detail = "";
        public List<string> Actions = new();
        public override string ToString() => Name;
    }

    public class Step
    {
        public string Name = "";
        public bool Fatal = true;
        public string Rollback = "ไม่มีการเปลี่ยนแปลงถาวร (verify-only)";
        public Func<string, string> Verify;          // return null = OK, else รายละเอียดความไม่พร้อม
        public Action<string, Action<string>> Fix;   // (detail, log) — idempotent, log ทุก action; null = verify-only step
        public Action OnOk = null;                   // ทำงานเมื่อ verify ผ่าน (เช่น รีเซ็ต guard) — ยัง idempotent
    }

    public static class Supervisor
    {
        // ---- paths (จาก start-osc genuine mode) ----
        public static readonly string Payload = @"C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\Docs ShadowPlay - Real\OscProvision\Payload";
        public static readonly string GfeExtract = @"C:\My Project\GFE\GeForce_Experience_v3.28.0.412";
        public static readonly string Build = P.BUILD;
        public static readonly string BuildLogs = Path.Combine(Build, "Logs");
        public static readonly string NodeExe = Build + @"\NvNode\NVIDIA Web Helper.exe";
        public static readonly string NodeWd = Build + @"\NvNode";
        public static readonly string HelperExe = Build + @"\ShadowPlay\nvsphelper64.exe";
        public static readonly string ShareExe = Build + @"\Overlay OSC\NVIDIA Share\NVIDIA Share.exe";
        public static readonly string ShareWd = Path.GetDirectoryName(ShareExe);
        public static readonly string Pf64 = P.PF;                       // C:\Program Files\NVIDIA Corporation
        public static readonly string Pf86 = @"C:\Program Files (x86)\NVIDIA Corporation";
        public static readonly string GfeDir = P.GFE;                    // PF\NVIDIA GeForce Experience
        public static readonly string PfContainer = Pf64 + @"\NvContainer";
        public static readonly string PfContainerPlugin = PfContainer + @"\plugins\LocalSystem\ShadowPlay\_nvspserviceplugin64.dll";
        public static readonly string PayloadPlugin = Payload + @"\NvContainer\plugins\LocalSystem\ShadowPlay\_nvspserviceplugin64.dll";
        public static readonly string Pf86Node = Pf86 + @"\NvNode";
        public static readonly string CaptureEngineExe = Build + @"\NvContainer\CaptureEngine\NvCapture.exe";
        public static readonly string GuardFile = BuildLogs + @"\launch-fail-count.txt";
        public static readonly string Journal = BuildLogs + @"\supervisor-journal.log";
        public static readonly string LaunchUrl = @"http://127.0.0.1:59001/ShadowPlay/v.1.0/Launch";
        public static readonly string HotkeyUrl = @"http://127.0.0.1:59001/ShadowPlay/v.1.0/Hotkey/openshare";
        const string PatchMarker = "[NvCapture-interim \u00A720.2]";

        static Action<string> _log = _ => { };
        static StepResult _cur;
        static bool _jsPatched;   // [0h] เพิ่งแทรก middleware → [3] ต้อง restart node ให้โหลดใหม่

        static void L(string m) { _log(m); if (_cur != null) _cur.Actions.Add(m); }
        static void J(string action, string revert)
        {
            try
            {
                Directory.CreateDirectory(BuildLogs);
                File.AppendAllText(Journal, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action}\n    rollback: {revert}\n");
            }
            catch { }
        }

        // ================= STEP DEFINITIONS =================
        public static List<Step> Steps() => new()
        {
            new Step {
                Name = "[0] stage build tree (node/helper/Share แท้จาก Payload)",
                Rollback = "ไฟล์ทั้งหมด restore ได้จาก Payload (revert = copy ทับจาก Payload)",
                Verify = _ => {
                    if (!U.FileOk(NodeExe, 20_000_000)) return "node ไม่ใช่ตัวแท้ (<20MB หรือไม่มี): " + NodeExe;
                    if (!File.Exists(HelperExe)) return "ไม่มี helper: " + HelperExe;
                    if (!File.Exists(ShareExe)) return "ไม่มี Share แท้ใน build: " + ShareExe;
                    try {
                        var js = File.ReadAllText(ShareWd + @"\NVIDIA Share.json");
                        if (js.Contains("nv-osc=false")) return "Share.json มี nv-osc=false — ห้ามบูต (แก้เป็น true ด้วยมือก่อน — สคริปต์/โค้ดห้ามแก้ค่าแทน)";
                    } catch { return "อ่าน NVIDIA Share.json ไม่ได้"; }
                    return null;
                },
                Fix = (d, log) => {
                    Directory.CreateDirectory(NodeWd);
                    CopyTree(Payload + @"\NvNode", NodeWd);
                    // MessageBus.dll ใน payload node tree = x64 ปนมา — บังคับ x86 (บทเรียน bridge fail 193)
                    var mb86 = Payload + @"\NvContainerX86Dlls\MessageBus.dll";
                    if (File.Exists(mb86)) { File.Copy(mb86, NodeWd + @"\MessageBus.dll", true); log("[0] MessageBus.dll → x86 (จาก NvContainerX86Dlls)"); }
                    CopyTree(Payload + @"\ShadowPlay", Build + @"\ShadowPlay");
                    if (!File.Exists(ShareExe)) throw new Exception("stage Share: ไม่พบ " + ShareExe + " — แหล่ง build หาย ต้อง restore ก่อน");
                }
            },
            new Step {
                Name = "[0e] PF anchor (node tree / helper anchor / telemetry)",
                Rollback = "restore ได้จาก Payload + GFE extract · manifest: Logs\\pf-anchor-manifest.txt",
                Verify = _ => {
                    var miss = new List<string>();
                    if (!File.Exists(Pf86Node + @"\index.js") || !Directory.Exists(Pf86Node + @"\node_modules")) miss.Add("PF(x86)\\NvNode tree");
                    if (!File.Exists(Pf64 + @"\ShadowPlay\nvsphelper64.exe")) miss.Add("PF\\ShadowPlay\\nvsphelper64.exe");
                    if (!File.Exists(Pf86Node + @"\MessageBus.dll")) miss.Add("PF(x86)\\NvNode\\MessageBus.dll (x86)");
                    if (!Directory.Exists(Pf64 + @"\NvTelemetry")) miss.Add("PF\\NvTelemetry");
                    if (!File.Exists(Pf64 + @"\NvDriverUpdateCheck\NvDriverUpdateCheck64.dll")) miss.Add("PF\\NvDriverUpdateCheck64");
                    return miss.Count == 0 ? null : "ขาด: " + string.Join(", ", miss);
                },
                Fix = (d, log) => {
                    if (!File.Exists(Pf86Node + @"\index.js") || !Directory.Exists(Pf86Node + @"\node_modules")) {
                        CopyTree(NodeWd, Pf86Node); log("[0e] restore PF(x86)\\NvNode tree");
                    }
                    var mb86 = Payload + @"\NvContainerX86Dlls\MessageBus.dll";
                    if (File.Exists(mb86)) { File.Copy(mb86, Pf86Node + @"\MessageBus.dll", true); log("[0e] PF(x86) MessageBus.dll → x86"); }
                    if (!File.Exists(Pf64 + @"\ShadowPlay\nvsphelper64.exe")) { CopyTree(Payload + @"\ShadowPlay", Pf64 + @"\ShadowPlay"); log("[0e] restore PF\\ShadowPlay tree"); }
                    if (!Directory.Exists(Pf64 + @"\NvTelemetry")) { CopyTree(GfeExtract + @"\NvTelemetry", Pf64 + @"\NvTelemetry"); log("[0e] restore PF\\NvTelemetry"); }
                    if (!Directory.Exists(Pf86 + @"\NvTelemetry")) { CopyTree(GfeExtract + @"\NvTelemetry", Pf86 + @"\NvTelemetry"); log("[0e] restore PF(x86)\\NvTelemetry"); }
                    if (!File.Exists(Pf64 + @"\NvDriverUpdateCheck\NvDriverUpdateCheck64.dll")) {
                        Directory.CreateDirectory(Pf64 + @"\NvDriverUpdateCheck");
                        File.Copy(GfeExtract + @"\NvBackend\NvDriverUpdateCheck64.dll", Pf64 + @"\NvDriverUpdateCheck\NvDriverUpdateCheck64.dll", true);
                        log("[0e] restore NvDriverUpdateCheck64");
                    }
                    WriteManifest(new[]{ Pf86Node, Pf64 + @"\ShadowPlay" }, BuildLogs + @"\pf-anchor-manifest.txt");
                }
            },
            new Step {
                Name = "[0g] runtime dirs",
                Rollback = "สร้าง dir เปล่า — ย้อน = ลบ dir (ไม่มีข้อมูล)",
                Fatal = false,
                Verify = _ => {
                    var miss = new[] {
                        @"C:\ProgramData\NVIDIA Corporation\NvNode",
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\NVIDIA Corporation\NVIDIA Share"
                    }.Where(d => !Directory.Exists(d)).ToList();
                    return miss.Count == 0 ? null : "ขาด: " + string.Join(", ", miss);
                },
                Fix = (d, log) => {
                    foreach (var dir in new[] {
                        @"C:\ProgramData\NVIDIA Corporation\NvNode",
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\NVIDIA Corporation\NVIDIA Share" }) {
                        if (!Directory.Exists(dir)) { Directory.CreateDirectory(dir); log("[0g] สร้าง " + dir); }
                    }
                }
            },
            new Step {
                Name = "[0f]/[0f.1] GFE hardlink + Share.json mirror",
                Rollback = "ลบ hardlink ใน GFE dir ได้ทันที (ไฟล์จริงอยู่ที่ build tree — hardlink = ไฟล์เดียวสองชื่อ)",
                Verify = _ => {
                    if (!File.Exists(GfeDir + @"\NVIDIA Share.exe")) return "GFE dir ไม่มี NVIDIA Share.exe (GetOSCPath จะตาย 0x80040233)";
                    if (!U.BytesEqual(ShareExe, GfeDir + @"\NVIDIA Share.exe")) return "GFE\\NVIDIA Share.exe ต่างจาก build";
                    var miss = new List<string>();
                    foreach (var f in Directory.EnumerateFiles(ShareWd, "*", SearchOption.AllDirectories)) {
                        var rel = f.Substring(ShareWd.Length + 1);
                        if (!File.Exists(Path.Combine(GfeDir, rel))) miss.Add(rel);
                    }
                    if (miss.Count > 0) return "GFE dir ขาด " + miss.Count + " ไฟล์ (เช่น " + miss.First() + ")";
                    try {
                        if (!U.BytesEqual(ShareWd + @"\NVIDIA Share.json", GfeDir + @"\NVIDIA Share.json")) return "Share.json ใน GFE dir ไม่ตรง build tree";
                    } catch { return "อ่าน Share.json (GFE) ไม่ได้"; }
                    return null;
                },
                Fix = (d, log) => {
                    int linked = 0;
                    foreach (var f in Directory.EnumerateFiles(ShareWd, "*", SearchOption.AllDirectories)) {
                        var rel = f.Substring(ShareWd.Length + 1);
                        var t = Path.Combine(GfeDir, rel);
                        if (File.Exists(t)) continue;
                        Directory.CreateDirectory(Path.GetDirectoryName(t));
                        HardLink.Create(t, f); linked++;
                        J("hardlink " + t, "File.Delete(\"" + t + "\") — ไฟล์จริงอยู่ที่ build tree");
                    }
                    log("[0f] hardlink เพิ่ม " + linked + " ไฟล์");
                    if (!File.Exists(GfeDir + @"\NVIDIA Share.exe") || !U.BytesEqual(ShareExe, GfeDir + @"\NVIDIA Share.exe")) {
                        if (File.Exists(GfeDir + @"\NVIDIA Share.exe")) File.Delete(GfeDir + @"\NVIDIA Share.exe");
                        HardLink.Create(GfeDir + @"\NVIDIA Share.exe", ShareExe);
                        log("[0f] hardlink NVIDIA Share.exe ใหม่ (ต่างจาก build)");
                    }
                    File.Copy(ShareWd + @"\NVIDIA Share.json", GfeDir + @"\NVIDIA Share.json", true);
                    log("[0f.1] Share.json force-mirror → GFE dir");
                }
            },
            new Step {
                Name = "[0h] registered JS boundary patch (ACTIVE — §20.2+§21.2 capture boundary → NvCapture.exe)",
                Rollback = "ลบบล็อก [NvCapture-boundary §21.2] และ [NvCapture-interim §20.2] แล้ว uncomment call เดิม (ทะเบียน: PATH-MANIFEST §20.2/§21.2)",
                Verify = _ => File.ReadAllText(Pf86Node + @"\NvShadowPlayAPI.js").Contains("[NvCapture-boundary \u00A721.2]")
                    ? null : "boundary middleware หาย (โดน restore ทับ) — NvShadowPlayAPI.js ที่ node แท้โหลด",
                Fix = (d, log) => {
                    var p = Pf86Node + @"\NvShadowPlayAPI.js";
                    var t = File.ReadAllText(p);
                    // 1) [0h] static patch ถ้ายังไม่มี (คงไว้เป็น fail-safe ชั้นสอง — middleware จะแย่งหน้าอยู่ดี)
                    if (!t.Contains(PatchMarker))
                    {
                        var a1 = "            api.CaptureState(doReply);";
                        var a2 = "            api.CaptureControlUnderPIDMode(doReply);";
                        if (!t.Contains(a1) || !t.Contains(a2)) throw new Exception("[0h] anchor ไม่ครบ — NvShadowPlayAPI.js เปลี่ยนรูป ตรวจก่อน (ทะเบียน: PATH-MANIFEST §20.2)");
                        var s = "\u00A7";
                        var r1 = "            /* " + PatchMarker + " capture boundary -> NvCapture.exe (/Duluka plane). INTERIM static reply - registered patch: PATH-MANIFEST " + s + "20.2 (OWNER decision B) */\r\n            doReply(undefined, { state: 'Ready' });\r\n            // api.CaptureState(doReply);";
                        var r2 = "            /* " + PatchMarker + " capture boundary - desktop truth: PID (co-proc) control not valid. Registered patch: PATH-MANIFEST " + s + "20.2 */\r\n            doReply(undefined, { valid: false });\r\n            // api.CaptureControlUnderPIDMode(doReply);";
                        t = t.Replace(a1, r1).Replace(a2, r2);
                        log("[0h] re-applied §20.2 static patch (ชั้น fail-safe)");
                    }
                    // 2) [NvCapture-boundary §21.2] middleware — แทรกก่อน route ทั้งหมด (express match ตามลำดับ = override)
                    var anchor = "function RegisterExpressEndpoints(app, io, logger) {";
                    if (!t.Contains(anchor)) throw new Exception("[0h] ไม่พบ anchor RegisterExpressEndpoints — JS เปลี่ยนรูป ตรวจก่อน");
                    var marker = "[NvCapture-boundary \u00A721.2]";
                    if (!t.Contains(marker))
                    {
                        var insert = anchor + "\r\n    // ===== " + marker + " Phase 3 capture boundary: capture-scope routes → NvCapture.exe 127.0.0.1:59077 =====\r\n" +
"    // กฎเหล็ก: หน้าต้องได้ 200 ทุกคำถาม — engine down = ตอบ static shape จริง ห้าม 500 · non-capture = genuine ทั้งหมด\r\n" +
"    (function () {\r\n" +
"        try {\r\n" +
"            var httpMod = require('http');\r\n" +
"            function ncReq(m, p2, body, cb) {\r\n" +
"                try {\r\n" +
"                    var rq = httpMod.request({ host: '127.0.0.1', port: 59077, path: p2, method: m, timeout: 1500 }, function (rs) {\r\n" +
"                        var d2 = ''; rs.on('data', function (c) { d2 += c; }); rs.on('end', function () { try { cb(null, JSON.parse(d2)); } catch (e) { cb(null, {}); } });\r\n" +
"                    });\r\n" +
"                    rq.on('timeout', function () { rq.destroy(); cb(new Error('timeout')); });\r\n" +
"                    rq.on('error', function (e) { cb(e); });\r\n" +
"                    if (body) { var bs = JSON.stringify(body); rq.setHeader('content-type', 'application/json'); rq.setHeader('content-length', Buffer.byteLength(bs)); rq.write(bs); }\r\n" +
"                    rq.end();\r\n" +
"                } catch (e2) { cb(e2); }\r\n" +
"            }\r\n" +
"            function reply(res, obj) { try { res.writeHead(200, { 'Content-Type': 'application/json' }); res.end(JSON.stringify(obj)); } catch (e) { try { res.end(); } catch (e2) {} } }\r\n" +
"            function readBody(req, cb) { var d = ''; req.on('data', function (c) { d += c; }); req.on('end', function () { try { cb(JSON.parse(d || '{}')); } catch (e) { cb({}); } }); }\r\n" +
"            var SCOPE = [\r\n" +
"                '/ShadowPlay/v.1.0/Record/Enable', '/ShadowPlay/v.1.0/Record/Running',\r\n" +
"                '/ShadowPlay/v.1.0/Record/Settings', '/ShadowPlay/v.1.0/Capture/State',\r\n" +
"                '/ShadowPlay/v.1.0/Broadcast/Support', '/ShadowPlay/v.1.0/InstantReplay/Enable'\r\n" +
"            ];\r\n" +
"            app.use(function (req, res, next) {\r\n" +
"                var u = (req.url || '').split('?')[0];\r\n" +
"                if (SCOPE.indexOf(u) < 0) return next();\r\n" +
"                if (u === '/ShadowPlay/v.1.0/Capture/State') {\r\n" +
"                    return ncReq('GET', '/state', null, function (e, st) { reply(res, { state: 'Ready' }); });\r\n" +
"                }\r\n" +
"                if (u === '/ShadowPlay/v.1.0/Record/Enable') {\r\n" +
"                    if (req.method === 'POST') return readBody(req, function (b) {\r\n" +
"                        var on = b && b.status === true;\r\n" +
"                        ncReq('POST', on ? '/record/start' : '/record/stop', {}, function (e, r) {\r\n" +
"                            try { _logger.info('[NvCapture-boundary] Enable ' + on + ' -> ' + JSON.stringify(r)); } catch (e2) {}\r\n" +
"                            try { res.writeHead(200); res.end(); } catch (e2) {}\r\n" +
"                        });\r\n" +
"                    });\r\n" +
"                    return ncReq('GET', '/state', null, function (e, st) { reply(res, { status: !!(st && st.enabled) }); });\r\n" +
"                }\r\n" +
"                if (u === '/ShadowPlay/v.1.0/Record/Running') {\r\n" +
"                    return ncReq('GET', '/state', null, function (e, st) { reply(res, { running: !!(st && st.recording) }); });\r\n" +
"                }\r\n" +
"                if (u === '/ShadowPlay/v.1.0/Record/Settings') {\r\n" +
"                    if (req.method === 'POST') return readBody(req, function (b) {\r\n" +
"                        ncReq('POST', '/settings', b, function (e, r) { reply(res, r || {}); });\r\n" +
"                    });\r\n" +
"                    return ncReq('GET', '/settings', null, function (e, r) { reply(res, r || { quality: 'Custom', resolution: 'In-game', framerate: 60, bitrateBps: 50000000 }); });\r\n" +
"                }\r\n" +
"                if (u === '/ShadowPlay/v.1.0/Broadcast/Support') { return reply(res, { support: false }); }\r\n" +
"                if (u === '/ShadowPlay/v.1.0/InstantReplay/Enable') {\r\n" +
"                    if (req.method === 'POST') { try { res.writeHead(200); res.end(); } catch (e) {} return; }\r\n" +
"                    return reply(res, { status: false });\r\n" +
"                }\r\n" +
"                return next();\r\n" +
"            });\r\n" +
"        } catch (eBoundary) {}\r\n" +
"    })();\r\n";
                        t = t.Replace(anchor, insert);
                        File.WriteAllText(p, t);
                        _jsPatched = true;
                        J("[0h→§21.2] boundary middleware inserted → " + p, "ลบบล็อก [NvCapture-boundary §21.2] จาก NvShadowPlayAPI.js (node restart หลังแก้)");
                        log("[0h] boundary middleware v2 inserted (record scope → NvCapture.exe :59077) — node จะถูก restart ที่ [3]");
                        return;
                    }
                    log("[0h] boundary middleware อยู่แล้ว");
                }
            },
            new Step {
                Name = "[0i] PF container anchor + plugin genuine invariant (§20.9)",
                Rollback = "ทั้ง tree restore จาก Payload (revert = Copy-Genuine-Tree จาก Payload ทับ)",
                Verify = _ => {
                    if (!File.Exists(PfContainer + @"\nvcontainer.exe")) return "PF\\NvContainer\\nvcontainer.exe ไม่มี";
                    if (!File.Exists(PfContainer + @"\NvContainerTelemetryApi.dll")) return "NvContainerTelemetryApi.dll ไม่มี";
                    if (!File.Exists(PfContainerPlugin)) return "plugin _nvspserviceplugin64.dll ไม่มีที่ PF";
                    if (!U.BytesEqual(PfContainerPlugin, PayloadPlugin)) return "plugin ที่ PF ไม่เท่า Payload — ห้าม! (patched = Secure loader ปฏิเสธ → ServicePlugin หาย → enable 0x800705b4) §20.10";
                    return null;
                },
                Fix = (d, log) => {
                    CopyTree(Payload + @"\NvContainer", PfContainer);
                    RemoveExtraTree(Payload + @"\NvContainer", PfContainer, log);
                    WriteManifest(new[]{ PfContainer }, BuildLogs + @"\pf-container-manifest.txt");
                    log("[0i] PF container anchor restore จาก Payload (mirror-clean แล้ว)");
                    if (!U.BytesEqual(PfContainerPlugin, PayloadPlugin))
                        throw new Exception("[0i] plugin ที่ PF ยังไม่เท่า Payload หลัง restore — หยุด (อย่า patch ไฟล์นี้: signature-verify)");
                }
            },
            new Step {
                Name = "[0j] ValidatePID patcher (REGISTERED-DORMANT — §20.9/§20.10)",
                Rollback = "verify-only — ไม่มีการเปลี่ยนแปลง",
                Fatal = false,
                Verify = _ => File.Exists(Build + @"\Runtime\apply-validatepid-patch.ps1")
                    ? null : "patcher หาย: Runtime\\apply-validatepid-patch.ps1",
                Fix = null   // ห้าม patch ใน boot — ต้องเป็นการตัดสินใจ OWNER เท่านั้น (option A ยังปิด)
            },
            new Step {
                Name = "[1b] registry (NvNode/GFExperience/NVSPCAPS — 2 views)",
                Rollback = "ค่าเก่าถูกจดใน journal ก่อนเขียนทุกครั้ง (revert = set ค่าเดิมกลับ)",
                Verify = _ => {
                    var bad = new List<string>();
                    void Chk(string k, string n, string want) {
                        var got = U.RegGet(k, n);
                        if (!string.Equals(got, want, StringComparison.OrdinalIgnoreCase)) bad.Add(k.Split('\\').Last() + "\\" + n + " = " + (got ?? "(null)") + " (ต้องการ " + want + ")");
                    }
                    void ChkD(string k, string n, int want) {
                        var got = U.RegGetDword(k, n, int.MinValue);
                        if (got != want) bad.Add(k.Split('\\').Last() + "\\" + n + " = " + got + " (ต้องการ " + want + ")");
                    }
                    foreach (var (k, view) in new[] {
                        (@"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", "64"),
                        (@"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\NvNode", "86") }) {
                        ChkD(k, "port", 59001); ChkD(k, "disableSecurity", 1);
                    }
                    foreach (var k in new[] { @"HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience", @"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience" }) {
                        Chk(k, "FullPath", ShareExe);
                        Chk(k, "Version", "3.28.0.412");
                        ChkD(k, "Installed", 1);
                    }
                    foreach (var k in new[] { @"HKLM\SOFTWARE\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS", @"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS" }) {
                        ChkD(k, "IsShadowPlayEnabled", 1);
                        ChkD(k, "IsShadowPlayEnabledUser", 1);
                    }
                    return bad.Count == 0 ? null : string.Join("; ", bad);
                },
                Fix = (d, log) => {
                    void Set(string k, string n, string v) {
                        var old = U.RegGet(k, n);
                        if (old == v) return;
                        J($"reg {k}\\{n}: {old} → {v}", $"set {k} [{n}] กลับเป็น \"{old}\"");
                        U.RegSet(k, n, v);
                    }
                    void SetD(string k, string n, int v) {
                        var old = U.RegGetDword(k, n, int.MinValue);
                        if (old == v) return;
                        J($"reg {k}\\{n}: {old} → {v}", $"set {k} [{n}] dword กลับเป็น {old}");
                        U.RegSetDword(k, n, v);
                    }
                    foreach (var k in new[] { @"HKLM\SOFTWARE\NVIDIA Corporation\Global\NvNode", @"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\NvNode" }) {
                        SetD(k, "port", 59001); SetD(k, "disableSecurity", 1);
                    }
                    foreach (var k in new[] { @"HKLM\SOFTWARE\NVIDIA Corporation\Global\GFExperience", @"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\GFExperience" }) {
                        Set(k, "FullPath", ShareExe); Set(k, "Version", "3.28.0.412"); SetD(k, "Installed", 1);
                    }
                    foreach (var k in new[] { @"HKLM\SOFTWARE\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS", @"HKLM\SOFTWARE\WOW6432Node\NVIDIA Corporation\Global\ShadowPlay\NVSPCAPS" }) {
                        SetD(k, "IsShadowPlayEnabled", 1); SetD(k, "IsShadowPlayEnabledUser", 1);
                    }
                    log("[1b] registry idempotent-set เสร็จ (เขียนเฉพาะค่าที่ต่าง)");
                }
            },
            new Step {
                Name = "[1d] container PF flip verify (ImagePath + Watchdog)",
                Rollback = "ค่าเก่าจดใน journal (revert = replace PF prefix กลับเป็น build prefix)",
                Verify = _ => {
                    var ip = U.RegGet(P.SVC, "ImagePath") ?? "";
                    var buildBase = Build + @"\NvContainer\genuine";
                    var pfBase = PfContainer;
                    if (ip.IndexOf(buildBase, StringComparison.OrdinalIgnoreCase) >= 0) return "ImagePath ยังชี้ build container — ต้อง flip → PF";
                    if (ip.IndexOf(pfBase, StringComparison.OrdinalIgnoreCase) < 0) return "ImagePath รูปแบบไม่รู้จัก: " + ip;
                    using var wd = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog");
                    if (wd == null) return "ไม่มี Watchdog key";
                    foreach (var pn in wd.GetSubKeyNames())
                        using (var k = wd.OpenSubKey(pn))
                            foreach (var n in new[] { "Folder", "Container", "Parameters" }) {
                                var v = k?.GetValue(n) as string;
                                if (v != null && v.IndexOf(buildBase, StringComparison.OrdinalIgnoreCase) >= 0) return $"Watchdog\\{pn}\\{n} ยังชี้ build";
                            }
                    return null;
                },
                Fix = (d, log) => {
                    var buildBase = Build + @"\NvContainer\genuine";
                    var pfBase = PfContainer;
                    var ip = U.RegGet(P.SVC, "ImagePath") ?? "";
                    if (ip.IndexOf(buildBase, StringComparison.OrdinalIgnoreCase) >= 0) {
                        J("ImagePath flip → PF", "set ImagePath กลับ: " + ip);
                        U.RegSetExpand(P.SVC, "ImagePath", ip.Replace(buildBase, pfBase));
                        log("[1d] ImagePath → PF");
                    }
                    using var wd = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NVIDIA Corporation\NvContainer\Watchdog", true);
                    if (wd != null)
                        foreach (var pn in wd.GetSubKeyNames())
                            using (var k = wd.OpenSubKey(pn, true))
                                foreach (var n in new[] { "Folder", "Container", "Parameters" }) {
                                    var v = k?.GetValue(n) as string;
                                    if (v != null && v.IndexOf(buildBase, StringComparison.OrdinalIgnoreCase) >= 0) {
                                        J($"Watchdog\\{pn}\\{n} flip → PF", "revert: " + v);
                                        k.SetValue(n, v.Replace(buildBase, pfBase));
                                        log($"[1d] Watchdog\\{pn}\\{n} → PF");
                                    }
                                }
                }
            },
            new Step {
                Name = "[1]/[2] service + containers ×3 จาก PF (healthy = ไม่แตะ)",
                Rollback = "sc start NvContainerLocalSystem (watchdog ปลุกลูกเอง)",
                Verify = _ => {
                    var st = ServiceState();
                    if (st != "Running") return "service = " + st;
                    var (spuser, user, pf) = ContainerCounts();
                    if (spuser < 1 || user < 1) return $"containers: SPUser={spuser} User={user} (รอ watchdog ปลุก)";
                    if (!pf) return "container รันจาก path อื่น (ไม่ใช่ PF)";
                    return null;
                },
                Fix = (d, log) => {
                    if (ServiceState() != "Running") {
                        CheckPlugin.Cmd("sc.exe start NvContainerLocalSystem", 8000);
                        Thread.Sleep(14000);
                        if (ServiceState() != "Running") {
                            log("[2] รอบแรกไม่ขึ้น — start รอบสอง (pattern ปกติของ container แท้)");
                            CheckPlugin.Cmd("sc.exe start NvContainerLocalSystem", 8000);
                            Thread.Sleep(14000);
                        }
                    }
                    for (int i = 0; i < 8; i++) {
                        var (spuser, user, pf) = ContainerCounts();
                        if (spuser >= 1 && user >= 1 && pf) return;
                        Thread.Sleep(3000);
                    }
                }
            },
            new Step {
                Name = "[3] node แท้ :59001 (ล่า node ร้าว + open-by-name MMF)",
                Rollback = "kill node ที่เรา spawn + start ใหม่ได้ทุกเมื่อ (exe = build\\NvNode)",
                Verify = _ => {
                    if (_jsPatched) return "JS boundary เพิ่งเปลี่ยน — node ต้อง restart เพื่อโหลด middleware";
                    if (U.NodeHttp() != 200) return ":59001 ไม่ตอบ 200";
                    var (rogue, unknown) = NodeHosts();
                    if (rogue.Count > 0) return "node ร้าว: " + string.Join(", ", rogue.Select(r => "PID " + r.Key + " " + r.Value.exe));
                    return unknown > 0 ? "OK — แต่มี node host path อ่านไม่ได้ " + unknown + " ตัว (ยืนยันชัด = รัน elevated)" : null;
                },
                Fix = (d, log) => {
                    if (_jsPatched) {
                        foreach (var (pid, path) in ProcList("NVIDIA Web Helper"))
                            if (path.Equals(NodeExe, StringComparison.OrdinalIgnoreCase)) {
                                try { Process.GetProcessById(pid).Kill(); log("[KILL] node PID " + pid + " (restart เพื่อโหลด boundary middleware)"); } catch { }
                            }
                        Thread.Sleep(2500);
                        _jsPatched = false;
                    }
                    var (rogue, _) = NodeHosts();
                    foreach (var r in rogue) {
                        try { Process.GetProcessById((int)r.Key).Kill(); log("[KILL] node ร้าว PID " + r.Key + " ← " + r.Value.exe); }
                        catch (Exception ex) { log("[3] kill ร้าวไม่สำเร็จ PID " + r.Key + ": " + ex.Message); }
                    }
                    Thread.Sleep(2000);
                    if (U.NodeHttp() == 200) { log("[3] :59001 ยังตอบจากตัวอื่น — หาเจ้าของ port จาก netstat"); KillPortOwner(59001, log); Thread.Sleep(1500); }
                    if (U.NodeHttp() != 200) {
                        log("[3] start node แท้: " + NodeExe);
                        Process.Start(new ProcessStartInfo(NodeExe) { WorkingDirectory = NodeWd, UseShellExecute = true });
                        for (int i = 0; i < 40 && U.NodeHttp() != 200; i++) Thread.Sleep(1000);
                        if (U.NodeHttp() != 200) {
                            // ClaimSingleInstance ตายจาก MMF ค้าง (§20.10) — เจ้าถือที่พบบ่อย = Share ค้าง/OscHotkey
                            log("[3] node ไม่ขึ้นใน 40 วิ — ลองครั้ง 2 หลังเคลียร์ผู้ถือ pairing MMF ที่พบบ่อย (Share ค้าง)");
                            foreach (var p in Process.GetProcessesByName("NVIDIA Share")) try { p.Kill(); } catch { }
                            Thread.Sleep(2000);
                            Process.Start(new ProcessStartInfo(NodeExe) { WorkingDirectory = NodeWd, UseShellExecute = true });
                            for (int i = 0; i < 20 && U.NodeHttp() != 200; i++) Thread.Sleep(1000);
                        }
                    }
                }
            },
            new Step {
                Name = "[4] helper (หลัง node 200 เท่านั้น — ล่า helper ผิด path)",
                Rollback = "kill + start ใหม่จาก build\\ShadowPlay ได้ทุกเมื่อ",
                Verify = _ => {
                    var ours = false; var rogue = new List<string>(); var unknown = 0;
                    foreach (var (pid, path) in ProcList("nvsphelper64")) {
                        if (path.Length == 0) { unknown++; continue; }
                        if (path.Equals(HelperExe, StringComparison.OrdinalIgnoreCase)) ours = true;
                        else rogue.Add("PID " + pid + " " + path);
                    }
                    if (rogue.Count > 0) return "helper ผิด path: " + string.Join(", ", rogue);
                    if (ours) return null;
                    return unknown > 0
                        ? "มี helper กำลังรัน " + unknown + " ตัว แต่อ่าน path ไม่ได้"
                        : "ไม่มี helper จาก build\\ShadowPlay";
                },
                Fix = (d, log) => {
                    foreach (var (pid, path) in ProcList("nvsphelper64"))
                        if (path.Length > 0 && !path.Equals(HelperExe, StringComparison.OrdinalIgnoreCase)) {
                            try { Process.GetProcessById(pid).Kill(); log("[KILL] helper ผิด path PID " + pid); } catch { }
                        }
                    Thread.Sleep(2000);
                    if (!HelperOurs()) {
                        Process.Start(new ProcessStartInfo(HelperExe) { WorkingDirectory = Path.GetDirectoryName(HelperExe), UseShellExecute = true });
                        Thread.Sleep(5000);
                    }
                }
            },
            new Step {
                Name = "[5] Share (attach — container จะ spawn คู่ของตัวเองตอน enable เสมอ)",
                Rollback = "kill + spawn ใหม่ (exe = build Overlay OSC\\NVIDIA Share)",
                Fatal = false,   // ตาม RE §20.11 — enable spawn Share เอง; ตัวนี้แค่ให้พร้อมเร็วขึ้น
                Verify = _ => Process.GetProcessesByName("NVIDIA Share").Length > 0
                    ? null : "ไม่มี Share มีชีวิต",
                Fix = (d, log) => {
                    foreach (var p in Process.GetProcessesByName("NVIDIA Share")) {
                        var exe = ""; try { exe = p.MainModule?.FileName ?? ""; } catch { }
                        log("[5] kill Share PID " + p.Id + " " + exe);
                        try { p.Kill(); } catch { }
                    }
                    Thread.Sleep(3000);
                    Process.Start(new ProcessStartInfo(ShareExe) { WorkingDirectory = ShareWd, UseShellExecute = true, WindowStyle = ProcessWindowStyle.Hidden });
                    Thread.Sleep(8000);
                }
            },
            new Step {
                Name = "[6] arm enable + guard รีเซ็ตเอง (m_pSettings gate)",
                Rollback = "POST /Launch {\"launch\":false} — guard file ลบได้",
                Verify = _ => LaunchState() ? null : "GET /Launch = false (server ยังไม่ enable)",
                OnOk = () => { try { if (File.ReadAllText(GuardFile).Trim() != "0") File.WriteAllText(GuardFile, "0"); } catch { try { File.WriteAllText(GuardFile, "0"); } catch { } } },
                Fix = (d, log) => {
                    var failCount = 0;
                    try { int.TryParse(File.ReadAllText(GuardFile).Trim(), out failCount); } catch { }
                    if (failCount >= 2) throw new Exception("[6] GUARD: Launch fail มาแล้ว " + failCount + " ครั้งติด — หยุด (ตรวจ CaptureCore m_pSettings/CreateSettings ก่อน — ลบ " + GuardFile + " เพื่อปลด)");
                    log("[6] POST /Launch {\"launch\":true}");
                    HttpPost(LaunchUrl, "{\"launch\":true}");
                    for (int i = 0; i < 25 && !LaunchState(); i++) Thread.Sleep(1000);
                    if (LaunchState()) {
                        if (failCount > 0) log("[6] สำเร็จ — รีเซ็ต guard");
                        try { File.WriteAllText(GuardFile, "0"); } catch { }
                    } else {
                        try { File.WriteAllText(GuardFile, (failCount + 1).ToString()); } catch { }
                        throw new Exception("[6] enable ไม่สำเร็จ (fail " + (failCount + 1) + "/2) — ดู CaptureCore: SetSP/ValidatePID/CreateSettings");
                    }
                }
            },
            new Step {
                Name = "[6c] hotkey openshare verify-only (ค่าจริงอยู่ที่ registry GFEOverlayHKeyV2)",
                Rollback = "verify-only — ห้าม POST ทับค่าที่ OWNER ตั้งผ่าน UI เด็ดขาด (§20.13)",
                Fatal = false,
                Verify = _ => {
                    var s = HttpGet(HotkeyUrl);
                    return s != null ? null : "อ่าน openshare ไม่ได้ (node/enabled ยังไม่พร้อม?)";
                },
                Fix = null   // log-only — ค่าจริงมาจาก registry ที่ server อ่านตอน init
            },
            new Step {
                Name = "[7] NvCapture.exe capture engine :59077 (เจ้าของการอัด — §21.2)",
                Rollback = "kill process ที่เรา spawn — engine ไม่เขียนอะไรนอกจาก config/ไฟล์อัดของตัวเอง",
                Fatal = false,   // engine ล่ม = boundary ตอบ fail-soft (200) — หน้ายังไม่ jam
                Verify = _ => {
                    var exe = CaptureEngineExe;
                    if (!File.Exists(exe)) return "ไม่มี engine build: " + exe;
                    foreach (var (pid, path) in ProcList("NvCapture"))
                        if (path.Equals(exe, StringComparison.OrdinalIgnoreCase) && HttpGetJson(59077, "/health") != null)
                            return null;
                    return "engine ไม่ได้รัน หรือ /health ไม่ตอบ (127.0.0.1:59077)";
                },
                Fix = (d, log) => {
                    foreach (var (pid, path) in ProcList("NvCapture"))
                        if (path.Length > 0 && !path.Equals(CaptureEngineExe, StringComparison.OrdinalIgnoreCase)) {
                            try { Process.GetProcessById(pid).Kill(); log("[KILL] NvCapture ผิด path PID " + pid); } catch { }
                        }
                    var dir = Path.GetDirectoryName(CaptureEngineExe);
                    Process.Start(new ProcessStartInfo(CaptureEngineExe) { WorkingDirectory = dir, UseShellExecute = true });
                    for (int i = 0; i < 20 && HttpGetJson(59077, "/health") == null; i++) Thread.Sleep(1000);
                }
            }
        };

        // ================= BOOT / STATUS =================
        static void LogFile(string m)
        {
            try
            {
                Directory.CreateDirectory(BuildLogs);
                File.AppendAllText(BuildLogs + @"\supervisor-boot.log", $"[{DateTime.Now:HH:mm:ss}] {m}\r\n");
            }
            catch { }
        }

        public static int Boot(Action<string> log)
        {
            var console = log;                       // กัน closure self-capture — lambda ต้องจับ original เสมอ
            _log = m => { console(m); LogFile(m); };
            log = _log;
            LogFile($"\n===== BOOT start {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
            var results = new List<StepResult>();
            foreach (var step in Steps())
            {
                _cur = new StepResult { Name = step.Name };
                log("=== " + step.Name + " ===");
                try
                {
                    var v = step.Verify?.Invoke(null);
                    if (v == null)
                    {
                        _cur.Status = St.OK; _cur.Detail = "พร้อมอยู่แล้ว";
                        step.OnOk?.Invoke();
                        log("  ✓ OK (idempotent — ข้าม fix)");
                    }
                    else
                    {
                        log("  ▲ " + v);
                        if (step.Fix == null)
                        {
                            _cur.Status = St.WARN; _cur.Detail = v + " — verify-only (ไม่ fix ใน boot)";
                            log("  ▲ verify-only — ไม่ fix");
                        }
                        else
                        {
                            step.Fix(v, L);
                            var v2 = step.Verify?.Invoke(null);
                            if (v2 == null) { _cur.Status = St.OK; _cur.Detail = "fix แล้ว verify ผ่าน"; log("  ✓ FIXED + verified"); }
                            else { _cur.Status = St.FAIL; _cur.Detail = "หลัง fix ยังไม่ผ่าน: " + v2; log("  ✗ FAIL หลัง fix: " + v2); }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _cur.Status = St.FAIL; _cur.Detail = ex.Message;
                    log("  ✗ EXCEPTION: " + ex.Message);
                }
                results.Add(_cur);
                if (_cur.Status == St.FAIL && step.Fatal)
                {
                    log("\n✗ หยุดที่ " + step.Name + " (fatal) — ไม่เดาต่อ แก้ตามรายงานแล้วรันใหม่");
                    PrintSummary(log, results);
                    _cur = null;
                    return 1;
                }
                _cur = null;
            }
            PrintSummary(log, results);
            return results.Any(r => r.Status == St.FAIL) ? 1 : 0;
        }

        public static int Status(Action<string> log)
        {
            var console = log;
            _log = m => { console(m); LogFile(m); };
            log = _log;
            LogFile($"\n===== STATUS {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
            var results = new List<StepResult>();
            foreach (var step in Steps())
            {
                _cur = new StepResult { Name = step.Name };
                try
                {
                    var v = step.Verify?.Invoke(null);
                    _cur.Status = v == null ? St.OK : (step.Fix == null ? St.WARN : St.FAIL);
                    _cur.Detail = v ?? "OK";
                }
                catch (Exception ex) { _cur.Status = St.FAIL; _cur.Detail = ex.Message; }
                results.Add(_cur);
                log($"[{(_cur.Status == St.OK ? "✓" : _cur.Status == St.FAIL ? "✗" : "▲")}] {step.Name}  |  {_cur.Detail}");
                _cur = null;
            }
            log("\n--- กระบวนการมีชีวิต ---");
            foreach (var (pid, path) in ProcList("nvcontainer", "NVIDIA Web Helper", "nvsphelper64", "NVIDIA Share"))
                log($"  PID {pid}  {(path.Length > 0 ? path : "(path อ่านไม่ได้)")}");
            var fail = results.Count(r => r.Status == St.FAIL);
            var warn = results.Count(r => r.Status == St.WARN);
            log($"\nสรุป: OK {results.Count - fail - warn} · FAIL {fail} · WARN {warn}" + (fail == 0 ? " — กด Alt+X เปิด overlay ได้" : " — รัน NvPlugins.exe boot เพื่อ self-heal"));
            return fail == 0 ? 0 : 1;
        }

        // ================= HELPERS =================
        static void PrintSummary(Action<string> log, List<StepResult> rs)
        {
            log("\n--- รายงานสรุป ---");
            foreach (var r in rs) log($"  [{(r.Status == St.OK ? "✓" : r.Status == St.FAIL ? "✗" : "▲")}] {r.Name}  |  {r.Detail}");
            var fail = rs.Count(r => r.Status == St.FAIL);
            log($"สรุป: OK {rs.Count(r => r.Status == St.OK)} · FAIL {fail} · WARN {rs.Count(r => r.Status == St.WARN)}");
            if (fail == 0) log("จบบูต — กด Alt+X เปิด Overlay");
        }

        static string ServiceState()
        {
            try { using var c = new System.ServiceProcess.ServiceController("NvContainerLocalSystem"); return c.Status.ToString(); }
            catch { return "NotFound"; }
        }

        static (int spuser, int user, bool pf) ContainerCounts()
        {
            int spuser = 0, user = 0; bool pf = true;
            foreach (var kv in U.ProcMap())
            {
                if (!kv.Value.exe.EndsWith("nvcontainer.exe", StringComparison.OrdinalIgnoreCase)) continue;
                var cmd = kv.Value.cmd ?? "";
                if (cmd.Contains("SPUser")) { spuser++; if (!kv.Value.exe.StartsWith(PfContainer, StringComparison.OrdinalIgnoreCase)) pf = false; }
                else if (cmd.Contains("plugins\\User") || cmd.Contains("plugins\\User\\")) { user++; if (!kv.Value.exe.StartsWith(PfContainer, StringComparison.OrdinalIgnoreCase)) pf = false; }
            }
            return (spuser, user, pf);
        }

        // ---- process path แบบ native (WMI อ่าน path ของ elevated process ไม่ได้/ข้ามแถว — §20.15) ----
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint access, bool inherit, int pid);
        [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
        static extern bool QueryFullProcessImageNameW(IntPtr h, uint flags, System.Text.StringBuilder sb, ref uint size);
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr h);
        const uint PROCESS_QUERY_LIMITED = 0x1000;

        public static string ProcPath(int pid)
        {
            var h = OpenProcess(PROCESS_QUERY_LIMITED, false, pid);
            if (h == IntPtr.Zero) return "";
            var sb = new System.Text.StringBuilder(1024); uint size = 1024;
            var ok = QueryFullProcessImageNameW(h, 0, sb, ref size);
            CloseHandle(h);
            return ok ? sb.ToString() : "";
        }

        // (pid, path) ของ process ตามชื่อ image — path อ่านได้แม้ process นั้น elevated
        public static List<(int pid, string path)> ProcList(params string[] imageNames)
        {
            var list = new List<(int, string)>();
            foreach (var name in imageNames)
                foreach (var p in Process.GetProcessesByName(name))
                {
                    var path = "";
                    try { path = ProcPath(p.Id); } catch { }
                    if (path.Length == 0) { try { path = p.MainModule?.FileName ?? ""; } catch { } }
                    list.Add((p.Id, path));
                }
            return list;
        }

        static (Dictionary<long, (string exe, string cmd)> rogue, int unknown) NodeHosts()
        {
            var rogue = new Dictionary<long, (string, string)>();
            int unknown = 0;
            foreach (var (pid, path) in ProcList("NVIDIA Web Helper", "node"))
            {
                var e = path.ToLowerInvariant();
                var isNodeHost = e.EndsWith("nvidia web helper.exe", StringComparison.OrdinalIgnoreCase) || e.EndsWith("node.exe", StringComparison.OrdinalIgnoreCase);
                if (path.Length == 0) { unknown++; continue; }   // อ่าน path ไม่ได้
                if (!isNodeHost) continue;
                if (e.EndsWith("node.exe", StringComparison.OrdinalIgnoreCase) && !path.StartsWith(Build, StringComparison.OrdinalIgnoreCase))
                { rogue[pid] = (path, ""); continue; }   // bare node.exe นอก build = ร้าว (shim tree เก่า)
                if (e.EndsWith("nvidia web helper.exe", StringComparison.OrdinalIgnoreCase) && !path.Equals(NodeExe, StringComparison.OrdinalIgnoreCase))
                    rogue[pid] = (path, "");
            }
            return (rogue, unknown);
        }

        static bool HelperOurs()
        {
            foreach (var kv in U.ProcMap())
                if (kv.Value.exe.Equals(HelperExe, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        static void KillPortOwner(int port, Action<string> log)
        {
            try
            {
                var psi = new ProcessStartInfo("netstat.exe", "-ano -p tcp") { UseShellExecute = false, RedirectStandardOutput = true, CreateNoWindow = true };
                using var p = Process.Start(psi);
                var outp = p.StandardOutput.ReadToEnd(); p.WaitForExit(3000);
                foreach (var line in outp.Split('\n'))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5 && parts[1].EndsWith(":" + port) && parts[3] == "LISTENING"
                        && long.TryParse(parts[4], out var pid) && pid != 0)
                    {
                        try { Process.GetProcessById((int)pid).Kill(); log("[KILL] เจ้าของ port " + port + " PID " + pid); } catch { }
                    }
                }
            }
            catch (Exception ex) { log("[3] netstat kill err: " + ex.Message); }
        }

        static bool LaunchState()
        {
            try
            {
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(LaunchUrl);
                req.Method = "GET"; req.Timeout = 3000; req.Proxy = null;
                using var rsp = (System.Net.HttpWebResponse)req.GetResponse();
                var body = new System.IO.StreamReader(rsp.GetResponseStream()).ReadToEnd();
                return body.Contains("\"launch\":true");
            }
            catch { return false; }
        }

        static string HttpGet(string url)
        {
            try
            {
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                req.Method = "GET"; req.Timeout = 3000; req.Proxy = null;
                using var rsp = (System.Net.HttpWebResponse)req.GetResponse();
                return new System.IO.StreamReader(rsp.GetResponseStream()).ReadToEnd();
            }
            catch (System.Net.WebException we) when (we.Response is System.Net.HttpWebResponse)
            { return ""; }   // got an HTTP error reply = server is alive
            catch { return null; }
        }

        static string HttpGetJson(int port, string path)   // null = ไม่ตอบ; "" = ตอบแต่ parse ไม่ได้
        {
            try
            {
                var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create($"http://127.0.0.1:{port}{path}");
                req.Method = "GET"; req.Timeout = 2000; req.Proxy = null;
                using var rsp = (System.Net.HttpWebResponse)req.GetResponse();
                return new System.IO.StreamReader(rsp.GetResponseStream()).ReadToEnd();
            }
            catch { return null; }
        }

        static void HttpPost(string url, string body)
        {
            var req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
            req.Method = "POST"; req.ContentType = "application/json"; req.Timeout = 8000; req.Proxy = null;
            var b = Encoding.UTF8.GetBytes(body);
            req.ContentLength = b.Length;
            using (var s = req.GetRequestStream()) s.Write(b, 0, b.Length);
            try { using var r = (System.Net.HttpWebResponse)req.GetResponse(); } catch { }
        }

        // CopyTree — ทีละไฟล์ (แบบ Copy-Genuine-Tree: leaf/dir ชนกันจัดการ, ข้ามไฟล์ล็อกเนื้อตรง)
        static void CopyTree(string src, string dst)
        {
            if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);
            foreach (var child in Directory.EnumerateFileSystemEntries(src))
            {
                var d = Path.Combine(dst, Path.GetFileName(child));
                try
                {
                    if (Directory.Exists(child))
                    {
                        if (File.Exists(d)) File.Delete(d);
                        CopyTree(child, d);
                    }
                    else
                    {
                        if (Directory.Exists(d)) Directory.Delete(d, true);
                        File.Copy(child, d, true);
                    }
                }
                catch (IOException) { }   // ไฟล์ล็อกโดย process ที่รันอยู่ — ข้าม (จะ verify อีกที)
                catch (UnauthorizedAccessException) { }
            }
        }

        // RemoveExtraTree — mirror-clean: ลบของแปลกใน dst ที่ไม่มีใน src (§20.5 NvMessageBusBroadcast trap)
        static void RemoveExtraTree(string src, string dst, Action<string> log)
        {
            if (!Directory.Exists(dst)) return;
            foreach (var child in Directory.EnumerateFileSystemEntries(dst))
            {
                var s = Path.Combine(src, Path.GetFileName(child));
                if (!File.Exists(s) && !Directory.Exists(s))
                {
                    try { Directory.Delete(child, true); log("  [0i] ลบของแปลก (ไม่มีใน Payload): " + child); } catch { }
                    J("ลบของแปลก " + child, "ไฟล์นี้ไม่มีใน Payload — ตรวจก่อนกู้คืน");
                }
                else if (Directory.Exists(child)) RemoveExtraTree(s, child, log);
            }
        }

        static void WriteManifest(string[] dirs, string outPath)
        {
            try
            {
                var sb = new StringBuilder();
                foreach (var d in dirs)
                    if (Directory.Exists(d))
                        foreach (var f in Directory.EnumerateFiles(d, "*", SearchOption.AllDirectories))
                            sb.Append(Md5(f)).Append("  ").Append(f).AppendLine();
                Directory.CreateDirectory(BuildLogs);
                File.WriteAllText(outPath, sb.ToString());
            }
            catch { }
        }

        static string Md5(string file)
        {
            using var md = System.Security.Cryptography.MD5.Create();
            using var fs = File.OpenRead(file);
            return Convert.ToHexString(md.ComputeHash(fs));
        }
    }
}
