using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;

namespace UnitTests.Tests.Services;

[TestFixture]
public class LocationLifecycleServiceTests
{
    // Note: The service has a heavy constructor with many DI deps. For testing the protected IsSide method,
    // we bypass construction entirely and invoke the method via reflection.

    private static bool InvokeIsSide(object instance, string playerSide, string sideCheck)
    {
        var mi = typeof(LocationLifecycleService).GetMethod("IsSide", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(mi, "Could not find protected method IsSide via reflection");

        var result = mi!.Invoke(instance, new object[] { playerSide, sideCheck });
        return result is bool b && b;
    }

    private static LocationLifecycleService CreateUninitializedService()
    {
        // Skips running the heavy ctor and DI; safe for IsSide which only compares strings
        return (LocationLifecycleService)FormatterServices.GetUninitializedObject(typeof(LocationLifecycleService));
    }

    [Test]
    public void IsSide_ReturnsTrue_ForPmc_DefaultCheck_IsCaseInsensitive()
    {
        var svc = CreateUninitializedService();

        // Default side is "pmc"; ensure case-insensitive match works
        Assert.IsTrue(InvokeIsSide(svc, "PMC", "pmc"));
        Assert.IsTrue(InvokeIsSide(svc, "pmc", "pmc"));
    }

    [Test]
    public void IsSide_ReturnsFalse_ForNonMatchingSide()
    {
        var svc = CreateUninitializedService();

        Assert.IsFalse(InvokeIsSide(svc, "savage", "pmc"));
        Assert.IsFalse(InvokeIsSide(svc, "beAr", "pmc"));
    }

    [Test]
    public void IsSide_ReturnsTrue_WhenCheckingScavAgainstSavage_IsCaseInsensitive()
    {
        var svc = CreateUninitializedService();

        // In code, scav side string used for extracts is "savage"
        Assert.IsTrue(InvokeIsSide(svc, "SAVAGE", "savage"));
        Assert.IsTrue(InvokeIsSide(svc, "savage", "savage"));
    }
}
