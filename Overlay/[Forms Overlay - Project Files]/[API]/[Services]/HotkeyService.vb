Public Class HotkeyService
    Private _hwnd As IntPtr
    Private ReadOnly _actions As New Dictionary(Of Integer, Action)

    Public Event AltZPressed()
    Public Event AltF1Pressed()
    Public Event AltShiftF10Pressed()
    Public Event AltF10Pressed()
    Public Event AltF9Pressed()
    Public Event AltF3Pressed()
    Public Event AltF2Pressed()
    Public Event AltF8Pressed()
    Public Event AltF12Pressed()
    Public Event AltTPressed()

    Public Sub RegisterAll(hWnd As IntPtr)
        _hwnd = hWnd

        Register(1, WinAPI.MOD_ALT, Keys.Z, AddressOf RaiseAltZ)
        Register(2, WinAPI.MOD_ALT, Keys.F1, AddressOf RaiseAltF1)
        Register(3, WinAPI.MOD_ALT Or WinAPI.MOD_SHIFT, Keys.F10, AddressOf RaiseAltShiftF10)
        Register(4, WinAPI.MOD_ALT, Keys.F10, AddressOf RaiseAltF10)
        Register(5, WinAPI.MOD_ALT, Keys.F9, AddressOf RaiseAltF9)
        Register(6, WinAPI.MOD_ALT, Keys.F3, AddressOf RaiseAltF3)
        Register(7, WinAPI.MOD_ALT, Keys.F2, AddressOf RaiseAltF2)
        Register(8, WinAPI.MOD_ALT, Keys.F8, AddressOf RaiseAltF8)
        Register(9, WinAPI.MOD_ALT, Keys.F12, AddressOf RaiseAltF12)
        Register(10, WinAPI.MOD_ALT, Keys.T, AddressOf RaiseAltT)

    End Sub

    Private Sub Register(id As Integer, modifiers As Integer, key As Keys, action As Action)
        If WinAPI.RegisterHotKey(_hwnd, id, modifiers, CInt(key)) Then
            _actions(id) = action
        End If
    End Sub

    Public Sub ProcessHotkey(id As Integer)
        If _actions.ContainsKey(id) Then
            _actions(id).Invoke()
        End If
    End Sub

    Public Sub UnregisterAll()
        For Each id In _actions.Keys
            WinAPI.UnregisterHotKey(_hwnd, id)
        Next
        _actions.Clear()
    End Sub

    ' ===== Methods that raise events =====
    Private Sub RaiseAltZ()
        RaiseEvent AltZPressed()
    End Sub

    Private Sub RaiseAltF1()
        RaiseEvent AltF1Pressed()
    End Sub

    Private Sub RaiseAltShiftF10()
        RaiseEvent AltShiftF10Pressed()
    End Sub

    Private Sub RaiseAltF10()
        RaiseEvent AltF10Pressed()
    End Sub

    Private Sub RaiseAltF9()
        RaiseEvent AltF9Pressed()
    End Sub

    Private Sub RaiseAltF3()
        RaiseEvent AltF3Pressed()
    End Sub

    Private Sub RaiseAltF2()
        RaiseEvent AltF2Pressed()
    End Sub

    Private Sub RaiseAltF8()
        RaiseEvent AltF8Pressed()
    End Sub

    Private Sub RaiseAltF12()
        RaiseEvent AltF12Pressed()
    End Sub

    Private Sub RaiseAltT()
        RaiseEvent AltTPressed()
    End Sub


End Class