using Types.Annotations;
using Types.Servers;

namespace Server;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        RegisterSptComponents(builder.Services);
        
        // TODO: deal with modding overriding services here!

        var httpServer = builder.Services.BuildServiceProvider().GetService<IHttpServer>();
        httpServer.Load(builder);
    }

    private static void RegisterComponents(IServiceCollection builderServices, IEnumerable<Type> types)
    {
        // We get all injectable services to register them on our services
        foreach (var injectableType in types)
        {
            var attribute = (Injectable) Attribute.GetCustomAttribute(injectableType, typeof(Injectable))!;
            var registerableType = injectableType;
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
            
            switch (attribute.InjectionType)
            {
                case InjectionType.Singleton:
                    builderServices.AddSingleton(registerableType, injectableType);
                    break;
                case InjectionType.Transient:
                    builderServices.AddTransient(registerableType, injectableType);
                    break;
                case InjectionType.Scoped:
                    builderServices.AddScoped(registerableType, injectableType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }        
    }
    
    private static void RegisterSptComponents(IServiceCollection builderServices)
    {
        // We get all the services from this assembly first, since mods will override them later
        RegisterComponents(builderServices, typeof(Program).Assembly.GetTypes()
            .Where(type => Attribute.IsDefined(type, typeof(Injectable))));
    }
}