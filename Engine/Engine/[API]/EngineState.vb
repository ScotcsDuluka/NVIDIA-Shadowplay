''' <summary>
''' ═══════════════════════════════════════════════════════════════════════════
''' Engine State — shared state สำหรับ Engine UI และ CommandHandler
''' ═══════════════════════════════════════════════════════════════════════════
''' </summary>
Public Class EngineState

    ''' <summary>ตรงกับ CommandHandler มีการ set ผ่าน events</summary>
    Public Property IsRecording As Boolean = False
    Public Property IsBuffering As Boolean = False
    Public Property IsSavingReplay As Boolean = False
    Public Property CurrentEncoder As String = "NVENC_H264"
    Public Property CurrentFPS As Integer = 60
    Public Property CurrentBitrate As Integer = 20000
    Public Property CurrentWidth As Integer = 1920
    Public Property CurrentHeight As Integer = 1080
    Public Property CurrentPreset As String = "Medium"
    Public Property OutputDirectory As String = ""
    Public Property HubConnected As Boolean = False

    ''' <summary>จำนวน commands ที่ประมวลผลแล้ว</summary>
    Public Property TotalCommands As Integer = 0

End Class
