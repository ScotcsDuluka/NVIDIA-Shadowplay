Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Friend Module Program
    Private _passed As Integer
    Private _failed As Integer
    Private ReadOnly _failures As New List(Of String)()

    Private Sub Run(name As String, test As Action)
        Console.Write($"  {name} ... ")
        Try
            test()
            Console.WriteLine("PASS")
            _passed += 1
        Catch ex As Exception
            Console.WriteLine("FAIL")
            Console.WriteLine("      " & ex.Message)
            _failures.Add(name & ": " & ex.Message)
            _failed += 1
        End Try
    End Sub

    Private Sub Assert(condition As Boolean, message As String)
        If Not condition Then Throw New InvalidOperationException(message)
    End Sub

    Private Function NewRoot() As String
        Return Path.Combine(Path.GetTempPath(), "duluka-client-tests-" & Guid.NewGuid().ToString("N"))
    End Function

    Private Function Protect(value As String) As String
        Return Convert.ToBase64String(ProtectedData.Protect(
            Encoding.UTF8.GetBytes(value), Nothing, DataProtectionScope.CurrentUser))
    End Function

    Private Sub TestIndependentStoresAndRepeatedLogin()
        Dim root = NewRoot()
        Dim a = DulukaAccountStore.CreateForTest(Path.Combine(root, "a.json"), Path.Combine(root, "legacy-a.json"))
        Dim b = DulukaAccountStore.CreateForTest(Path.Combine(root, "b.json"), Path.Combine(root, "legacy-b.json"))
        Dim keyA = a.EnsureDeviceKey()
        Dim keyB = b.EnsureDeviceKey()
        Assert(keyA <> keyB, "independent client stores reused the same device key")

        a.SetSession("duluka_st_a", "account-a", "device-a", "2099-01-01T00:00:00Z", "device-a")
        b.SetSession("duluka_st_b", "account-a", "device-b", "2099-01-01T00:00:00Z", "device-b")
        Assert(a.AccountId = b.AccountId, "same username/password contract resolved different accounts")
        Assert(a.DeviceId <> b.DeviceId, "new login reused Device A")

        Dim bReloaded = DulukaAccountStore.CreateForTest(Path.Combine(root, "b.json"), Path.Combine(root, "legacy-b.json"))
        Assert(bReloaded.EnsureDeviceKey() = keyB, "repeated Client B login minted Device C")
        Assert(bReloaded.DeviceId = "device-b", "repeated Client B login lost Device B")
    End Sub

    Private Sub TestLegacyMigration()
        Dim root = NewRoot()
        Directory.CreateDirectory(root)
        Dim legacy = Path.Combine(root, "legacy.json")
        Dim destination = Path.Combine(root, "new.json")
        Dim json = "{" &
                   """v"":1," &
                   """deviceKeyEncrypted"":""" & Protect("legacy-key") & """," &
                   """sessionTokenEncrypted"":""" & Protect("legacy-session") & """," &
                   """accountId"":""account-a""," &
                   """deviceId"":""device-a""}"
        File.WriteAllText(legacy, json)
        Dim migrated = DulukaAccountStore.CreateForTest(destination, legacy)
        Assert(migrated.EnsureDeviceKey() = "legacy-key", "decryptable legacy key was not migrated")
        Assert(migrated.SessionToken = "legacy-session", "decryptable legacy session was not migrated")
        Assert(File.Exists(destination), "migrated store was not written")

        Dim badLegacy = Path.Combine(root, "bad-legacy.json")
        Dim badDestination = Path.Combine(root, "bad-new.json")
        File.WriteAllText(badLegacy, "{""deviceKeyEncrypted"":""not-dpapi""}")
        Dim fresh = DulukaAccountStore.CreateForTest(badDestination, badLegacy)
        Dim freshKey = fresh.EnsureDeviceKey()
        Assert(freshKey <> "" AndAlso freshKey <> "not-dpapi",
               "undecryptable legacy store was copied instead of replaced")
    End Sub

    Private Sub TestSessionPersistenceAnd401Cleanup()
        Dim root = NewRoot()
        Dim sessionPath As String = System.IO.Path.Combine(root, "session.json")
        Dim first = DulukaAccountStore.CreateForTest(sessionPath, System.IO.Path.Combine(root, "legacy.json"))
        first.SetSession("duluka_st_a", "account-a", "device-a", "2099-01-01T00:00:00Z", "device-a")
        Dim restarted = DulukaAccountStore.CreateForTest(sessionPath, System.IO.Path.Combine(root, "legacy.json"))
        Assert(restarted.HasSession, "session was not restored after reload")
        Assert(restarted.SessionToken = "duluka_st_a", "restored session token mismatch")
        Assert(restarted.ClearSession(), "expired/401 cleanup did not clear the session")
        Assert(Not restarted.HasSession, "expired/401 cleanup left a session")
        Assert(Not restarted.ClearSession(), "expired/401 cleanup was not exactly-once/idempotent")
    End Sub

    Sub Main()
        Console.WriteLine("Overlay.Account.Tests — deterministic client store regressions")
        Run("CLIENT-1 independent stores + repeated Client B login", AddressOf TestIndependentStoresAndRepeatedLogin)
        Run("CLIENT-2 decryptable/undecryptable legacy migration", AddressOf TestLegacyMigration)
        Run("CLIENT-3 session persistence + exactly-once 401 cleanup", AddressOf TestSessionPersistenceAnd401Cleanup)
        Console.WriteLine($"RESULT: {_passed} passed, {_failed} failed")
        If _failures.Count > 0 Then
            For Each failure In _failures
                Console.WriteLine("  FAILED: " & failure)
            Next
        End If
        Environment.ExitCode = If(_failed = 0, 0, 1)
    End Sub
End Module
