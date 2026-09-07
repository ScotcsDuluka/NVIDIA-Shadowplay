using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Duluka.Server.Data;
using Duluka.Server.Domain;

namespace Duluka.Server.Admin;

/// <summary>
/// Local operator console. GET /admin serves a single-file dark dashboard;
/// /admin/api/* powers it (overview stats, accounts, sessions, devices,
/// provider links) plus a small set of destructive operator actions.
///
/// Guard model (defence in depth, NOT an auth boundary):
///   1. The server binds loopback only (Urls in appsettings.json) — the
///      console is unreachable from other machines by construction.
///   2. Every /admin/api call must carry the custom X-Admin-Request header.
///      A cross-origin browser request cannot set custom headers without a
///      CORS preflight, and this server has no CORS policy — so a drive-by
///      web page cannot fire state-changing admin calls at 127.0.0.1.
/// The operator must still never expose the port beyond localhost.
/// </summary>
internal static class AdminConsole
{
    private const string GuardHeader = "X-Admin-Request";
    private const string ResourceName = "Duluka.Server.Admin.admin.html";

    private static readonly Regex DataUrlPattern =
        new("^data:image/(png|jpeg|webp);base64,([A-Za-z0-9+/]*={0,2})$", RegexOptions.Compiled);

    public static void Map(WebApplication app, Database db)
    {
        // ── the single-file dashboard ──────────────────────────────────────
        app.MapGet("/admin", (HttpContext ctx) =>
        {
            var asm = Assembly.GetExecutingAssembly();
            using var stream = asm.GetManifestResourceStream(ResourceName);
            if (stream is null)
                return Wire.Err(ctx.Request, 500, WireCodes.ServerInternal, "Admin page resource missing.");
            using var reader = new StreamReader(stream);
            return Results.Text(reader.ReadToEnd(), "text/html; charset=utf-8");
        });

        // ── API: stats + listings ──────────────────────────────────────────
        app.MapGet("/admin/api/overview", (HttpContext ctx) =>
            Guarded(ctx, () => Wire.Ok(ctx.Request, new
            {
                accounts = db.AdminGetOverview().Accounts,
                activeSessions = db.AdminGetOverview().ActiveSessions,
                activeDevices = db.AdminGetOverview().ActiveDevices,
                activeLinks = db.AdminGetOverview().ActiveLinks,
                nativeCredentials = db.AdminGetOverview().NativeCredentials,
                dbPath = db.DbPath,
            })));

        app.MapGet("/admin/api/accounts", (HttpContext ctx) =>
            Guarded(ctx, () => Wire.Ok(ctx.Request, new { accounts = db.AdminListAccounts() })));

        app.MapGet("/admin/api/sessions", (HttpContext ctx) =>
            Guarded(ctx, () => Wire.Ok(ctx.Request, new { sessions = db.AdminListSessions() })));

        app.MapGet("/admin/api/devices", (HttpContext ctx) =>
            Guarded(ctx, () => Wire.Ok(ctx.Request, new { devices = db.AdminListDevices() })));

        app.MapGet("/admin/api/links", (HttpContext ctx) =>
            Guarded(ctx, () => Wire.Ok(ctx.Request, new { links = db.AdminListLinks() })));

        // ── avatar preview: decoded data URL → real image bytes ────────────
        app.MapGet("/admin/api/accounts/{id}/avatar", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                var image = db.GetAccount(id)?.ProfileImage;
                if (image is null) return Results.NotFound();
                var match = DataUrlPattern.Match(image);
                if (!match.Success) return Results.NotFound();
                var mime = "image/" + match.Groups[1].Value;
                byte[] bytes;
                try { bytes = Convert.FromBase64String(match.Groups[2].Value); }
                catch (FormatException) { return Results.NotFound(); }
                return Results.Bytes(bytes, mime);
            }));

        // ── operator actions ───────────────────────────────────────────────
        // Rename: only DisplayName is touched; the profile image is passed
        // through unchanged (UpdateAccountProfile writes both fields).
        app.MapPatch("/admin/api/accounts/{id}/profile", async (string id, HttpContext ctx) =>
            await GuardedAsync(ctx, async () =>
            {
                var account = db.GetAccount(id);
                if (account is null) return Wire.Err(ctx.Request, 404, "nf.account", "No such account.");
                using var body = await Wire.TryParseBodyAsync(ctx.Request);
                if (body is null) return Wire.Err(ctx.Request, 400, WireCodes.BadRequest, "Request body must be valid JSON.");
                var displayName = body.RootElement.TryGetProperty("displayName", out var dn) && dn.ValueKind == JsonValueKind.String
                    ? dn.GetString() : null;
                if (!ProfilePolicy.IsValidDisplayName(displayName))
                    return Wire.Err(ctx.Request, 400, WireCodes.InvalidDisplayName,
                        $"displayName must be at most {ProfilePolicy.MaxDisplayNameLength} characters with no control characters.");
                db.UpdateAccountProfile(id, string.IsNullOrWhiteSpace(displayName) ? null : displayName!.Trim(), account.ProfileImage);
                return Wire.Ok(ctx.Request, new { updated = true });
            }));

        // Clear the avatar only — the display name is passed through.
        app.MapDelete("/admin/api/accounts/{id}/profile/image", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                var account = db.GetAccount(id);
                if (account is null) return Wire.Err(ctx.Request, 404, "nf.account", "No such account.");
                db.UpdateAccountProfile(id, account.DisplayName, null);
                return Wire.Ok(ctx.Request, new { cleared = true });
            }));

        app.MapPost("/admin/api/accounts/{id}/status", async (string id, HttpContext ctx) =>
            await GuardedAsync(ctx, async () =>
            {
                using var body = await Wire.TryParseBodyAsync(ctx.Request);
                if (body is null) return Wire.Err(ctx.Request, 400, WireCodes.BadRequest, "Request body must be valid JSON.");
                var status = body.RootElement.TryGetProperty("status", out var st) && st.ValueKind == JsonValueKind.String
                    ? st.GetString() : null;
                if (status is not (AccountStatus.Active or AccountStatus.Suspended))
                    return Wire.Err(ctx.Request, 400, WireCodes.BadRequest, "status must be 'Active' or 'Suspended'.");
                if (!db.AdminSetAccountStatus(id, status!))
                    return Wire.Err(ctx.Request, 404, "nf.account", "No such account.");
                return Wire.Ok(ctx.Request, new { status });
            }));

        app.MapDelete("/admin/api/accounts/{id}", (string id, HttpContext ctx) =>
            Guarded(ctx, () => db.AdminDeleteAccount(id)
                ? Wire.Ok(ctx.Request, new { deleted = true })
                : Wire.Err(ctx.Request, 404, "nf.account", "No such account.")));

        app.MapDelete("/admin/api/sessions/{id}", (string id, HttpContext ctx) =>
            Guarded(ctx, () => db.AdminRevokeSession(id)
                ? Wire.Ok(ctx.Request, new { revoked = true })
                : Wire.Err(ctx.Request, 404, "nf.session", "No such session (or already revoked).")));

        app.MapPost("/admin/api/devices/{id}/revoke", (string id, HttpContext ctx) =>
            Guarded(ctx, () => db.RevokeDevice(id, DateTimeOffset.UtcNow) > 0
                ? Wire.Ok(ctx.Request, new { revoked = true })
                : Wire.Err(ctx.Request, 404, WireCodes.NfDevice, "No such device (or already revoked).")));

        app.MapDelete("/admin/api/devices/{id}", (string id, HttpContext ctx) =>
            Guarded(ctx, () => db.AdminDeleteDevice(id)
                ? Wire.Ok(ctx.Request, new { deleted = true })
                : Wire.Err(ctx.Request, 404, WireCodes.NfDevice, "No such device.")));

        app.MapPost("/admin/api/links/{id}/unlink", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                var link = db.GetLink(id);
                if (link is null) return Wire.Err(ctx.Request, 404, WireCodes.NfLink, "No such provider link.");
                if (link.Status == LinkStatus.Active)
                {
                    db.Unlink(id, DateTimeOffset.UtcNow);
                    db.RevokeSessionsForLink(id, DateTimeOffset.UtcNow, "admin-unlink");
                }
                return Wire.Ok(ctx.Request, new { unlinked = true });
            }));
    }

    // ── guard plumbing ─────────────────────────────────────────────────────

    private static IResult? Rejection(HttpContext ctx)
    {
        var ip = ctx.Connection.RemoteIpAddress;
        if (ip is null || !IPAddress.IsLoopback(ip))
            return Wire.Err(ctx.Request, 403, "admin.local_only", "The admin console answers loopback requests only.");
        if (!ctx.Request.Headers.TryGetValue(GuardHeader, out var value) || value.ToString() != "1")
            return Wire.Err(ctx.Request, 403, "admin.guard", $"Missing {GuardHeader}: 1 — drive-by browser requests are refused.");
        return null;
    }

    private static IResult Guarded(HttpContext ctx, Func<IResult> handler) =>
        Rejection(ctx) ?? handler();

    private static async Task<IResult> GuardedAsync(HttpContext ctx, Func<Task<IResult>> handler) =>
        Rejection(ctx) ?? await handler();
}
