<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class API_RUN
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(API_RUN))
        Load_APP = New Timer(components)
        lstLog = New ListBox()
        lblStatus = New GroupBox()
        lblUptime = New Label()
        lstClients = New ListBox()
        GroupBox2 = New GroupBox()
        lblClientsOnline = New Label()
        lblMessages = New Label()
        notifyIcon = New NotifyIcon(components)
        lblStatus.SuspendLayout()
        GroupBox2.SuspendLayout()
        SuspendLayout()
        ' 
        ' Load_APP
        ' 
        Load_APP.Enabled = True
        Load_APP.Interval = 1000
        ' 
        ' lstLog
        ' 
        lstLog.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lstLog.FormattingEnabled = True
        lstLog.IntegralHeight = False
        lstLog.ItemHeight = 15
        lstLog.Location = New Point(12, 30)
        lstLog.Name = "lstLog"
        lstLog.SelectionMode = SelectionMode.MultiExtended
        lstLog.Size = New Size(642, 265)
        lstLog.TabIndex = 1
        ' 
        ' lblStatus
        ' 
        lblStatus.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lblStatus.Controls.Add(lblUptime)
        lblStatus.Location = New Point(12, 12)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(642, 110)
        lblStatus.TabIndex = 2
        lblStatus.TabStop = False
        lblStatus.Text = "Server log"
        ' 
        ' lblUptime
        ' 
        lblUptime.AutoSize = True
        lblUptime.Location = New Point(576, 0)
        lblUptime.Name = "lblUptime"
        lblUptime.Size = New Size(66, 15)
        lblUptime.TabIndex = 5
        lblUptime.Text = "%Uptime%"
        ' 
        ' lstClients
        ' 
        lstClients.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        lstClients.FormattingEnabled = True
        lstClients.IntegralHeight = False
        lstClients.ItemHeight = 15
        lstClients.Location = New Point(12, 319)
        lstClients.Name = "lstClients"
        lstClients.SelectionMode = SelectionMode.MultiExtended
        lstClients.Size = New Size(642, 94)
        lstClients.TabIndex = 3
        ' 
        ' GroupBox2
        ' 
        GroupBox2.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        GroupBox2.Controls.Add(lblClientsOnline)
        GroupBox2.Location = New Point(12, 301)
        GroupBox2.Name = "GroupBox2"
        GroupBox2.Size = New Size(642, 58)
        GroupBox2.TabIndex = 4
        GroupBox2.TabStop = False
        GroupBox2.Text = "Clients Online"
        ' 
        ' lblClientsOnline
        ' 
        lblClientsOnline.AutoSize = True
        lblClientsOnline.Location = New Point(88, 0)
        lblClientsOnline.Name = "lblClientsOnline"
        lblClientsOnline.Size = New Size(98, 15)
        lblClientsOnline.TabIndex = 6
        lblClientsOnline.Text = "%ClientsOnline%"
        ' 
        ' lblMessages
        ' 
        lblMessages.AutoSize = True
        lblMessages.Location = New Point(110, 107)
        lblMessages.Name = "lblMessages"
        lblMessages.Size = New Size(71, 15)
        lblMessages.TabIndex = 7
        lblMessages.Text = "lblMessages"
        ' 
        ' notifyIcon
        ' 
        notifyIcon.Text = "NotifyIcon1"
        notifyIcon.Visible = True
        ' 
        ' API_RUN
        ' 
        AutoScaleMode = AutoScaleMode.None
        AutoSizeMode = AutoSizeMode.GrowAndShrink
        ClientSize = New Size(666, 425)
        Controls.Add(lstClients)
        Controls.Add(GroupBox2)
        Controls.Add(lstLog)
        Controls.Add(lblStatus)
        Controls.Add(lblMessages)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MaximizeBox = False
        Name = "API_RUN"
        Opacity = 0R
        StartPosition = FormStartPosition.CenterScreen
        Text = "API Server"
        TopMost = True
        lblStatus.ResumeLayout(False)
        lblStatus.PerformLayout()
        GroupBox2.ResumeLayout(False)
        GroupBox2.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents Load_APP As Timer
    Friend WithEvents lstLog As ListBox
    Friend WithEvents lblStatus As GroupBox
    Friend WithEvents lstClients As ListBox
    Friend WithEvents GroupBox2 As GroupBox
    Friend WithEvents lblUptime As Label
    Friend WithEvents lblClientsOnline As Label
    Friend WithEvents lblMessages As Label
    Friend WithEvents notifyIcon As NotifyIcon

End Class
