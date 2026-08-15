Imports System.Diagnostics
Imports System.IO
Imports System.Text

Partial Public Class CaptureEngine

    ''' <summary>
    ''' Build FFmpeg arguments for VIDEO-ONLY recording.
    '''
    ''' This is the Engine-Stable FFmpeg command — NO audio input, NO pipe,
    ''' NO -filter_complex amix. Just ddagrab → NVENC → output.
    '''
    ''' When audio is enabled, the output file is a TEMP .video.mp4 and a
    ''' separate AudioFileWriter records .wav files. At stop time, MuxVideoAudio
    ''' combines them into the final file.
    '''
    ''' When audio is disabled, output goes directly to the final file
    ''' (identical to Engine-Stable behavior).
    ''' </summary>
    Private Function BuildFFmpegArguments(outputFile As String) As String
        Dim sb As StringBuilder = New StringBuilder()
        Dim fpsStr As String = _settings.FPS.ToString()
        Dim br As String = _settings.Bitrate.ToString()
        Dim buf As String = (_settings.Bitrate * 2).ToString()
        Dim hwType As HwDeviceType = DetectHwDeviceType(_settings.Encoder)

        LogDebug($"[CaptureEngine] BuildFFmpegArguments: FPS={fpsStr}, Bitrate={br} bps ({(_settings.Bitrate / 1000000.0):F2} Mbps), Encoder={_settings.Encoder}, CaptureMethod={_settings.CaptureMethod}, Output={outputFile}")
        WriteDebugLog($"[CaptureEngine] BuildFFmpegArguments: FPS={fpsStr}, Bitrate={br} bps ({(_settings.Bitrate / 1000000.0):F2} Mbps), Encoder={_settings.Encoder}, CaptureMethod={_settings.CaptureMethod}, UseNativeRes={_settings.UseNativeResolution}, CustomW={_settings.CustomWidth}, CustomH={_settings.CustomHeight}")

        sb.Append("-hide_banner -loglevel info ")

        Dim videoFilter As String = ""

        Select Case _settings.CaptureMethod.ToLower()
            Case "ddagrab"
                sb.Append("-f lavfi -i ""ddagrab=output_idx=0:framerate=" & fpsStr & """ ")
                Select Case hwType
                    Case HwDeviceType.IntelQSV
                        videoFilter = "hwmap=derive_device=qsv"
                    Case HwDeviceType.None
                        videoFilter = "hwdownload,format=bgra,format=yuv420p"
                    Case HwDeviceType.NVIDIA, HwDeviceType.AMD
                        videoFilter = ""
                End Select

            Case "gdigrab"
                sb.Append("-f gdigrab -framerate " & fpsStr & " -i desktop ")
                videoFilter = ""

            Case "gfxcapture"
                sb.Append("-f lavfi -i ""gfxcapture=monitor_idx=0:max_framerate=" & fpsStr & """ ")
                Select Case hwType
                    Case HwDeviceType.IntelQSV
                        videoFilter = "fps=" & fpsStr & ",hwmap=derive_device=qsv"
                    Case HwDeviceType.None
                        videoFilter = "fps=" & fpsStr & ",hwdownload,format=bgra,format=yuv420p"
                    Case HwDeviceType.NVIDIA, HwDeviceType.AMD
                        videoFilter = "fps=" & fpsStr
                End Select
        End Select

        If Not _settings.UseNativeResolution AndAlso _settings.CustomWidth > 0 Then
            Dim isHw As Boolean = (_settings.CaptureMethod.ToLower() = "ddagrab" OrElse
                                   _settings.CaptureMethod.ToLower() = "gfxcapture")
            Dim scalePart As String = "scale=" & _settings.CustomWidth.ToString() & ":" & _settings.CustomHeight.ToString()

            If isHw Then
                If hwType = HwDeviceType.NVIDIA OrElse hwType = HwDeviceType.AMD Then
                    If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                    videoFilter = videoFilter & "hwdownload,format=bgra," & scalePart & ",hwupload"
                ElseIf hwType = HwDeviceType.IntelQSV Then
                    If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                    videoFilter = videoFilter & "hwdownload,format=bgra," & scalePart & ",hwmap=derive_device=qsv"
                Else
                    If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                    videoFilter = videoFilter & "hwdownload,format=bgra," & scalePart
                End If
            Else
                If videoFilter.Length > 0 Then videoFilter = videoFilter & ","
                videoFilter = videoFilter & scalePart
            End If
        End If

        ' ═══ VIDEO FILTER (single input, NO -filter_complex) ═══
        ' This is the Engine-Stable pattern — single video input means
        ' FFmpeg has zero audio synchronization overhead.
        If videoFilter.Length > 0 Then
            sb.Append("-vf """ & videoFilter & """ ")
        End If

        sb.Append("-c:v " & _settings.Encoder & " ")

        Dim nvencPreset As String = OverlayConfig.MapNvencPreset(_settings.NvencPreset)
        If String.IsNullOrEmpty(nvencPreset) Then nvencPreset = "p4"

        Select Case hwType
            Case HwDeviceType.NVIDIA
                sb.Append("-preset " & nvencPreset & " -tune ll -rc cbr ")
                sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & br & " ")
                sb.Append("-g " & fpsStr & " -fps_mode cfr ")
                sb.Append("-spatial-aq 1 -temporal-aq 1 ")

            Case HwDeviceType.IntelQSV
                sb.Append("-preset medium ")
                sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " -rc cbr ")
                sb.Append("-look_ahead 1 ")

            Case HwDeviceType.AMD
                sb.Append("-preset balanced -usage transcoding ")
                sb.Append("-b:v " & br & " -minrate " & br & " -maxrate " & br & " -bufsize " & buf & " -rc cbr ")

            Case HwDeviceType.None
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

        Dim isHwCapture As Boolean = (_settings.CaptureMethod.ToLower() = "ddagrab" OrElse
                                        _settings.CaptureMethod.ToLower() = "gfxcapture")

        If hwType = HwDeviceType.None Then
            If _settings.Encoder.IndexOf("libx265", StringComparison.OrdinalIgnoreCase) < 0 AndAlso
               _settings.Encoder.IndexOf("libx264", StringComparison.OrdinalIgnoreCase) < 0 AndAlso
               _settings.Encoder.IndexOf("svtav1", StringComparison.OrdinalIgnoreCase) < 0 Then
                sb.Append("-pix_fmt " & _settings.PixelFormat & " ")
            End If
        ElseIf hwType = HwDeviceType.NVIDIA OrElse hwType = HwDeviceType.AMD Then
            If Not isHwCapture Then
                sb.Append("-pix_fmt " & _settings.PixelFormat & " ")
            End If
        ElseIf hwType = HwDeviceType.IntelQSV Then
            If Not isHwCapture Then
                sb.Append("-pix_fmt " & _settings.PixelFormat & " ")
            End If
        End If

        ' ═══ NO AUDIO CODEC ARGS HERE ═══
        ' Audio is recorded separately by AudioFileWriter → .wav files.
        ' At stop time, BuildMuxArguments combines video + audio.

        Dim ext As String = Path.GetExtension(outputFile).ToLowerInvariant()
        If ext = ".mp4" OrElse ext = ".mov" OrElse ext = ".m4v" Then
            sb.Append("-movflags +faststart ")
        End If

        sb.Append("-y """ & outputFile & """")

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Build FFmpeg arguments for the MUX step — combines video + audio
    ''' into the final output file.
    '''
    ''' Command pattern:
    '''   ffmpeg -i temp_video.mp4 -i temp_system.wav [-i temp_mic.wav]
    '''          -map 0:v -map 1:a [-map 2:a]
    '''          -c:v copy
    '''          [-af:0 volume=X] [-af:1 volume=Y]
    '''          -c:a aac -b:a 320k -ar 48000
    '''          -movflags +faststart -y final.mp4
    '''
    ''' Video stream copy = instant (no re-encode). Audio AAC encode is fast.
    ''' For Separate mode: two audio tracks are kept as separate streams in MP4.
    ''' For Single mode: amix filter combines system + mic into one track.
    ''' </summary>
    ''' <summary>
    ''' Build FFmpeg arguments for the MUX step — combines video + audio
    ''' into the final output file with PER-TRACK high-precision sync.
    '''
    ''' Per-track sync (per GPT review):
    '''   systemOffsetSec: positive=skip (-ss), negative=delay (adelay)
    '''   micOffsetSec: same, INDEPENDENT of system offset
    '''   videoDurationSec: exact video duration from ffprobe
    '''
    ''' Positive offset (>0): audio started before video → skip leading audio
    '''   Pattern: -ss <offset> -i audio.wav
    '''
    ''' Negative offset (<0): audio started after video → delay audio
    '''   Pattern: -i audio.wav, then -af adelay=<ms>|<ms>
    '''   (adelay takes per-channel ms, e.g. adelay=500|500 for 500ms stereo)
    '''
    ''' Filters (always applied):
    '''   aresample=async=1:first_pts=0 = drift correction
    '''   apad = pad short audio with silence to match video duration
    '''
    ''' Duration:
    '''   -t <videoDuration> = trim output to exact video length
    ''' </summary>
    Private Function BuildMuxArguments(videoPath As String,
                                       systemWav As String, hasSystem As Boolean,
                                       micWav As String, hasMic As Boolean,
                                       outputFile As String,
                                       systemOffsetSec As Double,
                                       micOffsetSec As Double,
                                       videoDurationSec As Double) As String
        Dim sb As New StringBuilder()

        sb.Append("-hide_banner -loglevel info ")

        ' Input 0: video (temp .video.mp4) — NO -ss on video (video is the master timeline)
        sb.Append($"-i ""{videoPath}"" ")

        ' ── Per-track audio input with positive offset (-ss) ──
        ' Negative offset is handled via adelay filter AFTER input (see below).
        Dim sysOffsetStr As String = Math.Max(0, systemOffsetSec).ToString("0.000", Globalization.CultureInfo.InvariantCulture)
        Dim micOffsetStr As String = Math.Max(0, micOffsetSec).ToString("0.000", Globalization.CultureInfo.InvariantCulture)
        Dim sysDelayMs As Integer = CInt(Math.Max(0, -systemOffsetSec) * 1000)
        Dim micDelayMs As Integer = CInt(Math.Max(0, -micOffsetSec) * 1000)

        ' Input 1: system audio (.wav)
        If hasSystem Then
            If systemOffsetSec > 0.001 Then
                sb.Append($"-ss {sysOffsetStr} ")
            End If
            sb.Append($"-i ""{systemWav}"" ")
        End If

        ' Input 2: mic audio (.wav)
        If hasMic Then
            If micOffsetSec > 0.001 Then
                sb.Append($"-ss {micOffsetStr} ")
            End If
            sb.Append($"-i ""{micWav}"" ")
        End If

        Dim isSeparate As Boolean = (_settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack)

        ' ── Build per-track filter chains ──
        ' Each track: [optional adelay for negative offset] + volume + aresample + apad
        Dim sysFilter As String = BuildAudioFilter(_settings.SystemAudioVolume, sysDelayMs)
        Dim micFilter As String = BuildAudioFilter(_settings.MicVolume, micDelayMs)

        If hasSystem AndAlso hasMic Then
            If isSeparate Then
                ' Two separate audio tracks in output
                sb.Append("-map 0:v -map 1:a -map 2:a ")
                sb.Append($"-af:0 {sysFilter} ")
                sb.Append($"-af:1 {micFilter} ")
                sb.Append("-c:v copy ")
                sb.Append("-c:a:0 aac -b:a:0 320k -ar:a:0 48000 ")
                sb.Append("-c:a:1 aac -b:a:1 320k -ar:a:1 48000 ")
            Else
                ' Mix system + mic into single track using -filter_complex amix
                sb.Append("-filter_complex ""[1:a]" & sysFilter & "[a0];" &
                          "[2:a]" & micFilter & "[a1];" &
                          "[a0][a1]amix=inputs=2:duration=longest:normalize=0[aout]"" ")
                sb.Append("-map 0:v -map [aout] ")
                sb.Append("-c:v copy ")
                sb.Append("-c:a aac -b:a 320k -ar 48000 ")
            End If
        ElseIf hasSystem Then
            ' System only
            sb.Append("-map 0:v -map 1:a ")
            sb.Append($"-af {sysFilter} ")
            sb.Append("-c:v copy ")
            sb.Append("-c:a aac -b:a 320k -ar 48000 ")
        ElseIf hasMic Then
            ' Mic only (system failed/disabled)
            sb.Append("-map 0:v -map 1:a ")
            sb.Append($"-af {micFilter} ")
            sb.Append("-c:v copy ")
            sb.Append("-c:a aac -b:a 320k -ar 48000 ")
        Else
            ' No audio at all — just copy video
            sb.Append("-map 0:v -c:v copy ")
        End If

        ' ── -t: exact output duration (from ffprobe) ──
        If (hasSystem OrElse hasMic) AndAlso videoDurationSec > 0.001 Then
            Dim durStr As String = videoDurationSec.ToString("0.000", Globalization.CultureInfo.InvariantCulture)
            sb.Append($"-t {durStr} ")
        End If

        Dim ext As String = Path.GetExtension(outputFile).ToLowerInvariant()
        If ext = ".mp4" OrElse ext = ".mov" OrElse ext = ".m4v" Then
            sb.Append("-movflags +faststart ")
        End If

        ' -shortest as a safety net (only if we don't have -t from ffprobe)
        If (hasSystem OrElse hasMic) AndAlso videoDurationSec <= 0.001 Then
            sb.Append("-shortest ")
        End If

        sb.Append($"-y ""{outputFile}""")

        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Build audio filter chain for a single track.
    ''' Order: adelay (if negative offset) → volume → aresample → apad
    '''
    ''' - adelay: delays audio start (for negative offset — audio started after video)
    ''' - volume: apply volume multiplier
    ''' - aresample=async=1:first_pts=0: drift correction + sample-aligned start
    ''' - apad: pad with silence to match video duration
    ''' </summary>
    Private Function BuildAudioFilter(volume As Single, delayMs As Integer) As String
        Dim parts As New List(Of String)()

        ' adelay for negative offset (audio starts after video)
        ' Format: adelay=500|500 (per-channel ms for stereo)
        If delayMs > 0 Then
            parts.Add($"adelay={delayMs}|{delayMs}")
        End If

        ' Volume (skip if 1.0 to save encoding time)
        If Math.Abs(volume - 1.0F) > 0.001F Then
            parts.Add($"volume={FormatVolume(volume)}")
        End If

        ' Drift correction + sample-aligned start
        parts.Add("aresample=async=1:first_pts=0")

        ' Pad with silence to match video duration
        parts.Add("apad")

        Return String.Join(",", parts)
    End Function

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

    Private Shared Function FormatVolume(vol As Single) As String
        Dim v As Single = Math.Max(0.0F, Math.Min(2.0F, vol))
        Return v.ToString("0.000", Globalization.CultureInfo.InvariantCulture)
    End Function

End Class
