using System.Reflection;
using SPTarkov.Common.Annotations;

namespace SPTarkov.DI;

public static class DependencyInjectionRegistrator
{
    private static List<Type>? _allLoadedTypes;
    private static List<ConstructorInfo>? _allConstructors;

    public static void RegisterModOverrideComponents(IServiceCollection builderServices, List<Assembly> assemblies)
    {
        // We get all the services from this assembly first, since mods will override them later
        RegisterComponents(
            builderServices,
            assemblies.SelectMany(a =>
            {
                return a.GetTypes();
            })
                .Where(type =>
                {
                    return Attribute.IsDefined(type, typeof(Injectable));
                })
        );
    }

    public static void RegisterComponents(IServiceCollection builderServices, IEnumerable<Type> types)
    {
        var groupedTypes = types.SelectMany(t =>
                {
                    var attributes = (Injectable[]) Attribute.GetCustomAttributes(t, typeof(Injectable));
                    var registerableType = t;
                    var registerableComponents = new List<RegistrableType>();
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

                        registerableComponents.Add(new RegistrableType(registerableType, t, attribute));
                    }

                    return registerableComponents;
                }
            )
            .GroupBy(t =>
            {
                return $"{t.RegistrableInterface.Namespace}.{t.RegistrableInterface.Name}";
            });
        // We get all injectable services to register them on our services
        foreach (var groupedInjectables in groupedTypes)
        {
            foreach (var valueTuple in groupedInjectables.OrderBy(t =>
            {
                return t.InjectableAttribute.TypePriority;
            }))
            {
                if (valueTuple.TypeToRegister.IsGenericType)
                {
                    RegisterGenericComponents(builderServices, valueTuple);
                }
                else
                {
                    RegisterComponent(
                        builderServices,
                        valueTuple.InjectableAttribute.InjectionType,
                        valueTuple.RegistrableInterface,
                        valueTuple.TypeToRegister
                    );
                }
            }
        }
    }

    private static void RegisterGenericComponents(IServiceCollection builderServices, RegistrableType valueTuple)
    {
        try
        {
            _allLoadedTypes ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(t =>
            {
                return t.GetTypes();
            }).ToList();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine($"COULD NOT LOAD TYPE: {ex}");
        }

        _allConstructors ??= _allLoadedTypes.SelectMany(t =>
        {
            return t.GetConstructors();
        }).ToList();

        var typeName = $"{valueTuple.RegistrableInterface.Namespace}.{valueTuple.RegistrableInterface.Name}";
        try
        {
            var matchedConstructors = _allConstructors.Where(c =>
            {
                return c.GetParameters()
                                .Any(p =>
                                {
                                    return p.ParameterType.IsGenericType &&
                                              p.ParameterType.GetGenericTypeDefinition().FullName == typeName;
                                });
            });

            var constructorInfos = matchedConstructors.ToList();
            if (constructorInfos.Count == 0)
            {
                return;
            }

            foreach (var matchedConstructor in constructorInfos)
            {
                var constructorParams = matchedConstructor.GetParameters();
                foreach (var parameterInfo in constructorParams.Where(x =>
                {
                    return IsMatchingGenericType(x, typeName);
                }))
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static bool IsMatchingGenericType(ParameterInfo paramInfo, string typeName)
    {
        return paramInfo.ParameterType.IsGenericType &&
               paramInfo.ParameterType.GetGenericTypeDefinition().FullName == typeName;
    }

    private static void RegisterComponent(
        IServiceCollection builderServices,
        InjectionType injectionType,
        Type registrableInterface,
        Type implementationType
    )
    {
        switch (injectionType)
        {
            case InjectionType.Singleton:
                builderServices.AddSingleton(registrableInterface, implementationType);
                break;
            case InjectionType.Transient:
                builderServices.AddTransient(registrableInterface, implementationType);
                break;
            case InjectionType.Scoped:
                builderServices.AddScoped(registrableInterface, implementationType);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(injectionType), "unknown injection type");
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
            serverLauncherAssembly.GetTypes().Where(type =>
            {
                return Attribute.IsDefined(type, typeof(Injectable));
            })
                .Concat(coreAssembly.GetTypes().Where(type =>
                {
                    return Attribute.IsDefined(type, typeof(Injectable));
                }))
        );
    }

    private sealed class RegistrableType(Type registrableInterface, Type typeToRegister, Injectable injectableAttribute)
    {
        public Type RegistrableInterface
        {
            get;
        } = registrableInterface;

        public Type TypeToRegister
        {
            get;
        } = typeToRegister;

        public Injectable InjectableAttribute
        {
            get;
        } = injectableAttribute;
    }
}
