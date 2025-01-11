using System.Reflection;
using Core.Annotations;
using Core.Context;
using Core.Utils;

namespace Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var assemblies = ModDllLoader.LoadAllMods();
        HarmonyBootstrapper.LoadAllPatches(assemblies);
        var builder = WebApplication.CreateBuilder(args);

        RegisterSptComponents(builder.Services);
        RegisterModOverrideComponents(builder.Services, assemblies);
        
        try
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            var watermark = serviceProvider.GetService<Watermark>();
            watermark.Initialize();
            // TODO: var preSptModLoader = serviceProvider.GetService<PreSptModLoader>();
            var app = serviceProvider.GetService<App>();
            var appContext = serviceProvider.GetService<ApplicationContext>();
            appContext.AddValue(ContextVariableType.LOADED_MOD_ASSEMBLIES, assemblies);
            appContext.AddValue(ContextVariableType.APP_BUILDER, builder);
            app.Load().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
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
