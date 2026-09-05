Option Strict On
Option Explicit On
Option Infer On

' HardwareGate.vb — F-01 environment preflight (C/4 test-truthfulness pass).
'
' Separates TEST FAILURE from ENVIRONMENT NOT CAPABLE: the M1/M2 suites
' drive REAL DdagrabBackend + NvencEncoderBackend (production classes) and
' cannot run on a machine without an NVIDIA adapter. Before this gate those
' suites FAILED with "DdagrabBackend: no NVIDIA adapter found" on Intel
' machines — 5 false failures that drown real regressions.
'
' Contract (per C/4):
'   - the probe enumerates DXGI adapters through the REAL backend exactly
'     once per process (Initialize → Dispose);
'   - on an NVIDIA machine nothing changes — every test RUNS;
'   - without an adapter, matching tests report SKIP (never PASS, never a
'     swallowed failure) carrying the backend's own enumeration evidence;
'   - skipped tests never affect the exit code.

Imports System
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Video
Imports CaptureEngine.Video.Backends.Ddagrab

Namespace Engine.Concurrency.Tests

    ''' <summary>Thrown by a test to report ENVIRONMENT NOT CAPABLE.
    ''' RunTest converts this into a SKIP verdict (not PASS, not FAIL).</summary>
    Friend Class SkipException
        Inherits Exception

        Public Sub New(reason As String)
            MyBase.New(reason)
        End Sub
    End Class

    Friend Module HardwareGate

        Private _probed As Boolean = False
        Private _nvidiaAvailable As Boolean = False
        Private _reason As String = ""

        ''' <summary>One-time probe: build the REAL production DdagrabBackend
        ''' and Initialize it — the backend enumerates DXGI adapters itself
        ''' and throws VideoBackendRuntimeException when no VendorId=0x10DE
        ''' adapter exists. Its message IS the adapter-enumeration evidence.</summary>
        Friend Sub EnsureProbed()
            If _probed Then Return
            _probed = True
            Dim backend As DdagrabBackend = Nothing
            Try
                backend = New DdagrabBackend(New EngineLogger("nvidia-preflight", EngineLogger.LogLevel.Warning))
                backend.Initialize(New PreflightContext())
                _nvidiaAvailable = True
                _reason = ""
            Catch ex As Exception
                _nvidiaAvailable = False
                _reason = ex.Message
            Finally
                If backend IsNot Nothing Then
                    Try : backend.Dispose() : Catch : End Try
                End If
            End Try
        End Sub

        ''' <summary>True when <paramref name="testName"/> exercises the
        ''' NVIDIA-bound production backends and the probe proved the
        ''' environment cannot run them. Test names are stable prefixes
        ''' registered in M1M2Tests.RunAll.</summary>
        Friend Function ShouldSkip(testName As String) As Boolean
            EnsureProbed()
            If _nvidiaAvailable Then Return False
            Return testName.StartsWith("M1-", StringComparison.Ordinal) OrElse
                   testName.StartsWith("M2-", StringComparison.Ordinal)
        End Function

        Friend ReadOnly Property SkipReason As String
            Get
                Return _reason
            End Get
        End Property

        ''' <summary>Minimal IVideoBackendContext for the preflight backend —
        ''' same shape the M1/M2 suites use.</summary>
        Private NotInheritable Class PreflightContext
            Implements IVideoBackendContext

            Private ReadOnly _log As EngineLogger

            Public Sub New()
                _log = New EngineLogger("nvidia-preflight", EngineLogger.LogLevel.Warning)
            End Sub

            Public ReadOnly Property Logger As EngineLogger Implements IVideoBackendContext.Logger
                Get
                    Return _log
                End Get
            End Property

            Public ReadOnly Property BackendKind As VideoBackendKind Implements IVideoBackendContext.BackendKind
                Get
                    Return VideoBackendKind.Ddagrab
                End Get
            End Property
        End Class

    End Module

End Namespace
