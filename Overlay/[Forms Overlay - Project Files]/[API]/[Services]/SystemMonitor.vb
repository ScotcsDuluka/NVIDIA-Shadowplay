Imports System.Runtime.InteropServices
Imports System.Diagnostics

Public Class SystemMonitor
    
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

    
    Private timer As Timer
    Private cpuCounter As PerformanceCounter

    
    Public RamThreshold80 As Integer = 80
    Public RamThreshold95 As Integer = 95
    Public CpuThreshold As Integer = 95
    Public DiskThresholdGB As Integer = 10

    
    Private ram80Warned As Boolean = False
    Private ram95Warned As Boolean = False
    Private ramCriticalLastWarn As DateTime = DateTime.MinValue
    Private cpuWarned As Boolean = False
    Private diskLastWarn As DateTime = DateTime.MinValue

    
    
    Private diskPathFailLogged As Boolean = False

    
    Public MonitorDiskPath As String = ""

    
    Public Sub StartMonitoring()
        cpuCounter = New PerformanceCounter("Processor", "% Processor Time", "_Total")

        timer = New Timer()
        timer.Interval = 1000
        AddHandler timer.Tick, AddressOf CheckSystem
        timer.Start()
    End Sub

    
    Public Sub StopMonitoring()
        If timer IsNot Nothing Then
            timer.Stop()
            timer.Dispose()
        End If
        If cpuCounter IsNot Nothing Then
            cpuCounter.Dispose()
        End If
    End Sub

    
    Private Sub CheckSystem(sender As Object, e As EventArgs)
        CheckRam()
        CheckCpu()
        CheckDiskSpace()
    End Sub

    
    Private Sub CheckRam()
        Dim ramInfo As New MEMORYSTATUSEX()
        ramInfo.dwLength = CUInt(Marshal.SizeOf(GetType(MEMORYSTATUSEX)))

        If GlobalMemoryStatusEx(ramInfo) Then
            Dim ramPercent As Integer = CInt(ramInfo.dwMemoryLoad)

            
            
            
            
            
            If ramPercent >= 100 Then
                
                If Not ram95Warned OrElse (DateTime.Now - ramCriticalLastWarn).TotalSeconds >= 10 Then
                    Base.ShowNotifier("ramwramcritical")
                    ramCriticalLastWarn = DateTime.Now
                    ram95Warned = True
                End If
                ram80Warned = True

            ElseIf ramPercent >= RamThreshold95 Then
                
                
                
                
                Dim now As DateTime = DateTime.Now
                If (now - ramCriticalLastWarn).TotalSeconds >= 10 Then
                    Base.ShowNotifier("ramwram95")
                    ramCriticalLastWarn = now
                End If
                ram80Warned = True

            ElseIf ramPercent >= RamThreshold80 Then
                
                If Not ram80Warned Then
                    Base.ShowNotifier("ramwram")
                    ram80Warned = True
                End If
                ram95Warned = False
                ramCriticalLastWarn = DateTime.MinValue

            Else
                
                ram80Warned = False
                ram95Warned = False
                ramCriticalLastWarn = DateTime.MinValue
            End If
        End If
    End Sub

    
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

    
    Private Sub CheckDiskSpace()
        If Base.RecordValue = False Then
            Exit Sub
        End If
        Try
            Dim path As String = MonitorDiskPath
            If String.IsNullOrEmpty(path) Then path = "C:\"

            
            
            
            
            Dim root As String = IO.Path.GetPathRoot(IO.Path.GetFullPath(path))
            If root Is Nothing OrElse root.Length < 2 OrElse root(1) <> ":"c Then
                Throw New ArgumentException("MonitorDiskPath has no drive root: " & path)
            End If

            Dim drive As New IO.DriveInfo(root)
            Dim freeGB As Double = drive.AvailableFreeSpace / (1024 * 1024 * 1024)
            diskPathFailLogged = False

            If freeGB < DiskThresholdGB Then
                
                Dim now As DateTime = DateTime.Now
                If (now - diskLastWarn).TotalSeconds >= 10 Then
                    Base.ShowNotifier("diskspacelow")
                    diskLastWarn = now
                End If
            Else
                diskLastWarn = DateTime.MinValue
            End If
        Catch ex As Exception
            
            If Not diskPathFailLogged Then
                diskPathFailLogged = True
                Try
                    If Base.tcp IsNot Nothing Then
                        Base.tcp.Send("[SystemMonitor] disk check skipped: " & ex.Message)
                    End If
                Catch
                End Try
            End If
        End Try
    End Sub

    
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

    
    Public Function GetCpuPercent() As Integer
        If cpuCounter IsNot Nothing Then
            Return CInt(cpuCounter.NextValue())
        End If
        Return 0
    End Function

    
    Public Function GetDiskInfo() As (FreeGB As Double, TotalGB As Double)
        Try
            Dim path As String = MonitorDiskPath
            If String.IsNullOrEmpty(path) Then path = "C:\"

            
            Dim root As String = IO.Path.GetPathRoot(IO.Path.GetFullPath(path))
            If root Is Nothing OrElse root.Length < 2 OrElse root(1) <> ":"c Then
                Return (0, 0)
            End If

            Dim drive As New IO.DriveInfo(root)
            Return (
                drive.AvailableFreeSpace / (1024 * 1024 * 1024),
                drive.TotalSize / (1024 * 1024 * 1024)
            )
        Catch
            Return (0, 0)
        End Try
    End Function
End Class