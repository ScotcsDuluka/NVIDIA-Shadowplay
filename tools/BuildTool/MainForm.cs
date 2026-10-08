using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;

namespace BuildTool;

public class MainForm : Form
{
    private readonly WebView2 _web = new() { Dock = DockStyle.Fill };
    private readonly Bridge _bridge;
    private SynchronizationContext _uiSync;

    public static void UiLog(string line)
    {
        try
        {
            File.AppendAllText(
                Path.Combine(AppContext.BaseDirectory, "..", "Build", "Build-Config", "ui-log.txt"),
                "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + line + Environment.NewLine);
        }
        catch { }
    }

    public MainForm()
    {
        var root = RootLocator.Find();
        _bridge = new Bridge(root);

        Text = "NVIDIA ShadowPlay - Build";
        Width = 1180;
        Height = 820;
        MinimumSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(10, 10, 10);
        try
        {
            var ico = Path.Combine(root, "Project", "NVIDIA ShadowPlay.ico");
            if (File.Exists(ico)) Icon = new Icon(ico);
        }
        catch { }

        Controls.Add(_web);

        UiLog("form created");

        _uiSync = SynchronizationContext.Current;
        Load += async (_, _) =>
        {
            try
            {
                var envOpts = new CoreWebView2EnvironmentOptions();
                // GPU ของเครื่องนี้ render WebView2 เป็นดำ (เหมือน CEF เดิม) - บังคับ software rendering
                envOpts.AdditionalBrowserArguments = "--disable-gpu";
                var env = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: Path.Combine(root, "Build", "Build-Config", "webview2"),
                    options: envOpts);
                await _web.EnsureCoreWebView2Async(env);

                var core = _web.CoreWebView2;
                core.Settings.AreDefaultContextMenusEnabled = false;
                _web.CoreWebView2InitializationCompleted += (_, e) => UiLog("core init ok=" + e.IsSuccess);
                core.SetVirtualHostNameToFolderMapping(
                    "app.local",
                    Path.Combine(AppContext.BaseDirectory, "wwwroot"),
                    CoreWebView2HostResourceAccessKind.Allow);
                core.AddHostObjectToScript("hostLegacy", _bridge);   // สำรอง - SPA หลักใช้ postMessage RPC
                core.WebMessageReceived += RpcReceived;
                core.Navigate("https://app.local/index.html");
            }
            catch (Exception ex)
            {
                UiLog("init failed: " + ex.Message);
                MessageBox.Show(
                    "WebView2 init failed: " + ex.Message +
                    "\n\nติดตั้ง WebView2 Runtime: https://developer.microsoft.com/microsoft-edge/webview2/",
                    "NVIDIA ShadowPlay - Build",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        };
    }

    // ─── postMessage RPC dispatcher ───
    // JS: chrome.webview.postMessage(JSON.stringify({id, method, args}))
    // C#: dispatch บน background thread -> ตอบกลับ PostWebMessageAsJson({id, result})
    private async void RpcReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        JsonElement payload;
        try
        {
            // postMessage(payload): payload ที่เป็น JSON text จะถูก wrap เป็น JSON string
            // (double-encoded) - decode ชั้นนอกก่อนเสมอ
            var outer = JsonDocument.Parse(e.WebMessageAsJson).RootElement;
            var text = outer.ValueKind == JsonValueKind.String ? outer.GetString() : outer.GetRawText();
            if (string.IsNullOrWhiteSpace(text) || !text.TrimStart().StartsWith("{"))
            {
                // ข้อความธรรมดา (JSERR:/JSREJ:/DOM: debug ฯลฯ) = ไม่ใช่ RPC - บันทึกแล้วจบ
                UiLog("JS note: " + text);
                return;
            }
            payload = JsonDocument.Parse(text).RootElement;
        }
        catch (Exception ex)
        {
            UiLog("envelope failed: " + ex.Message);
            return;
        }

        // ข้อความธรรมดา (DOM debug ฯลฯ) = ไม่ใช่ RPC - บันทึกแล้วจบ
        if (payload.ValueKind != JsonValueKind.Object)
        {
            UiLog("JS note: " + payload.GetRawText());
            return;
        }
        if (!payload.TryGetProperty("id", out var idEl))
        {
            UiLog("JS note: " + payload.GetRawText());
            return;
        }
        var id = idEl.GetString();
        var method = payload.TryGetProperty("method", out var mEl) ? mEl.GetString() : "";
        var args = payload.TryGetProperty("args", out var aEl) && aEl.ValueKind == JsonValueKind.Array
            ? aEl.Clone()
            : (JsonElement?)default;

        var core = _web.CoreWebView2;
        await Task.Run(async () =>
        {
            try
            {
                string result = method switch
                {
                    "status" => _bridge.GetStatus(),
                    "startBuild" => _bridge.StartBuild(ArgsBool(args, 0)),
                    "cancelBuild" => _bridge.CancelBuild(),
                    "log" => _bridge.GetLog(ArgsInt(args, 0)),
                    "version" => _bridge.GetVersionConfig(),
                    "versions" => _bridge.GetVersions(),
                    "saveVersions" => _bridge.SaveVersions(ArgsStr(args, 0)),
                    "saveVersion" => _bridge.SaveVersionConfig(ArgsStr(args, 0)),
                    "preview" => _bridge.GetPreview(),
                    "launch" => _bridge.LaunchApp(),
                    "openFolder" => _bridge.OpenFolder(),
                    _ => JsonSerializer.Serialize(new { error = "unknown method " + method }),
                };
                var resp = JsonSerializer.Serialize(new { id, result });
                _uiSync.Post(_ => core.PostWebMessageAsJson(resp), null);
            }
            catch (Exception ex)
            {
                UiLog("RPC '" + method + "' failed: " + ex.Message);
                var errResp = JsonSerializer.Serialize(new { id, error = ex.Message });
                _uiSync.Post(_ => core.PostWebMessageAsJson(errResp), null);
            }
        });
    }

    private static bool ArgsBool(JsonElement? args, int idx) =>
        args.HasValue && args.Value.GetArrayLength() > idx && args.Value[idx].ValueKind == JsonValueKind.True;

    private static int ArgsInt(JsonElement? args, int idx) =>
        args.HasValue && args.Value.GetArrayLength() > idx ? args.Value[idx].GetInt32() : 0;

    private static string ArgsStr(JsonElement? args, int idx)
    {
        // arg จาก JS มักเป็น JSON string (JSON.stringify แล้ว) - ต้องไขชั้นคำพูดออกก่อน
        // ไม่งั้นปลายทางได้ JSON ที่ encode ซ้อนแล้ว Deserialize พังทุกครั้ง
        if (!args.HasValue || args.Value.GetArrayLength() <= idx) return "{}";
        var el = args.Value[idx];
        return el.ValueKind == JsonValueKind.String ? el.GetString() : el.GetRawText();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_bridge.IsRunning && MessageBox.Show(
                "build กำลังรันอยู่ — ปิดเลยนะเหรอ?\n(build process จะถูกยกเลิกทั้ง tree)",
                "NVIDIA ShadowPlay - Build", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
        {
            e.Cancel = true;
            return;
        }
        _bridge.KillIfRunning();
        base.OnFormClosing(e);
    }
}
