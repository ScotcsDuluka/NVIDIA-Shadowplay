using System.Security.Cryptography;
using System.Text;

namespace Duluka.Server.Security;

/// <summary>
/// C/6 security primitives: opaque token generation, hash-at-rest, PKCE, redaction.
/// Every multi-user secret in this server flows through this file so the hashing /
/// redaction policy has exactly one implementation to audit.
/// </summary>
public static class Secrets
{
    /// <summary>Opaque 256-bit random token, URL-safe, prefixed for identification.</summary>
    public static string NewToken(string prefix) =>
        prefix + Base64Url(RandomNumberGenerator.GetBytes(32));

    /// <summary>PKCE code verifier: 64 random bytes, base64url (86 chars — within RFC 7636 range).</summary>
    public static string NewPkceVerifier() => Base64Url(RandomNumberGenerator.GetBytes(64));

    /// <summary>RFC 7636 S256 challenge: BASE64URL-ENCODE(SHA256(ASCII(verifier))).</summary>
    public static string PkceChallenge(string verifier)
    {
        var challenge = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        return Base64Url(challenge);
    }

    /// <summary>
    /// Hash-at-rest for session tokens / device keys. Raw secrets are NEVER stored —
    /// the database holds only this digest, so a DB leak does not yield usable tokens.
    /// </summary>
    public static string Sha256Hex(string value)
    {
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(digest).ToLowerInvariant();
    }

    /// <summary>
    /// Redact a secret for safe logging: keeps a short prefix so operators can
    /// correlate, destroys the rest. Never log a raw token/code/state value.
    /// </summary>
    public static string Redact(string? secret)
    {
        if (string.IsNullOrEmpty(secret)) return "(empty)";
        var prefix = secret.Length <= 6 ? new string('·', secret.Length) : secret[..6];
        return $"{prefix}…(len={secret.Length})";
    }

    public static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>Constant-time equality for secret comparisons (state keys, hashes).</summary>
    public static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }

    // ─── password hashing (native Duluka credentials) ───────────────────────
    //
    // Framework primitive only (PBKDF2 via Rfc2898DeriveBytes) — no custom
    // crypto. Slow (210k SHA-256 iterations, OWASP guidance), salted per
    // credential, verified in constant time. Stored format:
    //   pbkdf2-sha256$<iterations>$<saltBase64>$<hashBase64>
    // The verifier is the ONLY thing persisted; the plaintext never leaves the
    // parameter and is never logged.

    private const int PasswordIterations = 210_000;
    private const int PasswordSaltBytes = 16;
    private const int PasswordHashBytes = 32;

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(PasswordSaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, PasswordIterations,
            HashAlgorithmName.SHA256, PasswordHashBytes);
        return FormattableString.Invariant(
            $"pbkdf2-sha256${PasswordIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}");
    }

    public static bool VerifyPassword(string password, string stored)
    {
        try
        {
            var parts = stored.Split('$');
            if (parts.Length != 4 || parts[0] != "pbkdf2-sha256") return false;
            if (!int.TryParse(parts[1], System.Globalization.CultureInfo.InvariantCulture, out var iterations))
                return false;
            if (iterations < 1) return false;
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations,
                HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException) { return false; }
        catch (ArgumentException) { return false; }
    }
}
