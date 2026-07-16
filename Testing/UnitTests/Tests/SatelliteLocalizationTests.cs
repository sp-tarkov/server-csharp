using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using SPTarkov.Server;

namespace UnitTests.Tests;

[TestFixture]
public class SatelliteLocalizationTests
{
    private string _tempBase = null!;

    [SetUp]
    public void SetUp()
    {
        _tempBase = Path.Combine(Path.GetTempPath(), "spt-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempBase);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempBase))
        {
            Directory.Delete(_tempBase, recursive: true);
        }
    }

    [Test]
    public void DiscoversAtLeastOneSatellite()
    {
        Assert.That(DiscoverSatellites(), Is.Not.Empty, "no satellite assemblies found in the test output to verify");
    }

    [TestCaseSource(nameof(DiscoverSatellites))]
    public void ResolvesRelocatedSatellite(string culture, string fileName)
    {
        // Relocate the real satellite into the layout the build produces, then resolve it.
        var relocated = Path.Combine(_tempBase, "SPT_Data", "dotnet", culture);
        Directory.CreateDirectory(relocated);
        File.Copy(Path.Combine(AppContext.BaseDirectory, culture, fileName), Path.Combine(relocated, fileName));

        var name = new AssemblyName(Path.GetFileNameWithoutExtension(fileName)) { CultureInfo = CultureInfo.GetCultureInfo(culture) };
        var resolved = Program.ResolveSatelliteAssembly(_tempBase, name, Assembly.LoadFrom);

        Assert.That(resolved, Is.Not.Null, "resolver failed to locate the relocated satellite");
        Assert.That(resolved!.GetName().CultureName, Is.EqualTo(culture));
        Assert.That(resolved.GetManifestResourceNames(), Is.Not.Empty, "resolved satellite carries no localized resources");
    }

    [Test]
    public void ReturnsNull_ForNonSatelliteAssembly()
    {
        var resolved = Program.ResolveSatelliteAssembly(_tempBase, new AssemblyName("SomeLibrary"), Assembly.LoadFrom);

        Assert.That(resolved, Is.Null, "non-satellite assemblies must fall through to default resolution");
    }

    [Test]
    public void ReturnsNull_WhenNoRelocatedCopyExists()
    {
        var missing = new AssemblyName("Some.Library.resources") { CultureInfo = CultureInfo.GetCultureInfo("de") };

        var resolved = Program.ResolveSatelliteAssembly(_tempBase, missing, Assembly.LoadFrom);

        Assert.That(resolved, Is.Null, "missing relocated copies must fall through to default resolution");
    }

    private static IEnumerable<TestCaseData> DiscoverSatellites()
    {
        foreach (var cultureDir in Directory.GetDirectories(AppContext.BaseDirectory))
        {
            var culture = Path.GetFileName(cultureDir);
            foreach (var dll in Directory.GetFiles(cultureDir, "*.resources.dll"))
            {
                yield return new TestCaseData(culture, Path.GetFileName(dll)).SetName($"Resolves {Path.GetFileName(dll)} [{culture}]");
            }
        }
    }
}
