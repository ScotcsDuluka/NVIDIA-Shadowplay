Imports NAudio.Wave
Imports System.IO
Imports System.Math

''' <summary>
''' Detects game highlights by matching live audio against pre-recorded .mp3 templates.
''' Uses WASAPI loopback capture (captures speaker output — no microphone needed)
''' and FFT-based spectral comparison.
''' 
''' Singleton pattern — use HighlightDetector.Instance to access.
''' Templates are loaded from: Application.StartupPath\NVIDIA_Shadowplay_Data\highlights\
''' Supported formats: .mp3, .wav, .ogg, .flac, .m4a
''' 
''' Requires: NAudio NuGet package
''' </summary>
Public Class HighlightDetector
    Implements IDisposable

#Region "Singleton"

    Private Shared ReadOnly _instance As New Lazy(Of HighlightDetector)(Function() New HighlightDetector())

    ''' <summary>Shared singleton instance — use this everywhere.</summary>
    Public Shared ReadOnly Property Instance As HighlightDetector
        Get
            Return _instance.Value
        End Get
    End Property

#End Region

#Region "Events"

    ''' <summary>Fired when a highlight sound is detected (e.g., kill sound matched).</summary>
    Public Event HighlightDetected(templateName As String, timestamp As DateTime)

#End Region

#Region "Nested Types"

    ''' <summary>
    ''' Pre-computed audio template loaded from an audio file.
    ''' Stores spectral fingerprint for fast comparison at runtime.
    ''' </summary>
    Public Class AudioTemplate
        Public Property Name As String
        Public Property FilePath As String
        Public Property Threshold As Single = 0.55F
        Public Property CooldownMs As Integer = 3000
        Public Property LastDetected As DateTime = DateTime.MinValue

        ''' <summary>Pre-computed spectral fingerprints (multiple FFT frames).</summary>
        Public Property SpectralFrames As New List(Of Single())()

        ''' <summary>Duration in seconds.</summary>
        Public Property Duration As Double

        ''' <summary>Whether this template is enabled for detection.</summary>
        Public Property Enabled As Boolean = True
    End Class

    Public Enum DetectionState
        Stopped
        Starting
        Running
        [Error]
    End Enum

#End Region

#Region "Constants & Fields"

    Private Const HIGHLIGHTS_FOLDER As String = "NVIDIA_Shadowplay_Data\highlights"

    ' Audio processing constants
    Private Const SAMPLE_RATE As Integer = 16000      ' Downsampled for efficiency
    Private Const FFT_SIZE As Integer = 512           ' 32ms at 16kHz
    Private Const HOP_SIZE As Integer = 256           ' 50% overlap
    Private Const ANALYSIS_INTERVAL_MS As Integer = 100  ' Check every 100ms
    Private Const RING_BUFFER_SECONDS As Single = 10.0F  ' Keep 10s of audio
    Private Const RMS_SILENCE_THRESHOLD As Single = 0.002F  ' Skip silence (low)

    Private _state As DetectionState = DetectionState.Stopped
    Private _templates As New List(Of AudioTemplate)()
    Private _isDisposed As Boolean = False

    ' Audio capture (WASAPI loopback — captures what speakers play)
    Private _capture As WasapiLoopbackCapture
    Private _ringBuffer As New List(Of Single)()
    Private _maxBufferSamples As Integer
    Private _captureWaveFormat As WaveFormat

    ' Resampling: capture rate → 16kHz via decimation
    Private _resampleRatio As Double = 1.0
    Private _resampleAccumulator As Double = 0.0

    ' Debug: last detection scores
    Private _lastRMS As Single = 0.0F
    Private _lastBestSimilarity As Single = 0.0F
    Private _lastBestTemplate As String = ""

    ' Analysis timer
    Private WithEvents _analysisTimer As System.Windows.Forms.Timer

    ' Thread safety
    Private ReadOnly _lock As New Object()

#End Region

