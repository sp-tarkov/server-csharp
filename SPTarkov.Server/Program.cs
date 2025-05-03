using System.Runtime;
using SPTarkov.Common.Semver;
using SPTarkov.Common.Semver.Implementations;
using SPTarkov.DI;
using SPTarkov.Server.Core.Context;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;
using SPTarkov.Server.Logger;
using SPTarkov.Server.Modding;

namespace SPTarkov.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        // Initialize the program variables
        ProgramStatics.Initialize();

        // Search for mod dlls
        var mods = ModDllLoader.LoadAllMods();

        // Create web builder and logger
        var builder = CreateNewHostBuilder(args);

        // validate and sort mods, this will also discard any mods that are invalid
        var sortedLoadedMods = ValidateMods(mods);
        // for harmony, we use the original list, as some mods may only be bepinex patches only
        HarmonyBootstrapper.LoadAllPatches(mods.SelectMany(asm =>
        {
            return asm.Assemblies;
        }).ToList());

        // register SPT components
        DependencyInjectionRegistrator.RegisterSptComponents(typeof(Program).Assembly, typeof(App).Assembly, builder.Services);

        if (ProgramStatics.MODS())
        {
            // register mod components from the filtered list
            DependencyInjectionRegistrator.RegisterModOverrideComponents(builder.Services, sortedLoadedMods.SelectMany(a =>
            {
                return a.Assemblies;
            }).ToList());
        }

        var serviceProvider = builder.Services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger("Server");

        try
        {
            var watermark = serviceProvider.GetService<Watermark>();
            // Initialize Watermark
            watermark?.Initialize();

            var appContext = serviceProvider.GetService<ApplicationContext>();
            appContext?.AddValue(ContextVariableType.SERVICE_PROVIDER, serviceProvider);

            // Initialize PreSptMods
            var preSptLoadMods = serviceProvider.GetServices<IPreSptLoadMod>();
            foreach (var preSptLoadMod in preSptLoadMods)
            {
                preSptLoadMod.PreSptLoad();
            }

            // Add the Loaded Mod Assemblies for later
            appContext?.AddValue(ContextVariableType.LOADED_MOD_ASSEMBLIES, mods);

            // This is the builder that will get use by the HttpServer to start up the web application
            appContext?.AddValue(ContextVariableType.APP_BUILDER, builder);

            // Get the Built app and run it
            var app = serviceProvider.GetService<App>();
            app?.Run().Wait();

            // Run garbage collection now the server is ready to start
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);


            var httpServerHelper = serviceProvider.GetService<HttpServerHelper>();
            // When the application is started by the HttpServer it will be added into the AppContext of the WebApplication
            // object, which we can use here to start the webapp.
            if (httpServerHelper != null)
            {
                appContext?.GetLatestValue(ContextVariableType.WEB_APPLICATION)?.GetValue<WebApplication>().Run();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            logger.LogCritical(ex, "Critical exception, stopping server...");
        }
        finally
        {
            serviceProvider.GetService<SptLogger<object>>()?.DumpAndStop();
        }
    }

    private static WebApplicationBuilder CreateNewHostBuilder(string[]? args = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Host.UseSptLogger();

        return builder;
    }

    private static List<SptMod> ValidateMods(List<SptMod> mods)
    {
        if (!ProgramStatics.MODS())
        {
            return [];
        }

        // We need the SPT dependencies for the ModValidator, but mods are loaded before the web application
        // So we create a disposable web application that we will throw away after getting the mods to load
        var builder = CreateNewHostBuilder();
        // register SPT components
        DependencyInjectionRegistrator.RegisterSptComponents(typeof(Program).Assembly, typeof(App).Assembly, builder.Services);
        // register the mod validator components
        var provider = builder.Services
            .AddScoped(typeof(ISptLogger<ModValidator>), typeof(SptLogger<ModValidator>))
            .AddScoped(typeof(ISemVer), typeof(SemanticVersioningSemVer))
            .AddSingleton<ModValidator>()
            .AddSingleton<ModLoadOrder>()
            .BuildServiceProvider();
        var modValidator = provider.GetRequiredService<ModValidator>();
        return modValidator.ValidateAndSort(mods);
    }
}
