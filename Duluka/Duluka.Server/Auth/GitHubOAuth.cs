using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Duluka.Server.Data;
using Duluka.Server.Domain;
using Duluka.Server.Security;

namespace Duluka.Server.Auth;

/// <summary>
/// One pending OAuth flow. Server-side state: the browser/client only ever sees
/// the random `State` and the provider authorization URL. Flows are single-use
/// (TryRemove before validation) and expire — a replayed callback cannot mint
/// a second session from the same authorization code.
/// v0 keeps flows in memory: a server restart mid-login simply means the user
/// retries. Persisting flows is not a security requirement.
/// </summary>
public sealed record OAuthFlow(
    string State,
    string CodeVerifier,
    string? SessionTokenHash,   // set for LINK flows (must already hold a session)
    string? DeviceKeyHash,      // set for LOGIN flows (device bound at completion)
    string DeviceName,
    DateTimeOffset ExpiresAt);

public sealed class OAuthFlowStore
{
    private readonly ConcurrentDictionary<string, OAuthFlow> _flows = new();
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(10);

    public OAuthFlow Put(OAuthFlow flow)
    {
        Sweep();
        _flows[flow.State] = flow;
        return flow;
    }

    /// <summary>Single-use consume: removes the flow, returns null on unknown/expired.</summary>
    public OAuthFlow? Consume(string state)
    {
        Sweep();
        if (!_flows.TryRemove(state, out var flow)) return null;
        return flow.ExpiresAt <= DateTimeOffset.UtcNow ? null : flow;
    }

    private void Sweep()
    {
        foreach (var (state, flow) in _flows)
            if (flow.ExpiresAt <= DateTimeOffset.UtcNow)
                _flows.TryRemove(state, out _);
    }
}

public sealed record GitHubIdentity(
    string ProviderKey, string ProviderUserId, string? ProviderEmail, string? DisplayName);

public sealed class GitHubOAuthException(string message) : Exception(message);

/// <summary>
/// GitHub Authorization Code + PKCE client. The HTTP pipeline is injectable so
/// tests can run the full flow against a fake GitHub without network access.
/// The access token is used transiently to fetch /user and then discarded —
/// v0 never persists provider tokens (see CredentialReference note).
/// </summary>
public sealed class GitHubOAuthService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<GitHubOAuthService> logger)
{
    public const string ProviderKey = Domain.ProviderKeys.GitHub;

    private string ClientId => config["GitHub:ClientId"] ?? "";
    private string ClientSecret => config["GitHub:ClientSecret"] ?? "";
    private string RedirectUri => config["GitHub:RedirectUri"] ?? "";

    /// <summary>Build the provider authorization URL. Only the random state +
    /// PKCE challenge travel to the client; the verifier stays server-side.</summary>
    public (string State, string AuthorizationUrl) BuildAuthorizeUrl(OAuthFlow flow)
    {
        var challenge = Secrets.PkceChallenge(flow.CodeVerifier);
        var url = "https://github.com/login/oauth/authorize" +
                  $"?client_id={Uri.EscapeDataString(ClientId)}" +
                  $"&redirect_uri={Uri.EscapeDataString(RedirectUri)}" +
                  "&scope=" + Uri.EscapeDataString("read:user") +
                  "&response_type=code" +
                  "&state=" + Uri.EscapeDataString(flow.State) +
                  "&code_challenge=" + Uri.EscapeDataString(challenge) +
                  "&code_challenge_method=S256";
        return (flow.State, url);
    }