#Region "Properties"

    ''' <summary>Current detection state.</summary>
    Public ReadOnly Property State As DetectionState
        Get
            Return _state
        End Get
    End Property

    ''' <summary>List of loaded audio templates.</summary>
    Public ReadOnly Property Templates As IReadOnlyList(Of AudioTemplate)
        Get
            Return _templates.AsReadOnly()
        End Get
    End Property

    ''' <summary>Default similarity threshold (0.0 - 1.0) for new templates.</summary>
    Public Property DefaultThreshold As Single = 0.55F

    ''' <summary>Default cooldown in ms between detections of the same template.</summary>
    Public Property DefaultCooldownMs As Integer = 3000

    ''' <summary>Full path to the highlights folder.</summary>
    Public ReadOnly Property HighlightsFolderPath As String
        Get
            Return Path.Combine(System.Windows.Forms.Application.StartupPath, HIGHLIGHTS_FOLDER)
        End Get
    End Property

    ''' <summary>Latest RMS level of captured audio (for debugging).</summary>
    Public ReadOnly Property LastRMS As Single
        Get
            Return _lastRMS
        End Get
    End Property

    ''' <summary>Best similarity score from last analysis cycle (for debugging).</summary>
    Public ReadOnly Property LastBestSimilarity As Single
        Get
            Return _lastBestSimilarity
        End Get
    End Property

    ''' <summary>Name of best-matching template from last cycle (for debugging).</summary>
    Public ReadOnly Property LastBestTemplate As String
        Get
            Return _lastBestTemplate
        End Get
    End Property

#End Region

#Region "Constructor / Dispose"

    ''' <summary>Private constructor — use HighlightDetector.Instance instead.</summary>
    Private Sub New()
        _maxBufferSamples = CInt(SAMPLE_RATE * RING_BUFFER_SECONDS)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _isDisposed Then
            [Stop]()
            _isDisposed = True
        End If
    End Sub

#End Region

#Region "Template Management"

    ''' <summary>
    ''' Loads all audio files from the highlights folder as detection templates.
    ''' Supported: .mp3, .wav, .ogg, .flac, .m4a
    ''' </summary>
    ''' <returns>Number of templates loaded.</returns>
    Public Function LoadTemplates() As Integer
        Dim folder As String = HighlightsFolderPath

        If Not Directory.Exists(folder) Then
            Directory.CreateDirectory(folder)
            Debug.WriteLine("HighlightDetector: Created highlights folder: " & folder)
            Return 0
        End If

        Dim count As Integer = 0
        Dim extensions As String() = {".mp3", ".wav", ".ogg", ".flac", ".m4a"}

        For Each file As String In Directory.GetFiles(folder)
            Dim ext As String = Path.GetExtension(file).ToLowerInvariant()
            If extensions.Contains(ext) Then
                If AddTemplate(file) Then
                    count += 1
                    Debug.WriteLine("HighlightDetector: Loaded template: " & Path.GetFileName(file))
                End If
            End If
        Next

        Debug.WriteLine("HighlightDetector: Total " & count & " templates loaded from " & folder)
        Return count
    End Function

    ''' <summary>
    ''' Adds a single audio file as a detection template.
    ''' Decodes, resamples to 16kHz mono, and pre-computes spectral fingerprint.
    ''' </summary>
    Public Function AddTemplate(filePath As String) As Boolean
        If Not File.Exists(filePath) Then Return False

        Try
            Dim template As New AudioTemplate() With {
                .Name = Path.GetFileNameWithoutExtension(filePath),
                .FilePath = filePath,
                .Threshold = DefaultThreshold,
                .CooldownMs = DefaultCooldownMs
            }

            ' Decode audio file → resample to 16kHz mono → get samples
            Dim samples As Single() = DecodeAudioToSamples(filePath)
            If samples Is Nothing OrElse samples.Length = 0 Then
                Debug.WriteLine("HighlightDetector: Failed to decode: " & filePath)
                Return False
            End If

            template.Duration = samples.Length / CDbl(SAMPLE_RATE)
            template.SpectralFrames = ComputeSpectralFrames(samples)

            If template.SpectralFrames.Count = 0 Then
                Debug.WriteLine("HighlightDetector: No spectral frames computed: " & filePath)
                Return False
            End If

            SyncLock _lock
                ' Remove existing template with same name
                _templates.RemoveAll(Function(t) t.Name = template.Name)
                _templates.Add(template)
            End SyncLock

            Debug.WriteLine("HighlightDetector: Template '" & template.Name & "' added (" & template.Duration.ToString("F1") & "s, " & template.SpectralFrames.Count & " frames, threshold=" & template.Threshold.ToString("F2") & ")")
            Return True

        Catch ex As Exception
            Debug.WriteLine("HighlightDetector AddTemplate Error: " & ex.Message)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Removes a template by name.
    ''' </summary>
    Public Sub RemoveTemplate(name As String)
        SyncLock _lock
            _templates.RemoveAll(Function(t) t.Name = name)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Removes all templates.
    ''' </summary>
    Public Sub ClearTemplates()
        SyncLock _lock
            _templates.Clear()
        End SyncLock
    End Sub

