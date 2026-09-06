namespace Duluka.Http.Integration.Tests;

/// <summary>
/// C/2 — Duluka black-box HTTP integration suite.
///
/// Under test: the REAL Duluka.Server binary over REAL HTTP with its REAL
/// SQLite store. Groups that need deterministic pre-exchange behavior run
/// against an unconfigured server; the one network-dependent group is
/// egress-gated with honest SKIP. Nothing about production behavior is
/// mocked or patched.
///
/// Outcome vocabulary (C/4 honesty):
///   PASS    — production behaves exactly as asserted
///   FAIL    — production drifted from the pinned behavior (alarm)
///   MISMATCH-CONFIRMED — the production behavior violates the v0.1 contract
///             in a way tracked in the mismatch ledger. The test passes ONLY
///             while the violation still reproduces exactly; fixing (or
///             changing) the behavior fails the test and forces a ledger
///             reclassification. These are the fixes-required-elsewhere.
///   SKIP    — environment not capable (recorded, never counted as PASS)
/// </summary>
internal static class Program
{
    private static int Main()
    {
        Console.WriteLine("==============================================================");
        Console.WriteLine(" Duluka.Http.Integration.Tests — C/2 black-box HTTP suite");
        Console.WriteLine(" real server process + real SQLite per group, loopback only");
        Console.WriteLine("==============================================================");

        try
        {
            Groups.Health(Runner.I);                   // G0
            Groups.AuthStart(Runner.I);                // G1
            Groups.Callback(Runner.I);                 // G2
            Groups.ProviderExchange(Runner.I);         // G3 (egress-gated)
            Groups.AuthenticatedSurface(Runner.I);     // G4
            Groups.Devices(Runner.I);                  // G5
            Groups.Sessions(Runner.I);                 // G6
            Groups.Providers(Runner.I);                // G7
            Groups.StoreRaceTests(Runner.I);           // G8 (store-level, no server)
            Groups.RateLimits(Runner.I);               // G9
            Groups.Restart(Runner.I);                  // G10
            Groups.ContractSweep(Runner.I);            // G11
            Groups.NativeAuth(Runner.I);               // G12 (native username/password)
            Groups.NativePasswordRotation(Runner.I);   // G13 (native password change)
        }
        catch (Exception ex)
        {
            Runner.Fail("SUITE/harness", ex);
        }

        var (passed, failed, skipped) = (Runner.I.Passed, Runner.I.Failed, Runner.I.Skipped);
        Console.WriteLine();
        Console.WriteLine("--------------------------------------------------------------");
        Console.WriteLine($" RESULT: {passed} passed, {failed} failed, {skipped} skipped, {passed + failed} total");
        if (Runner.I.Mismatches.Count > 0)
        {
            Console.WriteLine($" Contract mismatches confirmed (fixes required elsewhere): {Runner.I.Mismatches.Count}");
            foreach (var m in Runner.I.Mismatches)
                Console.WriteLine("   " + m);
        }
        if (Runner.I.SkipReasons.Count > 0)
        {
            Console.WriteLine($" Skips (environment-not-capable, honest): {Runner.I.SkipReasons.Count}");
            foreach (var s in Runner.I.SkipReasons)
                Console.WriteLine("   " + s);
        }
        Console.WriteLine("--------------------------------------------------------------");
        if (failed > 0)
            foreach (var f in Runner.I.FailureList)
                Console.WriteLine("  FAILED: " + f);
        return failed == 0 ? 0 : 1;
    }
}

/// <summary>Test runner + mismatch ledger. Single-threaded suite; statics are safe.</summary>
internal sealed class Runner
{
    public static readonly Runner I = new();

    public int Passed, Failed, Skipped;
    public string? Pending;
    public readonly List<string> Mismatches = new();
    public readonly List<string> SkipReasons = new();
    public readonly List<string> FailureList = new();

