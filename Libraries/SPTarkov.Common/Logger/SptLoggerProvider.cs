using SPTarkov.Common.Extensions;

namespace SPTarkov.Common.Logger;

public sealed class SptLoggerProvider(SPTLoggerDispatcher dispatcher) : ILoggerProvider, ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotSupportedException("Adding external providers to SptLoggerProvider is not supported.");
    }

    public static SptEarlyLoggerFactory Create(bool isDevelop)
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSptLogger(isDevelop);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        return new SptEarlyLoggerFactory(loggerFactory, serviceProvider);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SPTLoggerWrapper(categoryName, dispatcher);
    }

    public void Dispose()
    {
        dispatcher.DisposeAsync().AsTask().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }
}
