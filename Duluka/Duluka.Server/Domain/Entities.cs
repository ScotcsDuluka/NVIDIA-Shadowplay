namespace Duluka.Server.Domain;

/// <summary>
/// C/4 domain model, implementation slice. The Account is the identity/sync root;
/// providers are connectors and NEVER own the account. ProviderUserId is unique
/// within (ProviderKey) — that unique anchor is the only duplicate-account defense.
/// Email/login values are DISPLAY-ONLY and are never used for identity matching.
/// </summary>
public static class ProviderKeys
{
    public const string GitHub = "github";

    /// <summary>
    /// Reserved provider. v0 has NO auth flow for NVIDIA — any attempt to start a
    /// login/link flow for this key must be rejected with provider_reserved.
    ///
    /// HARD RULE (C/6): an NVIDIA auth identity must NEVER be derived from
    /// GPU UUID, GPU serial, driver version, machine fingerprint or any other
    /// hardware-derived value. Such an identity is 100% spoofable by a local
    /// process and would be an authentication bypass disguised as a feature.
    /// NVIDIA login becomes real only when an NVIDIA-hosted OAuth/OIDC flow exists.
    /// </summary>
    public const string Nvidia = "nvidia";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { GitHub, Nvidia };

    public static bool IsImplemented(string key) =>
        string.Equals(key, GitHub, StringComparison.OrdinalIgnoreCase);
}

public static class AccountStatus
{
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Closed = "Closed";
}

public static class LinkStatus
{
    public const string Active = "Active";
    public const string Unlinked = "Unlinked";
}

public record DulukaAccount(
    string AccountId,
    string Status,
    string? DisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record AccountProviderLink(
    string LinkId,
    string AccountId,
    string ProviderKey,
    string ProviderUserId,
    string? ProviderEmail,
    string Status,
    DateTimeOffset LinkedAt,
    DateTimeOffset? UnlinkedAt);

public record AccountDevice(
    string DeviceId,
    string AccountId,
    string DeviceName,
    string DeviceKeyHash,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSeenAt,
    DateTimeOffset? RevokedAt);

public record AccountSession(
    string SessionId,
    string SessionTokenHash,
    string AccountId,
    string DeviceId,
    string? IssuedViaLinkId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset LastSeenAt,
    DateTimeOffset? RevokedAt,
    string? RevokedReason);

/// <summary>Row exists for schema completeness; v0 writes nothing here because
/// provider tokens are used transiently and discarded (blast-radius minimization).</summary>
public record CredentialReference(
    string LinkId,
    string ProviderKey,
    string ProviderUserId,
    string? AccessTokenRef,
    string? RefreshTokenRef,
    string? Scopes,
    DateTimeOffset ObtainedAt,
    DateTimeOffset? ExpiresAt);

/// <summary>Identity as reported by the provider. Contains NO secrets — the
/// transient access token travels in a separate channel and is discarded.</summary>
public record ProviderIdentity(string ProviderKey, string ProviderUserId, string? ProviderEmail, string? DisplayName);

/// <summary>
/// Native Duluka credential — the account's FIRST-CLASS username/password way
/// in. 1:1 with an account (AccountId is the primary key). Only the slow-hash
/// verifier is stored; the plaintext password never reaches this layer.
/// UsernameCanonical is the uniqueness anchor (DB-enforced); UsernameDisplay
/// keeps the case the user chose and is display-only.
/// </summary>
public record NativeCredential(
    string AccountId,
    string UsernameCanonical,
    string UsernameDisplay,
    string PasswordHash,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Deterministic username rules. Canonical form = trimmed + lowercased, so
/// Alice / alice / ALICE collapse to one identity and cannot collide.
/// Charset is deliberately narrow (letters, digits, dot, underscore, hyphen).
/// </summary>
public static class UsernamePolicy
{
    public const int MinLength = 3;
    public const int MaxLength = 32;
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 128;

    private static readonly System.Text.RegularExpressions.Regex Pattern =
        new(@"^[A-Za-z0-9._-]{3,32}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public static bool IsValidFormat(string? username) =>
        !string.IsNullOrWhiteSpace(username) && Pattern.IsMatch(username.Trim());

    public static string Canonicalize(string username) => username.Trim().ToLowerInvariant();

    public static bool IsValidPassword(string? password) =>
        !string.IsNullOrEmpty(password)
        && password.Length >= MinPasswordLength
        && password.Length <= MaxPasswordLength;
}
