' ══════════════════════════════════════════════════════════════════════════════
' [1] Sub_Record.vb — Recording & Replay Control (TCP Architecture)
' ══════════════════════════════════════════════════════════════════════════════
' หน้าที่: จัดการ Record/Replay/Save ผ่าน TCP → Engine
' สถาปัตยกรรม: Overlay ไม่ถือ ScreenRecorder ตรง
'   → ส่งคำสั่ง TCP แบบ fire-and-forget
'   → Engine ตอบกลับผ่าน OnMessageReceived
'   → W2: UI สถานะจริงเท่านั้น — toast/state ผูกกับ engine_response /
'     engine_recording_saved จาก Engine (ไม่โชว์ก่อนยืนยันอีกต่อไป)
'
' TCP Commands:
'   RECORD_START <outputPath>
'   RECORD_STOP
'   REPLAY_START <seconds>
'   REPLAY_STOP
'   REPLAY_SAVE <outputPath>;<duration>
' ══════════════════════════════════════════════════════════════════════════════

Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms

Partial Public Class Base

    ' ─── Public State (อ่านจากหน้าอื่นได้) ────────────────────────────────
    Public ReplayValue As Boolean = False
    Public RecordValue As Boolean = False

#Region "Anti-Spam Cooldown"
    ' ป้องกัน hotkey/button กดซ้ำเร็วเกิน
    '   → ทุก action ใช้ cooldown เดียวกัน (200ms)
    '   → throttle log ที่ 500ms

    Private Shared _lastUiActionTime As DateTime = DateTime.MinValue
    Private Shared _uiActionLock As New Object()
    Private Const UI_ACTION_COOLDOWN_MS As Integer = 200
    Private Shared _lastCooldownLogTime As DateTime = DateTime.MinValue

    ''' <summary>ตรวจ cooldown — False = กดเร็วเกิน</summary>
    Private Function CheckUiCooldown() As Boolean
        SyncLock _uiActionLock
            Dim elapsed As Long = CLng((DateTime.Now - _lastUiActionTime).TotalMilliseconds)
            If elapsed < UI_ACTION_COOLDOWN_MS Then
                Dim now As Long = CLng(DateTime.Now.TimeOfDay.TotalMilliseconds)
                If (now - CLng(_lastCooldownLogTime.TimeOfDay.TotalMilliseconds)) > 500 Then
                    Debug.WriteLine($"UI cooldown: rejected ({elapsed}ms < {UI_ACTION_COOLDOWN_MS}ms)")
                    _lastCooldownLogTime = DateTime.Now
                End If
                Return False
            End If
            Return True
        End SyncLock
    End Function

    Private Sub MarkUiAction()
        SyncLock _uiActionLock
            _lastUiActionTime = DateTime.Now
        End SyncLock
    End Sub

#End Region

#Region "Recording State — Local Tracking (TCP Architecture)"

    ' Overlay ไม่ถือ ScreenRecorder → ติดตามสถานะเอง
    ' Engine จะส่ง status กลับมาทาง OnMessageReceived ยืนยัน

    Private _isRecordingLocal As Boolean = False
    Private _isBufferingLocal As Boolean = False
    Private Shared _isTogglingRecording As Boolean = False
    Private Shared _isTogglingReplay As Boolean = False

    ''' <summary>Replay buffer กำลังทำงานอยู่หรือไม่</summary>
    Public ReadOnly Property ReplayActive As Boolean
        Get
            Return _isBufferingLocal
        End Get
    End Property

    ''' <summary>กำลัง Record อยู่หรือไม่</summary>
    Public ReadOnly Property IsRecording As Boolean
        Get
            Return _isRecordingLocal
        End Get
    End Property

#End Region

#Region "Output Directory"

    ''' <summary>
    ''' ดัน Path สำหรับบันทึกไฟล์ (จาก config.json → Gallery fallback → Videos)
    ''' </summary>
    Private Function GetOutputDirectory() As String
        Dim outputDir As String = ""

        Try
            ' 1. จาก config.json
            outputDir = AppSettings.Instance.Paths.SavePath

            ' 2. fallback: จาก Gallery UI
            If String.IsNullOrEmpty(outputDir) AndAlso Base_Gallery IsNot Nothing AndAlso Base_Gallery.txtFilePath IsNot Nothing Then
                outputDir = Base_Gallery.txtFilePath.Text
            End If
        Catch ex As Exception
            Debug.WriteLine("GetOutputDirectory: Error - " & ex.Message)
        End Try

        ' 3. fallback: default path
        If String.IsNullOrEmpty(outputDir) OrElse Not Directory.Exists(outputDir) Then
            outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Shadowplay", "Gallery")
        End If

        ' สร้างโฟลเดอร์ถ้ายังไม่มี
        If Not Directory.Exists(outputDir) Then
            Try
                Directory.CreateDirectory(outputDir)
            Catch ex As Exception
                Debug.WriteLine("GetOutputDirectory: Failed to create - " & ex.Message)
                outputDir = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
            End Try
        End If

        Debug.WriteLine("GetOutputDirectory: " & outputDir)
        Return outputDir
    End Function

