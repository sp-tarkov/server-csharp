namespace SPTarkov.Server.Core.DI;

/// <summary>
/// A service locator designed specifically for Harmony patches and other
/// parts of the application that do not have direct access to the Dependency Injection (DI) system.
///
/// This should not be used at all when having direct access to DI.
/// </summary>
public static class ServiceLocator
{
    public static IServiceProvider ServiceProvider { get; private set; }

    internal static void SetServiceProvider(IServiceProvider provider)
    {
        ServiceProvider = provider;
    }
}
