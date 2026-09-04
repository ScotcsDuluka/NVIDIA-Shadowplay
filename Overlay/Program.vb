Imports System.Threading
Imports System.Windows.Forms

Namespace NVIDIA_Share
    Public Module Program
        Private ReadOnly InstanceMutexName As String = "Global\NVIDIA_ShadowPlay_Overlay_SingleInstance"

        <STAThread>
        Public Sub Main()
            Dim createdNew As Boolean = False
            Using instance As New Mutex(True, InstanceMutexName, createdNew)
                If Not createdNew Then Return

                AppLayout.Initialize()
                Application.SetHighDpiMode(HighDpiMode.SystemAware)
                Application.EnableVisualStyles()
                Application.SetCompatibleTextRenderingDefault(False)
                Application.Run(New Base())
            End Using
        End Sub
    End Module
End Namespace
