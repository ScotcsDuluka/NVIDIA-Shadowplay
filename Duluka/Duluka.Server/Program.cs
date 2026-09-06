using System.Threading.RateLimiting;
using System.Text.Json;
using Duluka.Server.Auth;
using Duluka.Server.Data;
using Duluka.Server.Domain;
using Duluka.Server.Security;

var builder = WebApplication.CreateBuilder(args);

// Secrets come from environment ONLY (DULUKA_GitHub__ClientSecret etc.) —
// appsettings.json carries empty placeholders by design (hard rule:
// no password/token in plain config).
builder.Configuration.AddEnvironmentVariables(prefix: "DULUKA_");

builder.Services.AddSingleton<OAuthFlowStore>();
builder.Services.AddSingleton<GitHubOAuthService>();
builder.Services.AddSingleton<AccountProvisioningService>();
builder.Services.AddSingleton<SessionService>();
builder.Services.AddHttpClient("github");

builder.Services.AddSingleton<Database>(sp =>
{
    var path = builder.Configuration["Database:Path"] ?? "AppData/DulukaAccount.db";
    var db = new Database(path, sp.GetRequiredService<ILogger<Database>>());
    db.Bootstrap();
    return db;
});

// Rate-limit boundary (C/6): auth entry points are the abuse surface — a fixed
// per-IP window on flow starts, a wider window on authenticated API calls.
// Rejections speak the same §7.1 envelope as every other error path.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.OnRejected = static (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(new
        {
            ok = false,
            reqId = Wire.ReqId(context.HttpContext.Request),
            errorCode = WireCodes.ServerRateLimited,
            httpStatus = 429,
            retryable = true,
            conflict = (object?)null,
            message = "Too many requests — retry later.",
        }, ct));
    };
    options.AddPolicy("auth-start", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
    options.AddPolicy("api", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 240, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();
app.UseRateLimiter();

// ─── helpers ────────────────────────────────────────────────────────────────

static string? BearerToken(HttpRequest req)
{
    var auth = req.Headers.Authorization.ToString();
    const string prefix = "Bearer ";
    return auth.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && auth.Length > prefix.Length
        ? auth[prefix.Length..].Trim()
        : null;
}

var sessionService = app.Services.GetRequiredService<SessionService>();
var db = app.Services.GetRequiredService<Database>();

// ─── health ─────────────────────────────────────────────────────────────────

app.MapGet("/healthz", () => Results.Ok(new { status = "live" }));
app.MapGet("/healthz/ready", () =>
{
    // readiness = schema present (Database.Bootstrap ran before endpoints bind)
    return Results.Ok(new { status = "ready", schema = Database.SchemaVersion });
});

// ─── auth: provider flows ───────────────────────────────────────────────────

var flows = app.Services.GetRequiredService<OAuthFlowStore>();
var github = app.Services.GetRequiredService<GitHubOAuthService>();
var provisioning = app.Services.GetRequiredService<AccountProvisioningService>();

app.MapPost("/v1/auth/{provider}/start", async (string provider, HttpRequest req) =>
{
    // NVIDIA (and any unimplemented provider) is RESERVED: no auth flow exists.
    // Hard rule: hardware-derived identity (GPU UUID/serial/driver/fingerprint)
    // is never accepted as an auth identity — that would be a spoofable bypass.
    if (!ProviderKeys.IsImplemented(provider))
        return Wire.Err(req, 501, WireCodes.ProviderReserved,
            $"Provider '{provider}' is reserved but has no authentication flow in this version.");

    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    var deviceName = body.RootElement.TryGetProperty("deviceName", out var dn) ? dn.GetString() : null;
    var deviceKey = body.RootElement.TryGetProperty("deviceKey", out var dk) ? dk.GetString() : null;

    if (string.IsNullOrWhiteSpace(deviceName) || deviceName.Length > 64)
        return Wire.Err(req, 400, "invalid_device_name", "deviceName is required (max 64 chars).");
    if (string.IsNullOrWhiteSpace(deviceKey) || deviceKey.Length < 43)
        return Wire.Err(req, 400, "invalid_device_key", "deviceKey must be a client-generated base64url value of at least 43 chars (256 bits).");

    var state = Secrets.NewToken("duluka_state_");
    var flow = flows.Put(new OAuthFlow(
        state, Secrets.NewPkceVerifier(), SessionTokenHash: null, DeviceKeyHash: null,
        deviceName.Trim(), DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)));

    // The client's device key is delivered with the COMPLETING callback request
    // (kept client-side until then); the flow binds it via the callback body.
    var (_, url) = github.BuildAuthorizeUrl(flow);
    // The state is returned RAW: it is this client's own correlation value and
    // must be echoed on the callback. Redaction (§9.2) applies to LOGS, never
    // to the value the flow itself handed to this client.
    return Wire.Ok(req, new { authorizationUrl = url, state = flow.State, expiresInMinutes = 10 });
}).RequireRateLimiting("auth-start");

app.MapPost("/v1/auth/{provider}/callback", async (string provider, HttpRequest req) =>
{
    if (!ProviderKeys.IsImplemented(provider))
        return Wire.Err(req, 501, WireCodes.ProviderReserved, $"Provider '{provider}' is reserved in this version.");

    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    string get(string name) =>
        body.RootElement.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString() ?? "" : "";

    var code = get("code");
    var state = get("state");
    var deviceKey = get("deviceKey");

    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        return Wire.Err(req, 400, "invalid_callback", "code and state are required.");
    if (string.IsNullOrWhiteSpace(deviceKey) || deviceKey.Length < 43)
        return Wire.Err(req, 400, "invalid_device_key", "deviceKey is required and must be at least 43 base64url chars.");

    var flow = flows.Consume(state);
    if (flow is null)
        return Wire.Err(req, 400, "invalid_state", "Unknown, expired, or already-used state — restart the flow.");

    var deviceKeyHash = Secrets.Sha256Hex(deviceKey);
    try
    {
        var identity = await github.ExchangeForIdentityAsync(code, flow);
        var (link, account, existed) = provisioning.LoginOrLink(identity, deviceKeyHash, flow.DeviceName);
        var device = db.FindDeviceByKeyHash(deviceKeyHash)
                     ?? throw new InvalidOperationException("device_missing_after_provision");
        var (token, session) = sessionService.Create(account.AccountId, device.DeviceId, link.LinkId);
        return Wire.Ok(req, new
        {
            sessionToken = token,
            accountId = account.AccountId,
            deviceId = device.DeviceId,
            existingAccount = existed,
            sessionExpiresAt = session.ExpiresAt,
        });
    }
    catch (GitHubOAuthException ex)
    {
        return Wire.Err(req, 400, ex.Message, "Provider callback rejected.");
    }
    catch (InvalidOperationException ex) when (ex.Message == "device_revoked")
    {
        return Wire.Err(req, 403, WireCodes.PermDeviceRemoved, "This device key was revoked. Generate a new device key and retry.");
    }
    catch (InvalidOperationException ex) when (ex.Message == "device_key_in_use")
    {
        return Wire.Err(req, 409, WireCodes.ConflictLink, "This device key is already bound to another account.");
    }
    catch (Exception ex)
    {
        // §5.4-2: every failure path converges to a well-formed terminal
        // envelope — never a bare 500 with an infrastructure body.
        app.Logger.LogError(ex, "Unhandled auth callback failure");
        return Wire.Err(req, 500, WireCodes.ServerInternal, "Internal error — retry the request with the same id.");
    }
}).RequireRateLimiting("auth-start");

// ─── auth: link flows for an authenticated account ─────────────────────────

app.MapPost("/v1/account/providers", async (HttpRequest req) =>
{
    var token = BearerToken(req);
    if (token is null) return Wire.Err(req, 401, WireCodes.AuthSessionExpired, "Authorization: Bearer <session token> required.");
    var validation = sessionService.Validate(token);
    if (validation is null) return Wire.Err(req, 401, sessionService.DeadSessionCode(token), "Session is expired, revoked, or unknown.");

    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    var provider = body.RootElement.TryGetProperty("provider", out var p) ? p.GetString() : null;
    if (string.IsNullOrWhiteSpace(provider)) return Wire.Err(req, 400, "invalid_provider", "provider is required.");
    if (!ProviderKeys.IsImplemented(provider))
        return Wire.Err(req, 501, WireCodes.ProviderReserved, $"Provider '{provider}' is reserved in this version.");

    var state = Secrets.NewToken("duluka_state_");
    var flow = flows.Put(new OAuthFlow(
        state, Secrets.NewPkceVerifier(),
        SessionTokenHash: Secrets.Sha256Hex(token),   // link flows complete onto THIS account
        DeviceKeyHash: null, DeviceName: "link-flow",
        DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)));
    var (_, url) = github.BuildAuthorizeUrl(flow);
    return Wire.Ok(req, new { authorizationUrl = url, state = flow.State });   // raw state — same rule as login start
}).RequireRateLimiting("auth-start");

