using NUnit.Framework;
using SPTarkov.Server.Core.Services;

namespace UnitTests.Tests.Services;

[TestFixture]
public class LocationLifecycleServiceTests
{
    private LocationLifecycleService _locationLifecycle;

    [OneTimeSetUp]
    public void Initialize()
    {
        _locationLifecycle = DI.GetInstance().GetService<LocationLifecycleService>();
    }

    [Test]
    public void IsSide_ReturnsTrue_ForPmc_DefaultCheck_IsCaseInsensitive()
    {
        // Default side is "pmc"; ensure case-insensitive match works
        Assert.IsTrue(_locationLifecycle.IsSide("PMC", "pmc"));
        Assert.IsTrue(_locationLifecycle.IsSide("pmc", "pmc"));
    }

    [Test]
    public void IsSide_ReturnsFalse_ForNonMatchingSide()
    {
        Assert.IsFalse(_locationLifecycle.IsSide("savage", "pmc"));
        Assert.IsFalse(_locationLifecycle.IsSide("beAr", "pmc"));
    }

    [Test]
    public void IsSide_ReturnsTrue_WhenCheckingScavAgainstSavage_IsCaseInsensitive()
    {
        // In code, scav side string used for extracts is "savage"
        Assert.IsTrue(_locationLifecycle.IsSide("SAVAGE", "savage"));
        Assert.IsTrue(_locationLifecycle.IsSide("savage", "savage"));
    }
}
