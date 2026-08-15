<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AudioSettingsForm
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()

        Me.lblTitle = New System.Windows.Forms.Label()
        Me.grpTrackMode = New System.Windows.Forms.GroupBox()
        Me.radSingle = New System.Windows.Forms.RadioButton()
        Me.radSeparate = New System.Windows.Forms.RadioButton()
        Me.lblModeHint = New System.Windows.Forms.Label()

        Me.grpSystem = New System.Windows.Forms.GroupBox()
        Me.chkSystem = New System.Windows.Forms.CheckBox()
        Me.trkSystemVol = New System.Windows.Forms.TrackBar()
        Me.lblSystemVol = New System.Windows.Forms.Label()

        Me.grpMic = New System.Windows.Forms.GroupBox()
        Me.chkMic = New System.Windows.Forms.CheckBox()
        Me.cboMic = New System.Windows.Forms.ComboBox()
        Me.lblMicDevice = New System.Windows.Forms.Label()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.trkMicVol = New System.Windows.Forms.TrackBar()
        Me.lblMicVol = New System.Windows.Forms.Label()

        Me.btnApply = New System.Windows.Forms.Button()
        Me.btnCancel = New System.Windows.Forms.Button()
        Me.btnTest = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()

        Me.grpTrackMode.SuspendLayout()
        Me.grpSystem.SuspendLayout()
        CType(Me.trkSystemVol, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpMic.SuspendLayout()
        CType(Me.trkMicVol, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()

        '
        ' lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New System.Drawing.Font("Segoe UI", 14.0F, System.Drawing.FontStyle.Bold)
        Me.lblTitle.Location = New System.Drawing.Point(12, 9)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Size = New System.Drawing.Size(141, 25)
        Me.lblTitle.Text = "Audio Settings"

        '
        ' grpTrackMode
        '
        Me.grpTrackMode.Controls.Add(Me.radSingle)
        Me.grpTrackMode.Controls.Add(Me.radSeparate)
        Me.grpTrackMode.Controls.Add(Me.lblModeHint)
        Me.grpTrackMode.Location = New System.Drawing.Point(12, 45)
        Me.grpTrackMode.Name = "grpTrackMode"
        Me.grpTrackMode.Size = New System.Drawing.Size(488, 95)
        Me.grpTrackMode.TabStop = False
        Me.grpTrackMode.Text = "Audio Track Mode"

        Me.radSingle.AutoSize = True
        Me.radSingle.Location = New System.Drawing.Point(15, 22)
        Me.radSingle.Name = "radSingle"
        Me.radSingle.Text = "Single track (system + mic mixed)"
        Me.radSingle.Checked = True

        Me.radSeparate.AutoSize = True
        Me.radSeparate.Location = New System.Drawing.Point(15, 45)
        Me.radSeparate.Name = "radSeparate"
        Me.radSeparate.Text = "Separate tracks (system + mic split)"

        Me.lblModeHint.AutoSize = True
        Me.lblModeHint.ForeColor = System.Drawing.Color.Gray
        Me.lblModeHint.Location = New System.Drawing.Point(15, 68)
        Me.lblModeHint.Name = "lblModeHint"
        Me.lblModeHint.Size = New System.Drawing.Size(460, 15)
        Me.lblModeHint.Text = "Single: 1 AAC track, mix of both sources.   Separate: 2 AAC tracks in same file."

        '
        ' grpSystem
        '
        Me.grpSystem.Controls.Add(Me.chkSystem)
        Me.grpSystem.Controls.Add(Me.trkSystemVol)
        Me.grpSystem.Controls.Add(Me.lblSystemVol)
        Me.grpSystem.Location = New System.Drawing.Point(12, 150)
        Me.grpSystem.Name = "grpSystem"
        Me.grpSystem.Size = New System.Drawing.Size(488, 90)
        Me.grpSystem.TabStop = False
        Me.grpSystem.Text = "System Audio (loopback)"

        Me.chkSystem.AutoSize = True
        Me.chkSystem.Location = New System.Drawing.Point(15, 22)
        Me.chkSystem.Name = "chkSystem"
        Me.chkSystem.Text = "Capture system audio"
        Me.chkSystem.Checked = True

        Me.trkSystemVol.Location = New System.Drawing.Point(15, 45)
        Me.trkSystemVol.Name = "trkSystemVol"
        Me.trkSystemVol.Size = New System.Drawing.Size(360, 45)
        Me.trkSystemVol.Minimum = 0
        Me.trkSystemVol.Maximum = 150
        Me.trkSystemVol.Value = 100

        Me.lblSystemVol.AutoSize = True
        Me.lblSystemVol.Location = New System.Drawing.Point(385, 50)
        Me.lblSystemVol.Name = "lblSystemVol"
        Me.lblSystemVol.Text = "100%"

        '
        ' grpMic
        '
        Me.grpMic.Controls.Add(Me.chkMic)
        Me.grpMic.Controls.Add(Me.cboMic)
        Me.grpMic.Controls.Add(Me.lblMicDevice)
        Me.grpMic.Controls.Add(Me.btnRefresh)
        Me.grpMic.Controls.Add(Me.trkMicVol)
        Me.grpMic.Controls.Add(Me.lblMicVol)
        Me.grpMic.Location = New System.Drawing.Point(12, 250)
        Me.grpMic.Name = "grpMic"
        Me.grpMic.Size = New System.Drawing.Size(488, 130)
        Me.grpMic.TabStop = False
        Me.grpMic.Text = "Microphone"

        Me.chkMic.AutoSize = True
        Me.chkMic.Location = New System.Drawing.Point(15, 22)
        Me.chkMic.Name = "chkMic"
        Me.chkMic.Text = "Capture microphone"

        Me.lblMicDevice.AutoSize = True
        Me.lblMicDevice.Location = New System.Drawing.Point(15, 48)
        Me.lblMicDevice.Name = "lblMicDevice"
        Me.lblMicDevice.Text = "Device:"

        Me.cboMic.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMic.Location = New System.Drawing.Point(70, 45)
        Me.cboMic.Name = "cboMic"
        Me.cboMic.Size = New System.Drawing.Size(330, 23)

        Me.btnRefresh.Location = New System.Drawing.Point(408, 44)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(70, 25)
        Me.btnRefresh.Text = "Refresh"

        Me.trkMicVol.Location = New System.Drawing.Point(15, 80)
        Me.trkMicVol.Name = "trkMicVol"
        Me.trkMicVol.Size = New System.Drawing.Size(360, 45)
        Me.trkMicVol.Minimum = 0
        Me.trkMicVol.Maximum = 150
        Me.trkMicVol.Value = 100

        Me.lblMicVol.AutoSize = True
        Me.lblMicVol.Location = New System.Drawing.Point(385, 85)
        Me.lblMicVol.Name = "lblMicVol"
        Me.lblMicVol.Text = "100%"

        '
        ' btnApply / btnCancel / btnTest
        '
        Me.btnApply.Location = New System.Drawing.Point(234, 395)
        Me.btnApply.Name = "btnApply"
        Me.btnApply.Size = New System.Drawing.Size(85, 30)
        Me.btnApply.Text = "Apply"
        Me.btnApply.UseVisualStyleBackColor = True

        Me.btnCancel.Location = New System.Drawing.Point(325, 395)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New System.Drawing.Size(85, 30)
        Me.btnCancel.Text = "Cancel"
        Me.btnCancel.UseVisualStyleBackColor = True

        Me.btnTest.Location = New System.Drawing.Point(12, 395)
        Me.btnTest.Name = "btnTest"
        Me.btnTest.Size = New System.Drawing.Size(85, 30)
        Me.btnTest.Text = "Test"
        Me.btnTest.UseVisualStyleBackColor = True

        Me.lblStatus.AutoSize = True
        Me.lblStatus.ForeColor = System.Drawing.Color.Gray
        Me.lblStatus.Location = New System.Drawing.Point(105, 402)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Text = ""

        '
        ' AudioSettingsForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0F, 15.0F)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(516, 437)
        Me.Controls.Add(Me.lblTitle)
        Me.Controls.Add(Me.grpTrackMode)
        Me.Controls.Add(Me.grpSystem)
        Me.Controls.Add(Me.grpMic)
        Me.Controls.Add(Me.btnApply)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnTest)
        Me.Controls.Add(Me.lblStatus)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "AudioSettingsForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Audio Settings — ShadowPlay Engine"
        Me.grpTrackMode.ResumeLayout(False)
        Me.grpTrackMode.PerformLayout()
        Me.grpSystem.ResumeLayout(False)
        Me.grpSystem.PerformLayout()
        CType(Me.trkSystemVol, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpMic.ResumeLayout(False)
        Me.grpMic.PerformLayout()
        CType(Me.trkMicVol, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitle As System.Windows.Forms.Label
    Friend WithEvents grpTrackMode As System.Windows.Forms.GroupBox
    Friend WithEvents radSingle As System.Windows.Forms.RadioButton
    Friend WithEvents radSeparate As System.Windows.Forms.RadioButton
    Friend WithEvents lblModeHint As System.Windows.Forms.Label
    Friend WithEvents grpSystem As System.Windows.Forms.GroupBox
    Friend WithEvents chkSystem As System.Windows.Forms.CheckBox
    Friend WithEvents trkSystemVol As System.Windows.Forms.TrackBar
    Friend WithEvents lblSystemVol As System.Windows.Forms.Label
    Friend WithEvents grpMic As System.Windows.Forms.GroupBox
    Friend WithEvents chkMic As System.Windows.Forms.CheckBox
    Friend WithEvents cboMic As System.Windows.Forms.ComboBox
    Friend WithEvents lblMicDevice As System.Windows.Forms.Label
    Friend WithEvents btnRefresh As System.Windows.Forms.Button
    Friend WithEvents trkMicVol As System.Windows.Forms.TrackBar
    Friend WithEvents lblMicVol As System.Windows.Forms.Label
    Friend WithEvents btnApply As System.Windows.Forms.Button
    Friend WithEvents btnCancel As System.Windows.Forms.Button
    Friend WithEvents btnTest As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
End Class
