using Microsoft.Data.Sqlite;
using Duluka.Server.Domain;
using Duluka.Server.Security;

namespace Duluka.Server.Data;

/// <summary>
/// v0 SQLite bootstrap + data access. Parameterized SQL everywhere; secrets
/// (session tokens, device keys) are stored ONLY as SHA-256 digests produced
/// by Secrets.Sha256Hex. The raw token never reaches this layer.
/// Schema is applied idempotently at startup with a SchemaHistory row — the
/// v0 stand-in for a migration framework (operator-readable, no auto-magic).
/// </summary>
public sealed class Database : IAsyncDisposable
{
    private readonly SqliteConnection _conn;
    private readonly ILogger<Database> _logger;

    /// <summary>The RESOLVED database file path (absolute; relative inputs were
    /// pinned to AppContext.BaseDirectory) — the single source of truth for
    /// where this server's state lives, regardless of the starting CWD.</summary>
    public string DbPath { get; }

    public Database(string dbPath, ILogger<Database> logger)
    {
        _logger = logger;
        // CWD-independence (C/1): a relative Database:Path must resolve against
        // the DEPLOYED application location (AppContext.BaseDirectory), never
        // the process's current working directory — otherwise a server started
        // from an unrelated CWD silently creates a fresh database there and
        // splits state. Absolute paths (operator config, tests) pass through.
        if (!Path.IsPathRooted(dbPath))
            dbPath = Path.GetFullPath(dbPath, AppContext.BaseDirectory);
        DbPath = dbPath;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dbPath))!);
        _conn = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString());
        _conn.Open();
        using var pragma = _conn.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON; PRAGMA synchronous=NORMAL;";
        pragma.ExecuteNonQuery();
    }

    public const int SchemaVersion = 4;

    private static readonly string[] Ddl =
    {
        """
        CREATE TABLE IF NOT EXISTS SchemaHistory(
            Version INTEGER PRIMARY KEY,
            AppliedAt TEXT NOT NULL)
        """,
        """
        CREATE TABLE IF NOT EXISTS DulukaAccount(
            AccountId TEXT PRIMARY KEY,
            Status TEXT NOT NULL DEFAULT 'Active',
            DisplayName TEXT,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL,
            ProfileImage TEXT)
        """,
        // NOTE: no table-level UNIQUE(ProviderKey, ProviderUserId) — the anchor
        // is enforced by the PARTIAL unique index below (Active rows only), so
        // a previously UNLINKED identity can be linked/login again. The v1
        // table-level constraint blocked that forever (Bootstrap migrates v1
        // databases to this shape).
        """
        CREATE TABLE IF NOT EXISTS AccountProviderLink(
            LinkId TEXT PRIMARY KEY,
            AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
            ProviderKey TEXT NOT NULL,
            ProviderUserId TEXT NOT NULL,
            ProviderEmail TEXT,
            Status TEXT NOT NULL DEFAULT 'Active',
            LinkedAt TEXT NOT NULL,
            UnlinkedAt TEXT)
        """,
        """
        CREATE TABLE IF NOT EXISTS AccountDevice(
            DeviceId TEXT PRIMARY KEY,
            AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
            DeviceName TEXT NOT NULL,
            DeviceKeyHash TEXT NOT NULL UNIQUE,
            CreatedAt TEXT NOT NULL,
            LastSeenAt TEXT NOT NULL,
            RevokedAt TEXT)
        """,
        """
        CREATE TABLE IF NOT EXISTS AccountSession(
            SessionId TEXT PRIMARY KEY,
            SessionTokenHash TEXT NOT NULL UNIQUE,
            AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
            DeviceId TEXT NOT NULL REFERENCES AccountDevice(DeviceId),
            IssuedViaLinkId TEXT,
            CreatedAt TEXT NOT NULL,
            ExpiresAt TEXT NOT NULL,
            LastSeenAt TEXT NOT NULL,
            RevokedAt TEXT,
            RevokedReason TEXT)
        """,
        """
        CREATE TABLE IF NOT EXISTS SyncProfile(
            AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
            ProductKey TEXT NOT NULL,
            SchemaVersion INTEGER NOT NULL,
            CurrentVersion INTEGER NOT NULL,
            DataBlob BLOB,
            UpdatedByDeviceId TEXT,
            UpdatedAt TEXT NOT NULL,
            UNIQUE(AccountId, ProductKey))
        """,
        """
        CREATE TABLE IF NOT EXISTS CredentialReference(
            LinkId TEXT PRIMARY KEY REFERENCES AccountProviderLink(LinkId),
            ProviderKey TEXT NOT NULL,
            ProviderUserId TEXT NOT NULL,
            AccessTokenRef TEXT,
            RefreshTokenRef TEXT,
            Scopes TEXT,
            ObtainedAt TEXT NOT NULL,
            ExpiresAt TEXT)
        """,
        // Native Duluka credential (schema v3) — 1:1 with the account
        // (AccountId PK) and DB-enforced username uniqueness (UNIQUE anchor).
        // PasswordHash holds ONLY the PBKDF2 verifier string (Secrets.HashPassword).
        """
        CREATE TABLE IF NOT EXISTS NativeCredential(
            AccountId TEXT PRIMARY KEY REFERENCES DulukaAccount(AccountId),
            UsernameCanonical TEXT NOT NULL UNIQUE,
            UsernameDisplay TEXT NOT NULL,
            PasswordHash TEXT NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT NOT NULL)
        """,
        // Indexes are created AFTER the possible v1→v2 table rebuild in
        // Bootstrap (DROP TABLE takes its indexes with it).
    };

    public void Bootstrap()
    {
        using (var tx = _conn.BeginTransaction())
        {
            foreach (var ddl in Ddl)
            {
                using var cmd = _conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = ddl;
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
        }

        // v1 → v2: v1 anchored (ProviderKey, ProviderUserId) with a table-level
        // UNIQUE spanning Unlinked rows — a re-login with a previously unlinked
        // identity could never insert again (permanent 500, contract §5.4-2
        // violation). Rebuild the table without it; the anchor moves to the
        // partial unique index over ACTIVE rows only.
        if (GetSchemaVersion() < 2)
        {
            if (LinkTableHasTableLevelUnique()) RebuildLinkTableWithoutTableUnique();
            MarkSchemaVersion(2);
        }

        // v2 → v3: native credentials. Purely additive (CREATE TABLE IF NOT
        // EXISTS above ran already) — existing accounts, links, devices and
        // sessions are untouched; the row simply records that v3 DDL applied.
        if (GetSchemaVersion() < 3)
        {
            MarkSchemaVersion(3);
        }

        // v3 → v4: account profile surface (ProfileImage). Purely additive —
        // a single nullable column on DulukaAccount; identity, links, devices
        // and sessions are untouched. Fresh databases already got the column
        // from the DDL above, so the ALTER only fires on upgraded stores.
        if (GetSchemaVersion() < 4)
        {
            if (!AccountTableHasColumn("ProfileImage"))
            {
                Exec("ALTER TABLE DulukaAccount ADD COLUMN ProfileImage TEXT");
            }
            MarkSchemaVersion(4);
        }

        using (var cmd = _conn.CreateCommand())
        {
            cmd.CommandText = """
                CREATE UNIQUE INDEX IF NOT EXISTS UX_Link_ActiveIdentity
                    ON AccountProviderLink(ProviderKey, ProviderUserId) WHERE Status='Active';
                CREATE INDEX IF NOT EXISTS IX_Session_Account ON AccountSession(AccountId, RevokedAt);
                CREATE INDEX IF NOT EXISTS IX_Session_Device ON AccountSession(DeviceId, RevokedAt);
                CREATE INDEX IF NOT EXISTS IX_Link_Account ON AccountProviderLink(AccountId, Status);
                """;
            cmd.ExecuteNonQuery();
        }

        _logger.LogInformation("Duluka database schema ready (schema v{Version})", GetSchemaVersion());
    }

    private int GetSchemaVersion()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(MAX(Version), 0) FROM SchemaHistory";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private void MarkSchemaVersion(int version)
    {
        Exec("INSERT OR REPLACE INTO SchemaHistory(Version, AppliedAt) VALUES ($v, $at)",
            ("$v", version), ("$at", DateTimeOffset.UtcNow.ToString("o")));
    }

    private bool LinkTableHasTableLevelUnique()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='AccountProviderLink'";
        return cmd.ExecuteScalar() is string sql
               && sql.Contains("UNIQUE(ProviderKey, ProviderUserId)", StringComparison.OrdinalIgnoreCase);
    }

    private bool AccountTableHasColumn(string columnName)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('DulukaAccount') WHERE name=$n";
        cmd.Parameters.AddWithValue("$n", columnName);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private void RebuildLinkTableWithoutTableUnique()
    {
        // PRAGMA foreign_keys cannot change inside a transaction — toggle around it.
        SetForeignKeys(false);
        try
        {
            using var tx = _conn.BeginTransaction();
            using (var create = _conn.CreateCommand())
            {
                create.Transaction = tx;
                create.CommandText = """
                    CREATE TABLE AccountProviderLink_v2(
                        LinkId TEXT PRIMARY KEY,
                        AccountId TEXT NOT NULL REFERENCES DulukaAccount(AccountId),
                        ProviderKey TEXT NOT NULL,
                        ProviderUserId TEXT NOT NULL,
                        ProviderEmail TEXT,
                        Status TEXT NOT NULL DEFAULT 'Active',
                        LinkedAt TEXT NOT NULL,
                        UnlinkedAt TEXT)
                    """;
                create.ExecuteNonQuery();
            }
            using (var copy = _conn.CreateCommand())
            {
                copy.Transaction = tx;
                copy.CommandText = """
                    INSERT INTO AccountProviderLink_v2(LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt, UnlinkedAt)
                    SELECT LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt, UnlinkedAt FROM AccountProviderLink
                    """;
                copy.ExecuteNonQuery();
            }
            using (var drop = _conn.CreateCommand())
            {
                drop.Transaction = tx;
                drop.CommandText = "DROP TABLE AccountProviderLink";
                drop.ExecuteNonQuery();
            }
            using (var rename = _conn.CreateCommand())
            {
                rename.Transaction = tx;
                rename.CommandText = "ALTER TABLE AccountProviderLink_v2 RENAME TO AccountProviderLink";
                rename.ExecuteNonQuery();
            }
            tx.Commit();
        }
        finally
        {
            SetForeignKeys(true);
        }
    }

    private void SetForeignKeys(bool on)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = on ? "PRAGMA foreign_keys=ON" : "PRAGMA foreign_keys=OFF";
        cmd.ExecuteNonQuery();
    }

    // ─── Accounts ───────────────────────────────────────────────────────────

    public DulukaAccount CreateAccount(string? displayName)
    {
        var account = new DulukaAccount(
            Secrets.NewToken("duluka_acc_"), AccountStatus.Active, displayName,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        Exec("INSERT INTO DulukaAccount(AccountId, Status, DisplayName, CreatedAt, UpdatedAt) " +
             "VALUES ($id, $st, $dn, $ca, $ua)",
            ("$id", account.AccountId), ("$st", account.Status), ("$dn", (object?)account.DisplayName ?? DBNull.Value),
            ("$ca", account.CreatedAt.ToString("o")), ("$ua", account.UpdatedAt.ToString("o")));
        return account;
    }

    public DulukaAccount? GetAccount(string accountId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT AccountId, Status, DisplayName, CreatedAt, UpdatedAt, ProfileImage FROM DulukaAccount WHERE AccountId=$id";
        cmd.Parameters.AddWithValue("$id", accountId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapAccount(r) : null;
    }

    private static DulukaAccount MapAccount(SqliteDataReader r) => new(
        r.GetString(0), r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2),
        DateTimeOffset.Parse(r.GetString(3)), DateTimeOffset.Parse(r.GetString(4)),
        r.IsDBNull(5) ? null : r.GetString(5));

    /// <summary>
    /// Update ONLY the profile presentation fields (DisplayName, ProfileImage).
    /// Identity (AccountId/Status/CreatedAt), the username credential and every
    /// device/session row are deliberately OUTSIDE this statement — a profile
    /// edit can never re-key an account or disturb its sessions.
    /// </summary>
    public void UpdateAccountProfile(string accountId, string? displayName, string? profileImage)
    {
        Exec("UPDATE DulukaAccount SET DisplayName=$dn, ProfileImage=$pi, UpdatedAt=$ua WHERE AccountId=$id",
            ("$dn", (object?)displayName ?? DBNull.Value),
            ("$pi", (object?)profileImage ?? DBNull.Value),
            ("$ua", DateTimeOffset.UtcNow.ToString("o")),
            ("$id", accountId));
    }

    // ─── Native credentials ─────────────────────────────────────────────────

    /// <summary>
    /// Create an account AND its native username/password credential in ONE
    /// transaction. On a username-unique violation the whole thing rolls back —
    /// there is never an orphan account without its credential — and the caller
    /// answers conflict.username_taken.
    /// </summary>
    public DulukaAccount CreateNativeAccount(string usernameCanonical, string usernameDisplay, string passwordHash)
    {
        var now = DateTimeOffset.UtcNow;
        var account = new DulukaAccount(
            Secrets.NewToken("duluka_acc_"), AccountStatus.Active, usernameDisplay, now, now);
        using var tx = _conn.BeginTransaction();
        try
        {
            using (var cmd = _conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO DulukaAccount(AccountId, Status, DisplayName, CreatedAt, UpdatedAt) " +
                                  "VALUES ($id, $st, $dn, $ca, $ua)";
                cmd.Parameters.AddWithValue("$id", account.AccountId);
                cmd.Parameters.AddWithValue("$st", account.Status);
                cmd.Parameters.AddWithValue("$dn", (object?)account.DisplayName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("$ca", account.CreatedAt.ToString("o"));
                cmd.Parameters.AddWithValue("$ua", account.UpdatedAt.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            using (var cmd = _conn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO NativeCredential(AccountId, UsernameCanonical, UsernameDisplay, PasswordHash, CreatedAt, UpdatedAt) " +
                                  "VALUES ($aid, $uc, $ud, $ph, $ca, $ua)";
                cmd.Parameters.AddWithValue("$aid", account.AccountId);
                cmd.Parameters.AddWithValue("$uc", usernameCanonical);
                cmd.Parameters.AddWithValue("$ud", usernameDisplay);
                cmd.Parameters.AddWithValue("$ph", passwordHash);
                cmd.Parameters.AddWithValue("$ca", now.ToString("o"));
                cmd.Parameters.AddWithValue("$ua", now.ToString("o"));
                cmd.ExecuteNonQuery();
            }
            tx.Commit();
            return account;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            tx.Rollback();
            throw new InvalidOperationException("username_taken", ex);
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public NativeCredential? FindNativeCredentialByUsername(string usernameCanonical)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT AccountId, UsernameCanonical, UsernameDisplay, PasswordHash, CreatedAt, UpdatedAt " +
                          "FROM NativeCredential WHERE UsernameCanonical=$uc";
        cmd.Parameters.AddWithValue("$uc", usernameCanonical);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapNativeCredential(r) : null;
    }

    public NativeCredential? FindNativeCredentialByAccount(string accountId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT AccountId, UsernameCanonical, UsernameDisplay, PasswordHash, CreatedAt, UpdatedAt " +
                          "FROM NativeCredential WHERE AccountId=$aid";
        cmd.Parameters.AddWithValue("$aid", accountId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapNativeCredential(r) : null;
    }

    public bool HasNativeCredential(string accountId)
        => FindNativeCredentialByAccount(accountId) is not null;

    /// <summary>Add a native credential to an EXISTING account (provider-only
    /// accounts adopting a password). Username uniqueness is DB-enforced; the
    /// AccountId PK guarantees one credential per account.</summary>
    public NativeCredential CreateNativeCredential(string accountId, string usernameCanonical,
        string usernameDisplay, string passwordHash)
    {
        var now = DateTimeOffset.UtcNow;
        try
        {
            Exec("INSERT INTO NativeCredential(AccountId, UsernameCanonical, UsernameDisplay, PasswordHash, CreatedAt, UpdatedAt) " +
                 "VALUES ($aid, $uc, $ud, $ph, $ca, $ua)",
                ("$aid", accountId), ("$uc", usernameCanonical), ("$ud", usernameDisplay),
                ("$ph", passwordHash), ("$ca", now.ToString("o")), ("$ua", now.ToString("o")));
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException("username_taken", ex);
        }
        return new NativeCredential(accountId, usernameCanonical, usernameDisplay,
            passwordHash, now, now);
    }

    public void UpdateNativePassword(string accountId, string newPasswordHash)
    {
        Exec("UPDATE NativeCredential SET PasswordHash=$ph, UpdatedAt=$ua WHERE AccountId=$aid",
            ("$ph", newPasswordHash), ("$ua", DateTimeOffset.UtcNow.ToString("o")), ("$aid", accountId));
    }

    private static NativeCredential MapNativeCredential(SqliteDataReader r) => new(
        r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
        DateTimeOffset.Parse(r.GetString(4)), DateTimeOffset.Parse(r.GetString(5)));

    // ─── Provider links ─────────────────────────────────────────────────────

    /// <summary>
    /// Upsert by the (ProviderKey, ProviderUserId) anchor. On unique-constraint
    /// violation (the duplicate-account race) re-reads the existing row instead
    /// of creating a second account — this is the anti-duplicate fallback.
    /// Returns the link AND whether an account already existed for it.
    /// </summary>
    public (AccountProviderLink Link, DulukaAccount Account, bool Existed) UpsertLink(
        string providerKey, string providerUserId, string? providerEmail, string? displayName)
    {
        var existing = FindActiveLink(providerKey, providerUserId);
        if (existing is not null)
        {
            var acc = GetAccount(existing.AccountId)!;
            return (existing, acc, true);
        }

        var account = CreateAccount(displayName);
        var link = new AccountProviderLink(
            Secrets.NewToken("duluka_link_"), account.AccountId, providerKey, providerUserId,
            providerEmail, LinkStatus.Active, DateTimeOffset.UtcNow, null);
        try
        {
            Exec("INSERT INTO AccountProviderLink(LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt) " +
                 "VALUES ($lid, $aid, $pk, $puid, $pe, 'Active', $la)",
                ("$lid", link.LinkId), ("$aid", link.AccountId), ("$pk", link.ProviderKey),
                ("$puid", link.ProviderUserId), ("$pe", (object?)link.ProviderEmail ?? DBNull.Value),
                ("$la", link.LinkedAt.ToString("o")));
            return (link, account, false);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            // Lost the unique-anchor race (concurrent first-login for the same
            // provider identity). Converge on the winner's account.
            var winner = FindActiveLink(providerKey, providerUserId)
                         ?? throw new InvalidOperationException(
                             "unique-anchor race converged to no row", ex);
            Exec("DELETE FROM DulukaAccount WHERE AccountId=$id AND AccountId NOT IN " +
                 "(SELECT AccountId FROM AccountProviderLink WHERE ProviderKey=$pk AND ProviderUserId=$puid)",
                ("$id", account.AccountId), ("$pk", providerKey), ("$puid", providerUserId));
            return (winner, GetAccount(winner.AccountId)!, true);
        }
    }

    public AccountProviderLink? FindActiveLink(string providerKey, string providerUserId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt, UnlinkedAt " +
                          "FROM AccountProviderLink WHERE ProviderKey=$pk AND ProviderUserId=$puid AND Status='Active'";
        cmd.Parameters.AddWithValue("$pk", providerKey);
        cmd.Parameters.AddWithValue("$puid", providerUserId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapLink(r) : null;
    }

    /// <summary>
    /// Attach a NEW provider link to an EXISTING account (the authenticated
    /// link flow). Unlike UpsertLink it never creates an account. On the
    /// unique-anchor race it returns the WINNER's row — the caller MUST check
    /// AccountId ownership before reporting success (no silent cross-account
    /// merge, contract §6.2).
    /// </summary>
    public AccountProviderLink CreateLink(string accountId, string providerKey, string providerUserId,
        string? providerEmail, string? displayName)
    {
        var link = new AccountProviderLink(
            Secrets.NewToken("duluka_link_"), accountId, providerKey, providerUserId,
            providerEmail, LinkStatus.Active, DateTimeOffset.UtcNow, null);
        try
        {
            Exec("INSERT INTO AccountProviderLink(LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt) " +
                 "VALUES ($lid, $aid, $pk, $puid, $pe, 'Active', $la)",
                ("$lid", link.LinkId), ("$aid", link.AccountId), ("$pk", link.ProviderKey),
                ("$puid", link.ProviderUserId), ("$pe", (object?)link.ProviderEmail ?? DBNull.Value),
                ("$la", link.LinkedAt.ToString("o")));
            return link;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            return FindActiveLink(providerKey, providerUserId)
                   ?? throw new InvalidOperationException("unique-anchor race converged to no row", ex);
        }
    }

    public AccountProviderLink? GetLink(string linkId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt, UnlinkedAt " +
                          "FROM AccountProviderLink WHERE LinkId=$lid";
        cmd.Parameters.AddWithValue("$lid", linkId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapLink(r) : null;
    }

    /// <summary>Explicit insert of a fully-formed link (test seeding / future
    /// admin flows). UpsertLink is the production path.</summary>
    public void AddLink(AccountProviderLink link)
    {
        Exec("INSERT INTO AccountProviderLink(LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt, UnlinkedAt) " +
             "VALUES ($lid, $aid, $pk, $puid, $pe, $st, $la, $ua)",
            ("$lid", link.LinkId), ("$aid", link.AccountId), ("$pk", link.ProviderKey),
            ("$puid", link.ProviderUserId), ("$pe", (object?)link.ProviderEmail ?? DBNull.Value),
            ("$st", link.Status), ("$la", link.LinkedAt.ToString("o")),
            ("$ua", link.UnlinkedAt is null ? DBNull.Value : link.UnlinkedAt.Value.ToString("o")));
    }

    public IReadOnlyList<AccountProviderLink> LinksForAccount(string accountId)
    {
        var list = new List<AccountProviderLink>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT LinkId, AccountId, ProviderKey, ProviderUserId, ProviderEmail, Status, LinkedAt, UnlinkedAt " +
                          "FROM AccountProviderLink WHERE AccountId=$aid ORDER BY LinkedAt";
        cmd.Parameters.AddWithValue("$aid", accountId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapLink(r));
        return list;
    }

    public int ActiveLinkCount(string accountId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM AccountProviderLink WHERE AccountId=$aid AND Status='Active'";
        cmd.Parameters.AddWithValue("$aid", accountId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Unlink(string linkId, DateTimeOffset at)
    {
        Exec("UPDATE AccountProviderLink SET Status='Unlinked', UnlinkedAt=$at WHERE LinkId=$lid",
            ("$at", at.ToString("o")), ("$lid", linkId));
        // Credential rows are destroyed with the link — no provider token outlives it.
        Exec("DELETE FROM CredentialReference WHERE LinkId=$lid", ("$lid", linkId));
    }

    private static AccountProviderLink MapLink(SqliteDataReader r) => new(
        r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
        r.IsDBNull(4) ? null : r.GetString(4), r.GetString(5),
        DateTimeOffset.Parse(r.GetString(6)), r.IsDBNull(7) ? null : DateTimeOffset.Parse(r.GetString(7)));

    // ─── Devices ────────────────────────────────────────────────────────────

    public AccountDevice? FindDeviceByKeyHash(string deviceKeyHash)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT DeviceId, AccountId, DeviceName, DeviceKeyHash, CreatedAt, LastSeenAt, RevokedAt " +
                          "FROM AccountDevice WHERE DeviceKeyHash=$h";
        cmd.Parameters.AddWithValue("$h", deviceKeyHash);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapDevice(r) : null;
    }

    public AccountDevice CreateDevice(string accountId, string deviceName, string deviceKeyHash)
    {
        var device = new AccountDevice(
            Secrets.NewToken("duluka_dev_"), accountId, deviceName, deviceKeyHash,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null);
        Exec("INSERT INTO AccountDevice(DeviceId, AccountId, DeviceName, DeviceKeyHash, CreatedAt, LastSeenAt) " +
             "VALUES ($id, $aid, $dn, $kh, $ca, $lsa)",
            ("$id", device.DeviceId), ("$aid", device.AccountId), ("$dn", device.DeviceName),
            ("$kh", device.DeviceKeyHash), ("$ca", device.CreatedAt.ToString("o")),
            ("$lsa", device.LastSeenAt.ToString("o")));
        return device;
    }

    public void TouchDevice(string deviceId)
    {
        Exec("UPDATE AccountDevice SET LastSeenAt=$t WHERE DeviceId=$id",
            ("$t", DateTimeOffset.UtcNow.ToString("o")), ("$id", deviceId));
    }

    public IReadOnlyList<AccountDevice> DevicesForAccount(string accountId)
    {
        var list = new List<AccountDevice>();
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT DeviceId, AccountId, DeviceName, DeviceKeyHash, CreatedAt, LastSeenAt, RevokedAt " +
                          "FROM AccountDevice WHERE AccountId=$aid ORDER BY CreatedAt";
        cmd.Parameters.AddWithValue("$aid", accountId);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(MapDevice(r));
        return list;
    }

    public AccountDevice? GetDevice(string deviceId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT DeviceId, AccountId, DeviceName, DeviceKeyHash, CreatedAt, LastSeenAt, RevokedAt " +
                          "FROM AccountDevice WHERE DeviceId=$id";
        cmd.Parameters.AddWithValue("$id", deviceId);
        using var r = cmd.ExecuteReader();
        return r.Read() ? MapDevice(r) : null;
    }

    public int RevokeDevice(string deviceId, DateTimeOffset at)
    {
        Exec("UPDATE AccountDevice SET RevokedAt=$at WHERE DeviceId=$id AND RevokedAt IS NULL",
            ("$at", at.ToString("o")), ("$id", deviceId));
        return RevokeSessionsForDevice(deviceId, at, "device-revoke");
    }

    private static AccountDevice MapDevice(SqliteDataReader r) => new(
        r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
        DateTimeOffset.Parse(r.GetString(4)), DateTimeOffset.Parse(r.GetString(5)),
        r.IsDBNull(6) ? null : DateTimeOffset.Parse(r.GetString(6)));

    // ─── Sessions ───────────────────────────────────────────────────────────

    public AccountSession CreateSession(string accountId, string deviceId, string? viaLinkId,
        string tokenHash, DateTimeOffset expiresAt)
    {
        var now = DateTimeOffset.UtcNow;
        // Contract §5.2: exactly ONE active session per DeviceId — a second
        // login supersedes the previous session FIRST (revoke-before-insert;
        // the transient zero-active window is the fail-closed direction and
        // invisible on the single connection).
        Exec("UPDATE AccountSession SET RevokedAt=$at, RevokedReason='superseded' " +
             "WHERE DeviceId=$did AND RevokedAt IS NULL",
            ("$at", now.ToString("o")), ("$did", deviceId));
        var session = new AccountSession(
            Secrets.NewToken("duluka_sess_"), tokenHash, accountId, deviceId, viaLinkId,
            now, expiresAt, now, null, null);
        Exec("INSERT INTO AccountSession(SessionId, SessionTokenHash, AccountId, DeviceId, IssuedViaLinkId, CreatedAt, ExpiresAt, LastSeenAt) " +
             "VALUES ($sid, $th, $aid, $did, $via, $ca, $ea, $lsa)",
            ("$sid", session.SessionId), ("$th", session.SessionTokenHash), ("$aid", session.AccountId),
            ("$did", session.DeviceId), ("$via", (object?)session.IssuedViaLinkId ?? DBNull.Value),
            ("$ca", session.CreatedAt.ToString("o")), ("$ea", session.ExpiresAt.ToString("o")),
            ("$lsa", session.LastSeenAt.ToString("o")));
        return session;
    }

    /// <summary>Full-chain validation: token hash → session not revoked/expired →
    /// device not revoked → account Active. Any broken link means 401.</summary>
    public (AccountSession Session, AccountDevice Device, DulukaAccount Account)? ValidateSession(string tokenHash)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = """
            SELECT s.SessionId, s.SessionTokenHash, s.AccountId, s.DeviceId, s.IssuedViaLinkId,
                   s.CreatedAt, s.ExpiresAt, s.LastSeenAt, s.RevokedAt, s.RevokedReason,
                   d.RevokedAt, a.Status
            FROM AccountSession s
            JOIN AccountDevice d ON d.DeviceId = s.DeviceId
            JOIN DulukaAccount a ON a.AccountId = s.AccountId
            WHERE s.SessionTokenHash = $th
            """;
        cmd.Parameters.AddWithValue("$th", tokenHash);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        var session = new AccountSession(
            r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
            r.IsDBNull(4) ? null : r.GetString(4), DateTimeOffset.Parse(r.GetString(5)),
            DateTimeOffset.Parse(r.GetString(6)), DateTimeOffset.Parse(r.GetString(7)),
            r.IsDBNull(8) ? null : DateTimeOffset.Parse(r.GetString(8)),
            r.IsDBNull(9) ? null : r.GetString(9));
        if (session.RevokedAt is not null) return null;
        if (session.ExpiresAt <= DateTimeOffset.UtcNow) return null;
        if (!r.IsDBNull(10)) return null;                                   // device revoked
        var status = r.GetString(11);
        if (!string.Equals(status, AccountStatus.Active, StringComparison.Ordinal)) return null;

        var device = GetDevice(session.DeviceId)!;
        var account = GetAccount(session.AccountId)!;
        TouchSession(session.SessionId);
        return (session, device, account);
    }

    public void TouchSession(string sessionId)
    {
        Exec("UPDATE AccountSession SET LastSeenAt=$t WHERE SessionId=$id",
            ("$t", DateTimeOffset.UtcNow.ToString("o")), ("$id", sessionId));
    }

    /// <summary>Sliding refresh bounded by the absolute cap from creation.
    /// The absolute window is the configured Session:AbsoluteDays value —
    /// never a hardcoded constant (operators may shorten the TTL).</summary>
    public DateTimeOffset? RefreshSession(string tokenHash, TimeSpan slidingWindow, TimeSpan absoluteWindow)
    {
        var validation = ValidateSession(tokenHash);
        if (validation is null) return null;
        var (session, _, _) = validation.Value;
        var proposed = DateTimeOffset.UtcNow + slidingWindow;
        var capped = DateTimeOffset.Compare(proposed, session.CreatedAt + absoluteWindow) > 0
            ? session.CreatedAt + absoluteWindow
            : proposed;
        if (capped <= session.ExpiresAt) return session.ExpiresAt;
        Exec("UPDATE AccountSession SET ExpiresAt=$ea WHERE SessionId=$id",
            ("$ea", capped.ToString("o")), ("$id", session.SessionId));
        return capped;
    }

    /// <summary>Whether this token hash belongs to a REVOKED session row —
    /// lets a 401 answer with auth.session_revoked instead of the generic
    /// auth.session_expired (§7.2 registry codes are distinguishable).</summary>
    public bool SessionTokenWasRevoked(string tokenHash)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM AccountSession WHERE SessionTokenHash=$th AND RevokedAt IS NOT NULL";
        cmd.Parameters.AddWithValue("$th", tokenHash);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public bool RevokeSessionByTokenHash(string tokenHash, string reason, DateTimeOffset at)
    {
        return Exec("UPDATE AccountSession SET RevokedAt=$at, RevokedReason=$r " +
                    "WHERE SessionTokenHash=$th AND RevokedAt IS NULL",
            ("$at", at.ToString("o")), ("$r", reason), ("$th", tokenHash)) == 1;
    }

    public int RevokeSessionsForDevice(string deviceId, DateTimeOffset at, string reason)
    {
        return Exec("UPDATE AccountSession SET RevokedAt=$at, RevokedReason=$r " +
                    "WHERE DeviceId=$id AND RevokedAt IS NULL",
            ("$at", at.ToString("o")), ("$r", reason), ("$id", deviceId));
    }

    public int RevokeSessionsForLink(string linkId, DateTimeOffset at, string reason)
    {
        return Exec("UPDATE AccountSession SET RevokedAt=$at, RevokedReason=$r " +
                    "WHERE IssuedViaLinkId=$lid AND RevokedAt IS NULL",
            ("$at", at.ToString("o")), ("$r", reason), ("$lid", linkId));
    }

    public int RevokeAllForAccount(string accountId, DateTimeOffset at, string reason)
    {
        return Exec("UPDATE AccountSession SET RevokedAt=$at, RevokedReason=$r " +
                    "WHERE AccountId=$aid AND RevokedAt IS NULL",
            ("$at", at.ToString("o")), ("$r", reason), ("$aid", accountId));
    }

    /// <summary>User-initiated IRREVERSIBLE account deletion. Every dependent
    /// row is removed in ONE transaction, children before parents so the FK
    /// chain (sessions→devices, credential-references→links, …) never blocks.
    /// Deleting — not just revoking — frees every UNIQUE anchor the account
    /// holds: the UsernameCanonical, the device key hashes and the
    /// (ProviderKey, ProviderUserId) identity anchor. Consequences by design:
    /// the username may be registered again, the same device key may enroll
    /// again, and the same GitHub identity may bootstrap a FRESH account.
    /// Session rows are DELETED (not revoked): a post-deletion token validates
    /// to a plain unknown session. Returns the number of session rows that
    /// died with the account.</summary>
    public int DeleteAccountCascade(string accountId)
    {
        using var tx = _conn.BeginTransaction();
        try
        {
            int sessions = ExecTx(tx,
                "DELETE FROM AccountSession WHERE AccountId=$aid", ("$aid", accountId));
            ExecTx(tx, "DELETE FROM AccountDevice WHERE AccountId=$aid", ("$aid", accountId));
            ExecTx(tx,
                "DELETE FROM CredentialReference WHERE LinkId IN " +
                "(SELECT LinkId FROM AccountProviderLink WHERE AccountId=$aid)", ("$aid", accountId));
            ExecTx(tx, "DELETE FROM AccountProviderLink WHERE AccountId=$aid", ("$aid", accountId));
            ExecTx(tx, "DELETE FROM NativeCredential WHERE AccountId=$aid", ("$aid", accountId));
            ExecTx(tx, "DELETE FROM SyncProfile WHERE AccountId=$aid", ("$aid", accountId));
            ExecTx(tx, "DELETE FROM DulukaAccount WHERE AccountId=$aid", ("$aid", accountId));
            tx.Commit();
            _logger.LogInformation(
                "Account {AccountId} deleted (cascade): {Sessions} session rows removed", accountId, sessions);
            return sessions;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // ─── infrastructure ─────────────────────────────────────────────────────

    private int Exec(string sql, params (string Name, object Value)[] parameters)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        return cmd.ExecuteNonQuery();
    }

    /// <summary>Exec inside an EXPLICIT transaction (cascade paths). Mirrors
    /// Exec exactly, but assigns the caller's transaction to the command —
    /// Microsoft.Data.Sqlite refuses a command that runs while a transaction
    /// is open on the connection without that assignment.</summary>
    private int ExecTx(SqliteTransaction tx, string sql, params (string Name, object Value)[] parameters)
    {
        using var cmd = _conn.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        return cmd.ExecuteNonQuery();
    }

    public async ValueTask DisposeAsync() => await _conn.DisposeAsync();
}
