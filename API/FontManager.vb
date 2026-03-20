Imports System.Runtime.InteropServices
Imports System.IO

Public Class FontManager
    <DllImport("gdi32.dll", SetLastError:=True)>
    Private Shared Function AddFontResource(ByVal lpszFilename As String) As Integer
    End Function

    <DllImport("gdi32.dll", SetLastError:=True)>
    Private Shared Function RemoveFontResource(ByVal lpszFilename As String) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal Msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As IntPtr
    End Function

    Private ReadOnly HWND_BROADCAST As IntPtr = CType(&HFFFF, IntPtr)
    Private Const WM_FONTCHANGE As Integer = &H1D

    Public Shared Function InstallFont(fontPath As String) As Boolean
        Try
            If Not File.Exists(fontPath) Then Return False
            Dim result As Integer = AddFontResource(fontPath)
            SendMessage(CType(&HFFFF, IntPtr), WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero)
            Return result > 0
        Catch
            Return False
        End Try
    End Function

    Public Shared Function UninstallFont(fontPath As String) As Boolean
        Try
            RemoveFontResource(fontPath)
            SendMessage(CType(&HFFFF, IntPtr), WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero)
            Return True
        Catch
            Return False
        End Try
    End Function
End Class