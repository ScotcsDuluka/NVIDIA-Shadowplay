Option Strict On
Option Explicit On
Option Infer On

' GalleryPlayerForm.vb — W2 Gallery playback screen (UI-side ONLY).
'
' Consumes the EXISTING Gallery.Video public API and nothing else:
'   PlaybackSession.Open / Play / Pause / Seek / Stop_ / Dispose
'   PlaybackSession.StateChanged / FaultRaised / EosReached / OpenCompleted
'   PlaybackSession.State / PositionTicks / DurationTicks / Media / Fault
' No engine file is modified; no new backend contract is introduced.
'
' Lifecycle discipline: ONE PlaybackSession per shown player; the session is
' always stopped + disposed before the form closes (the engine is never left
' running behind the UI). The WinForms UI timer only READS observability
' properties — it never blocks on the engine.

Imports System
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports Gallery.Video

Public Class GalleryPlayerForm
    Inherits System.Windows.Forms.Form

    ' ONE session per shown player; created at Open time (the D3D11 render
    ' target needs the panel's real window handle, which exists only after
    ' the form handle is created).
    Private _session As PlaybackSession = Nothing
    Private ReadOnly _filePath As String
    Private ReadOnly _displayName As String
    Private ReadOnly _ffmpegPath As String
    Private ReadOnly _ffprobePath As String
    Private ReadOnly _uiClock As New Timer With {.Interval = 250}

    ' Runtime-built controls (the app's convention: manual layout, no Designer)
    Private ReadOnly _videoPanel As New Panel()
    Private ReadOnly _transport As New Panel()
    Private ReadOnly _backBtn As New Label()
    Private ReadOnly _playBtn As New Label()
    Private ReadOnly _seekBar As New TrackBar()
    Private ReadOnly _timeLabel As New Label()
    Private ReadOnly _titleLabel As New Label()
    Private ReadOnly _centerState As New Label()

    Private _userDragging As Boolean = False
    Private _ended As Boolean = False
    Private _durationTicks As Long = 0

    Private Sub New(filePath As String, ffmpegPath As String, ffprobePath As String)
        _filePath = filePath
        _displayName = IO.Path.GetFileNameWithoutExtension(filePath)
        _ffmpegPath = ffmpegPath
        _ffprobePath = ffprobePath

        BuildUi()
    End Sub

    ''' <summary>Show the player for one file. Returns False (and shows nothing)
    ''' when the ffmpeg/ffprobe backend is not usable — honest gate, no crash.</summary>
    Public Shared Function TryShow(owner As Form, filePath As String) As Boolean
        Dim ffmpeg = ResolveFFmpeg()
        Dim ffprobe = If(String.IsNullOrEmpty(ffmpeg), "", IO.Path.Combine(IO.Path.GetDirectoryName(ffmpeg), "ffprobe.exe"))
        If String.IsNullOrEmpty(ffmpeg) OrElse Not FFmpegLocator.IsUsableFFmpeg(ffmpeg) OrElse
           Not IO.File.Exists(ffprobe) Then
            MessageBox.Show("ffmpeg/ffprobe backend not available for playback.", "Gallery",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End If

        Dim f As New GalleryPlayerForm(filePath, ffmpeg, ffprobe)
        f.Show(owner)
        f.BeginOpen()
        Return True
    End Function

    Friend Shared Function ResolveFFmpeg() As String
        Dim configured = AppSettings.Instance.Paths.FFmpegPath
        If Not String.IsNullOrEmpty(configured) AndAlso FFmpegLocator.IsUsableFFmpeg(configured) Then Return configured
        Dim candidates = {
            IO.Path.Combine(IO.Path.GetDirectoryName(Application.ExecutablePath), "API-Core", "ffmpeg.exe"),
            "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe"
        }
        Return FFmpegLocator.FirstUsableFFmpeg(candidates, Sub(m) ' quiet
                                                           End Sub)
    End Function

    ' ---- UI construction (runtime layout — app convention) ----

    Private Sub BuildUi()
        Text = "Gallery — " & _displayName
        FormBorderStyle = FormBorderStyle.Sizable
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1280, 760)
        BackColor = Color.FromArgb(12, 12, 14)
        KeyPreview = True
        DoubleBuffered = True

        _videoPanel.Dock = DockStyle.Fill
        _videoPanel.BackColor = Color.Black
        Controls.Add(_videoPanel)

        _transport.Dock = DockStyle.Bottom
        _transport.Height = 72
        _transport.BackColor = Color.FromArgb(20, 20, 24)
        Controls.Add(_transport)
        _transport.BringToFront()

        _backBtn.Text = "←"
        _backBtn.Font = New Font("Segoe UI", 14.0F, FontStyle.Bold)
        _backBtn.ForeColor = Color.White
        _backBtn.BackColor = Color.FromArgb(40, 40, 46)
        _backBtn.TextAlign = ContentAlignment.MiddleCenter
        _backBtn.Size = New Size(44, 40)
        _backBtn.Location = New Point(10, 16)
        _backBtn.Cursor = Cursors.Hand
        AddHandler _backBtn.Click, Sub(sender, e) Close()
        _transport.Controls.Add(_backBtn)

        _playBtn.Text = "⏸"
        _playBtn.Font = New Font("Segoe UI Symbol", 14.0F, FontStyle.Bold)
        _playBtn.ForeColor = Color.White
        _playBtn.BackColor = Color.FromArgb(40, 40, 46)
        _playBtn.TextAlign = ContentAlignment.MiddleCenter
        _playBtn.Size = New Size(44, 40)
        _playBtn.Location = New Point(64, 16)
        _playBtn.Cursor = Cursors.Hand
        AddHandler _playBtn.Click, Sub(sender, e) TogglePlayPause()
        _transport.Controls.Add(_playBtn)

        _seekBar.Minimum = 0
        _seekBar.Maximum = 1000
        _seekBar.TickStyle = TickStyle.None
        _seekBar.Size = New Size(820, 36)
        _seekBar.Location = New Point(120, 18)
        AddHandler _seekBar.MouseDown, Sub(sender, e) _userDragging = True
        AddHandler _seekBar.MouseUp,
            Sub(sender, e)
                If _durationTicks > 0 AndAlso Not _ended Then
                    Dim sec = _seekBar.Value / 1000.0 * (_durationTicks / 10000000.0)
                    _session.Seek(sec)
                End If
                _userDragging = False
            End Sub
        _transport.Controls.Add(_seekBar)

        _timeLabel.Text = "0:00 / 0:00"
        _timeLabel.Font = New Font("Consolas", 10.0F)
        _timeLabel.ForeColor = Color.Gainsboro
        _timeLabel.AutoSize = True
        _timeLabel.Location = New Point(950, 26)
        _transport.Controls.Add(_timeLabel)

        _titleLabel.Text = _displayName
        _titleLabel.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        _titleLabel.ForeColor = Color.White
        _titleLabel.AutoSize = True
        _titleLabel.Location = New Point(12, 8)
        _videoPanel.Controls.Add(_titleLabel)

        _centerState.Text = ""
        _centerState.Font = New Font("Segoe UI", 12.0F)
        _centerState.ForeColor = Color.Silver
        _centerState.AutoSize = True
        _centerState.Visible = False
        _videoPanel.Controls.Add(_centerState)

        AddHandler _uiClock.Tick, AddressOf UiClockTick
        AddHandler FormClosing, AddressOf OnPlayerFormClosing
        AddHandler _videoPanel.Resize, Sub(sender, e) CenterState()
    End Sub

    Private Sub CenterState()
        _centerState.Location = New Point(
            Math.Max(8, (_videoPanel.ClientSize.Width - _centerState.PreferredWidth) \ 2),
            Math.Max(8, (_videoPanel.ClientSize.Height - _centerState.PreferredHeight) \ 2))
    End Sub

    Private Sub ShowCenter(text As String)
        _centerState.Text = text
        _centerState.Visible = Not String.IsNullOrEmpty(text)
        CenterState()
        _centerState.BringToFront()
        _titleLabel.BringToFront()
    End Sub

    ' ---- engine integration ----

    Private Sub BeginOpen()
        ShowCenter("Opening…")
        SetPlayIcon(False)
        ' Engine API: PlaybackSessionOptions.RenderWindow = THIS panel's real
        ' handle — the D3D11 swapchain presents into the Gallery UI.
        _session = New PlaybackSession(New PlaybackSessionOptions With {
            .FfmpegExe = _ffmpegPath,
            .FfprobeExe = _ffprobePath,
            .RenderWindow = _videoPanel.Handle,
            .AudioEnabled = True,
            .OpenTimeoutMs = 15000
        })
        AddHandler _session.StateChanged, AddressOf SessionStateChanged
        AddHandler _session.FaultRaised, AddressOf SessionFault
        AddHandler _session.EosReached, AddressOf SessionEos
        _uiClock.Start()
        _session.Open(_filePath)
    End Sub

    Private Sub SessionStateChanged(sender As PlaybackSession, oldState As PlaybackState, newState As PlaybackState)
        If IsDisposed OrElse Disposing Then Return
        ' WinForms events from engine threads — marshal to the UI thread.
        BeginInvoke(Sub() OnStateChanged(newState))
    End Sub

    Private Sub OnStateChanged(newState As PlaybackState)
        Select Case newState
            Case PlaybackState.Playing
                ShowCenter("")
                SetPlayIcon(True)
            Case PlaybackState.Paused
                If _ended Then
                    ShowCenter("Ended — press ▶ to replay from the start")
                    SetPlayIcon(False)
                Else
                    ShowCenter("Paused")
                    SetPlayIcon(False)
                End If
            Case PlaybackState.Seeking
                ShowCenter("Seeking…")
            Case PlaybackState.Faulted
                ShowCenter("Playback failed: " & If(_session.Fault?.Kind.ToString(), "unknown"))
                SetPlayIcon(False)
        End Select
    End Sub

    Private Sub SessionFault(sender As PlaybackSession, f As GalleryVideoFault)
        If IsDisposed OrElse Disposing Then Return
        BeginInvoke(Sub()
                        ShowCenter("Playback failed: " & f.Kind.ToString() & " — " & f.Detail)
                        SetPlayIcon(False)
                    End Sub)
    End Sub

    Private Sub SessionEos(sender As PlaybackSession)
        If IsDisposed OrElse Disposing Then Return
        BeginInvoke(Sub()
                        _ended = True
                        SetPlayIcon(False)
                    End Sub)
    End Sub

    Private Sub TogglePlayPause()
        If _session Is Nothing Then Return
        Select Case _session.State
            Case PlaybackState.Playing
                _session.Pause()
            Case PlaybackState.Paused
                If _ended Then
                    ' Replay from the start (engine contract: Play-from-Stopped
                    ' restarts; from EOF-Paused, seek back then play).
                    _ended = False
                    _session.Seek(0.0)
                End If
                _session.Play()
        End Select
    End Sub

    Private Sub SetPlayIcon(playing As Boolean)
        _playBtn.Text = If(playing, "⏸", "▶")
    End Sub

    ' ---- progress (read-only observability, 4 Hz) ----

    Private Sub UiClockTick(sender As Object, e As EventArgs)
        If _session Is Nothing OrElse _userDragging OrElse IsDisposed OrElse Disposing Then Return
        _durationTicks = _session.DurationTicks
        Dim pos = _session.PositionTicks

        If pos >= 0 Then
            _timeLabel.Text = FormatTicks(pos) & " / " & FormatTicks(_durationTicks)
        End If
        If _durationTicks > 0 AndAlso pos >= 0 Then
            Dim frac = Math.Max(0, Math.Min(1000, CInt(pos * 1000.0 / _durationTicks)))
            If Not _userDragging Then
                _seekBar.Value = frac
            End If
        End If
    End Sub

    Private Shared Function FormatTicks(ticks As Long) As String
        If ticks < 0 Then ticks = 0
        Dim ts = TimeSpan.FromTicks(ticks)
        If ts.TotalHours >= 1 Then
            Return CInt(ts.TotalHours).ToString("0") & ":" & ts.Minutes.ToString("00") & ":" & ts.Seconds.ToString("00")
        End If
        Return ts.Minutes.ToString("0") & ":" & ts.Seconds.ToString("00")
    End Function

    ' ---- teardown: the engine NEVER outlives this form ----

    Private Sub OnPlayerFormClosing(sender As Object, e As FormClosingEventArgs)
        _uiClock.Stop()
        If _session Is Nothing Then Return
        Try
            _session.Stop_()
        Catch
        End Try
        RemoveHandler _session.StateChanged, AddressOf SessionStateChanged
        RemoveHandler _session.FaultRaised, AddressOf SessionFault
        RemoveHandler _session.EosReached, AddressOf SessionEos
        Try
            _session.Dispose()
        Catch
        End Try
    End Sub

    ' ---- keyboard ----

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        Select Case e.KeyCode
            Case Keys.Space
                TogglePlayPause()
                e.Handled = True
            Case Keys.Right
                If _durationTicks > 0 Then _session.Seek(Math.Max(0.0, _session.PositionTicks / 10000000.0 + 5.0))
                e.Handled = True
            Case Keys.Left
                If _durationTicks > 0 Then _session.Seek(Math.Max(0.0, _session.PositionTicks / 10000000.0 - 5.0))
                e.Handled = True
            Case Keys.Escape
                Close()
                e.Handled = True
        End Select
    End Sub

End Class
