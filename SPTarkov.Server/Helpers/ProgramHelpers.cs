using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Services.Hosted;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Modding;

namespace SPTarkov.Server.Helpers;

public static class ProgramHelpers
{
    public static WebApplicationBuilder CreateNewHostBuilder(
        SptEarlyLoggerFactory earlyFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration,
        DatabaseTables? databaseTables = null
    )
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = "./SPT_Data/wwwroot" });
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(earlyFactory.Provider);
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

        foreach (var configEntry in configuration)
        {
            builder.Services.AddSingleton(configEntry.Key, configEntry.Value);
        }

        if (databaseTables == null)
        {
            return builder;
        }

        builder.Services.AddSingleton(databaseTables.Bots);
        builder.Services.AddSingleton(databaseTables.Hideout);
        builder.Services.AddSingleton(databaseTables.Locales);
        builder.Services.AddSingleton(databaseTables.Locations);
        builder.Services.AddSingleton(databaseTables.Match);
        builder.Services.AddSingleton(databaseTables.Templates);
        builder.Services.AddSingleton(databaseTables.Traders);
        builder.Services.AddSingleton(databaseTables.Globals);
        builder.Services.AddSingleton(databaseTables.Server);
        builder.Services.AddSingleton(databaseTables.Settings);

        return builder;
    }

    public static ServiceProvider CreateEarlySptProvider(
        SptEarlyLoggerFactory loggerFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration
    )
    {
        // We need the SPT dependencies for the ModValidator, but mods are loaded before the web application
        // So we create a disposable web application that we will throw away after getting the mods to load
        var builder = CreateNewHostBuilder(loggerFactory, configuration);
        // register SPT components
        var diHandler = new DependencyInjectionHandler(builder.Services);
        diHandler.AddInjectableTypesFromAssembly(typeof(Program).Assembly);
        diHandler.AddInjectableTypesFromAssembly(typeof(SPTStartupHostedService).Assembly);
        diHandler.InjectAll();

        var serviceCollection = builder
            .Services.AddScoped<ISemVer, SemanticVersioningSemVer>()
            .AddSptLoggerWithoutProvider(loggerFactory.ServiceProvider);

        serviceCollection.AddSingleton<EarlyDatabaseImporter>();

        if (ProgramStatics.MODS())
        {
            serviceCollection.AddSingleton<ModLoader>().AddSingleton<ModValidator>();
        }

        return serviceCollection.BuildServiceProvider();
    }
}
