' OscHotkeyApplier.vb — applies the hotkey bindings the osc
' Keyboard-shortcuts page saved (Data\osc-settings.json, sections
' "hotkey:{name}" with {keys:[vk...]}) as REAL system-wide RegisterHotKey
' bindings, and dispatches them to the form as action names.
'
' This is the consumer side of the settings store: changing a binding in
' the UI now actually moves the OS hotkey (re-registered on every save).

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Public Class OscHotkeyApplier

    Public Event ActionTriggered(name As String)
    Public Event LogLine(message As String)

    Private Const ModifierAlt As Integer = &H1
    Private Const ModifierControl As Integer = &H2
    Private Const ModifierShift As Integer = &H4
    Private Const ModifierWin As Integer = &H8
    Private Const ModifierNoRepeat As Integer = &H4000

    ''' <summary>Page-side hotkey names applied as real global hotkeys.
    '     Overlay/OpenShare stay with OscHotkeys (Alt+Z ownership), and
    '     preset/monitor knobs are not actions yet.</summary>
    Private Shared ReadOnly AppliedNames As String() = {
        "Screenshot", "RecordToggle", "RecordSave", "DVRToggle",
        "NvCameraUI", "ModsUI", "CameraToggle", "MicToggle",
        "BroadcastToggle"
    }

    Private Structure Bind
        Public Id As Integer
        Public Name As String
    End Structure

    Private ReadOnly _binds As New List(Of Bind)()
    Private _hwnd As IntPtr = IntPtr.Zero
    Private _nextId As Integer = 100

    ''' <summary>Unregisters everything, re-reads the settings store and
    '     registers the current bindings. Safe to call on every save.</summary>
    Public Sub Reapply(hwnd As IntPtr)
        UnregisterAll()
        _hwnd = hwnd
        If hwnd = IntPtr.Zero Then Return
        For Each name As String In AppliedNames
            Dim keys As Integer() = OscControllerServer.GetStoredHotkeyKeys(name)
            If keys Is Nothing OrElse keys.Length = 0 Then Continue For
            Dim mods As Integer = ModifierNoRepeat
            Dim vk As Integer = 0
            For Each k As Integer In keys
                Select Case k
                    Case 16 : mods = mods Or ModifierShift
                    Case 17 : mods = mods Or ModifierControl
                    Case 18 : mods = mods Or ModifierAlt
                    Case 91, 92 : mods = mods Or ModifierWin
                    Case Else : vk = k
                End Select
            Next
            If vk = 0 Then Continue For
            Dim id As Integer = _nextId
            _nextId += 1
            If RegisterHotKey(hwnd, id, mods, vk) Then
                _binds.Add(New Bind With {.Id = id, .Name = name})
                RaiseEvent LogLine("action hotkey: " & name & " registered (vk=" &
                                   vk.ToString(CultureInfo.InvariantCulture) & ")")
            Else
                RaiseEvent LogLine("action hotkey busy (owned elsewhere): " & name)
            End If
        Next
    End Sub

    ''' <summary>Dispatches a WM_HOTKEY id to the bound action name.
    '     Unregistered ids are ignored.</summary>
    Public Sub HandleHotkey(id As Integer)
        For Each b As Bind In _binds
            If b.Id = id Then
                RaiseEvent ActionTriggered(b.Name)
                Return
            End If
        Next
    End Sub

    Public Sub UnregisterAll()
        If _hwnd <> IntPtr.Zero Then
            For Each b As Bind In _binds
                UnregisterHotKey(_hwnd, b.Id)
            Next
        End If
        _binds.Clear()
    End Sub

    <Runtime.InteropServices.DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function RegisterHotKey(hWnd As IntPtr, id As Integer, fsModifiers As Integer, vk As Integer) As Boolean
    End Function

    <Runtime.InteropServices.DllImport("user32.dll")>
    Private Shared Function UnregisterHotKey(hWnd As IntPtr, id As Integer) As Boolean
    End Function

End Class