#End Region

#Region "Start / Stop"

    ''' <summary>
    ''' Starts listening for highlight sounds via WASAPI loopback capture.
    ''' Captures whatever the speakers are playing — no microphone needed.
    ''' </summary>
    Public Function Start() As Boolean
        If _state = DetectionState.Running Then Return True

        SyncLock _lock
            If _templates.Count = 0 Then
                Debug.WriteLine("HighlightDetector: No templates loaded, cannot start")
                Return False
            End If
        End SyncLock

        Try
            _state = DetectionState.Starting

            ' Initialize WASAPI loopback capture (captures speaker output)
            _capture = New WasapiLoopbackCapture()
            _captureWaveFormat = _capture.WaveFormat

            AddHandler _capture.DataAvailable, AddressOf OnDataAvailable
            AddHandler _capture.RecordingStopped, AddressOf OnRecordingStopped

            _ringBuffer.Clear()

            _capture.StartRecording()

            ' Calculate resample ratio (capture rate / target 16kHz)
            _resampleRatio = _captureWaveFormat.SampleRate / CDbl(SAMPLE_RATE)
            _resampleAccumulator = 0.0
            Debug.WriteLine("HighlightDetector: Resample " & _captureWaveFormat.SampleRate & "Hz → " & SAMPLE_RATE & "Hz (ratio=" & _resampleRatio.ToString("F2") & ")")

            ' Start analysis timer
            If _analysisTimer IsNot Nothing Then
                _analysisTimer.Stop()
            End If
            _analysisTimer = New System.Windows.Forms.Timer With {.Interval = ANALYSIS_INTERVAL_MS}
            _analysisTimer.Start()

            _state = DetectionState.Running
            Debug.WriteLine("HighlightDetector: Started (capture: " & _captureWaveFormat.SampleRate & "Hz, " & _captureWaveFormat.BitsPerSample & "bit, " & _captureWaveFormat.Channels & "ch)")
            Return True

        Catch ex As Exception
            Debug.WriteLine("HighlightDetector Start Error: " & ex.Message)
            _state = DetectionState.Error
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Stops listening and releases capture resources.
    ''' </summary>
    Public Sub [Stop]()
        If _analysisTimer IsNot Nothing Then
            _analysisTimer.Stop()
        End If

        If _capture IsNot Nothing Then
            Try
                _capture.StopRecording()
            Catch
            End Try
            RemoveHandler _capture.DataAvailable, AddressOf OnDataAvailable
            RemoveHandler _capture.RecordingStopped, AddressOf OnRecordingStopped
            _capture.Dispose()
            _capture = Nothing
        End If

        SyncLock _lock
            _ringBuffer.Clear()
        End SyncLock

        _state = DetectionState.Stopped
        Debug.WriteLine("HighlightDetector: Stopped")
    End Sub

#End Region

#Region "Audio Capture (WASAPI Loopback + Resample)"

    Private Sub OnDataAvailable(sender As Object, e As WaveInEventArgs)
        Try
            Dim bytesPerSample As Integer = _captureWaveFormat.BitsPerSample \ 8
            Dim channelCount As Integer = _captureWaveFormat.Channels
            Dim frameSize As Integer = bytesPerSample * channelCount
            Dim sampleCount As Integer = e.BytesRecorded \ frameSize

            SyncLock _lock
                ' ── Step 1: Extract mono samples from capture buffer ──
                Dim monoSamples(sampleCount - 1) As Single

                If _captureWaveFormat.BitsPerSample = 16 Then
                    For i As Integer = 0 To sampleCount - 1
                        Dim channelSum As Double = 0.0
                        For ch As Integer = 0 To channelCount - 1
                            Dim offset As Integer = (i * channelCount + ch) * 2
                            If offset + 1 < e.BytesRecorded Then
                                Dim intSample As Short = BitConverter.ToInt16(e.Buffer, offset)
                                channelSum += intSample / 32768.0
                            End If
                        Next
                        monoSamples(i) = CSng(channelSum / channelCount)
                    Next

                ElseIf _captureWaveFormat.BitsPerSample = 32 Then
                    For i As Integer = 0 To sampleCount - 1
                        Dim channelSum As Double = 0.0
                        For ch As Integer = 0 To channelCount - 1
                            Dim offset As Integer = (i * channelCount + ch) * 4
                            If offset + 3 < e.BytesRecorded Then
                                Dim floatSample As Single = BitConverter.ToSingle(e.Buffer, offset)
                                channelSum += floatSample
                            End If
                        Next
                        monoSamples(i) = CSng(channelSum / channelCount)
                    Next
                Else
                    Exit Sub
                End If

                ' ── Step 2: Resample from capture rate → 16kHz via decimation ──
                ' e.g. 48kHz → 16kHz: take every 3rd sample
                ' e.g. 44.1kHz → 16kHz: take every ~2.75 sample (use accumulator)
                For i As Integer = 0 To monoSamples.Length - 1
                    _resampleAccumulator += 1.0
                    If _resampleAccumulator >= _resampleRatio Then
                        _resampleAccumulator -= _resampleRatio
                        _ringBuffer.Add(monoSamples(i))
                    End If
                Next

                ' Trim ring buffer to max size
                While _ringBuffer.Count > _maxBufferSamples
                    _ringBuffer.RemoveAt(0)
                End While
            End SyncLock

        Catch ex As Exception
            Debug.WriteLine("HighlightDetector OnDataAvailable Error: " & ex.Message)
        End Try
    End Sub

    Private Sub OnRecordingStopped(sender As Object, e As StoppedEventArgs)
        If e.Exception IsNot Nothing Then
            Debug.WriteLine("HighlightDetector RecordingStopped Error: " & e.Exception.Message)
            _state = DetectionState.Error
        End If
    End Sub

#End Region

#Region "Audio Analysis (Main Detection Loop)"

    Private Sub _analysisTimer_Tick(sender As Object, e As EventArgs) Handles _analysisTimer.Tick
        If _state <> DetectionState.Running Then Exit Sub

        SyncLock _lock
            If _templates.Count = 0 Then Exit Sub
        End SyncLock

        Try
            CheckDetection()
        Catch ex As Exception
            Debug.WriteLine("HighlightDetector Analysis Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Main detection routine: grabs latest audio, computes spectra, compares with templates.
    ''' </summary>
    Private Sub CheckDetection()
        Dim currentSamples As Single()

        SyncLock _lock
            If _ringBuffer.Count < FFT_SIZE Then Exit Sub

            ' Calculate how many samples we need (enough for the longest template)
            Dim maxTemplateSamples As Integer = FFT_SIZE * 2
            For Each t As AudioTemplate In _templates
                Dim templateSamples As Integer = CInt(t.Duration * SAMPLE_RATE)
                If templateSamples > maxTemplateSamples Then maxTemplateSamples = templateSamples
            Next

            ' Take the latest samples from the ring buffer
            Dim takeCount As Integer = Math.Min(_ringBuffer.Count, maxTemplateSamples + FFT_SIZE)
            currentSamples = New Single(takeCount - 1) {}
            _ringBuffer.CopyTo(_ringBuffer.Count - takeCount, currentSamples, 0, takeCount)
        End SyncLock

        ' ── Step 1: Check RMS — skip if too quiet (saves CPU) ──
        Dim rms As Single = ComputeRMS(currentSamples)
        _lastRMS = rms
        If rms < RMS_SILENCE_THRESHOLD Then Exit Sub

        ' Reset best score for this cycle
        _lastBestSimilarity = 0.0F
        _lastBestTemplate = ""

        ' ── Step 2: Compute spectral frames for live audio ──
        Dim liveFrames As List(Of Single()) = ComputeSpectralFrames(currentSamples)
        If liveFrames.Count = 0 Then Exit Sub

        ' ── Step 3: Compare against each enabled template ──
        Dim now As DateTime = DateTime.Now

        SyncLock _lock
            For Each template As AudioTemplate In _templates
                ' Skip disabled templates
                If Not template.Enabled Then Continue For

                ' Skip if still in cooldown period
                If (now - template.LastDetected).TotalMilliseconds < template.CooldownMs Then
                    Continue For
                End If

                ' Compare spectra using sliding-window cosine similarity
                Dim similarity As Single = CompareSpectra(liveFrames, template.SpectralFrames)

                ' Track best score for debugging
                If similarity > _lastBestSimilarity Then
                    _lastBestSimilarity = similarity
                    _lastBestTemplate = template.Name
                End If

                If similarity >= template.Threshold Then
                    template.LastDetected = now

                    Debug.WriteLine("HighlightDetector: DETECTED '" & template.Name &
                                    "' (similarity=" & similarity.ToString("F3") &
                                    ", threshold=" & template.Threshold.ToString("F2") &
                                    ", rms=" & rms.ToString("F4") & ")")

                    ' Raise event
                    RaiseEvent HighlightDetected(template.Name, now)

                    ' Auto-enable replay if game is detected
                    TryAutoEnableReplay(template.Name)

                    ' Only detect one template per cycle (avoid double-fire)
                    Exit For
                End If
            Next
        End SyncLock
    End Sub

