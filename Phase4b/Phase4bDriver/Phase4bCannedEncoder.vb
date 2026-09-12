Option Strict On
Option Explicit On
Option Infer On

' Phase4bCannedEncoder.vb — Phase 4B experiment-only encoder backend.
'
' Implements IEncoderBackend with the SAME Encode contract and the SAME
' forensic instrumentation format as production NvencEncoderBackend
' ("FRAME n ENCODER INPUT/OUTPUT"), but the payload is a pre-generated,
' REAL, valid H.264 access unit (SPS+PPS+IDR, 64x64, libx264 keyint=1)
' embedded at build time. Every Encode() returns the same valid AU, so:
'   - the live mux receives a well-formed CFR H.264 elementary stream,
'   - the final MP4 is genuinely playable and ffprobe-able (MP4 PTS stage
'     of the causality chain is REAL),
'   - encode cost is ~0 → the encoder stage contributes no timing noise.
'
' PTS propagation mirrors NVENC exactly:
'   packet.PresentationTimestampTicks = frame.Diagnostics.PresentationTimestampTicks
'
' On the NVIDIA machine, the runbook runs the REAL NvencEncoderBackend
' instead — this class is never used there.

Imports System
Imports System.Diagnostics
Imports System.Threading
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Encoder
Imports CaptureEngine.Video

