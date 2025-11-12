namespace SPTarkov.Common.Logger;

internal sealed class SptEarlyLoggerFactory(ILoggerFactory loggerFactory, ServiceProvider serviceProvider) : ILoggerFactory
{
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    public void Dispose()
    {
        loggerFactory.Dispose();
        serviceProvider.Dispose();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return loggerFactory.CreateLogger(categoryName);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        loggerFactory.AddProvider(provider);
    }
}
