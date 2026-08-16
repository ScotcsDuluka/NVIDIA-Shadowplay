Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports CaptureEngine.Configuration
Imports CaptureEngine.Diagnostics
Imports CaptureEngine.Engine
Imports CaptureEngineClass = CaptureEngine.Engine.CaptureEngine

Namespace CaptureEngine.Tests
    ''' <summary>
    ''' Minimal console test runner for the CaptureEngine Foundation (Task 001).
    '''
    ''' Deliberately avoids xUnit / NUnit / MSTest — the task forbids
    ''' pulling in dependencies that are not strictly necessary for Phase 0.
    '''
    ''' Exit code: 0 = all tests passed, 1 = at least one failure.
    ''' </summary>
    Friend Module Program
        Private _passed As Integer = 0
        Private _failed As Integer = 0
        Private ReadOnly _failures As New List(Of String)()

        Function Main(args As String()) As Integer
            Console.WriteLine("==================================================")
            Console.WriteLine(" CaptureEngine Foundation - Test Runner")
            Console.WriteLine("==================================================")
            Console.WriteLine()

            ' ----- Positive cases -----
            RunTest("Initialize -> Stopped", AddressOf Test_Initialize)
            RunTest("Start -> Running", AddressOf Test_Start)
            RunTest("Running (stable)", AddressOf Test_Running)
            RunTest("Stop -> Stopped", AddressOf Test_Stop)
            RunTest("Stopped (stable)", AddressOf Test_Stopped)
            RunTest("Dispose -> Disposed", AddressOf Test_Dispose)

            ' ----- Negative cases -----
            RunTest("Start before Initialize (negative)", AddressOf Test_StartBeforeInitialize)
            RunTest("Start twice (negative)", AddressOf Test_StartTwice)
            RunTest("Stop before Start (negative)", AddressOf Test_StopBeforeStart)
            RunTest("Dispose twice (negative)", AddressOf Test_DisposeTwice)

            Console.WriteLine()
            Console.WriteLine("--------------------------------------------------")
            Console.WriteLine(" Result: " & _passed & " passed, " & _failed & " failed, " & (_passed + _failed) & " total")
            Console.WriteLine("--------------------------------------------------")
            If _failed > 0 Then
                Console.WriteLine()
                Console.WriteLine("Failures:")
                For Each f As String In _failures
                    Console.WriteLine("  - " & f)
                Next
            End If
            Return If(_failed > 0, 1, 0)
        End Function

        Private Sub RunTest(name As String, test As Action)
            Console.Write("[" & name & "] ")
            ' Pad to fixed width so PASS/FAIL aligns.
            Dim pad As Integer = Math.Max(0, 50 - name.Length - 2)
            Console.Write(New String(" "c, pad))
            Try
                test()
                _passed += 1
                Console.WriteLine("PASS")
            Catch ex As Exception
                _failed += 1
                _failures.Add(name & ": " & ex.GetType().Name & ": " & ex.Message)
                Console.WriteLine("FAIL")
                Console.WriteLine("        " & ex.GetType().Name & ": " & ex.Message)
            End Try
        End Sub

        Private Sub Assert(condition As Boolean, message As String)
            If Not condition Then
                Throw New InvalidOperationException("Assertion failed: " & message)
            End If
        End Sub

        Private Sub AssertState(engine As CaptureEngineClass, expected As EngineState)
            Dim actual As EngineState = engine.CurrentState
            If actual <> expected Then
                Throw New InvalidOperationException(
                    "State assertion failed: expected '" & expected.ToString() &
                    "', actual '" & actual.ToString() & "'.")
            End If
        End Sub

        ' ===== Positive tests =====

        Private Sub Test_Initialize()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            AssertState(engine, EngineState.Stopped)
            engine.Dispose()
        End Sub

        Private Sub Test_Start()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            engine.Start()
            AssertState(engine, EngineState.Running)
            engine.Dispose()
        End Sub

        Private Sub Test_Running()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            engine.Start()
            AssertState(engine, EngineState.Running)
            ' Stay running briefly to confirm the state is stable, not transient.
            Thread.Sleep(10)
            AssertState(engine, EngineState.Running)
            engine.Dispose()
        End Sub

        Private Sub Test_Stop()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            engine.Start()
            engine.Stop()
            AssertState(engine, EngineState.Stopped)
            engine.Dispose()
        End Sub

        Private Sub Test_Stopped()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            engine.Start()
            engine.Stop()
            ' State must remain Stopped across repeated reads.
            AssertState(engine, EngineState.Stopped)
            AssertState(engine, EngineState.Stopped)
            engine.Dispose()
        End Sub

        Private Sub Test_Dispose()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            engine.Start()
            engine.Stop()
            engine.Dispose()
            AssertState(engine, EngineState.Disposed)
        End Sub

        ' ===== Negative tests =====

        Private Sub Test_StartBeforeInitialize()
            Dim engine As New CaptureEngineClass()
            Dim threw As Boolean = False
            Try
                engine.Start()
            Catch ex As InvalidOperationException
                threw = True
            End Try
            Assert(threw, "Start() before Initialize() must throw InvalidOperationException.")
            AssertState(engine, EngineState.Created)
            engine.Dispose()
        End Sub

        Private Sub Test_StartTwice()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            engine.Start()
            AssertState(engine, EngineState.Running)
            ' Second Start must not throw and must not change state.
            engine.Start()
            AssertState(engine, EngineState.Running)
            ' Third Start too — idempotent contract.
            engine.Start()
            AssertState(engine, EngineState.Running)
            engine.Dispose()
        End Sub

        Private Sub Test_StopBeforeStart()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            ' Stop before Start must NOT throw.
            engine.Stop()
            AssertState(engine, EngineState.Stopped)
            ' And Start should still work afterward (engine recovered cleanly).
            engine.Start()
            AssertState(engine, EngineState.Running)
            engine.Dispose()
        End Sub

        Private Sub Test_DisposeTwice()
            Dim engine As New CaptureEngineClass()
            engine.Initialize(New EngineConfig())
            engine.Start()
            engine.Stop()
            engine.Dispose()
            AssertState(engine, EngineState.Disposed)
            ' Second Dispose must be a no-op, must not throw.
            engine.Dispose()
            AssertState(engine, EngineState.Disposed)
            ' Third too.
            engine.Dispose()
            AssertState(engine, EngineState.Disposed)
        End Sub
    End Module
End Namespace
