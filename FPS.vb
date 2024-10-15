Imports System.Diagnostics
Imports System.Drawing
Imports System.Runtime.InteropServices

Public Class FPS


    Private Sub Form13_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        run.Start() ' เริ่ม Timer เพื่อรักษาฟอร์มให้อยู่ด้านบน
        HideFromAltTab()
    End Sub


    Private Sub close_Click(sender As Object, e As EventArgs)
        Close() ' ปิดฟอร์ม
    End Sub

    Private Sub run_Tick(sender As Object, e As EventArgs) Handles run.Tick
        TopMost = True ' ตั้งให้ฟอร์มอยู่ด้านบนเสมอ
    End Sub

    Private Sub lblFPS_Click(sender As Object, e As EventArgs) Handles lblFPS.Click

    End Sub


    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80 ' สถานะสำหรับ ToolWindow (ไม่แสดงใน Alt+Tab)
    Private Const WS_EX_APPWINDOW As Integer = &H40000 ' สถานะสำหรับการแสดงใน Task Switcher
    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, style Or WS_EX_TOOLWINDOW And Not WS_EX_APPWINDOW)
    End Sub

    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Public Shared Function CreateProcess(
        lpApplicationName As String,
        lpCommandLine As String,
        lpProcessAttributes As IntPtr,
        lpThreadAttributes As IntPtr,
        bInheritHandles As Boolean,
        dwCreationFlags As UInteger,
        lpEnvironment As IntPtr,
        lpCurrentDirectory As String,
        ByRef lpStartupInfo As StartupInfo,
        ByRef lpProcessInformation As ProcessInformation) As Boolean
    End Function


    <StructLayout(LayoutKind.Sequential)>
    Public Structure StartupInfo
        Public cb As UInteger
        Public lpReserved As String
        Public lpDesktop As String
        Public lpTitle As String
        Public dwX As UInteger
        Public dwY As UInteger
        Public dwXSize As UInteger
        Public dwYSize As UInteger
        Public dwFlags As UInteger
        Public wShowWindow As UShort
        Public cbReserved2 As UShort
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=80)>
        Public lpReserved2 As Byte()
        Public hStdInput As IntPtr
        Public hStdOutput As IntPtr
        Public hStdError As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Public Structure ProcessInformation
        Public hProcess As IntPtr
        Public hThread As IntPtr
        Public dwProcessId As UInteger
        Public dwThreadId As UInteger
    End Structure
End Class
