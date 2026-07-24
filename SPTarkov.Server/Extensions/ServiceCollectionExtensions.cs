using System.Reflection;
using System.Runtime.ExceptionServices;
using SPTarkov.Server.Core.DI;

namespace SPTarkov.Server.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public async Task AddModDIConstructorsAsync(IEnumerable<Assembly> modAssemblies, CancellationToken cancellationToken = default)
        {
            var candidates = modAssemblies
                .SelectMany(GetLoadableTypes)
                .Where(t => !t.IsInterface && !t.IsAbstract && typeof(IOnDIConstruct).IsAssignableFrom(t));

            foreach (var type in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await InvokeDIConstructorAsync(type, serviceCollection, cancellationToken);
            }
        }
    }

    private static async Task InvokeDIConstructorAsync(Type type, IServiceCollection serviceCollection, CancellationToken cancellationToken)
    {
        var method =
            type.GetMethod(nameof(IOnDIConstruct.OnDIConstructAsync), BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"Type '{type.FullName}' implements '{nameof(IOnDIConstruct)}' but does not expose a public static '{nameof(IOnDIConstruct.OnDIConstructAsync)}' method."
            );

        object? result;

        try
        {
            result = method.Invoke(null, [serviceCollection, cancellationToken]);
        }
        catch (TargetInvocationException e) when (e.InnerException is not null)
        {
            // Unwrap so cancellation and mod errors surface as themselves rather than a reflection wrapper
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            throw;
        }

        if (result is not Task task)
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
