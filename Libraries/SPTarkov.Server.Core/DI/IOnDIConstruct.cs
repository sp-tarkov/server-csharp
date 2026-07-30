using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.DI;

/// <summary>
/// Provides a static hook that is invoked while the dependency injection container is being constructed.
/// </summary>
/// <remarks>
/// This hook is intended for advanced or special-case service registration.
///
/// In SPT, normal dependency injection should usually be handled by applying the appropriate <see cref="Injectable"/>
/// attribute to a class. Classes marked as injectable are discovered and registered automatically, so mods
/// do not need to use this interface for standard service registration.
///
/// This hook is useful when a mod needs to manually add services to the <see cref="IServiceCollection"/>
/// before the service provider is built. A common use case is registering mod-specific configurations
/// or other services that cannot be registered through the normal injectable attribute flow.
///
/// Implementations should avoid resolving services from the container here. At this point, the service
/// provider has not been built yet. This method should only add registrations to the provided
/// <see cref="IServiceCollection"/>.
/// </remarks>
public interface IOnDIConstruct
{
    /// <summary>
    /// Called during dependency injection construction, before the service provider is built.
    /// </summary>
    /// <param name="serviceCollection">
    /// The service collection that the mod may add additional service registrations to.
    /// </param>
    /// <param name="cancellationToken">Cancelled when server startup is aborted, for example by Ctrl-C.</param>
    /// <returns>
    /// A task that completes when the mod has finished adding its service registrations.
    /// </returns>
    static abstract Task OnDIConstructAsync(IServiceCollection serviceCollection, CancellationToken cancellationToken);
}
