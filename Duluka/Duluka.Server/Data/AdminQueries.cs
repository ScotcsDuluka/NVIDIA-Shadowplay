using Duluka.Server.Domain;
using Microsoft.Data.Sqlite;

namespace Duluka.Server.Data;

// ─── Admin console queries (local operator surface) ─────────────────────────
// Read-mostly projections for the /admin operator dashboard plus the small
// set of destructive operations it exposes (revoke session, revoke/delete
// device, unlink link, suspend account, delete account). Every destructive
// operation runs in ONE transaction on the shared connection and removes
// child rows explicitly — the v0 schema declares no ON DELETE CASCADE, so a
// bare parent DELETE would either fail on the foreign key or orphan children.
// These methods NEVER touch secrets: token/key hashes stay unread.
public sealed partial class Database
{
    public sealed record AdminOverview(
        int Accounts, int ActiveSessions, int ActiveDevices,
        int ActiveLinks, int NativeCredentials, string DbPath);

    public sealed record AdminAccountRow(
        string AccountId, string Status, string? DisplayName, string? Username,
        bool HasProfileImage, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt,
        int ActiveSessions, int Devices, int ActiveLinks);

    public sealed record AdminSessionRow(
        string SessionId, string AccountId, string AccountLabel, string DeviceId, string? DeviceName,
        DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, DateTimeOffset LastSeenAt,
        DateTimeOffset? RevokedAt, string? RevokedReason);

    public sealed record AdminDeviceRow(
        string DeviceId, string AccountId, string AccountLabel, string DeviceName,
        DateTimeOffset CreatedAt, DateTimeOffset LastSeenAt, DateTimeOffset? RevokedAt,
        int ActiveSessions);

    public sealed record AdminLinkRow(
        string LinkId, string AccountId, string AccountLabel, string ProviderKey,
        string ProviderUserId, string? ProviderEmail, string Status,
        DateTimeOffset LinkedAt, DateTimeOffset? UnlinkedAt);

    private static string? NStr(SqliteDataReader r, int i) =>
        r.IsDBNull(i) ? null : r.GetString(i);

    /// <summary>A human label for the account's owner row: the native username
    /// when present, else the display name, else the raw account id.</summary>
    private const string AccountLabelSql = "COALESCE(n.UsernameDisplay, a.DisplayName, a.AccountId)";

    private static bool IsExpired(string expiresAt) =>
        DateTimeOffset.TryParse(expiresAt, out var e) && e <= DateTimeOffset.UtcNow;

