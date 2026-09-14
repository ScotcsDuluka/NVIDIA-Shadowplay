// MiSideHook — NVIDIA Share in-game hook (MelonMod for MiSide, Unity IL2CPP).
//
// Whitelist + consent model (owner-approved): runs ONLY inside MiSideFull.exe
// (offline, officially mod-friendly). Draws the LIVE osc overlay INSIDE the
// game frame (frames streamed by NVIDIA Share.exe through shared memory) and
// forwards mouse input back to the controller.
//
// Handshake: the controller writes NvidiaShareHook.json next to the game exe
// (port + secret). Polls /Hook/Poll (commands incl. overlayVisible), draws
// MMF "NVIDIA_Share_Overlay_Frame_v1" (BGRA32), reports /Hook/Report.

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Net.Http;
using System.Text;
using System.Threading;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(NvidiaShare.MiSideHook), "NVIDIA Share Hook", "1.1.0", "NVIDIA Share")]
[assembly: MelonGame(null, null)]

namespace NvidiaShare
{
    public class MiSideHook : MelonMod
    {
        private static bool markerVisible = true;
        private static bool overlayVisible;
        private static string markerText = "NVIDIA Share - hooked";
        private static string port;
        private static string secret;
        private static float smoothFps;
        private static int lastReportTick;

        private static Texture2D frameTex;
        private static byte[] frameBytes;
        private static int lastFrameId = -1;
        private static MemoryMappedFile frameMmf;
        private static MemoryMappedViewAccessor frameView;

        private static readonly HttpClient Http = new HttpClient();

        public override void OnInitializeMelon()
        {
            var bridge = Path.Combine(AppContext.BaseDirectory, "NvidiaShareHook.json");
            MelonLogger.Msg("NVIDIA Share hook: bridge=" + bridge + " exists=" + File.Exists(bridge));
            var t = new Thread(PollLoop) { IsBackground = true };
            t.Start();
        }

        private static void PollLoop()
        {
            while (true)
            {
                try
                {
                    if (port == null)
                    {
                        var bridge = Path.Combine(AppContext.BaseDirectory, "NvidiaShareHook.json");
                        if (!File.Exists(bridge)) { Thread.Sleep(2000); continue; }
                        var j = System.Text.Json.JsonDocument.Parse(File.ReadAllText(bridge)).RootElement;
                        port = j.GetProperty("port").ToString();
                        secret = j.GetProperty("secret").GetString();
                        Http.DefaultRequestHeaders.Remove("X_LOCAL_SECURITY_COOKIE");
                        Http.DefaultRequestHeaders.Add("X_LOCAL_SECURITY_COOKIE", secret);
                        MelonLogger.Msg("NVIDIA Share hook: connected to port " + port);
                    }

                    var body = Http.GetStringAsync("http://127.0.0.1:" + port + "/ShadowPlay/v.1.0/Hook/Poll").Result;
                    overlayVisible = body.Contains("\"overlayVisible\":true");

                    var report = "{\"width\":" + Screen.currentResolution.width +
                                 ",\"height\":" + Screen.currentResolution.height +
                                 ",\"fps\":" + smoothFps.ToString("F0", System.Globalization.CultureInfo.InvariantCulture) + "}";
                    Http.PostAsync("http://127.0.0.1:" + port + "/ShadowPlay/v.1.0/Hook/Report",
                        new StringContent(report, Encoding.UTF8, "application/json")).Wait();
                }
                catch
                {
                    port = null; // controller restarted → re-read the bridge
                }
                Thread.Sleep(1500);
            }
        }

        public override void OnUpdate()
        {
            if (!overlayVisible) return;
            try
            {
                if (frameMmf == null)
                {
                    frameMmf = MemoryMappedFile.OpenExisting(HookFrameMmf.Name, MemoryMappedFileRights.Read);
                    frameView = frameMmf.CreateViewAccessor();
                }
                int magic = frameView.ReadInt32(0);
                if (magic != HookFrameMmf.Magic) return;
                int w = frameView.ReadInt32(4);
                int h = frameView.ReadInt32(8);
                int fid = frameView.ReadInt32(12);
                if (w <= 0 || h <= 0 || fid == lastFrameId) return;

                int px = w * h * 4;
                if (frameBytes == null || frameBytes.Length != px) frameBytes = new byte[px];
                frameView.ReadArray(64, frameBytes, 0, px);
                lastFrameId = fid;

                if (frameTex == null || frameTex.width != w || frameTex.height != h)
                    frameTex = new Texture2D(w, h, TextureFormat.BGRA32, false);
                frameTex.LoadRawTextureData(frameBytes);
                frameTex.Apply(false);
            }
            catch (FileNotFoundException) { frameMmf = null; }   // pump not started yet
            catch { }
        }

        public override void OnGUI()
        {
            if (markerVisible)
            {
                var r = new Rect(16f, 16f, 380f, 64f);
                GUI.Box(r, GUIContent.none);
                GUI.Label(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, 24f), markerText);
                GUI.Label(new Rect(r.x + 12f, r.y + 34f, r.width - 24f, 22f),
                    "FPS " + smoothFps.ToString("F0") + "   Res " +
                    Screen.currentResolution.width + "x" + Screen.currentResolution.height);
            }

            if (!overlayVisible || frameTex == null) return;
            var full = new Rect(0f, 0f, Screen.width, Screen.height);
            GUI.DrawTexture(full, frameTex, ScaleMode.StretchToFill, alphaBlend: true);

            // forward mouse input over the overlay
            var e = Event.current;
            if (e == null) return;
            string type = null;
            int button = 0;
            if (e.type == EventType.MouseMove) type = "mousemove";
            else if (e.type == EventType.MouseDown) { type = "mousedown"; button = (int)e.button; }
            else if (e.type == EventType.MouseUp) { type = "mouseup"; button = (int)e.button; }
            if (type == null) return;
            var payload = "{\"type\":\"" + type + "\",\"x\":" + e.mousePosition.x.ToString("F0") +
                          ",\"y\":" + e.mousePosition.y.ToString("F0") + ",\"button\":" + button + "}";
            if (port != null)
                Http.PostAsync("http://127.0.0.1:" + port + "/ShadowPlay/v.1.0/Hook/Input",
                    new StringContent(payload, Encoding.UTF8, "application/json"));
        }
    }

    public static class HookFrameMmf
    {
        public const string Name = "NVIDIA_Share_Overlay_Frame_v1";
        public const int Magic = unchecked((int)0x4C50534E); // "NSPL"
    }
}
