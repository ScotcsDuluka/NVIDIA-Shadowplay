Option Strict On
Option Explicit On
Option Infer On

' InMemoryDulukaAccountService.vb — the v0.1 EXECUTABLE SPEC.
'
' A deterministic reference implementation of IDulukaAccountService. It is
' the contract the production service must honour, expressed as working
' code: every rule the contract tests assert lives here exactly once, in
' the smallest form the rule allows.
'
' Concurrency model: one lock around state mutation (single-process
' reference). The production implementation is REQUIRED to provide the
' equivalent serialization for the two race contracts (provider uniqueness,
' If-Match) — typically a unique index + optimistic concurrency at the
' store, not this lock. The race tests drive the seam concurrently and
' assert OUTCOMES (exactly-one-success), which both models must satisfy.

Imports System
Imports System.Collections.Generic

Public NotInheritable Class InMemoryDulukaAccountService
    Implements IDulukaAccountService

    Private Class ProviderIdentity
        Public Provider As String
        Public ProviderUserId As String
        Public Sub New(p As String, u As String)
            Provider = If(p, "").Trim().ToLowerInvariant()
            ProviderUserId = If(u, "").Trim()
        End Sub
        Public Function SameAs(other As ProviderIdentity) As Boolean
            Return Provider = other.Provider AndAlso ProviderUserId = other.ProviderUserId
        End Function
    End Class

    Private Class DeviceRec
        Public DeviceId As String
        Public AccountId As String
        Public Revoked As Boolean
    End Class

    Private Class SessionRec
        Public SessionId As String
        Public AccountId As String
        Public DeviceId As String
        Public ExpiresAt As DateTimeOffset
        Public Revoked As Boolean
    End Class

    Private Class AccountRec
        Public AccountId As String
        Public Providers As New List(Of ProviderIdentity)
        Public SyncVersion As Long = 1
        Public SyncPayload As String = ""
    End Class

    Private ReadOnly _lock As New Object()
    Private ReadOnly _accounts As New Dictionary(Of String, AccountRec)
    Private ReadOnly _providers As New Dictionary(Of String, String)  ' provider|pid → owning accountId
    Private ReadOnly _devices As New Dictionary(Of String, DeviceRec)
    Private ReadOnly _sessions As New Dictionary(Of String, SessionRec)
    Private _accountSeq As Long = 0
    Private _sessionSeq As Long = 0

    Public Sub New()
    End Sub

    ' ── helpers ────────────────────────────────────────────────────────

    Private Function ProviderKey(provider As String, pid As String) As String
        Return (If(provider, "").Trim().ToLowerInvariant() & "|" & If(pid, "").Trim())
    End Function

    ''' <summary>Resolve a session to its live record, applying the full
    ''' expiry/revocation cascade. Precedence: unknown → device_revoked (the
    ''' device is the ROOT CAUSE of a cascade kill) → session_revoked →
    ''' expired. Returns Nothing via `fail` when the session is not usable —
    ''' caller maps to the status contract.</summary>
    Private Function ResolveLiveSession(sessionId As String, now As DateTimeOffset,
                                        ByRef rec As SessionRec, ByRef fail As DulukaResult) As Boolean
        rec = Nothing
        fail = Nothing
        Dim s As SessionRec = Nothing
        If String.IsNullOrEmpty(sessionId) OrElse Not _sessions.TryGetValue(sessionId, s) Then
            fail = DulukaResult.Make(DulukaStatus.Unauthorized, DulukaErrorCodes.UnknownSession)
            Return False
        End If
        Dim d As DeviceRec = Nothing
        If _devices.TryGetValue(s.DeviceId, d) AndAlso d.Revoked Then
            fail = DulukaResult.Make(DulukaStatus.Unauthorized, DulukaErrorCodes.DeviceRevoked)
            Return False
        End If
        If s.Revoked Then
            fail = DulukaResult.Make(DulukaStatus.Unauthorized, DulukaErrorCodes.SessionRevoked)
            Return False
        End If
        If now >= s.ExpiresAt Then
            fail = DulukaResult.Make(DulukaStatus.Unauthorized, DulukaErrorCodes.SessionExpired)
            Return False
        End If
        rec = s
        Return True
    End Function

    ' ── account / provider ─────────────────────────────────────────────

    Public Function CreateOrLinkAccount(provider As String, providerUserId As String,
                                        deviceId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.CreateOrLinkAccount

        If String.IsNullOrWhiteSpace(provider) OrElse String.IsNullOrWhiteSpace(providerUserId) OrElse
           String.IsNullOrWhiteSpace(deviceId) Then
            Return DulukaResult.Make(DulukaStatus.Conflict, "invalid_identity")
        End If

        SyncLock _lock
            Dim key As String = ProviderKey(provider, providerUserId)
            Dim ident As New ProviderIdentity(provider, providerUserId)
            Dim ownerAccountId As String = Nothing

            If _providers.TryGetValue(key, ownerAccountId) Then
                ' SINGLE-ACCOUNT-PER-PROVIDER-IDENTITY invariant: the same
                ' provider identity resolves to the SAME account — never a
                ' second account.
                Return DulukaResult.Make(DulukaStatus.Ok, accountId:=ownerAccountId)
            End If

            _accountSeq += 1L
            Dim accountId As String = $"acc-{_accountSeq:D6}"

            Dim acc As New AccountRec With {.AccountId = accountId}
            acc.Providers.Add(ident)
            _accounts(accountId) = acc
            _providers(key) = accountId

            Dim dev As DeviceRec = Nothing
            If Not _devices.TryGetValue(deviceId, dev) Then
                dev = New DeviceRec With {.DeviceId = deviceId, .AccountId = accountId}
                _devices(deviceId) = dev
            End If

            Return DulukaResult.Make(DulukaStatus.Created, accountId:=accountId, deviceId:=deviceId)
        End SyncLock
    End Function

    ''' <summary>Authenticated link: attach an ADDITIONAL provider identity to
    ''' the session's account (this is how an account grows to N credentials —
    ''' bare CreateOrLinkAccount only ever resolves/creates one identity).</summary>
    Public Function LinkProvider(sessionId As String, provider As String,
                                 providerUserId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.LinkProvider

        SyncLock _lock
            Dim s As SessionRec = Nothing
            Dim fail As DulukaResult = Nothing
            If Not ResolveLiveSession(sessionId, now, s, fail) Then Return fail

            Dim key As String = ProviderKey(provider, providerUserId)
            Dim otherOwner As String = Nothing
            If _providers.TryGetValue(key, otherOwner) Then
                ' The identity already belongs to SOME account. If it is the
                ' caller's own account this is a no-op success; otherwise the
                ' identity is bound elsewhere and v0.1 refuses (no silent
                ' account merging).
                If otherOwner = s.AccountId Then
                    Return DulukaResult.Make(DulukaStatus.Ok, accountId:=otherOwner)
                End If
                Return DulukaResult.Make(DulukaStatus.Conflict,
                                         DulukaErrorCodes.ProviderLinkedToOtherAccount,
                                         accountId:=otherOwner)
            End If

            Dim acc As AccountRec = _accounts(s.AccountId)
            acc.Providers.Add(New ProviderIdentity(provider, providerUserId))
            _providers(key) = acc.AccountId
            Return DulukaResult.Make(DulukaStatus.Ok, accountId:=acc.AccountId)
        End SyncLock
    End Function

    Public Function UnlinkProvider(sessionId As String, provider As String,
                                   providerUserId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.UnlinkProvider

        SyncLock _lock
            Dim s As SessionRec = Nothing
            Dim fail As DulukaResult = Nothing
            If Not ResolveLiveSession(sessionId, now, s, fail) Then Return fail

            Dim acc As AccountRec = _accounts(s.AccountId)
            Dim victim As ProviderIdentity = Nothing
            Dim idx As Integer = -1
            For i As Integer = 0 To acc.Providers.Count - 1
                If acc.Providers(i).SameAs(New ProviderIdentity(provider, providerUserId)) Then
                    victim = acc.Providers(i)
                    idx = i
                    Exit For
                End If
            Next
            If idx < 0 Then
                Return DulukaResult.Make(DulukaStatus.Conflict, "provider_not_linked")
            End If

            ' LAST-PROVIDER rule: an account must always retain ≥1 credential.
            If acc.Providers.Count = 1 Then
                Return DulukaResult.Make(DulukaStatus.Conflict, DulukaErrorCodes.LastProvider)
            End If

            acc.Providers.RemoveAt(idx)
            _providers.Remove(ProviderKey(victim.Provider, victim.ProviderUserId))
            Return DulukaResult.Make(DulukaStatus.Ok, accountId:=acc.AccountId)
        End SyncLock
    End Function

    Public Function GetProviders(sessionId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.GetProviders

        SyncLock _lock
            Dim s As SessionRec = Nothing
            Dim fail As DulukaResult = Nothing
            If Not ResolveLiveSession(sessionId, now, s, fail) Then Return fail
            Dim acc As AccountRec = _accounts(s.AccountId)
            Dim sb As New Text.StringBuilder()
            For Each p In acc.Providers
                If sb.Length > 0 Then sb.Append(","c)
                sb.Append(p.Provider).Append(":"c).Append(p.ProviderUserId)
            Next
            Return DulukaResult.Make(DulukaStatus.Ok, accountId:=acc.AccountId, payload:=sb.ToString())
        End SyncLock
    End Function

    ' ── sessions ───────────────────────────────────────────────────────

    Public Function CreateSession(deviceId As String, ttlSeconds As Integer,
                                  now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.CreateSession

        SyncLock _lock
            Dim d As DeviceRec = Nothing
            If Not _devices.TryGetValue(deviceId, d) Then
                Return DulukaResult.Make(DulukaStatus.Conflict, "unknown_device")
            End If
            If d.Revoked Then
                Return DulukaResult.Make(DulukaStatus.Unauthorized, DulukaErrorCodes.DeviceRevoked)
            End If
            _sessionSeq += 1L
            Dim sid As String = $"ses-{_sessionSeq:D6}"
            Dim rec As New SessionRec With {
                .SessionId = sid, .AccountId = d.AccountId, .DeviceId = deviceId,
                .ExpiresAt = now.AddSeconds(ttlSeconds)}
            _sessions(sid) = rec
            Return DulukaResult.Make(DulukaStatus.Created, sessionId:=sid,
                                   accountId:=d.AccountId, deviceId:=deviceId)
        End SyncLock
    End Function

    Public Function UseSession(sessionId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.UseSession

        SyncLock _lock
            Dim s As SessionRec = Nothing
            Dim fail As DulukaResult = Nothing
            If Not ResolveLiveSession(sessionId, now, s, fail) Then Return fail
            Return DulukaResult.Make(DulukaStatus.Ok, sessionId:=s.SessionId, accountId:=s.AccountId)
        End SyncLock
    End Function

    Public Function RevokeSession(sessionId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.RevokeSession

        SyncLock _lock
            Dim s As SessionRec = Nothing
            If String.IsNullOrEmpty(sessionId) OrElse Not _sessions.TryGetValue(sessionId, s) Then
                Return DulukaResult.Make(DulukaStatus.Unauthorized, DulukaErrorCodes.UnknownSession)
            End If
            s.Revoked = True
            Return DulukaResult.Make(DulukaStatus.Ok, sessionId:=s.SessionId)
        End SyncLock
    End Function

    ' ── devices ────────────────────────────────────────────────────────

    Public Function RevokeDevice(deviceId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.RevokeDevice

        SyncLock _lock
            Dim d As DeviceRec = Nothing
            If Not _devices.TryGetValue(deviceId, d) Then
                Return DulukaResult.Make(DulukaStatus.Conflict, "unknown_device")
            End If
            d.Revoked = True
            ' CASCADE: every session of this device dies NOW, regardless of TTL.
            For Each s In _sessions.Values
                If s.DeviceId = deviceId Then s.Revoked = True
            Next
            Return DulukaResult.Make(DulukaStatus.Ok, deviceId:=deviceId)
        End SyncLock
    End Function

    ' ── sync (If-Match) ────────────────────────────────────────────────

    Public Function SyncWrite(sessionId As String, ifMatchVersion As Long?,
                              payload As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.SyncWrite

        SyncLock _lock
            Dim s As SessionRec = Nothing
            Dim fail As DulukaResult = Nothing
            If Not ResolveLiveSession(sessionId, now, s, fail) Then Return fail

            Dim acc As AccountRec = _accounts(s.AccountId)
            If Not ifMatchVersion.HasValue Then
                ' v0.1 contract: If-Match is REQUIRED on writes (lost-update
                ' protection is not optional).
                Return DulukaResult.Make(DulukaStatus.PreconditionRequired,
                                       DulukaErrorCodes.MissingIfMatch,
                                       version:=acc.SyncVersion)
            End If
            If ifMatchVersion.Value <> acc.SyncVersion Then
                Return DulukaResult.Make(DulukaStatus.Conflict,
                                       DulukaErrorCodes.StaleVersion,
                                       accountId:=acc.AccountId,
                                       version:=acc.SyncVersion)
            End If

            acc.SyncVersion += 1L
            acc.SyncPayload = payload
            Return DulukaResult.Make(DulukaStatus.Ok, accountId:=acc.AccountId,
                                   version:=acc.SyncVersion, payload:=payload)
        End SyncLock
    End Function

    Public Function SyncRead(sessionId As String, now As DateTimeOffset) As DulukaResult _
        Implements IDulukaAccountService.SyncRead

        SyncLock _lock
            Dim s As SessionRec = Nothing
            Dim fail As DulukaResult = Nothing
            If Not ResolveLiveSession(sessionId, now, s, fail) Then Return fail
            Dim acc As AccountRec = _accounts(s.AccountId)
            Return DulukaResult.Make(DulukaStatus.Ok, accountId:=acc.AccountId,
                                   version:=acc.SyncVersion, payload:=acc.SyncPayload)
        End SyncLock
    End Function

End Class
