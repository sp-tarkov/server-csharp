using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI;
using SPTarkov.Server.Core.Utils;

namespace ItemTplGenerator;

public class ItemTplGeneratorLauncher
{
    public static void Main(string[] args)
    {
        try
        {
            var serviceCollection = new ServiceCollection();
            DependencyInjectionRegistrator.RegisterSptComponents(
                typeof(ItemTplGeneratorLauncher).Assembly,
                typeof(App).Assembly,
                serviceCollection
            );
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
