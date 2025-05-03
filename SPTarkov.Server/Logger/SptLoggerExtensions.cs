using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Logger;

namespace SPTarkov.Server.Logger;

public static class SptLoggerExtensions
{

    public static IHostBuilder UseSptLogger(this IHostBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ConfigureServices((_, collection) =>
        {
            collection.AddSptLogger();
        });

        return builder;
    }

    public static IServiceCollection AddSptLogger(this IServiceCollection collection)
    {
        if (collection == null)
        {
            throw new ArgumentNullException(nameof(collection));
        }

        collection.AddSingleton<ILoggerFactory>(sp =>
        {
            return new SptLoggerProvider(sp.GetService<JsonUtil>(), sp.GetService<FileUtil>(), sp.GetService<SptLoggerQueueManager>());
        });

        return collection;
    }

}
