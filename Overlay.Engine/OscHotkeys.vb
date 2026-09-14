' OscHotkeys.vb — global hotkey ownership for the Overlay Engine.
'
' M0 scope: the toggle hotkey (Alt+Z by default), read from the SAME
' config key the Forms overlay uses (Hotkeys.ToggleOverlay in config.json)
' so a user-customized binding applies to both hosts.
'
' Ownership rule (M0): RegisterHotKey is first-come-first-served
' system-wide. If the Forms overlay (or anything else) already owns the
' combo, registration FAILS and this engine degrades honestly to the
' tray/hub toggles — it never steals, never retries aggressively, and
' never changes the Forms overlay's own registration.

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Public Class OscHotkeys
    Implements IDisposable

    Public Const HotkeyMsg As Integer = &H312 ' WM_HOTKEY

    Private Const ModifierAlt As Integer = &H1
    Private Const ModifierControl As Integer = &H2
    Private Const ModifierShift As Integer = &H4
    Private Const ModifierWin As Integer = &H8

    Private ReadOnly _id As Integer
    Private ReadOnly _binding As String
    Private _registered As Boolean

    Public Event LogLine(message As String)

    ''' <summary>Parsed binding: Win32 modifiers + virtual key.</summary>
    Public ReadOnly Property Modifiers As Integer
    Public ReadOnly Property VirtualKey As Integer

    Public Sub New(binding As String, id As Integer)
        _binding = If(String.IsNullOrWhiteSpace(binding), "Alt+Z", binding.Trim())
        _id = id
        Dim parsed = ParseBinding(_binding)
        If Not parsed.HasValue Then
            Dim fallback = ParseBinding("Alt+Z")
            Modifiers = fallback.Value.Modifiers
            VirtualKey = fallback.Value.VirtualKey
            RaiseEvent LogLine("hotkey binding '" & _binding & "' unparseable — fell back to Alt+Z")
        Else
            Modifiers = parsed.Value.Modifiers
            VirtualKey = parsed.Value.VirtualKey
        End If
    End Sub

    Public ReadOnly Property BindingText As String
        Get
            Return _binding
        End Get
    End Property

    Public ReadOnly Property IsRegistered As Boolean
        Get
            Return _registered
        End Get
    End Property

    ''' <summary>Registers against the form's handle. Returns False (and
    '     logs) when another process owns the combo.</summary>
    Public Function TryRegister(hostHandle As IntPtr) As Boolean
        If _registered Then Return True
        If hostHandle = IntPtr.Zero OrElse VirtualKey = 0 Then Return False
        _registered = RegisterHotKey(hostHandle, _id, Modifiers, VirtualKey)
        If _registered Then
            RaiseEvent LogLine("hotkey registered: " & _binding)
        Else
            RaiseEvent LogLine("hotkey NOT registered: " & _binding &
                               " is owned by another process (Forms overlay?) — tray/hub toggle only")
        End If
        Return _registered
    End Function

    Public Sub Unregister(hostHandle As IntPtr)
        If _registered AndAlso hostHandle <> IntPtr.Zero Then
            UnregisterHotKey(hostHandle, _id)
            _registered = False
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Unregister happens on the form (needs the handle); Dispose only
        ' guards against double disposal.
        _registered = False
    End Sub

    ' ── binding parser (pure — unit-tested) ────────────────────

    Public Structure ParsedBinding
        Public Modifiers As Integer
        Public VirtualKey As Integer
    End Structure

    ''' <summary>Parses "Alt+Z", "Ctrl+Alt+F9", "Shift+F10" — same shape the
    '     Forms overlay's HotkeyService stores in config.json
    '     (Hotkeys.ToggleOverlay). Returns Nothing when unparseable.</summary>
    Public Shared Function ParseBinding(binding As String) As ParsedBinding?
        If String.IsNullOrWhiteSpace(binding) Then Return Nothing
        Dim parts As String() = binding.Split("+"c)
        If parts.Length < 1 OrElse parts.Length > 5 Then Return Nothing

        Dim result As New ParsedBinding()
        For i As Integer = 0 To parts.Length - 2
            Dim modName As String = parts(i).Trim().ToUpperInvariant()
            Select Case modName
                Case "ALT" : result.Modifiers = result.Modifiers Or ModifierAlt
                Case "CTRL", "CONTROL" : result.Modifiers = result.Modifiers Or ModifierControl
                Case "SHIFT" : result.Modifiers = result.Modifiers Or ModifierShift
                Case "WIN", "WINDOWS" : result.Modifiers = result.Modifiers Or ModifierWin
                Case Else : Return Nothing
            End Select
        Next

        Dim keyName As String = parts(parts.Length - 1).Trim().ToUpperInvariant()
        If keyName.Length = 1 AndAlso keyName >= "A"c AndAlso keyName <= "Z"c Then
            result.VirtualKey = AscW(keyName(0)) ' 0x41..0x5A
            Return result
        End If
        If keyName.Length = 1 AndAlso keyName >= "0"c AndAlso keyName <= "9"c Then
            result.VirtualKey = AscW(keyName(0)) ' 0x30..0x39
            Return result
        End If
        If keyName.StartsWith("F", StringComparison.Ordinal) Then
            Dim fn As Integer
            If Integer.TryParse(keyName.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, fn) AndAlso
               fn >= 1 AndAlso fn <= 24 Then
                result.VirtualKey = &H70 + fn - 1 ' VK_F1 = 0x70
                Return result
            End If
        End If
        Dim namedKeys As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase) From {
            {"ESC", &H1B}, {"ESCAPE", &H1B}, {"SPACE", &H20}, {"TAB", &H9},
            {"ENTER", &HD}, {"RETURN", &HD}, {"BACKSPACE", &H8},
            {"INSERT", &H2D}, {"DELETE", &H2E}, {"HOME", &H24}, {"END", &H23},
            {"PAGEUP", &H21}, {"PAGEDOWN", &H22},
            {"LEFT", &H25}, {"ARROWLEFT", &H25}, {"UP", &H26}, {"ARROWUP", &H26},
            {"RIGHT", &H27}, {"ARROWRIGHT", &H27}, {"DOWN", &H28}, {"ARROWDOWN", &H28},
            {"PRINTSCREEN", &H2C}, {"CAPSLOCK", &H14}, {"NUMLOCK", &H90}
        }
        Dim namedVk As Integer
        If namedKeys.TryGetValue(keyName, namedVk) Then
            result.VirtualKey = namedVk
            Return result
        End If
        Return Nothing
    End Function

    <Runtime.InteropServices.DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function RegisterHotKey(hWnd As IntPtr, id As Integer, fsModifiers As Integer, vk As Integer) As Boolean
    End Function

    <Runtime.InteropServices.DllImport("user32.dll")>
    Private Shared Function UnregisterHotKey(hWnd As IntPtr, id As Integer) As Boolean
    End Function

End Class
