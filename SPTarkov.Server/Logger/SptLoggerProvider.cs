using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;

namespace SPTarkov.Server.Logger;

[Injectable]
public class SptLoggerProvider(JsonUtil jsonUtil, FileUtil fileUtil, SptLoggerQueueManager queueManager) : ILoggerProvider, ILoggerFactory
{
    private readonly List<ILoggerProvider> _loggerProviders = [];

    public void Dispose() { }

    public void AddProvider(ILoggerProvider provider)
    {
        _loggerProviders?.Add(provider);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new SptLoggerWrapper(categoryName, jsonUtil, fileUtil, queueManager);
    }
}
