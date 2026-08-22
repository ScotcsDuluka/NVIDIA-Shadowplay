Option Strict On
Option Explicit On
Option Infer On

' CaptureSession.vb
'
' Per-session resource owner. Composes:
'   - BoundedVideoFrameSink (frame queue)
'   - AudioFileWriter (NAudio WASAPI loopback → temp WAV)
'   - FFmpeg subprocess (raw H.264 → temp video file)
'   - MuxCoordinator (video + audio → final MP4)
'
' Lifecycle (per session):
'   1. Start audio capture
'   2. Start video capture (DdagrabBackend.Start(sink))
'   3. Start encoder (NvencEncoderBackend.Start())
'   4. Capture/encode loop: sink.Take → encoder.Encode → write H.264 to file
'   5. Stop video → drain sink → stop encoder
'   6. Stop audio → finalize WAV
'   7. FFmpeg mux → MP4
'   8. Verify MP4 streams
'   9. Return SessionResult
'
' Ownership:
'   - BORROWS _capture + _encoder from RecordingEngine (does NOT dispose)
'   - OWNS BoundedVideoFrameSink + audio + FFmpeg + mux + temp files

Imports System.Diagnostics
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports NAudio.Wave
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab
Imports CaptureEngine.Video.Handoff
Imports CaptureEngine.Encoder
Imports CaptureEngine.Encoder.Nvenc
Imports CaptureEngine.FFmpegBackend

