<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Base_KeySet
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_KeySet))
        keyset = New Panel()
        text_settings = New Label()
        lbl_Hint = New Label()
        header_strip = New PictureBox()
        bar_General = New Panel()
        lblCat_General = New Label()
        row_ToggleOverlay = New Panel()
        Desc_ToggleOverlay = New Label()
        lbl_ToggleOverlay = New Label()
        row_TestNotifier = New Panel()
        Desc_Test = New Label()
        lbl_TestNotifier = New Label()
        row_WebToggle = New Panel()
        Desc_Empty = New Label()
        lbl_WebToggle = New Label()
        bar_Record = New Panel()
        lblCat_Record = New Label()
        row_ManualRecordToggle = New Panel()
        Desc_ManualRecordToggle = New Label()
        lbl_ManualRecordToggle = New Label()
        row_InstantReplayToggle = New Panel()
        Desc_InstantReplayToggle = New Label()
        lbl_InstantReplayToggle = New Label()
        row_InstantReplaySave = New Panel()
        Desc_InstantReplaySave = New Label()
        lbl_InstantReplaySave = New Label()
        bar_Capture = New Panel()
        lblCat_Capture = New Label()
        row_Screenshot = New Panel()
        Desc_Screenshot = New Label()
        lbl_Screenshot = New Label()
        row_PhotosToggle = New Panel()
        Desc_PhotosToggle = New Label()
        lbl_PhotosToggle = New Label()
        row_GameFilterToggle = New Panel()
        Desc_GameFilterToggle = New Label()
        lbl_GameFilterToggle = New Label()
        bar_Broadcast = New Panel()
        lblCat_Broadcast = New Label()
        row_BroadcastToggle = New Panel()
        Desc_BroadcastToggle = New Label()
        lbl_BroadcastToggle = New Label()
        lbl_Note = New Label()
        settings_top = New PictureBox()
        action_fn = New Label()
        Reset = New Label()
        keyset.SuspendLayout()
        CType(header_strip, ComponentModel.ISupportInitialize).BeginInit()
        row_ToggleOverlay.SuspendLayout()
        row_TestNotifier.SuspendLayout()
        row_WebToggle.SuspendLayout()
        row_ManualRecordToggle.SuspendLayout()
        row_InstantReplayToggle.SuspendLayout()
        row_InstantReplaySave.SuspendLayout()
        row_Screenshot.SuspendLayout()
        row_PhotosToggle.SuspendLayout()
        row_GameFilterToggle.SuspendLayout()
        row_BroadcastToggle.SuspendLayout()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()

        keyset.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        keyset.AutoScroll = True
        keyset.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        keyset.Controls.Add(text_settings)
        keyset.Controls.Add(lbl_Hint)
        keyset.Controls.Add(header_strip)
        keyset.Controls.Add(bar_General)
        keyset.Controls.Add(lblCat_General)
        keyset.Controls.Add(row_ToggleOverlay)
        keyset.Controls.Add(row_TestNotifier)
        keyset.Controls.Add(row_WebToggle)
        keyset.Controls.Add(bar_Record)
        keyset.Controls.Add(lblCat_Record)
        keyset.Controls.Add(row_ManualRecordToggle)
        keyset.Controls.Add(row_InstantReplayToggle)
        keyset.Controls.Add(row_InstantReplaySave)
        keyset.Controls.Add(bar_Capture)
        keyset.Controls.Add(lblCat_Capture)
        keyset.Controls.Add(row_Screenshot)
        keyset.Controls.Add(row_PhotosToggle)
        keyset.Controls.Add(row_GameFilterToggle)
        keyset.Controls.Add(bar_Broadcast)
        keyset.Controls.Add(lblCat_Broadcast)
        keyset.Controls.Add(row_BroadcastToggle)
        keyset.Controls.Add(lbl_Note)
        keyset.Location = New Point(80, 160)
        keyset.Name = "keyset"
        keyset.Size = New Size(1467, 711)
        keyset.TabIndex = 3

        text_settings.AutoSize = True
        text_settings.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        text_settings.Font = New Font("GeForce", 24F, FontStyle.Bold)
        text_settings.ForeColor = Color.White
        text_settings.Location = New Point(32, 28)
        text_settings.Name = "text_settings"
        text_settings.Size = New Size(272, 42)
        text_settings.TabIndex = 0
        text_settings.Text = "Keyboard Shortcut"

        lbl_Hint.AutoSize = True
        lbl_Hint.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lbl_Hint.Font = New Font("Segoe UI", 9F)
        lbl_Hint.ForeColor = Color.FromArgb(CByte(146), CByte(152), CByte(158))
        lbl_Hint.Location = New Point(44, 70)
        lbl_Hint.Name = "lbl_Hint"
        lbl_Hint.Size = New Size(197, 15)
        lbl_Hint.TabIndex = 1
        lbl_Hint.Text = "Click a key to rebind  |  Esc to cancel"

        header_strip.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        header_strip.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        header_strip.Location = New Point(0, 0)
        header_strip.Name = "header_strip"
        header_strip.Size = New Size(1496, 100)
        header_strip.TabIndex = 2
        header_strip.TabStop = False

        bar_General.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bar_General.Location = New Point(28, 142)
        bar_General.Name = "bar_General"
        bar_General.Size = New Size(4, 22)
        bar_General.TabIndex = 3

        lblCat_General.AutoSize = True
        lblCat_General.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        lblCat_General.Font = New Font("Segoe UI", 12.5F, FontStyle.Bold)
        lblCat_General.ForeColor = Color.White
        lblCat_General.Location = New Point(44, 138)
        lblCat_General.Name = "lblCat_General"
        lblCat_General.Size = New Size(71, 23)
        lblCat_General.TabIndex = 4
        lblCat_General.Text = "General"

        row_ToggleOverlay.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_ToggleOverlay.Controls.Add(Desc_ToggleOverlay)
        row_ToggleOverlay.Controls.Add(lbl_ToggleOverlay)
        row_ToggleOverlay.Cursor = Cursors.Hand
        row_ToggleOverlay.Location = New Point(28, 180)
        row_ToggleOverlay.Name = "row_ToggleOverlay"
        row_ToggleOverlay.Size = New Size(690, 56)
        row_ToggleOverlay.TabIndex = 5

        Desc_ToggleOverlay.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_ToggleOverlay.Font = New Font("Segoe UI", 10F)
        Desc_ToggleOverlay.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_ToggleOverlay.Location = New Point(16, 8)
        Desc_ToggleOverlay.Name = "Desc_ToggleOverlay"
        Desc_ToggleOverlay.Size = New Size(480, 40)
        Desc_ToggleOverlay.TabIndex = 0
        Desc_ToggleOverlay.Text = "Open/close share overlay"
        Desc_ToggleOverlay.TextAlign = ContentAlignment.MiddleLeft

        lbl_ToggleOverlay.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_ToggleOverlay.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_ToggleOverlay.Cursor = Cursors.Hand
        lbl_ToggleOverlay.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_ToggleOverlay.ForeColor = Color.White
        lbl_ToggleOverlay.Location = New Point(506, 8)
        lbl_ToggleOverlay.Name = "lbl_ToggleOverlay"
        lbl_ToggleOverlay.Size = New Size(170, 40)
        lbl_ToggleOverlay.TabIndex = 1
        lbl_ToggleOverlay.Text = "Alt+Z"
        lbl_ToggleOverlay.TextAlign = ContentAlignment.MiddleCenter

        row_TestNotifier.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_TestNotifier.Controls.Add(Desc_Test)
        row_TestNotifier.Controls.Add(lbl_TestNotifier)
        row_TestNotifier.Cursor = Cursors.Hand
        row_TestNotifier.Location = New Point(28, 244)
        row_TestNotifier.Name = "row_TestNotifier"
        row_TestNotifier.Size = New Size(690, 56)
        row_TestNotifier.TabIndex = 6

        Desc_Test.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_Test.Font = New Font("Segoe UI", 10F)
        Desc_Test.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_Test.Location = New Point(16, 8)
        Desc_Test.Name = "Desc_Test"
        Desc_Test.Size = New Size(480, 40)
        Desc_Test.TabIndex = 0
        Desc_Test.Text = "Test notifier"
        Desc_Test.TextAlign = ContentAlignment.MiddleLeft

        lbl_TestNotifier.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_TestNotifier.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_TestNotifier.Cursor = Cursors.Hand
        lbl_TestNotifier.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_TestNotifier.ForeColor = Color.White
        lbl_TestNotifier.Location = New Point(506, 8)
        lbl_TestNotifier.Name = "lbl_TestNotifier"
        lbl_TestNotifier.Size = New Size(170, 40)
        lbl_TestNotifier.TabIndex = 1
        lbl_TestNotifier.Text = "Alt+T"
        lbl_TestNotifier.TextAlign = ContentAlignment.MiddleCenter

        row_WebToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_WebToggle.Controls.Add(Desc_Empty)
        row_WebToggle.Controls.Add(lbl_WebToggle)
        row_WebToggle.Cursor = Cursors.Hand
        row_WebToggle.Location = New Point(28, 308)
        row_WebToggle.Name = "row_WebToggle"
        row_WebToggle.Size = New Size(690, 56)
        row_WebToggle.TabIndex = 7

        Desc_Empty.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_Empty.Font = New Font("Segoe UI", 10F)
        Desc_Empty.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_Empty.Location = New Point(16, 8)
        Desc_Empty.Name = "Desc_Empty"
        Desc_Empty.Size = New Size(480, 40)
        Desc_Empty.TabIndex = 0
        Desc_Empty.TextAlign = ContentAlignment.MiddleLeft

        lbl_WebToggle.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_WebToggle.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_WebToggle.Cursor = Cursors.Hand
        lbl_WebToggle.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_WebToggle.ForeColor = Color.White
        lbl_WebToggle.Location = New Point(506, 8)
        lbl_WebToggle.Name = "lbl_WebToggle"
        lbl_WebToggle.Size = New Size(170, 40)
        lbl_WebToggle.TabIndex = 1
        lbl_WebToggle.Text = "Alt+W"
        lbl_WebToggle.TextAlign = ContentAlignment.MiddleCenter

        bar_Record.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bar_Record.Location = New Point(748, 142)
        bar_Record.Name = "bar_Record"
        bar_Record.Size = New Size(4, 22)
        bar_Record.TabIndex = 8

        lblCat_Record.AutoSize = True
        lblCat_Record.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        lblCat_Record.Font = New Font("Segoe UI", 12.5F, FontStyle.Bold)
        lblCat_Record.ForeColor = Color.White
        lblCat_Record.Location = New Point(764, 138)
        lblCat_Record.Name = "lblCat_Record"
        lblCat_Record.Size = New Size(66, 23)
        lblCat_Record.TabIndex = 9
        lblCat_Record.Text = "Record"

        row_ManualRecordToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_ManualRecordToggle.Controls.Add(Desc_ManualRecordToggle)
        row_ManualRecordToggle.Controls.Add(lbl_ManualRecordToggle)
        row_ManualRecordToggle.Cursor = Cursors.Hand
        row_ManualRecordToggle.Location = New Point(748, 180)
        row_ManualRecordToggle.Name = "row_ManualRecordToggle"
        row_ManualRecordToggle.Size = New Size(690, 56)
        row_ManualRecordToggle.TabIndex = 10

        Desc_ManualRecordToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_ManualRecordToggle.Font = New Font("Segoe UI", 10F)
        Desc_ManualRecordToggle.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_ManualRecordToggle.Location = New Point(16, 8)
        Desc_ManualRecordToggle.Name = "Desc_ManualRecordToggle"
        Desc_ManualRecordToggle.Size = New Size(480, 40)
        Desc_ManualRecordToggle.TabIndex = 0
        Desc_ManualRecordToggle.Text = "Toggle recording"
        Desc_ManualRecordToggle.TextAlign = ContentAlignment.MiddleLeft

        lbl_ManualRecordToggle.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_ManualRecordToggle.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_ManualRecordToggle.Cursor = Cursors.Hand
        lbl_ManualRecordToggle.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_ManualRecordToggle.ForeColor = Color.White
        lbl_ManualRecordToggle.Location = New Point(506, 8)
        lbl_ManualRecordToggle.Name = "lbl_ManualRecordToggle"
        lbl_ManualRecordToggle.Size = New Size(170, 40)
        lbl_ManualRecordToggle.TabIndex = 1
        lbl_ManualRecordToggle.Text = "Alt+F9"
        lbl_ManualRecordToggle.TextAlign = ContentAlignment.MiddleCenter

        row_InstantReplayToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_InstantReplayToggle.Controls.Add(Desc_InstantReplayToggle)
        row_InstantReplayToggle.Controls.Add(lbl_InstantReplayToggle)
        row_InstantReplayToggle.Cursor = Cursors.Hand
        row_InstantReplayToggle.Location = New Point(748, 244)
        row_InstantReplayToggle.Name = "row_InstantReplayToggle"
        row_InstantReplayToggle.Size = New Size(690, 56)
        row_InstantReplayToggle.TabIndex = 11

        Desc_InstantReplayToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_InstantReplayToggle.Font = New Font("Segoe UI", 10F)
        Desc_InstantReplayToggle.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_InstantReplayToggle.Location = New Point(16, 8)
        Desc_InstantReplayToggle.Name = "Desc_InstantReplayToggle"
        Desc_InstantReplayToggle.Size = New Size(480, 40)
        Desc_InstantReplayToggle.TabIndex = 0
        Desc_InstantReplayToggle.Text = "Toggle Instant Replay"
        Desc_InstantReplayToggle.TextAlign = ContentAlignment.MiddleLeft

        lbl_InstantReplayToggle.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_InstantReplayToggle.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_InstantReplayToggle.Cursor = Cursors.Hand
        lbl_InstantReplayToggle.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_InstantReplayToggle.ForeColor = Color.White
        lbl_InstantReplayToggle.Location = New Point(506, 8)
        lbl_InstantReplayToggle.Name = "lbl_InstantReplayToggle"
        lbl_InstantReplayToggle.Size = New Size(170, 40)
        lbl_InstantReplayToggle.TabIndex = 1
        lbl_InstantReplayToggle.Text = "Alt+Shift+F10"
        lbl_InstantReplayToggle.TextAlign = ContentAlignment.MiddleCenter

        row_InstantReplaySave.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_InstantReplaySave.Controls.Add(Desc_InstantReplaySave)
        row_InstantReplaySave.Controls.Add(lbl_InstantReplaySave)
        row_InstantReplaySave.Cursor = Cursors.Hand
        row_InstantReplaySave.Location = New Point(748, 308)
        row_InstantReplaySave.Name = "row_InstantReplaySave"
        row_InstantReplaySave.Size = New Size(690, 56)
        row_InstantReplaySave.TabIndex = 12

        Desc_InstantReplaySave.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_InstantReplaySave.Font = New Font("Segoe UI", 10F)
        Desc_InstantReplaySave.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_InstantReplaySave.Location = New Point(16, 8)
        Desc_InstantReplaySave.Name = "Desc_InstantReplaySave"
        Desc_InstantReplaySave.Size = New Size(480, 40)
        Desc_InstantReplaySave.TabIndex = 0
        Desc_InstantReplaySave.Text = "Save last N minutes"
        Desc_InstantReplaySave.TextAlign = ContentAlignment.MiddleLeft

        lbl_InstantReplaySave.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_InstantReplaySave.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_InstantReplaySave.Cursor = Cursors.Hand
        lbl_InstantReplaySave.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_InstantReplaySave.ForeColor = Color.White
        lbl_InstantReplaySave.Location = New Point(506, 8)
        lbl_InstantReplaySave.Name = "lbl_InstantReplaySave"
        lbl_InstantReplaySave.Size = New Size(170, 40)
        lbl_InstantReplaySave.TabIndex = 1
        lbl_InstantReplaySave.Text = "Alt+F10"
        lbl_InstantReplaySave.TextAlign = ContentAlignment.MiddleCenter

        bar_Capture.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bar_Capture.Location = New Point(28, 410)
        bar_Capture.Name = "bar_Capture"
        bar_Capture.Size = New Size(4, 22)
        bar_Capture.TabIndex = 13

        lblCat_Capture.AutoSize = True
        lblCat_Capture.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        lblCat_Capture.Font = New Font("Segoe UI", 12.5F, FontStyle.Bold)
        lblCat_Capture.ForeColor = Color.White
        lblCat_Capture.Location = New Point(44, 406)
        lblCat_Capture.Name = "lblCat_Capture"
        lblCat_Capture.Size = New Size(74, 23)
        lblCat_Capture.TabIndex = 14
        lblCat_Capture.Text = "Capture"

        row_Screenshot.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_Screenshot.Controls.Add(Desc_Screenshot)
        row_Screenshot.Controls.Add(lbl_Screenshot)
        row_Screenshot.Cursor = Cursors.Hand
        row_Screenshot.Location = New Point(28, 448)
        row_Screenshot.Name = "row_Screenshot"
        row_Screenshot.Size = New Size(690, 56)
        row_Screenshot.TabIndex = 15

        Desc_Screenshot.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_Screenshot.Font = New Font("Segoe UI", 10F)
        Desc_Screenshot.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_Screenshot.Location = New Point(16, 8)
        Desc_Screenshot.Name = "Desc_Screenshot"
        Desc_Screenshot.Size = New Size(480, 40)
        Desc_Screenshot.TabIndex = 0
        Desc_Screenshot.Text = "Save screenshot"
        Desc_Screenshot.TextAlign = ContentAlignment.MiddleLeft

        lbl_Screenshot.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_Screenshot.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_Screenshot.Cursor = Cursors.Hand
        lbl_Screenshot.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_Screenshot.ForeColor = Color.White
        lbl_Screenshot.Location = New Point(506, 8)
        lbl_Screenshot.Name = "lbl_Screenshot"
        lbl_Screenshot.Size = New Size(170, 40)
        lbl_Screenshot.TabIndex = 1
        lbl_Screenshot.Text = "Alt+F1"
        lbl_Screenshot.TextAlign = ContentAlignment.MiddleCenter

        row_PhotosToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_PhotosToggle.Controls.Add(Desc_PhotosToggle)
        row_PhotosToggle.Controls.Add(lbl_PhotosToggle)
        row_PhotosToggle.Cursor = Cursors.Hand
        row_PhotosToggle.Location = New Point(28, 512)
        row_PhotosToggle.Name = "row_PhotosToggle"
        row_PhotosToggle.Size = New Size(690, 56)
        row_PhotosToggle.TabIndex = 16

        Desc_PhotosToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_PhotosToggle.Font = New Font("Segoe UI", 10F)
        Desc_PhotosToggle.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_PhotosToggle.Location = New Point(16, 8)
        Desc_PhotosToggle.Name = "Desc_PhotosToggle"
        Desc_PhotosToggle.Size = New Size(480, 40)
        Desc_PhotosToggle.TabIndex = 0
        Desc_PhotosToggle.Text = "Open/close photo mode"
        Desc_PhotosToggle.TextAlign = ContentAlignment.MiddleLeft

        lbl_PhotosToggle.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_PhotosToggle.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_PhotosToggle.Cursor = Cursors.Hand
        lbl_PhotosToggle.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_PhotosToggle.ForeColor = Color.White
        lbl_PhotosToggle.Location = New Point(506, 8)
        lbl_PhotosToggle.Name = "lbl_PhotosToggle"
        lbl_PhotosToggle.Size = New Size(170, 40)
        lbl_PhotosToggle.TabIndex = 1
        lbl_PhotosToggle.Text = "Alt+F2"
        lbl_PhotosToggle.TextAlign = ContentAlignment.MiddleCenter

        row_GameFilterToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_GameFilterToggle.Controls.Add(Desc_GameFilterToggle)
        row_GameFilterToggle.Controls.Add(lbl_GameFilterToggle)
        row_GameFilterToggle.Cursor = Cursors.Hand
        row_GameFilterToggle.Location = New Point(28, 576)
        row_GameFilterToggle.Name = "row_GameFilterToggle"
        row_GameFilterToggle.Size = New Size(690, 56)
        row_GameFilterToggle.TabIndex = 17

        Desc_GameFilterToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_GameFilterToggle.Font = New Font("Segoe UI", 10F)
        Desc_GameFilterToggle.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_GameFilterToggle.Location = New Point(16, 8)
        Desc_GameFilterToggle.Name = "Desc_GameFilterToggle"
        Desc_GameFilterToggle.Size = New Size(480, 40)
        Desc_GameFilterToggle.TabIndex = 0
        Desc_GameFilterToggle.Text = "Toggle mods"
        Desc_GameFilterToggle.TextAlign = ContentAlignment.MiddleLeft

        lbl_GameFilterToggle.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_GameFilterToggle.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_GameFilterToggle.Cursor = Cursors.Hand
        lbl_GameFilterToggle.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_GameFilterToggle.ForeColor = Color.White
        lbl_GameFilterToggle.Location = New Point(506, 8)
        lbl_GameFilterToggle.Name = "lbl_GameFilterToggle"
        lbl_GameFilterToggle.Size = New Size(170, 40)
        lbl_GameFilterToggle.TabIndex = 1
        lbl_GameFilterToggle.Text = "Alt+F3"
        lbl_GameFilterToggle.TextAlign = ContentAlignment.MiddleCenter

        bar_Broadcast.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        bar_Broadcast.Location = New Point(748, 410)
        bar_Broadcast.Name = "bar_Broadcast"
        bar_Broadcast.Size = New Size(4, 22)
        bar_Broadcast.TabIndex = 18

        lblCat_Broadcast.AutoSize = True
        lblCat_Broadcast.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        lblCat_Broadcast.Font = New Font("Segoe UI", 12.5F, FontStyle.Bold)
        lblCat_Broadcast.ForeColor = Color.White
        lblCat_Broadcast.Location = New Point(764, 406)
        lblCat_Broadcast.Name = "lblCat_Broadcast"
        lblCat_Broadcast.Size = New Size(89, 23)
        lblCat_Broadcast.TabIndex = 19
        lblCat_Broadcast.Text = "Broadcast"

        row_BroadcastToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        row_BroadcastToggle.Controls.Add(Desc_BroadcastToggle)
        row_BroadcastToggle.Controls.Add(lbl_BroadcastToggle)
        row_BroadcastToggle.Cursor = Cursors.Hand
        row_BroadcastToggle.Location = New Point(748, 448)
        row_BroadcastToggle.Name = "row_BroadcastToggle"
        row_BroadcastToggle.Size = New Size(690, 56)
        row_BroadcastToggle.TabIndex = 20

        Desc_BroadcastToggle.BackColor = Color.FromArgb(CByte(37), CByte(40), CByte(44))
        Desc_BroadcastToggle.Font = New Font("Segoe UI", 10F)
        Desc_BroadcastToggle.ForeColor = Color.FromArgb(CByte(208), CByte(214), CByte(220))
        Desc_BroadcastToggle.Location = New Point(16, 8)
        Desc_BroadcastToggle.Name = "Desc_BroadcastToggle"
        Desc_BroadcastToggle.Size = New Size(480, 40)
        Desc_BroadcastToggle.TabIndex = 0
        Desc_BroadcastToggle.Text = "Open/close broadcasting"
        Desc_BroadcastToggle.TextAlign = ContentAlignment.MiddleLeft

        lbl_BroadcastToggle.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lbl_BroadcastToggle.BackColor = Color.FromArgb(CByte(55), CByte(60), CByte(65))
        lbl_BroadcastToggle.Cursor = Cursors.Hand
        lbl_BroadcastToggle.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        lbl_BroadcastToggle.ForeColor = Color.White
        lbl_BroadcastToggle.Location = New Point(506, 8)
        lbl_BroadcastToggle.Name = "lbl_BroadcastToggle"
        lbl_BroadcastToggle.Size = New Size(170, 40)
        lbl_BroadcastToggle.TabIndex = 1
        lbl_BroadcastToggle.Text = "Alt+F8"
        lbl_BroadcastToggle.TextAlign = ContentAlignment.MiddleCenter

        lbl_Note.AutoSize = True
        lbl_Note.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        lbl_Note.Font = New Font("Segoe UI", 9F)
        lbl_Note.ForeColor = Color.FromArgb(CByte(122), CByte(128), CByte(134))
        lbl_Note.Location = New Point(764, 514)
        lbl_Note.Name = "lbl_Note"
        lbl_Note.Size = New Size(246, 15)
        lbl_Note.TabIndex = 21
        lbl_Note.Text = "Each action needs a unique key combination."

        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(80, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(1467, 5)
        settings_top.TabIndex = 2
        settings_top.TabStop = False

        action_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(80, 110)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 50)
        action_fn.TabIndex = 0
        action_fn.Text = "Done"
        action_fn.TextAlign = ContentAlignment.MiddleCenter

        Reset.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Reset.Cursor = Cursors.Hand
        Reset.Font = New Font("Segoe UI", 11F)
        Reset.ForeColor = Color.FromArgb(CByte(220), CByte(224), CByte(228))
        Reset.Location = New Point(286, 115)
        Reset.Name = "Reset"
        Reset.Size = New Size(200, 50)
        Reset.TabIndex = 1
        Reset.Text = "Reset"
        Reset.TextAlign = ContentAlignment.MiddleCenter

        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.Red
        ClientSize = New Size(1627, 951)
        Controls.Add(action_fn)
        Controls.Add(Reset)
        Controls.Add(settings_top)
        Controls.Add(keyset)
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_KeySet"
        ShowInTaskbar = False
        Text = "Keyboard Shortcut"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        keyset.ResumeLayout(False)
        keyset.PerformLayout()
        CType(header_strip, ComponentModel.ISupportInitialize).EndInit()
        row_ToggleOverlay.ResumeLayout(False)
        row_TestNotifier.ResumeLayout(False)
        row_WebToggle.ResumeLayout(False)
        row_ManualRecordToggle.ResumeLayout(False)
        row_InstantReplayToggle.ResumeLayout(False)
        row_InstantReplaySave.ResumeLayout(False)
        row_Screenshot.ResumeLayout(False)
        row_PhotosToggle.ResumeLayout(False)
        row_GameFilterToggle.ResumeLayout(False)
        row_BroadcastToggle.ResumeLayout(False)
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents keyset As Panel
    Friend WithEvents header_strip As PictureBox
    Friend WithEvents text_settings As Label
    Friend WithEvents lbl_Hint As Label
    Friend WithEvents bar_General As Panel
    Friend WithEvents lblCat_General As Label
    Friend WithEvents row_ToggleOverlay As Panel
    Friend WithEvents Desc_ToggleOverlay As Label
    Friend WithEvents lbl_ToggleOverlay As Label
    Friend WithEvents row_TestNotifier As Panel
    Friend WithEvents Desc_Test As Label
    Friend WithEvents lbl_TestNotifier As Label
    Friend WithEvents row_WebToggle As Panel
    Friend WithEvents Desc_Empty As Label
    Friend WithEvents lbl_WebToggle As Label
    Friend WithEvents bar_Record As Panel
    Friend WithEvents lblCat_Record As Label
    Friend WithEvents row_ManualRecordToggle As Panel
    Friend WithEvents Desc_ManualRecordToggle As Label
    Friend WithEvents lbl_ManualRecordToggle As Label
    Friend WithEvents row_InstantReplayToggle As Panel
    Friend WithEvents Desc_InstantReplayToggle As Label
    Friend WithEvents lbl_InstantReplayToggle As Label
    Friend WithEvents row_InstantReplaySave As Panel
    Friend WithEvents Desc_InstantReplaySave As Label
    Friend WithEvents lbl_InstantReplaySave As Label
    Friend WithEvents bar_Capture As Panel
    Friend WithEvents lblCat_Capture As Label
    Friend WithEvents row_Screenshot As Panel
    Friend WithEvents Desc_Screenshot As Label
    Friend WithEvents lbl_Screenshot As Label
    Friend WithEvents row_PhotosToggle As Panel
    Friend WithEvents Desc_PhotosToggle As Label
    Friend WithEvents lbl_PhotosToggle As Label
    Friend WithEvents row_GameFilterToggle As Panel
    Friend WithEvents Desc_GameFilterToggle As Label
    Friend WithEvents lbl_GameFilterToggle As Label
    Friend WithEvents bar_Broadcast As Panel
    Friend WithEvents lblCat_Broadcast As Label
    Friend WithEvents row_BroadcastToggle As Panel
    Friend WithEvents Desc_BroadcastToggle As Label
    Friend WithEvents lbl_BroadcastToggle As Label
    Friend WithEvents lbl_Note As Label
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents action_fn As Label
    Friend WithEvents Reset As Label
End Class
