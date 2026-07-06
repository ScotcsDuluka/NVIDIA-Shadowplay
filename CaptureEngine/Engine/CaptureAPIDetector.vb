Imports System.Diagnostics
Imports System.IO

Namespace CaptureCore

    ''' <summary>
    ''' ★ Phase 2 refactor: Capture API availability detection.
    '''
    ''' WHY THIS EXISTS:
    '''   Before this file, the GFxCapture / DDAGrab availability checks
    '''   lived inside ScreenRecorder as Shared members. They have NO
    '''   dependency on instance state — they only spawn FFmpeg test
    '''   processes and cache the result. Moving them here shrinks
    '''   ScreenRecorder and gives the detection logic a clear home.
    '''
    '''   Instance-level methods that USE these results
    '''   (DetermineBestCaptureAPI, GetAvailableCaptureAPIs,
    '''   ResolvedCaptureAPI, ResolvedCaptureAPIDescription,
    '''   RequiresHDRSupport, NotifyCaptureAPIFailed) STAY in
    '''   ScreenRecorder because they depend on instance state
    '''   (_captureTargetType, _outputFormat, _encoder, _fallbackAPI, …).
    '''
    ''' MIGRATION POLICY:
    '''   ScreenRecorder.CheckGfxCaptureAvailabilityAsync(...) etc. remain
    '''   as thin forwarders, so external callers (Sub_Record.vb,
    '''   Base_RecordingsSet) keep working unchanged.
    ''' </summary>
    Public Module CaptureAPIDetector

        ' ════════════════════════════════════════════════════════════════════
        ' State — same shape as the original fields in ScreenRecorder.
        ' Kept Shared (Module-level) because the checks are global: the
        ' availability of gfxcapture/ddagrab does not change per recorder
        ' instance, only per FFmpeg build.
        ' ════════════════════════════════════════════════════════════════════
        Private _gfxcaptureAvailable As Boolean = False
        Private _gfxcaptureChecked As Boolean = False
        Private _ddagrabAvailable As Boolean = False
        Private _ddagrabChecked As Boolean = False
        Private _apiLock As New Object()

        ' Same timeout as the original ScreenRecorder.CAPTURE_API_CHECK_TIMEOUT.
        ' Duplicated here (instead of referencing CaptureLimits) because this
        ' is an implementation detail of detection, not a public limit.
        Private Const CAPTURE_API_CHECK_TIMEOUT As Integer = 3000

#Region "Public API"

        ''' <summary>
        ''' Fire-and-forget check of gfxcapture availability. Result is
        ''' cached internally and exposed via <see cref="IsGfxCaptureAvailable"/>.
        ''' </summary>
        Public Sub CheckGfxCaptureAvailabilityAsync(ffmpegPath As String)
            Task.Run(Sub()
                         Try
                             CheckGfxCaptureAvailability(ffmpegPath)
                         Catch ex As Exception
                             Debug.WriteLine("CheckGfxCaptureAvailabilityAsync Error: " & ex.Message)
                         End Try
                     End Sub)
        End Sub

        ''' <summary>
        ''' Fire-and-forget check of ddagrab availability. Result is
        ''' cached internally and exposed via <see cref="IsDDAGrabAvailable"/>.
        ''' </summary>
        Public Sub CheckDDAGrabAvailabilityAsync(ffmpegPath As String)
            Task.Run(Sub()
                         Try
                             CheckDDAGrabAvailability(ffmpegPath)
                         Catch ex As Exception
                             Debug.WriteLine("CheckDDAGrabAvailabilityAsync Error: " & ex.Message)
                         End Try
                     End Sub)
        End Sub

        Public Sub CheckGfxCaptureAvailability(ffmpegPath As String)
            SyncLock _apiLock
                If _gfxcaptureChecked Then Exit Sub

                Try
                    Debug.WriteLine("═══ CheckGfxCaptureAvailability START ═══")
                    Dim testArgs As String = "-filter_complex ""gfxcapture=monitor_idx=0:max_framerate=1:capture_cursor=0,hwdownload,format=bgra"" -t 0.1 -f null - -hide_banner -loglevel error"

                    Using proc As New Process()
                        proc.StartInfo = CreateProcessStartInfo(ffmpegPath, testArgs)
                        proc.Start()

                        Dim exited As Boolean = proc.WaitForExit(CAPTURE_API_CHECK_TIMEOUT)

                        If Not exited Then
                            Try
                                proc.Kill()
                                proc.WaitForExit(1000)
                            Catch
                            End Try
                            _gfxcaptureAvailable = False
                        Else
                            _gfxcaptureAvailable = (proc.ExitCode = 0)
                        End If
                    End Using
                Catch ex As Exception
                    Debug.WriteLine("CheckGfxCaptureAvailability Error: " & ex.Message)
                    _gfxcaptureAvailable = False
                Finally
                    _gfxcaptureChecked = True
                End Try
            End SyncLock
        End Sub

        Public Sub CheckDDAGrabAvailability(ffmpegPath As String)
            SyncLock _apiLock
                If _ddagrabChecked Then Exit Sub

                Try
                    Debug.WriteLine("═══ CheckDDAGrabAvailability START ═══")
                    Dim testArgs As String = "-f lavfi -i ""ddagrab=0:framerate=1:draw_mouse=0"" -t 0.1 -f null - -hide_banner -loglevel error"

                    Using proc As New Process()
                        proc.StartInfo = CreateProcessStartInfo(ffmpegPath, testArgs)
                        proc.Start()

                        Dim exited As Boolean = proc.WaitForExit(CAPTURE_API_CHECK_TIMEOUT)

                        If Not exited Then
                            Try
                                proc.Kill()
                                proc.WaitForExit(1000)
                            Catch
                            End Try
                            _ddagrabAvailable = False
                        Else
                            _ddagrabAvailable = (proc.ExitCode = 0)
                        End If
                    End Using
                Catch ex As Exception
                    Debug.WriteLine("CheckDDAGrabAvailability Error: " & ex.Message)
                    _ddagrabAvailable = False
                Finally
                    _ddagrabChecked = True
                End Try
            End SyncLock
        End Sub

        Public ReadOnly Property IsGfxCaptureAvailable As Boolean
            Get
                Return _gfxcaptureAvailable
            End Get
        End Property

        Public ReadOnly Property IsDDAGrabAvailable As Boolean
            Get
                Return _ddagrabAvailable
            End Get
        End Property

        ''' <summary>
        ''' True if <see cref="CheckGfxCaptureAvailability"/> has been called
        ''' at least once. Useful for callers that want to distinguish
        ''' "not available" from "not yet checked".
        ''' </summary>
        Public ReadOnly Property IsGfxCaptureChecked As Boolean
            Get
                Return _gfxcaptureChecked
            End Get
        End Property

        Public ReadOnly Property IsDDAGrabChecked As Boolean
            Get
                Return _ddagrabChecked
            End Get
        End Property

        Public Sub ResetAPIChecks()
            SyncLock _apiLock
                _gfxcaptureChecked = False
                _gfxcaptureAvailable = False
                _ddagrabChecked = False
                _ddagrabAvailable = False
            End SyncLock
        End Sub

#End Region

#Region "Private Helpers"

        ''' <summary>
        ''' Same as ScreenRecorder.CreateProcessStartInfo — kept private here
        ''' so this module is self-contained and doesn't reach back into
        ''' ScreenRecorder for a process-spawn helper.
        ''' </summary>
        Private Function CreateProcessStartInfo(ffmpegPath As String, arguments As String) As ProcessStartInfo
            Dim psi As New ProcessStartInfo() With {
                .FileName = ffmpegPath,
                .Arguments = arguments,
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True
            }
            Return psi
        End Function

#End Region

    End Module

End Namespace
