using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Duluka.Server.Auth;
using Duluka.Server.Data;
using Duluka.Server.Domain;
using Duluka.Server.Security;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Duluka.Http.Integration.Tests;

/// <summary>One HTTP exchange against the live server.</summary>
internal sealed record Resp(string Method, string Path, int Status, string Body)
{
    public bool IsJsonBody => Body.TrimStart().StartsWith('{') || Body.TrimStart().StartsWith('[');
}

/// <summary>
/// One REAL server process + its REAL SQLite database + a harness-side
/// production <see cref="Database"/> handle on the SAME file for seeding.
/// Nothing here mocks server behavior: the binary under test is the built
/// Duluka.Server.dll; seeding inserts rows through the production data layer
/// (same hashing, same store) exactly like a client that provisioned offline.
/// </summary>
internal sealed class ServerApp : IDisposable
{
    private static readonly object BuildLock = new();
    private static bool _serverBuilt;

    private readonly Process _proc;
    private readonly string _dir;
    private readonly StringBuilder _logBuffer;
    private readonly string? _inheritedLog;
    private readonly List<string> _secrets = new();   // raw values that must never leak into logs/bodies
    private bool _deleteStoreOnDispose;

    public List<Resp> Exchanges { get; } = new();
    public HttpClient Http { get; } = new() { Timeout = TimeSpan.FromSeconds(15) };
    public Database Db { get; }                       // production data layer, harness-side
    public string DbPath { get; }
    public int AuthStartUsed { get; private set; }
    public int AuthStartBudget { get; }
    public bool DeleteStoreOnDispose { set => _deleteStoreOnDispose = value; }

