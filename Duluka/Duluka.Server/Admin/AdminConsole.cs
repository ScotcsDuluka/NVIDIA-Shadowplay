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

    public static void Map(WebApplication app, Database db, AdminLogBuffer logs)
    {
        var log = app.Logger;

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
            Guarded(ctx, () =>
            {
                var overview = db.AdminGetOverview();   // computed ONCE per request
                return Wire.Ok(ctx.Request, new
                {
                    accounts = overview.Accounts,
                    activeSessions = overview.ActiveSessions,
                    activeDevices = overview.ActiveDevices,
                    activeLinks = overview.ActiveLinks,
                    nativeCredentials = overview.NativeCredentials,
                    dbPath = db.DbPath,
                });
            }));

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
                log.LogInformation("[admin] rename account={Id} ip={Ip}", id, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { updated = true });
            }));

        // Clear the avatar only — the display name is passed through.
        app.MapDelete("/admin/api/accounts/{id}/profile/image", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                var account = db.GetAccount(id);
                if (account is null) return Wire.Err(ctx.Request, 404, "nf.account", "No such account.");
                db.UpdateAccountProfile(id, account.DisplayName, null);
                log.LogInformation("[admin] clear-image account={Id} ip={Ip}", id, ctx.Connection.RemoteIpAddress);
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
                log.LogInformation("[admin] status={Status} account={Id} ip={Ip}", status, id, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { status });
            }));

        app.MapDelete("/admin/api/accounts/{id}", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                if (!db.AdminDeleteAccount(id))
                    return Wire.Err(ctx.Request, 404, "nf.account", "No such account.");
                log.LogInformation("[admin] delete-account account={Id} ip={Ip}", id, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { deleted = true });
            }));

        app.MapDelete("/admin/api/sessions/{id}", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                if (!db.AdminRevokeSession(id))
                    return Wire.Err(ctx.Request, 404, "nf.session", "No such session (or already revoked).");
                log.LogInformation("[admin] revoke-session session={Id} ip={Ip}", id, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { revoked = true });
            }));

        app.MapPost("/admin/api/devices/{id}/revoke", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                if (db.RevokeDevice(id, DateTimeOffset.UtcNow) <= 0)
                    return Wire.Err(ctx.Request, 404, WireCodes.NfDevice, "No such device (or already revoked).");
                log.LogInformation("[admin] revoke-device device={Id} ip={Ip}", id, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { revoked = true });
            }));

        app.MapDelete("/admin/api/devices/{id}", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                if (!db.AdminDeleteDevice(id))
                    return Wire.Err(ctx.Request, 404, WireCodes.NfDevice, "No such device.");
                log.LogInformation("[admin] delete-device device={Id} ip={Ip}", id, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { deleted = true });
            }));

        app.MapPost("/admin/api/links/{id}/unlink", (string id, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                var link = db.GetLink(id);
                if (link is null) return Wire.Err(ctx.Request, 404, WireCodes.NfLink, "No such provider link.");
                if (link.Status == LinkStatus.Active)
                {
                    db.Unlink(id, DateTimeOffset.UtcNow);
                    db.RevokeSessionsForLink(id, DateTimeOffset.UtcNow, "admin-unlink");
                    log.LogInformation("[admin] unlink link={Id} ip={Ip}", id, ctx.Connection.RemoteIpAddress);
                }
                return Wire.Ok(ctx.Request, new { unlinked = true });
            }));

        // ── operator console v2: system, logs, activity, export, sql, bulk ──

        // live process/runtime/database facts for the system panel
        app.MapGet("/admin/api/system", (HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                using var proc = System.Diagnostics.Process.GetCurrentProcess();
                ThreadPool.GetMaxThreads(out var maxWorkers, out _);
                ThreadPool.GetAvailableThreads(out var availWorkers, out _);
                var stats = db.AdminGetDbStats();
                return Wire.Ok(ctx.Request, new
                {
                    serverTime = DateTimeOffset.UtcNow.ToString("o"),
                    uptimeSeconds = (DateTimeOffset.UtcNow - proc.StartTime.ToUniversalTime()).TotalSeconds,
                    process = new
                    {
                        id = Environment.ProcessId,
                        bitness = Environment.Is64BitProcess ? "x64" : "x86",
                        processors = Environment.ProcessorCount,
                        threads = proc.Threads.Count,
                        threadPool = new { available = availWorkers, max = maxWorkers },
                        workingSetBytes = proc.WorkingSet64,
                        managedMemoryBytes = GC.GetTotalMemory(forceFullCollection: false),
                        gc = new { gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2) },
                        startTime = proc.StartTime.ToUniversalTime().ToString("o"),
                    },
                    runtime = new
                    {
                        version = Environment.Version.ToString(),
                        framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                        os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                    },
                    database = new
                    {
                        path = db.DbPath,
                        fileBytes = stats.FileBytes,
                        pageCount = stats.PageCount,
                        pageSize = stats.PageSize,
                        freelistPages = stats.FreelistPages,
                        journalMode = stats.JournalMode,
                    },
                });
            }));

        // incremental log tail: ?after=<seq> streams everything newer
        app.MapGet("/admin/api/logs", (long after, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                var (entries, last) = logs.Since(after);
                return Wire.Ok(ctx.Request, new { last, entries });
            }));

        // 14-day daily registration / session-issue counts for the sparklines
        app.MapGet("/admin/api/activity", (HttpContext ctx) =>
            Guarded(ctx, () => Wire.Ok(ctx.Request, new { days = db.AdminActivity(14) })));

        // JSON export download (accounts | sessions | devices | links)
        app.MapGet("/admin/api/export", (string what, HttpContext ctx) =>
            Guarded(ctx, () =>
            {
                if (what is not ("accounts" or "sessions" or "devices" or "links"))
                    return Wire.Err(ctx.Request, 400, WireCodes.BadRequest,
                        "what must be accounts, sessions, devices or links.");
                var rows = what switch
                {
                    "accounts" => (object)db.AdminListAccounts(5000),
                    "sessions" => (object)db.AdminListSessions(20000),
                    "devices" => (object)db.AdminListDevices(20000),
                    _ => (object)db.AdminListLinks(20000),
                };
                var count = (rows as System.Collections.ICollection)?.Count ?? 0;
                var json = JsonSerializer.Serialize(new
                {
                    what,
                    exportedAt = DateTimeOffset.UtcNow.ToString("o"),
                    count,
                    rows,
                }, new JsonSerializerOptions(JsonSerializerDefaults.Web) { WriteIndented = true });
                ctx.Response.Headers.ContentDisposition =
                    $"attachment; filename=\"duluka-{what}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json\"";
                return Results.Text(json, "application/json; charset=utf-8");
            }));

        // read-only ad-hoc SQL (SELECT-only, single statement, row-capped)
        app.MapPost("/admin/api/query", async (HttpContext ctx) =>
        {
            return await GuardedAsync(ctx, async () =>
            {
                using var body = await Wire.TryParseBodyAsync(ctx.Request);
                if (body is null)
                    return Wire.Err(ctx.Request, 400, WireCodes.BadRequest, "Request body must be valid JSON.");
                var sql = body.RootElement.TryGetProperty("sql", out var s) && s.ValueKind == JsonValueKind.String
                    ? s.GetString() : null;
                if (string.IsNullOrWhiteSpace(sql))
                    return Wire.Err(ctx.Request, 400, "admin.query", "Body must include a sql string.");
                try
                {
                    var result = db.AdminRunSelect(sql!);
                    return Wire.Ok(ctx.Request, result);
                }
                catch (ArgumentException ex)
                {
                    return Wire.Err(ctx.Request, 400, "admin.query", ex.Message);
                }
                catch (Microsoft.Data.Sqlite.SqliteException ex)
                {
                    return Wire.Err(ctx.Request, 400, "admin.query", "SQLite: " + ex.Message);
                }
            });
        });

        // bulk revoke — the server ALSO demands the typed confirmation phrase
        app.MapPost("/admin/api/maintenance/revoke-all-sessions", async (HttpContext ctx) =>
        {
            return await GuardedAsync(ctx, async () =>
            {
                using var body = await Wire.TryParseBodyAsync(ctx.Request);
                var confirm = body is not null &&
                              body.RootElement.TryGetProperty("confirm", out var c1) &&
                              c1.ValueKind == JsonValueKind.String ? c1.GetString() : null;
                if (confirm != "REVOKE-ALL")
                    return Wire.Err(ctx.Request, 400, "admin.confirm",
                        "Type REVOKE-ALL to confirm the bulk session revoke.");
                var revoked = db.AdminRevokeAllSessions();
                log.LogInformation("[admin] revoke-ALL sessions count={Count} ip={Ip}", revoked, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { revoked });
            });
        });

        app.MapPost("/admin/api/maintenance/vacuum", async (HttpContext ctx) =>
        {
            return await GuardedAsync(ctx, async () =>
            {
                using var body = await Wire.TryParseBodyAsync(ctx.Request);
                var confirm = body is not null &&
                              body.RootElement.TryGetProperty("confirm", out var c2) &&
                              c2.ValueKind == JsonValueKind.String ? c2.GetString() : null;
                if (confirm != "VACUUM")
                    return Wire.Err(ctx.Request, 400, "admin.confirm",
                        "Type VACUUM to confirm the database rebuild.");
                var before = db.AdminGetDbStats().FileBytes;
                db.AdminVacuum();
                var after = db.AdminGetDbStats().FileBytes;
                log.LogInformation("[admin] vacuum freed={Freed}B ip={Ip}", before - after, ctx.Connection.RemoteIpAddress);
                return Wire.Ok(ctx.Request, new { freedBytes = before - after, afterBytes = after });
            });
        });
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

    private static async Task<IResult> GuardedAsync(HttpContext ctx, Func<Task<IResult>> handler)
    {
        var rejection = Rejection(ctx);
        if (rejection is not null) return rejection;
        return await handler();
    }
}
