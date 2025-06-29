using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Utils;

namespace ItemTplGenerator;

public class ItemTplGeneratorLauncher
{
    public static void Main(string[] args)
    {
        try
        {
            ProgramStatics.Initialize();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(WebApplication.CreateBuilder());
            serviceCollection.AddSingleton<IReadOnlyList<SptMod>>([]);
            var diHandler = new DependencyInjectionHandler(serviceCollection);

            diHandler.AddInjectableTypesFromTypeAssembly(typeof(ItemTplGeneratorLauncher));
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(App));

            diHandler.InjectAll();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ItemTplGenerator>().Run().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
