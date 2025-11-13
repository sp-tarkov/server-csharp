using SPTarkov.Common.Extensions;
using SPTarkov.Common.Logger.Util;

namespace SPTarkov.Common.Logger;

public sealed class SptLoggerProvider(SptLoggerQueueManager sptLoggerQueueManager, SptLogger<SPTLoggerWrapper> logger)
    : ILoggerProvider,
        ILoggerFactory
{
    private readonly List<ILoggerProvider> _loggerProviders = [];

    public void AddProvider(ILoggerProvider provider)
    {
        _loggerProviders?.Add(provider);
    }

    public static ILoggerFactory Create(bool isDevelop)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSptLogger(isDevelop);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        return new SptEarlyLoggerFactory(loggerFactory, serviceProvider);
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!sptLoggerQueueManager.Initialized)
        {
            sptLoggerQueueManager.Initialize();
        }

        return new SPTLoggerWrapper(categoryName, logger);
    }

    public void Dispose()
    {
        // We can stop the file roller a bit early.
        LogFileRollMonitor.UnregisterHandler();

        sptLoggerQueueManager.DumpAndStop(TimeSpan.FromSeconds(1));

        GC.SuppressFinalize(this);
    }
}
