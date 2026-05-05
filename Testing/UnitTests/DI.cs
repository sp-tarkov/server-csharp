using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services.Hosted;
using UnitTests.Mock;

namespace UnitTests;

[TestFixture]
public class DI
{
    private static IServiceProvider _serviceProvider = default!;

    private static DI? _instance;

    private DI()
    {
        ConfigureServices();
    }

    public static DI GetInstance()
    {
        return _instance ??= new DI();
    }

    private void ConfigureServices()
    {
        if (_serviceProvider != null)
        {
            return;
        }

        var mockLogger = new MockLogger<DI>();
        var configuration = SPTConfigLoader.Initialize(mockLogger).GetAwaiter().GetResult();

        var services = new ServiceCollection();
        services.AddSingleton(mockLogger);
        services.AddSingleton(typeof(ILogger<>), typeof(MockLogger<>));
        services.AddSingleton(typeof(ISptLogger<>), typeof(MockLogger<>));

        foreach (var configEntry in configuration)
        {
            services.AddSingleton(configEntry.Key, configEntry.Value);
        }

        var diHandler = new DependencyInjectionHandler(services);

        diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTStartupHostedService));

        diHandler.InjectAll();

        services.AddSingleton<IReadOnlyList<SptMod>>(_ => []);

        _serviceProvider = services.BuildServiceProvider();

        var cancellationTokenSource = new CancellationTokenSource();

        foreach (var onLoad in _serviceProvider.GetServices<IOnLoad>())
        {
            onLoad.OnLoad(cancellationTokenSource.Token).Wait();
        }
    }

    public T GetService<T>()
        where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
