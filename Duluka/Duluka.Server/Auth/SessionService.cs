using Duluka.Server.Data;
using Duluka.Server.Domain;
using Duluka.Server.Security;

namespace Duluka.Server.Auth;

/// <summary>
/// Session issuance/validation. The client receives an opaque random token ONCE;
/// the database keeps only its SHA-256 digest. Validation is a full-chain check
/// (session → device → account) so a revoke at any layer takes effect on the
/// next request without waiting for expiry.
/// </summary>
public sealed class SessionService(Database db, IConfiguration config)
{
    public const string TokenPrefix = "duluka_st_";

    private TimeSpan AbsoluteDays => TimeSpan.FromDays(config.GetValue("Session:AbsoluteDays", 30));
    private TimeSpan SlidingDays => TimeSpan.FromDays(config.GetValue("Session:SlidingDays", 7));

    public (string Token, AccountSession Session) Create(string accountId, string deviceId, string? viaLinkId)
    {
        var token = Secrets.NewToken(TokenPrefix);
        var expires = DateTimeOffset.UtcNow + AbsoluteDays;
        var session = db.CreateSession(accountId, deviceId, viaLinkId, Secrets.Sha256Hex(token), expires);
        return (token, session);
    }

    public (AccountSession Session, AccountDevice Device, DulukaAccount Account)? Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || !token.StartsWith(TokenPrefix, StringComparison.Ordinal))
            return null;
        return db.ValidateSession(Secrets.Sha256Hex(token));
    }

    public DateTimeOffset? Refresh(string token) =>
        db.RefreshSession(Secrets.Sha256Hex(token), SlidingDays, AbsoluteDays);

    public bool Revoke(string token, string reason) =>
        db.RevokeSessionByTokenHash(Secrets.Sha256Hex(token), reason, DateTimeOffset.UtcNow);

    public int RevokeAll(string accountId, string reason) =>
        db.RevokeAllForAccount(accountId, DateTimeOffset.UtcNow, reason);

    /// <summary>Registry code for a session that failed validation (§7.2).
    /// Revocation is distinguishable from expiry and reported as its own
    /// code; unknown/expired/missing all fall under session_expired.</summary>
    public string DeadSessionCode(string token) =>
        db.SessionTokenWasRevoked(Secrets.Sha256Hex(token)) ? "auth.session_revoked" : "auth.session_expired";
}