#End Region

#Region "Toggle Recording (Alt+F9)"

    Public Async Sub ToggleRecording()
        ' Guard: cooldown
        If Not CheckUiCooldown() Then Exit Sub

        ' Guard: prevent overlapping toggle
        SyncLock _uiActionLock
            If _isTogglingRecording Then Exit Sub
            _isTogglingRecording = True
        End SyncLock
        MarkUiAction()

        ' Guard: privacy check
        If Not IsPrivacyEnabled() Then
            ShowMainPanel()
            OpenSettings()
            PrivacyOpen()
            ShowNotifier("notificationWarningDesktopCaptureDisabled")
            _isTogglingRecording = False
            Exit Sub
        End If

        Try
            If _isRecordingLocal Then

                ' ══════════════════ STOP ══════════════════
                _isRecordingLocal = False
                RecordValue = False
                ' W2-2: ไม่มี optimistic "saved" toast อีกต่อไป — ไฟล์ยังไม่ถูก
                ' เขียนด้วยซ้ำ ตอนนี้ toast จะยิงใน [Overlay] Client.vb
                ' HandleEngineRecordingSaved เมื่อ Engine เขียนไฟล์เสร็จจริง

                Await Task.Run(Sub()
                                   Try : tcp.Send("RECORD_STOP")
                                   Catch ex As Exception
                                       Debug.WriteLine("RECORD_STOP TCP Error: " & ex.Message)
                                   End Try
                               End Sub)

            Else

                ' ══════════════════ START ══════════════════
                ' W2-2: ไม่มี optimistic "started" toast — ย้ายไปยิงใน
                ' [Overlay] Client.vb เมื่อ engine_response = ok เท่านั้น
                ' (ถ้า Engine ปฏิเสธ ผู้ใช้จะเห็น recording_error แทน toast
                ' ที่เคยโกหก)

                Dim outputDir As String = GetOutputDirectory()
                Dim outputPath As String = Path.Combine(outputDir,
                    $"Record_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4")

                Try : tcp.Send("RECORD_START", outputPath)
                Catch ex As Exception
                    Debug.WriteLine("RECORD_START TCP Error: " & ex.Message)
                End Try

                ' Optimistic UI
                _isRecordingLocal = True
                RecordValue = True

            End If

        Catch ex As Exception
            Debug.WriteLine($"[ToggleRecording] Error: {ex.Message}")
            _isRecordingLocal = False
            RecordValue = False
            ShowNotifier("recording_error")
        Finally
            SyncLock _uiActionLock
                _isTogglingRecording = False
            End SyncLock
        End Try
    End Sub

#End Region

