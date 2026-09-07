

















Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms

Partial Public Class Base

    
    Public ReplayValue As Boolean = False
    Public RecordValue As Boolean = False

#Region "Anti-Spam Cooldown"
    
    
    

    Private Shared _lastUiActionTime As DateTime = DateTime.MinValue
    Private Shared _uiActionLock As New Object()
    Private Const UI_ACTION_COOLDOWN_MS As Integer = 200
    Private Shared _lastCooldownLogTime As DateTime = DateTime.MinValue

    
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

    
    

    Private _isRecordingLocal As Boolean = False
    Private _isBufferingLocal As Boolean = False
    Private Shared _isTogglingRecording As Boolean = False
    Private Shared _isTogglingReplay As Boolean = False

    
    Public ReadOnly Property ReplayActive As Boolean
        Get
            Return _isBufferingLocal
        End Get
    End Property

    
    Public ReadOnly Property IsRecording As Boolean
        Get
            Return _isRecordingLocal
        End Get
    End Property

#End Region

#Region "Output Directory"

    
    
    
    Private Function GetOutputDirectory() As String
        Dim outputDir As String = ""

        Try
            
            outputDir = AppSettings.Instance.Paths.SavePath

            
            If String.IsNullOrEmpty(outputDir) AndAlso Base_Gallery IsNot Nothing AndAlso Base_Gallery.txtFilePath IsNot Nothing Then
                outputDir = Base_Gallery.txtFilePath.Text
            End If
        Catch ex As Exception
            Debug.WriteLine("GetOutputDirectory: Error - " & ex.Message)
        End Try

        
        If String.IsNullOrEmpty(outputDir) OrElse Not Directory.Exists(outputDir) Then
            outputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Shadowplay", "Gallery")
        End If

        
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
        
        If Not CheckUiCooldown() Then Exit Sub

        
        SyncLock _uiActionLock
            If _isTogglingRecording Then Exit Sub
            _isTogglingRecording = True
        End SyncLock
        MarkUiAction()

        
        If Not IsPrivacyEnabled() Then
            ShowMainPanel()
            OpenSettings()
            PrivacyOpen()
            ShowNotifier("notificationWarningDesktopCaptureDisabled")
            _isTogglingRecording = False
            Exit Sub
        End If

        
        
        
        
        
        
        
        
        If tcp Is Nothing OrElse Not tcp.IsConnected Then
            Record_Stats.Text = "Hub Offline — unable to start recording"
            _isTogglingRecording = False
            Exit Sub
        End If

        Try
            If _isRecordingLocal Then

                
                _isRecordingLocal = False
                RecordValue = False
                
                
                

                Await Task.Run(Sub()
                                   Try : tcp.Send("RECORD_STOP")
                                   Catch ex As Exception
                                       Debug.WriteLine("RECORD_STOP TCP Error: " & ex.Message)
                                   End Try
                               End Sub)

            Else

                
                
                
                
                

                Dim outputDir As String = GetOutputDirectory()
                Dim outputPath As String = Path.Combine(outputDir,
                    $"Record_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4")

                Try : tcp.Send("RECORD_START", outputPath)
                Catch ex As Exception
                    Debug.WriteLine("RECORD_START TCP Error: " & ex.Message)
                End Try

                
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

    
    
    
    
    
    Public Async Sub ToggleInstantReplay()
        
        If Not CheckUiCooldown() Then Exit Sub

        
        SyncLock _uiActionLock
            If _isTogglingReplay Then Exit Sub
            _isTogglingReplay = True
        End SyncLock
        MarkUiAction()

        
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

                
                _isBufferingLocal = False
                ReplayValue = False

                
                SetControlColor(Replay_Logo, Color.White)
                SetControlEnabled(Menu_Replay_Box2, False)
                SetControlEnabled(Menu_Replay_save_text, False)
                SetControlEnabled(Menu_Replay_save_key, False)
                
                
                

                Await Task.Run(Sub()
                                   Try : tcp.Send("REPLAY_STOP")
                                   Catch ex As Exception
                                       Debug.WriteLine("REPLAY_STOP TCP Error: " & ex.Message)
                                   End Try
                               End Sub)

            Else

                
                Dim saveSeconds As Integer = AppSettings.Instance.Recording.ReplayDuration
                saveSeconds = Math.Max(15, Math.Min(1200, saveSeconds))

                Debug.WriteLine($"Replay duration: {saveSeconds}s")

                Try : tcp.Send("REPLAY_START", saveSeconds.ToString())
                Catch ex As Exception
                    Debug.WriteLine("REPLAY_START TCP Error: " & ex.Message)
                End Try

                
                
                
                

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
            
            If Not _isBufferingLocal Then
                ShowNotifier("replay_turn_on")
                Exit Sub
            End If

            
            SetControlEnabled(Menu_Replay_Box2, False)
            SetControlEnabled(Menu_Replay_save_text, False)
            SetControlEnabled(Menu_Replay_save_key, False)

            
            Dim outputDir As String = GetOutputDirectory()
            Dim outputPath As String = Path.Combine(outputDir,
                $"Replay_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.mp4")

            
            Dim duration As Integer = AppSettings.Instance.Recording.ReplayDuration
            duration = Math.Max(15, Math.Min(1200, duration))

            Debug.WriteLine($"SaveInstantReplay: {outputPath} ({duration}s)")

            
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

            
            
            

        Catch ex As Exception
            Debug.WriteLine($"[SaveInstantReplay] Error: {ex.Message}")
            ShowNotifier("replay_error")
        Finally
            
            If _isBufferingLocal Then
                SetControlEnabled(Menu_Replay_Box2, True)
                SetControlEnabled(Menu_Replay_save_text, True)
                SetControlEnabled(Menu_Replay_save_key, True)
            End If
        End Try
    End Sub

#End Region

#Region "Replay Honesty (W2-1)"

    
    
    
    
    
    
    
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