Namespace CaptureEngine.Recording

    Public NotInheritable Class CaptureSession
        Implements IDisposable

        Private ReadOnly _capture As DdagrabBackend
        Private ReadOnly _encoder As NvencEncoderBackend
        Private ReadOnly _config As SessionConfig
        Private ReadOnly _logger As EngineLogger
        Private _stopSignal As Boolean = False
        Private _disposed As Boolean = False

        Public Sub New(capture As DdagrabBackend,
                      encoder As NvencEncoderBackend,
                      config As SessionConfig,
                      logger As EngineLogger)
            _capture = capture
            _encoder = encoder
            _config = config
            _logger = logger
        End Sub

        Public Function Run() As SessionResult
            Dim result As New SessionResult() With {
                .OutputPath = _config.OutputPath,
                .RequestedDurationSec = _config.DurationSeconds
            }

            ' ─── Temp file paths ─────────────────────────────────────────
            Dim tempH264 As String = Path.ChangeExtension(_config.OutputPath, ".tmp.h264")
            Dim tempVideoMp4 As String = Path.ChangeExtension(_config.OutputPath, ".tmp.video.mp4")
            Dim tempWav As String = Path.ChangeExtension(_config.OutputPath, ".tmp.wav")

            ' ─── Resources ───────────────────────────────────────────────
            Dim sink As BoundedVideoFrameSink = Nothing
            Dim audioCapture As WasapiLoopbackCapture = Nothing
            Dim wavWriter As WaveFileWriter = Nothing
            Dim videoFile As FileStream = Nothing
            Dim audioThread As Thread = Nothing
            Dim totalAudioBytes As Long = 0
            Dim totalAudioSamples As Long = 0

            Dim sw As Stopwatch = Stopwatch.StartNew()
            Dim duration As TimeSpan = TimeSpan.FromSeconds(_config.DurationSeconds)

            Try
                ' ─── 1. Create sink ──────────────────────────────────────
                sink = New BoundedVideoFrameSink(4, BoundedHandoffPolicy.DropOldest, _logger)

                ' ─── 2. Start audio capture (NAudio WasapiLoopbackCapture) ─
                _logger.Info("[session] Starting audio capture...")
                audioCapture = New WasapiLoopbackCapture()
                Dim waveFormat = audioCapture.WaveFormat
                _logger.Info($"[session] Audio: {waveFormat.Channels}ch {waveFormat.SampleRate}Hz {waveFormat.BitsPerSample}bit")
                wavWriter = New WaveFileWriter(tempWav, waveFormat)

                AddHandler audioCapture.DataAvailable, Sub(s, e)
                    If e.BytesRecorded > 0 Then
                        wavWriter.Write(e.Buffer, 0, e.BytesRecorded)
                        totalAudioBytes += e.BytesRecorded
                        totalAudioSamples += e.BytesRecorded \ (waveFormat.Channels * waveFormat.BitsPerSample \ 8)
                    End If
                    If _stopSignal OrElse sw.Elapsed >= duration Then
                        audioCapture.StopRecording()
                    End If
                End Sub

                AddHandler audioCapture.RecordingStopped, Sub(s, e)
                    wavWriter.Flush()
                    wavWriter.Dispose()
                End Sub

                audioCapture.StartRecording()
                _logger.Info("[session] Audio capture started")

                ' ─── 3. Start video capture + encoder ─────────────────────
                _logger.Info("[session] Starting video capture...")
                _encoder.Start()
                _capture.Start(sink)

                ' ─── 4. Open video output file ───────────────────────────
                videoFile = New FileStream(tempH264, FileMode.Create, FileAccess.Write)
                _logger.Info($"[session] Writing H.264 to: {tempH264}")

                ' ─── 5. Capture/encode loop ─────────────────────────────
                _logger.Info($"[session] Recording for {_config.DurationSeconds}s...")
                Do While sw.Elapsed < duration AndAlso Not _stopSignal
                    Dim far As FrameAcquisitionResult
                    If sink.TryTake(far) Then
                        result.FramesCaptured += 1
                        Dim packet As EncodedPacket = Nothing
                        Try
                            If _encoder.Encode(far.Frame, packet) AndAlso packet IsNot Nothing Then
                                videoFile.Write(packet.Payload, 0, packet.PayloadLength)
                                result.TotalVideoBytes += packet.PayloadLength
                                result.FramesEncoded += 1
                                packet.Dispose()
                            End If
                        Catch ex As Exception
                            result.NvencErrors += 1
                            _logger.Error($"[session] Encode error: {ex.Message}")
                        End Try
                        far.Frame?.Dispose()
                    Else
                        Thread.Sleep(1)
                    End If
                Loop

                ' ─── 6. Stop video → drain sink → stop encoder ──────────
                _logger.Info("[session] Stopping video capture...")
                _capture.Stop()
                _logger.Info("[session] Draining remaining frames...")
                Dim far2 As FrameAcquisitionResult
                Do While sink.TryTake(far2)
                    result.FramesCaptured += 1
                    Dim packet2 As EncodedPacket = Nothing
                    Try
                        If _encoder.Encode(far2.Frame, packet2) AndAlso packet2 IsNot Nothing Then
                            videoFile.Write(packet2.Payload, 0, packet2.PayloadLength)
                            result.TotalVideoBytes += packet2.PayloadLength
                            result.FramesEncoded += 1
                            packet2.Dispose()
                        End If
                    Catch
                        result.NvencErrors += 1
                    End Try
                    far2.Frame?.Dispose()
                Loop

                _logger.Info("[session] Stopping encoder...")
                _encoder.Stop()

                ' ─── 7. Stop audio ───────────────────────────────────────
                _logger.Info("[session] Stopping audio capture...")
                _stopSignal = True
                audioCapture.StopRecording()
                ' Wait for RecordingStopped event (finalizes WAV)
                Thread.Sleep(500)

                ' ─── 8. Close video file ────────────────────────────────
                videoFile.Flush()
                videoFile.Dispose()
                videoFile = Nothing

                ' ─── 8b. Wrap raw H.264 into MP4 container ─────────────
                ' MuxCoordinator expects a container file (MP4), not raw H.264.
                ' FFmpeg reads raw H.264 with -f h264 -r <fps> and outputs MP4.
                _logger.Info("[session] Wrapping H.264 into MP4 container...")
                Dim refreshRate As Integer = 75  ' display refresh rate (TODO: expose from DdagrabBackend)
                Dim wrapArgs As String = $"-y -hide_banner -f h264 -r {refreshRate} -i ""{tempH264}"" -c:v copy ""{tempVideoMp4}"""
                _logger.Info($"[session] Wrap command: {_config.FFmpegPath} {wrapArgs}")
                Dim wrapPsi As New ProcessStartInfo With {
                    .FileName = _config.FFmpegPath,
                    .Arguments = wrapArgs,
                    .UseShellExecute = False,
                    .RedirectStandardError = True,
                    .CreateNoWindow = True
                }
                Try
                    Using wrapProc As Process = Process.Start(wrapPsi)
                        Dim wrapStderr As String = wrapProc.StandardError.ReadToEnd()
                        wrapProc.WaitForExit(30000)
                        If wrapProc.ExitCode <> 0 Then
                            _logger.Error($"[session] H.264 wrap failed: exit code {wrapProc.ExitCode}" &
                                          Environment.NewLine & "FFmpeg stderr:" & Environment.NewLine & wrapStderr)
                        Else
                            _logger.Info("[session] H.264 wrap succeeded")
                        End If
                    End Using
                Catch ex As Exception
                    _logger.Error($"[session] H.264 wrap threw: {ex.Message}")
                End Try

                result.AudioSamples = totalAudioSamples
                result.AudioBytes = totalAudioBytes
                result.ActualDurationSec = sw.Elapsed.TotalSeconds

                ' ─── 9. FFmpeg mux → MP4 ─────────────────────────────────
                _logger.Info("[session] Muxing video + audio → MP4...")
                Dim mux As New MuxCoordinator() With {
                    .FFmpegPath = _config.FFmpegPath,
                    .TempVideoPath = tempVideoMp4,
                    .TempSystemWavPath = tempWav,
                    .OutputPath = _config.OutputPath,
                    .HasSystemAudio = True,
                    .SystemVolume = 1.0F
                }

                ' Probe video duration
                mux.VideoDurationSec = result.ActualDurationSec
                Dim muxOk As Boolean = mux.Run()
                If muxOk Then
                    _logger.Info("[session] Mux succeeded — cleaning temp files")
                    mux.CleanupTempFiles()
                    ' Also delete raw H.264 + temp video MP4
                    Try : File.Delete(tempH264) : Catch : End Try
                    Try : File.Delete(tempVideoMp4) : Catch : End Try
                Else
                    _logger.Error("[session] Mux FAILED — keeping temp files for debugging")
                End If

                ' ─── 10. Verify MP4 ─────────────────────────────────────
                Dim fi As New FileInfo(_config.OutputPath)
                result.FileExists = fi.Exists
                result.FileSize = If(fi.Exists, fi.Length, 0)

                If fi.Exists Then
                    ' Verify streams via FFmpeg -i (info mode)
                    Dim verifyPsi As New ProcessStartInfo With {
                        .FileName = _config.FFmpegPath,
                        .Arguments = $"-hide_banner -i ""{_config.OutputPath}""",
                        .UseShellExecute = False,
                        .RedirectStandardError = True,
                        .CreateNoWindow = True
                    }
                    Try
                        Using verifyProc As Process = Process.Start(verifyPsi)
                            Dim stderr As String = verifyProc.StandardError.ReadToEnd()
                            verifyProc.WaitForExit(5000)
                            result.VideoStreamFound = stderr.Contains("Stream #") AndAlso stderr.Contains("Video:")
                            result.AudioStreamFound = stderr.Contains("Stream #") AndAlso stderr.Contains("Audio:")
                        End Using
                    Catch
                    End Try
                End If

                _logger.Info($"[session] Result: pass={result.Pass}, frames={result.FramesEncoded}, " &
                             $"video_bytes={result.TotalVideoBytes}, audio_samples={result.AudioSamples}, " &
                             $"file_size={result.FileSize}")

            Catch ex As Exception
                result.ErrorMessage = ex.Message
                _logger.Error($"[session] Failed: {ex.Message}", ex)
            Finally
                Try : videoFile?.Dispose() : Catch : End Try
                Try : wavWriter?.Dispose() : Catch : End Try
                Try : audioCapture?.Dispose() : Catch : End Try
                Try : sink?.Dispose() : Catch : End Try
            End Try

            Return result
        End Function

        Public Sub [Stop]()
            _stopSignal = True
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            _stopSignal = True
            ' NOTE: does NOT dispose _capture or _encoder — those are owned by RecordingEngine
        End Sub

    End Class

End Namespace