#Region "Toggle Instant Replay (Alt+Shift+F10)"

    ''' <summary>
    ''' เริ่ม/หยุด Replay Buffer ผ่าน TCP → Engine
    '''   Start: REPLAY_START &lt;seconds&gt;
    '''   Stop:  REPLAY_STOP
    ''' </summary>
    Public Async Sub ToggleInstantReplay()
        ' Guard: cooldown
        If Not CheckUiCooldown() Then Exit Sub

        ' Guard: prevent overlapping toggle
        SyncLock _uiActionLock
            If _isTogglingReplay Then Exit Sub
            _isTogglingReplay = True
        End SyncLock
        MarkUiAction()

        ' Guard: privacy check
        If Not IsPrivacyEnabled() Then
            ShowMainPanel()
            OpenSettings()
            PrivacyOpen()
            ShowNotifier("notificationWarningDesktopCaptureDisabled")
            _isTogglingReplay = False
            Exit Sub
        End If

        Try
            If _isBufferingLocal Then

                ' ══════════════════ STOP BUFFER ══════════════════
                _isBufferingLocal = False
                ReplayValue = False

                ' UI
                SetControlColor(Replay_Logo, Color.White)
                SetControlEnabled(Menu_Replay_Box2, False)
                SetControlEnabled(Menu_Replay_save_text, False)
                SetControlEnabled(Menu_Replay_save_key, False)
                ' W2-1: toast "off" ย้ายไปยิงเมื่อ Engine ตอบ ok เท่านั้น
                ' (state revert ด้านบนเป็น fail-safe ที่ปลอดภัยอยู่แล้ว —
                ' ถ้า Engine ปฏิเสธ แปลว่า buffer ไม่เคยมีจริง)

                Await Task.Run(Sub()
                                   Try : tcp.Send("REPLAY_STOP")
                                   Catch ex As Exception
                                       Debug.WriteLine("REPLAY_STOP TCP Error: " & ex.Message)
                                   End Try
                               End Sub)

            Else

                ' ══════════════════ START BUFFER ══════════════════
                Dim saveSeconds As Integer = AppSettings.Instance.Recording.ReplayDuration
                saveSeconds = Math.Max(15, Math.Min(1200, saveSeconds))

                Debug.WriteLine($"Replay duration: {saveSeconds}s")

                Try : tcp.Send("REPLAY_START", saveSeconds.ToString())
                Catch ex As Exception
                    Debug.WriteLine("REPLAY_START TCP Error: " & ex.Message)
                End Try

                ' W2-1: ไม่มี optimistic "on" toast/state — Engine ปฏิเสธทุก
                ' REPLAY_* ด้วย not_implemented (UI_Engine.vb) ดังนั้น UI จะ
                ' ติดสว่างเฉพาะเมื่อ engine_response = ok เท่านั้น
                ' (handler ใน [Overlay] Client.vb)

            End If

        Catch ex As Exception
            Debug.WriteLine($"[ToggleInstantReplay] Error: {ex.Message}")
            _isBufferingLocal = False
            ReplayValue = False
            ShowNotifier("replay_error")
        Finally
            SyncLock _uiActionLock
                _isTogglingReplay = False
            End SyncLock
        End Try
    End Sub

#End Region

#Region "Save Instant Replay"

    Public Async Sub SaveInstantReplay()
        If Not CheckUiCooldown() Then Exit Sub
        MarkUiAction()

        Try
            ' ตรวจ buffer
            If Not _isBufferingLocal Then
                ShowNotifier("replay_turn_on")
                Exit Sub
            End If

            ' UI: ปิดปุ่มชั่วคราว
            SetControlEnabled(Menu_Replay_Box2, False)
            SetControlEnabled(Menu_Replay_save_text, False)
            SetControlEnabled(Menu_Replay_save_key, False)

            ' Output path
            Dim outputDir As String = GetOutputDirectory()
            Dim outputPath As String = Path.Combine(outputDir,
                $"Replay_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4")

            ' Duration (clamp 15–1200s)
            Dim duration As Integer = AppSettings.Instance.Recording.ReplayDuration
            duration = Math.Max(15, Math.Min(1200, duration))

            Debug.WriteLine($"SaveInstantReplay: {outputPath} ({duration}s)")

            ' TCP: REPLAY_SAVE <path>;<duration>
            Try
                Await Task.Run(Sub()
                                   Try
                                       tcp.Send("REPLAY_SAVE", $"{outputPath};{duration}")
                                   Catch ex As Exception
                                       Debug.WriteLine("REPLAY_SAVE TCP Error: " & ex.Message)
                                   End Try
                               End Sub)
            Catch ex As Exception
                Debug.WriteLine("REPLAY_SAVE TCP Error: " & ex.Message)
            End Try

            ' W2-1/W2-2: ไม่มี optimistic "saved_last_15" toast — Engine ไม่เคย
            ' รองรับ REPLAY_SAVE (ตอบ not_implemented เสมอ) toast จะยิงใน
            ' [Overlay] Client.vb เมื่อ engine_response = ok เท่านั้น

        Catch ex As Exception
            Debug.WriteLine($"[SaveInstantReplay] Error: {ex.Message}")
            ShowNotifier("replay_error")
        Finally
            ' เปิดปุ่มกลับถ้า buffer ยังทำงาน
            If _isBufferingLocal Then
                SetControlEnabled(Menu_Replay_Box2, True)
                SetControlEnabled(Menu_Replay_save_text, True)
                SetControlEnabled(Menu_Replay_save_key, True)
            End If
        End Try
    End Sub

#End Region

#Region "Replay Honesty (W2-1)"

    ''' <summary>
    ''' Engine ปฏิเสธทุก REPLAY_* command ด้วย not_implemented
    ''' (UI_Engine.vb — ยังไม่มี replay buffer จริง) ดังนั้นปิดปุ่ม replay
    ''' ในแผงหลักและระบุให้ชัด แทนที่จะมีปุ่มที่กดแล้วได้แต่ความหลอก
    ''' ปุ่มจะกลับมาใช้ได้เฉพาะเมื่อ Engine ตอบ engine_replay_start = ok
    ''' (handler ใน [Overlay] Client.vb) — "until Engine has real buffer"
    ''' </summary>
    Public Sub InitReplayHonesty()
        Try
            Menu_Replay_key.Enabled = False
            Menu_Replay_Box1.Enabled = False
            Menu_Replay_text.Enabled = False
            Menu_Replay_Box2.Enabled = False
            Menu_Replay_save_text.Enabled = False
            Menu_Replay_save_key.Enabled = False
            Debug.WriteLine("[Overlay] replay controls disabled (engine replay not implemented)")
        Catch ex As Exception
            Debug.WriteLine("[Overlay] InitReplayHonesty error: " & ex.Message)
        End Try
    End Sub

