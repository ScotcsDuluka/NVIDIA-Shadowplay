Option Strict On
Option Explicit On
Option Infer On

' Program.vb — PHASE 0 CONFIG TRUTH test runner (no ffmpeg, no hardware,
' no recording). Custom runner matches CaptureEngine.Recording.Tests —
' same exit code convention: 0 = all green, 1 = failure.

Imports System
Imports System.Collections.Generic

Namespace Engine.ConfigTruth.Tests

    Friend Module TestRunner
        Friend _passed As Integer = 0
        Friend _failed As Integer = 0
        Friend ReadOnly _failures As New List(Of String)()

        Friend Sub RunTest(name As String, test As Action)
            Console.Write($"  {name} ... ")
            Try
                test()
                Console.WriteLine("PASS")
                _passed += 1
            Catch ex As Exception
                Console.WriteLine("FAIL")
                Console.WriteLine($"      → {ex.Message}")
                _failures.Add(name & ": " & ex.Message)
                _failed += 1
            End Try
        End Sub

        Friend Sub Assert(cond As Boolean, message As String)
            If Not cond Then Throw New Exception(message)
        End Sub
    End Module

    Friend Module Program

        Function Main(args As String()) As Integer
            Console.WriteLine("==================================================")
            Console.WriteLine(" Engine.ConfigTruth.Tests — PHASE 0 CONFIG TRUTH")
            Console.WriteLine(" CT-4: stale config reload (deterministic)")
            Console.WriteLine("==================================================")
            Console.WriteLine()

            CT4ConfigTruthTests.RunAll()
            VCTVideoWiringTests.RunAll()

            Console.WriteLine()
            Console.WriteLine("--------------------------------------------------")
            Console.WriteLine($" RESULT: {TestRunner._passed} passed, {TestRunner._failed} failed")
            If TestRunner._failures.Count > 0 Then
                Console.WriteLine(" Failures:")
                For Each f As String In TestRunner._failures
                    Console.WriteLine($"   - {f}")
                Next
            End If
            Console.WriteLine("--------------------------------------------------")
            Return If(TestRunner._failed > 0, 1, 0)
        End Function

    End Module

End Namespace
