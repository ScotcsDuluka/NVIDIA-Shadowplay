namespace NvPlugins
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label subLabel;
        private System.Windows.Forms.Label okBadge;
        private System.Windows.Forms.Label failBadge;
        private System.Windows.Forms.Label warnBadge;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Button btnFixAll;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabDownload;
        private System.Windows.Forms.TabPage tabOsc;
        private System.Windows.Forms.TabPage tabTree;
        private System.Windows.Forms.Panel heroPanel;
        private System.Windows.Forms.Label heroTitle;
        private System.Windows.Forms.Label heroSub;
        private System.Windows.Forms.Button btnDownloadSet;
        private System.Windows.Forms.Button btnUninstall;
        private System.Windows.Forms.TextBox serverUrlText;
        private System.Windows.Forms.Label serverUrlTextLabel;
        private System.Windows.Forms.Label bootStateLabel;
        private System.Windows.Forms.Label bootLogLabel;
        private System.Windows.Forms.TextBox bootLogBox;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Label detailLabel;
        private System.Windows.Forms.TextBox detailBox;
        private System.Windows.Forms.Label statusBar;
        private System.Windows.Forms.Label oscInfoLabel;
        private System.Windows.Forms.Button btnBootGenuine;
        private System.Windows.Forms.Button btnBootCustom;
        private System.Windows.Forms.Button btnOpenOsc;
        private System.Windows.Forms.Label oscStatusLabel;
        private System.Windows.Forms.TextBox oscLogBox;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            headerPanel = new System.Windows.Forms.Panel();
            subLabel = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            titleLabel = new System.Windows.Forms.Label();
            okBadge = new System.Windows.Forms.Label();
            failBadge = new System.Windows.Forms.Label();
            warnBadge = new System.Windows.Forms.Label();
            btnCheck = new System.Windows.Forms.Button();
            btnFixAll = new System.Windows.Forms.Button();
            tabControl = new System.Windows.Forms.TabControl();
            tabDownload = new System.Windows.Forms.TabPage();
            tabOsc = new System.Windows.Forms.TabPage();
            oscInfoLabel = new System.Windows.Forms.Label();
            btnBootGenuine = new System.Windows.Forms.Button();
            btnBootCustom = new System.Windows.Forms.Button();
            btnOpenOsc = new System.Windows.Forms.Button();
            oscStatusLabel = new System.Windows.Forms.Label();
            oscLogBox = new System.Windows.Forms.TextBox();
            heroPanel = new System.Windows.Forms.Panel();
            heroTitle = new System.Windows.Forms.Label();
            heroSub = new System.Windows.Forms.Label();
            btnDownloadSet = new System.Windows.Forms.Button();
            bootStateLabel = new System.Windows.Forms.Label();
            bootLogLabel = new System.Windows.Forms.Label();
            bootLogBox = new System.Windows.Forms.TextBox();
            tabTree = new System.Windows.Forms.TabPage();
            splitContainer = new System.Windows.Forms.SplitContainer();
            treeView = new System.Windows.Forms.TreeView();
            detailLabel = new System.Windows.Forms.Label();
            detailBox = new System.Windows.Forms.TextBox();
            serverUrlTextLabel = new System.Windows.Forms.Label();
            serverUrlText = new System.Windows.Forms.TextBox();
            statusBar = new System.Windows.Forms.Label();
            headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            tabControl.SuspendLayout();
            tabDownload.SuspendLayout();
            tabOsc.SuspendLayout();
            heroPanel.SuspendLayout();
            tabTree.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // headerPanel
            // 
            headerPanel.BackColor = System.Drawing.SystemColors.Control;
            headerPanel.Controls.Add(subLabel);
            headerPanel.Controls.Add(pictureBox1);
            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(okBadge);
            headerPanel.Controls.Add(failBadge);
            headerPanel.Controls.Add(warnBadge);
            headerPanel.Controls.Add(btnCheck);
            headerPanel.Controls.Add(btnFixAll);
            headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
            headerPanel.ForeColor = System.Drawing.SystemColors.Desktop;
            headerPanel.Location = new System.Drawing.Point(0, 0);
            headerPanel.Name = "headerPanel";
            headerPanel.Size = new System.Drawing.Size(1180, 58);
            headerPanel.TabIndex = 2;
            // 
            // subLabel
            // 
            subLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            subLabel.ForeColor = System.Drawing.Color.FromArgb(150, 150, 155);
            subLabel.Location = new System.Drawing.Point(59, 34);
            subLabel.Name = "subLabel";
            subLabel.Size = new System.Drawing.Size(262, 24);
            subLabel.TabIndex = 1;
            subLabel.Text = "ShadowPlay / Requirement";
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImage = Properties.Resources.lg_osc_logo_48x48;
            pictureBox1.Location = new System.Drawing.Point(5, 5);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(48, 48);
            pictureBox1.TabIndex = 7;
            pictureBox1.TabStop = false;
            // 
            // titleLabel
            // 
            titleLabel.BackColor = System.Drawing.Color.Transparent;
            titleLabel.Font = new System.Drawing.Font("GeForce", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            titleLabel.ForeColor = System.Drawing.Color.Black;
            titleLabel.Location = new System.Drawing.Point(59, 5);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(200, 29);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "NVIDIA Plugins";
            titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // okBadge
            // 
            okBadge.AutoSize = true;
            okBadge.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            okBadge.ForeColor = System.Drawing.Color.FromArgb(80, 200, 120);
            okBadge.Location = new System.Drawing.Point(547, 21);
            okBadge.Name = "okBadge";
            okBadge.Size = new System.Drawing.Size(45, 21);
            okBadge.TabIndex = 2;
            okBadge.Text = "OK 0";
            // 
            // failBadge
            // 
            failBadge.AutoSize = true;
            failBadge.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            failBadge.ForeColor = System.Drawing.Color.FromArgb(255, 82, 82);
            failBadge.Location = new System.Drawing.Point(677, 21);
            failBadge.Name = "failBadge";
            failBadge.Size = new System.Drawing.Size(54, 21);
            failBadge.TabIndex = 3;
            failBadge.Text = "FAIL 0";
            // 
            // warnBadge
            // 
            warnBadge.AutoSize = true;
            warnBadge.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            warnBadge.ForeColor = System.Drawing.Color.FromArgb(255, 179, 0);
            warnBadge.Location = new System.Drawing.Point(807, 21);
            warnBadge.Name = "warnBadge";
            warnBadge.Size = new System.Drawing.Size(72, 21);
            warnBadge.TabIndex = 4;
            warnBadge.Text = "WARN 0";
            // 
            // btnCheck
            // 
            btnCheck.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnCheck.BackColor = System.Drawing.Color.FromArgb(34, 34, 38);
            btnCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnCheck.Font = new System.Drawing.Font("GeForce", 13F, System.Drawing.FontStyle.Bold);
            btnCheck.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
            btnCheck.Location = new System.Drawing.Point(942, 12);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new System.Drawing.Size(110, 34);
            btnCheck.TabIndex = 5;
            btnCheck.Text = "CHECK";
            btnCheck.UseVisualStyleBackColor = false;
            btnCheck.Click += btnCheck_Click;
            // 
            // btnFixAll
            // 
            btnFixAll.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnFixAll.BackColor = System.Drawing.Color.FromArgb(34, 34, 38);
            btnFixAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFixAll.Font = new System.Drawing.Font("GeForce", 13F, System.Drawing.FontStyle.Bold);
            btnFixAll.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
            btnFixAll.Location = new System.Drawing.Point(1058, 12);
            btnFixAll.Name = "btnFixAll";
            btnFixAll.Size = new System.Drawing.Size(110, 34);
            btnFixAll.TabIndex = 6;
            btnFixAll.Text = "FIX ALL";
            btnFixAll.UseVisualStyleBackColor = false;
            btnFixAll.Click += btnFixAll_Click;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabDownload);
            tabControl.Controls.Add(tabOsc);
            tabControl.Controls.Add(tabTree);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Location = new System.Drawing.Point(0, 58);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(1180, 602);
            tabControl.TabIndex = 0;
            // 
            // tabDownload
            // 
            tabDownload.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            tabDownload.Controls.Add(heroPanel);
            tabDownload.Controls.Add(bootStateLabel);
            tabDownload.Controls.Add(bootLogLabel);
            tabDownload.Controls.Add(bootLogBox);
            tabDownload.Location = new System.Drawing.Point(4, 26);
            tabDownload.Name = "tabDownload";
            tabDownload.Size = new System.Drawing.Size(1172, 572);
            tabDownload.TabIndex = 0;
            tabDownload.Text = "Download Plugin (Genuine Set)";
            // 
            // heroPanel
            // 
            heroPanel.BackColor = System.Drawing.Color.FromArgb(224, 224, 224);
            heroPanel.Controls.Add(heroTitle);
            heroPanel.Controls.Add(heroSub);
            heroPanel.Controls.Add(btnDownloadSet);
            heroPanel.Dock = System.Windows.Forms.DockStyle.Top;
            heroPanel.Location = new System.Drawing.Point(0, 60);
            heroPanel.Name = "heroPanel";
            heroPanel.Padding = new System.Windows.Forms.Padding(16);
            heroPanel.Size = new System.Drawing.Size(1172, 130);
            heroPanel.TabIndex = 0;
            // 
            // heroTitle
            // 
            heroTitle.AutoSize = true;
            heroTitle.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            heroTitle.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            heroTitle.Location = new System.Drawing.Point(16, 10);
            heroTitle.Name = "heroTitle";
            heroTitle.Size = new System.Drawing.Size(421, 25);
            heroTitle.TabIndex = 0;
            heroTitle.Text = "Genuine ShadowPlay Set — everything required";
            // 
            // heroSub
            // 
            heroSub.AutoSize = true;
            heroSub.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            heroSub.ForeColor = System.Drawing.Color.FromArgb(150, 150, 155);
            heroSub.Location = new System.Drawing.Point(18, 40);
            heroSub.Name = "heroSub";
            heroSub.Size = new System.Drawing.Size(831, 17);
            heroSub.TabIndex = 1;
            heroSub.Text = "One button: download → place at hardcoded paths → set registry → boot the whole stack (PF container → Share → node → re-arm → helper)";
            // 
            // btnDownloadSet
            // 
            btnDownloadSet.BackColor = System.Drawing.Color.FromArgb(20, 90, 20);
            btnDownloadSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnDownloadSet.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            btnDownloadSet.ForeColor = System.Drawing.Color.White;
            btnDownloadSet.Location = new System.Drawing.Point(16, 68);
            btnDownloadSet.Name = "btnDownloadSet";
            btnDownloadSet.Size = new System.Drawing.Size(560, 48);
            btnDownloadSet.TabIndex = 2;
            btnDownloadSet.Text = "⬇  Download Genuine Set (download → place → configure → boot)";
            btnDownloadSet.UseVisualStyleBackColor = false;
            btnDownloadSet.Click += btnDownloadSet_Click;
            // 
            // bootStateLabel
            // 
            bootStateLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            bootStateLabel.Dock = System.Windows.Forms.DockStyle.Top;
            bootStateLabel.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            bootStateLabel.ForeColor = System.Drawing.Color.FromArgb(255, 179, 0);
            bootStateLabel.Location = new System.Drawing.Point(0, 30);
            bootStateLabel.Name = "bootStateLabel";
            bootStateLabel.Padding = new System.Windows.Forms.Padding(10, 4, 0, 0);
            bootStateLabel.Size = new System.Drawing.Size(1172, 30);
            bootStateLabel.TabIndex = 1;
            bootStateLabel.Text = "  Set status: not loaded yet";
            // 
            // bootLogLabel
            // 
            bootLogLabel.BackColor = System.Drawing.Color.WhiteSmoke;
            bootLogLabel.Dock = System.Windows.Forms.DockStyle.Top;
            bootLogLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            bootLogLabel.ForeColor = System.Drawing.Color.FromArgb(118, 185, 0);
            bootLogLabel.Location = new System.Drawing.Point(0, 0);
            bootLogLabel.Name = "bootLogLabel";
            bootLogLabel.Padding = new System.Windows.Forms.Padding(8, 4, 0, 0);
            bootLogLabel.Size = new System.Drawing.Size(1172, 30);
            bootLogLabel.TabIndex = 2;
            bootLogLabel.Text = "  Download log";
            // 
            // bootLogBox
            // 
            bootLogBox.BackColor = System.Drawing.Color.Gray;
            bootLogBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            bootLogBox.Font = new System.Drawing.Font("Consolas", 10F);
            bootLogBox.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
            bootLogBox.Location = new System.Drawing.Point(0, 196);
            bootLogBox.Multiline = true;
            bootLogBox.Name = "bootLogBox";
            bootLogBox.ReadOnly = true;
            bootLogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            bootLogBox.Size = new System.Drawing.Size(1172, 370);
            bootLogBox.TabIndex = 3;
            //
            // tabOsc
            //
            tabOsc.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            tabOsc.Controls.Add(oscInfoLabel);
            tabOsc.Controls.Add(btnBootGenuine);
            tabOsc.Controls.Add(btnBootCustom);
            tabOsc.Controls.Add(btnOpenOsc);
            tabOsc.Controls.Add(oscStatusLabel);
            tabOsc.Controls.Add(oscLogBox);
            tabOsc.Location = new System.Drawing.Point(4, 26);
            tabOsc.Name = "tabOsc";
            tabOsc.Size = new System.Drawing.Size(1172, 572);
            tabOsc.TabIndex = 2;
            tabOsc.Text = "OSC Mode";
            //
            // oscInfoLabel
            //
            oscInfoLabel.AutoSize = true;
            oscInfoLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            oscInfoLabel.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            oscInfoLabel.Location = new System.Drawing.Point(16, 14);
            oscInfoLabel.Name = "oscInfoLabel";
            oscInfoLabel.Text = "OSC Mode — who feeds the page (Genuine = NVIDIA container · Custom = Custom Data only, NVIDIA-free)";
            //
            // btnBootGenuine
            //
            btnBootGenuine.BackColor = System.Drawing.Color.FromArgb(20, 90, 20);
            btnBootGenuine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnBootGenuine.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnBootGenuine.ForeColor = System.Drawing.Color.White;
            btnBootGenuine.Location = new System.Drawing.Point(16, 52);
            btnBootGenuine.Name = "btnBootGenuine";
            btnBootGenuine.Size = new System.Drawing.Size(340, 52);
            btnBootGenuine.Text = "BOOT GENUINE NVIDIA";
            btnBootGenuine.UseVisualStyleBackColor = false;
            btnBootGenuine.Click += btnBootGenuine_Click;
            //
            // btnBootCustom
            //
            btnBootCustom.BackColor = System.Drawing.Color.FromArgb(20, 60, 90);
            btnBootCustom.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnBootCustom.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnBootCustom.ForeColor = System.Drawing.Color.White;
            btnBootCustom.Location = new System.Drawing.Point(372, 52);
            btnBootCustom.Name = "btnBootCustom";
            btnBootCustom.Size = new System.Drawing.Size(340, 52);
            btnBootCustom.Text = "BOOT CUSTOM (NVIDIA-free)";
            btnBootCustom.UseVisualStyleBackColor = false;
            btnBootCustom.Click += btnBootCustom_Click;
            //
            // btnOpenOsc
            //
            btnOpenOsc.BackColor = System.Drawing.Color.FromArgb(150, 100, 10);
            btnOpenOsc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnOpenOsc.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            btnOpenOsc.ForeColor = System.Drawing.Color.White;
            btnOpenOsc.Location = new System.Drawing.Point(728, 52);
            btnOpenOsc.Name = "btnOpenOsc";
            btnOpenOsc.Size = new System.Drawing.Size(280, 52);
            btnOpenOsc.Text = "OPEN OSC";
            btnOpenOsc.UseVisualStyleBackColor = false;
            btnOpenOsc.Click += btnOpenOsc_Click;
            //
            // oscStatusLabel
            //
            oscStatusLabel.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            oscStatusLabel.ForeColor = System.Drawing.Color.FromArgb(255, 179, 0);
            oscStatusLabel.Location = new System.Drawing.Point(16, 118);
            oscStatusLabel.Name = "oscStatusLabel";
            oscStatusLabel.Size = new System.Drawing.Size(1100, 26);
            oscStatusLabel.Text = "mode: —";
            //
            // oscLogBox
            //
            oscLogBox.BackColor = System.Drawing.Color.FromArgb(26, 26, 30);
            oscLogBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            oscLogBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            oscLogBox.Font = new System.Drawing.Font("Consolas", 10F);
            oscLogBox.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
            oscLogBox.Location = new System.Drawing.Point(0, 142);
            oscLogBox.Multiline = true;
            oscLogBox.Name = "oscLogBox";
            oscLogBox.ReadOnly = true;
            oscLogBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            oscLogBox.Size = new System.Drawing.Size(1172, 430);
            //
            // tabTree
            //
            tabTree.BackColor = System.Drawing.Color.FromArgb(18, 18, 20);
            tabTree.Controls.Add(splitContainer);
            tabTree.Location = new System.Drawing.Point(4, 24);
            tabTree.Name = "tabTree";
            tabTree.Size = new System.Drawing.Size(1172, 574);
            tabTree.TabIndex = 1;
            tabTree.Text = "System Tree";
            // 
            // splitContainer
            // 
            splitContainer.BackColor = System.Drawing.Color.FromArgb(45, 45, 50);
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 0);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(treeView);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(detailLabel);
            splitContainer.Panel2.Controls.Add(detailBox);
            splitContainer.Size = new System.Drawing.Size(1172, 574);
            splitContainer.SplitterDistance = 700;
            splitContainer.TabIndex = 0;
            // 
            // treeView
            // 
            treeView.BackColor = System.Drawing.Color.FromArgb(18, 18, 20);
            treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            treeView.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
            treeView.Indent = 30;
            treeView.ItemHeight = 26;
            treeView.Location = new System.Drawing.Point(0, 0);
            treeView.Name = "treeView";
            treeView.ShowLines = false;
            treeView.Size = new System.Drawing.Size(700, 574);
            treeView.TabIndex = 0;
            treeView.DrawNode += treeView_DrawNode;
            treeView.AfterSelect += treeView_AfterSelect;
            // 
            // detailLabel
            // 
            detailLabel.BackColor = System.Drawing.Color.FromArgb(26, 26, 30);
            detailLabel.Dock = System.Windows.Forms.DockStyle.Top;
            detailLabel.Font = new System.Drawing.Font("Segoe UI", 10.5F, System.Drawing.FontStyle.Bold);
            detailLabel.ForeColor = System.Drawing.Color.FromArgb(118, 185, 0);
            detailLabel.Location = new System.Drawing.Point(0, 0);
            detailLabel.Name = "detailLabel";
            detailLabel.Padding = new System.Windows.Forms.Padding(8, 4, 0, 0);
            detailLabel.Size = new System.Drawing.Size(468, 34);
            detailLabel.TabIndex = 0;
            detailLabel.Text = "  Details";
            // 
            // detailBox
            // 
            detailBox.BackColor = System.Drawing.Color.FromArgb(26, 26, 30);
            detailBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            detailBox.Dock = System.Windows.Forms.DockStyle.Fill;
            detailBox.Font = new System.Drawing.Font("Consolas", 10.5F);
            detailBox.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
            detailBox.Location = new System.Drawing.Point(0, 0);
            detailBox.Multiline = true;
            detailBox.Name = "detailBox";
            detailBox.ReadOnly = true;
            detailBox.Size = new System.Drawing.Size(468, 574);
            detailBox.TabIndex = 1;
            // 
            // serverUrlTextLabel
            // 
            serverUrlTextLabel.AutoSize = true;
            serverUrlTextLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            serverUrlTextLabel.ForeColor = System.Drawing.Color.FromArgb(150, 150, 155);
            serverUrlTextLabel.Location = new System.Drawing.Point(796, 667);
            serverUrlTextLabel.Name = "serverUrlTextLabel";
            serverUrlTextLabel.Size = new System.Drawing.Size(69, 17);
            serverUrlTextLabel.TabIndex = 3;
            serverUrlTextLabel.Text = "API server:";
            // 
            // serverUrlText
            // 
            serverUrlText.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            serverUrlText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            serverUrlText.Font = new System.Drawing.Font("Consolas", 10F);
            serverUrlText.ForeColor = System.Drawing.Color.FromArgb(224, 224, 224);
            serverUrlText.Location = new System.Drawing.Point(871, 668);
            serverUrlText.Name = "serverUrlText";
            serverUrlText.Size = new System.Drawing.Size(297, 16);
            serverUrlText.TabIndex = 4;
            serverUrlText.Text = "http://scotcsduluka.totddns.com:15246";
            serverUrlText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // statusBar
            // 
            statusBar.BackColor = System.Drawing.SystemColors.Control;
            statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            statusBar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            statusBar.ForeColor = System.Drawing.Color.Black;
            statusBar.Location = new System.Drawing.Point(0, 660);
            statusBar.Name = "statusBar";
            statusBar.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            statusBar.Size = new System.Drawing.Size(1180, 30);
            statusBar.TabIndex = 1;
            statusBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MainForm
            // 
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            BackColor = System.Drawing.SystemColors.Control;
            ClientSize = new System.Drawing.Size(1180, 690);
            Controls.Add(serverUrlTextLabel);
            Controls.Add(serverUrlText);
            Controls.Add(tabControl);
            Controls.Add(statusBar);
            Controls.Add(headerPanel);
            Font = new System.Drawing.Font("Segoe UI", 10F);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(980, 560);
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "NVIDIA Plugins";
            headerPanel.ResumeLayout(false);
            headerPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            tabControl.ResumeLayout(false);
            tabDownload.ResumeLayout(false);
            tabDownload.PerformLayout();
            tabOsc.ResumeLayout(false);
            tabOsc.PerformLayout();
            heroPanel.ResumeLayout(false);
            heroPanel.PerformLayout();
            tabTree.ResumeLayout(false);
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