app.MapPost("/v1/account/providers/{provider}/complete", async (string provider, HttpRequest req) =>
{
    if (!ProviderKeys.IsImplemented(provider))
        return Wire.Err(req, 501, WireCodes.ProviderReserved, $"Provider '{provider}' is reserved in this version.");

    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    string get(string name) =>
        body.RootElement.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString() ?? "" : "";
    var code = get("code");
    var state = get("state");

    var flow = flows.Consume(state);
    if (flow is null || flow.SessionTokenHash is null)
        return Wire.Err(req, 400, "invalid_state", "Unknown, expired, or non-link state.");
    var validation = db.ValidateSession(flow.SessionTokenHash);
    if (validation is null)
        return Wire.Err(req, 401, WireCodes.AuthSessionExpired, "The session that started this link flow is gone.");

    var accountId = validation.Value.Session.AccountId;
    try
    {
        var identity = await github.ExchangeForIdentityAsync(code, flow);
        var existing = db.FindActiveLink(identity.ProviderKey, identity.ProviderUserId);
        if (existing is not null)
            return existing.AccountId == accountId
                ? Wire.Ok(req, new { linked = true, linkId = existing.LinkId, already = true })
                : Wire.Err(req, 409, WireCodes.ConflictLink,
                    "This provider identity is already linked to another account. Unlink it there first.");
        var link = db.CreateLink(accountId, identity.ProviderKey, identity.ProviderUserId,
            identity.ProviderEmail, identity.DisplayName);
        if (!string.Equals(link.AccountId, accountId, StringComparison.Ordinal))
            // Lost the unique-anchor race — the surviving row belongs to
            // another account; never report a foreign link as ours (§6.2:
            // deterministic 409, no silent merge).
            return Wire.Err(req, 409, WireCodes.ConflictLink,
                "This provider identity is already linked to another account. Unlink it there first.");
        return Wire.Ok(req, new { linked = true, linkId = link.LinkId, already = false });
    }
    catch (GitHubOAuthException ex)
    {
        return Wire.Err(req, 400, ex.Message, "Provider callback rejected.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled link completion failure");
        return Wire.Err(req, 500, WireCodes.ServerInternal, "Internal error — retry the request with the same id.");
    }
}).RequireRateLimiting("auth-start");

