using System.Text.Json;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Logger.Handlers.File;
using SPTarkov.Common.Logger.Util;
using SPTarkov.Common.Models.Logging;

namespace SPTarkov.Common.Extensions;

public static class SptLoggerExtensions
{
    private const string ConfigurationPath = "./sptLogger.json";
    private const string ConfigurationPathDev = "./sptLogger.Development.json";

    private static SptLoggerConfiguration LoadConfig(string configPath)
    {
        if (File.Exists(configPath))
        {
            using (FileStream fs = new(configPath, FileMode.Open, FileAccess.Read))
            {
                return JsonSerializer.Deserialize<SptLoggerConfiguration>(fs)
                    ?? throw new InvalidDataException($"Could not read SPTLogger config file {configPath}");
            }
        }
        else
        {
            throw new Exception($"Unable to find SPTLogger file '{configPath}'");
        }
    }

    private static void RegisterImplementations<TInterface>(
        this IServiceCollection serviceCollection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    )
        where TInterface : class
    {
        var interfaceType = typeof(TInterface);
        var implementingTypes = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !a.FullName.StartsWith("System") && !a.FullName.StartsWith("Microsoft"))
            .SelectMany(a => a.GetTypes())
            .Where(type => interfaceType.IsAssignableFrom(type) && type != interfaceType && type.IsClass && !type.IsAbstract)
            .ToList();

        foreach (var implementation in implementingTypes)
        {
            serviceCollection.Add(new ServiceDescriptor(interfaceType, implementation, lifetime));
        }
    }

    public static IHostBuilder UseSptLogger(this IHostBuilder builder, bool isDevelop = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureServices(
            (_, collection) =>
            {
                collection.AddSptLogger(isDevelop);
            }
        );

        return builder;
    }

    public static IServiceCollection AddSptLogger(this IServiceCollection collection, bool isDevelop = false)
    {
        ArgumentNullException.ThrowIfNull(collection);

        LogFileRollMonitor.RegisterHandler();

        if (isDevelop)
        {
            collection.AddSingleton(LoadConfig(ConfigurationPathDev));
        }
        else
        {
            collection.AddSingleton(LoadConfig(ConfigurationPath));
        }

        collection.AddSingleton<ILoggerFactory, SptLoggerProvider>();
        collection.AddSingleton<SptLoggerQueueManager>();
        collection.AddTransient(typeof(SptLogger<>));
        collection.AddTransient(typeof(ISptLogger<>), typeof(SptLogger<>));

        collection.RegisterImplementations<ILogHandler>(ServiceLifetime.Singleton);
        collection.RegisterImplementations<IFilePatternReplacer>(ServiceLifetime.Transient);

        return collection;
    }
}
