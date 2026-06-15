using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ScottBrady91.AspNetCore.Identity;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Web.Services;

[Injectable(InjectionType.Singleton)]
public class Argon2idPasswordHasher : IPasswordHasher
{
    private const Argon2HashStrength Strength = Argon2HashStrength.Interactive;

    private static readonly object _dummyUser = new(); // Shared instance avoids per-call allocation.

    private readonly Argon2PasswordHasher<object> _hasher = new(Options.Create(new Argon2PasswordHasherOptions { Strength = Strength }));

    public Argon2idPasswordHasher()
    {
        DummyHash = Hash(Guid.NewGuid().ToString("N"));
    }

    public string DummyHash { get; }

    public string Hash(string password)
    {
        return _hasher.HashPassword(_dummyUser, password);
    }

    public bool IsEncodedHash(string value)
    {
        // Argon2 hashes are always strings that start with the variant marker, e.g. "$argon2id$".
        return !string.IsNullOrEmpty(value) && value.StartsWith("$argon2", StringComparison.Ordinal);
    }

    public bool Verify(string password, string encodedHash, out bool needsRehash)
    {
        needsRehash = false;

        if (string.IsNullOrEmpty(encodedHash) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        PasswordVerificationResult result;
        try
        {
            result = _hasher.VerifyHashedPassword(_dummyUser, encodedHash, password);
        }
        catch
        {
            return false; // Always fail closed.
        }

        if (result == PasswordVerificationResult.Failed)
        {
            return false;
        }

        needsRehash = result == PasswordVerificationResult.SuccessRehashNeeded;
        return true;
    }
}
