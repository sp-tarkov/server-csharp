using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI;
using SPTarkov.Server.Core.Utils;

namespace CreateSqlLiteDb;

public class CreateSqlLiteDbLauncher
{
    public static void Main(string[] args)
    {
        try
        {
            var serviceCollection = new ServiceCollection();
            DependencyInjectionRegistrator.RegisterSptComponents(
                typeof(CreateSqlLiteDbLauncher).Assembly,
                typeof(App).Assembly,
                serviceCollection
            );
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<CreateSqlLiteDb>().Run().Wait();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
