using System.Reflection;
using SPTarkov.DI.Annotations;

namespace SPTarkov.DI;

public class DependencyInjectionHandler
{
    private static List<Type>? _allLoadedTypes;
    private static List<ConstructorInfo>? _allConstructors;

    private readonly Dictionary<string, Type> _injectedTypeNames = new();
    private readonly IServiceCollection _serviceCollection;

    private readonly Dictionary<string, object> _injectedValues = new();
    private readonly object _injectedValuesLock = new();

    private bool _oneTimeUseFlag;


    public DependencyInjectionHandler(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

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
        var typesToInject = types.Where(type =>
            Attribute.IsDefined(type, typeof(Injectable)) &&
            !_injectedTypeNames.ContainsKey($"{type.Namespace}.{type.Name}"));
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
        var sortedInjectableTypes = _injectedTypeNames.Values
            .Select(t =>
                new TypeRefContainer(((Injectable[]) Attribute.GetCustomAttributes(t, typeof(Injectable)))[0], t, t))
            .OrderBy(tRef => tRef.InjectableAttribute.TypePriority);

        foreach (var typeRefToInject in sortedInjectableTypes)
        {
            var nodes = new Queue<TypeRefContainer>();
            nodes.Enqueue(typeRefToInject);
            foreach (var implementedInterface in typeRefToInject.Type.GetInterfaces()
                         .Where(t => !t.Namespace.StartsWith("System")))
            {
                nodes.Enqueue(new TypeRefContainer(typeRefToInject.InjectableAttribute, typeRefToInject.Type,
                    implementedInterface));
            }

            while (nodes.Any())
            {
                var node = nodes.Dequeue();
                if (node.Type.BaseType != null && node.Type.BaseType != typeof(object))
                {
                    nodes.Enqueue(new TypeRefContainer(node.InjectableAttribute, typeRefToInject.Type,
                        node.Type.BaseType));
                }

                if (node.Type.IsGenericType)
                {
                    RegisterGenericComponents(node);
                }
                else
                {
                    RegisterComponent(node.InjectableAttribute.InjectionType,
                        node.Type,
                        node.ParentType);
                }
            }
        }
    }

    private void RegisterGenericComponents(TypeRefContainer typeRef)
    {
        try
        {
            _allLoadedTypes ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).ToList();
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine($"COULD NOT LOAD TYPE: {ex}");
        }

        _allConstructors ??= _allLoadedTypes.SelectMany(t => t.GetConstructors()).ToList();

        var typeName = $"{typeRef.Type.Namespace}.{typeRef.Type.Name}";
        try
        {
            var matchedConstructors = _allConstructors.Where(c => c.GetParameters()
                .Any(p => p.ParameterType.IsGenericType &&
                          p.ParameterType.GetGenericTypeDefinition().FullName == typeName
                )
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
                    RegisterComponent(
                        typeRef.InjectableAttribute.InjectionType,
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

    private void RegisterComponent(
        InjectionType injectionType,
        Type registrableInterface,
        Type implementationType
    )
    {
        switch (injectionType)
        {
            case InjectionType.Singleton:
                HandleSingletonRegistration(registrableInterface, implementationType);
                break;
            case InjectionType.Transient:
                _serviceCollection.AddTransient(registrableInterface, implementationType);
                break;
            case InjectionType.Scoped:
                _serviceCollection.AddScoped(registrableInterface, implementationType);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(injectionType), "unknown injection type");
        }
    }

    private void HandleSingletonRegistration(Type registrableInterface, Type implementationType)
    {
        var serviceKey = $"{implementationType.Namespace}.{implementationType.Name}";
        if (registrableInterface != implementationType)
        {
            _serviceCollection.AddSingleton(registrableInterface, (serviceProvider) =>
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
            });
        }
        else
        {
            _serviceCollection.AddSingleton(registrableInterface, implementationType);
        }
    }

    private class TypeRefContainer
    {
        public Injectable InjectableAttribute { get; }
        public Type Type { get; }
        public Type ParentType { get; }

        public TypeRefContainer(Injectable injectable, Type parentType, Type type)
        {
            InjectableAttribute = injectable;
            Type = type;
            ParentType = parentType;
        }
    }
}
