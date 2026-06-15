namespace SPTarkov.Server.Web.Services;

/// <summary>
/// Hashes and verifies web-panel account passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password into a hash suitable for storage.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a plaintext password against a previously stored encoded hash. Returns false for a malformed or
    /// corrupted hash.
    /// </summary>
    /// <param name="password">The plaintext password supplied at login.</param>
    /// <param name="encodedHash">The stored encoded hash to verify against.</param>
    /// <param name="needsRehash">
    /// True when the password was correct but the stored hash was produced with cost parameters that differ from the
    /// current ones, so the caller should re-hash and persist it. Always false on a failed verification.
    /// </param>
    bool Verify(string password, string encodedHash, out bool needsRehash);

    /// <summary>
    /// Returns true when the supplied value is already one of this hasher's encoded hashes, rather than a plaintext
    /// secret. Used to detect pre-hashing credentials that still need migrating.
    /// </summary>
    bool IsEncodedHash(string value);

    /// <summary>
    /// A throwaway hash whose verification cost matches a real one. Verify a failed login against this so response
    /// timing is the same whether the supplied username exists.
    /// </summary>
    string DummyHash { get; }
}
