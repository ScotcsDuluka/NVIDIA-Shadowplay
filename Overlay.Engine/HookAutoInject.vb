' HookAutoInject.vb — watches whitelisted games and injects the in-game
' hook DLL (NvidiaShareHook.dll) the moment a game process appears.
' Pure memory injection (OpenProcess + CreateRemoteThread + LoadLibraryW) —
' ZERO files written to the game folder (owner rule). The DLL itself lives
' in <runtime>\Hooks\ and is never copied anywhere else.
'
' Whitelist: Data\hook-whitelist.json → games:[{name, exe, consent}]
' "exe" is the process filename (with or without .exe); path is optional
' and is never required for process detection. Only consent:true entries
' are injected.

Imports System.IO
Imports System.Diagnostics
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading

Public Class HookAutoInject

    Private Shared _thread As Thread
    Private Shared _stopFlag As Boolean
    Private Shared ReadOnly Injected As New HashSet(Of Integer)()   ' game PIDs already handled
    Private Shared ReadOnly Failed As New HashSet(Of Integer)()     ' do not retry-loop on failure

    Public Shared Sub Start()
        If _thread IsNot Nothing Then Return
        _stopFlag = False
        _thread = New Thread(AddressOf WatchLoop) With {.IsBackground = True, .Name = "HookAutoInject"}
        _thread.Start()
    End Sub

    Public Shared Sub [Stop]()
        _stopFlag = True
        Try : _thread?.Join(500) : Catch : End Try
    End Sub

    Private Shared Sub L(m As String)
        Try
            Dim p As String = AppLayout.P("Logs", "autoinject.log")
            Dim d As String = IO.Path.GetDirectoryName(p)
            If Not IO.Directory.Exists(d) Then IO.Directory.CreateDirectory(d)
            IO.File.AppendAllText(p, DateTime.Now.ToString("HH:mm:ss.fff") & " " & m & Environment.NewLine)
        Catch
        End Try
    End Sub

    Private Class WatchEntry
        Public Name As String
        Public Exe As String          ' process name, no .exe
        Public Consent As Boolean
    End Class

    Private Shared Function LoadWatchList() As List(Of WatchEntry)
        Dim out As New List(Of WatchEntry)()
        Try
            Dim wlPath As String = AppLayout.P("Data", "hook-whitelist.json")
            If Not File.Exists(wlPath) Then Return out
            Dim wl As System.Text.Json.Nodes.JsonObject =
                System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(wlPath)).AsObject()
            If wl("games") Is Nothing Then Return out
            For Each g As System.Text.Json.Nodes.JsonNode In wl("games").AsArray()
                Dim obj As System.Text.Json.Nodes.JsonObject = TryCast(g, System.Text.Json.Nodes.JsonObject)
                If obj Is Nothing Then Continue For
                Dim e As New WatchEntry()
                e.Name = If(obj("name")?.ToString(), "?")
                e.Consent = obj("consent")?.GetValue(Of Boolean)() = True
                e.Exe = obj("exe")?.ToString()
                If Not String.IsNullOrEmpty(e.Exe) Then
                    ' whitelist stores "Dungeons.exe" but GetProcessesByName
                    ' wants the bare name — strip any extension
                    If e.Exe.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) Then
                        e.Exe = e.Exe.Substring(0, e.Exe.Length - 4)
                    End If
                End If
                ' Smash is intentionally blocked: loading the native hook
                ' while the game is running has caused instability. Geometry
                ' Dash also needs Desktop mode because its OpenGL present
                ' boundary is unknown.
                If e.Consent AndAlso Not String.IsNullOrEmpty(e.Exe) AndAlso
                   Not IsNativeHookBlocked(e.Exe) Then out.Add(e)
            Next
        Catch ex As Exception
            L("whitelist read failed: " & ex.Message)
        End Try
        Return out
    End Function

    Private Shared Function IsRendererCompatible(p As Process) As Boolean
        Try
            Dim hasDxgi As Boolean = False
            Dim hasD3d As Boolean = False
            For Each m As ProcessModule In p.Modules
                Dim n As String = m.ModuleName
                If String.Equals(n, "dxgi.dll", StringComparison.OrdinalIgnoreCase) Then hasDxgi = True
                If String.Equals(n, "d3d11.dll", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(n, "d3d12.dll", StringComparison.OrdinalIgnoreCase) Then hasD3d = True
                If n.IndexOf("easyanticheat", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   n.IndexOf("eac", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   n.IndexOf("battleye", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   n.IndexOf("beservice", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   n.IndexOf("vgk", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   n.IndexOf("xigncode", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                   n.IndexOf("gameguard", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    L("compatibility denied for " & p.ProcessName & " (protected module " & n & ")")
                    Return False
                End If
            Next
            If Not hasDxgi OrElse Not hasD3d Then
                L("compatibility pending for " & p.ProcessName & " (D3D not loaded yet)")
                Return False
            End If
            Return True
        Catch ex As Exception
            L("compatibility check failed for " & p.ProcessName & ": " & ex.Message)
            Return False
        End Try
    End Function

    Private Shared Function IsNativeHookBlocked(exeName As String) As Boolean
        Return String.Equals(exeName, "Smash_Legends", StringComparison.OrdinalIgnoreCase) OrElse
               String.Equals(exeName, "GeometryDash", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Sub WatchLoop()
        ' settle: let the engine boot before the first scan
        Thread.Sleep(4000)
        While Not _stopFlag
            Try
                Dim dll As String = AppLayout.P("Hooks", "NvidiaShareHook.dll")
                If Not File.Exists(dll) Then
                    ' no DLL deployed — nothing to inject, idle quietly
                    Thread.Sleep(5000)
                    Continue While
                End If
                For Each e As WatchEntry In LoadWatchList()
                    For Each p As Process In Process.GetProcessesByName(e.Exe)
                        Dim pid As Integer = p.Id
                        If Not Injected.Contains(pid) AndAlso Not Failed.Contains(pid) Then
                            If Not IsRendererCompatible(p) Then
                                Continue For
                            End If
                            If IsHookModuleLoaded(p, dll) Then
                                Injected.Add(pid)
                                L("already loaded in " & e.Exe & " (pid " & pid & ", " & e.Name & ")")
                                Continue For
                            End If
                            Dim rc As Integer = Inject(p.Id, dll)
                            If rc = 0 Then
                                Injected.Add(pid)
                                L("injected into " & e.Exe & " (pid " & pid & ", " & e.Name & ")")
                            Else
                                Failed.Add(pid)
                                L("inject " & e.Exe & " (pid " & pid & ") failed rc=" & rc)
                            End If
                        End If
                        p.Dispose()
                    Next
                Next
            Catch ex As Exception
                L("watch loop error: " & ex.Message)
            End Try
            Thread.Sleep(3000)
        End While
    End Sub

    Private Shared Function IsHookModuleLoaded(p As Process, dllPath As String) As Boolean
        Try
            Dim expectedName As String = Path.GetFileName(dllPath)
            For Each m As ProcessModule In p.Modules
                If String.Equals(m.ModuleName, expectedName, StringComparison.OrdinalIgnoreCase) Then Return True
                If String.Equals(m.FileName, dllPath, StringComparison.OrdinalIgnoreCase) Then Return True
            Next
        Catch ex As Exception
            ' Module enumeration can fail on protected or exiting processes.
            ' In that case, fall back to the normal one-shot injection path.
        End Try
        Return False
    End Function

    ' ── native injection (same routine as GameHook\inject.ps1) ──
    Private Const PROCESS_ALL_ACCESS As UInteger = &H1F0FFFUI
    Private Const MEM_COMMIT As UInteger = &H1000UI
    Private Const MEM_RESERVE As UInteger = &H2000UI
    Private Const PAGE_READWRITE As UInteger = &H40UI

    Private Shared Function Inject(pid As Integer, dllPath As String) As Integer
        Dim proc As IntPtr = OpenProcess(PROCESS_ALL_ACCESS, False, CUInt(pid))
        If proc = IntPtr.Zero Then Return 1
        Try
            Dim k32 As IntPtr = GetModuleHandleW("kernel32.dll")
            Dim loadLib As IntPtr = GetProcAddress(k32, "LoadLibraryW")
            If loadLib = IntPtr.Zero Then Return 2
            Dim pathBytes As Byte() = Encoding.Unicode.GetBytes(dllPath & ControlChars.NullChar)
            Dim addr As IntPtr = VirtualAllocEx(proc, IntPtr.Zero,
                New UIntPtr(CUInt(pathBytes.Length)), MEM_COMMIT Or MEM_RESERVE, PAGE_READWRITE)
            If addr = IntPtr.Zero Then Return 3
            If Not WriteProcessMemory(proc, addr, pathBytes, New UIntPtr(CUInt(pathBytes.Length)), IntPtr.Zero) Then Return 4
            Dim thread As IntPtr = CreateRemoteThread(proc, IntPtr.Zero, UIntPtr.Zero,
                loadLib, addr, 0, IntPtr.Zero)
            If thread = IntPtr.Zero Then Return 5
            CloseHandle(thread)
            Return 0
        Finally
            CloseHandle(proc)
        End Try
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function OpenProcess(access As UInteger, inherit As Boolean, pid As UInteger) As IntPtr
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
    Private Shared Function GetModuleHandleW(name As String) As IntPtr
    End Function
    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Ansi)>
    Private Shared Function GetProcAddress(moduleHandle As IntPtr, name As String) As IntPtr
    End Function
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function VirtualAllocEx(proc As IntPtr, addr As IntPtr, size As UIntPtr, type As UInteger, protect As UInteger) As IntPtr
    End Function
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function WriteProcessMemory(proc As IntPtr, addr As IntPtr, buf As Byte(), size As UIntPtr, written As IntPtr) As <Runtime.InteropServices.MarshalAs(UnmanagedType.Bool)> Boolean
    End Function
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Shared Function CreateRemoteThread(proc As IntPtr, attr As IntPtr, size As UIntPtr, start As IntPtr, param As IntPtr, flags As UInteger, tid As IntPtr) As IntPtr
    End Function
    <DllImport("kernel32.dll")>
    Private Shared Function CloseHandle(h As IntPtr) As <Runtime.InteropServices.MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

End Class
