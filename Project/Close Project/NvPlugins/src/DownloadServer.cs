// DownloadServer.cs — API ดาวน์โหลดชุด: GET / (ปุ่มเดียว + %bar) · GET /api/manifest · GET /file/<rel>
// TcpListener ล้วน — ไม่ต้อง URLACL/admin · ป้องกัน path traversal
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NvPlugins
{
    public class DownloadServer
    {
        TcpListener _listener;
        Thread _thread;
        volatile bool _running;

        public int Port = 15246;
        public string StoreDir = @"C:\My Project\NVIDIA-Plugins\Plugins";
        public bool Running => _running;
        public string LocalUrl => "http://" + LocalIp() + ":" + Port;
        public event Action<string> Log = _ => { };

        public int FileCount
        {
            get { try { return Directory.GetFiles(StoreDir, "*", SearchOption.AllDirectories).Length; } catch { return 0; } }
        }

        public static string LocalIp()
        {
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
        }

        public void Start()
        {
            if (_running) return;
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            _running = true;
            _thread = new Thread(AcceptLoop) { IsBackground = true };
            _thread.Start();
        }

        public void Stop()
        {
            _running = false;
            try { _listener?.Stop(); } catch { }
        }

        void AcceptLoop()
        {
            while (_running)
            {
                TcpClient client;
                try { client = _listener.AcceptTcpClient(); } catch { break; }
                new Thread(() => Handle(client)) { IsBackground = true }.Start();
            }
        }

        void Handle(TcpClient client)
        {
            try
            {
                using var _c = client;
                var stream = client.GetStream();
                var line = ReadLine(stream);
                if (line == null) return;
                var parts = line.Split(' ');
                if (parts.Length < 2) return;
                string method = parts[0], raw = parts[1];
                // กิน headers จนครบ
                while ((line = ReadLine(stream)) != null && line.Length > 0) { }

                string path = Uri.UnescapeDataString(raw.Split('?')[0]);
                if (method != "GET") { SendText(stream, 405, "text/plain", "method not allowed"); return; }

                if (path == "/" || path == "/index.html")
                {
                    SendText(stream, 200, "text/html; charset=utf-8", Page());
                }
                else if (path == "/api/manifest")
                {
                    SendText(stream, 200, "application/json", ManifestJson());
                }
                else if (path.StartsWith("/file/"))
                {
                    string rel = path.Substring("/file/".Length);
                    SendStoreFile(stream, rel);
                }
                else SendText(stream, 404, "text/plain", "not found");
                Log("served " + path + " ← " + client.Client.RemoteEndPoint);
            }
            catch { try { client.Close(); } catch { } }
        }

        string ReadLine(NetworkStream s)
        {
            var sb = new StringBuilder();
            int b; int guard = 0;
            while (guard++ < 8192 && (b = s.ReadByte()) >= 0)
            {
                if (b == '\n') { if (sb.Length > 0 && sb[sb.Length - 1] == '\r') sb.Length--; return sb.ToString(); }
                sb.Append((char)b);
            }
            return sb.Length > 0 ? sb.ToString() : null;
        }

        void SendText(NetworkStream s, int code, string type, string body)
        {
            var b = Encoding.UTF8.GetBytes(body);
            var h = Encoding.ASCII.GetBytes(
                "HTTP/1.1 " + code + " OK\r\nContent-Type: " + type + "\r\nContent-Length: " + b.Length +
                "\r\nConnection: close\r\n\r\n");
            s.Write(h, 0, h.Length); s.Write(b, 0, b.Length); s.Flush();
        }

        void SendStoreFile(NetworkStream s, string rel)
        {
            if (rel.Contains("..") || Path.IsPathRooted(rel) || rel.Contains(':'))
            { SendText(s, 400, "text/plain", "bad path"); return; }
            string full = Path.Combine(StoreDir, rel.Replace('/', '\\'));
            if (!File.Exists(full)) { SendText(s, 404, "text/plain", "not found: " + rel); return; }
            var bytes = File.ReadAllBytes(full);
            var h = Encoding.ASCII.GetBytes(
                "HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: " + bytes.Length +
                "\r\nContent-Disposition: attachment; filename=\"" + Path.GetFileName(full) + "\"\r\nConnection: close\r\n\r\n");
            s.Write(h, 0, h.Length); s.Write(bytes, 0, bytes.Length); s.Flush();
        }

        // ---- manifest: รายชื่อไฟล์ (rel รวมหมวดปลายทาง) ----
        public List<(string rel, long size)> ManifestFiles()
        {
            var list = new List<(string rel, long size)>();
            if (!Directory.Exists(StoreDir)) return list;
            foreach (var f in Directory.GetFiles(StoreDir, "*", SearchOption.AllDirectories))
            {
                var rel = f.Substring(StoreDir.Length + 1).Replace('\\', '/');
                list.Add((rel, new FileInfo(f).Length));
            }
            return list.OrderBy(x => x.rel, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public string ManifestJson()
        {
            var files = ManifestFiles();
            var sb = new StringBuilder();
            sb.Append("{\"set\":\"ShadowPlay Genuine\",\"version\":1,\"files\":[");
            bool first = true;
            foreach (var (rel, size) in files)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append("{\"rel\":\"").Append(rel).Append("\",\"size\":").Append(size).Append("}");
            }
            sb.Append("]}");
            return sb.ToString();
        }

        // ---- หน้าเว็บ: ปุ่มเดียว + %bar ----
        string Page()
        {
            int n = FileCount;
            long total = ManifestFiles().Sum(x => x.size);
            string mb = (total / 1024.0 / 1024.0).ToString("0.0");
            return
@"<!DOCTYPE html><html><head><meta charset='utf-8'><title>NvPlugins — Genuine ShadowPlay Set</title>
<style>
body{background:#121214;color:#e0e0e0;font-family:'Segoe UI',Arial;margin:0;display:flex;align-items:center;justify-content:center;height:100vh}
.card{background:#1a1a1e;padding:36px 44px;border-radius:12px;text-align:center;min-width:420px}
h1{color:#76b900;font-size:22px;margin:0 0 6px}p{color:#96969b;font-size:13px;margin:0 0 22px}
#btn{background:#145a14;color:#fff;border:0;padding:14px 34px;font-size:16px;font-weight:bold;border-radius:8px;cursor:pointer}
#btn:disabled{opacity:.5}
#barwrap{background:#2d2d32;border-radius:6px;height:18px;margin-top:22px;overflow:hidden;display:none}
#bar{background:#76b900;height:100%;width:0%}
#pct{color:#96969b;font-size:12px;margin-top:8px;min-height:16px}
.done{color:#50c878!important}
</style></head><body><div class='card'>
<h1>NvPlugins — Genuine ShadowPlay Set</h1>
<p>" + n + @" files · " + mb + @" MB — one click, the whole set</p>
<button id='btn' onclick='dl()'>⬇  Download whole set</button>
<div id='barwrap'><div id='bar'></div></div>
<div id='pct'></div>
</div>
<script>
async function dl(){
  const btn=document.getElementById('btn'),bar=document.getElementById('bar'),wrap=document.getElementById('barwrap'),pct=document.getElementById('pct');
  btn.disabled=true;wrap.style.display='block';
  const man=await (await fetch('/api/manifest')).json();
  const files=man.files;
  for(let i=0;i<files.length;i++){
    pct.textContent='downloading '+(i+1)+'/'+files.length+' — '+files[i].rel;
    const blob=await (await fetch('/file/'+files[i].rel)).blob();
    const a=document.createElement('a');a.href=URL.createObjectURL(blob);
    a.download=files[i].rel.split('/').pop();document.body.appendChild(a);a.click();a.remove();
    bar.style.width=Math.round((i+1)*100/files.length)+'%';
    pct.textContent='downloading '+(i+1)+'/'+files.length+' — '+files[i].rel+' ('+Math.round((i+1)*100/files.length)+'%)';
  }
  pct.textContent='100% — set downloaded. Place with NvPlugins.exe → Download Plugin';
  pct.className='done';pct.style.color='#50c878';
  btn.disabled=false;
}
</script></body></html>";
        }
    }
}
