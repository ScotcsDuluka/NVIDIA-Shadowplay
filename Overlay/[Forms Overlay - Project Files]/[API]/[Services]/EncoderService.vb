Imports System.Diagnostics
Imports System.IO
Imports System.Threading.Tasks

''' <summary>
''' Phase 6 refactor: FFmpeg encoder availability service.
'''
''' WHY THIS EXISTS:
'''   Base_RecordingsSet (the Video Capture settings panel) used to carry a
'''   Shared encoder-availability cache + check logic + batch-verify logic
'''   all mixed in with the form's UI code. Those concerns have nothing to
'''   do with the form itself — they're cross-cutting encoder detection.
'''   Moved here so:
'''     - The cache + lock + verification live in one place.
'''     - Sub_Record.vb's ValidateEncoder/SelectBestEncoder can call
'''       EncoderService directly instead of going through Base_RecordingsSet.
'''     - Base_RecordingsSet no longer needs Shared state that isn't UI state.
'''
''' MIGRATION POLICY:
'''   Base_RecordingsSet.CheckEncoderAvailability / GetFFmpegCodecName /
'''   ClearEncoderAvailabilityCache remain as Shared forwarders so external
'''   callers continue to compile. They delegate to this module.
''' </summary>
Public Module EncoderService

    ' ════════════════════════════════════════════════════════════════════
    ' Cache + lock — same shape as the original fields in Base_RecordingsSet.
    ' ════════════════════════════════════════════════════════════════════
    Private _cache As New Dictionary(Of String, Boolean)()
    Private ReadOnly _lock As New Object()

    Private Const VERIFY_TIMEOUT_MS As Integer = 5000

    ''' <summary>
    ''' All encoder keys we know how to verify. Same list as the original
    ''' VerifyEncodersInBackground method.
    ''' </summary>
    Public ReadOnly Property AllEncoderKeys As String()
        Get
            Return {
                "NVENC_H264", "NVENC_HEVC", "NVENC_AV1",
                "QuickSync_H264", "QuickSync_HEVC",
                "AMF_H264", "AMF_HEVC",
                "LibX264", "LibX265"
            }
        End Get
    End Property

#Region "Public API"

    ''' <summary>
    ''' Returns True if the named encoder is available in the given FFmpeg
    ''' build. Result is cached per encoderName. If FFmpeg cannot be reached,
    ''' returns False.
    ''' </summary>
    Public Function CheckAvailability(ffmpegPath As String, encoderName As String) As Boolean
        SyncLock _lock
            If _cache.ContainsKey(encoderName) Then
                Return _cache(encoderName)
            End If
        End SyncLock

        If String.IsNullOrEmpty(ffmpegPath) OrElse Not File.Exists(ffmpegPath) Then
            Return False
        End If

        Try
            Dim codecName As String = GetFFmpegCodecName(encoderName)
            If String.IsNullOrEmpty(codecName) Then Return False

            Using proc As New Process()
                proc.StartInfo = New ProcessStartInfo() With {
                    .FileName = ffmpegPath,
                    .Arguments = "-hide_banner -encoders",
                    .UseShellExecute = False,
                    .CreateNoWindow = True,
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .StandardOutputEncoding = System.Text.Encoding.UTF8
                }

                proc.Start()

                Dim stdoutTask As Task(Of String) = proc.StandardOutput.ReadToEndAsync()
                Dim stderrTask As Task(Of String) = proc.StandardError.ReadToEndAsync()

                If proc.WaitForExit(1500) Then
                    Dim output As String = stdoutTask.Result
                    Dim isAvailable As Boolean = output.Contains(codecName)

                    SyncLock _lock
                        _cache(encoderName) = isAvailable
                    End SyncLock

                    Return isAvailable
                Else
                    Try
                        proc.Kill()
                    Catch
                    End Try

                    Try
                        stdoutTask.Wait(1000)
                    Catch
                    End Try
                    Try
                        stderrTask.Wait(1000)
                    Catch
                    End Try

                    Return False
                End If
            End Using
        Catch ex As Exception
            Debug.WriteLine("EncoderService.CheckAvailability Error: " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Maps an encoder key (e.g. "NVENC_HEVC") to its FFmpeg codec name
    ''' (e.g. "hevc_nvenc"). Returns Nothing if unknown.
    ''' </summary>
    Public Function GetFFmpegCodecName(encoderName As String) As String
        Select Case encoderName
            Case "NVENC_H264" : Return "h264_nvenc"
            Case "NVENC_HEVC" : Return "hevc_nvenc"
            Case "NVENC_AV1" : Return "av1_nvenc"
            Case "QuickSync_H264" : Return "h264_qsv"
            Case "QuickSync_HEVC" : Return "hevc_qsv"
            Case "AMF_H264" : Return "h264_amf"
            Case "AMF_HEVC" : Return "hevc_amf"
            Case "LibX264" : Return "libx264"
            Case "LibX265" : Return "libx265"
            Case Else : Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Clears the availability cache. Call this when the FFmpeg binary path
    ''' changes, since availability is per-build.
    ''' </summary>
    Public Sub ClearCache()
        SyncLock _lock
            _cache.Clear()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Spawns FFmpeg once with -encoders and updates the cache for every
    ''' known encoder key. Faster than calling CheckAvailability nine times
    ''' because we read FFmpeg's encoder list only once.
    ''' </summary>
    Public Sub VerifyAllInBackground(ffmpegPath As String)
        Try
            Using proc As New Process()
                proc.StartInfo = New ProcessStartInfo() With {
                    .FileName = ffmpegPath,
                    .Arguments = "-hide_banner -encoders",
                    .UseShellExecute = False,
                    .CreateNoWindow = True,
                    .RedirectStandardOutput = True,
                    .RedirectStandardError = True,
                    .StandardOutputEncoding = System.Text.Encoding.UTF8
                }

                proc.Start()

                Dim stdoutTask As Task(Of String) = proc.StandardOutput.ReadToEndAsync()
                Dim stderrTask As Task(Of String) = proc.StandardError.ReadToEndAsync()

                If proc.WaitForExit(VERIFY_TIMEOUT_MS) Then
                    Dim output As String = stdoutTask.Result

                    Debug.WriteLine("=== FFmpeg Encoder Verification ===")
                    SyncLock _lock
                        For Each encoderName As String In AllEncoderKeys
                            Dim codecName As String = GetFFmpegCodecName(encoderName)
                            If Not String.IsNullOrEmpty(codecName) Then
                                Dim available As Boolean = output.Contains(codecName)
                                _cache(encoderName) = available
                                Debug.WriteLine("  " & codecName & ": " & available.ToString())
                            End If
                        Next
                    End SyncLock
                    Debug.WriteLine("====================================")
                Else
                    Try
                        proc.Kill()
                    Catch
                    End Try
                    Debug.WriteLine("EncoderService.VerifyAllInBackground: FFmpeg timed out")
                End If

                stdoutTask.Wait(3000)
                stderrTask.Wait(3000)
            End Using
        Catch ex As Exception
            Debug.WriteLine("EncoderService.VerifyAllInBackground Error: " & ex.Message)
        End Try
    End Sub

#End Region

End Module
