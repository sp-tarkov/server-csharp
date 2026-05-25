using SPTarkov.Common.Logger;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Helpers;

public static class ProgramHelpers
{
    public static WebApplicationBuilder CreateNewHostBuilder(
        SptEarlyLoggerFactory earlyFactory,
        IReadOnlyDictionary<Type, BaseConfig> configuration
    )
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = "./SPT_Data/wwwroot" });
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(earlyFactory.Provider);
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());

        foreach (var configEntry in configuration)
        {
            builder.Services.AddSingleton(configEntry.Key, configEntry.Value);
        }

        return builder;
    }
}
