using System.Security.Cryptography;
using System.Text.Json;
using Duluka.Server.Auth;
using Duluka.Server.Data;
using Duluka.Server.Domain;
using Duluka.Server.Security;
using Microsoft.Data.Sqlite;

namespace Duluka.Http.Integration.Tests;

/// <summary>
/// The regression matrix, updated for the C/5 reconciliation of the server to
/// the frozen C/1 contract (commit bf54d08 / ee07eb7 lineage):
///   - every response now speaks the §7.1 envelope {ok, reqId, errorCode,
///     httpStatus, retryable, conflict, message} with the payload under
///     `resource` on success; the harness sends X-ReqId and asserts the echo.
///   - registry codes (auth.session_expired / auth.session_revoked / nf.* /
///     conflict.link_conflict / perm.device_removed / server.*) replaced the
///     v0 ad-hoc codes; invalid_* / provider_reserved remain as documented
///     extensions (§7.2 unknown-code fallback applies client-side).
///   - CreateSession supersedes the previous session per device (§5.2) —
///     fixtures therefore seed ONE live session per device.
/// Ledger state after this pass: M-3, M-4, M-5, M-8 FIXED (now contract-PASS
/// tests); M-1, M-2, M-6, M-7, M-9 still violated (tracked); M-10 newly
/// discovered (409s carry conflict=null, §6.2 wants the current resource).
///
/// Grouping law: one REAL server process per group with a fresh SQLite file,
/// and the in-memory per-IP rate limiter (10/min on auth-start endpoints) is
/// respected by an explicit per-group budget. Budgets: G0=3 G1=7 G2=7 G3=5
/// G7=9 G9=11 (the test IS the window) G10=2 G11=8; G4/G5/G6 use none.
/// </summary>
internal static class Groups
{
    private static string Key(int bits) => Secrets.Base64Url(RandomNumberGenerator.GetBytes(bits / 8));

    // ─────────────────────────────────────────────────────────────────────────
    // G0 — health / transport / reserved providers
    // ─────────────────────────────────────────────────────────────────────────

