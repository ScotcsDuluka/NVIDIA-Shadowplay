Imports System.Runtime.InteropServices
Imports System.Diagnostics

Public Class SystemMonitor
    ' ===== Windows API =====
    <DllImport("kernel32.dll")>
    Private Shared Function GlobalMemoryStatusEx(ByRef lpBuffer As MEMORYSTATUSEX) As Boolean
    End Function

    <StructLayout(LayoutKind.Sequential)>
    Private Structure MEMORYSTATUSEX
        Public dwLength As UInteger
        Public dwMemoryLoad As UInteger
        Public ullTotalPhys As ULong
        Public ullAvailPhys As ULong
        Public ullTotalPageFile As ULong
        Public ullAvailPageFile As ULong
        Public ullTotalVirtual As ULong
        Public ullAvailVirtual As ULong
        Public ullAvailExtendedVirtual As ULong
    End Structure

    ' ===== ตัวแปร =====
    Private timer As Timer
    Private cpuCounter As PerformanceCounter

    ' Threshold
    Public RamThreshold80 As Integer = 80
    Public RamThreshold95 As Integer = 95
    Public CpuThreshold As Integer = 95
    Public DiskThresholdGB As Integer = 10

    ' Flag ป้องกันเตือนซ้ำ
    Private ram80Warned As Boolean = False
    Private ram95Warned As Boolean = False
    Private ramCriticalLastWarn As DateTime = DateTime.MinValue
    Private cpuWarned As Boolean = False
    Private diskLastWarn As DateTime = DateTime.MinValue

    ' Path ที่จะตรวจพื้นที่
    Public MonitorDiskPath As String = ""

    ' ===== เริ่มตรวจจับ =====
    Public Sub StartMonitoring()
        cpuCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")

        timer = New Timer()
        timer.Interval = 1000
        AddHandler timer.Tick, AddressOf CheckSystem
        timer.Start()
    End Sub

    ' ===== หยุดตรวจจับ =====
    Public Sub StopMonitoring()
        If timer IsNot Nothing Then
            timer.Stop()
            timer.Dispose()
        End If
        If cpuCounter IsNot Nothing Then
            cpuCounter.Dispose()
        End If
    End Sub

    ' ===== ตรวจสอบระบบ =====
    Private Sub CheckSystem(sender As Object, e As EventArgs)
        CheckRam()
        CheckCpu()
        CheckDiskSpace()
    End Sub

    ' ===== ตรวจ RAM =====
    Private Sub CheckRam()
        Dim ramInfo As New MEMORYSTATUSEX()
        ramInfo.dwLength = CUInt(Marshal.SizeOf(GetType(MEMORYSTATUSEX)))

        If GlobalMemoryStatusEx(ramInfo) Then
            Dim ramPercent As Integer = CInt(ramInfo.dwMemoryLoad)

            ' ===== 95-99% : เตือนทุก 10 วินาที =====
            If ramPercent >= RamThreshold95 AndAlso ramPercent < 100 Then
                Dim now As DateTime = DateTime.Now
                If (now - ramCriticalLastWarn).TotalSeconds >= 10 Then
                    Base.ShowNotifier("ramwramcritical")
                    ramCriticalLastWarn = now
                End If

                ram80Warned = False
                ram95Warned = False

                ' ===== 95% : เตือนครั้งเดียว =====
            ElseIf ramPercent >= RamThreshold95 Then
                If Not ram95Warned Then
                    Base.ShowNotifier("ramwram95")
                    ram95Warned = True
                End If
                ram80Warned = False
                ramCriticalLastWarn = DateTime.MinValue

                ' ===== 80-95% : เตือนครั้งเดียว =====
            ElseIf ramPercent >= RamThreshold80 Then
                If Not ram80Warned Then
                    Base.ShowNotifier("ramwram")
                    ram80Warned = True
                End If
                ram95Warned = False
                ramCriticalLastWarn = DateTime.MinValue

                ' ===== < 80% : Reset ทุกอย่าง =====
            Else
                ram80Warned = False
                ram95Warned = False
                ramCriticalLastWarn = DateTime.MinValue
            End If
        End If
    End Sub

    ' ===== ตรวจ CPU =====
    Private Sub CheckCpu()
        Dim cpuPercent As Integer = CInt(cpuCounter.NextValue())

        If cpuPercent >= CpuThreshold Then
            If Not cpuWarned Then
                Base.ShowNotifier("cpuwram")
                cpuWarned = True
            End If
        Else
            cpuWarned = False
        End If
    End Sub

    ' ===== ตรวจพื้นที่ดิสก์ =====
    Private Sub CheckDiskSpace()
        If Base.RecordValue = False Then
            Exit Sub
        End If
        Try
            Dim path As String = MonitorDiskPath
            If String.IsNullOrEmpty(path) Then path = "C:\"

            Dim drive As New IO.DriveInfo(path)
            Dim freeGB As Double = drive.AvailableFreeSpace / (1024 * 1024 * 1024)

            If freeGB < DiskThresholdGB Then
                ' ===== เตือนทุก 10 วินาที =====
                Dim now As DateTime = DateTime.Now
                If (now - diskLastWarn).TotalSeconds >= 10 Then
                    Base.ShowNotifier("diskspacelow")
                    diskLastWarn = now
                End If
            Else
                diskLastWarn = DateTime.MinValue
            End If
        Catch ex As Exception
            ' ถ้าไม่เจอ Drive ข้ามไป
        End Try
    End Sub

    ' ===== ดึงข้อมูล RAM =====
    Public Function GetRamInfo() As (Percent As Integer, TotalGB As Double, AvailableGB As Double)
        Dim ramInfo As New MEMORYSTATUSEX()
        ramInfo.dwLength = CUInt(Marshal.SizeOf(GetType(MEMORYSTATUSEX)))

        If GlobalMemoryStatusEx(ramInfo) Then
            Return (
                CInt(ramInfo.dwMemoryLoad),
                ramInfo.ullTotalPhys / (1024 * 1024 * 1024),
                ramInfo.ullAvailPhys / (1024 * 1024 * 1024)
            )
        End If

        Return (0, 0, 0)
    End Function

    ' ===== ดึงข้อมูล CPU =====
    Public Function GetCpuPercent() As Integer
        If cpuCounter IsNot Nothing Then
            Return CInt(cpuCounter.NextValue())
        End If
        Return 0
    End Function

    ' ===== ดึงข้อมูล Disk =====
    Public Function GetDiskInfo() As (FreeGB As Double, TotalGB As Double)
        Try
            Dim path As String = MonitorDiskPath
            If String.IsNullOrEmpty(path) Then path = "C:\"

            Dim drive As New IO.DriveInfo(path)
            Return (
                drive.AvailableFreeSpace / (1024 * 1024 * 1024),
                drive.TotalSize / (1024 * 1024 * 1024)
            )
        Catch
            Return (0, 0)
        End Try
    End Function
End Class