// ─── auth: session lifecycle ────────────────────────────────────────────────

app.MapPost("/v1/auth/session/refresh", (HttpRequest req) =>
{
    var token = BearerToken(req);
    if (token is null) return Wire.Err(req, 401, WireCodes.AuthSessionExpired, "Authorization: Bearer <session token> required.");
    var expires = sessionService.Refresh(token);
    return expires is null
        ? Wire.Err(req, 401, sessionService.DeadSessionCode(token), "Session is expired, revoked, or unknown.")
        : Wire.Ok(req, new { sessionExpiresAt = expires });
});

app.MapPost("/v1/auth/session/revoke", (HttpRequest req) =>
{
    var token = BearerToken(req);
    if (token is null) return Wire.Err(req, 401, WireCodes.AuthSessionExpired, "Authorization: Bearer <session token> required.");
    var revoked = sessionService.Revoke(token, "user-logout");
    return revoked
        ? Wire.Ok(req, new { revoked = true })
        : Wire.Err(req, 401, sessionService.DeadSessionCode(token), "Unknown session.");
});

app.MapPost("/v1/auth/sessions/revoke-all", (HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null)
        return Wire.Err(req, 401, token is null ? WireCodes.AuthSessionExpired : sessionService.DeadSessionCode(token),
            "Session is expired, revoked, or unknown.");
    var count = sessionService.RevokeAll(validation.Value.Item3.AccountId, "logout-all");
    return Wire.Ok(req, new { revokedSessions = count });
});

// ─── account ────────────────────────────────────────────────────────────────

app.MapGet("/v1/account/me", (HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null)
        return Wire.Err(req, 401, token is null ? WireCodes.AuthSessionExpired : sessionService.DeadSessionCode(token),
            "Session is expired, revoked, or unknown.");
    var (_, device, account) = validation.Value;
    return Wire.Ok(req, new
    {
        accountId = account.AccountId,
        displayName = account.DisplayName,
        createdAt = account.CreatedAt,
        currentDevice = new { device.DeviceId, device.DeviceName },
    });
});

