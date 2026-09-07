






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
    Private ReadOnly _storePathOverride As String
    Private ReadOnly _legacyStorePathOverride As String

    
    Private _deviceKeyEncrypted As String = ""
    Private _sessionTokenEncrypted As String = ""

    
    Private _accountId As String = ""
    Private _deviceId As String = ""
    Private _deviceName As String = ""
    Private _displayName As String = ""
    Private _username As String = ""
    Private _profileImage As String = ""
    Private _sessionExpiresAtIso As String = ""

    Private Sub New(Optional storePathOverride As String = Nothing,
                    Optional legacyStorePathOverride As String = Nothing)
        _storePathOverride = storePathOverride
        _legacyStorePathOverride = legacyStorePathOverride
        Load()
    End Sub

    
    Friend Shared Function CreateForTest(storePath As String, legacyStorePath As String) As DulukaAccountStore
        Return New DulukaAccountStore(storePath, legacyStorePath)
    End Function

    

    Public ReadOnly Property HasSession As Boolean
        Get
            Return _sessionTokenEncrypted <> "" AndAlso Decrypt(_sessionTokenEncrypted) <> ""
        End Get
    End Property

    
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

    
    
    
    
    Public ReadOnly Property ProfileImage As String
        Get
            Return _profileImage
        End Get
    End Property

    
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

    
    
    Public Sub SetProfileWithImage(displayName As String, username As String, profileImage As String)
        SyncLock _lock
            _displayName = If(displayName, "")
            _username = If(username, "")
            _profileImage = If(profileImage, "")
            Save()
        End SyncLock
    End Sub

    
    
    Public Function ClearSession() As Boolean
        SyncLock _lock
            Dim hadSession As Boolean = _sessionTokenEncrypted <> "" OrElse _
                _accountId <> "" OrElse _deviceId <> "" OrElse _deviceName <> "" OrElse _
                _displayName <> "" OrElse _username <> "" OrElse _sessionExpiresAtIso <> ""
            _sessionTokenEncrypted = ""
            _accountId = ""
            _deviceId = ""
            _deviceName = ""
            _displayName = ""
            _username = ""
            _profileImage = ""
            _sessionExpiresAtIso = ""
            Save()
            Return hadSession
        End SyncLock
    End Function

    

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

    

    Private Class StoreDto
        
        
        
        <JsonPropertyName("v")>
        Public Property Version As Integer = 2
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
        <JsonPropertyName("profileImage")>
        Public Property ProfileImage As String = ""
        <JsonPropertyName("sessionExpiresAt")>
        Public Property SessionExpiresAt As String = ""
    End Class

    
    
    
    
    
    Private ReadOnly Property StorePath As String
        Get
            If Not String.IsNullOrEmpty(_storePathOverride) Then Return _storePathOverride
            Return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Duluka",
                "NVIDIA ShadowPlay",
                "Account",
                FileName)
        End Get
    End Property

    
    Private ReadOnly Property LegacyStorePath As String
        Get
            If Not String.IsNullOrEmpty(_legacyStorePathOverride) Then Return _legacyStorePathOverride
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
            dto.ProfileImage = _profileImage
            dto.SessionExpiresAt = _sessionExpiresAtIso
            AppLayout.EnsureParentDir(StorePath)
            File.WriteAllText(StorePath, JsonSerializer.Serialize(dto))
        Catch ex As Exception
            Debug.WriteLine($"DulukaAccountStore.Save failed: {ex.GetType().Name}")
        End Try
    End Sub

    Private Sub Load()
        Try
            Dim path As String = StorePath
            If File.Exists(path) Then
                ApplyDto(ReadDto(path))
                Return
            End If

            
            
            
            Dim legacyPath As String = LegacyStorePath
            If Not File.Exists(legacyPath) Then Return
            Dim legacyDto As StoreDto = ReadDto(legacyPath)
            If legacyDto Is Nothing OrElse Not CanDecryptPersistedSecrets(legacyDto) Then Return
            ApplyDto(legacyDto)
            Save()
        Catch ex As Exception
            
            Debug.WriteLine($"DulukaAccountStore.Load failed: {ex.GetType().Name}")
        End Try
    End Sub

    Private Function ReadDto(path As String) As StoreDto
        If String.IsNullOrEmpty(path) OrElse Not File.Exists(path) Then Return Nothing
        Return JsonSerializer.Deserialize(Of StoreDto)(File.ReadAllText(path))
    End Function

    Private Function CanDecryptPersistedSecrets(dto As StoreDto) As Boolean
        If dto Is Nothing Then Return False
        If Not String.IsNullOrEmpty(dto.DeviceKeyEncrypted) AndAlso
           String.IsNullOrEmpty(Decrypt(dto.DeviceKeyEncrypted)) Then
            Return False
        End If
        If Not String.IsNullOrEmpty(dto.SessionTokenEncrypted) AndAlso
           String.IsNullOrEmpty(Decrypt(dto.SessionTokenEncrypted)) Then
            Return False
        End If
        Return True
    End Function

    Private Sub ApplyDto(dto As StoreDto)
        If dto Is Nothing Then Return
        _deviceKeyEncrypted = If(dto.DeviceKeyEncrypted, "")
        _sessionTokenEncrypted = If(dto.SessionTokenEncrypted, "")
        _accountId = If(dto.AccountId, "")
        _deviceId = If(dto.DeviceId, "")
        _deviceName = If(dto.DeviceName, "")
        _displayName = If(dto.DisplayName, "")
        _profileImage = If(dto.ProfileImage, "")
        _username = If(dto.Username, "")
        _sessionExpiresAtIso = If(dto.SessionExpiresAt, "")
    End Sub

End Class
