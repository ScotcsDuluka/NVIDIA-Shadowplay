// MainForm.cs — logic only (layout lives in MainForm.Designer.cs)
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NvPlugins
{
    public partial class MainForm : Form
    {
        readonly PluginHost _host;
        readonly CheckPlugin _check;
        bool _booting;

        public MainForm()
        {
            InitializeComponent();
            _host = new PluginHost(AppDomain.CurrentDomain.BaseDirectory + "plugins");
            _check = new CheckPlugin();
            _host.RegisterBuiltIn(_check, _check.GetInfo());
            Load += (s, e) => { _host.LoadFromDir(); _ = RunCheckAsync(); };
        }

        // ---------------- CHECK ----------------
        void btnCheck_Click(object sender, EventArgs e) => _ = RunCheckAsync();

        void SetBusy(bool busy, string msg)
        {
            btnCheck.Enabled = !busy; btnFixAll.Enabled = !busy; btnDownloadSet.Enabled = !busy; btnUninstall?.Enabled = !busy;
            btnBootGenuine.Enabled = !busy; btnBootCustom.Enabled = !busy; btnOpenOsc.Enabled = !busy;
            statusBar.ForeColor = busy ? Color.FromArgb(150, 150, 155) : statusBar.ForeColor;
            statusBar.Text = msg;
        }

        async Task RunCheckAsync()
        {
            SetBusy(true, "Checking..."); UseWaitCursor = true;
            try
            {
                await Task.Run(() => _check.RunAll());
                RebuildTree();
                int ok = _check.Items.Count(i => i.Status == St.OK);
                int fail = _check.Items.Count(i => i.Status == St.FAIL);
                int warn = _check.Items.Count(i => i.Status == St.WARN);
                okBadge.Text = "OK " + ok; failBadge.Text = "FAIL " + fail; warnBadge.Text = "WARN " + warn;
                statusBar.ForeColor = fail == 0 ? Color.FromArgb(80, 200, 120) : Color.FromArgb(255, 82, 82);
                statusBar.Text = fail == 0 && warn == 0 ? "All systems ready — press Alt+Z"
                               : fail == 0 ? "Ready (with warnings)" : "Problems found — press FIX ALL";
            }
            finally { UseWaitCursor = false; SetBusy(false, statusBar.Text); }
        }

        void RebuildTree()
        {
            treeView.BeginUpdate(); treeView.Nodes.Clear();
            var root = treeView.Nodes.Add("System");
            foreach (var cat in new[] { "Server", "File", "Registry", "Port", "DLL", "Connection" })
            {
                var items = _check.Items.Where(i => i.Category == cat).ToList();
                if (items.Count == 0) continue;
                var cn = root.Nodes.Add(cat + $"   {items.Count(i => i.Status == St.OK)}/{items.Count}");
                cn.ForeColor = items.All(i => i.Status == St.OK) ? Color.FromArgb(80, 200, 120)
                             : items.Any(i => i.Status == St.FAIL) ? Color.FromArgb(255, 82, 82) : Color.FromArgb(255, 179, 0);
                foreach (var it in items) { var n = cn.Nodes.Add(it.Name); n.Tag = it; }
                cn.Expand();
            }
            root.Expand();
            treeView.EndUpdate();
        }

        void treeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = false;
            using var bg = new SolidBrush(treeView.BackColor);
            e.Graphics.FillRectangle(bg, e.Bounds);
            if (e.Node?.Tag is not CheckItem it)
            {
                using var pbg = new SolidBrush(Color.FromArgb(26, 26, 30));
                e.Graphics.FillRectangle(pbg, new Rectangle(e.Bounds.X, e.Bounds.Y, treeView.Width, e.Bounds.Height));
                e.Graphics.DrawString(e.Node.Text, new Font("Segoe UI", 10.5F, FontStyle.Bold), new SolidBrush(Color.FromArgb(118, 185, 0)), e.Bounds.X + 2, e.Bounds.Y + 3);
                return;
            }
            Color c = it.Status == St.OK ? Color.FromArgb(80, 200, 120) : it.Status == St.FAIL ? Color.FromArgb(255, 82, 82) : Color.FromArgb(255, 179, 0);
            using (var dot = new SolidBrush(c))
                e.Graphics.FillEllipse(dot, e.Bounds.X + 6, e.Bounds.Y + e.Bounds.Height / 2 - 5, 10, 10);
            var nameFont = new Font("Segoe UI", 10f);
            e.Graphics.DrawString(it.Name, nameFont, new SolidBrush(Color.FromArgb(224, 224, 224)), e.Bounds.X + 24, e.Bounds.Y + 3);
            float w = e.Graphics.MeasureString(it.Name, nameFont).Width;
            e.Graphics.DrawString(Trunc(it.Actual.Length > 0 ? it.Actual : it.Expected, 60), new Font("Segoe UI", 9f), new SolidBrush(Color.FromArgb(150, 150, 155)), e.Bounds.X + 24 + w + 10, e.Bounds.Y + 4);
        }

        void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var it = e.Node?.Tag as CheckItem;
            if (it == null) { detailBox.Text = e.Node?.Text ?? ""; return; }
            detailBox.Text =
                "Item     : " + it.Name + "\r\n" +
                "Category : " + it.Category + "\r\n" +
                "Status   : " + it.Status + "\r\n\r\n" +
                "Expected : " + it.Expected + "\r\n" +
                "Actual   : " + it.Actual + "\r\n" +
                (it.Note.Length > 0 ? "\r\nNote     : " + it.Note + "\r\n" : "") +
                (it.Fix != null ? "\r\nRepairable: " + it.FixLabel : "\r\nRepairable: no (manual)");
        }

        static string Trunc(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "…";

        // ---------------- FIX ----------------
        void btnFixAll_Click(object sender, EventArgs e) => _ = FixAllAsync();

        async Task FixAllAsync()
        {
            if (!U.IsAdmin()) { SelfElevate("fix"); return; }
            SetBusy(true, "Repairing...");
            int n = await Task.Run(() => _check.FixAll(m => Invoke(() => statusBar.Text = m)));
            await RunCheckAsync();
            MessageBox.Show("Repaired " + n + " item(s) — rechecked.", "NvPlugins");
        }

        void FixSelected()
        {
            var it = treeView.SelectedNode?.Tag as CheckItem;
            if (it == null) { MessageBox.Show("Select an item in the tree first."); return; }
            if (it.Fix == null) { MessageBox.Show("This item cannot be auto-repaired:\n" + it.Note); return; }
            if (!U.IsAdmin()) { SelfElevate("fix"); return; }
            SetBusy(true, "Fixing: " + it.Name);
            Task.Run(() =>
            {
                try { it.Fix(); } catch (Exception ex) { Invoke(() => MessageBox.Show("Fix failed: " + ex.Message)); }
                Invoke(() => { SetBusy(false, ""); _ = RunCheckAsync(); });
            });
        }

        void SelfElevate(string arg)
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = Application.ExecutablePath, Arguments = arg, Verb = "runas", UseShellExecute = true }); }
            catch { }
        }

        // ---------------- UNINSTALL ----------------
        void btnUninstall_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Uninstall the deployed set?\r\n\r\n- stops the whole stack\r\n- removes deployed files (System32/GFE/ShadowPlay/NvContainer/NvNode)\r\n- cleans our registry values\r\n- service disabled\r\n\r\nRe-install anytime: press Download Genuine Set.",
                "Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (!U.IsAdmin()) { SelfElevate("uninstall"); return; }
            _booting = true;
            bootLogBox.Clear();
            void Log(string m) { if (InvokeRequired) Invoke(() => bootLogBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + m + Environment.NewLine)); else bootLogBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + m + Environment.NewLine); }
            Task.Run(() =>
            {
                try { Deploy.UninstallAll(Log); Invoke(() => { bootStateLabel.Text = "  Set status: uninstalled — re-install = Download Genuine Set"; bootStateLabel.ForeColor = Color.FromArgb(255, 179, 0); _ = RunCheckAsync(); }); }
                catch (Exception ex) { Log("ERROR: " + ex.Message); }
                finally { _booting = false; }
            });
        }

        // ---------------- OSC MODES ----------------
        async void btnBootGenuine_Click(object sender, EventArgs e)
        {
            SetBusy(true, "Booting Genuine NVIDIA stack...");
            oscStatusLabel.ForeColor = System.Drawing.Color.FromArgb(255, 179, 0); oscStatusLabel.Text = "mode: booting GENUINE...";
            oscLogBox.Clear();
            void Log(string m) => Invoke(() => oscLogBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + m + Environment.NewLine));
            try
            {
                await System.Threading.Tasks.Task.Run(() => OscModes.BootGenuine(Log));
                oscStatusLabel.ForeColor = System.Drawing.Color.FromArgb(80, 200, 120);
                oscStatusLabel.Text = "mode: GENUINE NVIDIA — press Alt+Z or OPEN OSC";
            }
            catch (Exception ex) { Log("ERROR: " + ex.Message); oscStatusLabel.Text = "mode: GENUINE boot failed — see log"; }
            await RunCheckAsync();
            SetBusy(false, statusBar.Text);
        }

        async void btnBootCustom_Click(object sender, EventArgs e)
        {
            SetBusy(true, "Booting Custom OSC (NVIDIA-free)...");
            oscStatusLabel.ForeColor = System.Drawing.Color.FromArgb(255, 179, 0); oscStatusLabel.Text = "mode: booting CUSTOM...";
            oscLogBox.Clear();
            void Log(string m) => Invoke(() => oscLogBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + m + Environment.NewLine));
            try
            {
                await System.Threading.Tasks.Task.Run(() => OscModes.BootCustom(Log));
                oscStatusLabel.ForeColor = System.Drawing.Color.FromArgb(120, 180, 255);
                oscStatusLabel.Text = "mode: CUSTOM (NVIDIA-free) — press OPEN OSC or Alt+Z (DulukaPort)";
            }
            catch (Exception ex) { Log("ERROR: " + ex.Message); oscStatusLabel.Text = "mode: CUSTOM boot failed — see log"; }
            await RunCheckAsync();
            SetBusy(false, statusBar.Text);
        }

        void btnOpenOsc_Click(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Run(() => OscModes.OpenOsc(m => Invoke(() => oscLogBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + m + Environment.NewLine))));
            oscStatusLabel.ForeColor = System.Drawing.Color.FromArgb(80, 200, 120);
            oscStatusLabel.Text = "mode: OPEN OSC sent";
        }

        // ---------------- DOWNLOAD CLIENT (auto-deploy) ----------------
        async void btnDownloadSet_Click(object sender, EventArgs e)
        {
            if (_booting) return;
            string server = serverUrlText.Text.Trim();
            if (string.IsNullOrWhiteSpace(server)) server = "http://127.0.0.1:15246";
            if (!U.IsAdmin()) { SelfElevate("deploy " + server); return; }
            _booting = true;
            bootLogBox.Clear();
            void Log(string m) => Invoke(() => bootLogBox.AppendText("[" + DateTime.Now.ToString("HH:mm:ss") + "] " + m + Environment.NewLine));
            void Step(string m) { Invoke(() => { bootStateLabel.Text = "  Working: " + m; bootStateLabel.ForeColor = Color.FromArgb(255, 179, 0); }); Log(m); }
            void Done(string m) { Invoke(() => { bootStateLabel.Text = m; bootStateLabel.ForeColor = Color.FromArgb(80, 200, 120); }); Log(m); }

            try
            {
                await Task.Run(() => Deploy.RunFromServer(server, Log, m => Invoke(() => bootStateLabel.Text = "  " + m)));
                await RunCheckAsync();
                int fail = _check.Items.Count(i => i.Status == St.FAIL);
                Done(fail == 0 ? "Set status: ✅ deployed — press Alt+Z" : $"Set status: ⚠ {fail} item(s) still failing (see System Tree)");
                Log(fail == 0 ? "=== SET DEPLOYED — press Alt+Z ===" : $"=== {fail} failing — see System Tree ===");
            }
            catch (Exception ex)
            {
                Invoke(() => { bootStateLabel.Text = "  Error: " + ex.Message; bootStateLabel.ForeColor = Color.FromArgb(255, 82, 82); });
                Log("ERROR: " + ex.Message);
            }
            finally { _booting = false; }
        }
    }
}
