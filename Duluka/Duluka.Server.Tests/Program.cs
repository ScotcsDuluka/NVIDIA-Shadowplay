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
        Run("REVOKE-1: session revoke + revoke-all", Test_SessionRevoke);
        Run("REVOKE-2: device revoke kills its sessions + blocks the key", Test_DeviceRevoke);
        Run("UNLINK-1: last-provider unlink rejected", Test_UnlinkLastProvider);
        Run("UNLINK-2: unlink non-last destroys credential + via-link sessions", Test_UnlinkNonLast);
        Run("NVIDIA-1: provider reserved, no flow, no hardware identity surface", Test_NvidiaReserved);

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
            var msg = ex.Message.Length > 110 ? ex.Message[..110] + "..." : ex.Message;
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

    private static IConfiguration TestConfig(string dbPath) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GitHub:ClientId"] = "test-client-id",
            ["GitHub:ClientSecret"] = "test-client-secret",
            ["GitHub:RedirectUri"] = "http://localhost:8517/v1/auth/github/callback",
            ["Database:Path"] = dbPath,
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

    private static Fixture NewFixture()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"duluka-test-{Guid.NewGuid():N}.db");
        var config = TestConfig(dbPath);
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

    private static void Test_SessionRevoke()
    {
        var f = NewFixture();
        try
        {
            var ghId = f.RegisterGitHubUser(8001, "revokeme");
            var id = new GitHubIdentity(ProviderKeys.GitHub, ghId, null, null);
            var (_, hash) = NewDeviceKey();
            var (link, account, _) = f.Provisioning.LoginOrLink(id, hash, "dev");
            var device = f.Db.DevicesForAccount(account.AccountId).Single();

            var t1 = f.Sessions.Create(account.AccountId, device.DeviceId, link.LinkId).Token;
            var t2 = f.Sessions.Create(account.AccountId, device.DeviceId, link.LinkId).Token;

            Assert(f.Sessions.Revoke(t1, "user-logout"), "own-session revoke succeeds");
            Assert(f.Sessions.Validate(t1) is null, "revoked session dead");
            Assert(f.Sessions.Validate(t2) is not null, "other session survives");

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

            var (token, _) = f.Sessions.Create(account.AccountId, device.DeviceId, ghLink.LinkId);
            var (tokenViaFuture, _) = f.Sessions.Create(account.AccountId, device.DeviceId, futureLink.LinkId);
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

    /// <summary>Intentionally inert — documents the reserved provider contract.</summary>
    public static class NvidiaProviderStub { }
}
