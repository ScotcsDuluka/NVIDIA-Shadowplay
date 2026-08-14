' CaptureEngine.vb
' ShadowPlay Engine - FFmpeg Screen Capture Engine
' Manages FFmpeg process: Start/Stop/ForceStop
' Builds FFmpeg args based on CaptureSettings
' Supports: ddagrab, gdigrab, gfxcapture
' Encoders: NVENC, QSV, AMF, libx264, libx265, SVT-AV1
'
' ── FFmpeg Official Syntax ──
'
' ddagrab (Desktop Duplication via DXGI):
'   ffmpeg -f lavfi -i "ddagrab=output_idx=0:framerate=60" -c:v h264_nvenc out.mp4
'   Output: D3D11 hardware frames
'   -> NVENC/AMF: pass directly (d3d11 -> d3d11)
'   -> QSV: add -vf "hwmap=derive_device=qsv"
'   -> CPU: add -vf "hwdownload,format=bgra,format=yuv420p"
'
' gfxcapture (Windows.Graphics.Capture):
'   ffmpeg -f lavfi -i "gfxcapture=monitor_idx=0:max_framerate=60" -vf "fps=60" ...
'   Output: D3D11 hardware frames (VFR, needs fps filter)
'
' gdigrab (GDI Legacy Screen Capture):
'   ffmpeg -f gdigrab -framerate 60 -i desktop -c:v libx264 out.mp4
'   Output: System memory frames (BGRA)
'
' Audio capture:
'   -f dshow -i audio="Device Name"
'
' IMPORTANT: ddagrab and gfxcapture use -f lavfi -i (NOT -filter_complex alone)
' because -filter_complex without -i causes stream mapping issues with
' multiple inputs (audio from dshow). Use -vf for post-filters instead.

Imports System.Diagnostics
Imports System.IO
Imports System.Text

