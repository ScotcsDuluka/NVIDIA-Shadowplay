Option Strict On
Option Explicit On
Option Infer On

' Program.vb — Duluka v0.1 account/sync contract suite.
' Exit code: 0 = green (skips allowed and reported), 1 = failure.

Imports System

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
        Console.WriteLine(" Duluka.Account.Tests — v0.1 contract matrix")
        Console.WriteLine(" (injected clock; in-memory reference; no GPU)")
        Console.WriteLine("==================================================")
        Console.WriteLine()

        DulukaContractTests.RunAll()
        DulukaHttpContractProbe.RunAll(AddressOf TestRunner.RunTest)

        Console.WriteLine()
        Console.WriteLine("--------------------------------------------------")
        Console.WriteLine($" RESULT: {TestRunner._passed} passed, {TestRunner._failed} failed" &
                          $", {DulukaHttpContractProbe.SkippedCount} skipped-group(s)")
        If TestRunner._failures.Count > 0 Then
            For Each f As String In TestRunner._failures
                Console.WriteLine($"   - {f}")
            Next
        End If
        Console.WriteLine("--------------------------------------------------")
        Return If(TestRunner._failed > 0, 1, 0)
    End Function

End Module
