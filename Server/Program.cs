using System.Reflection;
using Core.Annotations;
using Core.Context;
using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var assemblies = ModDllLoader.LoadAllMods();
        HarmonyBootstrapper.LoadAllPatches(assemblies);
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Configuration.AddJsonFile("appsettings.json", true, true);
        
        CreateAndRegisterLogger(builder, out var registeredLogger);

        ProgramStatics.Initialize();

        DependencyInjectionRegistrator.RegisterSptComponents(builder.Services);
        DependencyInjectionRegistrator.RegisterModOverrideComponents(builder.Services, assemblies);
        ILogger logger = new SerilogLoggerProvider(registeredLogger).CreateLogger("Server");
        try
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var watermark = serviceProvider.GetService<Watermark>();
            // Initialize Watermak
            watermark.Initialize();
            
            // Initialize PreSptMods
            var preSptLoadMods = serviceProvider.GetServices<IPreSptLoadMod>();
            foreach (var preSptLoadMod in preSptLoadMods)
            {
                preSptLoadMod.PreSptLoad();
            }
            var appContext = serviceProvider.GetService<ApplicationContext>();
            // Add the Loaded Mod Assemblies for later
            appContext.AddValue(ContextVariableType.LOADED_MOD_ASSEMBLIES, assemblies);
            // This is the builder that will get use by the HttpServer to start up the web application
            appContext.AddValue(ContextVariableType.APP_BUILDER, builder);
            
            // Get the Built app and run it
            var app = serviceProvider.GetService<App>();
            app.Run().Wait();

            var httpConfig = serviceProvider.GetService<ConfigServer>().GetConfig<HttpConfig>();
            // When we application gets started by the HttpServer it will add into the AppContext the WebApplication
            // object, which we can use here to start the webapp.
            (appContext.GetLatestValue(ContextVariableType.WEB_APPLICATION).GetValue<WebApplication>()).Run($"http://{httpConfig.Ip}:{httpConfig.Port}");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical exception, stopping server...");
            // throw ex;
        }
    }
    
    public static void CreateAndRegisterLogger(WebApplicationBuilder builder, out Serilog.Core.Logger logger)
    {
        builder.Logging.ClearProviders();
        logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithExceptionDetails()
            .CreateLogger();
        builder.Logging.AddSerilog(logger);
    }
}