    /// <summary>Contract violations tracked against the v0.1 implementation
    /// contract (docs/DULUKA-V0.1-IMPLEMENTATION-CONTRACT.md) and the C/2
    /// executable spec (Duluka.Account.Tests/TEST-MATRIX.md). Every entry is a
    /// fix required OUTSIDE this test project.</summary>
    private static readonly string[] LedgerIds =
    {
        // Ledger after the C/5 reconcile verification pass: M-3, M-4, M-5 and
        // M-8 were FIXED by C/5 and became contract-PASS tests; M-1 was fixed
        // by the dedicated malformed-JSON commit and became a contract-PASS
        // test. Still violated:
        "M-2", "M-6", "M-7", "M-9", "M-10", "M-11",
    };

    public sealed class GroupCtx
    {
        private bool _configureGitHub;
        private int _budget;
        private string? _dbPath;

        public ServerApp App { get; private set; } = null!;

        internal void Init(bool configureGitHub, int budget)
        {
            _configureGitHub = configureGitHub;
            _budget = budget;
        }

        /// <summary>Kill the current server process and start a new one on the
        /// SAME SQLite store; secrets/exchanges/log carry over so sweeps stay
        /// whole across generations.</summary>
        public void Restart()
        {
            var carryLog = App.ReadLog();
            var carrySecrets = App.SecretsInternal();
            var carryExchanges = App.Exchanges.ToList();
            _dbPath ??= App.DbPath;
            App.DeleteStoreOnDispose = false; // the store outlives this process generation
            App.Dispose();
            App = ServerApp.Start(_configureGitHub, _budget, _dbPath, carryLog, carrySecrets, carryExchanges);
        }

        internal void SetApp(ServerApp app) => App = app;
        internal string? DbPath => _dbPath ?? App?.DbPath;
    }

    public void Group(string name, bool configureGitHub, int budget, Action<GroupCtx> body)
    {
        Console.WriteLine();
        Console.WriteLine($" ──── {name} ────");
        var ctx = new GroupCtx();
        ctx.Init(configureGitHub, budget);
        try
        {
            ctx.SetApp(ServerApp.Start(configureGitHub, budget));
            body(ctx);
        }
        catch (Exception ex)
        {
            Console.WriteLine("  GROUP-LEVEL FAILURE: " + ex.Message.ReplaceLineEndings(" | "));
            Fail($"[{name} group]", ex);
        }
        finally
        {
            // Group-level store cleanup (covers restart groups where no single
            // app generation owns the store).
            var dbPath = ctx.DbPath;
            ctx.App?.Dispose();
            if (dbPath is not null)
                foreach (var suffix in new[] { "", "-wal", "-shm" })
                    try { if (File.Exists(dbPath + suffix)) File.Delete(dbPath + suffix); } catch { }
        }
    }

    public void SkipGroup(string reason)
    {
        Skipped++;
        Console.WriteLine($"  ──── GROUP SKIPPED: {reason}");
        SkipReasons.Add("group skip — " + reason);
    }

    public void Run(string name, Action test)
    {
        Pending = null;
        Console.Write($"  {name.PadRight(96)}");
        try
        {
            test();
            Passed++;
            Console.WriteLine("PASS");
            if (Pending is not null)
                Console.WriteLine("      " + Pending);
        }
        catch (Exception ex)
        {
            Failed++;
            Console.WriteLine("FAIL");
            Fail(name, ex);
        }
    }

    /// <summary>Record that a tracked contract violation still reproduces.
    /// The surrounding test asserts the tracked production behavior first, so a
    /// fix (or any behavior change) fails the test and forces ledger
    /// reclassification — the ledger can never go stale silently.</summary>
    public void Mismatch(string id, string evidence)
    {
        if (!LedgerIds.Contains(id))
            throw new InvalidOperationException($"Mismatch id '{id}' is not in the ledger — add it first.");
        Pending = $"⚠ MISMATCH {id} CONFIRMED — {evidence}";
        Mismatches.Add($"{id}: {evidence}");
    }

    internal static void Fail(string name, Exception ex)
    {
        var msg = ex.Message.ReplaceLineEndings(" | ");
        if (msg.Length > 300) msg = msg[..300] + "…";
        I.FailureList.Add($"{name} — {msg}");
    }

    public static string? UrlParam(string url, string name)
    {
        var q = new Uri(url).Query.TrimStart('?');
        foreach (var pair in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length == 2 && kv[0] == name)
                return Uri.UnescapeDataString(kv[1]);
        }
        return null;
    }
}
