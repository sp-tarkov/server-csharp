using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SPTarkov.DI.Annotations;

namespace SPTarkov.DI;

public class DependencyInjectionHandler(IServiceCollection serviceCollection)
{
    private readonly HashSet<Assembly> _injectableAssemblies = [];
    private List<ConstructorInfo>? _candidateConstructors;

    private readonly Dictionary<string, Type> _injectedTypeNames = [];

    private readonly Dictionary<string, object> _injectedValues = [];
    private readonly Lock _injectedValuesLock = new();

    private bool _oneTimeUseFlag;

    public void AddInjectableTypesFromAssembly(Assembly assembly)
    {
        AddInjectableTypesFromTypeList(assembly.GetTypes());
    }

    public void AddInjectableTypesFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            AddInjectableTypesFromAssembly(assembly);
        }
    }

    public void AddInjectableTypesFromTypeAssembly(Type type)
    {
        AddInjectableTypesFromAssembly(type.Assembly);
    }

    public void AddInjectableTypesFromTypeList(IEnumerable<Type> types)
    {
        var candidates = types as IReadOnlyCollection<Type> ?? types.ToList();

        foreach (var assembly in candidates.Select(type => type.Assembly))
        {
            _injectableAssemblies.Add(assembly);
        }

        var typesToInject = candidates.Where(type =>
            Attribute.IsDefined(type, typeof(Injectable)) && !_injectedTypeNames.ContainsKey($"{type.Namespace}.{type.Name}")
        );
        if (typesToInject.Any())
        {
            foreach (var type in typesToInject)
            {
                _injectedTypeNames.Add($"{type.Namespace}.{type.Name}", type);
            }
        }
    }

    public void InjectAll()
    {
        if (_oneTimeUseFlag)
        {
            throw new Exception("Invalid usage of DependencyInjectionHandler, this is a one time use service!");
        }
        _oneTimeUseFlag = true;
        var typeRefValues = _injectedTypeNames.Values.Select(t => new DependencyInjectionContainer(
            ((Injectable[])Attribute.GetCustomAttributes(t, typeof(Injectable)))[0],
            t,
            t
        ));

        // All the components sorted and ready to be inserted into the DI container
        var sortedInjectableTypes = typeRefValues.OrderBy(tRef => tRef.InjectableAttribute.TypePriority);

        List<DependencyInjectionContainer> dependencyInjectionContainers = [];

        foreach (var typeRefToInject in sortedInjectableTypes)
        {
            var nodes = new Queue<DependencyInjectionContainer>();
            nodes.Enqueue(typeRefToInject);
            foreach (var implementedInterface in typeRefToInject.Type.GetInterfaces().Where(t => !t.Namespace.StartsWith("System")))
            {
                nodes.Enqueue(
                    new DependencyInjectionContainer(typeRefToInject.InjectableAttribute, typeRefToInject.Type, implementedInterface)
                );
            }

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();

                if (node.Type.BaseType != null && node.Type.BaseType != typeof(object))
                {
                    nodes.Enqueue(new DependencyInjectionContainer(node.InjectableAttribute, typeRefToInject.Type, node.Type.BaseType));
                }

                if (node.Type.IsGenericType)
                {
                    RegisterGenericComponents(node);
                }
                else
                {
                    RegisterComponent(node.InjectableAttribute.InjectionType, node.Type, node.ParentType);
                }

                dependencyInjectionContainers.Add(node);
            }
        }

        serviceCollection.AddSingleton<IReadOnlyList<DependencyInjectionContainer>>(dependencyInjectionContainers);
    }

    private void RegisterGenericComponents(DependencyInjectionContainer typeRef)
    {
        _candidateConstructors ??= _injectableAssemblies
            .SelectMany(GetLoadableTypes)
            .SelectMany(type => type.GetConstructors())
            .ToList();

        var typeName = $"{typeRef.Type.Namespace}.{typeRef.Type.Name}";
        try
        {
            var matchedConstructors = _candidateConstructors.Where(c =>
                c.GetParameters().Any(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition().FullName == typeName)
            );

            var constructorInfos = matchedConstructors.ToList();
            if (constructorInfos.Count == 0)
            {
                return;
            }

            foreach (var matchedConstructor in constructorInfos)
            {
                var constructorParams = matchedConstructor.GetParameters();
                foreach (var parameterInfo in constructorParams.Where(x => IsMatchingGenericType(x, typeName)))
                {
                    var parameters = parameterInfo.ParameterType.GetGenericArguments();
                    var typedGeneric = typeRef.ParentType.MakeGenericType(parameters);
                    RegisterComponent(typeRef.InjectableAttribute.InjectionType, parameterInfo.ParameterType, typedGeneric);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine($"COULD NOT LOAD TYPE: {ex}");

            return ex.Types.Where(type => type is not null)!;
        }
    }

    private static bool IsMatchingGenericType(ParameterInfo paramInfo, string typeName)
    {
        return paramInfo.ParameterType.IsGenericType && paramInfo.ParameterType.GetGenericTypeDefinition().FullName == typeName;
    }

    private void RegisterComponent(InjectionType injectionType, Type registrableInterface, Type implementationType)
    {
        switch (injectionType)
        {
            case InjectionType.HostedService:
                if (!typeof(IHostedService).IsAssignableFrom(implementationType))
                {
                    throw new ArgumentException(
                        $"Invalid hosted service registration: {implementationType.FullName} does not derive from or implement IHostedService.",
                        nameof(implementationType)
                    );
                }

                serviceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), implementationType));
                break;
            case InjectionType.Singleton:
                if (registrableInterface == typeof(IHostedService))
                {
                    throw new ArgumentException(
                        $"Invalid injection type on {implementationType.Namespace}.{implementationType.Name}, should be HostedService!",
                        nameof(injectionType)
                    );
                }

                HandleSingletonRegistration(registrableInterface, implementationType);
                break;
            case InjectionType.Transient:
                if (registrableInterface == typeof(IHostedService))
                {
                    throw new ArgumentException(
                        $"Invalid injection type on {implementationType.Namespace}.{implementationType.Name}, should be HostedService!",
                        nameof(injectionType)
                    );
                }

                serviceCollection.AddTransient(registrableInterface, implementationType);
                break;
            case InjectionType.Scoped:
                if (registrableInterface == typeof(IHostedService))
                {
                    throw new ArgumentException(
                        $"Invalid injection type on {implementationType.Namespace}.{implementationType.Name}, should be HostedService!",
                        nameof(injectionType)
                    );
                }

                serviceCollection.AddScoped(registrableInterface, implementationType);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(injectionType),
                    $"Unknown injection type on {implementationType.Namespace}.{implementationType.Name}"
                );
        }
    }

    private void HandleSingletonRegistration(Type registrableInterface, Type implementationType)
    {
        var serviceKey = $"{implementationType.Namespace}.{implementationType.Name}";
        if (registrableInterface != implementationType)
        {
            serviceCollection.AddSingleton(
                registrableInterface,
                (serviceProvider) =>
                {
                    object service;
                    lock (_injectedValuesLock)
                    {
                        if (!_injectedValues.TryGetValue(serviceKey, out service))
                        {
                            service = serviceProvider.GetService(implementationType);
                            _injectedValues.Add(serviceKey, service);
                        }
                    }

                    return service;
                }
            );
        }
        else
        {
            serviceCollection.AddSingleton(registrableInterface, implementationType);
        }
    }
}

public sealed record DependencyInjectionContainer
{
    public Injectable InjectableAttribute { get; init; }
    public Type Type { get; init; }
    public Type ParentType { get; init; }

    public DependencyInjectionContainer(Injectable injectable, Type parentType, Type type)
    {
        InjectableAttribute = injectable;
        Type = type;
        ParentType = parentType;
    }
}
