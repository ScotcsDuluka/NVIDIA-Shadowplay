Imports System.IO
Imports Microsoft.Win32
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports System.Diagnostics

Public NotInheritable Class FontHelper

    <DllImport("gdi32.dll", EntryPoint:="AddFontResourceW", SetLastError:=True)>
    Private Shared Function AddFontResource(lpFileName As String) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SendMessage(hWnd As IntPtr, msg As UInteger, wParam As IntPtr, lParam As IntPtr) As IntPtr
    End Function

    Private Const WM_FONTCHANGE As UInteger = &H1D
    Private Shared ReadOnly HWND_BROADCAST As IntPtr = CType(&HFFFF, IntPtr)

    ''' <summary>
    ''' Checks if a font exists; if not, installs it for the current user and kills NVIDIA processes.
    ''' Returns True if font exists or installed successfully.
    ''' </summary>
    Public Shared Function CheckAndInstallUserFont(fontFile As String) As Boolean
        Dim userFontFolder As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft\Windows\Fonts")
        If Not Directory.Exists(userFontFolder) Then Directory.CreateDirectory(userFontFolder)

        Dim fontPath As String = Path.Combine(userFontFolder, fontFile)

        ' ถ้ามีแล้ว → คืนค่า True
        If File.Exists(fontPath) Then Return True

        Dim sourcePath As String = Path.Combine(Application.StartupPath, fontFile)
        If Not File.Exists(sourcePath) Then
            MessageBox.Show("Font file not found in application folder: " & fontFile, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End If

        Try
            ' Copy ฟอนต์ไป user folder
            File.Copy(sourcePath, fontPath, True)

            ' Register font
            AddFontResource(fontPath)
            SendMessage(HWND_BROADCAST, WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero)

            ' ลง registry user
            Using regKey As RegistryKey = Registry.CurrentUser.CreateSubKey("Software\Microsoft\Windows NT\CurrentVersion\Fonts")
                regKey.SetValue(Path.GetFileNameWithoutExtension(fontFile) & " (TrueType)", fontPath)
            End Using

            KillProcess("NVIDIA Notifier.exe")
            KillProcess("NVIDIA ShadowPlay.exe")

            Return True
        Catch ex As Exception
            MessageBox.Show("Failed to install font: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Terminates the specified process by name (can include .exe).
    ''' </summary>
    Private Shared Sub KillProcess(processName As String)
        Try
            ' เอา .exe ออกถ้ามี
            Dim name As String = processName.Replace(".exe", "")
            For Each proc As Process In Process.GetProcessesByName(name)
                proc.Kill()
            Next
        Catch ex As Exception
            Debug.WriteLine("Error killing process " & processName & ": " & ex.Message)
        End Try
    End Sub

End Class