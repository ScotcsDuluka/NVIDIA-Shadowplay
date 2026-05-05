Imports System.Runtime.InteropServices

Public Class sha4

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim cp As CreateParams = MyBase.CreateParams
            cp.ExStyle = cp.ExStyle Or WS_EX_TRANSPARENT Or WS_EX_LAYERED
            Return cp
        End Get
    End Property

    Private Const WS_EX_TRANSPARENT As Integer = &H20
    Private Const WS_EX_LAYERED As Integer = &H80000

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    Public Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        Dim newStyle As Integer = (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW
        SetWindowLong(Me.Handle, GWL_EXSTYLE, newStyle)
    End Sub


    <DllImport("dwmapi.dll")>
    Private Shared Function DwmSetWindowAttribute(
        hwnd As IntPtr,
        dwAttribute As Integer,
        ByRef pvAttribute As Integer,
        cbAttribute As Integer
    ) As Integer
    End Function

    <DllImport("dwmapi.dll")>
    Private Shared Function DwmExtendFrameIntoClientArea(
        hwnd As IntPtr,
        ByRef pMarInset As MARGINS
    ) As Integer
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Public Structure MARGINS
        Public leftWidth As Integer
        Public rightWidth As Integer
        Public topHeight As Integer
        Public bottomHeight As Integer
    End Structure

    <DllImport("user32.dll")>
    Private Shared Function SetLayeredWindowAttributes(ByVal hWnd As IntPtr, ByVal crKey As Integer, ByVal bAlpha As Byte, ByVal dwFlags As Integer) As Boolean
    End Function

    Private Const LWA_COLORKEY As Integer = &H1
    Protected Overrides Sub OnHandleCreated(e As EventArgs)
        MyBase.OnHandleCreated(e)
        Debug.WriteLine("[Shadow] Handle created → DWM setup")

        Dim attrValue As Integer = 2
        DwmSetWindowAttribute(Me.Handle, 2, attrValue, 4)

        Dim margins As New MARGINS With {
            .leftWidth = 1,
            .rightWidth = 1,
            .topHeight = 1,
            .bottomHeight = 1
        }

        DwmExtendFrameIntoClientArea(Me.Handle, margins)
        SetLayeredWindowAttributes(Me.Handle, Color.Magenta.ToArgb(), 0, LWA_COLORKEY)
    End Sub


    Private Sub test_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        Dim screenPos As Point = Base.shadowplay.PointToScreen(Base.bg_action.Location)

        Using g As Graphics = Me.CreateGraphics()
            Dim scale As Single = g.DpiX / 96.0F
            Me.Location = New Point(CInt(screenPos.X / scale), CInt(screenPos.Y / scale))
        End Using
    End Sub
End Class