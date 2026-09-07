






Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Threading.Tasks

Public Class Base_Connect_Login
    Inherits System.Windows.Forms.Form

    Private Const GWL_EXSTYLE As Integer = -20
    Private Const WS_EX_TOOLWINDOW As Integer = &H80
    Private Const WS_EX_APPWINDOW As Integer = &H40000

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As Integer
    End Function

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_NCHITTEST As Integer = &H84
        Const HTTRANSPARENT As Integer = -1
        If m.Msg = WM_NCHITTEST Then
            Dim pos As Point = Me.PointToClient(Cursor.Position)
            If Me.GetChildAtPoint(pos) Is Nothing Then
                m.Result = CType(HTTRANSPARENT, IntPtr)
                Return
            End If
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub HideFromAltTab()
        Dim style As Integer = GetWindowLong(Me.Handle, GWL_EXSTYLE)
        SetWindowLong(Me.Handle, GWL_EXSTYLE, (style Or WS_EX_TOOLWINDOW) And Not WS_EX_APPWINDOW)
    End Sub

    Private _flow As DulukaAuthFlow

    Private Sub Page_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        HideFromAltTab()
        ResetUi()
    End Sub

    Private Sub Page_VisibleChanged(sender As Object, e As EventArgs) Handles MyBase.VisibleChanged
        If Visible AndAlso _flow Is Nothing Then
            ResetUi()
        End If
    End Sub

    Private Sub ResetUi()
        BT_StartLogin.Visible = True
        BT_CancelLogin.Visible = False
        If DulukaAccountStore.Instance.HasSession Then
            Status_TEXT.Text = "Already signed in on this device."
        Else
            Status_TEXT.Text = ""
        End If
    End Sub

    
    Private Sub Report(message As String)
        If IsDisposed OrElse Not IsHandleCreated Then Return
        Try
            Invoke(New Action(Sub() Status_TEXT.Text = message))
        Catch ex As ObjectDisposedException
        Catch ex As InvalidOperationException
        End Try
    End Sub

    Private Async Sub BT_StartLogin_Click(sender As Object, e As EventArgs) Handles BT_StartLogin.Click
        If DulukaAccountStore.Instance.HasSession Then
            Status_TEXT.Text = "Already signed in on this device."
            Return
        End If
        _flow = DulukaAuthFlow.TryBegin(DulukaAuthFlow.FlowKind.Login)
        If _flow Is Nothing Then
            Status_TEXT.Text = "A sign-in is already in progress."
            Return
        End If
        BT_StartLogin.Visible = False
        BT_CancelLogin.Visible = True


        Base_Connect.Hide()
        Base_Settings.Hide()
        Base.ME_CLOSE_BG.Visible = True
        Base.Opacity = 0
        Base.Settings_List.Visible = False
        Base.shadowplay.Visible = True

        Base_Background_Top.d.Visible = True
        Base_Background_Top.ME_CLOSE_BG_GRE.Visible = True
        Base_Background_Top.ME_CLOSE_BG.Visible = True

        Base.ShowMainPanel()
        Base.Opacity = 0.85
        Base.IF_OpenShare = True
        Base.HideAllControls()
        Me.Hide()

        Dim outcome As DulukaAuthFlow.Outcome = Await _flow.RunAsync(AddressOf Report)
        _flow = Nothing
        If IsDisposed OrElse Not IsHandleCreated Then Return

        BT_CancelLogin.Visible = False
        Base.ShowMainPanel()
        Base.OpenSettings()
        Base.IF_OpenShare = False
        If outcome.Succeeded Then
            Status_TEXT.Text = "Signed in to your Duluka Account."
            Await Task.Delay(900)
            If IsDisposed OrElse Not IsHandleCreated Then Return
            
            
            
            
            Base_Connect.ArmForcedSetupGate()
            Base.OpenPanel(Base_Connect, Base_Connect.Settings_Panel)
            Me.Hide()
        Else
            
            
            
            Me.Show()
            Status_TEXT.Text = outcome.Message
            BT_StartLogin.Visible = True
        End If
    End Sub

    Private Sub BT_CancelLogin_Click(sender As Object, e As EventArgs) Handles BT_CancelLogin.Click
        If _flow IsNot Nothing Then
            _flow.Cancel()
            Status_TEXT.Text = "Cancelling…"
        End If
    End Sub

    Private Sub BT_Back_Click(sender As Object, e As EventArgs) Handles BT_Back.Click
        If _flow IsNot Nothing Then
            _flow.Cancel()
        End If
        Me.Hide()
        Base_Connect.ReturnFromSubPage()
    End Sub

    Private Sub Settings_Panel_Paint(sender As Object, e As PaintEventArgs) Handles Settings_Panel.Paint

    End Sub
End Class
