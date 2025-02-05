using Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using SptDependencyInjection;

namespace HideoutCraftQuestIdGenerator;

public class HideoutCraftQuestIdGeneratorLauncher
{
    public static void Main(string[] args)
    {
        try
        {
            var serviceCollection = new ServiceCollection();
            DependencyInjectionRegistrator.RegisterSptComponents(
                typeof(HideoutCraftQuestIdGeneratorLauncher).Assembly,
                typeof(App).Assembly,
                serviceCollection
            );
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
