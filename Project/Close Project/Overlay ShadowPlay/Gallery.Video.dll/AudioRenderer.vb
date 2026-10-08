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
        ''' in 100-ns ticks (same domain as PlaybackClock — §3.6).
        ''' W2 clock-domain fix: the counter is REBASED onto the absolute media
        ''' PTS domain at Play-start / Resume / Seek, so the value reads as
        ''' media position — comparable with PositionTicks — instead of a
        ''' raw cumulative counter that resets semantics on every generation.</summary>
        Public ReadOnly Property AudioPositionTicks As Long
            Get
                Return _provider.PlayedSamplesTicks
            End Get
        End Property

        ''' <summary>Rebase the audio position onto absolute media PTS: from now
        ''' on, samples played map to `mediaTicks` + progress. Must be called
        ''' with the SAME anchor value the PlaybackClock just adopted (Play /
        ''' Resume / Seek), before or atomically with the device (re)start.</summary>
        Public Sub RebaseTo(mediaTicks As Long)
            If Volatile.Read(_disposed) <> 0 Then Return
            _provider.RebaseTo(mediaTicks)
        End Sub

        ''' <summary>Device-pull underruns (ring empty at pull time — answered
        ''' with silence). Observability seam for the memory/queue stress proof.</summary>
        Public ReadOnly Property Underruns As Long
            Get
                Return _provider.Underruns
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

        ' W2 clock-domain fix: rebase bookkeeping. The played-sample counter
        ' is cumulative since provider creation; RebaseTo pins the counter
        ' value that corresponds to an ABSOLUTE media PTS anchor, so the
        ' exposed position reads in the media domain (comparable with the
        ' PlaybackClock) rather than restarting at 0 in a stale domain.
        Private _rebaseAnchorTicks As Long = 0
        Private _rebaseBaseSamples As Long = 0
        Private _rebaseSeq As Integer = 0   ' publish sequence for lock-free readers

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

        ''' <summary>Rebase: `mediaTicks` happens NOW at the current sample
        ''' position. Lock-free publication (base → anchor → seq); readers
        ''' retry when they observe a torn rebase.</summary>
        Public Sub RebaseTo(mediaTicks As Long)
            Interlocked.Exchange(_rebaseBaseSamples, Interlocked.Read(_playedSamples))
            Interlocked.Exchange(_rebaseAnchorTicks, mediaTicks)
            Interlocked.Increment(_rebaseSeq)
        End Sub

        ''' <summary>Samples delivered → ABSOLUTE media position in 100-ns
        ''' ticks: anchor + (samples − base) × 1e7 / rate.</summary>
        Public ReadOnly Property PlayedSamplesTicks As Long
            Get
                Do
                    Dim seq1 = Interlocked.CompareExchange(_rebaseSeq, 0, 0)
                    Dim base = Interlocked.Read(_rebaseBaseSamples)
                    Dim anchor = Interlocked.Read(_rebaseAnchorTicks)
                    Dim played = Interlocked.Read(_playedSamples)
                    Dim seq2 = Interlocked.CompareExchange(_rebaseSeq, 0, 0)
                    If seq1 = seq2 Then
                        Dim delta = played - base
                        If delta < 0 Then delta = 0
                        Return anchor + delta * 10000000L \ _sampleRate
                    End If
                Loop
            End Get
        End Property

        ''' <summary>Device-pull underruns (ring empty at pull time). The
        ''' provider answers with SILENCE — never 0 (see Read) — so the device
        ''' pull loop stays alive; the counter only moves for REAL samples.</summary>
        Public ReadOnly Property Underruns As Long
            Get
                Return Interlocked.Read(_underruns)
            End Get
        End Property
        Private _underruns As Long = 0

        Public Function Read(dest() As Byte, offset As Integer, count As Integer) As Integer Implements IWaveProvider.Read
            If count <= 0 Then Return 0
            ' Min chunk: 20 ms of S16 — smooths bursty pipe reads without
            ' busy-spinning the device thread.
            Dim minBytes = Math.Min(count, Math.Max(2, (_sampleRate \ 50) * _channels * 2))
            Dim got = _buffer.Read(dest, offset, count, minBytes, 20)
            ' ★ W2: VB precedence fix — the unparenthesized form
            ' `got \ (ch*2) * (ch*2)` evaluated as `got \ ((ch*2)*(ch*2))`
            ' = got: the provider returned 1/16 of the decoded audio and
            ' under-counted playedSamples 16× (audio master clock 16× slow).
            Dim wholeFrames = got \ (_channels * 2)
            Dim whole = wholeFrames * (_channels * 2) ' whole frames only

            If got <= 0 OrElse whole <= 0 Then
                ' ★ W2 audio lifecycle fix: an IWaveProvider that returns 0
                ' signals END-OF-STREAM — NAudio's pull loop stops PERMANENTLY
                ' (measured: deviceReads=1 forever, samples frozen at 0, ring
                ' full — the opening frames of decode arrive after the first
                ' pull). Underrun = pad SILENCE for one min-chunk instead, so
                ' the device stays alive; real samples still drive the counter,
                ' and the session's advance-gated master clock falls back to
                ' QPC while the ring refills.
                Interlocked.Increment(_underruns)
                Dim silence = Math.Min(count, minBytes)
                silence = (silence \ (_channels * 2)) * (_channels * 2) ' frame-aligned
                If silence > 0 Then Array.Clear(dest, offset, silence)
                Return silence
            End If

            Interlocked.Add(_playedSamples, whole \ (_channels * 2))
            Return whole
        End Function

    End Class

End Namespace
