using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SPTarkov.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services.Hosted;
using SPTarkov.Server.Core.Utils;

namespace MongoIdTplGenerator;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            ProgramStatics.Initialize();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(WebApplication.CreateBuilder());
            serviceCollection.AddSingleton<IReadOnlyList<SptMod>>([]);
            var diHandler = new DependencyInjectionHandler(serviceCollection);

            diHandler.AddInjectableTypesFromTypeAssembly(typeof(Program));
            diHandler.AddInjectableTypesFromTypeAssembly(typeof(SPTStartupHostedService));

            diHandler.InjectAll();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            await serviceProvider.GetService<Application>()?.Run()!;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
