using Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using SptDependencyInjection;

namespace ItemTplGenerator;

public class ItemTplGeneratorLauncher
{
    public static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        DependencyInjectionRegistrator.RegisterSptComponents(typeof(ItemTplGeneratorLauncher).Assembly, typeof(App).Assembly, serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        serviceProvider.GetService<ItemTplGenerator>().Run();
    }
}