Public Class CaptureEngine
    Implements IDisposable

    ' ── Events ────────────────────────────────────────────────

    Public Event StateChanged As Action(Of CaptureState)
    Public Event RecordingStarted As Action(Of String)
    Public Event RecordingStopped As Action(Of String)
    Public Event ErrorOccurred As Action(Of String)
    Public Event FrameCaptured As Action(Of Long)

    ' ✅ P2.9: new event for progress reporting (frame + duration + file size).
    ' Fired on every FFmpeg stderr progress line (typically 1/sec at 60fps).
    ' UI_Engine listens and broadcasts to Overlay via TCP.
    Public Event ProgressUpdated As Action(Of Long, TimeSpan, Long)

    ' ── Enums ──────────────────────────────────────────────────
    ' NOTE: 'Error' is a VB.NET reserved keyword - using 'HasError' instead

    Public Enum CaptureState
        Idle
        Detecting
        Recording
        Paused
        Stopping
        HasError
    End Enum

    ' ── Hardware device type enum for filter chain ────────────

    Private Enum HwDeviceType
        None        ' CPU software encoder
        NVIDIA      ' h264_nvenc, hevc_nvenc (accepts d3d11 directly)
        IntelQSV    ' h264_qsv, hevc_qsv  (needs hwmap from d3d11)
        AMD         ' h264_amf, hevc_amf  (accepts d3d11 directly)
    End Enum

    ' ── Fields ────────────────────────────────────────────────

    Private _settings As CaptureSettings
    Private _ffmpegProcess As Process
    Private _state As CaptureState = CaptureState.Idle
    Private _outputFile As String = ""
    Private _stopwatch As Stopwatch
    Private _logBuffer As StringBuilder
    Private _disposed As Boolean = False
    ' ✅ P1: Job Object guard — when the Engine process dies for any reason
    ' (crash, Task Manager kill, logoff), Windows automatically kills every
    ' process assigned to this job. Prevents orphaned ffmpeg.exe from running
    ' indefinitely after Engine is gone.
    Private _jobGuard As JobObjectGuard

    ' ── Properties ────────────────────────────────────────────

    Public ReadOnly Property State As CaptureState
        Get
            Return _state
        End Get
    End Property

    Public ReadOnly Property IsRecording As Boolean
        Get
            Return _state = CaptureState.Recording
        End Get
    End Property

    Public ReadOnly Property OutputFile As String
        Get
            Return _outputFile
        End Get
    End Property

    Public ReadOnly Property RecordingDuration As TimeSpan
        Get
            If _stopwatch Is Nothing OrElse Not _stopwatch.IsRunning Then
                Return TimeSpan.Zero
            End If
            Return _stopwatch.Elapsed
        End Get
    End Property

    ' ── Constructor ───────────────────────────────────────────

    Public Sub New(settings As CaptureSettings)
        _settings = settings
        _logBuffer = New StringBuilder()
        Try
            _jobGuard = New JobObjectGuard()
        Catch ex As Exception
            ' Best-effort: if job creation fails (e.g. on Wine/older Windows),
            ' capture still works — we just lose orphan protection.
            LogDebug("JobObjectGuard init failed: " & ex.Message)
        End Try
    End Sub

    ' ── Start Recording ───────────────────────────────────────

    Public Async Function StartRecordingAsync(Optional overrideOutputPath As String = Nothing) As Task(Of Boolean)
        If _state <> CaptureState.Idle Then
            RaiseEvent ErrorOccurred("Cannot start: engine is not idle")
            Return False
        End If

        Dim validation As CaptureSettings.ValidationResult = _settings.Validate()
        If Not validation.Valid Then
            RaiseEvent ErrorOccurred(validation.Message)
            Return False
        End If

        If Not Directory.Exists(_settings.OutputDirectory) Then
            Directory.CreateDirectory(_settings.OutputDirectory)
        End If

        ' ✅ P1.5: if the caller (Overlay via Hub) supplied a specific output path,
        ' honor it. Otherwise fall back to settings.GenerateOutputFilename() which
        ' uses settings.OutputDirectory + timestamp. Old behavior always used the
        ' settings path, which meant files ended up in Engine's preferred folder
        ' instead of where the Overlay told the user they'd be saved.
        If Not String.IsNullOrEmpty(overrideOutputPath) Then
            ' Basic sanity: must end with a known video extension.
            Dim ext As String = Path.GetExtension(overrideOutputPath).ToLowerInvariant()
            If ext = ".mp4" OrElse ext = ".mov" OrElse ext = ".mkv" OrElse ext = ".avi" OrElse ext = ".m4v" Then
                _outputFile = overrideOutputPath
                ' Make sure parent dir exists.
                Dim parentDir As String = Path.GetDirectoryName(overrideOutputPath)
                If Not String.IsNullOrEmpty(parentDir) AndAlso Not Directory.Exists(parentDir) Then
                    Try
                        Directory.CreateDirectory(parentDir)
                    Catch ex As Exception
                        RaiseEvent ErrorOccurred("Cannot create output dir: " & ex.Message)
                        Return False
                    End Try
                End If
            Else
                RaiseEvent ErrorOccurred("Unsupported output extension: " & ext)
                Return False
            End If
        Else
            _outputFile = _settings.GenerateOutputFilename()
        End If

        ' ✅ C5 FIX: do NOT set Recording state until _ffmpegProcess is actually
        ' assigned inside the Task. Old code called SetState(Recording) here,
        ' before the Task even started — if StopRecordingAsync arrived in the
        ' window between SetState and the Task assigning _ffmpegProcess, Stop
        ' would see _state=Recording but _ffmpegProcess=Nothing → skip FFmpeg
        ' cleanup, set state back to Idle, raise RecordingStopped. The Task
        ' would then launch FFmpeg with no one tracking it (orphan).
        ' Now we set Recording state only AFTER _ffmpegProcess is alive.

        Return Await Task.Run(Function()
                                 Try
                                     Dim args As String = BuildFFmpegArguments(_outputFile)
                                     LogDebug("FFmpeg command: " & _settings.FFmpegPath & " " & args)
                                     WriteDebugLog("FFmpeg command: " & _settings.FFmpegPath & " " & args)

                                     Dim si As New ProcessStartInfo()
                                     si.FileName = _settings.FFmpegPath
                                     si.Arguments = args
                                     si.UseShellExecute = False
                                     si.RedirectStandardOutput = True
                                     si.RedirectStandardError = True
                                     si.RedirectStandardInput = True
                                     si.CreateNoWindow = True

                                     _ffmpegProcess = New Process()
                                     _ffmpegProcess.StartInfo = si
                                     ' ✅ FIX: Must set EnableRaisingEvents=True BEFORE Start() or the Exited event never fires.
                                     ' Without this, a crashed FFmpeg looks "still recording" forever.
                                     _ffmpegProcess.EnableRaisingEvents = True
                                     AddHandler _ffmpegProcess.OutputDataReceived, AddressOf OnStdOut
                                     AddHandler _ffmpegProcess.ErrorDataReceived, AddressOf OnStdErr
                                     AddHandler _ffmpegProcess.Exited, AddressOf OnExited

                                     If Not _ffmpegProcess.Start() Then
                                         SetState(CaptureState.HasError)
                                         RaiseEvent ErrorOccurred("Failed to start FFmpeg process")
                                         Return False
                                     End If

                                     ' ✅ P1: tie the FFmpeg child to our Job Object so it dies with us.
                                     If _jobGuard IsNot Nothing Then
                                         _jobGuard.Assign(_ffmpegProcess)
                                     End If

                                     _ffmpegProcess.BeginOutputReadLine()
                                     _ffmpegProcess.BeginErrorReadLine()
                                     _stopwatch = Stopwatch.StartNew()
                                     ' ✅ C5 FIX: now that _ffmpegProcess is alive, set Recording state.
                                     ' Any Stop arriving after this point will correctly see the process.
                                     SetState(CaptureState.Recording)
                                     RaiseEvent RecordingStarted(_outputFile)
                                     Return True

                                 Catch ex As Exception
                                     SetState(CaptureState.HasError)
                                     RaiseEvent ErrorOccurred("Start failed: " & ex.Message)
                                     LogDebug("Exception: " & ex.ToString())
                                     WriteDebugLog("Start exception: " & ex.ToString())
                                     Return False
                                 End Try
                             End Function)
    End Function

    ' ── Stop Recording ────────────────────────────────────────

    Public Async Function StopRecordingAsync() As Task(Of Boolean)
        If _state <> CaptureState.Recording Then
            Return False
        End If

        SetState(CaptureState.Stopping)
        _stopwatch?.Stop()

        Return Await Task.Run(Function()
                                 Try
                                     If _ffmpegProcess IsNot Nothing AndAlso Not _ffmpegProcess.HasExited Then
                                         Try
                                             _ffmpegProcess.StandardInput.Write("q" & vbLf)
                                             _ffmpegProcess.StandardInput.Flush()
                                         Catch
                                         End Try

                                         If Not _ffmpegProcess.WaitForExit(10000) Then
                                             _ffmpegProcess.Kill()
                                         End If
                                     End If

                                     SetState(CaptureState.Idle)
                                     RaiseEvent RecordingStopped(_outputFile)
                                     LogDebug("Recording saved: " & _outputFile)
                                     Return True

                                 Catch ex As Exception
                                     SetState(CaptureState.HasError)
                                     RaiseEvent ErrorOccurred("Stop failed: " & ex.Message)
                                     Return False
                                 End Try
                             End Function)
    End Function

    ' ── Force Stop ────────────────────────────────────────────

    Public Sub ForceStop()
        Try
            If _ffmpegProcess IsNot Nothing AndAlso Not _ffmpegProcess.HasExited Then
                _ffmpegProcess.Kill()
                _ffmpegProcess.WaitForExit(5000)
            End If
        Catch
        Finally
            If _ffmpegProcess IsNot Nothing Then
                _ffmpegProcess.Dispose()
                _ffmpegProcess = Nothing
            End If
            If _stopwatch IsNot Nothing Then
                _stopwatch.Stop()
            End If
            SetState(CaptureState.Idle)
        End Try
    End Sub

    ' ════════════════════════════════════════════════════════════
    ' ── Build FFmpeg Arguments ─────────────────────────────────
    ' ════════════════════════════════════════════════════════════
    '
    ' ALL capture methods use -f XXX -i "..." (standard FFmpeg input)
    ' Video post-processing uses -vf (video filter chain)
    ' Audio uses separate -f dshow -i audio="..."
    '
    ' This avoids stream mapping issues that -filter_complex alone causes.
    '
    ' ── ddagrab (DXGI Desktop Duplication) ──
    '   Input:  -f lavfi -i "ddagrab=output_idx=0:framerate=N"
    '   NVENC:  (direct, no -vf needed)
    '   QSV:    -vf "hwmap=derive_device=qsv"
    '   CPU:    -vf "hwdownload,format=bgra,format=yuv420p"
    '   AMF:    (direct, no -vf needed)
    '
    ' ── gfxcapture (Windows.Graphics.Capture) ──
    '   Input:  -f lavfi -i "gfxcapture=monitor_idx=0:max_framerate=N"
    '   All:    -vf "fps=N" (VFR -> CFR)
    '   + QSV:  -vf "fps=N,hwmap=derive_device=qsv"
    '   + CPU:  -vf "fps=N,hwdownload,format=bgra,format=yuv420p"
    '
    ' ── gdigrab (GDI Legacy) ──
    '   Input:  -f gdigrab -framerate N -i desktop
    '   System memory, no hw filters needed

    Private Function BuildFFmpegArguments(outputFile As String) As String
        Dim sb As StringBuilder = New StringBuilder()
        Dim fpsStr As String = _settings.FPS.ToString()
        Dim br As String = _settings.Bitrate.ToString()
        Dim buf As String = (_settings.Bitrate * 2).ToString()
        Dim hwType As HwDeviceType = DetectHwDeviceType(_settings.Encoder)

        ' ✅ P2.8: log the values actually being sent to FFmpeg so we can
        ' verify they match what Overlay's UI shows. This is the only way
        ' to debug "FPS 60 = 60, bitrate 15Mbps = 15Mbps" claims.
        LogDebug($"[CaptureEngine] BuildFFmpegArguments: FPS={fpsStr}, Bitrate={br} bps ({(_settings.Bitrate / 1000000.0):F2} Mbps), Encoder={_settings.Encoder}, CaptureMethod={_settings.CaptureMethod}, Output={outputFile}")
        WriteDebugLog($"[CaptureEngine] BuildFFmpegArguments: FPS={fpsStr}, Bitrate={br} bps ({(_settings.Bitrate / 1000000.0):F2} Mbps), Encoder={_settings.Encoder}, CaptureMethod={_settings.CaptureMethod}, UseNativeRes={_settings.UseNativeResolution}, CustomW={_settings.CustomWidth}, CustomH={_settings.CustomHeight}")

        ' ✅ M6 REVERTED: user confirmed QSV works without -init_hw_device.
        ' The audit was speculative — newer FFmpeg builds auto-init the QSV
        ' device when -c:v h264_qsv is selected, so hwmap=derive_device=qsv
        ' derives from it without explicit -init_hw_device.
        ' Adding -init_hw_device qsv:qsvhw 0 -filter_hw_device qsvhw could
        ' actually break things on FFmpeg builds that don't expect it
        ' (double-init, syntax mismatch, or conflict with encoder's own
        ' session). Reverted to the original behavior that the user verified
        ' works on Intel hardware.
        ' If a future FFmpeg version breaks this, we can re-add with proper
        ' testing on the specific version.

        sb.Append("-hide_banner -loglevel info ")

        ' ── Video input by capture method ──
        ' Also build -vf (video filter) chain for post-processing
        Dim videoFilter As String = ""

        Select Case _settings.CaptureMethod.ToLower()

            ' ═══ ddagrab: DXGI Desktop Duplication ═══
            Case "ddagrab"
                sb.Append("-f lavfi -i ""ddagrab=output_idx=0:framerate=" & fpsStr & """ ")

                Select Case hwType
                    Case HwDeviceType.IntelQSV
                        videoFilter = "hwmap=derive_device=qsv"

                    Case HwDeviceType.None
                        ' CPU: download D3D11 frames to system memory
                        videoFilter = "hwdownload,format=bgra,format=yuv420p"

                    Case HwDeviceType.NVIDIA, HwDeviceType.AMD
                        ' Direct D3D11 -> HW encoder, no filter needed
                        videoFilter = ""

                End Select

            ' ═══ gdigrab: GDI Legacy Screen Capture ═══
            Case "gdigrab"
                sb.Append("-f gdigrab -framerate " & fpsStr & " -i desktop ")
                ' gdigrab outputs system memory, no hw filter needed
                videoFilter = ""

            ' ═══ gfxcapture: Windows.Graphics.Capture API ═══
            Case "gfxcapture"
                sb.Append("-f lavfi -i ""gfxcapture=monitor_idx=0:max_framerate=" & fpsStr & """ ")

                ' gfxcapture outputs VFR - always add fps filter
                Select Case hwType
                    Case HwDeviceType.IntelQSV
                        videoFilter = "fps=" & fpsStr & ",hwmap=derive_device=qsv"

                    Case HwDeviceType.None
                        videoFilter = "fps=" & fpsStr & ",hwdownload,format=bgra,format=yuv420p"

                    Case HwDeviceType.NVIDIA, HwDeviceType.AMD
                        videoFilter = "fps=" & fpsStr

                End Select

        End Select

        ' ── Add scale filter if custom resolution ──
        ' ✅ FIX: for hw capture (ddagrab/gfxcapture) + NVENC/AMF, frames are d3d11.
        ' scale filter only handles software formats → must hwdownload first,
        ' scale, then hwupload back to d3d11 for the encoder.
        ' For QSV: hwdownload → scale → hwmap=derive_device=qsv
        ' For CPU: hwdownload → scale (stays in software)
        If Not _settings.UseNativeResolution AndAlso _settings.CustomWidth > 0 Then
            Dim isHw As Boolean = (_settings.CaptureMethod.ToLower() = "ddagrab" OrElse
                                   _settings.CaptureMethod.ToLower() = "gfxcapture")
            Dim scalePart As String = "scale=" & _settings.CustomWidth.ToString() & ":" & _settings.CustomHeight.ToString()

            If isHw Then
                ' Need to download from d3d11 → software → scale → upload back
                If hwType = HwDeviceType.NVIDIA OrElse hwType = HwDeviceType.AMD Then
                    ' NVENC/AMF accept d3d11 → hwupload back after scale
                    If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                    videoFilter = videoFilter & "hwdownload,format=bgra," & scalePart & ",hwupload"
                ElseIf hwType = HwDeviceType.IntelQSV
                    ' QSV: hwdownload → scale → hwmap
                    If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                    videoFilter = videoFilter & "hwdownload,format=bgra," & scalePart & ",hwmap=derive_device=qsv"
                Else
                    ' CPU encoder: hwdownload → scale (stays software)
                    If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                    videoFilter = videoFilter & "hwdownload,format=bgra," & scalePart
                End If
            Else
                ' gdigrab: software frames, just scale
                If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                videoFilter = videoFilter & scalePart
            End If
        End If

        ' ═══════════════════════════════════════════════════════════════
        ' ── Audio inputs (WASAPI) ──
        ' ═══════════════════════════════════════════════════════════════
        ' WASAPI = Windows Audio Session API (same as OBS/Discord/ShadowPlay)
        ' - Bit-perfect capture (no resample by Windows)
        ' - Low latency (~10ms vs dshow ~50ms)
        ' - Works on every Windows 10/11 machine (no Stereo Mix needed)
        '
        ' Audio quality features:
        ' - thread_queue_size 4096: large buffer to prevent underruns
        ' - aresample=async=1: auto-resample + sync to prevent drift
        ' - aformat=channel_layouts=stereo: force stereo for consistent mixing
        ' - highpass=20: cut sub-bass rumble (below human hearing)
        ' - lowpass=20000: cut ultrasonic noise (above human hearing)
        ' - afftdn=nr=10: light noise reduction (FFT-based, -10dB threshold)
        ' - loudnorm=I=-16:LUFS:TP=-1.5:LRA=11: EBU R128 loudness normalization
        '   (same standard as YouTube/Netflix — consistent volume across recordings)
        ' - amix normalize=0: don't reduce volume when mixing sources
        ' - AAC 320kbps @ 48kHz: high quality, universal compatibility
        ' ═══════════════════════════════════════════════════════════════

        Dim hasAudio As Boolean = _settings.SystemAudioCapture OrElse _settings.MicCapture
        Dim audioInputCount As Integer = 0

        If _settings.SystemAudioCapture Then
            ' WASAPI loopback: captures whatever the default output device plays.
            ' "default" = current default audio output (speakers/headphones).
            sb.Append("-thread_queue_size 4096 -f wasapi -i ""default"" ")
            audioInputCount += 1
        End If

        If _settings.MicCapture AndAlso Not String.IsNullOrEmpty(_settings.MicDeviceName) Then
            ' WASAPI input for microphone: lower latency than dshow, bit-perfect.
            sb.Append("-thread_queue_size 4096 -f wasapi -i """ & _settings.MicDeviceName & """ ")
            audioInputCount += 1
        End If

        ' ── Filter chain ──
        Dim useFilterComplex As Boolean = hasAudio AndAlso audioInputCount > 0

        If useFilterComplex Then
            Dim fc As New StringBuilder()

            ' ── Video portion: [0:v]<filter>[vout] ──
            If videoFilter.Length > 0 Then
                fc.Append("[0:v]" & videoFilter & "[vout];")
            Else
                fc.Append("[0:v]copy[vout];")
            End If

            ' ── Audio portion ──
            ' Each source gets a professional audio chain:
            '   volume → highpass → lowpass → afftdn (noise reduction) → aresample → aformat
            Dim audioLabels As New List(Of String)
            Dim nextIdx As Integer = 1

            If _settings.SystemAudioCapture Then
                Dim vol As String = _settings.SystemAudioVolume.ToString("F2", Globalization.CultureInfo.InvariantCulture)
                ' System audio chain: volume + EQ + noise gate + resample
                fc.Append($"[{nextIdx}:a]" &
                          $"volume={vol}," &
                          $"highpass=f=20," &
                          $"lowpass=f=20000," &
                          $"afftdn=nr=10:nf=-40," &
                          $"aresample=48000:async=1:" &
                          $"aformat=channel_layouts=stereo[sys];")
                audioLabels.Add("[sys]")
                nextIdx += 1
            End If

            If _settings.MicCapture AndAlso Not String.IsNullOrEmpty(_settings.MicDeviceName) Then
                Dim vol As String = _settings.MicVolume.ToString("F2", Globalization.CultureInfo.InvariantCulture)
                ' Mic chain: volume + highpass (cut low rumble) + lowpass (cut hiss)
                ' + afftdn (noise reduction) + resample + stereo
                fc.Append($"[{nextIdx}:a]" &
                          $"volume={vol}," &
                          $"highpass=f=80," &
                          $"lowpass=f=16000," &
                          $"afftdn=nr=15:nf=-35," &
                          $"aresample=48000:async=1:" &
                          $"aformat=channel_layouts=stereo[mic];")
                audioLabels.Add("[mic]")
                nextIdx += 1
            End If

            ' ── Mix audio sources ──
            If audioLabels.Count = 1 Then
                ' Single source: apply loudnorm after all processing
                fc.Append($"{audioLabels(0)}" &
                          $"loudnorm=I=-16:TP=-1.5:LRA=11[aout]")
            ElseIf audioLabels.Count = 2 Then
                ' Two sources: mix with normalize=0 (no volume reduction)
                ' then apply loudnorm on the mixed result
                fc.Append($"{audioLabels(0)}{audioLabels(1)}" &
                          $"amix=inputs=2:duration=first:normalize=0," &
                          $"loudnorm=I=-16:TP=-1.5:LRA=11[aout]")
            End If

            Dim fcStr As String = fc.ToString().TrimEnd(";"c)
            sb.Append($"-filter_complex ""{fcStr}"" ")
            sb.Append("-map ""[vout]"" -map ""[aout]"" ")

        Else
            ' No audio — use -vf as before (zero regression)
            If videoFilter.Length > 0 Then
                sb.Append("-vf """ & videoFilter & """ ")
            End If
        End If

        ' ── Video encoder settings ──
        sb.Append("-c:v " & _settings.Encoder & " ")

        ' ✅ P2.8: build NVENC preset string from Overlay's encoder_preset
        ' (1-7 → p1-p7). Was hardcoded to 'p4' before, so user's 'Maximum'
        ' preset (p7) was silently ignored.
        Dim nvencPreset As String = OverlayConfig.MapNvencPreset(_settings.NvencPreset)
        If String.IsNullOrEmpty(nvencPreset) Then nvencPreset = "p4"

        Select Case hwType
            Case HwDeviceType.NVIDIA
                ' ✅ CBR + CFR for 100% bitrate accuracy.
                sb.Append("-preset " & nvencPreset & " -tune ll -rc cbr ")
                sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & br & " ")
                sb.Append("-g " & fpsStr & " -fps_mode cfr ")
                ' ✅ FIX: -spatial-aq and -temporal-aq are h264_nvenc ONLY.
                ' hevc_nvenc and av1_nvenc don't support them → FFmpeg errors out.
                If _settings.Encoder.IndexOf("h264_nvenc", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    sb.Append("-spatial-aq 1 -temporal-aq 1 ")
                ElseIf _settings.Encoder.IndexOf("hevc_nvenc", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    ' HEVC: only spatial-aq (temporal-aq not supported)
                    sb.Append("-spatial-aq 1 ")
                End If
                ' av1_nvenc: no AQ options at all

            Case HwDeviceType.IntelQSV
                sb.Append("-preset medium ")
                sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " -rc cbr ")
                sb.Append("-look_ahead 1 ")

            Case HwDeviceType.AMD
                sb.Append("-preset balanced -usage transcoding ")
                sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " -rc cbr ")

            Case HwDeviceType.None
                ' ✅ C1 FIX: CPU encoders — drop -crf because it overrides -b:v.
                ' FFmpeg gives CRF mode precedence over -b:v, so the user's
                ' bitrate setting was silently ignored. Now use pure bitrate
                ' mode with minrate/maxrate = b:v for strict CBR (same as NVENC).
                If _settings.Encoder.IndexOf("libx265", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    sb.Append("-preset medium ")
                    sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " -pix_fmt yuv420p10le ")
                ElseIf _settings.Encoder.IndexOf("libx264", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    sb.Append("-preset ultrafast -tune zerolatency ")
                    sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " -pix_fmt yuv420p ")
                ElseIf _settings.Encoder.IndexOf("svtav1", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    sb.Append("-preset 6 ")
                    sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " -pix_fmt yuv420p ")
                Else
                    sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " ")
                End If
        End Select

        ' ── Pixel format ──
        ' IMPORTANT: ddagrab/gfxcapture output d3d11 hardware frames.
        ' -pix_fmt triggers FFmpeg auto_scale filter which ONLY handles software formats.
        ' So for hw encoder + hw capture: do NOT add -pix_fmt — the encoder handles d3d11 natively.
        ' For hw encoder + gdigrab (software frames): -pix_fmt is safe.
        Dim isHwCapture As Boolean = (_settings.CaptureMethod.ToLower() = "ddagrab" OrElse
                                        _settings.CaptureMethod.ToLower() = "gfxcapture")

        If hwType = HwDeviceType.None Then
            ' CPU encoders: pix_fmt already set in encoder section above (libx265/x264/svtav1)
            ' Only add for unknown CPU encoders
            If _settings.Encoder.IndexOf("libx265", StringComparison.OrdinalIgnoreCase) < 0 AndAlso
               _settings.Encoder.IndexOf("libx264", StringComparison.OrdinalIgnoreCase) < 0 AndAlso
               _settings.Encoder.IndexOf("svtav1", StringComparison.OrdinalIgnoreCase) < 0 Then
                sb.Append("-pix_fmt " & _settings.PixelFormat & " ")
            End If
        ElseIf hwType = HwDeviceType.NVIDIA OrElse hwType = HwDeviceType.AMD Then
            ' NVENC/AMF accept d3d11 natively — skip -pix_fmt for hw capture
            If Not isHwCapture Then
                sb.Append("-pix_fmt " & _settings.PixelFormat & " ")
            End If
        ElseIf hwType = HwDeviceType.IntelQSV Then
            ' QSV handles format via hwmap filter — skip -pix_fmt for hw capture
            If Not isHwCapture Then
                sb.Append("-pix_fmt " & _settings.PixelFormat & " ")
            End If
        End If

        ' ── Audio encoding ──
        ' ✅ Pro quality: AAC 320kbps @ 48kHz with -async 1 for sync.
        ' AAC is universally compatible (every player, every platform).
        ' 320kbps is transparent — no audible difference from lossless for AAC.
        ' 48kHz matches video timing exactly (prevents resampling artifacts).
        If hasAudio AndAlso audioInputCount > 0 Then
            sb.Append("-c:a aac -b:a 320k -ar 48000 -async 1 ")
            ' ✅ Pro: add -shortest to stop when the shortest input ends.
            ' Prevents runaway audio after video stops.
            sb.Append("-shortest ")
        End If

        ' ── MP4 faststart: write moov atom at the head so the file is playable
        '    even if FFmpeg is killed mid-write (Stop timeout, crash, Engine exit).
        '    Without this, a forced Kill on MP4 output leaves a corrupt file.
        Dim ext As String = Path.GetExtension(outputFile).ToLowerInvariant()
        If ext = ".mp4" OrElse ext = ".mov" OrElse ext = ".m4v" Then
            sb.Append("-movflags +faststart ")
        End If

        ' ── Output ──
        sb.Append("-y """ & outputFile & """")

        Return sb.ToString()
    End Function

    ' ── Detect hardware device type from encoder string ──────

    Private Function DetectHwDeviceType(encoderId As String) As HwDeviceType
        If encoderId.IndexOf("nvenc", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return HwDeviceType.NVIDIA
        End If
        If encoderId.IndexOf("qsv", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return HwDeviceType.IntelQSV
        End If
        If encoderId.IndexOf("amf", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return HwDeviceType.AMD
        End If
        Return HwDeviceType.None
    End Function

    ' ── FFmpeg Process Events ─────────────────────────────────

    Private Sub OnStdOut(sender As Object, e As DataReceivedEventArgs)
        If e.Data IsNot Nothing Then
            LogDebug("[stdout] " & e.Data)
        End If
    End Sub

    Private Sub OnStdErr(sender As Object, e As DataReceivedEventArgs)
        If e.Data Is Nothing Then Return

        LogDebug("[stderr] " & e.Data)
        WriteDebugLog("[stderr] " & e.Data)

        ' Parse frame progress: "frame=  120 fps=60 ..."
        If e.Data.IndexOf("frame=", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Try
                Dim idx As Integer = e.Data.IndexOf("frame=") + 6
                If idx < e.Data.Length Then
                    Dim remaining As String = e.Data.Substring(idx).TrimStart()
                    Dim spaceIdx As Integer = remaining.IndexOf(" "c)
                    Dim frameStr As String = ""
                    If spaceIdx > 0 Then
                        frameStr = remaining.Substring(0, spaceIdx).Trim()
                    Else
                        frameStr = remaining.Trim()
                    End If
                    Dim frameNum As Long = 0
                    If Long.TryParse(frameStr, frameNum) Then
                        RaiseEvent FrameCaptured(frameNum)

                        ' ✅ P2.9: also parse "time=00:00:42.50" and "size=    8420KiB"
                        ' and fire ProgressUpdated. Overlay uses this to show real-time
                        ' recording timer + file size in its UI.
                        Dim duration As TimeSpan = TimeSpan.Zero
                        Dim sizeBytes As Long = 0

                        ' Parse time=HH:MM:SS.mmm
                        Dim timeIdx As Integer = e.Data.IndexOf("time=", StringComparison.OrdinalIgnoreCase)
                        If timeIdx >= 0 Then
                            Dim timeStr As String = e.Data.Substring(timeIdx + 5).TrimStart()
                            Dim timeEnd As Integer = timeStr.IndexOf(" "c)
                            If timeEnd > 0 Then timeStr = timeStr.Substring(0, timeEnd)
                            Dim parsed As TimeSpan
                            ' ✅ C3 FIX: use InvariantCulture so the parser works on
                            ' machines with non-English locales (de-DE uses ',' as
                            ' decimal separator, which breaks Double.TryParse for the
                            ' millisecond portion).
                            If TimeSpan.TryParse(timeStr, Globalization.CultureInfo.InvariantCulture, parsed) Then
                                duration = parsed
                            End If
                        End If

                        ' Parse size=NNNNKiB or NNNNMiB
                        Dim sizeIdx As Integer = e.Data.IndexOf("size=", StringComparison.OrdinalIgnoreCase)
                        If sizeIdx >= 0 Then
                            Dim sizeStr As String = e.Data.Substring(sizeIdx + 5).TrimStart()
                            Dim sizeEnd As Integer = sizeStr.IndexOf(" "c)
                            If sizeEnd > 0 Then sizeStr = sizeStr.Substring(0, sizeEnd)
                            ' Strip trailing unit (kB, KiB, MB, MiB, etc.)
                            Dim unitStart As Integer = -1
                            For i As Integer = 0 To sizeStr.Length - 1
                                If Not (Char.IsDigit(sizeStr(i)) OrElse sizeStr(i) = "."c) Then
                                    unitStart = i
                                    Exit For
                                End If
                            Next
                            Dim numStr As String = If(unitStart >= 0, sizeStr.Substring(0, unitStart), sizeStr)
                            Dim unitStr As String = If(unitStart >= 0, sizeStr.Substring(unitStart).Trim(), "")
                            Dim sizeNum As Double = 0
                            ' ✅ C3 FIX: InvariantCulture for the same reason.
                            If Double.TryParse(numStr, Globalization.CultureInfo.InvariantCulture, sizeNum) Then
                                Select Case unitStr.ToUpperInvariant()
                                    Case "B" : sizeBytes = CLng(sizeNum)
                                    Case "KB", "KIB" : sizeBytes = CLng(sizeNum * 1024)
                                    Case "MB", "MIB" : sizeBytes = CLng(sizeNum * 1024 * 1024)
                                    Case "GB", "GIB" : sizeBytes = CLng(sizeNum * 1024 * 1024 * 1024)
                                End Select
                            End If
                        End If

                        ' Fallback: use stopwatch if FFmpeg's time= is missing.
                        If duration = TimeSpan.Zero AndAlso _stopwatch IsNot Nothing AndAlso _stopwatch.IsRunning Then
                            duration = _stopwatch.Elapsed
                        End If

                        RaiseEvent ProgressUpdated(frameNum, duration, sizeBytes)
                    End If
                End If
            Catch
            End Try
        End If

        ' Report errors — tightened: only match real FFmpeg error markers.
        ' Old code matched the substring "error" which fires on benign lines like
        ' "Error resilience", "errordetect", "max_error_rate", x264 "[error]:" notices, etc.
        Dim low As String = e.Data.ToLowerInvariant()
        Dim isError As Boolean =
            low.Contains("[error]") OrElse
            low.Contains("conversion failed") OrElse
            low.Contains("could not open") OrElse
            low.Contains("no such file or directory") OrElse
            low.Contains("invalid argument") OrElse
            low.Contains("device not found") OrElse
            low.Contains("unknown encoder") OrElse
            low.Contains("not currently supported in output") OrElse
            low.StartsWith("error") OrElse
            low.Contains("av_interleaved_write_header")

        If isError Then
            RaiseEvent ErrorOccurred(e.Data)
            WriteDebugLog("[ERROR] " & e.Data)
        End If
    End Sub

    Private Sub OnExited(sender As Object, e As EventArgs)
        ' Capture exit code first — _ffmpegProcess may be nulled by ForceStop()/Dispose() concurrently.
        Dim exitCode As String = "?"
        Dim proc As Process = _ffmpegProcess
        If proc IsNot Nothing Then
            Try
                exitCode = proc.ExitCode.ToString()
            Catch
            End Try
        End If

        LogDebug("FFmpeg exited with code: " & exitCode)
        WriteDebugLog("FFmpeg exited with code: " & exitCode)

        If _state = CaptureState.Recording Then
            If _stopwatch IsNot Nothing Then _stopwatch.Stop()
            ' Non-zero exit → treat as error (FFmpeg crashed or failed to start).
            ' This is the path that catches a crashed FFmpeg now that EnableRaisingEvents=True.
            If exitCode <> "0" AndAlso exitCode <> "?" Then
                SetState(CaptureState.HasError)
                RaiseEvent ErrorOccurred("FFmpeg exited unexpectedly with code " & exitCode)
            Else
                SetState(CaptureState.Idle)
                RaiseEvent RecordingStopped(_outputFile)
            End If
        End If
    End Sub

    ' ── Helpers ──────────────────────────────────────────────

    Private Sub SetState(newState As CaptureState)
        _state = newState
        RaiseEvent StateChanged(newState)
    End Sub

    Private Sub LogDebug(message As String)
        Dim line As String = "[" & DateTime.Now.ToString("HH:mm:ss.fff") & "] " & message
        ' ✅ C8 FIX: StringBuilder is not thread-safe. LogDebug is called from
        ' FFmpeg stdout, stderr, Exited, UI, and TCP listener threads concurrently.
        ' Without a lock, concurrent appends can corrupt internal state.
        SyncLock _logBuffer
            _logBuffer.AppendLine(line)
            If _logBuffer.Length > 10240 Then
                _logBuffer.Remove(0, _logBuffer.Length - 8192)
            End If
        End SyncLock
    End Sub

    ''' <summary>
    ''' Write debug info to log file on disk for troubleshooting.
    ''' </summary>
    Private Sub WriteDebugLog(message As String)
        Try
            Dim logDir As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
            Dim logPath As String = Path.Combine(logDir, "capture-engine.log")
            Dim logLine As String = "[" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") & "] " & message
            ' ✅ P1: route through BackgroundLogger instead of File.AppendAllText per line.
            ' FFmpeg progress goes to stderr at up to 60 lines/sec (one per frame); the old
            ' per-line AppendAllText was a real disk-thrash on long recordings.
            BackgroundLogger.Log(logPath, logLine)
        Catch
        End Try
    End Sub

    ' ── Dispose ────────────────────────────────────────────────

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                ForceStop()
                ' ✅ P1: dispose the job guard AFTER ForceStop — closing the job handle
                ' is what guarantees any straggler ffmpeg is killed.
                If _jobGuard IsNot Nothing Then
                    _jobGuard.Dispose()
                    _jobGuard = Nothing
                End If
            End If
            _disposed = True
        End If
    End Sub

End Class
