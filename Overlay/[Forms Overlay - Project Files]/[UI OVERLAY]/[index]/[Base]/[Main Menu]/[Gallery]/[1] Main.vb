Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports Notifier_API

Public Class Base_Gallery
    Inherits System.Windows.Forms.Form

    Public Sub InitForm()
    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
    End Sub

    Private Sub Gallery_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        Base.AlignPanelToTop()
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles Saved_l10n.Click, bg_fn.Click
        Hide()
        Base.IF_OpenShare = True
        Base.ShowMainPanel()
        Base.shadowplay.Visible = True
    End Sub

    Private Sub Openloaction_l10n_Click(sender As Object, e As EventArgs) Handles Openloaction_l10n.Click
          Base.IF_OpenShare = True
        If Directory.Exists(txtFilePath.Text) Then
            Process.Start("explorer.exe", txtFilePath.Text)
            Hide()
            Base.HideAllControls()
            Base.IF_OpenShare = True
        Else
            MessageBox.Show("foldererror")
        End If
    End Sub

    Private Sub save_sc_Click(sender As Object, e As EventArgs) Handles save_sc.Click
        Dim folderDlg As New FolderBrowserDialog With {
        .Description = "Select the folder to save the capture."
    }

        If folderDlg.ShowDialog = DialogResult.OK Then
            txtFilePath.Text = folderDlg.SelectedPath

            AppSettings.Instance.Paths.GalleryPath = txtFilePath.Text
            AppSettings.Instance.Save()
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        settings_1.Location = New Point((Me.ClientSize.Width - settings_1.Width) / 2, 160)
    End Sub

    ' ==================================================================
    '  W2 - Gallery media grid (browse view), runtime-built controls.
    '
    '  Consumes the existing Gallery.Video API for metadata (MediaProbe)
    '  and playback (PlaybackSession via GalleryPlayerForm). No engine
    '  semantics are touched; no new backend contract is introduced.
    '
    '  States: Loading (folder scan) / Empty (no MP4) / Error (folder
    '  missing) / Ready (grid). Selection: click; Open: double-click.
    ' ==================================================================

    Private ReadOnly _mediaStore As New GalleryMediaStore(ResolveStoreFFmpeg())
    Private _gridPanel As FlowLayoutPanel
    Private _statusLabel As Label
    Private _refreshBtn As Label
    Private _scanCts As CancellationTokenSource
    Private _selectedTile As GalleryMediaItem

    Friend Shared Function ResolveStoreFFmpeg() As String
        Dim configured = AppSettings.Instance.Paths.FFmpegPath
        If Not String.IsNullOrEmpty(configured) AndAlso FFmpegLocator.IsUsableFFmpeg(configured) Then Return configured
        Dim candidates = {
            IO.Path.Combine(IO.Path.GetDirectoryName(Application.ExecutablePath), "API-Core", "ffmpeg.exe"),
            "C:\My Project\NVIDIA-Shadowplay\Overlay\API-Core\ffmpeg.exe"
        }
        Return FFmpegLocator.FirstUsableFFmpeg(candidates, Sub(m)
                                                           End Sub)
    End Function

    ' <summary>Build the browse UI once, inside the existing settings_1
    ' content area. Idempotent.</summary>
    Private Sub EnsureBrowseUi()
        If _gridPanel IsNot Nothing Then Return

        _gridPanel = New FlowLayoutPanel With {
            .Location = New Point(4, 90),
            .Size = New Size(1032, 385),
            .BackColor = Color.FromArgb(16, 16, 20),
            .AutoScroll = True,
            .BorderStyle = BorderStyle.FixedSingle
        }

        _statusLabel = New Label With {
            .Text = "",
            .Font = New Font("Segoe UI", 11.0F),
            .ForeColor = Color.Silver,
            .AutoSize = False,
            .Size = New Size(600, 60),
            .TextAlign = ContentAlignment.MiddleCenter,
            .Location = New Point(216, 200),
            .Visible = False
        }

        _refreshBtn = New Label With {
            .Text = "Refresh",
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
            .ForeColor = Color.White,
            .BackColor = Color.FromArgb(40, 40, 46),
            .TextAlign = ContentAlignment.MiddleCenter,
            .Size = New Size(90, 28),
            .Location = New Point(930, 52),
            .Cursor = Cursors.Hand,
            .Visible = False
        }
        AddHandler _refreshBtn.Click, Sub(sender, e) LoadGalleryFolder()

        settings_1.Controls.Add(_gridPanel)
        settings_1.Controls.Add(_statusLabel)
        settings_1.Controls.Add(_refreshBtn)
        _gridPanel.BringToFront()
    End Sub

    Private Sub LoadGalleryFolder()
        EnsureBrowseUi()
        If _scanCts IsNot Nothing Then
            Try : _scanCts.Cancel() : Catch : End Try
        End If
        _scanCts = New CancellationTokenSource()
        Dim ct = _scanCts.Token

        Dim folder As String
        If Not String.IsNullOrWhiteSpace(txtFilePath.Text) Then
            folder = txtFilePath.Text
        Else
            folder = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Shadowplay", "Gallery")
            txtFilePath.Text = folder
        End If

        ShowBrowseState("Loading gallery...")
        ClearTiles()

        If Not Directory.Exists(folder) Then
            ShowBrowseState("Folder not found:" & Environment.NewLine & folder &
                            Environment.NewLine & "Create it or pick another folder below.")
            _refreshBtn.Visible = True
            Return
        End If

        Dim scanTask = _mediaStore.ScanAsync(folder, ct)
        scanTask.ContinueWith(
            Sub(t)
                If IsDisposed OrElse Disposing OrElse ct.IsCancellationRequested Then Return
                Invoke(New Action(Sub() OnScanCompleted(t, ct)))
            End Sub, TaskScheduler.Default)
    End Sub

    Private Sub OnScanCompleted(t As Task(Of List(Of GalleryMediaItem)), ct As CancellationToken)
        If t.IsCanceled OrElse t.IsFaulted Then
            ShowBrowseState("Gallery scan failed.")
            _refreshBtn.Visible = True
            Return
        End If

        Dim items = t.Result
        If items.Count = 0 Then
            ShowBrowseState("No recordings in this folder." &
                            Environment.NewLine & "Record something first, or pick another folder below.")
            _refreshBtn.Visible = True
            Return
        End If

        ShowBrowseState(Nothing)
        For Each item In items
            Dim tile As New GalleryTile(item)
            tile.TileOpened = Sub(selected As GalleryMediaItem) OpenInPlayer(selected)
            AddHandler tile.TileSelected, Sub(sel) _selectedTile = sel
            If InvokeRequired Then
                Invoke(New Action(Sub() _gridPanel.Controls.Add(tile)))
            Else
                _gridPanel.Controls.Add(tile)
            End If
            StartTileLoading(tile, item, ct)
        Next
        _refreshBtn.Visible = True
    End Sub

    Private Sub StartTileLoading(tile As GalleryTile, item As GalleryMediaItem, ct As CancellationToken)
        ' Metadata via the EXISTING Gallery.Video MediaProbe, then thumbnail.
        _mediaStore.ProbeAsync(item, ct).ContinueWith(
            Sub(t)
                If ct.IsCancellationRequested OrElse IsDisposed OrElse Disposing Then Return
                Invoke(New Action(Sub() tile.UpdateMetadata()))
                _mediaStore.GetThumbnailAsync(item, 320, ct).ContinueWith(
                    Sub(tt)
                        If ct.IsCancellationRequested OrElse IsDisposed OrElse Disposing Then Return
                        Dim thumbPath = tt.Result
                        Invoke(New Action(Sub()
                                              If thumbPath IsNot Nothing Then
                                                  tile.SetThumbnail(Image.FromFile(thumbPath))
                                              Else
                                                  tile.SetThumbnailFailed()
                                              End If
                                          End Sub))
                    End Sub, ct)
            End Sub, ct)
    End Sub

    Private Sub OpenInPlayer(item As GalleryMediaItem)
        ' The player form owns its own PlaybackSession; browse stays alive.
        GalleryPlayerForm.TryShow(Me, item.FilePath)
    End Sub

    Private Sub ShowBrowseState(text As String)
        If _statusLabel IsNot Nothing Then
            _statusLabel.Text = If(text, "")
            _statusLabel.Visible = Not String.IsNullOrEmpty(text)
        End If
        If _gridPanel IsNot Nothing AndAlso Not String.IsNullOrEmpty(text) Then
            ClearTiles()
        End If
    End Sub

    Private Sub ClearTiles()
        If _gridPanel Is Nothing Then Return
        For Each c As Control In _gridPanel.Controls
            Dim tile = TryCast(c, GalleryTile)
            If tile IsNot Nothing Then tile.DisposeTile()
        Next
        _gridPanel.Controls.Clear()
        _selectedTile = Nothing
    End Sub

    Private Sub Gallery_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible Then LoadGalleryFolder()
    End Sub

    Private Sub Gallery_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _scanCts IsNot Nothing Then
            Try : _scanCts.Cancel() : Catch : End Try
        End If
        ClearTiles()
    End Sub

    ' <summary>One selectable tile: thumbnail + name + metadata line.</summary>
    Private Class GalleryTile
        Inherits Panel

        Private ReadOnly _item As GalleryMediaItem
        Private ReadOnly _thumbBox As New PictureBox()
        Private ReadOnly _nameLabel As New Label()
        Private ReadOnly _metaLabel As New Label()
        Private _thumbImage As Image

        Public Event TileSelected(item As GalleryMediaItem)
        Public TileOpened As Action(Of GalleryMediaItem)

        Public Sub New(item As GalleryMediaItem)
            _item = item
            Size = New Size(200, 160)
            BackColor = Color.FromArgb(28, 28, 34)
            Margin = New Padding(6)
            Cursor = Cursors.Hand
            DoubleBuffered = True

            _thumbBox.Size = New Size(188, 100)
            _thumbBox.Location = New Point(6, 6)
            _thumbBox.BackColor = Color.FromArgb(18, 18, 22)
            _thumbBox.SizeMode = PictureBoxSizeMode.Zoom
            Controls.Add(_thumbBox)

            _nameLabel.Text = Truncate(item.DisplayName, 26)
            _nameLabel.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
            _nameLabel.ForeColor = Color.White
            _nameLabel.AutoSize = False
            _nameLabel.Size = New Size(188, 18)
            _nameLabel.Location = New Point(6, 110)
            Controls.Add(_nameLabel)

            _metaLabel.Text = FormatBytes(item.LengthBytes) &
                If(item.ModifiedUtc <> DateTime.MinValue, " - " & item.ModifiedUtc.ToString("MM/dd"), "")
            _metaLabel.Font = New Font("Segoe UI", 7.5F)
            _metaLabel.ForeColor = Color.Silver
            _metaLabel.AutoSize = False
            _metaLabel.Size = New Size(188, 16)
            _metaLabel.Location = New Point(6, 130)
            Controls.Add(_metaLabel)

            AddHandler Click, AddressOf RaiseSelected
            AddHandler DoubleClick, AddressOf RaiseOpened
            AddHandler _thumbBox.Click, AddressOf RaiseSelected
            AddHandler _thumbBox.DoubleClick, AddressOf RaiseOpened
        End Sub

        Public Sub UpdateMetadata()
            _metaLabel.Text = If(_item.DurationSec > 0, _item.DurationText & " - ", "") &
                _item.Resolution & If(_item.Resolution <> "", " - ", "") &
                FormatBytes(_item.LengthBytes)
        End Sub

        Public Sub SetThumbnail(img As Image)
            If _thumbImage IsNot Nothing Then _thumbImage.Dispose()
            _thumbImage = img
            _thumbBox.Image = img
        End Sub

        Public Sub SetThumbnailFailed()
            _thumbBox.BackColor = Color.FromArgb(50, 30, 30)
        End Sub

        Public Sub DisposeTile()
            If _thumbImage IsNot Nothing Then _thumbImage.Dispose()
            _thumbImage = Nothing
            _thumbBox.Image = Nothing
        End Sub

        Private Sub RaiseSelected(sender As Object, e As EventArgs)
            RaiseEvent TileSelected(_item)
            BackColor = Color.FromArgb(60, 90, 140)
        End Sub

        Private Sub RaiseOpened(sender As Object, e As EventArgs)
            RaiseEvent TileSelected(_item)
            If TileOpened IsNot Nothing Then TileOpened(_item)
        End Sub

        Private Shared Function Truncate(s As String, max As Integer) As String
            If String.IsNullOrEmpty(s) OrElse s.Length <= max Then Return s
            Return s.Substring(0, max - 1) & "..."
        End Function

        Private Shared Function FormatBytes(b As Long) As String
            If b >= 1073741824L Then Return (b / 1073741824.0).ToString("0.##") & " GB"
            Return (b / 1048576.0).ToString("0.#") & " MB"
        End Function

    End Class

End Class