using System.Runtime;
using Core.Context;
using Core.Helpers;
using Core.Models.External;
using Core.Models.Spt.Mod;
using Core.Models.Utils;
using Core.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using Server.Logger;
using Server.Modding;
using SptCommon.Semver;
using SptCommon.Semver.Implementations;
using SptDependencyInjection;

namespace Server;

public static class Program
{
    public static void Main(string[] args)
    {
        // Search for mod dlls
        var mods = ModDllLoader.LoadAllMods();
        // Create web builder and logger
        var builder = CreateNewHostBuilder(out var registeredLogger, args);
        // Initialize the program variables TODO: this needs to be implemented properly
        ProgramStatics.Initialize();

        // validate and sort mods, this will also discard any mods that are invalid
        var sortedLoadedMods = ValidateMods(mods);
        // for harmony we use the original list, as some mods may only be bepinex patches only
        HarmonyBootstrapper.LoadAllPatches(mods.Select(asm => asm.Assembly).ToList());

        // register SPT components
        DependencyInjectionRegistrator.RegisterSptComponents(typeof(Program).Assembly, typeof(App).Assembly, builder.Services);
        // register mod components from the filtered list
        DependencyInjectionRegistrator.RegisterModOverrideComponents(builder.Services, sortedLoadedMods.Select(a => a.Assembly).ToList());
        var logger = new SerilogLoggerProvider(registeredLogger).CreateLogger("Server");
        try
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var watermark = serviceProvider.GetService<Watermark>();
            // Initialize Watermak
            watermark?.Initialize();

            // Initialize PreSptMods
            var preSptLoadMods = serviceProvider.GetServices<IPreSptLoadMod>();
            foreach (var preSptLoadMod in preSptLoadMods)
            {
                preSptLoadMod.PreSptLoad();
            }

            var appContext = serviceProvider.GetService<ApplicationContext>();
            // Add the Loaded Mod Assemblies for later
            appContext?.AddValue(ContextVariableType.LOADED_MOD_ASSEMBLIES, mods);
            // This is the builder that will get use by the HttpServer to start up the web application
            appContext?.AddValue(ContextVariableType.APP_BUILDER, builder);

            // Get the Built app and run it
            var app = serviceProvider.GetService<App>();
            app?.Run().Wait();

            // Run garbage collection now server is ready to start
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
            // throw ex;
        }
    }

    private static WebApplicationBuilder CreateNewHostBuilder(out Serilog.Core.Logger logger, string[]? args = null)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();
        builder.Configuration.AddJsonFile("appsettings.json", true, true);
        CreateAndRegisterLogger(builder, out logger);
        return builder;
    }

    private static List<SptMod> ValidateMods(List<SptMod> mods)
    {
        // We need the SPT dependencies for the ModValidator, but mods are loaded before the web application
        // So we create a disposable web application that we will throw away after getting the mods to load
        var builder = CreateNewHostBuilder(out _);
        // register SPT components
        DependencyInjectionRegistrator.RegisterSptComponents(typeof(Program).Assembly, typeof(App).Assembly, builder.Services);
        // register the mod validator components
        var provider = builder.Services
            .AddScoped(typeof(ISptLogger<ModValidator>), typeof(SptWebApplicationLogger<ModValidator>))
            .AddScoped(typeof(ISemVer), typeof(SemanticVersioningSemVer))
            .AddSingleton<ModValidator>()
            .AddSingleton<ModLoadOrder>()
            .BuildServiceProvider();
        var modValidator = provider.GetRequiredService<ModValidator>();
        return modValidator.ValidateAndSort(mods);
    }

    public static void CreateAndRegisterLogger(WebApplicationBuilder builder, out Serilog.Core.Logger logger)
    {
        builder.Logging.ClearProviders();
        logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
# if DEBUG
            .MinimumLevel.Debug()
# endif
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithExceptionDetails()
            .CreateLogger();
        builder.Logging.AddSerilog(logger);
    }
}
