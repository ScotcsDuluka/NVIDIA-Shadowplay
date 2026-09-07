Imports System.Diagnostics
Imports System.IO
Imports System.Threading.Tasks




















Public Module EncoderService

    
    
    
    Private _cache As New Dictionary(Of String, Boolean)()
    Private ReadOnly _lock As New Object()

    Private Const VERIFY_TIMEOUT_MS As Integer = 5000

    
    
    
    
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

    
    
    
    
    Public Sub ClearCache()
        SyncLock _lock
            _cache.Clear()
        End SyncLock
    End Sub

    
    
    
    
    
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
