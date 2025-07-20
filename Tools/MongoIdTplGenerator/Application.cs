using MongoIdTplGenerator.Generators;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;

namespace MongoIdTplGenerator;

[Injectable(InjectionType.Singleton)]
public class Application(
    ISptLogger<Application> logger,
    IEnumerable<IOnLoad> onloadComponents,
    IEnumerable<IMongoIdGenerator> generators
)
{
    public async Task Run()
    {
        foreach (var onLoad in onloadComponents)
        {
            await onLoad.OnLoad();
        }

        try
        {
            foreach (var generator in generators)
            {
                await generator.Run();
            }
        }
        catch (Exception e)
        {
            logger.Critical("Error running generator(s)", e);
        }
    }
}
