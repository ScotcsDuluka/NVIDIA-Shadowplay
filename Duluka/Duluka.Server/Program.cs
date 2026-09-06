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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
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

static IResult Err(int status, string code, string message) =>
    Results.Json(new { error = new { code, message } }, statusCode: status);

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
        return Err(501, "provider_reserved",
            $"Provider '{provider}' is reserved but has no authentication flow in this version.");

    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    var deviceName = body.RootElement.TryGetProperty("deviceName", out var dn) ? dn.GetString() : null;
    var deviceKey = body.RootElement.TryGetProperty("deviceKey", out var dk) ? dk.GetString() : null;

    if (string.IsNullOrWhiteSpace(deviceName) || deviceName.Length > 64)
        return Err(400, "invalid_device_name", "deviceName is required (max 64 chars).");
    if (string.IsNullOrWhiteSpace(deviceKey) || deviceKey.Length < 43)
        return Err(400, "invalid_device_key", "deviceKey must be a client-generated base64url value of at least 43 chars (256 bits).");

    var state = Secrets.NewToken("duluka_state_");
    var flow = flows.Put(new OAuthFlow(
        state, Secrets.NewPkceVerifier(), SessionTokenHash: null, DeviceKeyHash: null,
        deviceName.Trim(), DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)));

    // The client's device key is delivered with the COMPLETING callback request
    // (kept client-side until then); the flow binds it via the callback body.
    var (_, url) = github.BuildAuthorizeUrl(flow);
    return Results.Ok(new { authorizationUrl = url, state = Secrets.Redact(state), expiresInMinutes = 10 });
}).RequireRateLimiting("auth-start");

app.MapPost("/v1/auth/{provider}/callback", async (string provider, HttpRequest req) =>
{
    if (!ProviderKeys.IsImplemented(provider))
        return Err(501, "provider_reserved", $"Provider '{provider}' is reserved in this version.");

    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    string get(string name) =>
        body.RootElement.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString() ?? "" : "";

    var code = get("code");
    var state = get("state");
    var deviceKey = get("deviceKey");

    if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
        return Err(400, "invalid_callback", "code and state are required.");
    if (string.IsNullOrWhiteSpace(deviceKey) || deviceKey.Length < 43)
        return Err(400, "invalid_device_key", "deviceKey is required and must be at least 43 base64url chars.");

    var flow = flows.Consume(state);
    if (flow is null)
        return Err(400, "invalid_state", "Unknown, expired, or already-used state — restart the flow.");

    var deviceKeyHash = Secrets.Sha256Hex(deviceKey);
    try
    {
        var identity = await github.ExchangeForIdentityAsync(code, flow);
        var (link, account, existed) = provisioning.LoginOrLink(identity, deviceKeyHash, flow.DeviceName);
        var device = db.FindDeviceByKeyHash(deviceKeyHash)
                     ?? throw new InvalidOperationException("device_missing_after_provision");
        var (token, session) = app.Services.GetRequiredService<SessionService>()
            .Create(account.AccountId, device.DeviceId, link.LinkId);
        return Results.Ok(new
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
        return Err(400, ex.Message, "Provider callback rejected.");
    }
    catch (InvalidOperationException ex) when (ex.Message == "device_revoked")
    {
        return Err(403, "device_revoked", "This device key was revoked. Generate a new device key and retry.");
    }
}).RequireRateLimiting("auth-start");

// ─── auth: link flows for an authenticated account ─────────────────────────

app.MapPost("/v1/account/providers", async (HttpRequest req) =>
{
    var token = BearerToken(req);
    if (token is null) return Err(401, "session_missing", "Authorization: Bearer <session token> required.");
    var validation = sessionService.Validate(token);
    if (validation is null) return Err(401, "session_invalid", "Session is expired, revoked, or unknown.");

    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    var provider = body.RootElement.TryGetProperty("provider", out var p) ? p.GetString() : null;
    if (string.IsNullOrWhiteSpace(provider)) return Err(400, "invalid_provider", "provider is required.");
    if (!ProviderKeys.IsImplemented(provider))
        return Err(501, "provider_reserved", $"Provider '{provider}' is reserved in this version.");

    var state = Secrets.NewToken("duluka_state_");
    var flow = flows.Put(new OAuthFlow(
        state, Secrets.NewPkceVerifier(),
        SessionTokenHash: Secrets.Sha256Hex(token),   // link flows complete onto THIS account
        DeviceKeyHash: null, DeviceName: "link-flow",
        DateTimeOffset.UtcNow + TimeSpan.FromMinutes(10)));
    var (_, url) = github.BuildAuthorizeUrl(flow);
    return Results.Ok(new { authorizationUrl = url, state = Secrets.Redact(state) });
}).RequireRateLimiting("auth-start");

app.MapPost("/v1/account/providers/{provider}/complete", async (string provider, HttpRequest req) =>
{
    using var body = await System.Text.Json.JsonDocument.ParseAsync(req.Body);
    string get(string name) =>
        body.RootElement.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String
            ? el.GetString() ?? "" : "";
    var code = get("code");
    var state = get("state");

    var flow = flows.Consume(state);
    if (flow is null || flow.SessionTokenHash is null)
        return Err(400, "invalid_state", "Unknown, expired, or non-link state.");
    var validation = db.ValidateSession(flow.SessionTokenHash);
    if (validation is null) return Err(401, "session_invalid", "The session that started this link flow is gone.");

    try
    {
        var identity = await github.ExchangeForIdentityAsync(code, flow);
        var existing = db.FindActiveLink(identity.ProviderKey, identity.ProviderUserId);
        if (existing is not null)
            return existing.AccountId == validation.Value.Session.AccountId
                ? Results.Ok(new { linked = true, linkId = existing.LinkId, already = true })
                : Err(409, "identity_already_linked",
                    "This provider identity is already linked to another account. Unlink it there first.");
        var link = db.UpsertLink(identity.ProviderKey, identity.ProviderUserId,
            identity.ProviderEmail, identity.DisplayName);
        return Results.Ok(new { linked = true, linkId = link.Link.LinkId, already = false });
    }
    catch (GitHubOAuthException ex)
    {
        return Err(400, ex.Message, "Provider callback rejected.");
    }
}).RequireRateLimiting("auth-start");

// ─── auth: session lifecycle ────────────────────────────────────────────────

app.MapPost("/v1/auth/session/refresh", (HttpRequest req) =>
{
    var token = BearerToken(req);
    if (token is null) return Err(401, "session_missing", "Authorization: Bearer <session token> required.");
    var expires = sessionService.Refresh(token);
    return expires is null
        ? Err(401, "session_invalid", "Session is expired, revoked, or unknown.")
        : Results.Ok(new { sessionExpiresAt = expires });
});

app.MapPost("/v1/auth/session/revoke", (HttpRequest req) =>
{
    var token = BearerToken(req);
    if (token is null) return Err(401, "session_missing", "Authorization: Bearer <session token> required.");
    var revoked = sessionService.Revoke(token, "user-logout");
    return revoked ? Results.Ok(new { revoked = true }) : Err(401, "session_invalid", "Unknown session.");
});

app.MapPost("/v1/auth/sessions/revoke-all", (HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null) return Err(401, "session_invalid", "Session is expired, revoked, or unknown.");
    var count = sessionService.RevokeAll(validation.Value.Item3.AccountId, "logout-all");
    return Results.Ok(new { revokedSessions = count });
});

