using System.Text.Json;
using SPTarkov.Common.Logger;
using SPTarkov.Common.Models.Logging;
using ZLinq;

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

        var implementingTypes = interfaceType
            .Assembly.GetTypes()
            .AsValueEnumerable()
            .Where(type => interfaceType.IsAssignableFrom(type) && type != interfaceType && type.IsClass && !type.IsAbstract)
            .ToList();

        foreach (var implementation in implementingTypes)
        {
            serviceCollection.Add(new ServiceDescriptor(interfaceType, implementation, lifetime));
        }
    }

    public static IHostBuilder UseSptLoggerWithoutProvider(this IHostBuilder builder, IServiceProvider earlyLoggerServiceProvider)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ConfigureServices(
            (_, collection) =>
            {
                collection.AddSptLoggerWithoutProvider(earlyLoggerServiceProvider);
            }
        );

        return builder;
    }

    public static IServiceCollection AddSptLogger(this IServiceCollection collection, bool isDevelop = false)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (isDevelop)
        {
            collection.AddSingleton(LoadConfig(ConfigurationPathDev));
        }
        else
        {
            collection.AddSingleton(LoadConfig(ConfigurationPath));
        }

        collection.RegisterImplementations<ILogHandler>(ServiceLifetime.Singleton);

        collection.AddSingleton<SPTLoggerDispatcher>();
        collection.AddSingleton<SptLoggerProvider>();
        collection.AddSingleton<ILoggerProvider>(sp => sp.GetRequiredService<SptLoggerProvider>());
        collection.AddSingleton<ILoggerFactory>(sp => sp.GetRequiredService<SptLoggerProvider>());

        collection.AddTransient(typeof(SptLogger<>));
        collection.AddTransient(typeof(ISptLogger<>), typeof(SptLogger<>));

        return collection;
    }

    public static IServiceCollection AddSptLoggerWithoutProvider(
        this IServiceCollection collection,
        IServiceProvider earlyLoggerServiceProvider
    )
    {
        collection.AddSingleton(earlyLoggerServiceProvider.GetRequiredService<SptLoggerConfiguration>());
        collection.AddSingleton(earlyLoggerServiceProvider.GetRequiredService<SPTLoggerDispatcher>());
        collection.AddTransient(typeof(SptLogger<>));
        collection.AddTransient(typeof(ISptLogger<>), typeof(SptLogger<>));
        return collection;
    }
}
