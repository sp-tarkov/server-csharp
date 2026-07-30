using Argon2Sharp;
using NUnit.Framework;
using SPTarkov.Server.Web.Services;

namespace UnitTests.Tests.Services;

[TestFixture]
public class Argon2idPasswordHasherTests
{
    private Argon2idPasswordHasher _hasher;

    [OneTimeSetUp]
    public void Initialize()
    {
        _hasher = new Argon2idPasswordHasher();
    }

    [Test]
    public void HashProducesArgon2idEncodedString()
    {
        var hash = _hasher.Hash("correct horse battery staple");

        Assert.That(hash, Does.StartWith("$argon2id$"));
    }

    [Test]
    public void VerifyReturnsTrueForCorrectPassword()
    {
        const string password = "correct horse battery staple";
        var hash = _hasher.Hash(password);

        Assert.That(_hasher.Verify(password, hash, out _), Is.True);
    }

    [Test]
    public void VerifyReturnsFalseForWrongPassword()
    {
        var hash = _hasher.Hash("correct horse battery staple");

        Assert.That(_hasher.Verify("staple battery horse correct", hash, out _), Is.False);
    }

    [Test]
    public void VerifyDoesNotFlagRehashForCurrentParameters()
    {
        const string password = "correct horse battery staple";
        var hash = _hasher.Hash(password);
        var ok = _hasher.Verify(password, hash, out var needsRehash);

        Assert.That(ok, Is.True);
        Assert.That(needsRehash, Is.False);
    }

    [Test]
    public void VerifyFlagsRehashWhenStoredParametersDiffer()
    {
        const string password = "correct horse battery staple";

        // A hash produced with weaker cost parameters than the hasher's current settings should verify but also be flagged for upgrade.
        var staleHash = Argon2PhcFormat.HashToPhcStringWithAutoSalt(password);

        var ok = _hasher.Verify(password, staleHash, out var needsRehash);

        Assert.That(ok, Is.True);
        Assert.That(needsRehash, Is.True);
    }

    [Test]
    public void HashUsesRandomSaltSoEqualPasswordsHashDifferently()
    {
        const string password = "correct horse battery staple";

        Assert.That(_hasher.Hash(password), Is.Not.EqualTo(_hasher.Hash(password)));
    }

    [Test]
    public void VerifyFailsClosedOnMalformedHash()
    {
        Assert.That(_hasher.Verify("correct horse battery staple", "not a hash", out _), Is.False);
        Assert.That(_hasher.Verify("correct horse battery staple", string.Empty, out _), Is.False);
    }

    [Test]
    public void DummyHashIsAValidArgon2idHash()
    {
        Assert.That(_hasher.DummyHash, Does.StartWith("$argon2id$"));
    }

    [Test]
    public void IsEncodedHashRecognizesOwnOutput()
    {
        Assert.That(_hasher.IsEncodedHash(_hasher.Hash("correct horse battery staple")), Is.True);
    }

    [Test]
    public void IsEncodedHashRejectsPlaintextAndEmpty()
    {
        Assert.That(_hasher.IsEncodedHash("correct horse battery staple"), Is.False);
        Assert.That(_hasher.IsEncodedHash(string.Empty), Is.False);
    }
}
