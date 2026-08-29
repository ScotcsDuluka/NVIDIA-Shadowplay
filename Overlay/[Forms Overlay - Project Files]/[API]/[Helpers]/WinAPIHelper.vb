Imports System.Runtime.InteropServices

Public Module WinAPI
    Public Const WM_HOTKEY As Integer = &H312
    Public Const MOD_ALT As Integer = &H1
    Public Const MOD_CONTROL As Integer = &H2
    Public Const MOD_SHIFT As Integer = &H4
    Public Const MOD_NOREPEAT As Integer = &H4000   ' Win7+: กดค้างแล้วไม่ยิงซ้ำ (ตัด auto-repeat)

    <DllImport("user32.dll")>
    Public Function RegisterHotKey(hWnd As IntPtr, id As Integer, fsModifiers As Integer, vk As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Public Function UnregisterHotKey(hWnd As IntPtr, id As Integer) As Boolean
    End Function
End Module