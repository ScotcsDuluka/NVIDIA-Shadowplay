' DulukaAccount.vb — the client-side Duluka account store.
' F-S2 at-rest rules: the device key and the session token are DPAPI
' (CurrentUser) protected inside duluka_account.json; plain values exist in
' memory only and are never serialized. The device key SURVIVES logout (it is
' the device's long-lived identity — revoked keys are dead forever); the
' session token does not.

Imports System.Diagnostics
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Serialization

Friend Class DulukaAccountStore

    Private Shared ReadOnly _instance As New DulukaAccountStore()
    Public Shared ReadOnly Property Instance As DulukaAccountStore
        Get
            Return _instance
        End Get
    End Property

    Private Const FileName As String = "duluka_account.json"
    Private ReadOnly _lock As New Object()

    ' Persisted, DPAPI-encrypted. Never serialized as plain text.
    Private _deviceKeyEncrypted As String = ""
    Private _sessionTokenEncrypted As String = ""

    ' Persisted, non-secret.
    Private _accountId As String = ""
    Private _deviceId As String = ""
    Private _deviceName As String = ""
    Private _displayName As String = ""
    Private _username As String = ""
    Private _sessionExpiresAtIso As String = ""

    Private Sub New()
        Load()
    End Sub

    ' ── session state ───────────────────────────────────────────────────────

    Public ReadOnly Property HasSession As Boolean
        Get
            Return _sessionTokenEncrypted <> "" AndAlso Decrypt(_sessionTokenEncrypted) <> ""
        End Get
    End Property

    ''' <summary>Plain session token — decrypt-on-read, memory only.</summary>
    Public ReadOnly Property SessionToken As String
        Get
            Return Decrypt(_sessionTokenEncrypted)
        End Get
    End Property

    Public ReadOnly Property AccountId As String
        Get
            Return _accountId
        End Get
    End Property

    Public ReadOnly Property DeviceId As String
        Get
            Return _deviceId
        End Get
    End Property

    Public ReadOnly Property DeviceName As String
        Get
            Return _deviceName
        End Get
    End Property

    Public ReadOnly Property DisplayName As String
        Get
            Return _displayName
        End Get
    End Property

    Public ReadOnly Property Username As String
        Get
            Return _username
        End Get
    End Property

    ''' <summary>Session expiry rendered for the Security page ("" if unknown).</summary>
    Public ReadOnly Property SessionExpiresAtText As String
        Get
            If _sessionExpiresAtIso = "" Then Return ""
            Dim parsed As DateTimeOffset
            If DateTimeOffset.TryParse(_sessionExpiresAtIso, parsed) Then
                Return parsed.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            End If
            Return _sessionExpiresAtIso
        End Get
    End Property

    ''' <summary>Client-generated device key (≥256-bit base64url) — created once
    ' per machine, DPAPI-persisted, never re-generated after revocation (the
    ' server rejects revoked keys; a REVOKED device must generate a NEW key
    ' via RevokeDeviceKey()).</summary>
    Public Function EnsureDeviceKey() As String
        SyncLock _lock
            Dim plain As String = Decrypt(_deviceKeyEncrypted)
            If plain <> "" Then Return plain
            plain = NewToken(48)
            _deviceKeyEncrypted = Encrypt(plain)
            Save()
            Return plain
        End SyncLock
    End Function

    ''' <summary>Drops the local device key (used after the server reports the
    ' key as revoked — 403 perm.device_removed — so the next login mints a
    ' fresh key instead of replaying a dead one).</summary>
    Public Sub RevokeDeviceKey()
        SyncLock _lock
            _deviceKeyEncrypted = ""
            Save()
        End SyncLock
    End Sub

    Public Sub SetSession(sessionToken As String, accountId As String, deviceId As String,
                          expiresAtIso As String, deviceName As String)
        SyncLock _lock
            _sessionTokenEncrypted = Encrypt(sessionToken)
            _accountId = If(accountId, "")
            _deviceId = If(deviceId, "")
            _sessionExpiresAtIso = If(expiresAtIso, "")
            _deviceName = If(deviceName, "")
            Save()
        End SyncLock
    End Sub

    Public Sub SetSessionExpiry(expiresAtIso As String)
        SyncLock _lock
            _sessionExpiresAtIso = If(expiresAtIso, "")
            Save()
        End SyncLock
    End Sub

    Public Sub SetProfile(displayName As String, username As String)
        SyncLock _lock
            _displayName = If(displayName, "")
            _username = If(username, "")
            Save()
        End SyncLock
    End Sub

    ''' <summary>Logout / terminal session handling: clears everything
    ' session-scoped. The device key is intentionally kept.</summary>
    Public Sub ClearSession()
        SyncLock _lock
            _sessionTokenEncrypted = ""
            _accountId = ""
            _deviceId = ""
            _displayName = ""
            _username = ""
            _sessionExpiresAtIso = ""
            Save()
        End SyncLock
    End Sub

    ' ── DPAPI (same discipline as AppSettings.GitHubTokenEncrypted) ─────────

    Private Function Encrypt(plain As String) As String
        If String.IsNullOrEmpty(plain) Then Return ""
        Try
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(plain)
            Dim cipher As Byte() = ProtectedData.Protect(bytes, Nothing, DataProtectionScope.CurrentUser)
            Return Convert.ToBase64String(cipher)
        Catch ex As Exception
            Debug.WriteLine($"DulukaAccountStore.Encrypt failed: {ex.GetType().Name}")
            Return ""
        End Try
    End Function

    Private Function Decrypt(cipherB64 As String) As String
        If String.IsNullOrEmpty(cipherB64) Then Return ""
        Try
            Dim cipher As Byte() = Convert.FromBase64String(cipherB64)
            Dim plain As Byte() = ProtectedData.Unprotect(cipher, Nothing, DataProtectionScope.CurrentUser)
            Return Encoding.UTF8.GetString(plain)
        Catch ex As Exception
            ' Failed decrypt = treat as missing (wrong user profile / corrupted
            ' store). Never throw into the UI from a property getter.
            Return ""
        End Try
    End Function

    Private Function NewToken(byteLength As Integer) As String
        Dim bytes(byteLength - 1) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(bytes)
        End Using
        Return Base64UrlEncode(bytes)
    End Function

    Private Function Base64UrlEncode(bytes As Byte()) As String
        Dim base64 As String = Convert.ToBase64String(bytes)
        Return base64.Replace("+", "-").Replace("/", "_").Replace("=", "")
    End Function

    ' ── file shape ──────────────────────────────────────────────────────────

    Private Class StoreDto
        ' NB: System.Text.Json serializes PROPERTIES only — public FIELDS are
        ' silently skipped (the file came out as "{}" and nothing persisted
        ' across restarts). Every member below must stay a Property.
        <JsonPropertyName("v")>
        Public Property Version As Integer = 1
        <JsonPropertyName("deviceKeyEncrypted")>
        Public Property DeviceKeyEncrypted As String = ""
        <JsonPropertyName("sessionTokenEncrypted")>
        Public Property SessionTokenEncrypted As String = ""
        <JsonPropertyName("accountId")>
        Public Property AccountId As String = ""
        <JsonPropertyName("deviceId")>
        Public Property DeviceId As String = ""
        <JsonPropertyName("deviceName")>
        Public Property DeviceName As String = ""
        <JsonPropertyName("displayName")>
        Public Property DisplayName As String = ""
        <JsonPropertyName("username")>
        Public Property Username As String = ""
        <JsonPropertyName("sessionExpiresAt")>
        Public Property SessionExpiresAt As String = ""
    End Class

    Private ReadOnly Property StorePath As String
        Get
            Return AppLayout.P(FileName)
        End Get
    End Property

    Private Sub Save()
        Try
            Dim dto As New StoreDto()
            dto.DeviceKeyEncrypted = _deviceKeyEncrypted
            dto.SessionTokenEncrypted = _sessionTokenEncrypted
            dto.AccountId = _accountId
            dto.DeviceId = _deviceId
            dto.DeviceName = _deviceName
            dto.DisplayName = _displayName
            dto.Username = _username
            dto.SessionExpiresAt = _sessionExpiresAtIso
            File.WriteAllText(StorePath, JsonSerializer.Serialize(dto))
        Catch ex As Exception
            Debug.WriteLine($"DulukaAccountStore.Save failed: {ex.GetType().Name}")
        End Try
    End Sub

    Private Sub Load()
        Try
            Dim path As String = StorePath
            If Not File.Exists(path) Then Return
            Dim dto As StoreDto = JsonSerializer.Deserialize(Of StoreDto)(File.ReadAllText(path))
            If dto Is Nothing Then Return
            _deviceKeyEncrypted = If(dto.DeviceKeyEncrypted, "")
            _sessionTokenEncrypted = If(dto.SessionTokenEncrypted, "")
            _accountId = If(dto.AccountId, "")
            _deviceId = If(dto.DeviceId, "")
            _deviceName = If(dto.DeviceName, "")
            _displayName = If(dto.DisplayName, "")
            _username = If(dto.Username, "")
            _sessionExpiresAtIso = If(dto.SessionExpiresAt, "")
        Catch ex As Exception
            ' Corrupt store = start clean; the next login re-provisions.
            Debug.WriteLine($"DulukaAccountStore.Load failed: {ex.GetType().Name}")
        End Try
    End Sub

End Class
