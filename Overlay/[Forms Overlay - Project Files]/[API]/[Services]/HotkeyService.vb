Imports System.IO

Public Class HotkeyService
    Public Class HotkeyDef
        Public Property ActionKey As String
        Public Property DefaultBinding As String
        Public Property GetSetting As Func(Of String)
        Public Property SetSetting As Action(Of String)
    End Class

    ' ====================================================================
    ' <<<< จุดเดียว!! ถ้าอยากเพิ่ม Key ใหม่ แค่เพิ่มบรรทัดเดียวตรงนี้ >>>>
    ' ====================================================================
    Public Shared ReadOnly AllHotkeys As List(Of HotkeyDef)

    Shared Sub New()
        AllHotkeys = New List(Of HotkeyDef) From {
            CreateDef("ToggleOverlay", "Alt+Z"),
            CreateDef("Screenshot", "Alt+F1"),
            CreateDef("PhotosToggle", "Alt+F2"),
            CreateDef("GameFilterToggle", "Alt+F3"),
            CreateDef("ManualRecordToggle", "Alt+F9"),
            CreateDef("InstantReplayToggle", "Alt+Shift+F10"),
            CreateDef("InstantReplaySave", "Alt+F10"),
            CreateDef("BroadcastToggle", "Alt+F8"),
            CreateDef("TestNotifier", "Alt+T"),
            CreateDef("WebToggle", "Alt+CAPITAL")
        }
    End Sub

    Private Shared Function CreateDef(key As String, defBind As String) As HotkeyDef
        Dim def As New HotkeyDef()
        def.ActionKey = key
        def.DefaultBinding = defBind
        def.GetSetting = Function() GetHotkeyValue(key, defBind)
        def.SetSetting = Sub(v) SetHotkeyValue(key, v)
        Return def
    End Function

    Private Shared Function GetHotkeyValue(key As String, defaultBind As String) As String
        Dim hk = AppSettings.Instance.Hotkeys
        If hk IsNot Nothing AndAlso hk.ContainsKey(key) Then
            Return hk(key)
        End If
        Return defaultBind
    End Function

    Private Shared Sub SetHotkeyValue(key As String, value As String)
        Dim hk = AppSettings.Instance.Hotkeys
        If hk IsNot Nothing Then
            hk(key) = value
        End If
    End Sub
    ' ====================================================================

    Private _hwnd As IntPtr
    Private ReadOnly _actions As New Dictionary(Of Integer, Action)

    ' Events
    Public Event Key_OpenShare()

    Public Event Key_CaptureScreen()
    Public Event Key_PhotosToggle()
    Public Event Key_GameFilterToggle()

    Public Event Key_ManualRecordToggle()
    Public Event Key_InstantReplayToggle()
    Public Event Key_InstantReplaySave()
    Public Event Key_BroadcastToggle()

    Public Event Key_TestNotifier()

    Public Event Key_Web()
    Public Sub RegisterAll(hWnd As IntPtr)
        UnregisterAll()
        _hwnd = hWnd

        Dim usedCombos As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim id As Integer = 1

        For Each def As HotkeyDef In AllHotkeys
            Dim configuredBinding As String = def.GetSetting.Invoke()
            Call RegisterCommand(id, def, configuredBinding, usedCombos)
            id += 1
        Next
    End Sub

    Private Sub Register(id As Integer, modifiers As Integer, key As Keys, action As Action)
        If WinAPI.RegisterHotKey(_hwnd, id, modifiers, CInt(key)) Then
            _actions(id) = action
        End If
    End Sub

    ' <<< ระบุ Type ให้ชัดๆ ว่าเป็น String >>>
    Private Sub RegisterCommand(id As Integer, def As HotkeyDef, configuredBinding As String, usedCombos As HashSet(Of String))
        Dim modifiers As Integer = 0
        Dim key As Keys = Keys.None

        If Not TryParseHotkey(configuredBinding, modifiers, key) Then
            configuredBinding = def.DefaultBinding
            TryParseHotkey(configuredBinding, modifiers, key)
        End If

        Dim comboKey As String = modifiers.ToString() & ":" & CInt(key).ToString()
        If usedCombos.Contains(comboKey) Then
            configuredBinding = def.DefaultBinding
            TryParseHotkey(configuredBinding, modifiers, key)
            comboKey = modifiers.ToString() & ":" & CInt(key).ToString()
        End If

        Dim actionKeyToRaise As String = def.ActionKey
        Register(id, modifiers, key, Sub() RaiseSpecificEvent(actionKeyToRaise))

        usedCombos.Add(comboKey)
    End Sub

    Private Sub RaiseSpecificEvent(actionKey As String)
        Select Case actionKey
            Case "ToggleOverlay" : RaiseEvent Key_OpenShare()

            Case "Screenshot" : RaiseEvent Key_CaptureScreen()
            Case "PhotosToggle" : RaiseEvent Key_PhotosToggle()
            Case "GameFilterToggle" : RaiseEvent Key_GameFilterToggle()

            Case "ManualRecordToggle" : RaiseEvent Key_ManualRecordToggle()
            Case "InstantReplayToggle" : RaiseEvent Key_InstantReplayToggle()
            Case "InstantReplaySave" : RaiseEvent Key_InstantReplaySave()
            Case "BroadcastToggle" : RaiseEvent Key_BroadcastToggle()

            Case "TestNotifier" : RaiseEvent Key_TestNotifier()

            Case "WebToggle" : RaiseEvent Key_Web()
        End Select
    End Sub

    Public Shared Function TryParseHotkey(binding As String, ByRef modifiers As Integer, ByRef key As Keys) As Boolean
        modifiers = 0
        key = Keys.None
        If String.IsNullOrWhiteSpace(binding) Then Return False
        binding = binding.Replace("\u002B", "+")

        Dim parts As String() = binding.Split("+"c)
        For Each rawPart In parts
            Dim part As String = rawPart.Trim()
            If part.Length = 0 Then Continue For

            Select Case part.ToUpperInvariant()
                Case "ALT" : modifiers = modifiers Or WinAPI.MOD_ALT
                Case "CTRL", "CONTROL" : modifiers = modifiers Or WinAPI.MOD_CONTROL
                Case "SHIFT" : modifiers = modifiers Or WinAPI.MOD_SHIFT
                Case Else
                    Dim partUpper As String = part.ToUpperInvariant()
                    Dim parsed As Keys

                    If partUpper.Length = 1 AndAlso Char.IsDigit(partUpper(0)) Then
                        partUpper = "D" & partUpper
                    End If

                    If [Enum].TryParse(partUpper, True, parsed) Then
                        If parsed = Keys.LButton OrElse parsed = Keys.RButton OrElse parsed = Keys.MButton OrElse parsed = Keys.XButton1 OrElse parsed = Keys.XButton2 Then Return False
                        key = parsed
                    Else
                        Return False
                    End If
            End Select
        Next

        If key = Keys.None Then Return False
        If modifiers = 0 Then Return False
        If key = Keys.ControlKey OrElse key = Keys.Menu OrElse key = Keys.ShiftKey Then Return False
        Return True
    End Function

    Public Shared Function NormalizeHotkeyText(modifiers As Integer, key As Keys) As String
        Dim parts As New List(Of String)
        If (modifiers And WinAPI.MOD_CONTROL) <> 0 Then parts.Add("Ctrl")
        If (modifiers And WinAPI.MOD_ALT) <> 0 Then parts.Add("Alt")
        If (modifiers And WinAPI.MOD_SHIFT) <> 0 Then parts.Add("Shift")

        Dim keyName As String = key.ToString().ToUpperInvariant()
        If keyName.Length = 2 AndAlso keyName.StartsWith("D") AndAlso Char.IsDigit(keyName(1)) Then
            keyName = keyName.Substring(1)
        ElseIf keyName.StartsWith("NUMPAD") Then
            keyName = "Num" & keyName.Substring(6)
        End If

        parts.Add(keyName)
        Return String.Join("+", parts)
    End Function

    Public Shared Function NormalizeHotkeyText(binding As String) As String
        If String.IsNullOrWhiteSpace(binding) Then Return binding
        binding = binding.Replace("\u002B", "+")
        Dim modifiers As Integer
        Dim key As Keys
        If Not TryParseHotkey(binding, modifiers, key) Then Return binding
        Return NormalizeHotkeyText(modifiers, key)
    End Function

    Public Sub ProcessHotkey(id As Integer)
        If _actions.ContainsKey(id) Then _actions(id).Invoke()
    End Sub

    Public Sub UnregisterAll()
        For Each id In _actions.Keys
            WinAPI.UnregisterHotKey(_hwnd, id)
        Next
        _actions.Clear()
    End Sub
End Class