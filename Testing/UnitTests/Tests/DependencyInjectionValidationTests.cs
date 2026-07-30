using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Helpers;
using SPTarkov.Server.Modding;
using UnitTests.Mock;

namespace UnitTests.Tests;

/// <summary>
/// Builds the same containers the server builds at startup, so a missing or broken registration
/// fails the build instead of only showing up when someone launches the server.
///
/// Both run with mods on and off: a registration that only happens on one side of the mods
/// branch is exactly the bug this is here to catch.
/// </summary>
[TestFixture]
public class DependencyInjectionValidationTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task EveryRegisteredServiceCanBeResolved(bool modsEnabled)
    {
        var logger = new MockLogger<DependencyInjectionValidationTests>();
        var configuration = await ConfigLoader.Initialize(logger);

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();

        builder.Host.UseDefaultServiceProvider(options =>
        {
            options.ValidateOnBuild = true;
            options.ValidateScopes = true;
        });

        builder.Services.AddSingleton(logger);
        builder.Services.AddSingleton(typeof(ILogger<>), typeof(MockLogger<>));
        builder.Services.AddSingleton(typeof(ISptLogger<>), typeof(MockLogger<>));

        foreach (var configEntry in configuration)
        {
            builder.Services.AddSingleton(configEntry.Key, configEntry.Value);
        }

        // The database tables come from the importer at runtime. Validation only walks constructors,
        // it never invokes factories, so stubs are enough and we avoid loading the whole database.
        AddDatabaseTableStubs(builder.Services);

        await ProgramHelpers.RegisterSptServicesAsync(builder, [], modsEnabled);

        // Throws an AggregateException listing every service that can't be constructed.
        builder.Build();
    }

    /// <summary>
    /// The early provider is built before the web host and is only asked for a couple of services,
    /// so it's deliberately partial and can't be whole-container validated. Resolve what
    /// <see cref="SPTarkov.Server.Program" /> actually pulls out of it instead.
    /// </summary>
    [TestCase(true)]
    [TestCase(false)]
    public async Task EarlyProviderResolvesTheServicesStartupNeeds(bool modsEnabled)
    {
        var logger = new MockLogger<DependencyInjectionValidationTests>();
        var configuration = await ConfigLoader.Initialize(logger);

        var loggerFactory = SptLoggerProvider.Create(false);
        try
        {
            var provider = ProgramHelpers.CreateEarlySptProvider(loggerFactory, configuration, modsEnabled);

            Assert.That(provider.GetRequiredService<DatabaseImporter>(), Is.Not.Null);

            if (modsEnabled)
            {
                Assert.That(provider.GetRequiredService<ModLoader>(), Is.Not.Null);
            }
        }
        finally
        {
            loggerFactory.Provider.Dispose();
        }
    }

    private static void AddDatabaseTableStubs(IServiceCollection services)
    {
        AddStub<BotTable>(services);
        AddStub<HideoutTable>(services);
        AddStub<LocaleTable>(services);
        AddStub<LocationTable>(services);
        AddStub<MatchTable>(services);
        AddStub<TemplateTable>(services);
        AddStub<TradersTable>(services);
        AddStub<GlobalTable>(services);
        AddStub<ServerTable>(services);
        AddStub<SettingsTable>(services);
    }

    private static void AddStub<T>(IServiceCollection services)
        where T : class
    {
        services.AddSingleton<T>(_ => throw new NotSupportedException($"{typeof(T).Name} is not available during DI validation."));
    }
}
