' AppSettings (hardware detection) — GPU vendor flags via three probes:
'   FAST  (sync, UI thread): Registry DriverDesc → System32 DLL check  (<50ms)
'   SLOW  (background once): PowerShell Get-CimInstance                (~5s worst case)
' AV1 (NVENC) is assumed for RTX 40/50 series ("RTX 40"/"RTX 50"/"Ada").

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Threading

Partial Public Class AppSettings
    Private Shared ReadOnly NvidiaKeywords As String() = {"NVIDIA", "GEFORCE", "GTX", "RTX"}
    Private Shared ReadOnly AmdKeywords As String() = {"AMD", "RADEON", "RX "}
    Private Shared ReadOnly IntelKeywords As String() = {"INTEL"}
    Private Shared ReadOnly IntelIGpuKeywords As String() = {"UHD", "IRIS", "HD GRAPHICS", "INTEL(R) GRAPHICS"}

#Region "Hardware Detection"
    Private Shared _hasNvidia As Boolean? = Nothing
    Private Shared _hasIntel As Boolean? = Nothing
    Private Shared _hasAMD As Boolean? = Nothing
    Private Shared _gpuName As String = ""
    Private Shared _intelGpuName As String = ""
    Private Shared _supportsAV1 As Boolean? = Nothing

    ' Store all detected GPU names
    Private Shared _allGpuNames As New List(Of String)()

    ''' <summary>
    ''' Check if NVIDIA GPU is available
    ''' </summary>
    Public Shared ReadOnly Property HasNvidia As Boolean
        Get
            Return _hasNvidia.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Check if Intel GPU is available
    ''' </summary>
    Public Shared ReadOnly Property HasIntel As Boolean
        Get
            Return _hasIntel.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Check if AMD GPU is available
    ''' </summary>
    Public Shared ReadOnly Property HasAMD As Boolean
        Get
            Return _hasAMD.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Get primary GPU name (NVIDIA > AMD > Intel)
    ''' </summary>
    Public Shared ReadOnly Property GPUName As String
        Get
            Return _gpuName
        End Get
    End Property

    ''' <summary>
    ''' Get Intel iGPU name
    ''' </summary>
    Public Shared ReadOnly Property IntelGPUName As String
        Get
            Return _intelGpuName
        End Get
    End Property

    ''' <summary>
    ''' Check if GPU supports AV1 encoding (RTX 40 series+)
    ''' </summary>
    Public Shared ReadOnly Property SupportsNVENCAV1 As Boolean
        Get
            If _supportsAV1 Is Nothing Then
                DetectAV1Support()
            End If
            Return _supportsAV1.GetValueOrDefault(False)
        End Get
    End Property

    ''' <summary>
    ''' Detect AV1 support - RTX 40 series or newer
    ''' </summary>
    Private Shared Sub DetectAV1Support()
        _supportsAV1 = False

        If Not _hasNvidia.GetValueOrDefault(False) Then
            Exit Sub
        End If

        ' AV1 supported on: RTX 40 series (Ada Lovelace)
        Dim gpuUpper As String = _gpuName.ToUpperInvariant()

        If gpuUpper.Contains("RTX 40") OrElse
           gpuUpper.Contains("RTX 50") OrElse
           gpuUpper.Contains("ADA") Then
            _supportsAV1 = True
        End If

        Debug.WriteLine("AV1 Support: " & _supportsAV1.ToString() & " (GPU: " & _gpuName & ")")
    End Sub

    ' PowerShell CIM probe ตัวเดียวที่ช้า (สูงสุด ~5s) — ต้องไม่ถูกเรียกบน UI thread
    ' Interlocked guard: ตลอดอายุโปรเซส probe ตัวนี้ถูกเรียก "ครั้งเดียว"
    Private Shared _psProbeStarted As Integer = 0

    ''' <summary>
    ''' Detect available GPUs
    ''' </summary>
    Public Shared Sub DetectHardware()
        ' Skip if already detected
        If _hardwareDetected Then
            Debug.WriteLine("DetectHardware: Already detected, skipping")
            Exit Sub
        End If

        Try
            Debug.WriteLine("══════════ DetectHardware START ══════════")

            _hasNvidia = False
            _hasIntel = False
            _hasAMD = False
            _allGpuNames.Clear()

            ' FAST probes มาก่อน (Registry + DLL < 50ms) — UI thread ไม่ต้องรอ
            ' ผลลัพธ์เกือบทุกเครื่องเหมือน PowerShell CIM (DriverDesc ก็คือชื่อ GPU เดียวกัน)

            ' Registry Detection
            DetectGPUsViaRegistry()

            ' DLL Check (final fallback)
            Dim system32 As String = Environment.SystemDirectory

            ' NVIDIA - ต้องมี nvenc.dll
            If Not _hasNvidia.GetValueOrDefault(False) Then
                If File.Exists(Path.Combine(system32, "nvenc.dll")) Then
                    _hasNvidia = True
                    Debug.WriteLine("NVIDIA detected via nvenc.dll")
                End If
            End If

            ' AMD - amdocl64.dll
            If Not _hasAMD.GetValueOrDefault(False) Then
                If File.Exists(Path.Combine(system32, "amdocl64.dll")) Then
                    _hasAMD = True
                    Debug.WriteLine("AMD detected via amdocl64.dll")
                End If
            End If

            RecomputePrimaryGpuName()

            ' Mark as detected — แอปใช้งานได้เต็มรูปแบบจากผล fast probes แล้ว
            _hardwareDetected = True

            Debug.WriteLine("══════════ DetectHardware RESULT (fast probes) ══════════")
            Debug.WriteLine("  NVIDIA: " & _hasNvidia.ToString())
            Debug.WriteLine("  Intel:  " & _hasIntel.ToString())
            Debug.WriteLine("  AMD:    " & _hasAMD.ToString())
            Debug.WriteLine("  Primary GPU: " & _gpuName)
            Debug.WriteLine("═══════════════════════════════════════════")

            ' SLOW probe (PowerShell CIM, สูงสุด ~5s) → background เท่านั้น
            ' ทำหน้าที่ยืนยัน/เสริมผล ไม่ใช่เงื่อนไขของ startup
            ' Thread-safety: _allGpuNames ถูกเขียนโดย registry probe (จบก่อน Task.Run
            ' ถูก schedule) และ PowerShell probe ที่วิ่งครั้งเดียวจาก flag นี้ — ไม่มีทางทับกัน
            If Interlocked.CompareExchange(_psProbeStarted, 1, 0) = 0 Then
                Task.Run(Sub()
                             DetectGPUsViaPowerShell()
                             RecomputePrimaryGpuName()
                             _supportsAV1 = Nothing   ' ให้คำนวณใหม่จากชื่อที่อาจละเอียดขึ้น
                             Debug.WriteLine("PowerShell GPU probe finished (background)")
                         End Sub)
            End If

        Catch ex As Exception
            Debug.WriteLine("DetectHardware Error: " & ex.Message)
            _hardwareDetected = True ' Still mark as detected to prevent loops
        End Try
    End Sub

    ''' <summary>เลือกชื่อ GPU หลักจากทุกชื่อที่ probe เจอ (NVIDIA &gt; AMD &gt; Intel)</summary>
    Private Shared Sub RecomputePrimaryGpuName()
        If _hasNvidia.GetValueOrDefault(False) Then
            _gpuName = _allGpuNames.FirstOrDefault(Function(n) n.ToUpperInvariant().Contains("NVIDIA"), "NVIDIA GPU")
        ElseIf _hasAMD.GetValueOrDefault(False) Then
            _gpuName = _allGpuNames.FirstOrDefault(Function(n) n.ToUpperInvariant().Contains("AMD") OrElse n.ToUpperInvariant().Contains("RADEON"), "AMD GPU")
        ElseIf _hasIntel.GetValueOrDefault(False) Then
            _gpuName = _intelGpuName
        End If
    End Sub

    ''' <summary>
    ''' Detect GPUs using PowerShell
    ''' </summary>
    Private Shared Sub DetectGPUsViaPowerShell()
        Try
            Dim psi As New ProcessStartInfo With {
                .FileName = "powershell.exe",
                .Arguments = "-NoProfile -Command " & Chr(34) & "Get-CimInstance Win32_VideoController | Select-Object -ExpandProperty Name" & Chr(34),
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .CreateNoWindow = True
            }

            Using proc As Process = Process.Start(psi)
                If proc IsNot Nothing Then
                    Dim output As String = proc.StandardOutput.ReadToEnd()
                    proc.WaitForExit(5000)

                    Debug.WriteLine("PowerShell GPU Output: " & output.Trim())

                    Dim lines() As String = output.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)

                    For Each line As String In lines
                        Dim trimmed As String = line.Trim()
                        If String.IsNullOrEmpty(trimmed) Then Continue For
                        If trimmed.ToUpperInvariant().Contains("NAME") Then Continue For

                        AddGpuNameIfMissing(trimmed)
                        UpdateGpuFlagsFromName(trimmed)
                    Next
                End If
            End Using

        Catch ex As Exception
            Debug.WriteLine("DetectGPUsViaPowerShell Error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Detect GPUs via Windows Registry
    ''' </summary>
    Private Shared Sub DetectGPUsViaRegistry()
        Try
            Const GPU_REGISTRY_PATH As String = "SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}"

            Using key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(GPU_REGISTRY_PATH)
                If key Is Nothing Then Exit Sub

                For Each subKeyName As String In key.GetSubKeyNames()
                    Using subKey = key.OpenSubKey(subKeyName)
                        If subKey Is Nothing Then Continue For

                        Dim driverDesc As String = subKey.GetValue("DriverDesc", "").ToString()
                        If String.IsNullOrEmpty(driverDesc) Then Continue For
                        AddGpuNameIfMissing(driverDesc)
                        UpdateGpuFlagsFromName(driverDesc, True)
                    End Using
                Next
            End Using

        Catch ex As Exception
            Debug.WriteLine("DetectGPUsViaRegistry Error: " & ex.Message)
        End Try
    End Sub

    Private Shared Sub AddGpuNameIfMissing(gpuName As String)
        If String.IsNullOrWhiteSpace(gpuName) Then Return
        If Not _allGpuNames.Contains(gpuName) Then
            _allGpuNames.Add(gpuName)
        End If
    End Sub

    Private Shared Sub UpdateGpuFlagsFromName(gpuName As String, Optional fromRegistry As Boolean = False)
        Dim upper As String = gpuName.ToUpperInvariant()
        Dim source As String = If(fromRegistry, " via Registry", "")

        If ContainsAny(upper, NvidiaKeywords) Then
            If Not _hasNvidia.GetValueOrDefault(False) Then
                _hasNvidia = True
            End If
            Debug.WriteLine("  NVIDIA detected" & source & ": " & gpuName)
            Return
        End If

        If ContainsAny(upper, AmdKeywords) Then
            If Not _hasAMD.GetValueOrDefault(False) Then
                _hasAMD = True
            End If
            Debug.WriteLine("  AMD detected" & source & ": " & gpuName)
            Return
        End If

        If ContainsAny(upper, IntelKeywords) AndAlso Not ContainsAny(upper, NvidiaKeywords) AndAlso Not ContainsAny(upper, AmdKeywords) Then
            If fromRegistry AndAlso Not ContainsAny(upper, IntelIGpuKeywords) Then
                Return
            End If

            If Not _hasIntel.GetValueOrDefault(False) Then
                _hasIntel = True
                _intelGpuName = gpuName
            End If
            Debug.WriteLine("  Intel detected" & source & ": " & gpuName)
        End If
    End Sub

    Private Shared Function ContainsAny(value As String, keywords As IEnumerable(Of String)) As Boolean
        For Each keyword As String In keywords
            If value.IndexOf(keyword, StringComparison.Ordinal) >= 0 Then
                Return True
            End If
        Next
        Return False
    End Function
#End Region

End Class

