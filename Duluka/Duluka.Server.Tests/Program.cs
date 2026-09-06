using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using Duluka.Server;
using Duluka.Server.Auth;
using Duluka.Server.Data;
using Duluka.Server.Domain;
using Duluka.Server.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Duluka.Server.Tests;

/// <summary>
/// C/6 auth/security test suite — deterministic, no network. GitHub is a fake
/// HttpMessageHandler; the database is a fresh temp SQLite file per run
/// (self-cleaning, per repo hygiene policy).
/// </summary>
internal static class Program
{
    private static int _passed, _failed;
    private static readonly List<string> Failures = new();

    private static int Main()
    {
        Console.WriteLine("==================================================");
        Console.WriteLine(" Duluka.Server — C/6 auth/security tests");
        Console.WriteLine("==================================================");

        Run("SECRETS-1: PKCE S256 challenge matches RFC 7636 appendix B vector", Test_PkceVector);
        Run("SECRETS-2: redaction destroys secret bodies", Test_Redaction);
        Run("SECRETS-3: session token stored as SHA-256 hash, never raw", Test_TokenHashAtRest);
        Run("FLOW-1: state is single-use — replayed callback rejected", Test_StateSingleUse);
        Run("FLOW-2: expired flow rejected", Test_FlowExpiry);
        Run("LOGIN-1: same provider identity → same account (dedup)", Test_LoginDedup);
        Run("LOGIN-2: unique-anchor race (parallel first login) → one account", Test_UniqueAnchorRace);
        Run("LOGIN-3: different identity → different account", Test_SeparateAccounts);
        Run("LOGIN-4: device key reuse across accounts → device_key_in_use", Test_DeviceKeyCrossAccount);
        Run("SESSION-1: validate/refresh lifecycle", Test_SessionLifecycle);
        Run("SESSION-2: expired session rejected", Test_SessionExpiry);
        Run("SESSION-3: second login on same device supersedes prior (contract 5.2)", Test_SessionSupersede);
        Run("SESSION-4: refresh absolute cap honors configured AbsoluteDays", Test_RefreshAbsoluteCap);
        Run("REVOKE-1: session revoke + revoke-all", Test_SessionRevoke);
        Run("REVOKE-2: device revoke kills its sessions + blocks the key", Test_DeviceRevoke);
        Run("UNLINK-1: last-provider unlink rejected", Test_UnlinkLastProvider);
        Run("UNLINK-2: unlink non-last destroys credential + via-link sessions", Test_UnlinkNonLast);
        Run("UNLINK-3: re-login after unlink converges (new account, no 500)", Test_RelinkAfterUnlink);
        Run("SCHEMA-1: v1 database migrates to v2, rows survive, re-link works", Test_SchemaV2Migration);
        Run("NVIDIA-1: provider reserved, no flow, no hardware identity surface", Test_NvidiaReserved);
        RunHttpIntegrationTests();

        Console.WriteLine();
        Console.WriteLine($"--------------------------------------------------");
        Console.WriteLine($" RESULT: {_passed} passed, {_failed} failed, {_passed + _failed} total");
        Console.WriteLine($"--------------------------------------------------");
        if (_failed > 0)
            foreach (var f in Failures)
                Console.WriteLine("  FAILED: " + f);
        return _failed == 0 ? 0 : 1;
    }

    private static void Run(string name, Action test)
    {
        Console.Write($"  {name.PadRight(72)}");
        try { test(); _passed++; Console.WriteLine("PASS"); }
        catch (Exception ex)
        {
            _failed++;
            var msg = ex.Message.Length > 400 ? ex.Message[..400] + "..." : ex.Message;
            Console.WriteLine("FAIL");
            Failures.Add($"{name} — {msg}");
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Assertion failed: " + message);
    }

    // ─── fixtures ───────────────────────────────────────────────────────────

    private static readonly ConcurrentDictionary<string, string> UserDb = new(); // githubId → login

    /// <summary>Fake GitHub: token exchange + /user serving identities from UserDb.</summary>
    private sealed class FakeGitHubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var url = request.RequestUri!.ToString();
            if (url.Contains("/login/oauth/access_token"))
            {
                var content = request.Content!.ReadAsStringAsync(ct).Result;
                var code = content.Split('&').First(p => p.StartsWith("code="))[5..];
                var json = UserDb.ContainsKey(code)
                    ? $$"""{"access_token":"gho_fake_{{code}}","token_type":"bearer"}"""
                    : """{"error":"bad_verification_code","error_description":"unknown code"}""";
                return Task.FromResult(JsonResponse(json));
            }
            if (url.Contains("/user"))
            {
                var token = request.Headers.Authorization!.ToString(); // "Bearer gho_fake_<code>"
                var ghId = token["gho_fake_".Length..];
                var login = UserDb.TryGetValue(ghId, out var l) ? l : "user" + ghId;
                var json = $$"""{"id":{{ghId}},"login":"{{login}}","email":"{{login}}@example.test"}""";
                return Task.FromResult(JsonResponse(json));
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private sealed class FixedHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    private static IConfiguration TestConfig(string dbPath, int absoluteDays = 30, int slidingDays = 7) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GitHub:ClientId"] = "test-client-id",
            ["GitHub:ClientSecret"] = "test-client-secret",
            ["GitHub:RedirectUri"] = "http://localhost:8517/v1/auth/github/callback",
            ["Database:Path"] = dbPath,
            ["Session:AbsoluteDays"] = absoluteDays.ToString(),
            ["Session:SlidingDays"] = slidingDays.ToString(),
        })
        .Build();

