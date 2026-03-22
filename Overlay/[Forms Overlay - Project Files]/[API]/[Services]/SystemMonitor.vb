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
    Public RamThreshold As Integer = 80
    Public CpuThreshold As Integer = 80

    ' Flag ป้องกันเตือนซ้ำ
    Private ramWarned As Boolean = False
    Private cpuWarned As Boolean = False

    ' ===== เริ่มตรวจจับ =====
    Public Sub StartMonitoring()
        ' สร้าง CPU Counter
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
    End Sub

    ' ===== ตรวจ RAM =====
    Private Sub CheckRam()
        Dim ramInfo As New MEMORYSTATUSEX()
        ramInfo.dwLength = CUInt(Marshal.SizeOf(GetType(MEMORYSTATUSEX)))

        If GlobalMemoryStatusEx(ramInfo) Then
            Dim ramPercent As Integer = CInt(ramInfo.dwMemoryLoad)

            If ramPercent >= RamThreshold Then
                If Not ramWarned Then
                    Dim totalGB As Double = ramInfo.ullTotalPhys / (1024 * 1024 * 1024)
                    Dim availGB As Double = ramInfo.ullAvailPhys / (1024 * 1024 * 1024)

                    Base.ShowNotifier("ramwram")
                    ramWarned = True
                End If
            Else
                ramWarned = False
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
End Class