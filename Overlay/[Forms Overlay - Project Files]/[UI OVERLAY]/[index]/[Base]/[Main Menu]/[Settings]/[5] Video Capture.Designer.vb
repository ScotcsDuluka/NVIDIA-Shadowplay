<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Base_RecordingsSet
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Base_RecordingsSet))
        ALTZ = New Timer(components)
        setre = New Panel()
        Label4 = New Label()
        Panel_SetVDO = New Panel()
        Panel_SET = New Panel()
        warm_re = New Label()
        lblBitrateRange = New Label()
        PictureBox8 = New PictureBox()
        PictureBox7 = New PictureBox()
        PictureBox4 = New PictureBox()
        PictureBox3 = New PictureBox()
        Button_Copy = New Button()
        Button1 = New Button()
        prearg = New TextBox()
        TextBox1 = New TextBox()
        Label3 = New Label()
        Label14 = New Label()
        TrackBar_Replaylast = New TrackBar()
        lblBitrateValue = New Label()
        TrackBar_BITRATE = New TrackBar()
        Resolution_BOX = New ComboBox()
        P_BOX = New TextBox()
        Label20 = New Label()
        PictureBox5 = New PictureBox()
        Label2 = New Label()
        custom_main = New Label()
        Label19 = New Label()
        advanced_main = New Label()
        Label16 = New Label()
        lblEncoderInfo = New Label()
        cmbEncoder = New ComboBox()
        FPS_BOX = New TextBox()
        Label13 = New Label()
        Encoder_CODE = New Label()
        Label12 = New Label()
        C_R = New PictureBox()
        Label1 = New Label()
        C_L = New PictureBox()
        fps = New TextBox()
        H_R = New PictureBox()
        H_L = New PictureBox()
        M_R = New PictureBox()
        M_L = New PictureBox()
        C_ICO = New Label()
        L_T = New PictureBox()
        L_R = New PictureBox()
        L_B = New PictureBox()
        C_TEXT = New Label()
        M_T = New PictureBox()
        L_L = New PictureBox()
        Label10 = New Label()
        Label7 = New Label()
        M_B = New PictureBox()
        C_T = New PictureBox()
        Label11 = New Label()
        Label6 = New Label()
        H_B = New PictureBox()
        H_T = New PictureBox()
        Label8 = New Label()
        Label9 = New Label()
        C_B = New PictureBox()
        C_BG = New PictureBox()
        PictureBox2 = New PictureBox()
        PictureBox1 = New PictureBox()
        low = New PictureBox()
        PictureBox6 = New PictureBox()
        lbl_BufferDuration = New Label()
        captrueblock_ico = New Label()
        captrueblock = New Label()
        settings_top = New PictureBox()
        text_settings = New Label()
        box_settings = New PictureBox()
        vdo_resetall = New Label()
        action_fn = New Label()
        Quality = New Timer(components)
        PictureBox22 = New PictureBox()
        PictureBox9 = New PictureBox()
        PictureBox10 = New PictureBox()
        setre.SuspendLayout()
        Panel_SetVDO.SuspendLayout()
        Panel_SET.SuspendLayout()
        CType(PictureBox8, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).BeginInit()
        CType(TrackBar_Replaylast, ComponentModel.ISupportInitialize).BeginInit()
        CType(TrackBar_BITRATE, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).BeginInit()
        CType(C_R, ComponentModel.ISupportInitialize).BeginInit()
        CType(C_L, ComponentModel.ISupportInitialize).BeginInit()
        CType(H_R, ComponentModel.ISupportInitialize).BeginInit()
        CType(H_L, ComponentModel.ISupportInitialize).BeginInit()
        CType(M_R, ComponentModel.ISupportInitialize).BeginInit()
        CType(M_L, ComponentModel.ISupportInitialize).BeginInit()
        CType(L_T, ComponentModel.ISupportInitialize).BeginInit()
        CType(L_R, ComponentModel.ISupportInitialize).BeginInit()
        CType(L_B, ComponentModel.ISupportInitialize).BeginInit()
        CType(M_T, ComponentModel.ISupportInitialize).BeginInit()
        CType(L_L, ComponentModel.ISupportInitialize).BeginInit()
        CType(M_B, ComponentModel.ISupportInitialize).BeginInit()
        CType(C_T, ComponentModel.ISupportInitialize).BeginInit()
        CType(H_B, ComponentModel.ISupportInitialize).BeginInit()
        CType(H_T, ComponentModel.ISupportInitialize).BeginInit()
        CType(C_B, ComponentModel.ISupportInitialize).BeginInit()
        CType(C_BG, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).BeginInit()
        CType(low, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).BeginInit()
        CType(settings_top, ComponentModel.ISupportInitialize).BeginInit()
        CType(box_settings, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox22, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).BeginInit()
        CType(PictureBox10, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' ALTZ
        ' 
        ALTZ.Enabled = True
        ' 
        ' setre
        ' 
        setre.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        setre.BackColor = Color.Red
        setre.Controls.Add(Label4)
        setre.Controls.Add(Panel_SetVDO)
        setre.Location = New Point(695, 160)
        setre.Name = "setre"
        setre.Size = New Size(925, 832)
        setre.TabIndex = 44
        ' 
        ' Label4
        ' 
        Label4.AutoSize = True
        Label4.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label4.Font = New Font("Segoe UI", 17F, FontStyle.Bold)
        Label4.ForeColor = Color.White
        Label4.Location = New Point(62, 43)
        Label4.Name = "Label4"
        Label4.Size = New Size(163, 31)
        Label4.TabIndex = 51
        Label4.Text = "Video capture"
        ' 
        ' Panel_SetVDO
        ' 
        Panel_SetVDO.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Panel_SetVDO.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Panel_SetVDO.Controls.Add(Panel_SET)
        Panel_SetVDO.Controls.Add(captrueblock_ico)
        Panel_SetVDO.Controls.Add(captrueblock)
        Panel_SetVDO.Location = New Point(0, 0)
        Panel_SetVDO.Name = "Panel_SetVDO"
        Panel_SetVDO.Size = New Size(925, 832)
        Panel_SetVDO.TabIndex = 110
        ' 
        ' Panel_SET
        ' 
        Panel_SET.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        Panel_SET.AutoScroll = True
        Panel_SET.Controls.Add(warm_re)
        Panel_SET.Controls.Add(lblBitrateRange)
        Panel_SET.Controls.Add(PictureBox8)
        Panel_SET.Controls.Add(PictureBox7)
        Panel_SET.Controls.Add(PictureBox4)
        Panel_SET.Controls.Add(PictureBox3)
        Panel_SET.Controls.Add(Button_Copy)
        Panel_SET.Controls.Add(Button1)
        Panel_SET.Controls.Add(prearg)
        Panel_SET.Controls.Add(TextBox1)
        Panel_SET.Controls.Add(Label3)
        Panel_SET.Controls.Add(Label14)
        Panel_SET.Controls.Add(TrackBar_Replaylast)
        Panel_SET.Controls.Add(lblBitrateValue)
        Panel_SET.Controls.Add(TrackBar_BITRATE)
        Panel_SET.Controls.Add(Resolution_BOX)
        Panel_SET.Controls.Add(P_BOX)
        Panel_SET.Controls.Add(Label20)
        Panel_SET.Controls.Add(PictureBox5)
        Panel_SET.Controls.Add(Label2)
        Panel_SET.Controls.Add(custom_main)
        Panel_SET.Controls.Add(Label19)
        Panel_SET.Controls.Add(advanced_main)
        Panel_SET.Controls.Add(Label16)
        Panel_SET.Controls.Add(lblEncoderInfo)
        Panel_SET.Controls.Add(cmbEncoder)
        Panel_SET.Controls.Add(FPS_BOX)
        Panel_SET.Controls.Add(Label13)
        Panel_SET.Controls.Add(Encoder_CODE)
        Panel_SET.Controls.Add(Label12)
        Panel_SET.Controls.Add(C_R)
        Panel_SET.Controls.Add(Label1)
        Panel_SET.Controls.Add(C_L)
        Panel_SET.Controls.Add(fps)
        Panel_SET.Controls.Add(H_R)
        Panel_SET.Controls.Add(H_L)
        Panel_SET.Controls.Add(M_R)
        Panel_SET.Controls.Add(M_L)
        Panel_SET.Controls.Add(C_ICO)
        Panel_SET.Controls.Add(L_T)
        Panel_SET.Controls.Add(L_R)
        Panel_SET.Controls.Add(L_B)
        Panel_SET.Controls.Add(C_TEXT)
        Panel_SET.Controls.Add(M_T)
        Panel_SET.Controls.Add(L_L)
        Panel_SET.Controls.Add(Label10)
        Panel_SET.Controls.Add(Label7)
        Panel_SET.Controls.Add(M_B)
        Panel_SET.Controls.Add(C_T)
        Panel_SET.Controls.Add(Label11)
        Panel_SET.Controls.Add(Label6)
        Panel_SET.Controls.Add(H_B)
        Panel_SET.Controls.Add(H_T)
        Panel_SET.Controls.Add(Label8)
        Panel_SET.Controls.Add(Label9)
        Panel_SET.Controls.Add(C_B)
        Panel_SET.Controls.Add(C_BG)
        Panel_SET.Controls.Add(PictureBox2)
        Panel_SET.Controls.Add(PictureBox1)
        Panel_SET.Controls.Add(low)
        Panel_SET.Controls.Add(PictureBox6)
        Panel_SET.Controls.Add(lbl_BufferDuration)
        Panel_SET.Location = New Point(62, 73)
        Panel_SET.Name = "Panel_SET"
        Panel_SET.Size = New Size(816, 672)
        Panel_SET.TabIndex = 121
        ' 
        ' warm_re
        ' 
        warm_re.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        warm_re.Font = New Font("Segoe UI", 10F)
        warm_re.ForeColor = Color.Coral
        warm_re.Location = New Point(601, 82)
        warm_re.Name = "warm_re"
        warm_re.Size = New Size(177, 140)
        warm_re.TabIndex = 136
        warm_re.Text = "warm"
        ' 
        ' lblBitrateRange
        ' 
        lblBitrateRange.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblBitrateRange.Font = New Font("Segoe UI Semibold", 10F)
        lblBitrateRange.ForeColor = Color.White
        lblBitrateRange.Location = New Point(360, 284)
        lblBitrateRange.Name = "lblBitrateRange"
        lblBitrateRange.Size = New Size(381, 19)
        lblBitrateRange.TabIndex = 127
        lblBitrateRange.Text = "Bitแนะนำ"
        lblBitrateRange.TextAlign = ContentAlignment.BottomLeft
        ' 
        ' PictureBox8
        ' 
        PictureBox8.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox8.Location = New Point(367, 277)
        PictureBox8.Name = "PictureBox8"
        PictureBox8.Size = New Size(400, 4)
        PictureBox8.TabIndex = 145
        PictureBox8.TabStop = False
        ' 
        ' PictureBox7
        ' 
        PictureBox7.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox7.Location = New Point(367, 249)
        PictureBox7.Name = "PictureBox7"
        PictureBox7.Size = New Size(400, 4)
        PictureBox7.TabIndex = 144
        PictureBox7.TabStop = False
        ' 
        ' PictureBox4
        ' 
        PictureBox4.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox4.Location = New Point(67, 395)
        PictureBox4.Name = "PictureBox4"
        PictureBox4.Size = New Size(690, 4)
        PictureBox4.TabIndex = 143
        PictureBox4.TabStop = False
        ' 
        ' PictureBox3
        ' 
        PictureBox3.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        PictureBox3.Location = New Point(67, 367)
        PictureBox3.Name = "PictureBox3"
        PictureBox3.Size = New Size(690, 4)
        PictureBox3.TabIndex = 142
        PictureBox3.TabStop = False
        ' 
        ' Button_Copy
        ' 
        Button_Copy.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Button_Copy.FlatStyle = FlatStyle.Flat
        Button_Copy.ForeColor = SystemColors.Control
        Button_Copy.Location = New Point(675, 540)
        Button_Copy.Name = "Button_Copy"
        Button_Copy.Size = New Size(77, 112)
        Button_Copy.TabIndex = 141
        Button_Copy.Text = "Copy"
        Button_Copy.UseVisualStyleBackColor = False
        ' 
        ' Button1
        ' 
        Button1.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Button1.FlatStyle = FlatStyle.Flat
        Button1.ForeColor = SystemColors.Control
        Button1.Location = New Point(675, 499)
        Button1.Name = "Button1"
        Button1.Size = New Size(77, 35)
        Button1.TabIndex = 140
        Button1.Text = "Reload"
        Button1.UseVisualStyleBackColor = False
        ' 
        ' prearg
        ' 
        prearg.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        prearg.BorderStyle = BorderStyle.None
        prearg.Enabled = False
        prearg.ForeColor = Color.White
        prearg.Location = New Point(303, 506)
        prearg.Multiline = True
        prearg.Name = "prearg"
        prearg.ReadOnly = True
        prearg.Size = New Size(359, 137)
        prearg.TabIndex = 139
        prearg.Text = "Command"
        ' 
        ' TextBox1
        ' 
        TextBox1.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        TextBox1.BorderStyle = BorderStyle.None
        TextBox1.Enabled = False
        TextBox1.ForeColor = Color.White
        TextBox1.Location = New Point(297, 499)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.Size = New Size(372, 153)
        TextBox1.TabIndex = 138
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label3.Font = New Font("Segoe UI Semibold", 10F)
        Label3.ForeColor = Color.White
        Label3.Location = New Point(297, 477)
        Label3.Name = "Label3"
        Label3.Size = New Size(219, 19)
        Label3.TabIndex = 137
        Label3.Text = "Preview Build FFmpeg Arguments"
        ' 
        ' Label14
        ' 
        Label14.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label14.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label14.ForeColor = Color.White
        Label14.Location = New Point(22, 292)
        Label14.Name = "Label14"
        Label14.Size = New Size(39, 67)
        Label14.TabIndex = 134
        Label14.Text = ""
        Label14.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' TrackBar_Replaylast
        ' 
        TrackBar_Replaylast.LargeChange = 1
        TrackBar_Replaylast.Location = New Point(67, 362)
        TrackBar_Replaylast.Maximum = 1200
        TrackBar_Replaylast.Name = "TrackBar_Replaylast"
        TrackBar_Replaylast.Size = New Size(685, 45)
        TrackBar_Replaylast.TabIndex = 131
        TrackBar_Replaylast.TickFrequency = 15
        TrackBar_Replaylast.TickStyle = TickStyle.Both
        ' 
        ' lblBitrateValue
        ' 
        lblBitrateValue.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lblBitrateValue.Font = New Font("Segoe UI Semibold", 10F)
        lblBitrateValue.ForeColor = Color.White
        lblBitrateValue.Location = New Point(360, 222)
        lblBitrateValue.Name = "lblBitrateValue"
        lblBitrateValue.Size = New Size(242, 19)
        lblBitrateValue.TabIndex = 129
        lblBitrateValue.Text = "Bit rate"
        ' 
        ' TrackBar_BITRATE
        ' 
        TrackBar_BITRATE.Location = New Point(360, 244)
        TrackBar_BITRATE.Name = "TrackBar_BITRATE"
        TrackBar_BITRATE.Size = New Size(392, 45)
        TrackBar_BITRATE.TabIndex = 128
        TrackBar_BITRATE.TickStyle = TickStyle.Both
        ' 
        ' Resolution_BOX
        ' 
        Resolution_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Resolution_BOX.DropDownStyle = ComboBoxStyle.DropDownList
        Resolution_BOX.ForeColor = Color.White
        Resolution_BOX.FormattingEnabled = True
        Resolution_BOX.Location = New Point(73, 547)
        Resolution_BOX.Name = "Resolution_BOX"
        Resolution_BOX.Size = New Size(206, 23)
        Resolution_BOX.TabIndex = 126
        ' 
        ' P_BOX
        ' 
        P_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        P_BOX.BorderStyle = BorderStyle.None
        P_BOX.Font = New Font("nvgcshare", 20F)
        P_BOX.ForeColor = Color.White
        P_BOX.Location = New Point(305, 247)
        P_BOX.MaxLength = 1
        P_BOX.Name = "P_BOX"
        P_BOX.Size = New Size(41, 27)
        P_BOX.TabIndex = 123
        P_BOX.Text = "60"
        P_BOX.TextAlign = HorizontalAlignment.Center
        ' 
        ' Label20
        ' 
        Label20.AutoSize = True
        Label20.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label20.Font = New Font("Segoe UI Semibold", 10F)
        Label20.ForeColor = Color.White
        Label20.Location = New Point(297, 222)
        Label20.Name = "Label20"
        Label20.Size = New Size(50, 19)
        Label20.TabIndex = 121
        Label20.Text = "Preset:"
        ' 
        ' PictureBox5
        ' 
        PictureBox5.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        PictureBox5.Location = New Point(303, 244)
        PictureBox5.Name = "PictureBox5"
        PictureBox5.Size = New Size(44, 37)
        PictureBox5.TabIndex = 122
        PictureBox5.TabStop = False
        ' 
        ' Label2
        ' 
        Label2.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label2.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label2.ForeColor = Color.White
        Label2.Location = New Point(22, 12)
        Label2.Name = "Label2"
        Label2.Size = New Size(39, 67)
        Label2.TabIndex = 110
        Label2.Text = ""
        Label2.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' custom_main
        ' 
        custom_main.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        custom_main.Font = New Font("Segoe UI Semibold", 18F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        custom_main.ForeColor = Color.White
        custom_main.Location = New Point(67, 155)
        custom_main.Name = "custom_main"
        custom_main.Size = New Size(121, 67)
        custom_main.TabIndex = 120
        custom_main.Text = "Custom:"
        custom_main.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label19
        ' 
        Label19.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label19.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label19.ForeColor = Color.White
        Label19.Location = New Point(22, 155)
        Label19.Name = "Label19"
        Label19.Size = New Size(39, 67)
        Label19.TabIndex = 119
        Label19.Text = ""
        Label19.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' advanced_main
        ' 
        advanced_main.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        advanced_main.Font = New Font("Segoe UI Semibold", 18F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        advanced_main.ForeColor = Color.White
        advanced_main.Location = New Point(67, 408)
        advanced_main.Name = "advanced_main"
        advanced_main.Size = New Size(138, 67)
        advanced_main.TabIndex = 118
        advanced_main.Text = "Advanced:"
        advanced_main.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' Label16
        ' 
        Label16.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label16.Font = New Font("nvgcshare", 50F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label16.ForeColor = Color.White
        Label16.Location = New Point(22, 408)
        Label16.Name = "Label16"
        Label16.Size = New Size(39, 67)
        Label16.TabIndex = 117
        Label16.Text = ""
        Label16.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' lblEncoderInfo
        ' 
        lblEncoderInfo.AutoSize = True
        lblEncoderInfo.Font = New Font("Segoe UI Semibold", 9F)
        lblEncoderInfo.ForeColor = Color.White
        lblEncoderInfo.Location = New Point(67, 589)
        lblEncoderInfo.Name = "lblEncoderInfo"
        lblEncoderInfo.Size = New Size(30, 15)
        lblEncoderInfo.TabIndex = 116
        lblEncoderInfo.Text = "GPU"
        ' 
        ' cmbEncoder
        ' 
        cmbEncoder.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        cmbEncoder.DropDownStyle = ComboBoxStyle.DropDownList
        cmbEncoder.ForeColor = Color.White
        cmbEncoder.FormattingEnabled = True
        cmbEncoder.Location = New Point(73, 499)
        cmbEncoder.Name = "cmbEncoder"
        cmbEncoder.Size = New Size(206, 23)
        cmbEncoder.TabIndex = 115
        ' 
        ' FPS_BOX
        ' 
        FPS_BOX.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        FPS_BOX.BorderStyle = BorderStyle.None
        FPS_BOX.Font = New Font("nvgcshare", 20F)
        FPS_BOX.ForeColor = Color.White
        FPS_BOX.Location = New Point(81, 247)
        FPS_BOX.MaxLength = 3
        FPS_BOX.Name = "FPS_BOX"
        FPS_BOX.Size = New Size(185, 27)
        FPS_BOX.TabIndex = 114
        FPS_BOX.Text = "60"
        ' 
        ' Label13
        ' 
        Label13.AutoSize = True
        Label13.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label13.Font = New Font("Segoe UI Semibold", 10F)
        Label13.ForeColor = Color.White
        Label13.Location = New Point(67, 222)
        Label13.Name = "Label13"
        Label13.Size = New Size(78, 19)
        Label13.TabIndex = 86
        Label13.Text = "Frame rate:"
        ' 
        ' Encoder_CODE
        ' 
        Encoder_CODE.AutoSize = True
        Encoder_CODE.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Encoder_CODE.Font = New Font("Segoe UI Semibold", 10F)
        Encoder_CODE.ForeColor = Color.White
        Encoder_CODE.Location = New Point(67, 477)
        Encoder_CODE.Name = "Encoder_CODE"
        Encoder_CODE.Size = New Size(99, 19)
        Encoder_CODE.TabIndex = 111
        Encoder_CODE.Text = "Code Encoder:"
        ' 
        ' Label12
        ' 
        Label12.AutoSize = True
        Label12.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label12.Font = New Font("Segoe UI Semibold", 10F)
        Label12.ForeColor = Color.White
        Label12.Location = New Point(67, 525)
        Label12.Name = "Label12"
        Label12.Size = New Size(79, 19)
        Label12.TabIndex = 84
        Label12.Text = "Resolution:"
        ' 
        ' C_R
        ' 
        C_R.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        C_R.Location = New Point(579, 82)
        C_R.Name = "C_R"
        C_R.Size = New Size(3, 70)
        C_R.TabIndex = 109
        C_R.TabStop = False
        C_R.Visible = False
        ' 
        ' Label1
        ' 
        Label1.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        Label1.Font = New Font("Segoe UI Semibold", 18F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label1.ForeColor = Color.White
        Label1.Location = New Point(67, 12)
        Label1.Name = "Label1"
        Label1.Size = New Size(138, 67)
        Label1.TabIndex = 72
        Label1.Text = "Quality:"
        Label1.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' C_L
        ' 
        C_L.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        C_L.Location = New Point(461, 82)
        C_L.Name = "C_L"
        C_L.Size = New Size(3, 70)
        C_L.TabIndex = 108
        C_L.TabStop = False
        C_L.Visible = False
        ' 
        ' fps
        ' 
        fps.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        fps.BorderStyle = BorderStyle.None
        fps.Enabled = False
        fps.Font = New Font("nvgcshare", 22F)
        fps.ForeColor = Color.Gray
        fps.Location = New Point(80, 247)
        fps.MaxLength = 2
        fps.Name = "fps"
        fps.ReadOnly = True
        fps.Size = New Size(217, 30)
        fps.TabIndex = 90
        fps.TextAlign = HorizontalAlignment.Right
        ' 
        ' H_R
        ' 
        H_R.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        H_R.Location = New Point(439, 82)
        H_R.Name = "H_R"
        H_R.Size = New Size(3, 70)
        H_R.TabIndex = 107
        H_R.TabStop = False
        H_R.Visible = False
        ' 
        ' H_L
        ' 
        H_L.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        H_L.Location = New Point(321, 82)
        H_L.Name = "H_L"
        H_L.Size = New Size(3, 70)
        H_L.TabIndex = 106
        H_L.TabStop = False
        H_L.Visible = False
        ' 
        ' M_R
        ' 
        M_R.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        M_R.Location = New Point(312, 82)
        M_R.Name = "M_R"
        M_R.Size = New Size(3, 70)
        M_R.TabIndex = 105
        M_R.TabStop = False
        M_R.Visible = False
        ' 
        ' M_L
        ' 
        M_L.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        M_L.Location = New Point(194, 82)
        M_L.Name = "M_L"
        M_L.Size = New Size(3, 70)
        M_L.TabIndex = 104
        M_L.TabStop = False
        M_L.Visible = False
        ' 
        ' C_ICO
        ' 
        C_ICO.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        C_ICO.Cursor = Cursors.Hand
        C_ICO.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        C_ICO.ForeColor = Color.White
        C_ICO.Location = New Point(461, 94)
        C_ICO.Name = "C_ICO"
        C_ICO.Size = New Size(121, 27)
        C_ICO.TabIndex = 76
        C_ICO.Text = ""
        C_ICO.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' L_T
        ' 
        L_T.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        L_T.Location = New Point(67, 82)
        L_T.Name = "L_T"
        L_T.Size = New Size(121, 3)
        L_T.TabIndex = 94
        L_T.TabStop = False
        L_T.Visible = False
        ' 
        ' L_R
        ' 
        L_R.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        L_R.Location = New Point(185, 82)
        L_R.Name = "L_R"
        L_R.Size = New Size(3, 70)
        L_R.TabIndex = 103
        L_R.TabStop = False
        L_R.Visible = False
        ' 
        ' L_B
        ' 
        L_B.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        L_B.Location = New Point(67, 149)
        L_B.Name = "L_B"
        L_B.Size = New Size(121, 3)
        L_B.TabIndex = 95
        L_B.TabStop = False
        L_B.Visible = False
        ' 
        ' C_TEXT
        ' 
        C_TEXT.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        C_TEXT.Cursor = Cursors.Hand
        C_TEXT.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        C_TEXT.ForeColor = Color.White
        C_TEXT.Location = New Point(461, 122)
        C_TEXT.Name = "C_TEXT"
        C_TEXT.Size = New Size(121, 19)
        C_TEXT.TabIndex = 77
        C_TEXT.Text = "Custom"
        C_TEXT.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' M_T
        ' 
        M_T.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        M_T.Location = New Point(194, 82)
        M_T.Name = "M_T"
        M_T.Size = New Size(121, 3)
        M_T.TabIndex = 96
        M_T.TabStop = False
        M_T.Visible = False
        ' 
        ' L_L
        ' 
        L_L.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        L_L.Location = New Point(67, 82)
        L_L.Name = "L_L"
        L_L.Size = New Size(3, 70)
        L_L.TabIndex = 102
        L_L.TabStop = False
        L_L.Visible = False
        ' 
        ' Label10
        ' 
        Label10.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Label10.Cursor = Cursors.Hand
        Label10.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        Label10.ForeColor = Color.White
        Label10.Location = New Point(67, 122)
        Label10.Name = "Label10"
        Label10.Size = New Size(121, 19)
        Label10.TabIndex = 83
        Label10.Text = "Low"
        Label10.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label7
        ' 
        Label7.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Label7.Cursor = Cursors.Hand
        Label7.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label7.ForeColor = Color.White
        Label7.Location = New Point(321, 94)
        Label7.Name = "Label7"
        Label7.Size = New Size(121, 27)
        Label7.TabIndex = 78
        Label7.Text = ""
        Label7.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' M_B
        ' 
        M_B.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        M_B.Location = New Point(194, 149)
        M_B.Name = "M_B"
        M_B.Size = New Size(121, 3)
        M_B.TabIndex = 97
        M_B.TabStop = False
        M_B.Visible = False
        ' 
        ' C_T
        ' 
        C_T.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        C_T.Location = New Point(461, 82)
        C_T.Name = "C_T"
        C_T.Size = New Size(121, 3)
        C_T.TabIndex = 101
        C_T.TabStop = False
        C_T.Visible = False
        ' 
        ' Label11
        ' 
        Label11.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Label11.Cursor = Cursors.Hand
        Label11.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label11.ForeColor = Color.White
        Label11.Location = New Point(67, 94)
        Label11.Name = "Label11"
        Label11.Size = New Size(121, 27)
        Label11.TabIndex = 82
        Label11.Text = ""
        Label11.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label6
        ' 
        Label6.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Label6.Cursor = Cursors.Hand
        Label6.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        Label6.ForeColor = Color.White
        Label6.Location = New Point(321, 122)
        Label6.Name = "Label6"
        Label6.Size = New Size(121, 19)
        Label6.TabIndex = 79
        Label6.Text = "High"
        Label6.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' H_B
        ' 
        H_B.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        H_B.Location = New Point(321, 149)
        H_B.Name = "H_B"
        H_B.Size = New Size(121, 3)
        H_B.TabIndex = 98
        H_B.TabStop = False
        H_B.Visible = False
        ' 
        ' H_T
        ' 
        H_T.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        H_T.Location = New Point(321, 82)
        H_T.Name = "H_T"
        H_T.Size = New Size(121, 3)
        H_T.TabIndex = 100
        H_T.TabStop = False
        H_T.Visible = False
        ' 
        ' Label8
        ' 
        Label8.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Label8.Cursor = Cursors.Hand
        Label8.Font = New Font("Segoe UI", 10F, FontStyle.Bold)
        Label8.ForeColor = Color.White
        Label8.Location = New Point(194, 122)
        Label8.Name = "Label8"
        Label8.Size = New Size(121, 19)
        Label8.TabIndex = 81
        Label8.Text = "Medium"
        Label8.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Label9
        ' 
        Label9.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        Label9.Cursor = Cursors.Hand
        Label9.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        Label9.ForeColor = Color.White
        Label9.Location = New Point(194, 94)
        Label9.Name = "Label9"
        Label9.Size = New Size(121, 27)
        Label9.TabIndex = 80
        Label9.Text = ""
        Label9.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' C_B
        ' 
        C_B.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        C_B.Location = New Point(461, 149)
        C_B.Name = "C_B"
        C_B.Size = New Size(121, 3)
        C_B.TabIndex = 99
        C_B.TabStop = False
        C_B.Visible = False
        ' 
        ' C_BG
        ' 
        C_BG.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        C_BG.Cursor = Cursors.Hand
        C_BG.Location = New Point(461, 82)
        C_BG.Name = "C_BG"
        C_BG.Size = New Size(121, 70)
        C_BG.TabIndex = 75
        C_BG.TabStop = False
        ' 
        ' PictureBox2
        ' 
        PictureBox2.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        PictureBox2.Cursor = Cursors.Hand
        PictureBox2.Location = New Point(321, 82)
        PictureBox2.Name = "PictureBox2"
        PictureBox2.Size = New Size(121, 70)
        PictureBox2.TabIndex = 74
        PictureBox2.TabStop = False
        ' 
        ' PictureBox1
        ' 
        PictureBox1.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        PictureBox1.Cursor = Cursors.Hand
        PictureBox1.Location = New Point(194, 82)
        PictureBox1.Name = "PictureBox1"
        PictureBox1.Size = New Size(121, 70)
        PictureBox1.TabIndex = 73
        PictureBox1.TabStop = False
        ' 
        ' low
        ' 
        low.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        low.Cursor = Cursors.Hand
        low.Location = New Point(67, 82)
        low.Name = "low"
        low.Size = New Size(121, 70)
        low.TabIndex = 71
        low.TabStop = False
        ' 
        ' PictureBox6
        ' 
        PictureBox6.BackColor = Color.FromArgb(CByte(33), CByte(35), CByte(38))
        PictureBox6.Location = New Point(73, 244)
        PictureBox6.Name = "PictureBox6"
        PictureBox6.Size = New Size(224, 37)
        PictureBox6.TabIndex = 87
        PictureBox6.TabStop = False
        ' 
        ' lbl_BufferDuration
        ' 
        lbl_BufferDuration.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        lbl_BufferDuration.Font = New Font("Segoe UI Semibold", 18F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        lbl_BufferDuration.ForeColor = Color.White
        lbl_BufferDuration.Location = New Point(67, 292)
        lbl_BufferDuration.Name = "lbl_BufferDuration"
        lbl_BufferDuration.Size = New Size(685, 67)
        lbl_BufferDuration.TabIndex = 135
        lbl_BufferDuration.Text = "instantReplayLength:"
        lbl_BufferDuration.TextAlign = ContentAlignment.MiddleLeft
        ' 
        ' captrueblock_ico
        ' 
        captrueblock_ico.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        captrueblock_ico.Cursor = Cursors.Hand
        captrueblock_ico.Font = New Font("nvgcshare", 20F, FontStyle.Regular, GraphicsUnit.Point, CByte(0))
        captrueblock_ico.ForeColor = Color.Peru
        captrueblock_ico.Location = New Point(139, 104)
        captrueblock_ico.Name = "captrueblock_ico"
        captrueblock_ico.Size = New Size(24, 31)
        captrueblock_ico.TabIndex = 131
        captrueblock_ico.Text = ""
        captrueblock_ico.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' captrueblock
        ' 
        captrueblock.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        captrueblock.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        captrueblock.Font = New Font("Segoe UI", 13F)
        captrueblock.ForeColor = Color.White
        captrueblock.Location = New Point(176, 104)
        captrueblock.Name = "captrueblock"
        captrueblock.Size = New Size(665, 76)
        captrueblock.TabIndex = 73
        captrueblock.Text = "Settings"
        ' 
        ' settings_top
        ' 
        settings_top.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        settings_top.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        settings_top.Location = New Point(695, 160)
        settings_top.Name = "settings_top"
        settings_top.Size = New Size(925, 5)
        settings_top.TabIndex = 0
        settings_top.TabStop = False
        ' 
        ' text_settings
        ' 
        text_settings.BackColor = Color.Black
        text_settings.Font = New Font("Segoe UI Semibold", 14F, FontStyle.Bold)
        text_settings.ForeColor = Color.White
        text_settings.Location = New Point(465, 160)
        text_settings.Name = "text_settings"
        text_settings.Size = New Size(200, 50)
        text_settings.TabIndex = 59
        text_settings.Text = "Settings"
        text_settings.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' box_settings
        ' 
        box_settings.BackColor = Color.Black
        box_settings.Location = New Point(465, 160)
        box_settings.Name = "box_settings"
        box_settings.Size = New Size(200, 50)
        box_settings.TabIndex = 58
        box_settings.TabStop = False
        ' 
        ' vdo_resetall
        ' 
        vdo_resetall.BackColor = Color.FromArgb(CByte(38), CByte(43), CByte(47))
        vdo_resetall.Cursor = Cursors.Hand
        vdo_resetall.Font = New Font("Segoe UI", 14F)
        vdo_resetall.ForeColor = Color.White
        vdo_resetall.Location = New Point(465, 300)
        vdo_resetall.Name = "vdo_resetall"
        vdo_resetall.Size = New Size(200, 70)
        vdo_resetall.TabIndex = 70
        vdo_resetall.Text = "Reset All"
        vdo_resetall.TextAlign = ContentAlignment.MiddleCenter
        vdo_resetall.Visible = False
        ' 
        ' action_fn
        ' 
        action_fn.BackColor = Color.FromArgb(CByte(118), CByte(185), CByte(0))
        action_fn.Cursor = Cursors.Hand
        action_fn.Font = New Font("Segoe UI", 12F, FontStyle.Bold)
        action_fn.ForeColor = Color.White
        action_fn.Location = New Point(465, 220)
        action_fn.Name = "action_fn"
        action_fn.Size = New Size(200, 70)
        action_fn.TabIndex = 58
        action_fn.Text = "Saved"
        action_fn.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' Quality
        ' 
        Quality.Enabled = True
        Quality.Interval = 1
        ' 
        ' PictureBox22
        ' 
        PictureBox22.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        PictureBox22.Location = New Point(-3, -16)
        PictureBox22.Name = "PictureBox22"
        PictureBox22.Size = New Size(1951, 176)
        PictureBox22.TabIndex = 72
        PictureBox22.TabStop = False
        ' 
        ' PictureBox9
        ' 
        PictureBox9.BackColor = Color.Blue
        PictureBox9.BackgroundImageLayout = ImageLayout.None
        PictureBox9.Location = New Point(873, 990)
        PictureBox9.Name = "PictureBox9"
        PictureBox9.Size = New Size(80, 80)
        PictureBox9.TabIndex = 90
        PictureBox9.TabStop = False
        PictureBox9.Visible = False
        ' 
        ' PictureBox10
        ' 
        PictureBox10.BackColor = Color.Blue
        PictureBox10.BackgroundImageLayout = ImageLayout.None
        PictureBox10.Location = New Point(1620, 166)
        PictureBox10.Name = "PictureBox10"
        PictureBox10.Size = New Size(80, 80)
        PictureBox10.TabIndex = 91
        PictureBox10.TabStop = False
        PictureBox10.Visible = False
        ' 
        ' Base_RecordingsSet
        ' 
        AutoScaleMode = AutoScaleMode.None
        BackColor = Color.Red
        ClientSize = New Size(1700, 1070)
        Controls.Add(PictureBox10)
        Controls.Add(PictureBox9)
        Controls.Add(vdo_resetall)
        Controls.Add(settings_top)
        Controls.Add(text_settings)
        Controls.Add(box_settings)
        Controls.Add(PictureBox22)
        Controls.Add(setre)
        Controls.Add(action_fn)
        DoubleBuffered = True
        FormBorderStyle = FormBorderStyle.None
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "Base_RecordingsSet"
        Opacity = 0R
        ShowInTaskbar = False
        Text = "Recordings"
        TopMost = True
        TransparencyKey = Color.Red
        WindowState = FormWindowState.Maximized
        setre.ResumeLayout(False)
        setre.PerformLayout()
        Panel_SetVDO.ResumeLayout(False)
        Panel_SET.ResumeLayout(False)
        Panel_SET.PerformLayout()
        CType(PictureBox8, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox7, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox4, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox3, ComponentModel.ISupportInitialize).EndInit()
        CType(TrackBar_Replaylast, ComponentModel.ISupportInitialize).EndInit()
        CType(TrackBar_BITRATE, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox5, ComponentModel.ISupportInitialize).EndInit()
        CType(C_R, ComponentModel.ISupportInitialize).EndInit()
        CType(C_L, ComponentModel.ISupportInitialize).EndInit()
        CType(H_R, ComponentModel.ISupportInitialize).EndInit()
        CType(H_L, ComponentModel.ISupportInitialize).EndInit()
        CType(M_R, ComponentModel.ISupportInitialize).EndInit()
        CType(M_L, ComponentModel.ISupportInitialize).EndInit()
        CType(L_T, ComponentModel.ISupportInitialize).EndInit()
        CType(L_R, ComponentModel.ISupportInitialize).EndInit()
        CType(L_B, ComponentModel.ISupportInitialize).EndInit()
        CType(M_T, ComponentModel.ISupportInitialize).EndInit()
        CType(L_L, ComponentModel.ISupportInitialize).EndInit()
        CType(M_B, ComponentModel.ISupportInitialize).EndInit()
        CType(C_T, ComponentModel.ISupportInitialize).EndInit()
        CType(H_B, ComponentModel.ISupportInitialize).EndInit()
        CType(H_T, ComponentModel.ISupportInitialize).EndInit()
        CType(C_B, ComponentModel.ISupportInitialize).EndInit()
        CType(C_BG, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox2, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox1, ComponentModel.ISupportInitialize).EndInit()
        CType(low, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox6, ComponentModel.ISupportInitialize).EndInit()
        CType(settings_top, ComponentModel.ISupportInitialize).EndInit()
        CType(box_settings, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox22, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox9, ComponentModel.ISupportInitialize).EndInit()
        CType(PictureBox10, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents ALTZ As Timer
    Friend WithEvents setre As Panel
    Friend WithEvents action_fn As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents settings_top As PictureBox
    Friend WithEvents vdo_resetall As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents low As PictureBox
    Friend WithEvents C_TEXT As Label
    Friend WithEvents C_ICO As Label
    Friend WithEvents C_BG As PictureBox
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label12 As Label
    Friend WithEvents Label10 As Label
    Friend WithEvents Label11 As Label
    Friend WithEvents Label8 As Label
    Friend WithEvents Label9 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents PictureBox6 As PictureBox
    Friend WithEvents Label13 As Label
    Friend WithEvents fps As TextBox
    Friend WithEvents text_settings As Label
    Friend WithEvents box_settings As PictureBox
    Friend WithEvents C_R As PictureBox
    Friend WithEvents C_L As PictureBox
    Friend WithEvents H_R As PictureBox
    Friend WithEvents H_L As PictureBox
    Friend WithEvents M_R As PictureBox
    Friend WithEvents M_L As PictureBox
    Friend WithEvents L_R As PictureBox
    Friend WithEvents L_L As PictureBox
    Friend WithEvents C_T As PictureBox
    Friend WithEvents H_T As PictureBox
    Friend WithEvents C_B As PictureBox
    Friend WithEvents H_B As PictureBox
    Friend WithEvents M_B As PictureBox
    Friend WithEvents M_T As PictureBox
    Friend WithEvents L_B As PictureBox
    Friend WithEvents L_T As PictureBox
    Friend WithEvents Quality As Timer
    Friend WithEvents PictureBox22 As PictureBox
    Friend WithEvents Panel_SetVDO As Panel
    Friend WithEvents Label2 As Label
    Friend WithEvents Encoder_CODE As Label
    Friend WithEvents FPS_BOX As TextBox
    Friend WithEvents lblEncoderInfo As Label
    Friend WithEvents cmbEncoder As ComboBox
    Friend WithEvents advanced_main As Label
    Friend WithEvents Label16 As Label
    Friend WithEvents custom_main As Label
    Friend WithEvents Label19 As Label
    Friend WithEvents Panel_SET As Panel
    Friend WithEvents Label20 As Label
    Friend WithEvents PictureBox5 As PictureBox
    Friend WithEvents P_BOX As TextBox
    Friend WithEvents Resolution_BOX As ComboBox
    Friend WithEvents lblBitrateRange As Label
    Friend WithEvents TrackBar_BITRATE As TrackBar
    Friend WithEvents lblBitrateValue As Label
    Friend WithEvents captrueblock As Label
    Friend WithEvents captrueblock_ico As Label
    Friend WithEvents TrackBar_Replaylast As TrackBar
    Friend WithEvents lbl_BufferDuration As Label
    Friend WithEvents Label14 As Label
    Friend WithEvents warm_re As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents prearg As TextBox
    Friend WithEvents Button1 As Button
    Friend WithEvents Button_Copy As Button
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents PictureBox8 As PictureBox
    Friend WithEvents PictureBox7 As PictureBox
    Friend WithEvents PictureBox4 As PictureBox
    Friend WithEvents PictureBox9 As PictureBox
    Friend WithEvents PictureBox10 As PictureBox
End Class
