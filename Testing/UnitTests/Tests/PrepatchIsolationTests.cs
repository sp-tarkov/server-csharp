using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SPTarkov.Common.Extensions;
using SPTarkov.DI;

namespace UnitTests.Tests;

/// <summary>
///     When a prepatcher is present the server re-hosts itself inside its own AssemblyLoadContext, but the early
///     logger is built before that decision. Anything reflecting over SPT.Server at that point resolves its types'
///     member signatures and loads the assemblies they name into the default context. A duplicate
///     SPTarkov.Server.Web there is fatal: Blazor resolves interactive root components from the copy that never saw
///     the service registrations, so every circuit dies on startup and the web panel goes dead.
/// </summary>
[TestFixture]
public class PrepatchIsolationTests
{
    private const string WebAssemblyName = "SPTarkov.Server.Web";

    [Test]
    public void BuildingTheEarlyLogger_DoesNotPullWebIntoTheHostingContext()
    {
        AssertDoesNotPullWebIn(
            AddSptLoggerFrom,
            $"building the early logger loaded {WebAssemblyName}; the log handler scan is reaching outside the "
                + "assembly that declares ILogHandler again"
        );
    }

    [Test]
    public void RegisteringTheEarlyInjectables_DoesNotPullWebIntoTheHostingContext()
    {
        AssertDoesNotPullWebIn(
            BuildEarlyInjectablesFrom,
            $"registering the early injectables loaded {WebAssemblyName}; the generic component scan is reaching "
                + "outside the assemblies the handler was given again"
        );
    }

    private static void AssertDoesNotPullWebIn(Action<AssemblyLoadContext> startupStep, string because)
    {
        Assert.That(
            File.Exists(Path.Combine(AppContext.BaseDirectory, $"{WebAssemblyName}.dll")),
            Is.True,
            "the probe context has nothing to pull in, this test would pass vacuously"
        );

        var context = new SptProbeLoadContext();

        try
        {
            // Mirrors the pre-rehost process: SPT.Server is loaded, the web assembly is not (yet).
            context.LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, "SPT.Server.dll"));

            startupStep(context);

            Assert.That(context.Assemblies.Select(assembly => assembly.GetName().Name), Does.Not.Contain(WebAssemblyName), because);
        }
        finally
        {
            context.Unload();
        }
    }

    /// <summary>
    ///     Reproduces what CreateEarlySptProvider does before the prepatcher decides whether to re-host: seed the
    ///     handler with the core assembly only, then register everything.
    /// </summary>
    private static void BuildEarlyInjectablesFrom(AssemblyLoadContext context)
    {
        var handlerType = LoadFrom(context, typeof(DependencyInjectionHandler));
        var core = context.LoadFromAssemblyPath(Path.Combine(AppContext.BaseDirectory, "SPTarkov.Server.Core.dll"));
        var handler = Activator.CreateInstance(handlerType, new ServiceCollection());

        Invoke(handlerType, nameof(DependencyInjectionHandler.AddInjectableTypesFromAssembly), handler, [core]);
        Invoke(handlerType, nameof(DependencyInjectionHandler.InjectAll), handler, []);
    }

    /// <summary>
    ///     Calls AddSptLogger on the copy of SPTarkov.Common owned by <paramref name="context" />, so any type
    ///     resolution it triggers lands in that context rather than the default one.
    /// </summary>
    private static void AddSptLoggerFrom(AssemblyLoadContext context)
    {
        var extensions = LoadFrom(context, typeof(SptLoggerExtensions));

        // Not the develop config: only sptLogger.json is copied next to the tests in every configuration.
        Invoke(extensions, nameof(SptLoggerExtensions.AddSptLogger), instance: null, [new ServiceCollection(), false]);
    }

    /// <summary>
    ///     Loads <paramref name="type" />'s assembly into <paramref name="context" /> and returns that context's copy
    ///     of the type, so calls made through it resolve inside the probe rather than the default context.
    /// </summary>
    private static Type LoadFrom(AssemblyLoadContext context, Type type)
    {
        var assembly = context.LoadFromAssemblyPath(
            Path.Combine(AppContext.BaseDirectory, $"{type.Assembly.GetName().Name}.dll")
        );

        return assembly.GetType(type.FullName!) ?? throw new InvalidOperationException($"{type.Name} missing from the probe context");
    }

    private static void Invoke(Type declaringType, string methodName, object? instance, object?[] arguments)
    {
        var method =
            declaringType.GetMethod(methodName)
            ?? throw new InvalidOperationException($"{declaringType.Name}.{methodName} missing from the probe context");

        method.Invoke(instance, arguments);
    }

    /// <summary>
    ///     Mirrors the prepatcher's context: SPT assemblies get their own copy here, everything else falls back to the
    ///     default context. Without that fallback the framework would be duplicated too and nothing would load.
    /// </summary>
    private sealed class SptProbeLoadContext() : AssemblyLoadContext("SPT.PrepatchIsolationProbe", isCollectible: true)
    {
        protected override Assembly? Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name is null || !assemblyName.Name.StartsWith("SPT", StringComparison.Ordinal))
            {
                return null;
            }

            var path = Path.Combine(AppContext.BaseDirectory, $"{assemblyName.Name}.dll");

            return File.Exists(path) ? LoadFromAssemblyPath(path) : null;
        }
    }
}
