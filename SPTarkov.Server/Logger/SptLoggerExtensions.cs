using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;

namespace SPTarkov.Server.Logger;

public static class SptLoggerExtensions
{

    public static IHostBuilder UseSptLogger(this IHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureServices((_, collection) =>
        {
            collection.AddSptLogger();
        });

        return builder;
    }

    public static IServiceCollection AddSptLogger(this IServiceCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        collection.AddSingleton<ILoggerFactory>(sp =>
            new SptLoggerProvider(sp.GetService<JsonUtil>(), sp.GetService<FileUtil>(), sp.GetService<SptLoggerQueueManager>()));

        return collection;
    }

}
