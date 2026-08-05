Imports System.Diagnostics
Imports System.Runtime.InteropServices

Namespace CaptureCore
    Public Module JobObjectManager

        ' ════════════════════════════════════════════════════════════════════
        ' P/Invoke — kernel32
        ' ════════════════════════════════════════════════════════════════════
        <DllImport("kernel32.dll", CharSet:=CharSet.Unicode)>
        Private Function CreateJobObject(ByVal lpJobAttributes As IntPtr, ByVal lpName As String) As IntPtr
        End Function

        <DllImport("kernel32.dll")>
        Private Function AssignProcessToJobObject(ByVal hJob As IntPtr, ByVal hProcess As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll")>
        Private Function SetInformationJobObject(ByVal hJob As IntPtr, ByVal JobObjectInfoClass As JOBOBJECTINFOCLASS, ByVal lpJobObjectInfo As JOBOBJECT_BASIC_LIMIT_INFORMATION, ByVal cbJobObjectInfoLength As UInteger) As Boolean
        End Function

        <DllImport("kernel32.dll")>
        Private Function CloseHandle(ByVal hObject As IntPtr) As Boolean
        End Function

        Private Enum JOBOBJECTINFOCLASS
            BasicLimitInformation = 2
        End Enum

        <StructLayout(LayoutKind.Sequential)>
        Private Structure JOBOBJECT_BASIC_LIMIT_INFORMATION
            Public PerProcessUserTimeLimit As Long
            Public PerJobUserTimeLimit As Long
            Public LimitFlags As UInteger
            Public MinimumWorkingSetSize As IntPtr
            Public MaximumWorkingSetSize As IntPtr
            Public ActiveProcessLimit As UInteger
            Public Affinity As IntPtr
            Public PriorityClass As UInteger
            Public SchedulingClass As UInteger
        End Structure

        Private Const JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE As UInteger = &H2000

        Private jobHandle As IntPtr = IntPtr.Zero
        Private jobInitialized As Boolean = False
        Private jobLock As New Object()

        Public Sub InitializeJobObject()
            SyncLock jobLock
                If jobInitialized Then Exit Sub

                Try
                    jobHandle = CreateJobObject(IntPtr.Zero, Nothing)
                    If jobHandle = IntPtr.Zero Then Exit Sub

                    Dim info As New JOBOBJECT_BASIC_LIMIT_INFORMATION()
                    info.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE

                    SetInformationJobObject(
                        jobHandle,
                        JOBOBJECTINFOCLASS.BasicLimitInformation,
                        info,
                        CUInt(Marshal.SizeOf(GetType(JOBOBJECT_BASIC_LIMIT_INFORMATION)))
                    )

                    jobInitialized = True
                Catch ex As Exception
                    Debug.WriteLine("Job Object init error: " & ex.Message)
                End Try
            End SyncLock
        End Sub

        ''' <summary>
        ''' Attaches a Process to the Job Object so it is auto-killed when
        ''' the parent process exits. Should be called right after proc.Start().
        ''' No-op if the Job Object failed to initialize.
        ''' </summary>
        Public Sub AddProcessToJob(proc As Process)
            If proc Is Nothing Then Exit Sub
            InitializeJobObject()

            SyncLock jobLock
                If jobHandle <> IntPtr.Zero Then
                    Try
                        AssignProcessToJobObject(jobHandle, proc.Handle)
                    Catch ex As Exception
                        Debug.WriteLine("AddProcessToJob error: " & ex.Message)
                    End Try
                End If
            End SyncLock
        End Sub

    End Module

End Namespace