    private ServerApp(Process proc, string dir, string dbPath, Database db, int authStartBudget,
        StringBuilder logBuffer, string? inheritedLog, IEnumerable<string>? inheritedSecrets, IEnumerable<Resp>? inheritedExchanges)
    {
        _proc = proc; _dir = dir; DbPath = dbPath; Db = db;
        AuthStartBudget = authStartBudget;
        _logBuffer = logBuffer;
        _inheritedLog = inheritedLog;
        if (inheritedSecrets is not null) _secrets.AddRange(inheritedSecrets);
        if (inheritedExchanges is not null) Exchanges.AddRange(inheritedExchanges);
    }

    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Duluka", "Duluka.Server", "Duluka.Server.csproj")))
            dir = dir.Parent!;
        return dir?.FullName ?? throw new InvalidOperationException("repo root not found from " + AppContext.BaseDirectory);
    }

    private static string ServerDll()
    {
        lock (BuildLock)
        {
            var dll = Path.Combine(RepoRoot(), "Duluka", "Duluka.Server", "bin", "Release", "net10.0", "Duluka.Server.dll");
            if (!_serverBuilt || !File.Exists(dll))
            {
                var csproj = Path.Combine(RepoRoot(), "Duluka", "Duluka.Server", "Duluka.Server.csproj");
                var psi = new ProcessStartInfo("dotnet", $"build \"{csproj}\" -c Release --nologo -v q")
                {
                    RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
                };
                using var p = Process.Start(psi)!;
                var output = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
                p.WaitForExit();
                if (p.ExitCode != 0 || !File.Exists(dll))
                    throw new InvalidOperationException("server build failed:\n" + output);
                _serverBuilt = true;
            }
            return dll;
        }
    }

    /// <summary>Launch a fresh real server. dbPath null → brand-new temp database
    /// (deleted on Dispose); non-null → reuse an existing database across a
    /// restart (store cleanup then belongs to the caller). inherited* carry
    /// secrets/exchanges/log across a restart so sweeps stay whole.</summary>
    public static ServerApp Start(bool configureGitHub, int authStartBudget, string? dbPath = null,
        string? inheritedLog = null, IEnumerable<string>? inheritedSecrets = null, IEnumerable<Resp>? inheritedExchanges = null)
    {
        string dir = Path.Combine(Path.GetTempPath(), "duluka-itest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        dbPath ??= Path.Combine(Path.GetTempPath(), $"duluka-itest-{Guid.NewGuid():N}.db");
        dbPath = Path.GetFullPath(dbPath);

        var port = FreePort();
        var dll = ServerDll();
        var psi = new ProcessStartInfo("dotnet", $"\"{dll}\" --urls http://127.0.0.1:{port}")
        {
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(dll)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        // Composition is 100% launch configuration — no production code is touched.
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        psi.Environment["DULUKA_Database__Path"] = dbPath;
        psi.Environment["DOTNET_NOLOGO"] = "1";
        psi.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        if (configureGitHub)
        {
            psi.Environment["DULUKA_GitHub__ClientId"] = "duluka-itest-client";
            psi.Environment["DULUKA_GitHub__ClientSecret"] = "duluka-itest-secret";
        }

        var logBuffer = new StringBuilder();
        var proc = Process.Start(psi)!;
        proc.OutputDataReceived += (_, e) => { if (e.Data is not null) lock (logBuffer) logBuffer.AppendLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data is not null) lock (logBuffer) logBuffer.AppendLine(e.Data); };
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        var app = new ServerApp(proc, dir, dbPath, OpenStore(dbPath), authStartBudget,
            logBuffer, inheritedLog, inheritedSecrets, inheritedExchanges);
        app.Http.BaseAddress = new Uri($"http://127.0.0.1:{port}/");

        // Bounded readiness wait (startup, not a timing assertion).
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{port}/healthz");
                using var resp = app.Http.SendAsync(req).GetAwaiter().GetResult();
                if (resp.StatusCode == HttpStatusCode.OK) return app;
            }
            catch { /* not up yet */ }
            if (proc.HasExited) break;
            Thread.Sleep(100);
        }
        var tail = app.ReadLog();
        app.KillOnly();
        try { Directory.Delete(dir, true); } catch { }
        throw new InvalidOperationException(
            $"server did not become healthy on :{port} (exited={proc.HasExited})\n--- server log tail ---\n{tail}");
    }

    public string ReadLog()
    {
        lock (_logBuffer) return (_inheritedLog ?? "") + _logBuffer.ToString();
    }

    private static Database OpenStore(string dbPath)
    {
        // Harness-side handle on the SAME SQLite file the server uses — the
        // production data class, so seeded hashes/ids are byte-identical to
        // what the server itself would write.
        return new Database(dbPath, NullLogger<Database>.Instance);
    }

    private static int FreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        try { return ((IPEndPoint)l.LocalEndpoint).Port; }
        finally { l.Stop(); }
    }

    // ─── HTTP ───────────────────────────────────────────────────────────────

    private static bool IsAuthStart(string method, string path) =>
        method == "POST" &&
        (path.EndsWith("/start") && path.StartsWith("/v1/auth/") ||
         path.EndsWith("/callback") && path.StartsWith("/v1/auth/") ||
         path == "/v1/account/providers" ||
         path.StartsWith("/v1/account/providers/") && path.EndsWith("/complete"));

    public Resp Send(string method, string path, string? json = null, string? bearer = null,
        string? rawAuthorization = null, bool jsonContentType = true)
    {
        if (IsAuthStart(method, path) && ++AuthStartUsed > AuthStartBudget)
            throw new InvalidOperationException(
                $"auth-start budget {AuthStartBudget} exceeded ({AuthStartUsed} used) — the fixed 10/min window would make tests flaky");
        using var req = new HttpRequestMessage(new HttpMethod(method), path);
        if (rawAuthorization is not null) req.Headers.TryAddWithoutValidation("Authorization", rawAuthorization);
        else if (bearer is not null) req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + bearer);
        if (json is not null)
        {
            var content = new StringContent(json, Encoding.UTF8);
            if (jsonContentType) content.Headers.ContentType = new("application/json");
            req.Content = content;
        }
        using var resp = Http.SendAsync(req).GetAwaiter().GetResult();
        var body = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var r = new Resp(method, path, (int)resp.StatusCode, body);
        Exchanges.Add(r);
        return r;
    }

    public Resp Get(string path, string? bearer = null) => Send("GET", path, bearer: bearer);
    public Resp Post(string path, object? body = null, string? bearer = null) =>
        Send("POST", path, body is null ? "{}" : JsonSerializer.Serialize(body), bearer);
    public Resp PostRaw(string path, string rawBody, string? bearer = null, bool jsonContentType = true) =>
        Send("POST", path, rawBody, bearer, jsonContentType: jsonContentType);
    public Resp Delete(string path, string? bearer = null) => Send("DELETE", path, bearer: bearer);

    // ─── assertion helpers ──────────────────────────────────────────────────

    public static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Assertion failed: " + message);
    }

    public static Resp ExpectStatus(Resp r, int status, string label)
    {
        Assert(r.Status == status, $"{label}: expected HTTP {status}, got {r.Status} — body: {Trunc(r.Body)}");
        return r;
    }

    /// <summary>Assert the single error-envelope shape and return (code, message).</summary>
    public static (string Code, string Message) ExpectErr(Resp r, int status, string code, string label)
    {
        ExpectStatus(r, status, label);
        Assert(r.IsJsonBody, $"{label}: expected JSON error envelope, got: {Trunc(r.Body)}");
        using var doc = JsonDocument.Parse(r.Body);
        var root = doc.RootElement;
        Assert(root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object,
            $"{label}: envelope has no error object: {Trunc(r.Body)}");
        var gotCode = err.GetProperty("code").GetString();
        var msg = err.GetProperty("message").GetString();
        Assert(!string.IsNullOrEmpty(gotCode) && !string.IsNullOrEmpty(msg),
            $"{label}: error.code/error.message must be non-empty: {Trunc(r.Body)}");
        Assert(gotCode == code, $"{label}: expected error.code '{code}', got '{gotCode}'");
        return (gotCode!, msg!);
    }

    public static string Prop(Resp r, string name)
    {
        using var doc = JsonDocument.Parse(r.Body);
        return doc.RootElement.GetProperty(name).GetString()
               ?? throw new InvalidOperationException($"{name} missing/null in: {Trunc(r.Body)}");
    }

    public static JsonElement Json(Resp r)
    {
        using var doc = JsonDocument.Parse(r.Body);
        return doc.RootElement.Clone();
    }

    public static string Trunc(string s) => s.Length <= 160 ? s : s[..160] + "…";

    /// <summary>Value that must never appear in any response body or server log line.</summary>
    public void TrackSecret(string secret)
    {
        if (!string.IsNullOrEmpty(secret)) _secrets.Add(secret);
    }

    internal IReadOnlyList<string> SecretsInternal() => _secrets;

    /// <summary>R3/C/2 redaction gate over the live server: no tracked secret may
    /// appear in any collected response body or in the server's own log output.</summary>
    public void Sweep(string label)
    {
        var log = ReadLog();
        foreach (var r in Exchanges)
            foreach (var s in _secrets)
                Assert(!r.Body.Contains(s, StringComparison.Ordinal),
                    $"{label}: SECRET LEAK in response body of {r.Method} {r.Path} (status {r.Status}): tracked secret value present");
        foreach (var s in _secrets)
            Assert(!log.Contains(s, StringComparison.Ordinal),
                $"{label}: SECRET LEAK in server log: tracked secret value present");
    }

    // ─── seeding (production data layer on the server's live store) ─────────

    public string SeedAccount(string displayName) => Db.CreateAccount(displayName).AccountId;

    public (string DeviceId, string DeviceKey) SeedDevice(string accountId, string name, bool revoked = false)
    {
        var deviceKey = Secrets.NewToken("devk_");
        var device = Db.CreateDevice(accountId, name, Secrets.Sha256Hex(deviceKey));
        if (revoked) Raw($"UPDATE AccountDevice SET RevokedAt='{DateTimeOffset.UtcNow.ToString("o")}' WHERE DeviceId='{device.DeviceId}'");
        TrackSecret(deviceKey);
        return (device.DeviceId, deviceKey);
    }

    public string SeedLink(string accountId, string providerUserId)
    {
        var link = new AccountProviderLink(
            Secrets.NewToken("duluka_link_"), accountId, ProviderKeys.GitHub, providerUserId,
            providerUserId + "@example.test", LinkStatus.Active, DateTimeOffset.UtcNow, null);
        Db.AddLink(link);
        return link.LinkId;
    }

    public string SeedSession(string accountId, string deviceId, string? viaLinkId,
        DateTimeOffset? expires = null, DateTimeOffset? createdAt = null, bool revoked = false)
    {
        var token = Secrets.NewToken("duluka_st_");
        var now = DateTimeOffset.UtcNow;
        Db.CreateSession(accountId, deviceId, viaLinkId, Secrets.Sha256Hex(token),
            expires ?? now + TimeSpan.FromDays(30));
        if (createdAt is not null || expires is not null)
            Raw($"UPDATE AccountSession SET CreatedAt='{(createdAt ?? now).ToString("o")}', " +
                $"ExpiresAt='{(expires ?? now + TimeSpan.FromDays(30)).ToString("o")}' WHERE SessionTokenHash='{Secrets.Sha256Hex(token)}'");
        if (revoked)
            Raw($"UPDATE AccountSession SET RevokedAt='{now.ToString("o")}', RevokedReason='seed-revoked' WHERE SessionTokenHash='{Secrets.Sha256Hex(token)}'");
        TrackSecret(token);
        return token;
    }

    public void Suspend(string accountId) =>
        Raw($"UPDATE DulukaAccount SET Status='Suspended' WHERE AccountId='{accountId}'");

    /// <summary>Direct store read (SchemaHistory etc.) through a short-lived
    /// connection with a busy timeout — harness-side only.</summary>
    public string? RawScalar(string sql)
    {
        using var conn = OpenRawConn(DbPath);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd.ExecuteScalar()?.ToString();
    }

    private static SqliteConnection OpenRawConn(string dbPath)
    {
        var conn = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath, Pooling = false }.ToString());
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA busy_timeout=5000;";
        pragma.ExecuteNonQuery();
        return conn;
    }

    private void Raw(string sql)
    {
        using var conn = OpenRawConn(DbPath);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    // ─── teardown ───────────────────────────────────────────────────────────

    private void KillOnly()
    {
        try { if (!_proc.HasExited) _proc.Kill(entireProcessTree: true); } catch { }
        _proc.WaitForExit(5000);
        _proc.Dispose();
    }

    public void Dispose()
    {
        Http.Dispose();
        KillOnly();
        Db.DisposeAsync().AsTask().GetAwaiter().GetResult();
        if (_deleteStoreOnDispose)
            foreach (var suffix in new[] { "", "-wal", "-shm" })
                try { if (File.Exists(DbPath + suffix)) File.Delete(DbPath + suffix); } catch { }
        try { Directory.Delete(_dir, true); } catch { }
    }
}

