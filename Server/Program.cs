using System.Reflection;
using System.Security.Cryptography;
using Core.Annotations;
using Core.Context;
using Core.Models.Enums;
using Core.Models.External;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Utils;
using Serilog;
using Serilog.Events;

namespace Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var assemblies = ModDllLoader.LoadAllMods();
        HarmonyBootstrapper.LoadAllPatches(assemblies);
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Configuration.AddJsonFile("appsettings.json", true, true);
        
        CreateAndRegisterLogger(builder);

        RegisterSptComponents(builder.Services);
        RegisterModOverrideComponents(builder.Services, assemblies);
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

            var httpConfig = serviceProvider.GetService<ConfigServer>().GetConfig<HttpConfig>(ConfigTypes.HTTP);
            // When we application gets started by the HttpServer it will add into the AppContext the WebApplication
            // object, which we can use here to start the webapp.
            (appContext.GetLatestValue(ContextVariableType.WEB_APPLICATION).GetValue<WebApplication>()).Run($"http://{httpConfig.Ip}:{httpConfig.Port}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static void CreateAndRegisterLogger(WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Warning)
            .CreateLogger();
        builder.Logging.AddSerilog(logger);
    }

    private static void RegisterModOverrideComponents(IServiceCollection builderServices, List<Assembly> assemblies)
    {
        // We get all the services from this assembly first, since mods will override them later
        RegisterComponents(builderServices, assemblies.SelectMany(a => a.GetTypes())
            .Where(type => Attribute.IsDefined(type, typeof(Injectable))));
    }

    private static void RegisterComponents(IServiceCollection builderServices, IEnumerable<Type> types)
    {
        var groupedTypes = types.SelectMany(t =>
        {
            var attributes = (Injectable[]) Attribute.GetCustomAttributes(t, typeof(Injectable))!;
            var registerableType = t;
            var registerableComponents = new List<RegisterableType>();
            foreach (var attribute in attributes)
            {
                // if we have a type override this takes priority
                if (attribute.InjectableTypeOverride != null)
                {
                    registerableType = attribute.InjectableTypeOverride;
                }
                // if this class only has 1 interface we register it on that interface
                else if (registerableType.GetInterfaces().Length == 1)
                {
                    registerableType = registerableType.GetInterfaces()[0];
                }
                registerableComponents.Add(new(registerableType, t, attribute));
            }
            return registerableComponents;
        }).GroupBy(t => t.RegisterableInterface.FullName);
        // We get all injectable services to register them on our services
        foreach (var groupedInjectables in groupedTypes)
        {
            foreach (var valueTuple in groupedInjectables.OrderBy(t => t.InjectableAttribute.TypePriority))
            {
                switch (valueTuple.InjectableAttribute.InjectionType)
                {
                    case InjectionType.Singleton:
                        builderServices.AddSingleton(valueTuple.RegisterableInterface, valueTuple.TypeToRegister);
                        break;
                    case InjectionType.Transient:
                        builderServices.AddTransient(valueTuple.RegisterableInterface, valueTuple.TypeToRegister);
                        break;
                    case InjectionType.Scoped:
                        builderServices.AddScoped(valueTuple.RegisterableInterface, valueTuple.TypeToRegister);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private static void RegisterSptComponents(IServiceCollection builderServices)
    {
        // We get all the services from this assembly first, since mods will override them later
        RegisterComponents(builderServices, typeof(Program).Assembly.GetTypes()
            .Where(type => Attribute.IsDefined(type, typeof(Injectable))));
        RegisterComponents(builderServices, typeof(App).Assembly.GetTypes()
            .Where(type => Attribute.IsDefined(type, typeof(Injectable))));
    }

    class RegisterableType(Type registerableInterface, Type typeToRegister, Injectable injectableAttribute)
    {
        public Type RegisterableInterface { get; } = registerableInterface;
        public Type TypeToRegister { get; } = typeToRegister;
        public Injectable InjectableAttribute { get; } = injectableAttribute;
    }
}
