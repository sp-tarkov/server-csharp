using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Models.Logging;

namespace UnitTests.Tests.Extensions;

[TestFixture]
public class SptLoggerExtensionsTests
{
    [Test]
    public void AddSptLogger_RegistersEveryLogHandlerInTheDeclaringAssembly()
    {
        var expected = typeof(ILogHandler)
            .Assembly.GetTypes()
            .Where(type => typeof(ILogHandler).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
            .ToList();

        var registered = new ServiceCollection()
            .AddSptLogger(isDevelop: true)
            .Where(descriptor => descriptor.ServiceType == typeof(ILogHandler))
            .Select(descriptor => descriptor.ImplementationType)
            .ToList();

        Assert.That(expected, Is.Not.Empty, "SPTarkov.Common declares no log handlers, the test has nothing to verify");
        Assert.That(registered, Is.EquivalentTo(expected));
    }

    /// <summary>
    ///     The handler scan runs before the prepatcher decides whether to re-host, so it must not reach outside the
    ///     assembly that declares the interface. Scanning every loaded assembly resolves their member signatures and
    ///     pulls the assemblies those signatures name into the default load context.
    /// </summary>
    [Test]
    public void AddSptLogger_DoesNotScanAssembliesOutsideTheDeclaringOne()
    {
        var registered = new ServiceCollection()
            .AddSptLogger(isDevelop: true)
            .Where(descriptor => descriptor.ServiceType == typeof(ILogHandler))
            .Select(descriptor => descriptor.ImplementationType!)
            .ToList();

        Assert.That(registered, Is.Not.Empty);
        Assert.That(
            registered.Select(type => type.Assembly),
            Is.All.EqualTo(typeof(ILogHandler).Assembly),
            "a handler was picked up from outside SPTarkov.Common, meaning the scan swept foreign assemblies"
        );
    }

    [Test]
    public void AddSptLogger_IsNotFooledByTheTestAssemblysOwnHandler()
    {
        Assert.That(
            typeof(TestOnlyLogHandler).Assembly,
            Is.Not.EqualTo(typeof(ILogHandler).Assembly),
            "guard type must live outside SPTarkov.Common for this test to mean anything"
        );

        var registered = new ServiceCollection()
            .AddSptLogger(isDevelop: true)
            .Where(descriptor => descriptor.ServiceType == typeof(ILogHandler))
            .Select(descriptor => descriptor.ImplementationType)
            .ToList();

        Assert.That(registered, Does.Not.Contain(typeof(TestOnlyLogHandler)));
    }

    private sealed class TestOnlyLogHandler : ILogHandler
    {
        public LoggerType LoggerType => LoggerType.Console;

        public void Log(SptLogMessage message, BaseSptLoggerReference reference)
        {
            throw new NotSupportedException("test guard type, never invoked");
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
