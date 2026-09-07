Option Strict On
Option Explicit On
Option Infer On

' DulukaContracts.vb — the v0.1 account/sync CONTRACT as code.
'
' This is the seam the test matrix speaks to. A production implementation
' (HTTP service or local agent) implements IDulukaAccountService; the
' contract tests assert STATUS-CODE SEMANTICS (401/403/409/428 mapped
' 1:1 to HTTP) so the same assertions hold against either the in-memory
' reference adapter (always available, deterministic) or the live service
' (DULUKA_BASE_URL harness).
'
' Identity model (v0.1):
'   Account 1..N ←→ ProviderIdentity(provider, providerUserId)  (≥1 required —
'                  the LAST credential of an account cannot be unlinked)
'   Device  N..1 → Account           (one account, many devices)
'   Session N..1 → Device            (sessions belong to a device; killing
'                                     the device kills every live session)
'   SyncDoc   1..1 → Account         (monotonic integer version; If-Match)

Imports System.Collections.Generic

Public Enum DulukaStatus
    Ok = 200
    Created = 201
    NoContent = 204
    Unauthorized = 401
    Forbidden = 403
    Conflict = 409
    PreconditionRequired = 428
End Enum

''' <summary>Stable machine-readable error codes (never localized, never
''' free-form — the regression suite asserts on these).</summary>
Public NotInheritable Class DulukaErrorCodes
    Public Const ProviderLinkedToOtherAccount As String = "provider_linked_to_other_account"
    Public Const LastProvider As String = "last_provider_cannot_unlink"
    Public Const SessionExpired As String = "session_expired"
    Public Const SessionRevoked As String = "session_revoked"
    Public Const DeviceRevoked As String = "device_revoked"
    Public Const UnknownSession As String = "unknown_session"
    Public Const StaleVersion As String = "stale_version"
    Public Const MissingIfMatch As String = "missing_if_match"
    Public Const CrossAccount As String = "cross_account_forbidden"
    Private Sub New()
    End Sub
End Class

''' <summary>One operation result. Status mirrors the HTTP contract.</summary>
Public NotInheritable Class DulukaResult
    Public Property Status As DulukaStatus
    Public Property ErrorCode As String          ' Nothing on success
    Public Property AccountId As String
    Public Property SessionId As String
    Public Property DeviceId As String
    Public Property Version As Long?             ' sync version AFTER the operation
    Public Property Payload As String            ' sync doc / echoes

    Public Shared Function Make(status As DulukaStatus,
                              Optional err As String = Nothing,
                              Optional accountId As String = Nothing,
                              Optional sessionId As String = Nothing,
                              Optional deviceId As String = Nothing,
                              Optional version As Long? = Nothing,
                              Optional payload As String = Nothing) As DulukaResult
        Return New DulukaResult With {
            .Status = status, .ErrorCode = err, .AccountId = accountId,
            .SessionId = sessionId, .DeviceId = deviceId,
            .Version = version, .Payload = payload}
    End Function
End Class

''' <summary>
''' v0.1 account/sync surface. ALL methods take `now` (UTC) explicitly so
''' expiry semantics are testable without sleeps or timing races.
''' </summary>
Public Interface IDulukaAccountService

    ' ── account / provider ────────────────────────────────────────────
    ' Same (provider, providerUserId) MUST always resolve to the SAME
    ' account (single-account-per-provider-identity invariant).
    Function CreateOrLinkAccount(provider As String, providerUserId As String,
                                 deviceId As String, now As DateTimeOffset) As DulukaResult
    ''' Authenticated: attach an ADDITIONAL provider identity to the
    ''' session account. Refuses identities already bound to another
    ''' account (no silent merging).
    Function LinkProvider(sessionId As String, provider As String,
                          providerUserId As String, now As DateTimeOffset) As DulukaResult
    Function UnlinkProvider(sessionId As String, provider As String,
                            providerUserId As String, now As DateTimeOffset) As DulukaResult
    Function GetProviders(sessionId As String, now As DateTimeOffset) As DulukaResult

    ' ── sessions ──────────────────────────────────────────────────────
    Function CreateSession(deviceId As String, ttlSeconds As Integer, now As DateTimeOffset) As DulukaResult
    Function UseSession(sessionId As String, now As DateTimeOffset) As DulukaResult
    Function RevokeSession(sessionId As String, now As DateTimeOffset) As DulukaResult

    ' ── devices ───────────────────────────────────────────────────────
    Function RevokeDevice(deviceId As String, now As DateTimeOffset) As DulukaResult

    ' ── sync (If-Match) ───────────────────────────────────────────────
    Function SyncWrite(sessionId As String, ifMatchVersion As Long?,
                       payload As String, now As DateTimeOffset) As DulukaResult
    Function SyncRead(sessionId As String, now As DateTimeOffset) As DulukaResult

End Interface