Namespace Phase4b

    Public NotInheritable Class Phase4bCannedEncoder
        Implements IEncoderBackend
        Implements IEncoderDiagnostics

        ' Real H.264 SPS+PPS+IDR access unit (64x64 black, x264 ultrafast,
        ' keyint=1, source r=60 so the SPS timing matches the mux-declared
        ' 60fps) generated once with the product-tree ffmpeg — see
        ' Phase4b\canned-frame.h264.
        Private Shared ReadOnly CannedAuBase64 As String =
            "AAAAAWdCwArcQmwEQAAAAwBAAAAeA8SJ4AAAAAFozg/IAAABBgX//z/cRem95tlIt5Ys2CDZI+7veDI2NCAtIGNvcmUgMTY1IC0gSC4yNjQvTVBFRy00IEFWQyBjb2RlYyAtIENvcHlsZWZ0IDIwMDMtMjAyNSAtIGh0dHA6Ly93d3cudmlkZW9sYW4ub3JnL3gyNjQuaHRtbCAtIG9wdGlvbnM6IGNhYmFjPTAgcmVmPTEgZGVibG9jaz0wOjA6MCBhbmFseXNlPTA6MCBtZT1kaWEgc3VibWU9MCBwc3k9MSBwc3lfcmQ9MS4wMDowLjAwIG1peGVkX3JlZj0wIG1lX3JhbmdlPTE2IGNocm9tYV9tZT0xIHRyZWxsaXM9MCA4eDhkY3Q9MCBjcW09MCBkZWFkem9uZT0yMSwxMSBmYXN0X3Bza2lwPTEgY2hyb21hX3FwX29mZnNldD0wIHRocmVhZHM9MiBsb29rYWhlYWRfdGhyZWFkcz0xIHNsaWNlZF90aHJlYWRzPTAgbnI9MCBkZWNpbWF0ZT0xIGludGVybGFjZWQ9MCBibHVyYXlfY29tcGF0PTAgY29uc3RyYWluZWRfaW50cmE9MCBiZnJhbWVzPTAgd2VpZ2h0cD0wIGtleWludD0xIGtleWludF9taW49MSBzY2VuZWN1dD0wIGludHJhX3JlZnJlc2g9MCByYz1jcmYgbWJ0cmVlPTAgY3JmPTIzLjAgcWNvbXA9MC42MCBxcG1pbj0wIHFwbWF4PTY5IHFwc3RlcD00IGlwX3JhdGlvPTEuNDAgYXE9MACAAAABZYiEOiYoAAkCycnJ1111111111114A=="

        Private Shared _cannedAu As Byte() = Nothing

        Private ReadOnly _sync As New Object()
        Private ReadOnly _logger As EngineLogger

        Private _config As EncoderConfig
        Private _state As EncoderState = EncoderState.Created
        Private _disposed As Boolean = False
        Private _firstFrameOfSession As Boolean = False

        ' Diagnostics counters
        Private _submittedFrames As Long = 0
        Private _encodedPackets As Long = 0
        Private _droppedFrames As Long = 0
        Private _flushCycles As Long = 0
        Private _errorCount As Long = 0
        Private _lastErrorIfAny As String = ""
        Private _lastErrorType As String = ""

        Public Sub New(Optional logger As EngineLogger = Nothing)
            _logger = If(logger, New EngineLogger("Phase4bCannedEncoder"))
        End Sub

        Private Shared Function CannedAu() As Byte()
            If _cannedAu Is Nothing Then
                _cannedAu = Convert.FromBase64String(CannedAuBase64)
            End If
            Return _cannedAu
        End Function

        ' ===== IEncoderBackend =====

        Public ReadOnly Property Diagnostics As IEncoderDiagnostics Implements IEncoderBackend.Diagnostics
            Get
                Return Me
            End Get
        End Property

        Public ReadOnly Property CurrentState As EncoderState Implements IEncoderBackend.CurrentState
            Get
                SyncLock _sync
                    Return _state
                End SyncLock
            End Get
        End Property

        Public Sub Initialize(config As EncoderConfig) Implements IEncoderBackend.Initialize
            SyncLock _sync
                ThrowIfDisposed()
                If _state <> EncoderState.Created AndAlso _state <> EncoderState.Stopped Then
                    Throw New InvalidOperationException($"Initialize from state {_state}")
                End If
                If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))
                _config = config
                _state = EncoderState.Initialized
            End SyncLock
            _logger.Info($"Phase4bCannedEncoder: Initialize complete (canned real-H264 IDR AU {CannedAu().Length}B, codecKey={config.CodecKey}) — SYNTHETIC encoder (experiment)")
        End Sub

        Public Sub Start() Implements IEncoderBackend.Start
            SyncLock _sync
                ThrowIfDisposed()
                Select Case _state
                    Case EncoderState.Running
                        _logger.Warning("Phase4bCannedEncoder: Start ignored, already Running")
                        Return
                    Case EncoderState.Initialized, EncoderState.Stopped
                        _state = EncoderState.Running
                        _firstFrameOfSession = True
                        ' Per-session packet-sequence reset (log-gating only):
                        ' keeps the first-4 ENCODER OUTPUT instrumentation
                        ' active in EVERY session. Production NvencEncoderBackend
                        ' keeps the counter continuous across sessions — its
                        ' session-2+ ENCODER OUTPUT instrumentation is silent
                        ' by the same mechanism (documented in Phase 4B report).
                        Interlocked.Exchange(_encodedPackets, 0)
                    Case Else
                        Throw New InvalidOperationException($"Start from state {_state}")
                End Select
            End SyncLock
            _logger.Info("Phase4bCannedEncoder: started")
        End Sub

        Public Function Encode(frame As IVideoFrame, ByRef packet As EncodedPacket) As Boolean Implements IEncoderBackend.Encode
            packet = Nothing
            If frame Is Nothing Then Throw New ArgumentNullException(NameOf(frame))

            ' ★ FORENSIC INSTRUMENTATION — same format as NvencEncoderBackend
            Dim frameSequence As Long = frame.Diagnostics.Sequence
            If frameSequence <= 4 Then
                Dim encodeInputTick As Long = Stopwatch.GetTimestamp()
                _logger.Info($"NvencEncoderBackend: FRAME {frameSequence} ENCODER INPUT:")
                _logger.Info($"  inputTick={encodeInputTick}")
                _logger.Info($"  inputQpc={encodeInputTick}")
                _logger.Info($"  frame.Diagnostics.PresentationTimestampTicks={frame.Diagnostics.PresentationTimestampTicks}")
                _logger.Info($"  frame.Diagnostics.CaptureTimeTicks={frame.Diagnostics.CaptureTimeTicks}")
            End If

            SyncLock _sync
                ThrowIfDisposed()
                If _state <> EncoderState.Running Then
                    Throw New InvalidOperationException($"Encode from state {_state} (must be Running)")
                End If
            End SyncLock

            Interlocked.Increment(_submittedFrames)

            Dim pts As Long = frame.Diagnostics.PresentationTimestampTicks
            Dim seq As Long = Interlocked.Increment(_encodedPackets) - 1
            Dim payload As Byte() = CannedAu()

            If _firstFrameOfSession Then
                _firstFrameOfSession = False
                _logger.Info("NvencEncoderBackend: first frame of session — FORCEIDR + SPS/PPS")
            End If

            Dim metadata As New PacketMetadata(
                sequence:=seq,
                presentationTimeTicks:=pts,
                decodingTimeTicks:=pts,
                durationTicks:=0,
                isKeyFrame:=True,
                isReferenceFrame:=True,
                codecKey:=_config.CodecKey,
                codecSpecificFlags:=0)

            ' ★ FORENSIC INSTRUMENTATION — same format as NvencEncoderBackend
            If seq <= 4 Then
                Dim encodeOutputTick As Long = Stopwatch.GetTimestamp()
                _logger.Info($"NvencEncoderBackend: FRAME {seq} ENCODER OUTPUT:")
                _logger.Info($"  encodeOutputTick={encodeOutputTick}")
                _logger.Info($"  encodeOutputQpc={encodeOutputTick}")
                _logger.Info($"  packet.PresentationTimestampTicks={pts}")
                _logger.Info($"  packet.IsKeyFrame=True")
            End If

            packet = New EncodedPacket(metadata, payload, payload.Length)
            Return True
        End Function

        Public Function Flush(sink As Action(Of EncodedPacket)) As Integer Implements IEncoderBackend.Flush
            SyncLock _sync
                ThrowIfDisposed()
                Interlocked.Increment(_flushCycles)
            End SyncLock
            ' Synchronous encoder: nothing in flight.
            Return 0
        End Function

        Public Sub [Stop]() Implements IEncoderBackend.[Stop]
            SyncLock _sync
                ThrowIfDisposed()
                If _state = EncoderState.Running Then
                    _state = EncoderState.Stopped
                End If
            End SyncLock
            _logger.Info("Phase4bCannedEncoder: stopped")
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _sync
                If _disposed Then Return
                _disposed = True
                _state = EncoderState.Disposed
            End SyncLock
            _logger.Info("Phase4bCannedEncoder: disposed")
        End Sub

        ' ===== IEncoderDiagnostics =====

        Public ReadOnly Property SubmittedFrames As Long Implements IEncoderDiagnostics.SubmittedFrames
            Get
                Return Interlocked.Read(_submittedFrames)
            End Get
        End Property
        Public ReadOnly Property EncodedPackets As Long Implements IEncoderDiagnostics.EncodedPackets
            Get
                Return Interlocked.Read(_encodedPackets)
            End Get
        End Property
        Public ReadOnly Property DroppedFrames As Long Implements IEncoderDiagnostics.DroppedFrames
            Get
                Return Interlocked.Read(_droppedFrames)
            End Get
        End Property
        Public ReadOnly Property FlushCycles As Long Implements IEncoderDiagnostics.FlushCycles
            Get
                Return Interlocked.Read(_flushCycles)
            End Get
        End Property
        Public ReadOnly Property ErrorCount As Long Implements IEncoderDiagnostics.ErrorCount
            Get
                Return Interlocked.Read(_errorCount)
            End Get
        End Property
        Public ReadOnly Property LastErrorIfAny As String Implements IEncoderDiagnostics.LastErrorIfAny
            Get
                Return _lastErrorIfAny
            End Get
        End Property
        Public ReadOnly Property LastErrorType As String Implements IEncoderDiagnostics.LastErrorType
            Get
                Return _lastErrorType
            End Get
        End Property

        Private Sub ThrowIfDisposed()
            If _disposed Then Throw New ObjectDisposedException(NameOf(Phase4bCannedEncoder))
        End Sub
    End Class

End Namespace
