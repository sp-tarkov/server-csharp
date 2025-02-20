using System.Runtime;
using Core.Context;
using Core.Helpers;
using Core.Models.External;
using Core.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using SptDependencyInjection;

namespace Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var mods = ModDllLoader.LoadAllMods();
        HarmonyBootstrapper.LoadAllPatches(mods.Select(asm => asm.Assembly).ToList());
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();

        builder.Configuration.AddJsonFile("appsettings.json", true, true);

        CreateAndRegisterLogger(builder, out var registeredLogger);

        ProgramStatics.Initialize();

        DependencyInjectionRegistrator.RegisterSptComponents(typeof(Program).Assembly, typeof(App).Assembly, builder.Services);
        DependencyInjectionRegistrator.RegisterModOverrideComponents(builder.Services, mods.Select(a => a.Assembly).ToList());
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

            // RUn garbage collection now server is ready to start
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