    /// <summary>
    /// Exchange the callback code for identity. Redirect-uri validation happens
    /// here too: GitHub rejects a code exchange whose redirect_uri differs from
    /// the authorize request, and the configured value is the single allow-listed
    /// URI — client-supplied redirect_uri values are never accepted.
    /// </summary>
    public async Task<GitHubIdentity> ExchangeForIdentityAsync(string code, OAuthFlow flow)
    {
        if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
            throw new GitHubOAuthException("github_not_configured");

        var client = httpClientFactory.CreateClient("github");
        var form = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = RedirectUri,
            ["code_verifier"] = flow.CodeVerifier,
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(form),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new GitHubOAuthException("provider_token_exchange_failed");
        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("error", out var err))
        {
            logger.LogWarning("GitHub token exchange rejected: {Error} (code={Code}, verifier={Verifier})",
                err.GetString(), Secrets.Redact(code), Secrets.Redact(flow.CodeVerifier));
            throw new GitHubOAuthException("provider_callback_rejected");
        }
        var accessToken = doc.RootElement.GetProperty("access_token").GetString()
                          ?? throw new GitHubOAuthException("provider_token_exchange_failed");

        return await FetchIdentityAsync(client, accessToken);
    }

    private static async Task<GitHubIdentity> FetchIdentityAsync(HttpClient client, string accessToken)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        req.Headers.UserAgent.ParseAdd("Duluka-Account/0.1");
        using var resp = await client.SendAsync(req);
        var json = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode)
            throw new GitHubOAuthException("provider_identity_fetch_failed");
        using var doc = JsonDocument.Parse(json);
        var id = doc.RootElement.GetProperty("id").GetInt64();
        var login = doc.RootElement.GetProperty("login").GetString() ?? "";
        string? email = doc.RootElement.TryGetProperty("email", out var e) && e.ValueKind == JsonValueKind.String
            ? e.GetString() : null;
        // The transient access token dies with this method — nothing downstream
        // receives it, nothing persists it (v0 credential policy).
        return new GitHubIdentity(ProviderKey, id.ToString(), email, login);
    }
}

/// <summary>
/// Login-or-link provisioning on top of the unique (ProviderKey, ProviderUserId)
/// anchor. No email matching anywhere: an unknown identity creates a NEW account,
/// a known identity logs into ITS account — even under concurrent first-login.
/// </summary>
public sealed class AccountProvisioningService(Database db)
{
    public (AccountProviderLink Link, DulukaAccount Account, bool ExistingAccount) LoginOrLink(
        GitHubIdentity identity, string deviceKeyHash, string deviceName)
    {
        // Device guard BEFORE provisioning: a revoked device key can never
        // re-register silently — the client must generate a new device key
        // through interactive auth. Cross-account reuse is rejected after the
        // account anchor resolves (below): a device key is not a transferable
        // credential between accounts.
        var existingDevice = db.FindDeviceByKeyHash(deviceKeyHash);
        if (existingDevice is not null && existingDevice.RevokedAt is not null)
            throw new InvalidOperationException("device_revoked");

        // Resolve the identity's home FIRST (without persisting anything):
        // the active anchor, or — after an unlink — the most recent link
        // period on its account. Returning home with the SAME device key is
        // the normal unlink → logout → login-again journey and must succeed.
        var latestLink = db.FindLatestLinkByKey(identity.ProviderKey, identity.ProviderUserId);

        // Ownership guard BEFORE the anchor upsert: for a NEVER-SEEN identity
        // the upsert would bootstrap a NEW account — if the device key is
        // already bound, that binding belongs to a DIFFERENT home, so refuse
        // here and persist nothing (a refused login never leaves an orphan
        // account/provider-link behind, same class as the native-register
        // ordering fix). A returning identity skips this guard: its device
        // key typically belongs to its own home.
        if (latestLink is null && existingDevice is not null)
            throw new InvalidOperationException("device_key_in_use");

        // Known identity + device bound to a DIFFERENT account: neither home
        // may claim the other — refuse BEFORE the upsert so no link period is
        // opened by a refused login.
        if (latestLink is not null && existingDevice is not null &&
            existingDevice.AccountId != latestLink.AccountId)
            throw new InvalidOperationException("device_key_in_use");

        var (link, account, existed) = db.UpsertLink(
            identity.ProviderKey, identity.ProviderUserId, identity.ProviderEmail, identity.DisplayName);

        if (existingDevice is not null)
        {
            if (existingDevice.AccountId != account.AccountId)
                throw new InvalidOperationException("device_key_in_use");
            db.TouchDevice(existingDevice.DeviceId);
            return (link, account, existed);
        }

        var device = db.CreateDevice(account.AccountId, deviceName, deviceKeyHash);
        db.TouchDevice(device.DeviceId);
        return (link, account, existed);
    }

