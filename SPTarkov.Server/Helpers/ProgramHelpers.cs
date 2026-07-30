using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Services.Hosted;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Json;
using SPTarkov.Server.Extensions;
using SPTarkov.Server.Modding;
using SPTarkov.Server.Web;

namespace SPTarkov.Server.Helpers;

public static class ProgramHelpers
{
    private static readonly JsonSerializerOptions _earlyLocaleJsonSerializerOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static WebApplicationBuilder CreateNewHostBuilder(
        SptEarlyLoggerFactory earlyFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration,
        DatabaseTables? databaseTables = null,
        LocaleTable? localeTable = null
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
            if (localeTable is not null)
            {
                builder.Services.AddSingleton(localeTable);
            }

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

    /// <summary>
    /// Registers every SPT service onto the builder.
    /// Kept in one place so the DI validation test can build the exact same container the server does.
    /// </summary>
    public static async Task RegisterSptServicesAsync(
        WebApplicationBuilder builder,
        IReadOnlyList<SptMod> loadedMods,
        bool modsEnabled,
        CancellationToken cancellationToken = default
    )
    {
        var diHandler = new DependencyInjectionHandler(builder.Services);

        // register SPT components
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(Program));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(PatchManager));
        diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTWeb));

        builder.Services.AddSingleton(new ClientEnumDefinitions());

        if (modsEnabled)
        {
            diHandler.AddInjectableTypesFromAssemblies(loadedMods.SelectMany(a => a.Assemblies));
        }

        diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTStartupHostedService));

        diHandler.InjectAll();

        builder.InitializeSptBlazor(loadedMods);

        builder.Services.AddSingleton(builder);
        builder.Services.AddSingleton(loadedMods);

        await builder.Services.AddModDIConstructorsAsync(loadedMods.SelectMany(mod => mod.Assemblies).ToArray(), cancellationToken);

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClient(
            "Github",
            httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://api.github.com/");

                // These headers are _required_ by GitHub API
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("spt-csharp-server");
                httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            }
        );
    }

    public static ServiceProvider CreateEarlySptProvider(
        SptEarlyLoggerFactory loggerFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration,
        bool modsEnabled
    )
    {
        // We need the SPT dependencies for the ModValidator, but mods are loaded before the web application
        // So we create a disposable web application that we will throw away after getting the mods to load
        var builder = CreateNewHostBuilder(loggerFactory, configuration, localeTable: CreateEarlyLocaleTable());
        // register SPT components
        var diHandler = new DependencyInjectionHandler(builder.Services);
        diHandler.AddInjectableTypesFromAssembly(typeof(SPTStartupHostedService).Assembly);
        diHandler.InjectAll();

        var serviceCollection = builder
            .Services.AddScoped<ISemVer, SemanticVersioningSemVer>()
            .AddSptLoggerWithoutProvider(loggerFactory.ServiceProvider);

        serviceCollection.AddSingleton<DatabaseImporter>();

        if (modsEnabled)
        {
            serviceCollection.AddSingleton<ModLoader>().AddSingleton<ModValidator>();
        }

        return serviceCollection.BuildServiceProvider();
    }

    public static LocaleTable CreateEarlyLocaleTable()
    {
        var localesPath = Path.Combine(".", "SPT_Data", "database", "locales");
        var globalPath = Path.Combine(localesPath, "global");
        var menuPath = Path.Combine(localesPath, "menu");
        var languagesPath = Path.Combine(localesPath, "languages.json");

        if (!Directory.Exists(globalPath) || !Directory.Exists(menuPath) || !File.Exists(languagesPath))
        {
            throw new InvalidOperationException($"Unable to load early locale table from '{Path.GetFullPath(localesPath)}'.");
        }

        return new LocaleTable
        {
            Global = Directory
                .EnumerateFiles(globalPath, "*.json")
                .ToDictionary(
                    GetLocaleKey,
                    file => new LazyLoad<GlobalLocaleDictionary>(() => DeserializeFromFile<GlobalLocaleDictionary>(file) ?? []),
                    StringComparer.OrdinalIgnoreCase
                ),
            Menu = Directory
                .EnumerateFiles(menuPath, "*.json")
                .ToDictionary(
                    GetLocaleKey,
                    file => DeserializeFromFile<Dictionary<string, object>>(file) ?? [],
                    StringComparer.OrdinalIgnoreCase
                ),
            Languages = DeserializeFromFile<Dictionary<string, string>>(languagesPath) ?? [],
        };
    }

    private static T? DeserializeFromFile<T>(string file)
    {
        using var stream = File.OpenRead(file);
        return JsonSerializer.Deserialize<T>(stream, _earlyLocaleJsonSerializerOptions);
    }

    private static string GetLocaleKey(string file)
    {
        return Path.GetFileNameWithoutExtension(file) ?? throw new InvalidOperationException($"Unable to get locale key from '{file}'.");
    }
}