/// <summary>Egress gate for the one network-dependent test group (C/4 honesty:
/// RUN when the provider is reachable, SKIP when not — never a fake PASS).</summary>
internal static class Egress
{
    public static bool Reachable()
    {
        try
        {
            using var c = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            using var resp = c.PostAsync("https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent(new Dictionary<string, string> { ["client_id"] = "duluka-itest-probe", ["code"] = "probe" }))
                .GetAwaiter().GetResult();
            return true; // ANY HTTP answer means github.com is reachable
        }
        catch { return false; }
    }
}

/// <summary>Fixture for store-level race tests: the REAL production data layer +
/// REAL SQLite file, no HTTP, no server (the races live below the transport).
/// Each racing thread opens its OWN connection (SqliteConnection/SqliteCommand
/// are not thread-safe at the managed layer); write-write collisions are
/// absorbed by SQLite's busy handler (Microsoft.Data.Sqlite Default Timeout).</summary>
internal static class StoreRaces
{
    public sealed class Store : IDisposable
    {
        public Database Db { get; }
        public AccountProvisioningService Provisioning { get; }
        public string Path { get; }

        public Store()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"duluka-itest-store-{Guid.NewGuid():N}.db");
            Db = new Database(Path, NullLogger<Database>.Instance);
            Db.Bootstrap();
            Provisioning = new AccountProvisioningService(Db);
        }

        /// <summary>A thread-local production store handle (own SQLite connection).</summary>
        public (Database Db, AccountProvisioningService Provisioning) OpenThreadStore()
        {
            var db = new Database(Path, NullLogger<Database>.Instance);
            return (db, new AccountProvisioningService(db));
        }

        /// <summary>Raw scalar read through a short-lived busy-timed-out connection.</summary>
        public string? Scalar(string sql)
        {
            using var conn = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path, Pooling = false }.ToString());
            conn.Open();
            using var pragma = conn.CreateCommand();
            pragma.CommandText = "PRAGMA busy_timeout=5000;";
            pragma.ExecuteNonQuery();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteScalar()?.ToString();
        }

        public void Dispose()
        {
            Db.DisposeAsync().AsTask().GetAwaiter().GetResult();
            foreach (var suffix in new[] { "", "-wal", "-shm" })
                try { if (File.Exists(Path + suffix)) File.Delete(Path + suffix); } catch { }
        }
    }
}
