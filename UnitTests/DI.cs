using Microsoft.Extensions.DependencyInjection;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Json.Converters;
using UnitTests.Mock;

namespace UnitTests;

[TestClass]
public class DI
{
    private static IServiceProvider _serviceProvider;

    [AssemblyInitialize]
    public static void ConfigureServices(TestContext context)
    {
        if (_serviceProvider != null)
        {
            return;
        }

        var services = new ServiceCollection();
        var jsonUtil = new JsonUtil([ new SptJsonConverterRegistrator() ]);
        var mathUtil = new MathUtil();

        services.AddSingleton<JsonUtil>(jsonUtil);
        services.AddSingleton<MathUtil>(mathUtil);
        services.AddSingleton<ICloner,JsonCloner>();
        services.AddSingleton<ISptLogger<RandomUtil>,MockLogger<RandomUtil>>();
        services.AddSingleton<RandomUtil>();
        services.AddSingleton<HashUtil>();

        _serviceProvider = services.BuildServiceProvider();
    }

    public static T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}
