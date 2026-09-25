using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace BuildTool;

public class MainForm : Form
{
    private readonly WebView2 _web = new() { Dock = DockStyle.Fill };
    private readonly Bridge _bridge;

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

        Load += async (_, _) =>
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: Path.Combine(root, "Build", "Build-Config", "webview2"));
                await _web.EnsureCoreWebView2Async(env);

                var core = _web.CoreWebView2;
                core.Settings.AreDefaultContextMenusEnabled = false;
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
