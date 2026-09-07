Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Linq

Public Class Base_KeySet
    Inherits System.Windows.Forms.Form

    Private _captureActionKey As String = Nothing
    Private ReadOnly _keyLabels As New Dictionary(Of String, Label)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _rowPanels As New Dictionary(Of String, Control)(StringComparer.OrdinalIgnoreCase)

    Private ReadOnly _colorNormal As Color = Color.FromArgb(55, 60, 65)
    Private ReadOnly _colorHover As Color = Color.FromArgb(74, 80, 86)
    Private ReadOnly _colorCapture As Color = Color.FromArgb(118, 185, 0)
    Private ReadOnly _colorError As Color = Color.FromArgb(150, 52, 52)

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

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        If _captureActionKey IsNot Nothing Then CancelCapture()
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1
        If m.Msg = WM_NCHITTEST Then
            m.Result = CType(HTTRANSPARENT, IntPtr)
            Return
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub set_key_Load(sender As Object, e As EventArgs) Handles Me.Load
        HideFromAltTab()
        KeyPreview = True
        Reset.Visible = True
        InitKeyLabels()
        WireEvents()
        LoadHotkeyValues()
        LayoutColumns()
    End Sub

    Public Sub InitKeyLabels()
        _keyLabels.Clear()
        _rowPanels.Clear()
        For Each def In HotkeyService.AllHotkeys
            Dim lblName As String = "lbl_" & def.ActionKey
            Dim foundControls() As Control = keyset.Controls.Find(lblName, True)

            If foundControls.Length > 0 AndAlso TypeOf foundControls(0) Is Label Then
                Dim lbl As Label = CType(foundControls(0), Label)
                lbl.Tag = def.ActionKey
                _keyLabels.Add(def.ActionKey, lbl)
            End If

            Dim rowName As String = "row_" & def.ActionKey
            Dim foundRows() As Control = keyset.Controls.Find(rowName, True)

            If foundRows.Length > 0 Then
                foundRows(0).Tag = def.ActionKey
                _rowPanels.Add(def.ActionKey, foundRows(0))
            End If
        Next
    End Sub

    Private _wiredEvents As Boolean = False

    Public Sub WireEvents()
        If _wiredEvents Then Return
        _wiredEvents = True
        For Each kvp In _keyLabels
            Dim lbl As Label = kvp.Value
            AddHandler lbl.Click, AddressOf HotkeyLabel_Click
            AddHandler lbl.MouseEnter, Sub(s, ea)
                                           Dim l As Label = CType(s, Label)
                                           If _captureActionKey <> CStr(l.Tag) Then l.BackColor = _colorHover
                                       End Sub
            AddHandler lbl.MouseLeave, Sub(s, ea)
                                           Dim l As Label = CType(s, Label)
                                           If _captureActionKey <> CStr(l.Tag) Then l.BackColor = _colorNormal
                                       End Sub
        Next
        For Each rowKvp As System.Collections.Generic.KeyValuePair(Of String, Control) In _rowPanels
            AddHandler rowKvp.Value.Click, AddressOf RowPanel_Click
        Next
    End Sub

    Private ReadOnly _colLeftRows As String() = {"ToggleOverlay", "TestNotifier", "WebToggle", "Screenshot", "PhotosToggle", "GameFilterToggle"}

    Private Sub Keyset_Resize(sender As Object, e As EventArgs) Handles keyset.Resize
        LayoutColumns()
    End Sub

    Private Sub LayoutColumns()
        If _rowPanels.Count = 0 Then Return
        If keyset.ClientSize.Width < 200 Then Return

        Const marginL As Integer = 28
        Const marginR As Integer = 28
        Const gap As Integer = 30

        Const minColWidth As Integer = 690

        Dim colWidth As Integer = Math.Max(minColWidth, (keyset.ClientSize.Width - marginL - marginR - gap) \ 2)
        Dim col2X As Integer = marginL + colWidth + gap

        bar_General.Left = marginL
        lblCat_General.Left = marginL + 16
        bar_Capture.Left = marginL
        lblCat_Capture.Left = marginL + 16

        bar_Record.Left = col2X
        lblCat_Record.Left = col2X + 16
        bar_Broadcast.Left = col2X
        lblCat_Broadcast.Left = col2X + 16
        lbl_Note.Left = col2X + 16

        For Each kvp As System.Collections.Generic.KeyValuePair(Of String, Control) In _rowPanels
            If _colLeftRows.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase) Then
                kvp.Value.Left = marginL
            Else
                kvp.Value.Left = col2X
            End If
            kvp.Value.Width = colWidth
        Next
    End Sub

    Public Sub LoadHotkeyValues()
        For Each def As HotkeyService.HotkeyDef In HotkeyService.AllHotkeys
            Dim val As String = def.GetSetting.Invoke()
            Call SetLabelText(def.ActionKey, val)
        Next
    End Sub

    Private Sub SetLabelText(actionKey As String, bindingText As String)
        If Not _keyLabels.ContainsKey(actionKey) Then Return
        _keyLabels(actionKey).Text = HotkeyService.NormalizeHotkeyText(bindingText)
    End Sub

    Private Sub HotkeyLabel_Click(sender As Object, e As EventArgs)
        Dim label As Label = CType(sender, Label)
        StartCapture(CStr(label.Tag), label)
    End Sub

    Private Sub RowPanel_Click(sender As Object, e As EventArgs)
        Dim row As Control = CType(sender, Control)
        Dim actionKey As String = CStr(row.Tag)
        If String.IsNullOrWhiteSpace(actionKey) Then Return
        If _keyLabels.ContainsKey(actionKey) Then
            StartCapture(actionKey, _keyLabels(actionKey))
        End If
    End Sub

    Private Sub StartCapture(actionKey As String, label As Label)
        If String.IsNullOrWhiteSpace(actionKey) Then Return

        If _captureActionKey IsNot Nothing Then
            
            If String.Equals(_captureActionKey, actionKey, StringComparison.OrdinalIgnoreCase) Then
                CancelCapture()
                Return
            End If
            LoadHotkeyValues()
            ResetCaptureVisuals()
        End If

        Base.PauseHotkeys()

        _captureActionKey = actionKey
        label.BackColor = _colorCapture
        label.Text = "Press keys..."
        Me.Focus()
    End Sub

    Private Sub Base_KeySet_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If String.IsNullOrEmpty(_captureActionKey) Then Return

        e.SuppressKeyPress = True
        e.Handled = True

        If e.KeyCode = Keys.Escape Then
            CancelCapture()
            Return
        End If

        Dim actionKey As String = _captureActionKey

        Dim modifiers As Integer = 0
        If e.Control Then modifiers = modifiers Or WinAPI.MOD_CONTROL
        If e.Alt Then modifiers = modifiers Or WinAPI.MOD_ALT
        If e.Shift Then modifiers = modifiers Or WinAPI.MOD_SHIFT

        Dim key As Keys = e.KeyCode
        If key = Keys.ControlKey OrElse key = Keys.Menu OrElse key = Keys.ShiftKey OrElse key = Keys.LWin OrElse key = Keys.RWin Then Return

        If modifiers = 0 Then
            ShowCaptureError(actionKey, "Needs Ctrl/Alt/Shift")
            Return
        End If

        Dim normalized As String = HotkeyService.NormalizeHotkeyText(modifiers, key)

        If IsDuplicateBinding(actionKey, normalized) Then
            ShowCaptureError(actionKey, "Already in use")
            Return
        End If

        SaveBinding(actionKey, normalized)

        _captureActionKey = Nothing
        ResetCaptureVisuals()
        LoadHotkeyValues()
        Base.ReloadHotkeys()
    End Sub

    Private Function IsDuplicateBinding(actionKey As String, binding As String) As Boolean
        For Each kvp In _keyLabels
            If String.Equals(kvp.Key, actionKey, StringComparison.OrdinalIgnoreCase) Then Continue For
            If String.Equals(kvp.Value.Text, binding, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    Private Sub SaveBinding(actionKey As String, binding As String)
        Dim def As HotkeyService.HotkeyDef = HotkeyService.AllHotkeys.FirstOrDefault(Function(x) x.ActionKey.Equals(actionKey, StringComparison.OrdinalIgnoreCase))
        If def IsNot Nothing Then
            def.SetSetting.Invoke(binding)
            AppSettings.Instance.Save()
        End If
    End Sub

    Private Sub ResetCaptureVisuals()
        For Each kvp In _keyLabels
            kvp.Value.BackColor = _colorNormal
        Next
    End Sub

    Private Sub CancelCapture()
        _captureActionKey = Nothing
        ResetCaptureVisuals()
        LoadHotkeyValues()
        Base.ResumeHotkeys()
    End Sub

    Private Sub ShowCaptureError(actionKey As String, message As String)
        CancelCapture()

        If Not _keyLabels.ContainsKey(actionKey) Then Return
        Dim lbl As Label = _keyLabels(actionKey)
        lbl.BackColor = _colorError
        lbl.Text = message

        Dim restore As New Timer With {.Interval = 1500}
        AddHandler restore.Tick,
            Sub(s, ea)
                restore.Stop()
                restore.Dispose()
                
                If Not String.Equals(_captureActionKey, actionKey, StringComparison.OrdinalIgnoreCase) Then
                    lbl.BackColor = _colorNormal
                    lbl.Text = GetBindingText(actionKey)
                End If
            End Sub
        restore.Start()
    End Sub

    Private Function GetBindingText(actionKey As String) As String
        Dim def As HotkeyService.HotkeyDef = HotkeyService.AllHotkeys.FirstOrDefault(Function(x) x.ActionKey.Equals(actionKey, StringComparison.OrdinalIgnoreCase))
        If def Is Nothing Then Return ""
        Return HotkeyService.NormalizeHotkeyText(def.GetSetting.Invoke())
    End Function

    Private Sub Reset_Click(sender As Object, e As EventArgs) Handles Reset.Click
        For Each def As HotkeyService.HotkeyDef In HotkeyService.AllHotkeys
            def.SetSetting.Invoke(def.DefaultBinding)
        Next
        AppSettings.Instance.Save()

        _captureActionKey = Nothing
        ResetCaptureVisuals()
        LoadHotkeyValues()
        Base.ReloadHotkeys()
    End Sub
End Class
