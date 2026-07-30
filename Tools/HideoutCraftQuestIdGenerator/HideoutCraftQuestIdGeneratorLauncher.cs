using HideoutCraftQuestIdGenerator.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services.Hosted;
using SPTarkov.Server.Core.Utils;

namespace HideoutCraftQuestIdGenerator;

public class HideoutCraftQuestIdGeneratorLauncher
{
    public static async Task Main(string[] args)
    {
        try
        {
            ProgramStatics.Initialize();

            var configuration = await SPTConfigLoader.Initialize();

            var serviceCollection = new ServiceCollection();

            foreach (var configEntry in configuration)
            {
                serviceCollection.AddSingleton(configEntry.Key, configEntry.Value);
            }

            // NOTE:
            // This can be removed after SPT 4.1, it is to make ConfigServer backwards compatible with the older way of doing things giving people time to migrate.
            IReadOnlyDictionary<Type, BaseConfig> readonlyConfigurationDictionary = configuration;
            serviceCollection.AddSingleton(readonlyConfigurationDictionary);

            serviceCollection.AddSingleton(WebApplication.CreateBuilder());
            serviceCollection.AddSingleton<IReadOnlyList<SptMod>>([]);
            var diHandler = new DependencyInjectionHandler(serviceCollection);
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(HideoutCraftQuestIdGeneratorLauncher));
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTStartupHostedService));
            diHandler.InjectAll();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            await serviceProvider.GetRequiredService<HideoutCraftQuestIdGenerator>().Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
