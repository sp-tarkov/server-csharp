using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SptCommon.Annotations;

namespace SptDependencyInjection;

public static class DependencyInjectionRegistrator
{
    public static void RegisterModOverrideComponents(IServiceCollection builderServices, List<Assembly> assemblies)
    {
        // We get all the services from this assembly first, since mods will override them later
        RegisterComponents(
            builderServices,
            assemblies.SelectMany(a => a.GetTypes())
                .Where(type => Attribute.IsDefined(type, typeof(Injectable)))
        );
    }

    public static void RegisterComponents(IServiceCollection builderServices, IEnumerable<Type> types)
    {
        var groupedTypes = types.SelectMany(
                t =>
                {
                    var attributes = (Injectable[])Attribute.GetCustomAttributes(t, typeof(Injectable));
                    var registerableType = t;
                    var registerableComponents = new List<RegisterableType>();
                    foreach (var attribute in attributes)
                    {
                        // if we have a type override this takes priority
                        if (attribute.InjectableTypeOverride != null)
                            registerableType = attribute.InjectableTypeOverride;
                        // if this class only has 1 interface we register it on that interface
                        else if (registerableType.GetInterfaces().Length == 1) registerableType = registerableType.GetInterfaces()[0];

                        registerableComponents.Add(new RegisterableType(registerableType, t, attribute));
                    }

                    return registerableComponents;
                }
            )
            .GroupBy(t => t.RegisterableInterface.FullName);
        // We get all injectable services to register them on our services
        foreach (var groupedInjectables in groupedTypes)
        foreach (var valueTuple in groupedInjectables.OrderBy(t => t.InjectableAttribute.TypePriority))
            if (valueTuple.TypeToRegister.IsGenericType)
                RegisterGenericComponents(builderServices, valueTuple);
            else
                RegisterComponent(
                    builderServices,
                    valueTuple.InjectableAttribute.InjectionType,
                    valueTuple.RegisterableInterface,
                    valueTuple.TypeToRegister
                );
    }

    private static List<Type>? _allLoadedTypes;
    private static List<ConstructorInfo>? _allConstructors;

    private static void RegisterGenericComponents(IServiceCollection builderServices, RegisterableType valueTuple)
    {
        _allLoadedTypes ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).ToList();
        _allConstructors ??= _allLoadedTypes.SelectMany(t => t.GetConstructors()).ToList();

        var typeName = $"{valueTuple.RegisterableInterface.Namespace}.{valueTuple.RegisterableInterface.Name}";
        try
        {
            var matchedConstructors = _allConstructors.Where(
                c => c.GetParameters()
                    .Any(
                        p => p.ParameterType.IsGenericType &&
                             p.ParameterType.GetGenericTypeDefinition().FullName == typeName
                    )
            );

            var constructorInfos = matchedConstructors.ToList();
            if (constructorInfos.Count == 0) return;

            foreach (var matchedConstructor in constructorInfos)
            foreach (var parameterInfo in matchedConstructor.GetParameters()
                         .Where(
                             p => p.ParameterType.IsGenericType &&
                                  p.ParameterType.GetGenericTypeDefinition().FullName == typeName
                         ))
            {
                var parameters = parameterInfo.ParameterType.GetGenericArguments();
                var typedGeneric = valueTuple.TypeToRegister.MakeGenericType(parameters);
                RegisterComponent(
                    builderServices,
                    valueTuple.InjectableAttribute.InjectionType,
                    parameterInfo.ParameterType,
                    typedGeneric
                );
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static void RegisterComponent(
        IServiceCollection builderServices,
        InjectionType injectionType,
        Type registerableInterface,
        Type imlementationType
    )
    {
        switch (injectionType)
        {
            case InjectionType.Singleton:
                builderServices.AddSingleton(registerableInterface, imlementationType);
                break;
            case InjectionType.Transient:
                builderServices.AddTransient(registerableInterface, imlementationType);
                break;
            case InjectionType.Scoped:
                builderServices.AddScoped(registerableInterface, imlementationType);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void RegisterSptComponents(
        Assembly serverLauncherAssembly,
        Assembly coreAssembly,
        IServiceCollection builderServices
    )
    {
        // We get all the services from this assembly first, since mods will override them later
        RegisterComponents(
            builderServices,
            serverLauncherAssembly.GetTypes().Where(type => Attribute.IsDefined(type, typeof(Injectable)))
        );
        RegisterComponents(
            builderServices,
            coreAssembly.GetTypes().Where(type => Attribute.IsDefined(type, typeof(Injectable)))
        );
    }

    private class RegisterableType(Type registerableInterface, Type typeToRegister, Injectable injectableAttribute)
    {
        public Type RegisterableInterface { get; } = registerableInterface;
        public Type TypeToRegister { get; } = typeToRegister;
        public Injectable InjectableAttribute { get; } = injectableAttribute;
    }
}
