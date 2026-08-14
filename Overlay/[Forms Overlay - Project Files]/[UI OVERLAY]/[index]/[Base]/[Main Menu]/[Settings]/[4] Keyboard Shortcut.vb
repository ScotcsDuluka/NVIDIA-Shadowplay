Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Linq

Public Class Base_KeySet
    Inherits NoCloseForm

    Private _captureActionKey As String = Nothing
    Private ReadOnly _keyLabels As New Dictionary(Of String, Label)(StringComparer.OrdinalIgnoreCase)

    Private ReadOnly _colorNormal As Color = Color.FromArgb(38, 43, 47)
    Private ReadOnly _colorHover As Color = Color.FromArgb(55, 60, 65)
    Private ReadOnly _colorCapture As Color = Color.FromArgb(118, 185, 0)

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
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        If _captureActionKey IsNot Nothing Then CancelCapture()
        Me.Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub

    Private Sub settings_top_Click(sender As Object, e As EventArgs) Handles settings_top.Click
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
    End Sub

    ' <<<< ไม่ต้อง Hardcode แล้ว! มันหา Label เองจากชื่อ >>>>
    Public Sub InitKeyLabels()
        _keyLabels.Clear()
        For Each def In HotkeyService.AllHotkeys
            ' มันจะไปหา Label ที่ชื่อ "lbl_<ActionKey>" เช่น lbl_ToggleOverlay ใน Panel keyset ให้เอง
            Dim lblName As String = "lbl_" & def.ActionKey
            Dim foundControls() As Control = keyset.Controls.Find(lblName, True)

            If foundControls.Length > 0 AndAlso TypeOf foundControls(0) Is Label Then
                Dim lbl As Label = CType(foundControls(0), Label)
                lbl.Tag = def.ActionKey
                _keyLabels.Add(def.ActionKey, lbl)
            End If
        Next
    End Sub

    Public Sub WireEvents()
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
        Dim actionKey As String = CStr(label.Tag)
        If String.IsNullOrWhiteSpace(actionKey) Then Return

        If _captureActionKey IsNot Nothing Then LoadHotkeyValues()
        Base.PauseHotkeys()

        _captureActionKey = actionKey
        For Each kvp In _keyLabels
            kvp.Value.BackColor = _colorNormal
        Next

        label.BackColor = _colorCapture
        label.Text = "Press keys..."
        Me.Focus()
    End Sub

    Private Sub Base_KeySet_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If String.IsNullOrEmpty(_captureActionKey) Then Return

        If e.KeyCode = Keys.Escape Then
            CancelCapture()
            Return
        End If

        Dim modifiers As Integer = 0
        If e.Control Then modifiers = modifiers Or WinAPI.MOD_CONTROL
        If e.Alt Then modifiers = modifiers Or WinAPI.MOD_ALT
        If e.Shift Then modifiers = modifiers Or WinAPI.MOD_SHIFT

        Dim key As Keys = e.KeyCode
        If key = Keys.ControlKey OrElse key = Keys.Menu OrElse key = Keys.ShiftKey Then Return
        If modifiers = 0 Then
            ShowCaptureError("Please include Ctrl, Alt, or Shift.")
            Return
        End If

        Dim normalized As String = HotkeyService.NormalizeHotkeyText(modifiers, key)

        If IsDuplicateBinding(_captureActionKey, normalized) Then
            ShowCaptureError("This key is already assigned.")
            Return
        End If

        SaveBinding(_captureActionKey, normalized)

        If _keyLabels.ContainsKey(_captureActionKey) Then
            _keyLabels(_captureActionKey).BackColor = _colorNormal
        End If

        _captureActionKey = Nothing
        LoadHotkeyValues()
        Base.ReloadHotkeys()
        e.SuppressKeyPress = True
    End Sub

    Private Function IsDuplicateBinding(actionKey As String, binding As String) As Boolean
        For Each kvp In _keyLabels
            If kvp.Key.Equals(actionKey, StringComparison.OrdinalIgnoreCase) Then Continue For
            If String.Equals(kvp.Value.Text, binding, StringComparison.OrdinalIgnoreCase) Then Return True
        Next
        Return False
    End Function

    Private Sub SaveBinding(actionKey As String, binding As String)
        Dim def As HotkeyService.HotkeyDef = HotkeyService.AllHotkeys.FirstOrDefault(Function(x) x.ActionKey.Equals(actionKey, StringComparison.OrdinalIgnoreCase))
        If def IsNot Nothing Then
            def.SetSetting.Invoke(binding) ' <<< เพิ่ม .Invoke()
            AppSettings.Instance.Save()
        End If
    End Sub

    Private Sub CancelCapture()
        _captureActionKey = Nothing
        For Each kvp In _keyLabels
            kvp.Value.BackColor = _colorNormal
        Next
        LoadHotkeyValues()
        Base.ResumeHotkeys()
    End Sub

    Private Sub ShowCaptureError(message As String)
        MessageBox.Show(message, "Invalid hotkey", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Sub Reset_Click(sender As Object, e As EventArgs) Handles Reset.Click
        For Each def As HotkeyService.HotkeyDef In HotkeyService.AllHotkeys
            def.SetSetting.Invoke(def.DefaultBinding) ' <<< เพิ่ม .Invoke()
        Next
        AppSettings.Instance.Save()

        _captureActionKey = Nothing
        For Each kvp In _keyLabels
            kvp.Value.BackColor = _colorNormal
        Next
        LoadHotkeyValues()
        Base.ReloadHotkeys()
    End Sub
End Class