    /// <summary>Register a NEW device for an already-authenticated account.</summary>
    public AccountDevice RegisterDevice(string accountId, string deviceName, string deviceKeyHash)
    {
        var existing = db.FindDeviceByKeyHash(deviceKeyHash);
        if (existing is not null)
        {
            if (existing.RevokedAt is not null)
                throw new InvalidOperationException("device_revoked");
            if (existing.AccountId != accountId)
                throw new InvalidOperationException("device_key_in_use");
            return existing; // re-login from a known device
        }
        return db.CreateDevice(accountId, deviceName, deviceKeyHash);
    }

    /// <summary>
    /// Unlink with the last-provider guard: the account must keep at least one
    /// active way in. Unlinking destroys the CredentialReference and revokes
    /// every session that was issued THROUGH this provider link.
    /// </summary>
    public UnlinkResult Unlink(string accountId, string linkId)
    {
        var link = db.GetLink(linkId);
        if (link is null || link.AccountId != accountId)
            return new UnlinkResult(false, "link_not_found", 0);

        if (!string.Equals(link.Status, LinkStatus.Active, StringComparison.Ordinal))
            return new UnlinkResult(false, "link_already_unlinked", 0);

        // Last-way-in guard: with a native credential, the password IS a way in,
        // so the last provider link may be unlinked (the account survives).
        // Without one, the last active provider link is the only way in and is
        // refused — an account must never be locked out silently.
        if (db.ActiveLinkCount(accountId) <= 1 && !db.HasNativeCredential(accountId))
            return new UnlinkResult(false, "last_provider", 0);

        var at = DateTimeOffset.UtcNow;
        db.Unlink(linkId, at);
        var revoked = db.RevokeSessionsForLink(linkId, at, "provider-unlink");
        return new UnlinkResult(true, "ok", revoked);
    }
}

public sealed record UnlinkResult(bool Ok, string Code, int RevokedSessions);

/// <summary>
/// Native (username/password) authentication on top of the SAME account,
/// device and session machinery the provider flow uses — no separate session
/// system. Unknown username and wrong password converge on the SAME generic
/// failure, with a dummy verifier burn on the unknown-username path so the two
/// are not distinguishable by timing (no account enumeration).
/// </summary>
public sealed class NativeAuthService(Database db)
{
    private static readonly string DummyVerifier = Secrets.HashPassword("duluka-dummy-credential-timing-burn");

    /// <summary>Create a fresh account + credential + device. Contract guards:
    /// revoked device keys are dead forever; a device key already bound to ANY
    /// account is rejected — BOTH BEFORE any persistence, so a refused
    /// registration never leaves an orphan account/credential behind.
    /// Duplicate usernames roll back the whole account+credential transaction.</summary>
    public (DulukaAccount Account, AccountDevice Device) Register(
        string username, string password, string deviceKeyHash, string deviceName)
    {
        // Device ownership guards FIRST — before the account/credential
        // transaction even starts. The account does not exist yet, so any
        // existing live device key belongs to a different account by
        // definition: fail here, persist nothing.
        var existingDevice = db.FindDeviceByKeyHash(deviceKeyHash);
        if (existingDevice is not null && existingDevice.RevokedAt is not null)
            throw new InvalidOperationException("device_revoked");
        if (existingDevice is not null)
            throw new InvalidOperationException("device_key_in_use");

        var canonical = UsernamePolicy.Canonicalize(username);
        var display = username.Trim();
        var account = db.CreateNativeAccount(canonical, display, Secrets.HashPassword(password));

        var device = db.CreateDevice(account.AccountId, deviceName, deviceKeyHash);
        db.TouchDevice(device.DeviceId);
        return (account, device);
    }

