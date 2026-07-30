namespace SPTarkov.Common.Logger;

public sealed class SptEarlyLoggerFactory(ILoggerFactory loggerFactory, ServiceProvider serviceProvider) : ILoggerFactory
{
    public ServiceProvider ServiceProvider { get; } = serviceProvider;
    public SptLoggerProvider Provider { get; } = serviceProvider.GetRequiredService<SptLoggerProvider>();

    public ILogger CreateLogger(string categoryName)
    {
        return loggerFactory.CreateLogger(categoryName);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        loggerFactory.AddProvider(provider);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
