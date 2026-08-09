' HotkeyManager.vb
' ShadowPlay Engine - Global Hotkey Registration (Win32 API)
' Uses RegisterHotKey / UnregisterHotKey from user32.dll

Imports System.Runtime.InteropServices

Public Class HotkeyManager
    Implements IDisposable

    ' ── Win32 API ──────────────────────────────────────────────

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function RegisterHotKey(
        hWnd As IntPtr,
        id As Integer,
        fsModifiers As UInteger,
        vk As UInteger
    ) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function UnregisterHotKey(
        hWnd As IntPtr,
        id As Integer
    ) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function GetLastError() As Integer
    End Function

    ' ── Modifiers ──────────────────────────────────────────────

    <Flags()>
    Public Enum HotkeyModifiers As UInteger
        None = 0
        Alt = &H1
        Control = &H2
        Shift = &H4
        Win = &H8
    End Enum

    ' ── Events ────────────────────────────────────────────────

    Public Event HotkeyPressed As Action(Of Integer)

    ' ── Hidden Window for WM_HOTKEY ────────────────────────────

    Private _msgWindow As MessageWindowHelper
    Private _hotkeys As New Dictionary(Of Integer, HotkeyModifiers)()
    Private _nextId As Integer = 1
    Private _disposed As Boolean = False

    ' ── Constructor ───────────────────────────────────────────

    Public Sub New()
        _msgWindow = New MessageWindowHelper()
        _msgWindow.CreateHandle(New System.Windows.Forms.CreateParams())
        AddHandler _msgWindow.HotkeyReceived, AddressOf OnHotkeyReceived
    End Sub

    ' ── Register ───────────────────────────────────────────────

    Public Function Register(modifiers As HotkeyModifiers, key As System.Windows.Forms.Keys) As Integer
        Dim id As Integer = _nextId
        _nextId += 1

        Dim success As Boolean = RegisterHotKey(_msgWindow.Handle, id, CUInt(modifiers), CUInt(key))
        If Not success Then
            Dim errCode As Integer = GetLastError()
            Debug.WriteLine("RegisterHotKey failed: error " & errCode.ToString() & ", id=" & id.ToString())
            Return -1
        End If

        _hotkeys(id) = modifiers
        Return id
    End Function

    Public Function RegisterFromString(hotkeyString As String) As Integer
        If String.IsNullOrWhiteSpace(hotkeyString) Then Return -1

        Dim parts As String() = hotkeyString.Split(New Char() {"+"c}, StringSplitOptions.RemoveEmptyEntries)
        Dim mods As HotkeyModifiers = HotkeyModifiers.None
        Dim key As System.Windows.Forms.Keys = System.Windows.Forms.Keys.None

        For Each part In parts
            Dim p As String = part.Trim().ToLower()
            Select Case p
                Case "control", "ctrl"
                    mods = mods Or HotkeyModifiers.Control
                Case "shift"
                    mods = mods Or HotkeyModifiers.Shift
                Case "alt"
                    mods = mods Or HotkeyModifiers.Alt
                Case "win"
                    mods = mods Or HotkeyModifiers.Win
                Case Else
                    Dim parsedKey As System.Windows.Forms.Keys = System.Windows.Forms.Keys.None
                    If System.Enum.TryParse(Of System.Windows.Forms.Keys)(p, True, parsedKey) Then
                        key = parsedKey
                    End If
            End Select
        Next

        If key = System.Windows.Forms.Keys.None Then Return -1
        Return Register(mods, key)
    End Function

    ' ── Unregister ─────────────────────────────────────────────

    Public Function Unregister(id As Integer) As Boolean
        If _hotkeys.ContainsKey(id) Then
            _hotkeys.Remove(id)
        End If
        Return UnregisterHotKey(_msgWindow.Handle, id)
    End Function

    Public Sub UnregisterAll()
        Dim ids As New List(Of Integer)(_hotkeys.Keys)
        For Each id In ids
            UnregisterHotKey(_msgWindow.Handle, id)
        Next
        _hotkeys.Clear()
    End Sub

    ' ── Event Handler ─────────────────────────────────────────

    Private Sub OnHotkeyReceived(id As Integer)
        RaiseEvent HotkeyPressed(id)
    End Sub

    ' ── Dispose ────────────────────────────────────────────────

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not _disposed Then
            If disposing Then
                UnregisterAll()
                If _msgWindow IsNot Nothing Then
                    _msgWindow.DestroyHandle()
                    _msgWindow = Nothing
                End If
            End If
            _disposed = True
        End If
    End Sub

    ' ── Message Window Helper ──────────────────────────────────

    Private Class MessageWindowHelper
        Inherits System.Windows.Forms.NativeWindow
        Implements IDisposable

        Public Event HotkeyReceived As Action(Of Integer)

        Private Const WM_HOTKEY As Integer = &H312

        Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
            If m.Msg = WM_HOTKEY Then
                Dim id As Integer = m.WParam.ToInt32()
                RaiseEvent HotkeyReceived(id)
            End If
            MyBase.WndProc(m)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            DestroyHandle()
        End Sub

    End Class

End Class
