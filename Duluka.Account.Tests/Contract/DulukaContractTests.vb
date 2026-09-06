Option Strict On
Option Explicit On
Option Infer On

' DulukaContractTests.vb — the v0.1 matrix as executable contract tests.
' Every test is deterministic: the clock is INJECTED, nothing sleeps, no
' GPU, no network (the live-HTTP harness lives in DulukaHttpContractProbe).

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks

Friend Module DulukaContractTests

    Private Const Ttl As Integer = 60

    Private ReadOnly T0 As DateTimeOffset = New DateTimeOffset(2026, 9, 7, 0, 0, 0, TimeSpan.Zero)

    Private Function NewService() As InMemoryDulukaAccountService
        Return New InMemoryDulukaAccountService()
    End Function

    ''' <summary>Standard fixture: one account (gh:alice) on device dev-A with
    ''' a live session. Returns (service, accountId, deviceId, sessionId).</summary>
    Private Function NewFixture() As Tuple(Of InMemoryDulukaAccountService, String, String, String)
        Dim svc = NewService()
        Dim created = svc.CreateOrLinkAccount("github", "alice", "dev-A", T0)
        Dim ses = svc.CreateSession("dev-A", Ttl, T0)
        Return Tuple.Create(svc, created.AccountId, "dev-A", ses.SessionId)
    End Function

    Public Sub RunAll()
        Console.WriteLine(" ──── ACC: account creation / provider identity ────")

        TestRunner.RunTest("ACC-1 create → 201 + accountId", Sub()
            Dim svc = NewService()
            Dim r = svc.CreateOrLinkAccount("github", "alice", "dev-A", T0)
            TestRunner.Assert(r.Status = DulukaStatus.Created, "expected 201")
            TestRunner.Assert(Not String.IsNullOrEmpty(r.AccountId), "accountId required")
        End Sub)

        TestRunner.RunTest("ACC-2 SAME provider identity → SAME account, never 2 accounts", Sub()
            Dim svc = NewService()
            Dim r1 = svc.CreateOrLinkAccount("github", "alice", "dev-A", T0)
            Dim r2 = svc.CreateOrLinkAccount("github", "alice", "dev-B", T0)   ' different device, same identity
            Dim r3 = svc.CreateOrLinkAccount("GitHub", " alice ", "dev-C", T0) ' case/space variant → SAME identity
            TestRunner.Assert(r1.Status = DulukaStatus.Created, "first create must be 201")
            TestRunner.Assert(r2.Status = DulukaStatus.Ok AndAlso r2.AccountId = r1.AccountId,
                $"same identity resolved to a DIFFERENT account ({r2.AccountId} vs {r1.AccountId})")
            TestRunner.Assert(r3.Status = DulukaStatus.Ok AndAlso r3.AccountId = r1.AccountId,
                "identity normalization broken (case/whitespace created a new account)")
        End Sub)

        TestRunner.RunTest("ACC-3 same PROVIDER different user → different account", Sub()
            Dim svc = NewService()
            Dim a = svc.CreateOrLinkAccount("github", "alice", "dev-A", T0)
            Dim b = svc.CreateOrLinkAccount("github", "bob", "dev-B", T0)
            TestRunner.Assert(a.AccountId <> b.AccountId, "distinct users collapsed into one account")
        End Sub)

        Console.WriteLine(" ──── UNL: unlink / last-provider ────")

        TestRunner.RunTest("UNL-1 unlink one of two providers → 200, remaining survives", Sub()
            Dim f = NewFixture()
            Dim linked = f.Item1.LinkProvider(f.Item4, "gitlab", "alice", T0)
            TestRunner.Assert(linked.Status = DulukaStatus.Ok, "fixture: second provider link failed")
            Dim r = f.Item1.UnlinkProvider(f.Item4, "gitlab", "alice", T0)
            TestRunner.Assert(r.Status = DulukaStatus.Ok, "expected 200, got " & r.Status)
            Dim left = f.Item1.GetProviders(f.Item4, T0)
            TestRunner.Assert(left.Payload = "github:alice", "remaining provider list wrong: " & left.Payload)
        End Sub)

        TestRunner.RunTest("UNL-2 LAST provider unlink → 409 last_provider_cannot_unlink", Sub()
            Dim f = NewFixture()
            Dim r = f.Item1.UnlinkProvider(f.Item4, "github", "alice", T0)
            TestRunner.Assert(r.Status = DulukaStatus.Conflict AndAlso
                              r.ErrorCode = DulukaErrorCodes.LastProvider,
                              $"expected 409 last_provider, got {r.Status}/{r.ErrorCode}")
            ' account still usable afterwards
            Dim still = f.Item1.GetProviders(f.Item4, T0)
            TestRunner.Assert(still.Status = DulukaStatus.Ok, "account broken after refused unlink")
        End Sub)

        Console.WriteLine(" ──── SES: expiry / revoke ────")

        TestRunner.RunTest("SES-1a live session use → 200", Sub()
            Dim f = NewFixture()
            Dim r = f.Item1.UseSession(f.Item4, T0.AddSeconds(10))
            TestRunner.Assert(r.Status = DulukaStatus.Ok, "expected 200 within TTL")
        End Sub)

        TestRunner.RunTest("SES-1b expired session → 401 session_expired", Sub()
            Dim f = NewFixture()
            Dim r = f.Item1.UseSession(f.Item4, T0.AddSeconds(Ttl).AddSeconds(1))
            TestRunner.Assert(r.Status = DulukaStatus.Unauthorized AndAlso
                              r.ErrorCode = DulukaErrorCodes.SessionExpired,
                              $"expected 401 session_expired, got {r.Status}/{r.ErrorCode}")
        End Sub)

        TestRunner.RunTest("SES-2a revoked session → 401 session_revoked", Sub()
            Dim f = NewFixture()
            f.Item1.RevokeSession(f.Item4, T0)
            Dim r = f.Item1.UseSession(f.Item4, T0.AddSeconds(1))
            TestRunner.Assert(r.Status = DulukaStatus.Unauthorized AndAlso
                              r.ErrorCode = DulukaErrorCodes.SessionRevoked,
                              $"expected 401 session_revoked, got {r.Status}/{r.ErrorCode}")
        End Sub)

        TestRunner.RunTest("SES-2b revoke twice → idempotent", Sub()
            Dim f = NewFixture()
            TestRunner.Assert(f.Item1.RevokeSession(f.Item4, T0).Status = DulukaStatus.Ok, "first revoke")
            TestRunner.Assert(f.Item1.RevokeSession(f.Item4, T0).Status = DulukaStatus.Ok, "second revoke")
        End Sub)

        TestRunner.RunTest("SES-3 unknown session → 401 unknown_session", Sub()
            Dim svc = NewService()
            Dim r = svc.UseSession("ses-999999", T0)
            TestRunner.Assert(r.Status = DulukaStatus.Unauthorized AndAlso
                              r.ErrorCode = DulukaErrorCodes.UnknownSession,
                              $"expected 401 unknown_session, got {r.Status}/{r.ErrorCode}")
        End Sub)

        Console.WriteLine(" ──── DEV: revoke cascade ────")

        TestRunner.RunTest("DEV-1 device revoke → ALL its sessions 401 device_revoked (cascade)", Sub()
            Dim svc = NewService()
            svc.CreateOrLinkAccount("github", "alice", "dev-A", T0)
            Dim s1 = svc.CreateSession("dev-A", 3600, T0)
            Dim s2 = svc.CreateSession("dev-A", 3600, T0)   ' two live sessions on one device
            Dim other = svc.CreateOrLinkAccount("github", "bob", "dev-B", T0)
            Dim otherSes = svc.CreateSession("dev-B", 3600, T0)

            Dim rd = svc.RevokeDevice("dev-A", T0)
            TestRunner.Assert(rd.Status = DulukaStatus.Ok, "revoke device failed")

            Dim u1 = svc.UseSession(s1.SessionId, T0)
            Dim u2 = svc.UseSession(s2.SessionId, T0)
            TestRunner.Assert(u1.Status = DulukaStatus.Unauthorized AndAlso
                              u1.ErrorCode = DulukaErrorCodes.DeviceRevoked,
                              $"session 1 survived device revoke: {u1.Status}/{u1.ErrorCode}")
            TestRunner.Assert(u2.Status = DulukaStatus.Unauthorized AndAlso
                              u2.ErrorCode = DulukaErrorCodes.DeviceRevoked,
                              "session 2 survived device revoke")
            ' no collateral: other device's session still fine
            TestRunner.Assert(svc.UseSession(otherSes.SessionId, T0).Status = DulukaStatus.Ok,
                "device revoke cascaded into an unrelated device")
            ' new sessions on the revoked device are refused at creation
            Dim fresh = svc.CreateSession("dev-A", 3600, T0)
            TestRunner.Assert(fresh.Status = DulukaStatus.Unauthorized AndAlso
                              fresh.ErrorCode = DulukaErrorCodes.DeviceRevoked,
                              "revoked device minted a new session")
        End Sub)

        Console.WriteLine(" ──── SYN: If-Match ────")

        TestRunner.RunTest("SYN-1 If-Match current → 200, version increments, read-back matches", Sub()
            Dim f = NewFixture()
            Dim w = f.Item1.SyncWrite(f.Item4, 1L, "doc-v2", T0)
            TestRunner.Assert(w.Status = DulukaStatus.Ok AndAlso w.Version.GetValueOrDefault() = 2L,
                $"expected 200 v2, got {w.Status}/{w.Version}")
            Dim rd = f.Item1.SyncRead(f.Item4, T0)
            TestRunner.Assert(rd.Version.GetValueOrDefault() = 2L AndAlso rd.Payload = "doc-v2", "read-back mismatch")
        End Sub)

        TestRunner.RunTest("SYN-2 stale If-Match → 409 stale_version (+ current version revealed)", Sub()
            Dim f = NewFixture()
            f.Item1.SyncWrite(f.Item4, 1L, "doc-v2", T0)          ' current = 2
            Dim stale = f.Item1.SyncWrite(f.Item4, 1L, "lost", T0) ' writer had v1
            TestRunner.Assert(stale.Status = DulukaStatus.Conflict AndAlso
                              stale.ErrorCode = DulukaErrorCodes.StaleVersion AndAlso
                              stale.Version.GetValueOrDefault() = 2L,
                              $"expected 409 stale_version (current=2), got {stale.Status}/{stale.ErrorCode}/{stale.Version}")
            Dim rd = f.Item1.SyncRead(f.Item4, T0)
            TestRunner.Assert(rd.Payload = "doc-v2", "loser must not overwrite the doc")
        End Sub)

        TestRunner.RunTest("SYN-3 missing If-Match → 428 missing_if_match", Sub()
            Dim f = NewFixture()
            Dim r = f.Item1.SyncWrite(f.Item4, Nothing, "doc", T0)
            TestRunner.Assert(r.Status = DulukaStatus.PreconditionRequired AndAlso
                              r.ErrorCode = DulukaErrorCodes.MissingIfMatch,
                              $"expected 428 missing_if_match, got {r.Status}/{r.ErrorCode}")
        End Sub)

        Console.WriteLine(" ──── AUTH: 401/403 contract ────")

        TestRunner.RunTest("AUTH-1 every authenticated surface with an unknown session → 401", Sub()
            Dim svc = NewService()
            TestRunner.Assert(svc.UseSession("nope", T0).Status = DulukaStatus.Unauthorized, "use")
            TestRunner.Assert(svc.GetProviders("nope", T0).Status = DulukaStatus.Unauthorized, "providers")
            TestRunner.Assert(svc.SyncRead("nope", T0).Status = DulukaStatus.Unauthorized, "sync read")
            TestRunner.Assert(svc.SyncWrite("nope", 1L, "x", T0).Status = DulukaStatus.Unauthorized, "sync write")
            TestRunner.Assert(svc.UnlinkProvider("nope", "github", "alice", T0).Status = DulukaStatus.Unauthorized, "unlink")
        End Sub)

        TestRunner.RunTest("AUTH-2 cross-account resource access → refused (no leak, no silent merge)", Sub()
            ' v0.1 seam: session-scoped operations never touch another
            ' account's resources. Three cross-account probes:
            '  a) alice's session cannot SEE bob's providers
            '  b) alice's session cannot UNLINK bob's provider identity
            '  c) linking an identity already owned by bob → 409
            '     provider_linked_to_other_account (no silent merge)
            Dim svc = NewService()
            svc.CreateOrLinkAccount("github", "alice", "dev-A", T0)
            Dim sesA = svc.CreateSession("dev-A", 3600, T0)
            svc.CreateOrLinkAccount("github", "bob", "dev-B", T0)
            svc.LinkProvider(svc.CreateSession("dev-B", 3600, T0).SessionId, "gitlab", "bob", T0)

            Dim seen = svc.GetProviders(sesA.SessionId, T0)
            TestRunner.Assert(seen.Payload.Contains("alice") AndAlso Not seen.Payload.Contains("bob"),
                "session leaked another account's providers: " & seen.Payload)

            Dim crossUnlink = svc.UnlinkProvider(sesA.SessionId, "gitlab", "bob", T0)
            TestRunner.Assert(crossUnlink.Status = DulukaStatus.Conflict AndAlso
                              crossUnlink.ErrorCode = "provider_not_linked",
                              "cross-account unlink must not succeed")

            Dim merge = svc.LinkProvider(sesA.SessionId, "gitlab", "bob", T0)
            TestRunner.Assert(merge.Status = DulukaStatus.Conflict AndAlso
                              merge.ErrorCode = DulukaErrorCodes.ProviderLinkedToOtherAccount,
                              $"identity merge must be 409, got {merge.Status}/{merge.ErrorCode}")
        End Sub)

        TestRunner.RunTest("AUTH-3 revoked-session cleanup is permanent across expiry boundary", Sub()
            Dim f = NewFixture()
            f.Item1.RevokeSession(f.Item4, T0)
            Dim r = f.Item1.UseSession(f.Item4, T0.AddYears(1))
            TestRunner.Assert(r.Status = DulukaStatus.Unauthorized, "revocation must outlive TTL")
        End Sub)

        Console.WriteLine(" ──── RACE: uniqueness / If-Match concurrency ────")

        TestRunner.RunTest("RACE-1 concurrent create, SAME provider identity → exactly ONE account", Sub()
            Dim svc = NewService()
            Dim barrier As New Barrier(8)
            Dim results As DulukaResult() = New DulukaResult(7) {}
            Dim tasks As New List(Of Task)
            For i As Integer = 0 To 7
                Dim idx As Integer = i
                tasks.Add(Task.Run(Sub()
                    Try
                        barrier.SignalAndWait(5000)
                        results(idx) = svc.CreateOrLinkAccount("github", "race-user", "dev-" & idx, T0)
                    Catch
                        results(idx) = DulukaResult.Make(DulukaStatus.Conflict, "barrier_timeout")
                    End Try
                End Sub))
            Next
            Task.WaitAll(tasks.ToArray(), 15000)

            Dim accountIds As New HashSet(Of String)
            For Each r In results
                TestRunner.Assert(r IsNot Nothing AndAlso
                                  (r.Status = DulukaStatus.Created OrElse r.Status = DulukaStatus.Ok),
                                  "racer failed unexpectedly: " & If(r?.ErrorCode, "null"))
                accountIds.Add(r.AccountId)
            Next
            TestRunner.Assert(accountIds.Count = 1,
                $"provider-identity race produced {accountIds.Count} accounts — uniqueness broken")
        End Sub)

        TestRunner.RunTest("RACE-2 concurrent If-Match writes at the same version → exactly ONE winner, rest 409", Sub()
            Dim f = NewFixture()
            Dim barrier As New Barrier(8)
            Dim results As DulukaResult() = New DulukaResult(7) {}
            Dim tasks As New List(Of Task)
            For i As Integer = 0 To 7
                Dim idx As Integer = i
                tasks.Add(Task.Run(Sub()
                    Try
                        barrier.SignalAndWait(5000)
                        results(idx) = f.Item1.SyncWrite(f.Item4, 1L, "writer-" & idx, T0)
                    Catch
                        results(idx) = DulukaResult.Make(DulukaStatus.Conflict, "barrier_timeout")
                    End Try
                End Sub))
            Next
            Task.WaitAll(tasks.ToArray(), 15000)

            Dim winners As Integer = 0
            Dim finalVersion As Long? = Nothing
            For Each r In results
                TestRunner.Assert(r IsNot Nothing, "racer returned nothing")
                If r.Status = DulukaStatus.Ok Then
                    winners += 1
                    finalVersion = r.Version
                Else
                    TestRunner.Assert(r.Status = DulukaStatus.Conflict AndAlso
                                      r.ErrorCode = DulukaErrorCodes.StaleVersion,
                                      "loser must be 409 stale_version, got " & r.Status & "/" & r.ErrorCode)
                End If
            Next
            TestRunner.Assert(winners = 1, $"expected exactly 1 winner, got {winners}")
            TestRunner.Assert(finalVersion.HasValue AndAlso finalVersion.Value = 2L,
                "final version must advance by exactly one")
        End Sub)

    End Sub

End Module