#End Region

#Region "Encoder Info — from AppSettings"

    ''' <summary>ชื่อ Encoder แบบอ่านง่าย</summary>
    Public Function GetEncoderInfo() As String
        Try
            Select Case AppSettings.Instance.Recording.Encoder
                Case "NVENC_H264" : Return "NVIDIA NVENC H.264"
                Case "NVENC_HEVC" : Return "NVIDIA NVENC HEVC"
                Case "NVENC_AV1" : Return "NVIDIA NVENC AV1"
                Case "QuickSync_H264" : Return "Intel QuickSync H.264"
                Case "QuickSync_HEVC" : Return "Intel QuickSync HEVC"
                Case "AMF_H264" : Return "AMD AMF H.264"
                Case "AMF_HEVC" : Return "AMD AMF HEVC"
                Case "LibX264" : Return "CPU LibX264"
                Case "LibX265" : Return "CPU LibX265"
                Case Else : Return AppSettings.Instance.Recording.Encoder
            End Select
        Catch ex As Exception
            Return "Unknown"
        End Try
    End Function

    ''' <summary>ข้อมูล Encoder แบบละเอียด</summary>
    Public Function GetEncoderInfoDetailed() As String
        Try
            Dim rec = AppSettings.Instance.Recording
            Dim info As New System.Text.StringBuilder()

            info.AppendLine("Encoder: " & GetEncoderInfo())
            info.AppendLine("Preset: " & rec.EncoderPreset)
            info.AppendLine("Bitrate: " & rec.Bitrate & " kbps")
            info.AppendLine("FPS: " & rec.FPS)

            If rec.UseNativeResolution Then
                info.AppendLine("Resolution: Native")
            Else
                info.AppendLine($"Resolution: {rec.Width}x{rec.Height}")
            End If

            Return info.ToString()
        Catch ex As Exception
            Return "Error: " & ex.Message
        End Try
    End Function

#End Region

End Class