// ─── account ────────────────────────────────────────────────────────────────

app.MapGet("/v1/account/me", (HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null) return Err(401, "session_invalid", "Session is expired, revoked, or unknown.");
    var (_, device, account) = validation.Value;
    return Results.Ok(new
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
    if (validation is null) return Err(401, "session_invalid", "Session is expired, revoked, or unknown.");
    var links = db.LinksForAccount(validation.Value.Item3.AccountId)
        .Select(l => new { l.LinkId, l.ProviderKey, l.ProviderEmail, l.Status, l.LinkedAt });
    return Results.Ok(new { providers = links });
});

app.MapDelete("/v1/account/providers/{linkId}", (string linkId, HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null) return Err(401, "session_invalid", "Session is expired, revoked, or unknown.");
    var result = provisioning.Unlink(validation.Value.Item3.AccountId, linkId);
    return result.Ok
        ? Results.Ok(new { unlinked = true, revokedSessions = result.RevokedSessions })
        : Err(result.Code switch
        {
            "last_provider" => 409,
            "link_not_found" => 404,
            _ => 400,
        }, result.Code, "Unlink rejected.");
});

// ─── devices ────────────────────────────────────────────────────────────────

app.MapGet("/v1/account/devices", (HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null) return Err(401, "session_invalid", "Session is expired, revoked, or unknown.");
    var devices = db.DevicesForAccount(validation.Value.Item3.AccountId)
        .Select(d => new { d.DeviceId, d.DeviceName, d.CreatedAt, d.LastSeenAt, d.RevokedAt });
    return Results.Ok(new { devices });
});

app.MapPost("/v1/account/devices/{deviceId}/revoke", (string deviceId, HttpRequest req) =>
{
    var token = BearerToken(req);
    var validation = token is null ? null : sessionService.Validate(token);
    if (validation is null) return Err(401, "session_invalid", "Session is expired, revoked, or unknown.");
    var (_, device, account) = validation.Value;
    var target = db.GetDevice(deviceId);
    if (target is null || target.AccountId != account.AccountId)
        return Err(404, "device_not_found", "No such device for this account.");
    var revoked = db.RevokeDevice(deviceId, DateTimeOffset.UtcNow);
    return Results.Ok(new { revoked = true, sessionsRevoked = revoked });
});

app.Run();
