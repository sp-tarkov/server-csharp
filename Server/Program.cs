using Core.Annotations;
using Core.Context;
using Core.Servers;
using Core.Utils;

namespace Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        RegisterSptComponents(builder.Services);

        // TODO: deal with modding overriding services here!

        try
        {

            var serviceProvider = builder.Services.BuildServiceProvider();
            var app = serviceProvider.GetService<App>();
            var appContext = serviceProvider.GetService<ApplicationContext>();
            appContext.AddValue(ContextVariableType.APP_BUILDER, builder);
            app.Load().Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static void RegisterComponents(IServiceCollection builderServices, IEnumerable<Type> types)
    {
        var groupedTypes = types.Select(t =>
        {
            var attribute = (Injectable)Attribute.GetCustomAttribute(t, typeof(Injectable))!;
            var registerableType = t;
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

            return (registerableInterface: registerableType, typeToRegister: t, injectableAttribute: attribute);
        }).GroupBy(t => t.registerableInterface.FullName);
        // We get all injectable services to register them on our services
        foreach (var groupedInjectables in groupedTypes)
        {
            foreach (var valueTuple in groupedInjectables.OrderBy(t => t.injectableAttribute.TypePriority))
            {
                switch (valueTuple.injectableAttribute.InjectionType)
                {
                    case InjectionType.Singleton:
                        builderServices.AddSingleton(valueTuple.registerableInterface, valueTuple.typeToRegister);
                        break;
                    case InjectionType.Transient:
                        builderServices.AddTransient(valueTuple.registerableInterface, valueTuple.typeToRegister);
                        break;
                    case InjectionType.Scoped:
                        builderServices.AddScoped(valueTuple.registerableInterface, valueTuple.typeToRegister);
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
}