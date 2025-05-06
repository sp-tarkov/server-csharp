using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI;
using SPTarkov.Server.Core.Utils;

namespace HideoutCraftQuestIdGenerator;

public class HideoutCraftQuestIdGeneratorLauncher
{
    public static void Main(string[] args)
    {
        try
        {
            var serviceCollection = new ServiceCollection();
            var diHandler = new DependencyInjectionHandler(serviceCollection);
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(HideoutCraftQuestIdGeneratorLauncher));
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(App));
            diHandler.InjectAll();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<HideoutCraftQuestIdGenerator>().Run().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
