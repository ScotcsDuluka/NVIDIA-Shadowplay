Option Strict On
Option Explicit On
Option Infer On

' AudioRenderer.vb — WasapiOut render side over the bounded PCM ring
' (design doc §2.1 "audio output stack" + §3.6 master clock).
'
' OWNER RULE honored: NAudio WasapiOut IS the product's existing audio-output
' abstraction (proven in CaptureEngine.Recording/SilenceKeepAlive.vb with the
' exact constructor used here). NO new audio stack, no duplicated abstraction.
'
' RUNTIME GATE (honest): WasapiOut is Windows-only. TryCreate returns Nothing
' (never throws) off-Windows or when no render endpoint exists — the session
' then degrades to video-only mode, COUNTED (§7 spirit: degraded, not hidden).
'
' Clock: every byte read into the device is S16LE samples; samplesPlayed/rate
' is the audio master clock in the SAME 100-ns domain as PlaybackClock (§3.6 —
' one timestamp domain for the whole session).

Imports System
Imports System.Runtime.InteropServices
Imports System.Threading
Imports NAudio.CoreAudioApi
Imports NAudio.Wave

Namespace Gallery.Video

    Public NotInheritable Class AudioRenderer
        Implements IDisposable

        Private ReadOnly _buffer As AudioPcmBuffer
        Private ReadOnly _format As WaveFormat
        Private ReadOnly _provider As PcmBufferWaveProvider
        Private ReadOnly _out As WasapiOut
        Private _disposed As Integer = 0
        Private ReadOnly _deviceName As String = ""

        Private Sub New(buffer As AudioPcmBuffer, sampleRate As Integer, channels As Integer,
                        deviceName As String, out As WasapiOut)
            _buffer = buffer
            _deviceName = deviceName
            _format = New WaveFormat(sampleRate, 16, channels)
            _provider = New PcmBufferWaveProvider(buffer, _format, sampleRate, channels)
            _out = out
            _out.Init(_provider)
        End Sub

        ''' <summary>Factory: Nothing = no audio output available (honest gate).
        ''' The constructor pattern is cloned from SilenceKeepAlive.vb:72
        ''' (MMDevice default endpoint, Shared, event-sync, 50 ms latency).</summary>
        Public Shared Function TryCreate(buffer As AudioPcmBuffer,
                                         sampleRate As Integer, channels As Integer) As AudioRenderer
            If buffer Is Nothing Then Return Nothing
            If Not RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then Return Nothing

            Try
                Dim device As MMDevice
                Using enumr As New MMDeviceEnumerator()
                    device = enumr.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)
                End Using
                Dim name = If(device?.FriendlyName, "?")
                Dim outp = New WasapiOut(device, AudioClientShareMode.Shared, True, 50)
                Return New AudioRenderer(buffer, sampleRate, channels, name, outp)
            Catch
                ' No endpoint / device busy / COM failure → video-only fallback.
                Return Nothing
            End Try
        End Function

        Public ReadOnly Property DeviceName As String
            Get
                Return _deviceName
            End Get
        End Property

        Public ReadOnly Property IsPlaying As Boolean
            Get
                If _out Is Nothing OrElse Volatile.Read(_disposed) <> 0 Then Return False
                Try
                    Return _out.PlaybackState = NAudio.Wave.PlaybackState.Playing
                Catch
                    Return False
                End Try
            End Get
        End Property

        ''' <summary>Audio master clock: samples actually handed to the device,
        ''' in 100-ns ticks (same domain as PlaybackClock — §3.6).</summary>
        Public ReadOnly Property AudioPositionTicks As Long
            Get
                Return _provider.PlayedSamplesTicks
            End Get
        End Property

        Public Sub Play()
            If Volatile.Read(_disposed) <> 0 Then Return
            Try
                If _out.PlaybackState <> NAudio.Wave.PlaybackState.Playing Then _out.Play()
            Catch
            End Try
        End Sub

        Public Sub Pause()
            If Volatile.Read(_disposed) <> 0 Then Return
            Try
                If _out.PlaybackState = NAudio.Wave.PlaybackState.Playing Then _out.Pause()
            Catch
            End Try
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            If Interlocked.CompareExchange(_disposed, 1, 0) <> 0 Then Return
            Try
                If _out IsNot Nothing Then
                    _out.Stop()
                    _out.Dispose()
                End If
            Catch
            End Try
        End Sub

    End Class

    ''' <summary>IWaveProvider over the bounded AudioPcmRing. The NAudio device
    ''' thread pulls; Read waits briefly for a min chunk to avoid device
    ''' underruns on slow decode starts, then returns what exists (possibly 0).</summary>
    Public NotInheritable Class PcmBufferWaveProvider
        Implements IWaveProvider

        Private ReadOnly _buffer As AudioPcmBuffer
        Private ReadOnly _format As WaveFormat
        Private ReadOnly _sampleRate As Integer
        Private ReadOnly _channels As Integer
        Private _playedSamples As Long

        Public Sub New(buffer As AudioPcmBuffer, format As WaveFormat,
                       sampleRate As Integer, channels As Integer)
            _buffer = buffer
            _format = format
            _sampleRate = Math.Max(1, sampleRate)
            _channels = Math.Max(1, channels)
        End Sub

        Public ReadOnly Property WaveFormat As WaveFormat Implements IWaveProvider.WaveFormat
            Get
                Return _format
            End Get
        End Property

        ''' <summary>Samples delivered × 1e7 / rate = position in 100-ns ticks.</summary>
        Public ReadOnly Property PlayedSamplesTicks As Long
            Get
                Return Interlocked.Read(_playedSamples) * 10000000L \ _sampleRate
            End Get
        End Property

        Public Function Read(dest() As Byte, offset As Integer, count As Integer) As Integer Implements IWaveProvider.Read
            If count <= 0 Then Return 0
            ' Min chunk: 20 ms of S16 — smooths bursty pipe reads without
            ' busy-spinning the device thread.
            Dim minBytes = Math.Min(count, Math.Max(2, _sampleRate \ 50 * _channels * 2))
            Dim got = _buffer.Read(dest, offset, count, minBytes, 20)
            If got <= 0 Then Return 0
            Dim whole = got \ (_channels * 2) * (_channels * 2) ' whole frames only
            If whole <= 0 Then Return 0
            Interlocked.Add(_playedSamples, whole \ (_channels * 2))
            Return whole
        End Function

    End Class

End Namespace
