using Microsoft.Web.WebView2.Core;
using System.Text.Json;
using Microsoft.Web.WebView2.WinForms;

namespace BuildTool;

public class MainForm : Form
{
    private readonly WebView2 _web = new() { Dock = DockStyle.Fill };
    private readonly Bridge _bridge;

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
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, ".."));
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

        Load += async (_, _) =>
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: Path.Combine(root, "Build", "Build-Config", "webview2"));
                await _web.EnsureCoreWebView2Async(env);

                var core = _web.CoreWebView2;
                core.Settings.AreDefaultContextMenusEnabled = false;
                _web.CoreWebView2InitializationCompleted += (_, e) => UiLog("core init ok=" + e.IsSuccess);
                core.WebMessageReceived += async (_, e) =>
                {
                    try
                    {
                        // postMessage(string) arrives DOUBLE-ENCODED as a JSON string
                        var outer = JsonDocument.Parse(e.WebMessageAsJson).RootElement;
                        var rootEl = outer.ValueKind == JsonValueKind.String
                            ? JsonDocument.Parse(outer.GetString()).RootElement
                            : outer;
                        if (rootEl.ValueKind != JsonValueKind.Object || !rootEl.TryGetProperty("id", out var idEl))
                        {
                            UiLog("JS: " + e.WebMessageAsJson);
                            return;
                        }
                        var id = idEl.GetString();
                        var method = rootEl.TryGetProperty("method", out var mEl) ? mEl.GetString() : "";
                        var args = rootEl.TryGetProperty("args", out var aEl) && aEl.ValueKind == JsonValueKind.Array ? aEl : default;

                        string result = method switch
                        {
                            "status" => _bridge.GetStatus(),
                            "startBuild" => _bridge.StartBuild(args.ValueKind == JsonValueKind.Array && args.GetArrayLength() > 0 && args[0].GetBoolean()),
                            "log" => _bridge.GetLog(args.ValueKind == JsonValueKind.Array && args.GetArrayLength() > 0 ? args[0].GetInt32() : 0),
                            "version" => _bridge.GetVersionConfig(),
                            "saveVersion" => _bridge.SaveVersionConfig(args.ValueKind == JsonValueKind.Array && args.GetArrayLength() > 0 ? args[0].GetRawText() : "{}"),
                            "preview" => _bridge.GetPreview(),
                            "launch" => _bridge.LaunchApp(),
                            "openFolder" => _bridge.OpenFolder(),
                            _ => JsonSerializer.Serialize(new { error = "unknown method " + method }),
                        };
                        var resp = JsonSerializer.Serialize(new { id, result });
                        core.PostWebMessageAsJson(resp);
                    }
                    catch (Exception ex)
                    {
                        UiLog("RPC failed: " + ex.Message);
                    }
                };
                core.NavigationCompleted += (_, e) => UiLog("nav done ok=" + e.IsSuccess + " err=" + e.WebErrorStatus);
                core.ProcessFailed += (_, e) => UiLog("PROCESS FAILED: " + e.ProcessFailedKind);
                core.Settings.IsStatusBarEnabled = false;
                core.SetVirtualHostNameToFolderMapping(
                    "app.local",
                    Path.Combine(AppContext.BaseDirectory, "wwwroot"),
                    CoreWebView2HostResourceAccessKind.Allow);
                core.AddHostObjectToScript("host", _bridge);
                core.NewWindowRequested += (_, e) =>
                {
                    e.Handled = true;   // แอปเดสก์ท็อป - ไม่เปิดหน้าต่าง CEF ใหม่
                };
                core.Navigate("https://app.local/index.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "WebView2 init failed: " + ex.Message +
                    "\n\nติดตั้ง WebView2 Runtime: https://developer.microsoft.com/microsoft-edge/webview2/",
                    "NVIDIA ShadowPlay - Build",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        };
    }
}