app.MapGet("/v1/account/providers", (HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null)
        return Wire.Err(req, 401, token is null ? WireCodes.AuthSessionExpired : sessionService.DeadSessionCode(token),
            "Session is expired, revoked, or unknown.");
    var links = db.LinksForAccount(validation.Value.Item3.AccountId)
        .Select(l => new { l.LinkId, l.ProviderKey, l.ProviderEmail, l.Status, l.LinkedAt });
    return Wire.Ok(req, new { providers = links });
});

app.MapDelete("/v1/account/providers/{linkId}", (string linkId, HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null)
        return Wire.Err(req, 401, token is null ? WireCodes.AuthSessionExpired : sessionService.DeadSessionCode(token),
            "Session is expired, revoked, or unknown.");
    var result = provisioning.Unlink(validation.Value.Item3.AccountId, linkId);
    return result.Ok
        ? Wire.Ok(req, new { unlinked = true, revokedSessions = result.RevokedSessions })
        : result.Code switch
        {
            "last_provider" => Wire.Err(req, 409, WireCodes.ConflictLink,
                "This is the account's last active provider and cannot be unlinked."),
            "link_already_unlinked" => Wire.Err(req, 409, WireCodes.ConflictLink, "This provider link is already unlinked."),
            "link_not_found" => Wire.Err(req, 404, WireCodes.NfLink, "No such provider link for this account."),
            _ => Wire.Err(req, 500, WireCodes.ServerInternal, "Unlink failed."),
        };
});

// ─── devices ────────────────────────────────────────────────────────────────

app.MapGet("/v1/account/devices", (HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null)
        return Wire.Err(req, 401, token is null ? WireCodes.AuthSessionExpired : sessionService.DeadSessionCode(token),
            "Session is expired, revoked, or unknown.");
    var devices = db.DevicesForAccount(validation.Value.Item3.AccountId)
        .Select(d => new { d.DeviceId, d.DeviceName, d.CreatedAt, d.LastSeenAt, d.RevokedAt });
    return Wire.Ok(req, new { devices });
});

app.MapPost("/v1/account/devices/{deviceId}/revoke", (string deviceId, HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null)
        return Wire.Err(req, 401, token is null ? WireCodes.AuthSessionExpired : sessionService.DeadSessionCode(token),
            "Session is expired, revoked, or unknown.");
    var (_, device, account) = validation.Value;
    var target = db.GetDevice(deviceId);
    if (target is null || target.AccountId != account.AccountId)
        return Wire.Err(req, 404, WireCodes.NfDevice, "No such device for this account.");
    var revoked = db.RevokeDevice(deviceId, DateTimeOffset.UtcNow);
    return Wire.Ok(req, new { revoked = true, sessionsRevoked = revoked });
});

app.Run();

/// <summary>§7.1 wire envelope: every response carries ok + reqId echo
/// (client-issued X-ReqId, verbatim); errors add errorCode/httpStatus/
/// retryable per §7.2, successes nest the payload under `resource`.</summary>
internal static class Wire
{
    public static IResult Err(HttpRequest req, int status, string code, string message) =>
        Results.Json(new
        {
            ok = false,
            reqId = ReqId(req),
            errorCode = code,
            httpStatus = status,
            retryable = status >= 500 || status == 429,
            conflict = (object?)null,
            message,
        }, statusCode: status);

    public static IResult Ok(HttpRequest req, object resource) =>
        Results.Json(new { ok = true, reqId = ReqId(req), resource });

    public static string? ReqId(HttpRequest req)
    {
        if (!req.Headers.TryGetValue("X-ReqId", out var values) || values.Count == 0) return null;
        var v = values[0]?.Trim();
        return string.IsNullOrEmpty(v) ? null : v;
    }
}

/// <summary>§7.2 registry codes actually emitted on the wire. Codes outside
/// the frozen registry (provider_*, invalid_*) are documented extensions —
/// clients fall back per HTTP class (§7.2 unknown-code rule).</summary>
internal static class WireCodes
{
    public const string AuthSessionExpired = "auth.session_expired";
    public const string AuthSessionRevoked = "auth.session_revoked";
    public const string PermDeviceRemoved = "perm.device_removed";
    public const string NfLink = "nf.link";
    public const string NfDevice = "nf.device";
    public const string ConflictLink = "conflict.link_conflict";
    public const string ServerInternal = "server.internal";
    public const string ServerRateLimited = "server.rate_limited";
    public const string ProviderReserved = "provider_reserved";
}
