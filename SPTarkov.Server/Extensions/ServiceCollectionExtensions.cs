using System.Reflection;
using SPTarkov.Server.Core.DI;

namespace SPTarkov.Server.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public async Task AddModDIConstructorsAsync(IEnumerable<Assembly> modAssemblies)
        {
            var candidates = modAssemblies
                .SelectMany(GetLoadableTypes)
                .Where(t => !t.IsInterface && !t.IsAbstract && typeof(IOnDIConstruct).IsAssignableFrom(t));

            foreach (var type in candidates)
            {
                await InvokeDIConstructorAsync(type, serviceCollection);
            }
        }
    }

    private static async Task InvokeDIConstructorAsync(Type type, IServiceCollection serviceCollection)
    {
        var method =
            type.GetMethod(nameof(IOnDIConstruct.OnDIConstructAsync), BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"Type '{type.FullName}' implements '{nameof(IOnDIConstruct)}' but does not expose a public static '{nameof(IOnDIConstruct.OnDIConstructAsync)}' method."
            );

        if (method.Invoke(null, [serviceCollection]) is not Task task)
        {
            throw new InvalidOperationException($"Method '{type.FullName}.{method.Name}' must return Task.");
        }

        await task;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }
}