    public AdminOverview AdminGetOverview()
    {
        var now = DateTimeOffset.UtcNow.ToString("o");
        int Count(string sql, params (string Name, object Value)[] ps)
        {
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = sql;
            foreach (var (name, value) in ps) cmd.Parameters.AddWithValue(name, value);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        return new AdminOverview(
            Accounts: Count("SELECT COUNT(*) FROM DulukaAccount"),
            ActiveSessions: Count("SELECT COUNT(*) FROM AccountSession WHERE RevokedAt IS NULL AND ExpiresAt > $now", ("$now", now)),
            ActiveDevices: Count("SELECT COUNT(*) FROM AccountDevice WHERE RevokedAt IS NULL"),
            ActiveLinks: Count("SELECT COUNT(*) FROM AccountProviderLink WHERE Status='Active'"),
            NativeCredentials: Count("SELECT COUNT(*) FROM NativeCredential"),
            DbPath: DbPath);
    }

    public IReadOnlyList<AdminAccountRow> AdminListAccounts(int limit = 500)
    {
        var now = DateTimeOffset.UtcNow.ToString("o");
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $$"""
            SELECT a.AccountId, a.Status, a.DisplayName, n.UsernameDisplay,
                   (a.ProfileImage IS NOT NULL), a.CreatedAt, a.UpdatedAt,
                   (SELECT COUNT(*) FROM AccountSession s WHERE s.AccountId = a.AccountId AND s.RevokedAt IS NULL AND s.ExpiresAt > $now),
                   (SELECT COUNT(*) FROM AccountDevice d WHERE d.AccountId = a.AccountId AND d.RevokedAt IS NULL),
                   (SELECT COUNT(*) FROM AccountProviderLink l WHERE l.AccountId = a.AccountId AND l.Status = 'Active')
            FROM DulukaAccount a
            LEFT JOIN NativeCredential n ON n.AccountId = a.AccountId
            ORDER BY a.CreatedAt DESC
            LIMIT $lim
            """;
        cmd.Parameters.AddWithValue("$now", now);
        cmd.Parameters.AddWithValue("$lim", limit);
        var rows = new List<AdminAccountRow>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            rows.Add(new AdminAccountRow(
                r.GetString(0), r.GetString(1), NStr(r, 2), NStr(r, 3),
                r.GetInt64(4) != 0, DateTimeOffset.Parse(r.GetString(5)), DateTimeOffset.Parse(r.GetString(6)),
                (int)r.GetInt64(7), (int)r.GetInt64(8), (int)r.GetInt64(9)));
        return rows;
    }

    public IReadOnlyList<AdminSessionRow> AdminListSessions(int limit = 300)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $$"""
            SELECT s.SessionId, s.AccountId, {{AccountLabelSql}}, s.DeviceId, d.DeviceName,
                   s.CreatedAt, s.ExpiresAt, s.LastSeenAt, s.RevokedAt, s.RevokedReason
            FROM AccountSession s
            JOIN DulukaAccount a ON a.AccountId = s.AccountId
            JOIN AccountDevice d ON d.DeviceId = s.DeviceId
            LEFT JOIN NativeCredential n ON n.AccountId = a.AccountId
            ORDER BY s.CreatedAt DESC
            LIMIT $lim
            """;
        cmd.Parameters.AddWithValue("$lim", limit);
        var rows = new List<AdminSessionRow>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            rows.Add(new AdminSessionRow(
                r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3), NStr(r, 4),
                DateTimeOffset.Parse(r.GetString(5)), DateTimeOffset.Parse(r.GetString(6)),
                DateTimeOffset.Parse(r.GetString(7)), NStr(r, 8) is null ? null : DateTimeOffset.Parse(r.GetString(8)),
                NStr(r, 9)));
        return rows;
    }

    public IReadOnlyList<AdminDeviceRow> AdminListDevices(int limit = 300)
    {
        var now = DateTimeOffset.UtcNow.ToString("o");
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $$"""
            SELECT d.DeviceId, d.AccountId, {{AccountLabelSql}}, d.DeviceName,
                   d.CreatedAt, d.LastSeenAt, d.RevokedAt,
                   (SELECT COUNT(*) FROM AccountSession s WHERE s.DeviceId = d.DeviceId AND s.RevokedAt IS NULL AND s.ExpiresAt > $now)
            FROM AccountDevice d
            JOIN DulukaAccount a ON a.AccountId = d.AccountId
            LEFT JOIN NativeCredential n ON n.AccountId = a.AccountId
            ORDER BY d.CreatedAt DESC
            LIMIT $lim
            """;
        cmd.Parameters.AddWithValue("$now", now);
        cmd.Parameters.AddWithValue("$lim", limit);
        var rows = new List<AdminDeviceRow>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            rows.Add(new AdminDeviceRow(
                r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
                DateTimeOffset.Parse(r.GetString(4)), DateTimeOffset.Parse(r.GetString(5)),
                NStr(r, 6) is null ? null : DateTimeOffset.Parse(r.GetString(6)), (int)r.GetInt64(7)));
        return rows;
    }

    public IReadOnlyList<AdminLinkRow> AdminListLinks(int limit = 300)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = $$"""
            SELECT l.LinkId, l.AccountId, {{AccountLabelSql}}, l.ProviderKey,
                   l.ProviderUserId, l.ProviderEmail, l.Status, l.LinkedAt, l.UnlinkedAt
            FROM AccountProviderLink l
            JOIN DulukaAccount a ON a.AccountId = l.AccountId
            LEFT JOIN NativeCredential n ON n.AccountId = a.AccountId
            ORDER BY l.LinkedAt DESC
            LIMIT $lim
            """;
        cmd.Parameters.AddWithValue("$lim", limit);
        var rows = new List<AdminLinkRow>();
        using var r = cmd.ExecuteReader();
        while (r.Read())
            rows.Add(new AdminLinkRow(
                r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3),
                r.GetString(4), NStr(r, 5), r.GetString(6),
                DateTimeOffset.Parse(r.GetString(7)),
                NStr(r, 8) is null ? null : DateTimeOffset.Parse(r.GetString(8))));
        return rows;
    }

    /// <summary>Revoke one session by its id (never by token). Idempotent:
    /// an already-revoked session returns false and changes nothing.</summary>
    public bool AdminRevokeSession(string sessionId)
    {
        return Exec("UPDATE AccountSession SET RevokedAt=$at, RevokedReason='admin-revoked' " +
                    "WHERE SessionId=$sid AND RevokedAt IS NULL",
            ("$at", DateTimeOffset.UtcNow.ToString("o")), ("$sid", sessionId)) > 0;
    }

    /// <summary>Suspend / re-activate an account. Only the two statuses the
    /// session-validation chain understands are accepted. A suspended account
    /// fails validation on its very next request — live sessions die naturally
    /// without a revoke sweep.</summary>
    public bool AdminSetAccountStatus(string accountId, string status)
    {
        if (status is not (AccountStatus.Active or AccountStatus.Suspended))
            throw new ArgumentException($"Status '{status}' is not a valid admin-settable status.");
        return Exec("UPDATE DulukaAccount SET Status=$st, UpdatedAt=$ua WHERE AccountId=$id",
            ("$st", status), ("$ua", DateTimeOffset.UtcNow.ToString("o")), ("$id", accountId)) > 0;
    }

    /// <summary>Hard-delete a device AND its sessions (admin cleanup — distinct
    /// from RevokeDevice, which keeps the history and only invalidates).</summary>
    public bool AdminDeleteDevice(string deviceId)
    {
        using var tx = _conn.BeginTransaction();
        var deleted = Exec("DELETE FROM AccountSession WHERE DeviceId=$id", ("$id", deviceId));
        deleted += Exec("DELETE FROM AccountDevice WHERE DeviceId=$id", ("$id", deviceId));
        tx.Commit();
        return deleted > 0;
    }

    /// <summary>Hard-delete an account and EVERY row that references it, in one
    /// transaction: sessions → devices → credential references → links → sync
    /// profiles → native credential → account. Returns false when the account
    /// does not exist (nothing is deleted).</summary>
    public bool AdminDeleteAccount(string accountId)
    {
        using var tx = _conn.BeginTransaction();
        Exec("DELETE FROM AccountSession WHERE AccountId=$id", ("$id", accountId));
        Exec("DELETE FROM AccountDevice WHERE AccountId=$id", ("$id", accountId));
        Exec("DELETE FROM CredentialReference WHERE LinkId IN " +
             "(SELECT LinkId FROM AccountProviderLink WHERE AccountId=$id)", ("$id", accountId));
        Exec("DELETE FROM AccountProviderLink WHERE AccountId=$id", ("$id", accountId));
        Exec("DELETE FROM SyncProfile WHERE AccountId=$id", ("$id", accountId));
        Exec("DELETE FROM NativeCredential WHERE AccountId=$id", ("$id", accountId));
        var deleted = Exec("DELETE FROM DulukaAccount WHERE AccountId=$id", ("$id", accountId));
        tx.Commit();
        return deleted > 0;
    }
}
