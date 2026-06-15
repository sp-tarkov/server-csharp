using Argon2Sharp;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Web.Services;

[Injectable(InjectionType.Singleton)]
public class Argon2idPasswordHasher : IPasswordHasher
{
    // Cost parameters for newly created hashes. RFC 9106's memory-constrained "uniformly safe" recommendation is currently (2026-06-15)
    // 64 MiB of memory, 3 iterations, 4 lanes, a 128-bit salt, and a 256-bit tag.
    private const int MemorySizeKB = 65536;
    private const int Iterations = 3;
    private const int Parallelism = 4;
    private const int HashLength = 32;
    private const int SaltLength = 16;
    private const Argon2Type Variant = Argon2Type.Argon2id;

    // Immutable cost template reused across calls. The salt is a placeholder.
    private readonly Argon2Parameters _parameters = Argon2Parameters
        .CreateBuilder()
        .WithType(Variant)
        .WithMemorySizeKB(MemorySizeKB)
        .WithIterations(Iterations)
        .WithParallelism(Parallelism)
        .WithHashLength(HashLength)
        .WithRandomSalt(SaltLength)
        .Build();

    public Argon2idPasswordHasher()
    {
        DummyHash = Hash(Guid.NewGuid().ToString("N"));
    }

    public string DummyHash { get; }

    public string Hash(string password)
    {
        return Argon2PhcFormat.HashToPhcStringWithAutoSalt(password, _parameters, SaltLength);
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

        bool isValid;
        Argon2Parameters? stored;
        try
        {
            (isValid, stored) = Argon2PhcFormat.VerifyPhcString(password, encodedHash);
        }
        catch
        {
            return false; // Always fail closed.
        }

        if (!isValid || stored is null)
        {
            return false;
        }

        // Flag the hash for upgrade on the next login when it was produced with cost parameters other than current.
        needsRehash =
            stored.MemorySizeKB != MemorySizeKB
            || stored.Iterations != Iterations
            || stored.Parallelism != Parallelism
            || stored.HashLength != HashLength
            || stored.Type != Variant;

        return true;
    }
}
