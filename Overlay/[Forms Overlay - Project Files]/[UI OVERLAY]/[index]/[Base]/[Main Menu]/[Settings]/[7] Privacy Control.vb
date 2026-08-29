Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
Public Class Base_Privacy_Control
    Inherits System.Windows.Forms.Form
    Protected Overrides Sub WndProc(ByRef m As Message)

        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1

        If m.Msg = WM_NCHITTEST Then
            m.Result = CType(HTTRANSPARENT, IntPtr)
            Return
        End If

        MyBase.WndProc(m)

    End Sub
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
    Private Sub py_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
    End Sub

    Private Sub action_fn_Click(sender As Object, e As EventArgs) Handles action_fn.Click
        Hide()
        Base_Settings.Show()
        Base.AMY(Base_Settings.Main_Menu_SET, -2000, 160, 300)
        Base.Settings_List.Visible = True
    End Sub
    Private Sub Base_Privacy_Control_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim privacyPath As String = AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "privacy")

        ' เช็กว่ามีไฟล์ไหม ถ้ามีให้ Toggle ชี้ที่ On
        TogglePrivacy.IsOn = File.Exists(privacyPath)
    End Sub
    Private Sub TogglePrivacy_ValueChanged(sender As Object, e As EventArgs) Handles TogglePrivacy.ValueChanged
        ' แก้ Path ให้ถูกต้อง (เดิมขาด \ ด้านหลัง StartupPath)
        Dim privacyPath As String = AppLayout.P("Data", "NVIDIA_Shadowplay_Data", "privacy")

        If TogglePrivacy.IsOn Then
            ' เปิด
            File.Create(privacyPath).Dispose()
        Else
            ' ปิด
            If File.Exists(privacyPath) Then File.Delete(privacyPath)
        End If
    End Sub

    Private Sub IF_Use_Engine_Tick(sender As Object, e As EventArgs) Handles IF_Use_Engine.Tick
        If Base.RecordValue = True Or Base.ReplayValue = True Then
            TogglePrivacy.Enabled = False
            captrueblock.Visible = True
            captrueblock_ico.Visible = True
        Else
            TogglePrivacy.Enabled = True
            captrueblock.Visible = False
            captrueblock_ico.Visible = False
        End If
    End Sub
End Class