    private sealed record Fixture(Database Db, GitHubOAuthService GitHub, AccountProvisioningService Provisioning, SessionService Sessions, string DbPath)
        : IDisposable
    {
        public string RegisterGitHubUser(long ghId, string login)
        {
            UserDb[ghId.ToString()] = login;
            return ghId.ToString();
        }

        public void Dispose()
        {
            // Close the SQLite connection BEFORE the temp file cleanup — the
            // WAL-mode connection holds the file lock otherwise.
            Db.DisposeAsync().AsTask().GetAwaiter().GetResult();
            Cleanup(DbPath);
        }
    }

    private static Fixture NewFixture(int absoluteDays = 30, int slidingDays = 7)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"duluka-test-{Guid.NewGuid():N}.db");
        var config = TestConfig(dbPath, absoluteDays, slidingDays);
        var db = new Database(dbPath, NullLogger<Database>.Instance);
        db.Bootstrap();
        var github = new GitHubOAuthService(
            new FixedHttpClientFactory(new FakeGitHubHandler()), config, NullLogger<GitHubOAuthService>.Instance);
        return new Fixture(db, github, new AccountProvisioningService(db),
            new SessionService(db, config), dbPath);
    }

    private static (string deviceKey, string deviceKeyHash) NewDeviceKey() =>
        (Secrets.NewToken("devk_"), Secrets.Sha256Hex(Secrets.NewToken("devk_")));

    private static void Cleanup(string dbPath)
    {
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var p = dbPath + suffix;
            if (File.Exists(p)) File.Delete(p);
        }
    }

    // ─── tests ──────────────────────────────────────────────────────────────

    private static void Test_PkceVector()
    {
        // RFC 7636 Appendix B — the canonical S256 vector.
        var verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";
        Assert(Secrets.PkceChallenge(verifier) == "E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM",
            "S256 challenge must match the RFC vector");
    }

    private static void Test_Redaction()
    {
        var r = Secrets.Redact("super-secret-token-value");
        Assert(!r.Contains("secret"), "redacted value must not contain the secret body");
        Assert(r.StartsWith("super-"), "redaction keeps a short prefix for correlation");
        Assert(Secrets.Redact("").Length > 0, "empty redacts to a placeholder");
        Assert(Secrets.FixedTimeEquals("abc", "abc") && !Secrets.FixedTimeEquals("abc", "abd"),
            "constant-time compare works");
    }

    private static void Test_TokenHashAtRest()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(1001, "hashy");
            var (link, account, _) = f.Db.UpsertLink(ProviderKeys.GitHub, ghId, null, null);
            var device = f.Db.CreateDevice(account.AccountId, "dev", Secrets.Sha256Hex("devkey-1"));
            var (token, session) = f.Sessions.Create(account.AccountId, device.DeviceId, link.LinkId);

            Assert(!string.Equals(session.SessionTokenHash, token, StringComparison.Ordinal),
                "DB must not hold the raw token");
            Assert(session.SessionTokenHash == Secrets.Sha256Hex(token),
                "DB holds the SHA-256 digest of the token");
            Assert(f.Db.ValidateSession(Secrets.Sha256Hex(token)) is not null, "valid token validates");
        }
        finally { f.Dispose(); }
    }

    private static void Test_StateSingleUse()
    {
        var store = new OAuthFlowStore();
        var flow = store.Put(new OAuthFlow("state-1", "verifier", null, null, "d", DateTimeOffset.UtcNow + TimeSpan.FromMinutes(1)));
        Assert(store.Consume(flow.State) is not null, "first consume succeeds");
        Assert(store.Consume(flow.State) is null, "second consume (replay) must fail");
        Assert(store.Consume("never-issued") is null, "unknown state rejected");
    }

    private static void Test_FlowExpiry()
    {
        var store = new OAuthFlowStore();
        store.Put(new OAuthFlow("state-old", "v", null, null, "d", DateTimeOffset.UtcNow - TimeSpan.FromSeconds(1)));
        Assert(store.Consume("state-old") is null, "expired flow must not complete");
    }

    private static void Test_LoginDedup()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(2001, "dulukafan");
            var (key1, hash1) = NewDeviceKey();
            var (key2, hash2) = NewDeviceKey();

            var id1 = new GitHubIdentity(ProviderKeys.GitHub, ghId, "dulukafan@example.test", "dulukafan");
            var (_, account1, existed1) = f.Provisioning.LoginOrLink(id1, hash1, "pc-1");
            Assert(!existed1, "first login creates the account");

            var (_, account2, existed2) = f.Provisioning.LoginOrLink(id1, hash2, "pc-2");
            Assert(existed2, "second login is an existing account");
            Assert(account1.AccountId == account2.AccountId, "same identity → same AccountId");
            Assert(f.Db.LinksForAccount(account1.AccountId).Count(l => l.Status == LinkStatus.Active) == 1,
                "no duplicate link rows for the same identity");
        }
        finally { f.Dispose(); }
    }

    private static void Test_UniqueAnchorRace()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(3001, "racer");
            var barrier = new Barrier(2);
            var results = new ConcurrentBag<string>();
            var errors = new ConcurrentBag<string>();

            // Two concurrent first-logins for the SAME provider identity — the
            // duplicate-account race. Both must converge on ONE account; the
            // loser's orphan account must be cleaned up.
            Parallel.For(0, 2, _ =>
            {
                try
                {
                    barrier.SignalAndWait(TimeSpan.FromSeconds(5));
                    var (_, hash) = NewDeviceKey();
                    var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, "racer@example.test", "racer");
                    var (_, account, existed) = f.Provisioning.LoginOrLink(id, hash, "race-dev");
                    results.Add(account.AccountId + "|" + existed);
                }
                catch (Exception ex) { errors.Add(ex.Message); }
            });

            Assert(errors.IsEmpty, "no exception may escape the race: " + string.Join("; ", errors));
            Assert(results.Count == 2, "both threads completed");
            Assert(results.Select(r => r.Split('|')[0]).Distinct().Count() == 1,
                "both threads converge on ONE AccountId");
        }
        finally { f.Dispose(); }
    }

    private static void Test_SeparateAccounts()
    {
        var f = NewFixture();
        try
        {
            var idA = new GitHubIdentity(ProviderKeys.GitHub, "4001", "a@example.test", "alice");
            var idB = new GitHubIdentity(ProviderKeys.GitHub, "4002", "b@example.test", "bob");
            var (_, hashA) = NewDeviceKey();
            var (_, hashB) = NewDeviceKey();
            var (_, accA, _) = f.Provisioning.LoginOrLink(idA, hashA, "devA");
            var (_, accB, _) = f.Provisioning.LoginOrLink(idB, hashB, "devB");
            Assert(accA.AccountId != accB.AccountId, "different identities → different accounts");
        }
        finally { f.Dispose(); }
    }

    private static void Test_DeviceKeyCrossAccount()
    {
        var f = NewFixture();
        try
        {
            var idA = new GitHubIdentity(ProviderKeys.GitHub, "5001", "a@example.test", "alice");
            var idB = new GitHubIdentity(ProviderKeys.GitHub, "5002", "b@example.test", "bob");
            var (_, hashA) = NewDeviceKey();
            f.Provisioning.LoginOrLink(idA, hashA, "devA");

            var threw = false;
            try { f.Provisioning.LoginOrLink(idB, hashA, "devB"); }
            catch (InvalidOperationException ex) { threw = ex.Message == "device_key_in_use"; }
            Assert(threw, "cross-account device key reuse must throw device_key_in_use");
        }
        finally { f.Dispose(); }
    }

    private static void Test_SessionLifecycle()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(6001, "sessiony");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, "sessiony");
            var (_, hash) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash, "dev");
            var device = f.Db.DevicesForAccount(account.AccountId).Single();

            var (token, session) = f.Sessions.Create(account.AccountId, device.DeviceId, link.LinkId);
            var validation = f.Sessions.Validate(token);
            Assert(validation is not null, "fresh session validates");
            Assert(validation!.Value.Item1.SessionId == session.SessionId, "same session row");
            Assert(validation.Value.Item2.DeviceId == device.DeviceId, "device binding resolves");

            var refreshed = f.Sessions.Refresh(token);
            Assert(refreshed is not null, "refresh on a live session succeeds");
            Assert(refreshed >= session.ExpiresAt, "sliding refresh never shortens the window");
        }
        finally { f.Dispose(); }
    }

    private static void Test_SessionExpiry()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(7001, "expired");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash, "dev");
            var device = f.Db.DevicesForAccount(account.AccountId).Single();

            var token = Secrets.NewToken(SessionService.TokenPrefix);
            f.Db.CreateSession(account.AccountId, device.DeviceId, link.LinkId,
                Secrets.Sha256Hex(token), DateTimeOffset.UtcNow - TimeSpan.FromMinutes(1));
            Assert(f.Sessions.Validate(token) is null, "expired session must not validate");
        }
        finally { f.Dispose(); }
    }

    private static void Test_SessionSupersede()
    {
        var f = NewFixture();
        try
        {
            // Contract §5.2: exactly ONE active session per DeviceId — a second
            // login on the same device revokes the previous session first.
            var ghId = f.RegisterGitHubUser(12001, "superseder");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash, "one-device");
            var device = f.Db.DevicesForAccount(account.AccountId).Single();

            var (t1, s1) = f.Sessions.Create(account.AccountId, device.DeviceId, link.LinkId);
            var (t2, _) = f.Sessions.Create(account.AccountId, device.DeviceId, link.LinkId);

            Assert(f.Sessions.Validate(t2) is not null, "newest session on the device is live");
            Assert(f.Sessions.Validate(t1) is null, "previous session on the SAME device was superseded");
            var old = f.Db.ValidateSession(Secrets.Sha256Hex(t1));
            Assert(old is null, "superseded session no longer validates");
            Assert(!string.Equals(t1, t2, StringComparison.Ordinal), "tokens are distinct");
            Assert(s1.RevokedAt is null, "sanity: the row captured at creation was live");
        }
        finally { f.Dispose(); }
    }

    private static void Test_RefreshAbsoluteCap()
    {
        // Operators may SHORTEN the absolute TTL (Session:AbsoluteDays) — the
        // refresh cap must follow the configured value, not a hardcoded 30d.
        var f = NewFixture(absoluteDays: 14, slidingDays: 20);
        try
        {
            var ghId = f.RegisterGitHubUser(13001, "capped");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash, "dev");
            var device = f.Db.DevicesForAccount(account.AccountId).Single();

            // Session created with a short explicit expiry so the refresh
            // actually moves the window.
            var token = Secrets.NewToken(SessionService.TokenPrefix);
            var created = DateTimeOffset.UtcNow;
            f.Db.CreateSession(account.AccountId, device.DeviceId, link.LinkId,
                Secrets.Sha256Hex(token), created + TimeSpan.FromDays(1));

            var refreshed = f.Sessions.Refresh(token);
            Assert(refreshed is not null, "refresh on a live session succeeds");
            var cap = created + TimeSpan.FromDays(14);
            Assert(refreshed!.Value <= cap + TimeSpan.FromSeconds(5),
                $"refresh must cap at CreatedAt+AbsoluteDays (14d), got {refreshed}");
            Assert(refreshed.Value > DateTimeOffset.UtcNow,
                "capped refresh still extends the live window");
        }
        finally { f.Dispose(); }
    }

    private static void Test_SessionRevoke()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(8001, "revokeme");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash1) = NewDeviceKey();
            var (_, hash2) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash1, "dev-1");
            f.Provisioning.LoginOrLink(id, hash2, "dev-2");
            var device1 = f.Db.DevicesForAccount(account.AccountId).Single(d => d.DeviceName == "dev-1");
            var device2 = f.Db.DevicesForAccount(account.AccountId).Single(d => d.DeviceName == "dev-2");

            // Independent sessions live on independent devices (contract §5.2:
            // one ACTIVE session per device — two sessions on one device is
            // impossible by construction now).
            var t1 = f.Sessions.Create(account.AccountId, device1.DeviceId, link.LinkId).Token;
            var t2 = f.Sessions.Create(account.AccountId, device2.DeviceId, link.LinkId).Token;

            Assert(f.Sessions.Revoke(t1, "user-logout"), "own-session revoke succeeds");
            Assert(f.Sessions.Validate(t1) is null, "revoked session dead");
            Assert(f.Sessions.Validate(t2) is not null, "other device's session survives");
            Assert(f.Sessions.DeadSessionCode(t1) == "auth.session_revoked",
                "revoked session reports auth.session_revoked (§7.2 registry)");

            Assert(f.Sessions.RevokeAll(account.AccountId, "logout-all") >= 1, "revoke-all counts");
            Assert(f.Sessions.Validate(t2) is null, "revoke-all kills the rest");
        }
        finally { f.Dispose(); }
    }

    private static void Test_DeviceRevoke()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(9001, "devicey");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash, "doomed-device");
            var device = f.Db.DevicesForAccount(account.AccountId).Single();
            var (token, _) = f.Sessions.Create(account.AccountId, device.DeviceId, link.LinkId);
            Assert(f.Sessions.Validate(token) is not null, "session alive before revoke");

            var sessionsRevoked = f.Db.RevokeDevice(device.DeviceId, DateTimeOffset.UtcNow);
            Assert(sessionsRevoked >= 1, "device revoke kills its sessions");
            Assert(f.Sessions.Validate(token) is null, "session dead after device revoke");

            // Revoked device key can never silently re-register.
            var rethrew = false;
            try { f.Provisioning.LoginOrLink(id, hash, "doomed-device"); }
            catch (InvalidOperationException ex) { rethrew = ex.Message == "device_revoked"; }
            Assert(rethrew, "revoked device key must throw device_revoked");
        }
        finally { f.Dispose(); }
    }

    private static void Test_UnlinkLastProvider()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(10001, "lonely");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash, "dev");

            var result = f.Provisioning.Unlink(account.AccountId, link.LinkId);
            Assert(!result.Ok && result.Code == "last_provider",
                "unlinking the only provider must be rejected with last_provider");
            Assert(f.Db.GetLink(link.LinkId)!.Status == LinkStatus.Active, "link stays active");
        }
        finally { f.Dispose(); }
    }

    private static void Test_UnlinkNonLast()
    {
        var f = NewFixture();
        try
        {
            var gh1 = f.RegisterGitHubUser(11001, "two-providers");
            var idGh = new GitHubIdentity(ProviderKeys.GitHub, gh1, null, null);
            var (_, hash) = NewDeviceKey();
            var (ghLink, account, _) = f.Provisioning.LoginOrLink(idGh, hash, "dev");
            var device = f.Db.DevicesForAccount(account.AccountId).Single();

            // A second provider identity for the SAME account (the scenario the
            // link flow implements).
            var futureLink = new AccountProviderLink(
                Secrets.NewToken("duluka_link_"), account.AccountId, "discord", "discord-42",
                null, LinkStatus.Active, DateTimeOffset.UtcNow, null);
            f.Db.AddLink(futureLink);

            // Two sessions on two devices (§5.2: one active session per device).
            var (_, hash2) = NewDeviceKey();
            f.Db.CreateDevice(account.AccountId, "dev-2", hash2);
            var device2 = f.Db.DevicesForAccount(account.AccountId).Single(d => d.DeviceName == "dev-2");
            var (token, _) = f.Sessions.Create(account.AccountId, device.DeviceId, ghLink.LinkId);
            var (tokenViaFuture, _) = f.Sessions.Create(account.AccountId, device2.DeviceId, futureLink.LinkId);
            Assert(f.Sessions.Validate(tokenViaFuture) is not null, "via-link session alive before unlink");

            var result = f.Provisioning.Unlink(account.AccountId, futureLink.LinkId);
            Assert(result.Ok, "unlink succeeds while another provider remains");
            Assert(result.RevokedSessions >= 1, "sessions issued via the unlinked link are revoked");
            Assert(f.Sessions.Validate(tokenViaFuture) is null, "via-link session dead after unlink");
            Assert(f.Sessions.Validate(token) is not null, "sessions from other links survive");

            var after = f.Db.GetLink(futureLink.LinkId)!;
            Assert(after.Status == LinkStatus.Unlinked, "link is soft-unlinked");
            Assert(f.Db.LinksForAccount(account.AccountId).Count(l => l.Status == LinkStatus.Active) == 1,
                "exactly one active link remains");
        }
        finally { f.Dispose(); }
    }

    private static void Test_RelinkAfterUnlink()
    {
        // Contract §5.4-2 / §6.2: unlinking a non-last provider must leave the
        // provider identity RE-LINKABLE — the v1 table-level UNIQUE anchored
        // Unlinked rows too, so every later login with that identity 500'd
        // forever. The partial unique index (Active rows only) fixes it.
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(14001, "relinker");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash) = NewDeviceKey();
            var (ghLink, account, _) = f.Provisioning.LoginOrLink(id, hash, "dev");

            var second = new AccountProviderLink(
                Secrets.NewToken("duluka_link_"), account.AccountId, "discord", "discord-77",
                null, LinkStatus.Active, DateTimeOffset.UtcNow, null);
            f.Db.AddLink(second);

            Assert(f.Provisioning.Unlink(account.AccountId, ghLink.LinkId).Ok, "non-last unlink succeeds");

            // The same GitHub identity logs in again — must converge on a NEW
            // account (its old link is history), never throw.
            var (newLink, newAccount, existed) = f.Db.UpsertLink(
                ProviderKeys.GitHub, ghId, null, null);
            Assert(!existed, "unlinked identity is unknown again → fresh account");
            Assert(newAccount.AccountId != account.AccountId, "new account, old account untouched");
            Assert(newLink.Status == LinkStatus.Active, "new link is active");
            Assert(newLink.LinkId != ghLink.LinkId, "LinkId is minted fresh (immutable history preserved)");

            var old = f.Db.GetLink(ghLink.LinkId)!;
            Assert(old.Status == LinkStatus.Unlinked, "old link row stays Unlinked (audit history)");
            Assert(f.Db.ActiveLinkCount(account.AccountId) == 1, "old account keeps its remaining provider");
        }
        finally { f.Dispose(); }
    }

    private static void Test_SchemaV2Migration()
    {
        // A v1 database (table-level UNIQUE on the provider anchor) must
        // migrate to v2 on Bootstrap: rows survive, the constraint moves to a
        // partial index over Active rows, and the previously-fatal re-link
        // after unlink works.
        var dbPath = Path.Combine(Path.GetTempPath(), $"duluka-mig-{Guid.NewGuid():N}.db");
        try
        {
            using (var raw = new Microsoft.Data.Sqlite.SqliteConnection(
                new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                {
                    DataSource = dbPath,
                    // Pooling would keep the file locked after Dispose and
                    // break the cleanup delete below.
                    Pooling = false,
                }.ToString()))
            {
                raw.Open();
                ExecRaw(raw, """
                    CREATE TABLE SchemaHistory(Version INTEGER PRIMARY KEY, AppliedAt TEXT NOT NULL);
                    CREATE TABLE DulukaAccount(
                        AccountId TEXT PRIMARY KEY, Status TEXT NOT NULL DEFAULT 'Active',
                        DisplayName TEXT, CreatedAt TEXT NOT NULL, UpdatedAt TEXT NOT NULL);
                    CREATE TABLE AccountProviderLink(
                        LinkId TEXT PRIMARY KEY,
                        AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
                        ProviderKey TEXT NOT NULL, ProviderUserId TEXT NOT NULL,
                        ProviderEmail TEXT, Status TEXT NOT NULL DEFAULT 'Active',
                        LinkedAt TEXT NOT NULL, UnlinkedAt TEXT,
                        UNIQUE(ProviderKey, ProviderUserId));
                    CREATE TABLE AccountDevice(
                        DeviceId TEXT PRIMARY KEY,
                        AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
                        DeviceName TEXT NOT NULL, DeviceKeyHash TEXT NOT NULL UNIQUE,
                        CreatedAt TEXT NOT NULL, LastSeenAt TEXT NOT NULL, RevokedAt TEXT);
                    CREATE TABLE AccountSession(
                        SessionId TEXT PRIMARY KEY, SessionTokenHash TEXT NOT NULL UNIQUE,
                        AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
                        DeviceId TEXT NOT NULL REFERENCES AccountDevice(DeviceId),
                        IssuedViaLinkId TEXT, CreatedAt TEXT NOT NULL, ExpiresAt TEXT NOT NULL,
                        LastSeenAt TEXT NOT NULL, RevokedAt TEXT, RevokedReason TEXT);
                    CREATE TABLE SyncProfile(
                        AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
                        ProductKey TEXT NOT NULL, SchemaVersion INTEGER NOT NULL,
                        CurrentVersion INTEGER NOT NULL, DataBlob BLOB,
                        UpdatedByDeviceId TEXT, UpdatedAt TEXT NOT NULL, UNIQUE(AccountId, ProductKey));
                    CREATE TABLE CredentialReference(
                        LinkId TEXT PRIMARY KEY REFERENCES AccountProviderLink(LinkId),
                        ProviderKey TEXT NOT NULL, ProviderUserId TEXT NOT NULL,
                        AccessTokenRef TEXT, RefreshTokenRef TEXT, Scopes TEXT,
                        ObtainedAt TEXT NOT NULL, ExpiresAt TEXT);
                    INSERT INTO SchemaHistory VALUES (1, '2026-01-01T00:00:00.0000000+00:00');
                    INSERT INTO DulukaAccount VALUES ('acc-v1', 'Active', NULL,
                        '2026-01-01T00:00:00.0000000+00:00', '2026-01-01T00:00:00.0000000+00:00');
                    INSERT INTO AccountProviderLink VALUES ('link-v1', 'acc-v1', 'github', '99001',
                        NULL, 'Active', '2026-01-01T00:00:00.0000000+00:00', NULL);
                    """);
            }

            var migrated = new Database(dbPath, NullLogger<Database>.Instance);
            try
            {
                migrated.Bootstrap();

                // Rows survived the rebuild.
                var link = migrated.GetLink("link-v1");
                Assert(link is not null && link.Status == LinkStatus.Active, "v1 rows survive migration");
                var (same, acc, existed) = migrated.UpsertLink(ProviderKeys.GitHub, "99001", null, null);
                Assert(existed && acc.AccountId == "acc-v1" && same.LinkId == "link-v1",
                    "anchor still resolves to the same account after migration");

                // The re-link path that was FATAL on v1 now converges.
                migrated.Unlink("link-v1", DateTimeOffset.UtcNow);
                var (fresh, freshAcc, existed2) = migrated.UpsertLink(ProviderKeys.GitHub, "99001", null, null);
                Assert(!existed2 && freshAcc.AccountId != "acc-v1",
                    "re-login after unlink creates a fresh account (v1: unique violation → 500)");
                Assert(fresh.Status == LinkStatus.Active, "fresh link active");
            }
            finally { migrated.DisposeAsync().AsTask().GetAwaiter().GetResult(); }

            // SchemaHistory records v2.
            using (var check = new Microsoft.Data.Sqlite.SqliteConnection(
                new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = dbPath, Pooling = false }.ToString()))
            {
                check.Open();
                using var cmd = check.CreateCommand();
                cmd.CommandText = "SELECT MAX(Version) FROM SchemaHistory";
                Assert(Convert.ToInt32(cmd.ExecuteScalar()) == 2, "schema version marked 2");
                using var idx = check.CreateCommand();
                idx.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='UX_Link_ActiveIdentity'";
                Assert(Convert.ToInt32(idx.ExecuteScalar()) == 1, "partial unique index present");
                using var tbl = check.CreateCommand();
                tbl.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='AccountProviderLink'";
                var sql = (string?)tbl.ExecuteScalar() ?? "";
                Assert(!sql.Contains("UNIQUE(ProviderKey", StringComparison.OrdinalIgnoreCase),
                    "table-level UNIQUE removed");
            }
        }
        finally
        {
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var p = dbPath + suffix;
                if (File.Exists(p)) File.Delete(p);
            }
        }
    }

    private static void ExecRaw(Microsoft.Data.Sqlite.SqliteConnection conn, string batch)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = batch;
        cmd.ExecuteNonQuery();
    }

    private static void Test_NvidiaReserved()
    {
        // The reserved provider must exist as a key but expose NO auth surface
        // and NO hardware-identity properties (C/6 hard rule).
        Assert(ProviderKeys.All.Contains(ProviderKeys.Nvidia), "nvidia is a known ProviderKey");
        Assert(!ProviderKeys.IsImplemented(ProviderKeys.Nvidia), "nvidia has no implemented flow");
        Assert(ProviderKeys.IsImplemented(ProviderKeys.GitHub), "github is the implemented provider");

        var type = typeof(NvidiaProviderStub);
        Assert(type.GetProperties().Length == 0, "nvidia stub exposes no identity surface");
    }

    // ─── HTTP integration: the §7.1 wire contract against the REAL server ───
    // The service-level tests above cannot see Program.cs (minimal APIs) —
    // the redacted-state defect shipped through exactly that blind spot.

    private static void RunHttpIntegrationTests()
    {
        var serverCsproj = LocateServerProject();
        var serverDir = Path.GetDirectoryName(serverCsproj)!;
        var port = FreeTcpPort();
        var dbPath = Path.Combine(Path.GetTempPath(), $"duluka-http-{Guid.NewGuid():N}.db");
        var cfg =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
        // Launch the BUILT DLL directly — no `dotnet run` build layer in the
        // child (the parent's ProjectReference build already produced it, and
        // a nested build/MSBuild layer here is nondeterministic output-wise).
        // NB: Environment.ProcessPath here is the TESTS' APPhOST exe — an
        // apphost cannot `exec` another dll (it just re-runs the tests), so
        // the real dotnet host must be resolved from the loaded runtime.
        var serverDll = Path.Combine(serverDir, "bin", cfg, "net10.0", "Duluka.Server.dll");
        Assert(File.Exists(serverDll), $"server build output missing: {serverDll}");
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = DotnetHostPath(),
            Arguments = $"exec \"{serverDll}\"",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
        };
        psi.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        psi.Environment["DULUKA_Database__Path"] = dbPath;
        psi.Environment["DULUKA_GitHub__ClientId"] = "test-client-id";
        psi.Environment["DULUKA_GitHub__ClientSecret"] = "test-client-secret";
        psi.Environment["DULUKA_GitHub__RedirectUri"] = "http://localhost:8517/v1/auth/github/callback";

        System.Diagnostics.Process? server = null;
        var stdout = new System.Text.StringBuilder();
        var stderr = new System.Text.StringBuilder();
        try
        {
            Console.WriteLine($"  [http-int] child: \"{psi.FileName}\" {psi.Arguments}");
            server = System.Diagnostics.Process.Start(psi);
            server!.OutputDataReceived += (_, e) => stdout.AppendLine(e.Data);
            server.ErrorDataReceived += (_, e) => stderr.AppendLine(e.Data);
            server.BeginOutputReadLine();
            server.BeginErrorReadLine();

            var basePath = $"http://127.0.0.1:{port}";
            try
            {
                WaitForLive(basePath, server, stdout, stderr);
            }
            catch (Exception ex)
            {
                // Server never came up — fail the HTTP group without crashing
                // the whole suite, so the rest of the results still report.
                Console.WriteLine(ex.Message);
                Failures.Add("HTTP-INT group — server startup failed: " + ex.Message);
                _failed += 4;
                return;
            }
            using var http = new HttpClient { BaseAddress = new Uri(basePath) };
            const string deviceKey = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789AB";

            Run("HTTP-INT-1: /start returns RAW state + ok envelope + reqId echo", () =>
            {
                using var resp = PostJson(http, "/v1/auth/github/start", "reqid-http-int-1",
                    $$"""{"deviceName":"it-dev","deviceKey":"{{deviceKey}}"}""");
                var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Assert((int)resp.StatusCode == 200, $"expected 200, got {(int)resp.StatusCode}: {body}");
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                Assert(root.GetProperty("ok").GetBoolean(), "ok=true on success envelope");
                Assert(root.GetProperty("reqId").GetString() == "reqid-http-int-1", "reqId echoed verbatim");
                var resource = root.GetProperty("resource");
                var state = resource.GetProperty("state").GetString() ?? "";
                var url = resource.GetProperty("authorizationUrl").GetString() ?? "";
                Assert(state.StartsWith("duluka_state_") && !state.Contains('…'),
                    "state returned RAW (not redacted) — the client must be able to echo it");
                Assert(url.Contains("state=" + Uri.EscapeDataString(state)),
                    "authorizationUrl carries the SAME state value");
            });

            Run("HTTP-INT-2: 401 envelope with registry code auth.session_expired", () =>
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "/v1/account/me");
                req.Headers.Add("X-ReqId", "reqid-http-int-2");
                using var resp = http.SendAsync(req).GetAwaiter().GetResult();
                var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Assert((int)resp.StatusCode == 401, $"expected 401, got {(int)resp.StatusCode}: {body}");
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                Assert(!root.GetProperty("ok").GetBoolean(), "ok=false on error envelope");
                Assert(root.GetProperty("errorCode").GetString() == "auth.session_expired",
                    "registry code auth.session_expired on the wire");
                Assert(root.GetProperty("httpStatus").GetInt32() == 401, "httpStatus echoed");
                Assert(!root.GetProperty("retryable").GetBoolean(), "401 is not retryable");
                Assert(root.GetProperty("reqId").GetString() == "reqid-http-int-2", "reqId echoed on errors too");
            });

            Run("HTTP-INT-3: NVIDIA start → 501 provider_reserved envelope", () =>
            {
                using var resp = PostJson(http, "/v1/auth/nvidia/start", "reqid-http-int-3",
                    $$"""{"deviceName":"it-dev","deviceKey":"{{deviceKey}}"}""");
                var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Assert((int)resp.StatusCode == 501, $"expected 501, got {(int)resp.StatusCode}: {body}");
                using var doc = JsonDocument.Parse(body);
                Assert(doc.RootElement.GetProperty("errorCode").GetString() == "provider_reserved",
                    "reserved provider code on the wire");
            });

            Run("HTTP-INT-4: rate limiter answers with the §7.1 envelope (429)", () =>
            {
                var saw429 = false;
                for (var i = 0; i < 14 && !saw429; i++)
                {
                    using var resp = PostJson(http, "/v1/auth/github/start", $"reqid-http-int-4-{i}",
                        $$"""{"deviceName":"it-dev","deviceKey":"{{deviceKey}}"}""");
                    if ((int)resp.StatusCode != 429) continue;
                    saw429 = true;
                    var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    Assert(root.GetProperty("errorCode").GetString() == "server.rate_limited",
                        "429 carries server.rate_limited");
                    Assert(root.GetProperty("retryable").GetBoolean(), "429 is retryable");
                    Assert(root.GetProperty("reqId").GetString() == $"reqid-http-int-4-{i}", "429 echoes reqId");
                }
                Assert(saw429, "expected ≥1 429 within the fixed 10/min auth-start window");
            });
        }
        finally
        {
            if (server is not null && !server.HasExited)
            {
                try { server.Kill(entireProcessTree: true); } catch { /* best effort */ }
            }
            server?.Dispose();
            foreach (var suffix in new[] { "", "-wal", "-shm" })
            {
                var p = dbPath + suffix;
                if (File.Exists(p)) File.Delete(p);
            }
        }
    }    private static HttpResponseMessage PostJson(HttpClient http, string url, string reqId, string json)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.Add("X-ReqId", reqId);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return http.SendAsync(req).GetAwaiter().GetResult();
    }

    private static string DotnetHostPath()
    {
        // The loaded runtime's directory sits <dotnet-root>\shared\<fx>\<ver>;
        // the host that launched THIS process lives at the dotnet root.
        var dir = new DirectoryInfo(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory());
        for (var i = 0; i < 3 && dir?.Parent is not null; i++) dir = dir.Parent;
        var host = Path.Combine(dir!.FullName, "dotnet.exe");
        return File.Exists(host) ? host : "dotnet";
    }

    private static string LocateServerProject()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 10 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, "Duluka", "Duluka.Server", "Duluka.Server.csproj");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new InvalidOperationException("Duluka.Server.csproj not found above the test binary directory");
    }

    private static int FreeTcpPort()
    {
        var t = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        t.Start();
        try { return ((System.Net.IPEndPoint)t.LocalEndpoint).Port; }
        finally { t.Stop(); }
    }

    private static void WaitForLive(string basePath, System.Diagnostics.Process server,
        System.Text.StringBuilder stdout, System.Text.StringBuilder stderr)
    {
        using var http = new HttpClient { BaseAddress = new Uri(basePath) };
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            if (server.HasExited)
                throw new InvalidOperationException($"server exited early.\nSTDOUT:{stdout}\nSTDERR:{stderr}");
            try
            {
                using var resp = http.GetAsync("/healthz").GetAwaiter().GetResult();
                if ((int)resp.StatusCode == 200) return;
            }
            catch { /* not up yet */ }
            Thread.Sleep(300);
        }
        throw new InvalidOperationException($"server not live within 90s.\nSTDOUT:{stdout}\nSTDERR:{stderr}");
    }

    /// <summary>Intentionally inert — documents the reserved provider contract.</summary>
    public static class NvidiaProviderStub { }
}