    /// <summary>Username/password login onto the EXISTING account (never
    /// creates one). Device binding follows the provider-flow rules exactly.</summary>
    public (DulukaAccount Account, AccountDevice Device) Login(
        string username, string password, string deviceKeyHash, string deviceName)
    {
        var canonical = UsernamePolicy.Canonicalize(username);
        var credential = db.FindNativeCredentialByUsername(canonical);
        if (credential is null)
        {
            // Burn the same PBKDF2 cost the success path would pay, so response
            // timing does not reveal whether the username exists.
            Secrets.VerifyPassword(password, DummyVerifier);
            throw new InvalidOperationException("invalid_credentials");
        }
        if (!Secrets.VerifyPassword(password, credential.PasswordHash))
            throw new InvalidOperationException("invalid_credentials");

        var account = db.GetAccount(credential.AccountId)
            ?? throw new InvalidOperationException("invalid_credentials");
        if (!string.Equals(account.Status, AccountStatus.Active, StringComparison.Ordinal))
            throw new InvalidOperationException("account_suspended");

        var existingDevice = db.FindDeviceByKeyHash(deviceKeyHash);
        if (existingDevice is not null && existingDevice.RevokedAt is not null)
            throw new InvalidOperationException("device_revoked");

        if (existingDevice is not null)
        {
            if (existingDevice.AccountId != account.AccountId)
                throw new InvalidOperationException("device_key_in_use");
            db.TouchDevice(existingDevice.DeviceId);
            return (account, existingDevice);
        }

        var device = db.CreateDevice(account.AccountId, deviceName, deviceKeyHash);
        db.TouchDevice(device.DeviceId);
        return (account, device);
    }

    /// <summary>Change the password of the authenticated account. The current
    /// password must verify — possession of a session alone is not enough.</summary>
    public void ChangePassword(string accountId, string currentPassword, string newPasswordHash)
    {
        var credential = db.FindNativeCredentialByAccount(accountId)
            ?? throw new InvalidOperationException("native_credential_absent");
        if (!Secrets.VerifyPassword(currentPassword, credential.PasswordHash))
            throw new InvalidOperationException("invalid_credentials");
        db.UpdateNativePassword(accountId, newPasswordHash);
    }

    /// <summary>First-time password for a PROVIDER-ONLY (bootstrapped) account:
    /// the user picks a username and password; the account then has two ways in
    /// and its provider link can finally be unlinked (releases the
    /// last-provider trap). Refuses if a native credential already exists —
    /// those accounts change passwords via ChangePassword instead.</summary>
    public void SetInitialPassword(string accountId, string username, string password)
    {
        if (db.HasNativeCredential(accountId))
            throw new InvalidOperationException("credential_exists");
        db.CreateNativeCredential(accountId,
            UsernamePolicy.Canonicalize(username), username.Trim(),
            Secrets.HashPassword(password));
    }

    /// <summary>Delete the account IRREVERSIBLY (user-initiated cascade). An
    /// account WITH a native credential must present its current password —
    /// possession of a session alone is not enough to destroy a
    /// password-protected identity (the same bar ChangePassword applies). A
    /// provider-only account has no second factor; its live session IS the
    /// proof. Returns the number of sessions that died with the account.</summary>
    public int DeleteAccount(string accountId, string currentPassword)
    {
        var credential = db.FindNativeCredentialByAccount(accountId);
        if (credential is not null)
        {
            if (string.IsNullOrEmpty(currentPassword))
                throw new InvalidOperationException("password_required");
            if (!Secrets.VerifyPassword(currentPassword, credential.PasswordHash))
                throw new InvalidOperationException("invalid_credentials");
        }
        return db.DeleteAccountCascade(accountId);
    }
}