#End Region

#Region "Audio Processing Helpers"

    ''' <summary>
    ''' Decodes an audio file (.mp3, .wav, etc.) to mono 16kHz samples.
    ''' Uses NAudio for decoding and resampling.
    ''' </summary>
    Private Function DecodeAudioToSamples(filePath As String) As Single()
        Try
            Dim reader As WaveStream = Nothing
            Dim ext As String = Path.GetExtension(filePath).ToLowerInvariant()

            Select Case ext
                Case ".mp3"
                    reader = New Mp3FileReader(filePath)
                Case ".wav"
                    reader = New WaveFileReader(filePath)
                Case Else
                    ' Let NAudio auto-detect format
                    reader = New AudioFileReader(filePath)
            End Select

            If reader Is Nothing Then Return Nothing

            ' Resample to 16kHz mono using MediaFoundationResampler
            Dim targetFormat As New WaveFormat(SAMPLE_RATE, 16, 1)
            Dim resampler As New MediaFoundationResampler(reader, targetFormat)
            resampler.ResamplerQuality = 60

            ' Read all samples through ISampleProvider
            Dim sampleProvider As ISampleProvider = resampler.ToSampleProvider()
            Dim allSamples As New List(Of Single)()
            Dim buffer(4095) As Single
            Dim read As Integer

            Do
                read = sampleProvider.Read(buffer, 0, buffer.Length)
                If read > 0 Then
                    For i As Integer = 0 To read - 1
                        allSamples.Add(buffer(i))
                    Next
                End If
            Loop While read > 0

            resampler.Dispose()
            reader.Dispose()

            Return allSamples.ToArray()

        Catch ex As Exception
            Debug.WriteLine("HighlightDetector DecodeAudioToSamples Error (" & filePath & "): " & ex.Message)
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Computes spectral frames (FFT magnitude spectra) from a block of samples.
    ''' Each frame = FFT_SIZE samples with HOP_SIZE overlap, Hann-windowed.
    ''' </summary>
    Private Function ComputeSpectralFrames(samples As Single()) As List(Of Single())
        Dim frames As New List(Of Single())()

        If samples Is Nothing OrElse samples.Length < FFT_SIZE Then Return frames

        Dim pos As Integer = 0
        While pos + FFT_SIZE <= samples.Length
            ' Apply Hann window to reduce spectral leakage
            Dim windowed(FFT_SIZE - 1) As Single
            For i As Integer = 0 To FFT_SIZE - 1
                Dim hann As Single = CSng(0.5 * (1.0 - Cos(2.0 * Math.PI * i / (FFT_SIZE - 1))))
                windowed(i) = samples(pos + i) * hann
            Next

            ' Compute FFT magnitude spectrum
            Dim spectrum As Single() = ComputeFFTMagnitude(windowed)
            frames.Add(spectrum)

            pos += HOP_SIZE
        End While

        Return frames
    End Function

    ''' <summary>
    ''' Computes FFT magnitude spectrum from windowed samples.
    ''' Returns normalized magnitudes of the first half (N/2+1 bins).
    ''' </summary>
    Private Function ComputeFFTMagnitude(samples As Single()) As Single()
        Dim n As Integer = samples.Length
        Dim real(n - 1) As Single
        Dim imag(n - 1) As Single

        ' Copy input to real array
        Array.Copy(samples, real, n)

        ' In-place Cooley-Tukey radix-2 FFT
        FFT(real, imag)

        ' Compute magnitude of first half (positive frequencies only)
        Dim halfN As Integer = n \ 2 + 1
        Dim magnitude(halfN - 1) As Single

        For i As Integer = 0 To halfN - 1
            Dim re As Double = real(i)
            Dim im As Double = imag(i)
            magnitude(i) = CSng(Math.Sqrt(re * re + im * im))
        Next

        ' Normalize to 0-1 range
        Dim maxVal As Single = 0.0F
        For i As Integer = 0 To magnitude.Length - 1
            If magnitude(i) > maxVal Then maxVal = magnitude(i)
        Next

        If maxVal > 0.0F Then
            For i As Integer = 0 To magnitude.Length - 1
                magnitude(i) = magnitude(i) / maxVal
            Next
        End If

        Return magnitude
    End Function

    ''' <summary>
    ''' In-place Cooley-Tukey radix-2 FFT.
    ''' Input: real() and imag() arrays of length N (must be power of 2).
    ''' Output: real() and imag() contain the complex FFT result.
    ''' </summary>
    Private Sub FFT(real() As Single, imag() As Single)
        Dim n As Integer = real.Length

        ' ── Bit-reversal permutation ──
        Dim j As Integer = 0
        For i As Integer = 0 To n - 2
            If i < j Then
                ' Swap real
                Dim tempR As Single = real(i)
                real(i) = real(j)
                real(j) = tempR
                ' Swap imag
                Dim tempI As Single = imag(i)
                imag(i) = imag(j)
                imag(j) = tempI
            End If

            Dim k As Integer = n \ 2
            Do While k > 0 AndAlso j >= k
                j -= k
                k \= 2
            Loop
            j += k
        Next

        ' ── FFT butterfly stages ──
        Dim stride As Integer = 2
        Do While stride <= n
            Dim halfStride As Integer = stride \ 2
            Dim angle As Double = -2.0 * Math.PI / stride

            For s As Integer = 0 To n - 1 Step stride
                For k As Integer = 0 To halfStride - 1
                    Dim evenIdx As Integer = s + k
                    Dim oddIdx As Integer = s + k + halfStride

                    Dim tAngle As Double = angle * k
                    Dim twiddleReal As Single = CSng(Math.Cos(tAngle))
                    Dim twiddleImag As Single = CSng(Math.Sin(tAngle))

                    ' Complex multiplication: twiddle * odd
                    Dim tReal As Single = twiddleReal * real(oddIdx) - twiddleImag * imag(oddIdx)
                    Dim tImag As Single = twiddleReal * imag(oddIdx) + twiddleImag * real(oddIdx)

                    ' Butterfly: even ± t
                    real(oddIdx) = real(evenIdx) - tReal
                    imag(oddIdx) = imag(evenIdx) - tImag
                    real(evenIdx) = real(evenIdx) + tReal
                    imag(evenIdx) = imag(evenIdx) + tImag
                Next
            Next

            stride *= 2
        Loop
    End Sub

    ''' <summary>
    ''' Compares live spectral frames against a template using sliding-window
    ''' cosine similarity. Finds the best-matching segment in the live audio.
    ''' </summary>
    Private Function CompareSpectra(liveFrames As List(Of Single()), templateFrames As List(Of Single())) As Single
        If templateFrames.Count = 0 OrElse liveFrames.Count = 0 Then Return 0.0F
        If liveFrames.Count < templateFrames.Count Then Return 0.0F

        Dim maxSimilarity As Single = 0.0F
        Dim templateLen As Integer = templateFrames.Count

        ' Slide template over recent portion of live frames
        ' Only check the latest portion (not the entire 10s buffer)
        Dim searchStart As Integer = Math.Max(0, liveFrames.Count - templateLen * 3)
        Dim searchEnd As Integer = liveFrames.Count - templateLen

        For offset As Integer = searchStart To searchEnd
            Dim dotProduct As Double = 0.0
            Dim normA As Double = 0.0
            Dim normB As Double = 0.0

            For t As Integer = 0 To templateLen - 1
                Dim liveFrame As Single() = liveFrames(offset + t)
                Dim templateFrame As Single() = templateFrames(t)

                Dim minLen As Integer = Math.Min(liveFrame.Length, templateFrame.Length)

                For f As Integer = 0 To minLen - 1
                    dotProduct += liveFrame(f) * templateFrame(f)
                    normA += liveFrame(f) * liveFrame(f)
                    normB += templateFrame(f) * templateFrame(f)
                Next
            Next

            Dim similarity As Single
            If normA > 0.0 AndAlso normB > 0.0 Then
                similarity = CSng(dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB)))
            Else
                similarity = 0.0F
            End If

            If similarity > maxSimilarity Then
                maxSimilarity = similarity
            End If

            ' Early exit: if we already have a very strong match, no need to check further
            If maxSimilarity > 0.95F Then Exit For
        Next

        Return maxSimilarity
    End Function

    ''' <summary>
    ''' Computes RMS (Root Mean Square) energy of audio samples.
    ''' Used as a quick silence gate to skip quiet segments.
    ''' </summary>
    Private Function ComputeRMS(samples As Single()) As Single
        If samples Is Nothing OrElse samples.Length = 0 Then Return 0.0F

        Dim sumSquares As Double = 0.0
        For i As Integer = 0 To samples.Length - 1
            sumSquares += samples(i) * samples(i)
        Next

        Return CSng(Math.Sqrt(sumSquares / samples.Length))
    End Function

#End Region

#Region "Auto Replay Integration"

    ''' <summary>
    ''' When a highlight is detected and the game is recognized,
    ''' automatically enable replay recording.
    ''' </summary>
    Private Sub TryAutoEnableReplay(templateName As String)
        Try
            ' Check if recording/replay is already active
            If Base.ReplayValue OrElse Base.RecordValue Then
                ' Already recording — just bookmark the highlight timestamp
                Debug.WriteLine("HighlightDetector: Already recording, bookmarked '" & templateName & "'")
                Exit Sub
            End If

            ' Auto-enable replay if game is detected
            ' TODO: Integrate with game detection system
            Debug.WriteLine("HighlightDetector: Auto-replay triggered by '" & templateName & "'")

        Catch ex As Exception
            Debug.WriteLine("HighlightDetector TryAutoEnableReplay Error: " & ex.Message)
        End Try
    End Sub

#End Region

#Region "Utility"

    ''' <summary>
    ''' Resets cooldown for all templates (allows immediate re-detection).
    ''' </summary>
    Public Sub ResetCooldowns()
        SyncLock _lock
            For Each t As AudioTemplate In _templates
                t.LastDetected = DateTime.MinValue
            Next
        End SyncLock
    End Sub

    ''' <summary>
    ''' Returns info string about loaded templates and current state.
    ''' </summary>
    Public Function GetStatusInfo() As String
        SyncLock _lock
            Dim sb As New System.Text.StringBuilder()
            sb.AppendLine("State: " & _state.ToString())
            sb.AppendLine("Templates: " & _templates.Count)
            sb.AppendLine("Buffer: " & _ringBuffer.Count & " samples")
            sb.AppendLine("Last RMS: " & _lastRMS.ToString("F4") & " (threshold: " & RMS_SILENCE_THRESHOLD.ToString("F4") & ")")
            sb.AppendLine("Last Similarity: " & _lastBestSimilarity.ToString("F3") & " (" & _lastBestTemplate & ")")

            For Each t As AudioTemplate In _templates
                sb.AppendLine("  - " & t.Name & " (" & t.Duration.ToString("F1") & "s, threshold=" & t.Threshold.ToString("F2") & ", enabled=" & t.Enabled.ToString() & ")")
            Next

            Return sb.ToString()
        End SyncLock
    End Function

#End Region

End Class
