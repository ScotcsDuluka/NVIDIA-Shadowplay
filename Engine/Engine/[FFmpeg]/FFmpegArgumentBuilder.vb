Imports System.Diagnostics
Imports System.IO
Imports System.IO.Pipes
Imports System.Text
Imports NAudio.CoreAudioApi
Imports NAudio.Wave

Partial Public Class CaptureEngine

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
                ElseIf hwType = HwDeviceType.IntelQSV
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

        Dim hasNAudio As Boolean = (_settings.SystemAudioCapture OrElse _settings.MicCapture)

        ' Declare outside the If so they're visible to the later -filter_complex
        ' block (which references sysFmt.ChannelLayout + micFmt.ChannelLayout).
        Dim sysFmt As AudioFormat = Nothing
        Dim micFmt As AudioFormat = Nothing

        If hasNAudio Then
            sysFmt = DetectSystemFormat()
            micFmt = DetectMicFormat(_settings.MicDeviceId, _settings.MicDeviceName)

            Dim sysEnabled As Boolean = _settings.SystemAudioCapture
            Dim micEnabled As Boolean = _settings.MicCapture AndAlso
                (Not String.IsNullOrEmpty(_settings.MicDeviceId) OrElse
                 Not String.IsNullOrEmpty(_settings.MicDeviceName)) AndAlso
                micFmt IsNot Nothing
            Dim isSeparate As Boolean = (_settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack)

            If isSeparate AndAlso micEnabled AndAlso sysEnabled Then
                sb.Append($"-thread_queue_size 1024 -f {sysFmt.FFmpegFormatArg} -ar {sysFmt.SampleRate} -ac {sysFmt.Channels} -i ""{_sysPipePath}"" ")
                sb.Append($"-thread_queue_size 1024 -f {micFmt.FFmpegFormatArg} -ar {micFmt.SampleRate} -ac {micFmt.Channels} -i ""{_micPipePath}"" ")
            ElseIf isSeparate AndAlso micEnabled AndAlso Not sysEnabled Then
                sb.Append($"-thread_queue_size 1024 -f {micFmt.FFmpegFormatArg} -ar {micFmt.SampleRate} -ac {micFmt.Channels} -i ""{_sysPipePath}"" ")
            ElseIf sysEnabled AndAlso micEnabled Then
                sb.Append($"-thread_queue_size 1024 -f {sysFmt.FFmpegFormatArg} -ar {sysFmt.SampleRate} -ac {sysFmt.Channels} -i ""{_sysPipePath}"" ")
                sb.Append($"-thread_queue_size 1024 -f {micFmt.FFmpegFormatArg} -ar {micFmt.SampleRate} -ac {micFmt.Channels} -i ""{_micPipePath}"" ")
            ElseIf sysEnabled Then
                sb.Append($"-thread_queue_size 1024 -f {sysFmt.FFmpegFormatArg} -ar {sysFmt.SampleRate} -ac {sysFmt.Channels} -i ""{_sysPipePath}"" ")
            ElseIf micEnabled Then
                sb.Append($"-thread_queue_size 1024 -f {micFmt.FFmpegFormatArg} -ar {micFmt.SampleRate} -ac {micFmt.Channels} -i ""{_sysPipePath}"" ")
            End If
        ElseIf _settings.AudioCapture AndAlso Not String.IsNullOrEmpty(_settings.AudioDevice) Then
            sb.Append("-f dshow -i audio=""" & _settings.AudioDevice & """ ")
        End If

        Dim useFilterComplex As Boolean = hasNAudio AndAlso
            (_settings.SystemAudioCapture AndAlso
             _settings.MicCapture AndAlso
             (_settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SingleTrack))

        If useFilterComplex Then
            Dim fc As New StringBuilder()
            If videoFilter.Length > 0 Then
                fc.Append("[0:v]" & videoFilter & "[vout];")
            Else
                fc.Append("[0:v]null[vout];")
            End If
            fc.Append($"[1:a]volume={FormatVolume(_settings.SystemAudioVolume)},aresample=48000,aformat=channel_layouts={sysFmt.ChannelLayout}[sysv];")
            fc.Append($"[2:a]volume={FormatVolume(_settings.MicVolume)},aresample=48000,aformat=channel_layouts={micFmt.ChannelLayout}[micv];")
            fc.Append("[sysv][micv]amix=inputs=2:duration=longest:normalize=0[aout] ")
            sb.Append("-filter_complex """ & fc.ToString() & """ ")
            sb.Append("-map [vout] -map [aout] ")
        ElseIf videoFilter.Length > 0 Then
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

        Dim hasAnyAudio As Boolean = hasNAudio OrElse _settings.AudioCapture
        If hasAnyAudio Then
            Dim sysAndMicSeparate As Boolean = hasNAudio AndAlso
                (_settings.AudioTrackMode = CaptureSettings.AudioTrackModeEnum.SeparateTrack) AndAlso
                _settings.SystemAudioCapture AndAlso _settings.MicCapture

            If useFilterComplex Then
                sb.Append("-c:a aac -b:a 320k -ar 48000 ")
            ElseIf sysAndMicSeparate Then
                sb.Append("-map 0:v -map 1:a -map 2:a ")
                sb.Append($"-af:0 volume={FormatVolume(_settings.SystemAudioVolume)} ")
                sb.Append($"-af:1 volume={FormatVolume(_settings.MicVolume)} ")
                sb.Append("-c:a:0 aac -b:a:0 320k -ar 48000 ")
                sb.Append("-c:a:1 aac -b:a:1 320k -ar 48000 ")
            Else
                Dim vol As Single = If(_settings.SystemAudioCapture, _settings.SystemAudioVolume, _settings.MicVolume)
                sb.Append($"-af volume={FormatVolume(vol)} ")
                sb.Append("-c:a aac -b:a 320k -ar 48000 ")
            End If
        End If

        Dim ext As String = Path.GetExtension(outputFile).ToLowerInvariant()
        If ext = ".mp4" OrElse ext = ".mov" OrElse ext = ".m4v" Then
            sb.Append("-movflags +faststart ")
        End If

        sb.Append("-y """ & outputFile & """")

        Return sb.ToString()
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

    Private Function DetectSystemFormat() As AudioFormat
        Try
            Using devEnum As New MMDeviceEnumerator()
                Dim defaultOut As MMDevice = devEnum.GetDefaultAudioEndpoint(
                    DataFlow.Render, Role.Multimedia)
                If defaultOut IsNot Nothing Then
                    Using cap As New WasapiLoopbackCapture(defaultOut)
                        Return WaveFormatToInfo(cap.WaveFormat)
                    End Using
                End If
            End Using
        Catch
        End Try
        Return New AudioFormat()
    End Function

    Private Function DetectMicFormat(deviceId As String, deviceName As String) As AudioFormat
        Try
            Using devEnum As New MMDeviceEnumerator()
                Dim devices = devEnum.EnumerateAudioEndPoints(
                    DataFlow.Capture, DeviceState.Active)

                Dim target As MMDevice = Nothing
                If Not String.IsNullOrEmpty(deviceId) Then
                    For Each dev As MMDevice In devices
                        If dev.ID = deviceId Then
                            target = dev
                            Exit For
                        End If
                    Next
                End If
                If target Is Nothing AndAlso Not String.IsNullOrEmpty(deviceName) Then
                    For Each dev As MMDevice In devices
                        If String.Equals(dev.FriendlyName, deviceName, StringComparison.Ordinal) Then
                            target = dev
                            Exit For
                        End If
                    Next
                End If
                If target Is Nothing Then Return Nothing

                Using cap As New WasapiCapture(target)
                    Return WaveFormatToInfo(cap.WaveFormat)
                End Using
            End Using
        Catch
        End Try
        Return Nothing
    End Function

    Private Shared Function WaveFormatToInfo(wf As WaveFormat) As AudioFormat
        Dim info As New AudioFormat()
        If wf Is Nothing Then Return info
        info.SampleRate = wf.SampleRate
        info.Channels = wf.Channels
        If TypeOf wf Is WaveFormatExtensible Then
            Dim wfe As WaveFormatExtensible = DirectCast(wf, WaveFormatExtensible)
            info.IsFloat = (wfe.Encoding = WaveFormatEncoding.IeeeFloat)
            info.BitsPerSample = wfe.BitsPerSample
        Else
            info.IsFloat = (wf.Encoding = WaveFormatEncoding.IeeeFloat)
            info.BitsPerSample = wf.BitsPerSample
        End If
        Return info
    End Function

    Private Shared Function FormatVolume(vol As Single) As String
        Dim v As Single = Math.Max(0.0F, Math.Min(2.0F, vol))
        Return v.ToString("0.000", Globalization.CultureInfo.InvariantCulture)
    End Function

End Class