    public static void Health(Runner r)
    {
        r.Group("G0 health/transport", configureGitHub: true, budget: 3, ctx =>
        {
            var app = ctx.App;

            r.Run("H-1 GET /healthz → 200 {status=live}", () =>
            {
                var resp = ServerApp.ExpectStatus(app.Get("/healthz"), 200, "H-1");
                ServerApp.Assert(ServerApp.JsonRoot(resp).GetProperty("status").GetString() == "live",
                    "H-1: status must be 'live'");
            });

            r.Run("H-2 GET /healthz/ready → 200 {status=ready, schema=2}", () =>
            {
                var resp = ServerApp.ExpectStatus(app.Get("/healthz/ready"), 200, "H-2");
                var json = ServerApp.JsonRoot(resp);
                ServerApp.Assert(json.GetProperty("status").GetString() == "ready", "H-2: status must be 'ready'");
                ServerApp.Assert(json.GetProperty("schema").GetInt32() == Database.SchemaVersion,
                    "H-2: schema must report Database.SchemaVersion");
            });

            r.Run("H-3 GET unknown route → 404, empty body (pin)", () =>
            {
                var resp = app.Get("/definitely-not-a-route");
                ServerApp.ExpectStatus(resp, 404, "H-3");
                ServerApp.Assert(resp.Body.Length == 0,
                    $"H-3: unknown route must not produce a body, got: {ServerApp.Trunc(resp.Body)}");
            });

            r.Run("H-4 PUT /healthz → 405 (method mismatch)", () =>
                ServerApp.ExpectStatus(app.Send("PUT", "/healthz"), 405, "H-4"));

            r.Run("H-5 POST /v1/auth/nvidia/start → 501 provider_reserved", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/nvidia/start", new { deviceName = "d", deviceKey = Key(256) }),
                    501, "provider_reserved", "H-5"));

            r.Run("H-6 POST /v1/auth/{unknown}/start → 501 provider_reserved", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/acme/start", new { deviceName = "d", deviceKey = Key(256) }),
                    501, "provider_reserved", "H-6"));

            r.Run("H-7 provider route is case-insensitive (GitHub = github)", () =>
                ServerApp.ExpectStatus(app.Post("/v1/auth/GitHub/start", new { deviceName = "d", deviceKey = Key(256) }),
                    200, "H-7"));

            r.Run("H-8 transport gate: server binds loopback only (R1)", () =>
            {
                var log = app.ReadLog();
                ServerApp.Assert(log.Contains("Now listening on: http://127.0.0.1:"),
                    "H-8: loopback bind line missing from server log");
                ServerApp.Assert(!log.Contains("http://0.0.0.0") && !log.Contains("http://*") && !log.Contains("http://+"),
                    "H-8: server must never widen its bind beyond loopback");
            });

            r.Run("H-9 log sweep G0 (no secrets in server logs)", () => app.Sweep("H-9"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G1 — auth/start validation
    // ─────────────────────────────────────────────────────────────────────────

    public static void AuthStart(Runner r)
    {
        r.Group("G1 auth/start validation", configureGitHub: true, budget: 10, ctx =>
        {
            var app = ctx.App;
            // Seeded session: S-10 proves the link-flow start endpoint parses
            // its body AFTER auth — the malformed-body 400 needs a valid bearer.
            var accG1 = app.SeedAccount("g1");
            var (devG1, _) = app.SeedDevice(accG1, "g1-dev");
            var tG1 = app.SeedSession(accG1, devG1, null);

            r.Run("S-1 missing deviceName → 400 invalid_device_name", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/github/start", new { deviceKey = Key(256) }),
                    400, "invalid_device_name", "S-1"));

            r.Run("S-2 deviceName 65 chars → 400 invalid_device_name", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/github/start", new { deviceName = new string('x', 65), deviceKey = Key(256) }),
                    400, "invalid_device_name", "S-2"));

            r.Run("S-3 deviceKey 42 chars → 400 invalid_device_key", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/github/start", new { deviceName = "d", deviceKey = Key(252) }),
                    400, "invalid_device_key", "S-3"));

            r.Run("S-4 43-char deviceKey (boundary) → 200; state returned RAW (F-1 fixed)", () =>
            {
                var resp = ServerApp.ExpectStatus(
                    app.Post("/v1/auth/github/start", new { deviceName = "d", deviceKey = Key(256) }), 200, "S-4");
                var url = ServerApp.Prop(resp, "authorizationUrl");
                ServerApp.Assert(url.StartsWith("https://github.com/login/oauth/authorize?"), "S-4: authorize URL host/path");
                foreach (var part in new[] { "client_id=", "redirect_uri=", "scope=read%3Auser", "response_type=code",
                                             "state=duluka_state_", "code_challenge=", "code_challenge_method=S256" })
                    ServerApp.Assert(url.Contains(part), $"S-4: authorize URL missing '{part}': {url}");
                // F-1 was: the JSON `state` field was redacted and only the URL
                // carried the usable value. The C/5 reconcile returns the state
                // RAW (it is the client's own correlation value; §9.2 redaction
                // applies to logs). The pin is inverted accordingly.
                var bodyState = ServerApp.Prop(resp, "state");
                var urlState = Runner.UrlParam(url, "state");
                ServerApp.Assert(urlState is { Length: > 0 } && urlState.StartsWith("duluka_state_"),
                    "S-4: URL state must be the real server-issued value");
                ServerApp.Assert(bodyState == urlState,
                    "S-4: body state must now equal the URL state (F-1 fixed by C/5)");
            });

            r.Run("S-5 deviceKey 300 chars → 200 (no upper bound pin)", () =>
                ServerApp.ExpectStatus(
                    app.Post("/v1/auth/github/start", new { deviceName = "d", deviceKey = Key(300 * 8) }), 200, "S-5"));

            r.Run("S-6 malformed JSON body → 400 bad_request envelope (M-1 FIXED)", () =>
            {
                // M-1 was: malformed/empty JSON escaped as a naked 500 with an
                // empty body. The fix maps every client protocol error to the
                // canonical 400 envelope (ExpectErr asserts ok=false, reqId
                // echo, httpStatus=400, retryable=false, conflict, message).
                ServerApp.ExpectErr(app.PostRaw("/v1/auth/github/start", "not json"),
                    400, "bad_request", "S-6");
            });

            r.Run("S-7 empty body → 400 bad_request envelope (M-1 FIXED)", () =>
                ServerApp.ExpectErr(app.PostRaw("/v1/auth/github/start", ""), 400, "bad_request", "S-7"));

            r.Run("S-8 truncated JSON body → 400 bad_request envelope", () =>
                ServerApp.ExpectErr(app.PostRaw("/v1/auth/github/start", "{\"deviceName\":\"d\","),
                    400, "bad_request", "S-8"));

            r.Run("S-9 malformed JSON on callback → 400 bad_request (all parse sites covered)", () =>
                ServerApp.ExpectErr(app.PostRaw("/v1/auth/github/callback", "not json"), 400, "bad_request", "S-9"));

            r.Run("S-10 malformed JSON on link-flow start (authed) → 400 bad_request", () =>
                ServerApp.ExpectErr(app.PostRaw("/v1/account/providers", "not json", bearer: tG1), 400, "bad_request", "S-10"));

            r.Run("S-8 log sweep G1 (raw device keys never logged)", () => app.Sweep("S-8"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G2 — callback validation + exactly-once state consumption.
    // Runs against an UNCONFIGURED server: the exchange throws
    // github_not_configured BEFORE any network I/O → deterministic offline.
    // ─────────────────────────────────────────────────────────────────────────

    public static void Callback(Runner r)
    {
        r.Group("G2 callback validation + exactly-once (unconfigured GitHub)", configureGitHub: false, budget: 7, ctx =>
        {
            var app = ctx.App;

            r.Run("C-1 callback nvidia → 501 provider_reserved", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/nvidia/callback", new { code = "c", state = "s", deviceKey = Key(256) }),
                    501, "provider_reserved", "C-1"));

            r.Run("C-2 callback missing code → 400 invalid_callback", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/github/callback", new { state = "s", deviceKey = Key(256) }),
                    400, "invalid_callback", "C-2"));

            r.Run("C-3 callback 42-char deviceKey → 400 invalid_device_key", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/github/callback", new { code = "c", state = "s", deviceKey = Key(252) }),
                    400, "invalid_device_key", "C-3"));

            r.Run("C-4 callback unknown state → 400 invalid_state", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/github/callback", new { code = "c", state = "duluka_state_unknown", deviceKey = Key(256) }),
                    400, "invalid_state", "C-4"));

            r.Run("C-5 state is single-use even when the exchange fails (C/3 exactly-once)", () =>
            {
                var start = ServerApp.ExpectStatus(
                    app.Post("/v1/auth/github/start", new { deviceName = "d", deviceKey = Key(256) }), 200, "C-5/start");
                var state = ServerApp.Prop(start, "state");
                ServerApp.Assert(state.StartsWith("duluka_state_"), "C-5: raw state issued");
                ServerApp.ExpectErr(app.Post("/v1/auth/github/callback", new { code = "bogus-code", state, deviceKey = Key(256) }),
                    400, "github_not_configured", "C-5/first");
                // The state was consumed BEFORE the exchange ran — a replayed
                // callback cannot mint a second attempt from it.
                ServerApp.ExpectErr(app.Post("/v1/auth/github/callback", new { code = "bogus-code", state, deviceKey = Key(256) }),
                    400, "invalid_state", "C-5/replay");
            });

            r.Run("C-6 log sweep G2 (codes/states never logged)", () => app.Sweep("C-6"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G3 — real provider exchange failure path. Egress-gated: RUN when
    // github.com is reachable, honest SKIP otherwise (never a fake PASS).
    // ─────────────────────────────────────────────────────────────────────────

    public static void ProviderExchange(Runner r)
    {
        r.Group("G3 provider exchange failure (real GitHub, egress-gated)", configureGitHub: true, budget: 5, ctx =>
        {
            var app = ctx.App;

            if (!Egress.Reachable())
            {
                r.SkipGroup("github.com unreachable in this environment — exchange path cannot be exercised honestly");
                return;
            }

            r.Run("E-1 bogus code → 400 exchange-rejected envelope; state still single-use", () =>
            {
                var code = "bogus-code-" + Guid.NewGuid().ToString("N");
                var start = ServerApp.ExpectStatus(
                    app.Post("/v1/auth/github/start", new { deviceName = "d", deviceKey = Key(256) }), 200, "E-1/start");
                var state = ServerApp.Prop(start, "state");
                // GitHub's rejection class varies with how it answers the token
                // exchange (200+error body → provider_callback_rejected; non-2xx
                // → provider_token_exchange_failed). The deterministic contract:
                // a 400 envelope — never a 5xx — and the state stays single-use.
                var resp = app.Post("/v1/auth/github/callback", new { code, state, deviceKey = Key(256) });
                ServerApp.ExpectStatus(resp, 400, "E-1/first");
                ServerApp.Assert(resp.Body.Contains("provider_callback_rejected") || resp.Body.Contains("provider_token_exchange_failed"),
                    $"E-1/first: unexpected envelope: {ServerApp.Trunc(resp.Body)}");
                ServerApp.ExpectErr(app.Post("/v1/auth/github/callback", new { code, state, deviceKey = Key(256) }),
                    400, "invalid_state", "E-1/replay");
            });

            r.Run("E-2 rejected exchange never echoes the raw code (body or log)", () =>
            {
                var code = "bogus-code-" + Guid.NewGuid().ToString("N");
                var start = ServerApp.ExpectStatus(
                    app.Post("/v1/auth/github/start", new { deviceName = "d", deviceKey = Key(256) }), 200, "E-2/start");
                var state = ServerApp.Prop(start, "state");
                app.Post("/v1/auth/github/callback", new { code, state, deviceKey = Key(256) });
                app.Sweep("E-2"); // sweep asserts the raw code appears nowhere
            });
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G4 — authenticated surface / 401 matrix (one live session per device)
    // ─────────────────────────────────────────────────────────────────────────

    public static void AuthenticatedSurface(Runner r)
    {
        r.Group("G4 authenticated surface / 401 matrix", configureGitHub: true, budget: 0, ctx =>
        {
            var app = ctx.App;
            var accA = app.SeedAccount("alice");
            var linkA = app.SeedLink(accA, "777");
            var (devA1, _) = app.SeedDevice(accA, "alice-pc");
            var (devA2, _) = app.SeedDevice(accA, "alice-old-pc");
            var (devA3, _) = app.SeedDevice(accA, "alice-laptop");
            var tA1 = app.SeedSession(accA, devA1, linkA);                     // live
            var tExpired = app.SeedSession(accA, devA2, linkA,
                expires: DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1));     // expired
            var tRevoked = app.SeedSession(accA, devA3, linkA, revoked: true); // revoked
            var accS = app.SeedAccount("suspended");
            var (devS, _) = app.SeedDevice(accS, "s-pc");
            var tSuspended = app.SeedSession(accS, devS, null);
            app.Suspend(accS);

            r.Run("ME-1 GET /v1/account/me with live session → 200 full shape", () =>
            {
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA1), "ME-1");
                var json = ServerApp.Json(app.Get("/v1/account/me", tA1));
                ServerApp.Assert(json.GetProperty("accountId").GetString() == accA, "ME-1: accountId mismatch");
                ServerApp.Assert(json.GetProperty("displayName").GetString() == "alice", "ME-1: displayName mismatch");
                ServerApp.Assert(json.GetProperty("currentDevice").GetProperty("deviceId").GetString() == devA1,
                    "ME-1: currentDevice.deviceId mismatch");
                ServerApp.Assert(json.GetProperty("currentDevice").GetProperty("deviceName").GetString() == "alice-pc",
                    "ME-1: currentDevice.deviceName mismatch");
                DateTimeOffset.Parse(json.GetProperty("createdAt").GetString()!);
            });

            r.Run("ME-2 /me with NO Authorization header → 401 auth.session_expired (F-2 resolved: uniform code)", () =>
                ServerApp.ExpectErr(app.Get("/v1/account/me"), 401, "auth.session_expired", "ME-2"));

            r.Run("ME-3 /me with empty Bearer → 401 auth.session_expired", () =>
                ServerApp.ExpectErr(app.Get("/v1/account/me", ""), 401, "auth.session_expired", "ME-3"));

            r.Run("ME-4 /me with non-Bearer scheme → 401 auth.session_expired", () =>
                ServerApp.ExpectErr(app.Send("GET", "/v1/account/me", rawAuthorization: "Basic dXNlcjpwYXNz"),
                    401, "auth.session_expired", "ME-4"));

            r.Run("ME-5 /me with unknown duluka_st_ token → 401 auth.session_expired", () =>
                ServerApp.ExpectErr(app.Get("/v1/account/me", "duluka_st_" + Secrets.Base64Url(new byte[32])),
                    401, "auth.session_expired", "ME-5"));

            r.Run("ME-6 /me with expired session → 401 auth.session_expired", () =>
                ServerApp.ExpectErr(app.Get("/v1/account/me", tExpired), 401, "auth.session_expired", "ME-6"));

            r.Run("ME-7 revoked vs expired distinguished (M-8 FIXED) → auth.session_revoked", () =>
            {
                ServerApp.ExpectErr(app.Get("/v1/account/me", tRevoked), 401, "auth.session_revoked", "ME-7/revoked");
                ServerApp.ExpectErr(app.Get("/v1/account/me", tExpired), 401, "auth.session_expired", "ME-7/expired");
                // M-8 was: everything conflated into session_invalid. The C/5
                // reconcile distinguishes revocation (§7.2 registry codes);
                // unknown/expired/missing share auth.session_expired per the
                // frozen registry (no unknown_session code exists in §7.2).
            });

            r.Run("ME-8 second session on a device SUPERSEDES the first (M-3 FIXED, §5.2)", () =>
            {
                // The old M-3 violation: two concurrent live sessions on one
                // device. CreateSession now revokes-before-insert; the old
                // token answers 401 auth.session_revoked, the new one 200.
                var t2 = app.SeedSession(accA, devA1, linkA);
                ServerApp.ExpectErr(app.Get("/v1/account/me", tA1), 401, "auth.session_revoked", "ME-8/superseded");
                ServerApp.ExpectOk(app.Get("/v1/account/me", t2), "ME-8/current");
            });

            r.Run("ME-9 suspended account → observed 401, contract §6.3 requires 403 — MISMATCH M-2 (unfixed)", () =>
            {
                var resp = app.Get("/v1/account/me", tSuspended);
                ServerApp.ExpectStatus(resp, 401, "ME-9");
                ServerApp.Assert(resp.Body.Contains("auth.session_expired"), "ME-9: observed code changed");
                r.Mismatch("M-2", $"suspended account → HTTP {resp.Status} auth.session_expired (contract: 403 perm.account_suspended)");
            });

            r.Run("ME-10 body sweep G4 (no raw token in any response body)", () => app.Sweep("ME-10"));

            r.Run("ME-11 log sweep G4", () => app.Sweep("ME-11"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G5 — devices + revoke cascade (one live session per device)
    // ─────────────────────────────────────────────────────────────────────────

    public static void Devices(Runner r)
    {
        r.Group("G5 devices + revoke cascade", configureGitHub: true, budget: 0, ctx =>
        {
            var app = ctx.App;
            var accA = app.SeedAccount("alice");
            var (devA1, _) = app.SeedDevice(accA, "alice-pc");
            var (devA2, _) = app.SeedDevice(accA, "alice-tablet");
            var (devA3, _) = app.SeedDevice(accA, "alice-phone2");
            var (devOld, _) = app.SeedDevice(accA, "alice-old", revoked: true); // pre-revoked, must stay listed
            var (devA4, _) = app.SeedDevice(accA, "alice-phone");
            var accB = app.SeedAccount("bob");
            var (devB1, _) = app.SeedDevice(accB, "bob-pc");

            var tA1 = app.SeedSession(accA, devA1, null);
            var tA2a = app.SeedSession(accA, devA2, null);
            var tA2b = app.SeedSession(accA, devA3, null); // sibling device — unaffected by revoking devA2
            var tA4 = app.SeedSession(accA, devA4, null);
            var tB1 = app.SeedSession(accB, devB1, null);

            r.Run("DEV-1 GET /devices lists own devices incl. revoked one, never foreign", () =>
            {
                var resp = ServerApp.ExpectStatus(app.Get("/v1/account/devices", tA1), 200, "DEV-1");
                var arr = ServerApp.Json(resp).GetProperty("devices");
                ServerApp.Assert(arr.GetArrayLength() == 5, $"DEV-1: exactly 5 own devices, got {arr.GetArrayLength()}");
                var ids = arr.EnumerateArray().Select(d => d.GetProperty("deviceId").GetString()).ToHashSet();
                ServerApp.Assert(ids.SetEquals(new[] { devA1, devA2, devA3, devOld, devA4 }), "DEV-1: device id set mismatch");
                var doomed = arr.EnumerateArray().Single(d => d.GetProperty("deviceId").GetString() == devOld);
                ServerApp.Assert(doomed.GetProperty("revokedAt").ValueKind == JsonValueKind.String,
                    "DEV-1: pre-revoked device must be visible with RevokedAt");
            });

            r.Run("DEV-2 revoke D_A2 → 200, sessionsRevoked=1 (its live session)", () =>
            {
                var resp = ServerApp.ExpectStatus(app.Post($"/v1/account/devices/{devA2}/revoke", bearer: tA1), 200, "DEV-2");
                var json = ServerApp.Json(resp);
                ServerApp.Assert(json.GetProperty("revoked").GetBoolean(), "DEV-2: revoked flag");
                ServerApp.Assert(json.GetProperty("sessionsRevoked").GetInt32() == 1, "DEV-2: cascade count must be 1");
            });

            r.Run("DEV-3 cascade: the revoked device's session is dead", () =>
                ServerApp.ExpectErr(app.Get("/v1/account/me", tA2a), 401, "auth.session_revoked", "DEV-3"));

            r.Run("DEV-4 no collateral: sessions on sibling devices survive", () =>
            {
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA2b), "DEV-4/a");
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA4), "DEV-4/b");
            });

            r.Run("DEV-5 revoke D_A2 again → 200, sessionsRevoked=0 (idempotent)", () =>
            {
                var json = ServerApp.Json(
                    ServerApp.ExpectStatus(app.Post($"/v1/account/devices/{devA2}/revoke", bearer: tA1), 200, "DEV-5"));
                ServerApp.Assert(json.GetProperty("sessionsRevoked").GetInt32() == 0, "DEV-5: second revoke must not re-count");
            });

            r.Run("DEV-6 revoke unknown device → 404 nf.device", () =>
                ServerApp.ExpectErr(app.Post("/v1/account/devices/duluka_dev_unknown/revoke", bearer: tA1),
                    404, "nf.device", "DEV-6"));

            r.Run("DEV-7 cross-tenant revoke → 404 (cross-tenant REQUIRED 404), no collateral", () =>
            {
                ServerApp.ExpectErr(app.Post($"/v1/account/devices/{devB1}/revoke", bearer: tA1), 404, "nf.device", "DEV-7");
                ServerApp.ExpectOk(app.Get("/v1/account/me", tB1), "DEV-7/bob-alive");
            });

            r.Run("DEV-8 revoke OWN current device → own session dies in cascade", () =>
            {
                ServerApp.ExpectStatus(app.Post($"/v1/account/devices/{devA1}/revoke", bearer: tA1), 200, "DEV-8");
                ServerApp.ExpectErr(app.Get("/v1/account/me", tA1), 401, "auth.session_revoked", "DEV-8/after");
            });

            r.Run("DEV-9 log sweep G5", () => app.Sweep("DEV-9"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G6 — session lifecycle (refresh sliding/absolute cap, revoke, revoke-all)
    // ─────────────────────────────────────────────────────────────────────────

    public static void Sessions(Runner r)
    {
        r.Group("G6 session lifecycle", configureGitHub: true, budget: 0, ctx =>
        {
            var app = ctx.App;
            var accA = app.SeedAccount("alice");
            var (devA1, _) = app.SeedDevice(accA, "alice-pc");
            var (devA2, _) = app.SeedDevice(accA, "alice-laptop");
            var (devA3, _) = app.SeedDevice(accA, "alice-tablet");
            var (devA4, _) = app.SeedDevice(accA, "alice-netbook");
            var (devA5, _) = app.SeedDevice(accA, "alice-cap");

            var t0 = DateTimeOffset.UtcNow;
            var tA1 = app.SeedSession(accA, devA1, null, expires: t0 + TimeSpan.FromDays(30)); // fresh 30d, explicit for a tick-exact assertion
            var tA2 = app.SeedSession(accA, devA2, null);
            var tA3 = app.SeedSession(accA, devA3, null);
            var tExpired = app.SeedSession(accA, devA4, null, expires: t0 - TimeSpan.FromMinutes(1));
            // Cap probe: created 26d ago, slid to +2d. Absolute cap = createdAt+30d = t0+4d.
            var tCap = app.SeedSession(accA, devA5, null,
                expires: t0 + TimeSpan.FromDays(2), createdAt: t0 - TimeSpan.FromDays(26));

            r.Run("SES-1 refresh fresh session → returns the unchanged 30d expiry (never shortens)", () =>
            {
                var json = ServerApp.Json(
                    ServerApp.ExpectStatus(app.Post("/v1/auth/session/refresh", bearer: tA1), 200, "SES-1"));
                var got = DateTimeOffset.Parse(json.GetProperty("sessionExpiresAt").GetString()!);
                ServerApp.Assert(got == t0 + TimeSpan.FromDays(30), $"SES-1: expected unchanged expiry {t0 + TimeSpan.FromDays(30):O}, got {got:O}");
            });

            r.Run("SES-2 refresh hits the absolute cap exactly (createdAt+30d)", () =>
            {
                var json = ServerApp.Json(
                    ServerApp.ExpectStatus(app.Post("/v1/auth/session/refresh", bearer: tCap), 200, "SES-2"));
                var got = DateTimeOffset.Parse(json.GetProperty("sessionExpiresAt").GetString()!);
                ServerApp.Assert(got == t0 + TimeSpan.FromDays(4), $"SES-2: expected exact cap {t0 + TimeSpan.FromDays(4):O}, got {got:O}");
            });

            r.Run("SES-3 refresh: expired/unknown/no-token → 401 with registry codes", () =>
            {
                ServerApp.ExpectErr(app.Post("/v1/auth/session/refresh", bearer: tExpired), 401, "auth.session_expired", "SES-3/expired");
                ServerApp.ExpectErr(app.Post("/v1/auth/session/refresh", bearer: "duluka_st_" + Secrets.Base64Url(new byte[32])),
                    401, "auth.session_expired", "SES-3/unknown");
                ServerApp.ExpectErr(app.Post("/v1/auth/session/refresh"), 401, "auth.session_expired", "SES-3/no-token");
            });

            r.Run("SES-4 revoke own session → 200; sibling session survives", () =>
            {
                ServerApp.ExpectOk(app.Post("/v1/auth/session/revoke", bearer: tA2), "SES-4");
                ServerApp.ExpectErr(app.Get("/v1/account/me", tA2), 401, "auth.session_revoked", "SES-4/after");
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA3), "SES-4/sibling");
            });

            r.Run("SES-5 revoke again → 401 auth.session_revoked — MISMATCH M-9 (unfixed)", () =>
            {
                // The session STAYS dead (no resurrect) — the security property
                // holds. But the STATUS is a tracked mismatch: C/2 SES-2b (and
                // R2 §14 G-3) requires the second revoke to answer an idempotent
                // 200, because the goal state is already reached. C/5 kept the
                // 401 (now with the distinguishing registry code).
                ServerApp.ExpectErr(app.Post("/v1/auth/session/revoke", bearer: tA2), 401, "auth.session_revoked", "SES-5");
                r.Mismatch("M-9", "second session revoke → 401 auth.session_revoked (C/2 SES-2b / R2 G-3: idempotent 200 — goal state already reached)");
            });

            r.Run("SES-6 revoke-all → counts exactly the unrevoked sessions, all live ones die incl. caller", () =>
            {
                var json = ServerApp.Json(
                    ServerApp.ExpectStatus(app.Post("/v1/auth/sessions/revoke-all", bearer: tA1), 200, "SES-6"));
                // Finding F-3 (still current): the count includes the
                // ALREADY-EXPIRED-but-unrevoked session (RevokeAllForAccount has
                // no expiry filter): 4 = tA1, tA3, tCap, tExpired. Harmless —
                // the expired one was already unusable — but overcounts.
                ServerApp.Assert(json.GetProperty("revokedSessions").GetInt32() == 4,
                    "SES-6: expected 4 unrevoked sessions counted (3 live + 1 expired — finding F-3)");
                foreach (var t in new[] { tA1, tA3, tCap })
                    ServerApp.ExpectErr(app.Get("/v1/account/me", t), 401, "auth.session_revoked", "SES-6/after");
            });

            r.Run("SES-7 revoke-all is idempotent: only NEWLY live sessions are counted", () =>
            {
                var tA4 = app.SeedSession(accA, devA1, null); // devA1's tA1 already revoked — supersedes nothing
                var tA5 = app.SeedSession(accA, devA2, null);
                var json = ServerApp.Json(
                    ServerApp.ExpectStatus(app.Post("/v1/auth/sessions/revoke-all", bearer: tA4), 200, "SES-7"));
                ServerApp.Assert(json.GetProperty("revokedSessions").GetInt32() == 2,
                    "SES-7: dead sessions must never be re-counted");
                ServerApp.ExpectErr(app.Get("/v1/account/me", tA5), 401, "auth.session_revoked", "SES-7/after");
            });

            r.Run("SES-8 revoke-all with unknown/no token → 401 auth.session_expired (uniform)", () =>
            {
                ServerApp.ExpectErr(app.Post("/v1/auth/sessions/revoke-all", bearer: "duluka_st_" + Secrets.Base64Url(new byte[32])),
                    401, "auth.session_expired", "SES-8/unknown");
                ServerApp.ExpectErr(app.Post("/v1/auth/sessions/revoke-all"), 401, "auth.session_expired", "SES-8/no-token");
            });

            r.Run("SES-9 log sweep G6", () => app.Sweep("SES-9"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G7 — provider links + unlink guards (unconfigured GitHub for the
    // deterministic pre-exchange behavior of /complete). Budget: 9 auth-start.
    // ─────────────────────────────────────────────────────────────────────────

    public static void Providers(Runner r)
    {
        r.Group("G7 provider links + unlink", configureGitHub: false, budget: 9, ctx =>
        {
            var app = ctx.App;
            var accA = app.SeedAccount("alice");
            var linkA1 = app.SeedLink(accA, "777");
            var linkA2 = app.SeedLink(accA, "778");
            var accB = app.SeedAccount("bob");
            var linkB1 = app.SeedLink(accB, "999");
            var (devA1, _) = app.SeedDevice(accA, "alice-pc");
            var (devA2, _) = app.SeedDevice(accA, "alice-laptop");
            var (devA3, _) = app.SeedDevice(accA, "alice-via");
            var (devB, _) = app.SeedDevice(accB, "bob-pc");
            var tA1 = app.SeedSession(accA, devA1, linkA1);
            var tA2 = app.SeedSession(accA, devA2, linkA2);
            var tB1 = app.SeedSession(accB, devB, null);

            r.Run("PRV-1 GET /providers lists exactly own ACTIVE links, no foreign rows", () =>
            {
                var resp = ServerApp.ExpectStatus(app.Get("/v1/account/providers", tA1), 200, "PRV-1");
                var arr = ServerApp.Json(resp).GetProperty("providers");
                ServerApp.Assert(arr.GetArrayLength() == 2, $"PRV-1: exactly 2 links, got {arr.GetArrayLength()}");
                foreach (var l in arr.EnumerateArray())
                {
                    ServerApp.Assert(l.GetProperty("status").GetString() == "Active", "PRV-1: all seeded links Active");
                    ServerApp.Assert(l.GetProperty("providerKey").GetString() == "github", "PRV-1: providerKey");
                    ServerApp.Assert(l.GetProperty("linkId").GetString() is { Length: > 0 }, "PRV-1: linkId present");
                }
            });

            r.Run("PRV-5 start link flow for github → 200, state bound to the session", () =>
            {
                var resp = ServerApp.ExpectStatus(
                    app.Post("/v1/account/providers", new { provider = "github" }, tA1), 200, "PRV-5");
                var url = ServerApp.Prop(resp, "authorizationUrl");
                ServerApp.Assert(Runner.UrlParam(url, "state") is { Length: > 0 }, "PRV-5: state must travel in the URL");
                ServerApp.Assert(ServerApp.Prop(resp, "state").StartsWith("duluka_state_"), "PRV-5: raw state also in body (F-1 fixed)");
            });

            r.Run("PRV-6 complete with garbage code → 400 github_not_configured pre-network; session intact", () =>
            {
                var start = ServerApp.ExpectStatus(
                    app.Post("/v1/account/providers", new { provider = "github" }, tA1), 200, "PRV-6/start");
                var state = ServerApp.Prop(start, "state");
                ServerApp.ExpectErr(app.Post("/v1/account/providers/github/complete", new { code = "bogus-code", state }),
                    400, "github_not_configured", "PRV-6");
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA1), "PRV-6/session-intact");
            });

            r.Run("PRV-7 complete replay → 400 invalid_state (single-use)", () =>
            {
                var start = ServerApp.ExpectStatus(
                    app.Post("/v1/account/providers", new { provider = "github" }, tA1), 200, "PRV-7/start");
                var state = ServerApp.Prop(start, "state");
                app.Post("/v1/account/providers/github/complete", new { code = "x", state });
                ServerApp.ExpectErr(app.Post("/v1/account/providers/github/complete", new { code = "x", state }),
                    400, "invalid_state", "PRV-7");
            });

            r.Run("PRV-8 complete unknown state → 400 invalid_state", () =>
                ServerApp.ExpectErr(app.Post("/v1/account/providers/github/complete", new { code = "x", state = "duluka_state_unknown" }),
                    400, "invalid_state", "PRV-8"));

            r.Run("PRV-9 complete after the binding session died → 401, but code is folded — MISMATCH M-11 (new)", () =>
            {
                var start = ServerApp.ExpectStatus(
                    app.Post("/v1/account/providers", new { provider = "github" }, tA2), 200, "PRV-9/start");
                var state = ServerApp.Prop(start, "state");
                ServerApp.ExpectOk(app.Post("/v1/auth/session/revoke", bearer: tA2), "PRV-9/revoke");
                // The complete endpoint hardcodes auth.session_expired for the
                // dead binding session, while every OTHER endpoint distinguishes
                // revocation via DeadSessionCode (§7.2). One-endpoint vocabulary
                // gap discovered by this pass.
                ServerApp.ExpectErr(app.Post("/v1/account/providers/github/complete", new { code = "x", state }),
                    401, "auth.session_expired", "PRV-9");
                r.Mismatch("M-11", "link-complete 401 folds a REVOKED binding session into auth.session_expired (other endpoints answer auth.session_revoked) — endpoint should use DeadSessionCode like the rest");
            });

            r.Run("UNL-1 unlink non-last provider → 200 + via-link session revoked (M-5 status fixed: 409→ now normal path)", () =>
            {
                var tVia = app.SeedSession(accA, devA3, linkA2); // fresh session issued VIA linkA2 (own device)
                var json = ServerApp.Json(ServerApp.ExpectStatus(app.Delete($"/v1/account/providers/{linkA2}", tA1), 200, "UNL-1"));
                ServerApp.Assert(json.GetProperty("unlinked").GetBoolean(), "UNL-1: unlinked flag");
                ServerApp.Assert(json.GetProperty("revokedSessions").GetInt32() == 1, "UNL-1: exactly the via-link session dies");
                ServerApp.ExpectErr(app.Get("/v1/account/me", tVia), 401, "auth.session_revoked", "UNL-1/via-dead");
            });

            r.Run("UNL-2 after unlink: other session alive, status Unlinked, superseded sibling still dead", () =>
            {
                ServerApp.ExpectErr(app.Get("/v1/account/me", tA2), 401, "auth.session_revoked", "UNL-2/a");
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA1), "UNL-2/b");
                var arr = ServerApp.Json(app.Get("/v1/account/providers", tA1)).GetProperty("providers");
                var l2 = arr.EnumerateArray().Single(l => l.GetProperty("linkId").GetString() == linkA2);
                ServerApp.Assert(l2.GetProperty("status").GetString() == "Unlinked", "UNL-2: link must be soft-unlinked");
            });

            r.Run("UNL-3 last-provider unlink → 409 conflict.link_conflict (C/2 UNL-2; M-5 status FIXED)", () =>
                ServerApp.ExpectErr(app.Delete($"/v1/account/providers/{linkA1}", tA1), 409, "conflict.link_conflict", "UNL-3"));

            r.Run("UNL-4 unlink unknown linkId → 404 nf.link", () =>
                ServerApp.ExpectErr(app.Delete("/v1/account/providers/duluka_link_unknown", tA1), 404, "nf.link", "UNL-4"));

            r.Run("UNL-5 cross-tenant unlink → 404 nf.link, no leak, no collateral", () =>
            {
                ServerApp.ExpectErr(app.Delete($"/v1/account/providers/{linkB1}", tA1), 404, "nf.link", "UNL-5");
                ServerApp.ExpectOk(app.Get("/v1/account/me", tB1), "UNL-5/bob-alive");
            });

            r.Run("UNL-6 unlink already-unlinked link → 409 conflict.link_conflict (M-5 FIXED: was 400)", () =>
            {
                // C/2 pins the resource-shape re-unlink at 409 (its code string
                // provider_not_linked was replaced by the §7.2 registry family
                // code conflict.link_conflict — accepted alias, frozen registry
                // wins over the C/2 literal).
                ServerApp.ExpectErr(app.Delete($"/v1/account/providers/{linkA2}", tA1), 409, "conflict.link_conflict", "UNL-6");
            });

            r.Run("UNL-7 log sweep G7", () => app.Sweep("UNL-7"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G8 — store-level CAS races on the REAL SQLite file through the REAL
    // production data layer (no HTTP: the races live below the transport).
    // ─────────────────────────────────────────────────────────────────────────

    public static void StoreRaceTests(Runner r)
    {
        r.Run("RACE-1 8 concurrent first-logins, same identity → exactly ONE account (real store)", () =>
        {
            using var store = new StoreRaces.Store();
            var barrier = new Barrier(8);
            var accounts = new System.Collections.Concurrent.ConcurrentBag<string>();
            var errors = new System.Collections.Concurrent.ConcurrentBag<string>();
            Parallel.For(0, 8, i =>
            {
                try
                {
                    barrier.SignalAndWait(TimeSpan.FromSeconds(10));
                    var (db, provisioning) = store.OpenThreadStore();
                    try
                    {
                        var identity = new GitHubIdentity(ProviderKeys.GitHub, "race-identity", "race@example.test", "racer");
                        var deviceKeyHash = Secrets.Sha256Hex(Secrets.NewToken("devk_"));
                        var (_, account, _) = provisioning.LoginOrLink(identity, deviceKeyHash, "race-dev-" + i);
                        accounts.Add(account.AccountId);
                    }
                    finally { db.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
                }
                catch (Exception ex) { errors.Add(ex.GetType().Name + ": " + ex.Message); }
            });
            ServerApp.Assert(errors.IsEmpty, "RACE-1: no exception may escape: " + string.Join("; ", errors.Take(3)));
            ServerApp.Assert(accounts.Distinct().Count() == 1,
                $"RACE-1: must converge on ONE account, got {accounts.Distinct().Count()}");
            ServerApp.Assert(store.Db.ActiveLinkCount(accounts.First()) == 1, "RACE-1: exactly one active link");
            ServerApp.Assert(store.Scalar("SELECT COUNT(*) FROM DulukaAccount") == "1",
                "RACE-1: loser's orphan account must be cleaned up");
        });

        r.Run("RACE-DEV same NEW device key racing → one device row survives; only UNIQUE violations may escape (M-6 evidence)", () =>
        {
            using var store = new StoreRaces.Store();
            var barrier = new Barrier(8);
            var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();
            Parallel.For(0, 8, i =>
            {
                try
                {
                    barrier.SignalAndWait(TimeSpan.FromSeconds(10));
                    var (db, provisioning) = store.OpenThreadStore();
                    try
                    {
                        var identity = new GitHubIdentity(ProviderKeys.GitHub, "race-dev-identity", "race@example.test", "racer");
                        provisioning.LoginOrLink(identity, Secrets.Sha256Hex("devk_shared-race-key"), "race-dev");
                    }
                    finally { db.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
                }
                catch (Exception ex) { errors.Add(ex); }
            });
            // Deterministic invariants under ANY interleaving:
            foreach (var ex in errors)
                ServerApp.Assert(ex is SqliteException { SqliteErrorCode: 19 },
                    "RACE-DEV: only the known unguarded UNIQUE violation may escape (M-6); got: " +
                    ex.GetType().Name + ": " + ex.Message);
            ServerApp.Assert(
                store.Scalar($"SELECT COUNT(*) FROM AccountDevice WHERE DeviceKeyHash='{Secrets.Sha256Hex("devk_shared-race-key")}'") == "1",
                "RACE-DEV: exactly one device row may survive");
            ServerApp.Assert(store.Scalar("SELECT COUNT(*) FROM DulukaAccount") == "1", "RACE-DEV: accounts must converge to one");
            if (!errors.IsEmpty)
                Console.WriteLine(
                    $"      ⚠ evidence for M-6: {errors.Count}/8 threads hit the unguarded device-key UNIQUE violation " +
                    "(still escapes LoginOrLink; the callback's catch-all now answers a well-formed 500 envelope — the functional loss remains)");
        });

        r.Run("RACE-3 parallel revoke + refresh converge to revoked (no resurrect)", () =>
        {
            using var store = new StoreRaces.Store();
            var acc = store.Db.CreateAccount("racer");
            var dev = store.Db.CreateDevice(acc.AccountId, "d", Secrets.Sha256Hex(Secrets.NewToken("devk_")));
            var token = Secrets.NewToken("duluka_st_");
            store.Db.CreateSession(acc.AccountId, dev.DeviceId, null, Secrets.Sha256Hex(token), DateTimeOffset.UtcNow + TimeSpan.FromDays(1));
            var barrier = new Barrier(5);
            var refreshResults = new System.Collections.Concurrent.ConcurrentBag<DateTimeOffset?>();
            var revoked = false;
            Parallel.For(0, 5, i =>
            {
                barrier.SignalAndWait(TimeSpan.FromSeconds(10));
                if (i == 0)
                {
                    revoked = store.Db.RevokeSessionByTokenHash(Secrets.Sha256Hex(token), "race", DateTimeOffset.UtcNow);
                }
                else
                {
                    var (db, _) = store.OpenThreadStore();
                    // compile-fix: RefreshSession gained the absoluteWindow param (assertions unchanged)
                    try { refreshResults.Add(db.RefreshSession(Secrets.Sha256Hex(token), TimeSpan.FromDays(7), TimeSpan.FromDays(30))); }
                    finally { db.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
                }
            });
            ServerApp.Assert(revoked, "RACE-3: revoke must succeed");
            ServerApp.Assert(store.Db.ValidateSession(Secrets.Sha256Hex(token)) is null,
                "RACE-3: final state must be revoked (no resurrect)");
        });

        r.Run("RACE-4 sequential duplicate identity → same account, existed=true (dedup path)", () =>
        {
            using var store = new StoreRaces.Store();
            var identity = new GitHubIdentity(ProviderKeys.GitHub, "dup-identity", "dup@example.test", "dup");
            var (_, acc1, existed1) = store.Provisioning.LoginOrLink(identity, Secrets.Sha256Hex(Secrets.NewToken("devk_")), "d1");
            var (_, acc2, existed2) = store.Provisioning.LoginOrLink(identity, Secrets.Sha256Hex(Secrets.NewToken("devk_")), "d2");
            ServerApp.Assert(!existed1 && existed2, "RACE-4: second login must be marked existing");
            ServerApp.Assert(acc1.AccountId == acc2.AccountId, "RACE-4: same identity → same account");
            ServerApp.Assert(store.Scalar("SELECT COUNT(*) FROM DulukaAccount") == "1", "RACE-4: no duplicate account");
        });

        r.Run("RACE-5 provider-link race guard: CreateLink loser converges on the winner's row, never a silent merge", () =>
        {
            // The C/5 CreateLink returns the WINNER's row on the unique-anchor
            // race; the endpoint must (and does) check AccountId ownership
            // before reporting success. Here we pin the store-level behavior:
            // the loser of the insert gets the winner's link, which belongs to
            // a DIFFERENT account than the one it was asked to link.
            using var store = new StoreRaces.Store();
            var accB = store.Db.CreateAccount("B");
            var identity = new GitHubIdentity(ProviderKeys.GitHub, "link-race-identity", "lr@example.test", "lr");
            var (_, accountWinner, _) = store.Provisioning.LoginOrLink(identity, Secrets.Sha256Hex(Secrets.NewToken("devk_")), "winner-dev");
            // Direct second link for the same identity onto a DIFFERENT account:
            // the partial unique index must reject it (19) and CreateLink must
            // converge on the winner's row.
            SqliteException? caught = null;
            AccountProviderLink? loserRow = null;
            try { loserRow = store.Db.CreateLink(accB.AccountId, identity.ProviderKey, identity.ProviderUserId, null, null); }
            catch (SqliteException ex) { caught = ex; }
            if (caught is not null)
            {
                ServerApp.Assert(caught.SqliteErrorCode == 19, "RACE-5: only the unique-anchor violation may surface");
            }
            else
            {
                ServerApp.Assert(loserRow!.AccountId == accountWinner.AccountId,
                    "RACE-5: CreateLink must converge on the winner's row (ownership check upstream turns this into 409)");
            }
            ServerApp.Assert(store.Db.ActiveLinkCount(accountWinner.AccountId) + store.Db.ActiveLinkCount(accB.AccountId) == 1,
                "RACE-5: exactly one active link for the identity across both accounts");
            ServerApp.Assert(store.Scalar("SELECT COUNT(*) FROM AccountProviderLink WHERE Status='Active'") == "1",
                "RACE-5: no silent merge — one active link row total");
        });

        r.Run("RACE-6 re-login with a PREVIOUSLY UNLINKED identity succeeds (v1→v2 partial-index fix)", () =>
        {
            // v1's table-level UNIQUE(ProviderKey, ProviderUserId) made a
            // re-login with a previously unlinked identity a permanent 500.
            // Schema v2 anchors uniqueness on ACTIVE rows only.
            using var store = new StoreRaces.Store();
            var identity = new GitHubIdentity(ProviderKeys.GitHub, "relink-identity", "re@example.test", "re");
            var (_, acc1, _) = store.Provisioning.LoginOrLink(identity, Secrets.Sha256Hex(Secrets.NewToken("devk_")), "d1");
            var link = store.Db.LinksForAccount(acc1.AccountId).Single();
            store.Db.Unlink(link.LinkId, DateTimeOffset.UtcNow); // direct store unlink (last-provider guard bypassed on purpose)
            // Documented v2 behavior: an unlinked identity reads as UNKNOWN --
            // the re-login provisions a NEW account (C/2 ACC-2 same-account rule
            // covers ACTIVE identities only). The v1 permanent-500 is gone.
            var (link2, acc2, existed2) = store.Provisioning.LoginOrLink(identity, Secrets.Sha256Hex(Secrets.NewToken("devk_")), "d2");
            ServerApp.Assert(existed2 == false, "RACE-6: unlinked identity reads as unknown (documented v2 behavior)");
            ServerApp.Assert(acc2.AccountId != acc1.AccountId, "RACE-6: re-login lands on a fresh account");
            ServerApp.Assert(store.Db.GetLink(link.LinkId)!.Status == "Unlinked", "RACE-6: old row stays Unlinked");
            ServerApp.Assert(store.Db.ActiveLinkCount(acc2.AccountId) == 1, "RACE-6: new account holds exactly one active link");
            ServerApp.Assert(store.Scalar("SELECT COUNT(*) FROM AccountProviderLink WHERE Status='Active'") == "1",
                "RACE-6: unique anchor holds over active rows");
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G9 — rate limiting (fresh process; the group IS the window)
    // ─────────────────────────────────────────────────────────────────────────

    public static void RateLimits(Runner r)
    {
        r.Group("G9 rate limiting", configureGitHub: true, budget: 11, ctx =>
        {
            var app = ctx.App;

            r.Run("RL-1 auth-start fixed window: 10 pass, 11th → 429 envelope (server.rate_limited)", () =>
            {
                for (var i = 0; i < 10; i++)
                    ServerApp.ExpectStatus(
                        app.Post("/v1/auth/nvidia/start", new { deviceName = "d", deviceKey = Key(256) }), 501, $"RL-1/{i}");
                var resp = app.Post("/v1/auth/nvidia/start", new { deviceName = "d", deviceKey = Key(256) });
                // The 429 now speaks the §7.1 envelope (was an empty body) —
                // errorCode server.rate_limited, retryable=true per §7.2.
                ServerApp.ExpectErr(resp, 429, "server.rate_limited", "RL-1/11th");
            });

            r.Run("RL-2 limiter is per-policy: healthz unaffected by auth-start exhaustion", () =>
                ServerApp.ExpectStatus(app.Get("/healthz"), 200, "RL-2"));

            r.Run("RL-3 untagged endpoints have NO api limiter: 260 hits, zero 429 — MISMATCH M-7 (unfixed)", () =>
            {
                var bad = "duluka_st_" + Secrets.Base64Url(new byte[32]);
                for (var i = 0; i < 260; i++)
                {
                    var resp = app.Get("/v1/account/me", bad);
                    if (resp.Status == 429)
                    {
                        r.Mismatch("M-7", "api limiter appeared (240/min) — the C/6 claim is now true; update M-7 and unpin RL-3");
                        return;
                    }
                    ServerApp.ExpectStatus(resp, 401, $"RL-3/{i}");
                }
                r.Mismatch("M-7", "260 consecutive /me hits → zero 429 (C/6 claims 240/min on the authenticated API)");
            });

            r.Run("RL-4 log sweep G9", () => app.Sweep("RL-4"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G10 — restart behavior (deterministic: same SQLite file, new process)
    // ─────────────────────────────────────────────────────────────────────────

    public static void Restart(Runner r)
    {
        r.Group("G10 restart persistence", configureGitHub: true, budget: 2, ctx =>
        {
            var app = ctx.App;
            var accA = app.SeedAccount("alice");
            var (devA1, _) = app.SeedDevice(accA, "alice-pc");
            var (devA2, _) = app.SeedDevice(accA, "alice-laptop");
            var (devR, _) = app.SeedDevice(accA, "doomed");

            var t0 = DateTimeOffset.UtcNow;
            var tA1 = app.SeedSession(accA, devA1, null,
                expires: t0 + TimeSpan.FromDays(2), createdAt: t0 - TimeSpan.FromDays(26)); // cap probe
            var tA2 = app.SeedSession(accA, devA2, null);   // revoked pre-restart
            var tR = app.SeedSession(accA, devR, null);     // dies with its device

            r.Run("RE-1 ready pre-restart (schema=2)", () =>
                ServerApp.ExpectStatus(app.Get("/healthz/ready"), 200, "RE-1"));

            r.Run("RE-2 refresh pre-restart → extends to the absolute cap exactly", () =>
            {
                var json = ServerApp.Json(
                    ServerApp.ExpectStatus(app.Post("/v1/auth/session/refresh", bearer: tA1), 200, "RE-2"));
                var got = DateTimeOffset.Parse(json.GetProperty("sessionExpiresAt").GetString()!);
                ServerApp.Assert(got == t0 + TimeSpan.FromDays(4), $"RE-2: expected cap {t0 + TimeSpan.FromDays(4):O}, got {got:O}");
            });

            var flowState = "";
            r.Run("RE-3 start a login flow pre-restart (state lives in memory)", () =>
            {
                var start = ServerApp.ExpectStatus(
                    app.Post("/v1/auth/github/start", new { deviceName = "d", deviceKey = Key(256) }), 200, "RE-3");
                flowState = ServerApp.Prop(start, "state");
            });

            r.Run("RE-4 revoke session T_A2 pre-restart", () =>
                ServerApp.ExpectOk(app.Post("/v1/auth/session/revoke", bearer: tA2), "RE-4"));

            r.Run("RE-5 device revoke D_R pre-restart (kills T_R)", () =>
                ServerApp.ExpectStatus(app.Post($"/v1/account/devices/{devR}/revoke", bearer: tA1), 200, "RE-5"));

            ctx.Restart();
            app = ctx.App;

            r.Run("RE-6 ready post-restart (schema=2)", () =>
                ServerApp.ExpectStatus(app.Get("/healthz/ready"), 200, "RE-6"));

            r.Run("RE-7 OAuth flow state does NOT survive restart → 400 invalid_state (documented v0 design)", () =>
                ServerApp.ExpectErr(app.Post("/v1/auth/github/callback", new { code = "c", state = flowState, deviceKey = Key(256) }),
                    400, "invalid_state", "RE-7"));

            r.Run("RE-8 session survives restart (persisted, still valid)", () =>
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA1), "RE-8"));

            r.Run("RE-9 refresh post-restart returns the EXACT same capped expiry (persisted)", () =>
            {
                var json = ServerApp.Json(
                    ServerApp.ExpectStatus(app.Post("/v1/auth/session/refresh", bearer: tA1), 200, "RE-9"));
                var got = DateTimeOffset.Parse(json.GetProperty("sessionExpiresAt").GetString()!);
                ServerApp.Assert(got == t0 + TimeSpan.FromDays(4), $"RE-9: expected persisted cap {t0 + TimeSpan.FromDays(4):O}, got {got:O}");
            });

            r.Run("RE-10 session revocation survives restart", () =>
                ServerApp.ExpectErr(app.Get("/v1/account/me", tA2), 401, "auth.session_revoked", "RE-10"));

            r.Run("RE-11 device revocation survives restart (its session stays dead)", () =>
                ServerApp.ExpectErr(app.Get("/v1/account/me", tR), 401, "auth.session_revoked", "RE-11"));

            r.Run("RE-12 device list post-restart shows RevokedAt persisted", () =>
            {
                var arr = ServerApp.Json(app.Get("/v1/account/devices", tA1)).GetProperty("devices");
                var doomed = arr.EnumerateArray().Single(d => d.GetProperty("deviceId").GetString() == devR);
                ServerApp.Assert(doomed.GetProperty("revokedAt").ValueKind == JsonValueKind.String, "RE-12: RevokedAt must be persisted");
                ServerApp.Assert(arr.GetArrayLength() == 3, "RE-12: all three devices listed");
            });

            r.Run("RE-13 SchemaHistory at current schema (v2 ran; v3 = native credentials, additive)", () =>
            {
                var v = app.RawScalar("SELECT MAX(Version) FROM SchemaHistory");
                ServerApp.Assert(int.Parse(v) >= 2 && int.Parse(v) == Database.SchemaVersion,
                    $"RE-13: SchemaHistory must hold the current schema version ({Database.SchemaVersion}), got {v}");
            });

            r.Run("RE-14 log sweep G10 (both process generations)", () => app.Sweep("RE-14"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G11 — cross-cutting envelope + contract compliance sweep
    // ─────────────────────────────────────────────────────────────────────────

    public static void ContractSweep(Runner r)
    {
        r.Group("G11 envelope + contract sweep", configureGitHub: true, budget: 9, ctx =>
        {
            var app = ctx.App;
            var accA = app.SeedAccount("alice");
            var linkA1 = app.SeedLink(accA, "777");
            var (devA1, _) = app.SeedDevice(accA, "alice-pc");
            var tA1 = app.SeedSession(accA, devA1, linkA1);

            r.Run("ENV-1 error envelope: §7.1 shape, registry codes, no internals over a 9-case corpus", () =>
            {
                var corpus = new (string Label, Resp Resp)[]
                {
                    ("me/unknown", app.Get("/v1/account/me", "duluka_st_" + Secrets.Base64Url(new byte[32]))),
                    ("refresh/no-token", app.Post("/v1/auth/session/refresh")),
                    ("start/missing-name", app.Post("/v1/auth/github/start", new { deviceKey = Key(256) })),
                    ("callback/unknown-state", app.Post("/v1/auth/github/callback", new { code = "c", state = "x", deviceKey = Key(256) })),
                    ("link-start/nvidia", app.Post("/v1/account/providers", new { provider = "nvidia" }, tA1)),
                    ("unlink/unknown", app.Delete("/v1/account/providers/duluka_link_unknown", tA1)),
                    ("device-revoke/unknown", app.Post("/v1/account/devices/duluka_dev_unknown/revoke", bearer: tA1)),
                    ("last-provider", app.Delete($"/v1/account/providers/{linkA1}", tA1)),
                    ("start/malformed-json", app.PostRaw("/v1/auth/github/start", "not json")),
                    ("complete/malformed-json", app.PostRaw("/v1/account/providers/github/complete", "not json")),
                };
                var expected = new Dictionary<string, (int Status, string? Code)>
                {
                    ["me/unknown"] = (401, "auth.session_expired"),
                    ["refresh/no-token"] = (401, "auth.session_expired"),
                    ["start/missing-name"] = (400, "invalid_device_name"),
                    ["callback/unknown-state"] = (400, "invalid_state"),
                    ["link-start/nvidia"] = (501, "provider_reserved"),
                    ["unlink/unknown"] = (404, "nf.link"),
                    ["device-revoke/unknown"] = (404, "nf.device"),
                    ["last-provider"] = (409, "conflict.link_conflict"),
                    ["start/malformed-json"] = (400, "bad_request"), // M-1 fixed
                    ["complete/malformed-json"] = (400, "bad_request"), // M-1 fixed
                };
                foreach (var (label, resp) in corpus)
                {
                    var (status, code) = expected[label];
                    ServerApp.ExpectStatus(resp, status, $"ENV-1/{label}");
                    if (code is null)
                        ServerApp.Assert(resp.Body.Length == 0, $"ENV-1/{label}: known naked error must have empty body");
                    else
                        ServerApp.ExpectErr(resp, status, code, $"ENV-1/{label}");
                    if (resp.Body.Length > 0)
                        ServerApp.Assert(
                            !resp.Body.Contains("Exception") && !resp.Body.Contains("at Program") && !resp.Body.Contains("at System."),
                            $"ENV-1/{label}: no internals may leak into error bodies: {ServerApp.Trunc(resp.Body)}");
                }
            });

            r.Run("ENV-2 envelope carries the full §7.1 shape (M-4 FIXED)", () =>
            {
                // M-4 was: {error:{code,message}} with no ok/reqId/errorCode/
                // httpStatus/retryable/conflict. Every ExpectErr in this suite
                // now asserts the full §7.1 envelope; this test pins the
                // success side too (ok=true + resource nesting + reqId echo).
                ServerApp.ExpectOk(app.Get("/v1/account/me", tA1), "ENV-2/success");
            });

            r.Run("ENV-3 409s carry conflict=null — MISMATCH M-10 (new; §6.2 wants the current resource attached)", () =>
            {
                var resp = app.Delete($"/v1/account/providers/{linkA1}", tA1); // last-provider 409
                ServerApp.ExpectErr(resp, 409, "conflict.link_conflict", "ENV-3");
                using var doc = JsonDocument.Parse(resp.Body);
                var conflict = doc.RootElement.GetProperty("conflict");
                ServerApp.Assert(conflict.ValueKind == JsonValueKind.Null,
                    "ENV-3: conflict became populated — §6.2 attachment implemented; update M-10");
                r.Mismatch("M-10", "409 conflict.link_conflict carries conflict=null (§6.2: the CURRENT server resource + version must be attached so the client can re-apply or drop)");
            });

            r.Run("XREQ-1 X-ReqId echoed verbatim on error and success paths", () =>
            {
                var rid = "itest-fixed-" + Guid.NewGuid().ToString("N")[..8];
                var err = app.Get("/v1/account/me", "duluka_st_" + Secrets.Base64Url(new byte[32]), reqId: rid);
                ServerApp.ExpectErr(err, 401, "auth.session_expired", "XREQ-1/error");
                ServerApp.Assert(err.ReqId == rid, "XREQ-1: request carried the fixed id");
                var okResp = app.Get("/healthz", reqId: rid + "b");
                ServerApp.Assert(okResp.ReqId == rid + "b", "XREQ-1: success request carried its id");
            });

            r.Run("PRV-0 auth gates on link flow start (uniform 401 codes)", () =>
            {
                ServerApp.ExpectErr(app.Post("/v1/account/providers", new { provider = "github" }), 401, "auth.session_expired", "PRV-0/no-token");
                ServerApp.ExpectErr(app.Post("/v1/account/providers", new { provider = "github" }, "duluka_st_" + Secrets.Base64Url(new byte[32])),
                    401, "auth.session_expired", "PRV-0/bad-token");
                ServerApp.ExpectErr(app.Post("/v1/account/providers", new { provider = "nvidia" }, tA1), 501, "provider_reserved", "PRV-0/nvidia");
            });

            r.Run("COMP-1 401 matrix: every authenticated surface answers 401 auth.session_expired for foreign tokens", () =>
            {
                var bad = "duluka_st_" + Secrets.Base64Url(new byte[32]);
                var probes = new (string Label, Func<Resp> Call)[]
                {
                    ("GET /me", () => app.Get("/v1/account/me", bad)),
                    ("GET /providers", () => app.Get("/v1/account/providers", bad)),
                    ("GET /devices", () => app.Get("/v1/account/devices", bad)),
                    ("POST /refresh", () => app.Post("/v1/auth/session/refresh", bearer: bad)),
                    ("POST /revoke", () => app.Post("/v1/auth/session/revoke", bearer: bad)),
                    ("POST /revoke-all", () => app.Post("/v1/auth/sessions/revoke-all", bearer: bad)),
                    ("POST /link-start", () => app.Post("/v1/account/providers", new { provider = "github" }, bad)),
                    ("POST /device-revoke", () => app.Post("/v1/account/devices/duluka_dev_x/revoke", bad)),
                    ("DELETE /unlink", () => app.Delete("/v1/account/providers/duluka_link_x", bad)),
                };
                foreach (var (label, call) in probes)
                    ServerApp.ExpectErr(call(), 401, "auth.session_expired", $"COMP-1/{label}");
            });

            r.Run("COMP-2 sync surface tripwire: /v1/sync* → 404 (absent; 428/409-stale untestable until it ships)", () =>
            {
                foreach (var (method, path) in new[] { ("GET", "/v1/sync"), ("PUT", "/v1/sync"), ("GET", "/v1/sync/shadowplay") })
                {
                    var resp = app.Send(method, path, method == "PUT" ? "\"doc\"" : null, tA1);
                    ServerApp.ExpectStatus(resp, 404, $"COMP-2/{method} {path}");
                    ServerApp.Assert(resp.Body.Length == 0, $"COMP-2/{path}: absent-route body pin");
                }
            });

            r.Run("ENV-4 log sweep G11", () => app.Sweep("ENV-4"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G12 — native username/password auth (register / login / password change).
    // Every error path must speak the §7.1 envelope; invalid_credentials must
    // be GENERIC (unknown username and wrong password are indistinguishable);
    // passwords and tokens must never reach the logs.
    // Auth-start budget: register/login are auth-start-limited; malformed-JSON
    // probes also cross the limiter, so the budget covers every call below.
    // ─────────────────────────────────────────────────────────────────────────

    public static void NativeAuth(Runner r)
    {
        r.Group("G12 native username/password auth", configureGitHub: false, budget: 13, ctx =>
        {
            var app = ctx.App;
            const string password = "native-pass-123";
            const string newPassword = "rotated-pass-456";
            var loginToken = "";

            r.Run("NAT-1 register → 200 envelope, session shape, username echoed", () =>
            {
                app.TrackSecret(password);
                var resp = app.Post("/v1/auth/register", new
                {
                    username = "ada",
                    password,
                    deviceKey = Key(256),
                    deviceName = "it-native-dev",
                });
                ServerApp.ExpectOk(resp, "NAT-1");
                var res = ServerApp.Json(resp);
                ServerApp.Assert(res.GetProperty("sessionToken").GetString()!.StartsWith("duluka_st_"),
                    "NAT-1: sessionToken must be a Duluka session token");
                ServerApp.Assert(res.GetProperty("accountId").GetString()!.StartsWith("duluka_acc_"),
                    "NAT-1: accountId must be a Duluka account id");
                ServerApp.Assert(res.GetProperty("deviceId").GetString()!.StartsWith("duluka_dev_"),
                    "NAT-1: deviceId must be a Duluka device id");
                ServerApp.Assert(res.GetProperty("username").GetString() == "ada",
                    "NAT-1: username echoed as registered");
                ServerApp.Assert(!string.IsNullOrEmpty(res.GetProperty("sessionExpiresAt").GetString()),
                    "NAT-1: sessionExpiresAt present");
                // NB: the sessionToken is NOT tracked as a sweep secret — the
                // issuing response is its one sanctioned appearance ("shown to
                // the client EXACTLY ONCE"). Passwords are tracked: they must
                // never appear in any response or log.
            });

            r.Run("NAT-2 duplicate username (case-insensitive) → 409 conflict.username_taken", () =>
            {
                var resp = app.Post("/v1/auth/register", new
                {
                    username = "ADA",
                    password,
                    deviceKey = Key(256),
                    deviceName = "it-native-dev",
                });
                ServerApp.ExpectErr(resp, 409, "conflict.username_taken", "NAT-2");
            });

            r.Run("NAT-3 register field validation → 400 invalid_username / invalid_password / invalid_device_key", () =>
            {
                ServerApp.ExpectErr(app.Post("/v1/auth/register", new
                {
                    username = "bad name!", password, deviceKey = Key(256), deviceName = "d",
                }), 400, "invalid_username", "NAT-3a");

                ServerApp.ExpectErr(app.Post("/v1/auth/register", new
                {
                    username = "shortpass", password = "short", deviceKey = Key(256), deviceName = "d",
                }), 400, "invalid_password", "NAT-3b");

                ServerApp.ExpectErr(app.Post("/v1/auth/register", new
                {
                    username = "shortkey", password, deviceKey = "too-short", deviceName = "d",
                }), 400, "invalid_device_key", "NAT-3c");
            });

            r.Run("NAT-4 login success → session works on /me (username present)", () =>
            {
                var resp = app.Post("/v1/auth/login", new
                {
                    username = "ada",
                    password,
                    deviceKey = Key(256),
                    deviceName = "it-native-dev-2",
                });
                ServerApp.ExpectOk(resp, "NAT-4");
                var res = ServerApp.Json(resp);
                loginToken = res.GetProperty("sessionToken").GetString()!;

                var me = ServerApp.Json(app.Get("/v1/account/me", bearer: loginToken));
                ServerApp.Assert(me.GetProperty("username").GetString() == "ada",
                    "NAT-4: /me must surface the native username");
                ServerApp.Assert(me.GetProperty("accountId").GetString()!.StartsWith("duluka_acc_"),
                    "NAT-4: same account identity");
            });

            r.Run("NAT-5 login failures are GENERIC — wrong password and unknown username share invalid_credentials", () =>
            {
                var wrongPw = app.Post("/v1/auth/login", new
                {
                    username = "ada", password = "definitely-wrong-1", deviceKey = Key(256), deviceName = "d",
                });
                var (codePw, _) = ServerApp.ExpectErr(wrongPw, 401, "invalid_credentials", "NAT-5a");

                var unknown = app.Post("/v1/auth/login", new
                {
                    username = "no-such-user", password = "whatever-pass-1", deviceKey = Key(256), deviceName = "d",
                });
                var (codeUn, msgUn) = ServerApp.ExpectErr(unknown, 401, "invalid_credentials", "NAT-5b");
                ServerApp.Assert(codePw == codeUn, "NAT-5: identical errorCode for both failure kinds");
                ServerApp.Assert(!msgUn.Contains("no-such-user", StringComparison.OrdinalIgnoreCase),
                    "NAT-5: failure message must not echo the username");
            });

            r.Run("NAT-6 malformed JSON on the native endpoints → 400 bad_request envelope (M-1 intact)", () =>
            {
                ServerApp.ExpectErr(app.PostRaw("/v1/auth/register", "{\"username\": \"ada\", \"pass"),
                    400, "bad_request", "NAT-6a");
                ServerApp.ExpectErr(app.PostRaw("/v1/auth/login", ""),
                    400, "bad_request", "NAT-6b");
            });

            r.Run("NAT-7 log sweep G12 (passwords never in server logs or bodies)", () => app.Sweep("NAT-7"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G13 — native password rotation. Separate group = fresh server + fresh
    // auth-start window (the shared 10/min limiter is per process; G12 already
    // spends its budget on register/login validation).
    // ─────────────────────────────────────────────────────────────────────────

    public static void NativePasswordRotation(Runner r)
    {
        r.Group("G13 native password rotation", configureGitHub: false, budget: 5, ctx =>
        {
            var app = ctx.App;
            const string password = "native-pass-123";
            const string newPassword = "rotated-pass-456";
            var loginToken = "";

            r.Run("ROT-1 seed: register + login (auth-start ×2)", () =>
            {
                app.TrackSecret(password);
                app.TrackSecret(newPassword);
                ServerApp.ExpectOk(app.Post("/v1/auth/register", new
                {
                    username = "rotation-user",
                    password,
                    deviceKey = Key(256),
                    deviceName = "it-rot",
                }), "ROT-1a");
                var login = ServerApp.Json(app.Post("/v1/auth/login", new
                {
                    username = "rotation-user",
                    password,
                    deviceKey = Key(256),
                    deviceName = "it-rot-2",
                }));
                loginToken = login.GetProperty("sessionToken").GetString()!;
            });

            r.Run("ROT-2 wrong current password → 400 invalid_credentials (session alone is not enough)", () =>
            {
                ServerApp.ExpectErr(app.Post("/v1/account/password",
                    new { currentPassword = "not-the-current-1", newPassword }, bearer: loginToken),
                    400, "invalid_credentials", "ROT-2");
            });

            r.Run("ROT-3 correct rotation → 200; old password dies, new password logs in", () =>
            {
                ServerApp.ExpectOk(app.Post("/v1/account/password",
                    new { currentPassword = password, newPassword }, bearer: loginToken), "ROT-3a");

                ServerApp.ExpectOk(app.Post("/v1/auth/login", new
                {
                    username = "rotation-user", password = newPassword, deviceKey = Key(256), deviceName = "d",
                }), "ROT-3b");

                ServerApp.ExpectErr(app.Post("/v1/auth/login", new
                {
                    username = "rotation-user", password, deviceKey = Key(256), deviceName = "d",
                }), 401, "invalid_credentials", "ROT-3c");
            });

            r.Run("ROT-4 log sweep G13 (both passwords never in server logs or bodies)", () => app.Sweep("ROT-4"));
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // G14 — provider-only (bootstrapped) account adopts a password; releases
    // the last-provider trap. Own server process: G12's auth-start window is
    // already spent at the real 10/min limiter and adoption's login probe
    // needs a fresh one.
    // ─────────────────────────────────────────────────────────────────────────

    public static void NativeAdoption(Runner r)
    {
        r.Group("G14 provider-only password adoption", configureGitHub: false, budget: 4, ctx =>
        {
            var app = ctx.App;
            app.TrackSecret("adopted-pass-1");

            r.Run("ADOPT-1 provider-only account adopts username+password (no current password needed)", () =>
            {
                // Bootstrapped account: GitHub link seeded, NO native credential.
                var accountId = app.SeedAccount("bootstrapped-user");
                app.SeedLink(accountId, "gh-adopt-1");
                var (deviceId, _) = app.SeedDevice(accountId, "it-adopt");
                var token = app.SeedSession(accountId, deviceId, null);
                app.TrackSecret(token);

                var resp = app.Post("/v1/account/password", new
                {
                    username = "adopter",
                    newPassword = "adopted-pass-1",
                }, bearer: token);
                ServerApp.ExpectOk(resp, "ADOPT-1a");
                ServerApp.Assert(ServerApp.Json(resp).GetProperty("username").GetString() == "adopter",
                    "ADOPT-1a: response echoes the adopted username");

                // After adoption the account HAS a credential — the same
                // endpoint now requires the current password (change path).
                var changeWithoutCurrent = app.Post("/v1/account/password", new
                {
                    username = "adopter",
                    newPassword = "adopted-pass-2",
                }, bearer: token);
                ServerApp.ExpectErr(changeWithoutCurrent, 400, "invalid_credentials", "ADOPT-1b");

                // A DIFFERENT provider-only account trying to adopt a taken
                // username is refused with 409.
                var otherId = app.SeedAccount("other-bootstrapped");
                app.SeedLink(otherId, "gh-adopt-2");
                var (otherDev, _) = app.SeedDevice(otherId, "it-adopt-2");
                var otherToken = app.SeedSession(otherId, otherDev, null);
                var dup = app.Post("/v1/account/password", new
                {
                    username = "adopter",
                    newPassword = "adopted-pass-1",
                }, bearer: otherToken);
                ServerApp.ExpectErr(dup, 409, "conflict.username_taken", "ADOPT-1c");
            });

            r.Run("ADOPT-2 adopted account: password login works + last-provider trap released", () =>
            {
                var login = ServerApp.Json(app.Post("/v1/auth/login", new
                {
                    username = "adopter", password = "adopted-pass-1", deviceKey = Key(256), deviceName = "d",
                }));
                // NB: the sessionToken is NOT tracked — its issuing response is
                // the one sanctioned appearance ("shown EXACTLY ONCE").
                var token = login.GetProperty("sessionToken").GetString()!;

                var links = ServerApp.Json(app.Get("/v1/account/providers", bearer: token));
                var linkId = links.GetProperty("providers").EnumerateArray().Single().GetProperty("linkId").GetString()!;

                // With a password present, the ONLY provider link is unlinkable.
                var un = ServerApp.Json(app.Delete("/v1/account/providers/" + linkId, bearer: token));
                ServerApp.Assert(un.GetProperty("unlinked").GetBoolean(), "ADOPT-2: unlink must succeed");
            });

            r.Run("ADOPT-3 log sweep G14 (adopted password never in server logs or bodies)", () => app.Sweep("ADOPT-3"));
        });
    }
}
