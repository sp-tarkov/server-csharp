using MongoIdTplGenerator.Generators;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Models.Logging;

namespace MongoIdTplGenerator;

[Injectable(InjectionType.Singleton)]
public class Application(ISptLogger<Application> logger, IEnumerable<IOnLoad> onloadComponents, IEnumerable<IMongoIdGenerator> generators)
{
    public async Task Run()
    {
        var cancellationTokenSource = new CancellationTokenSource();

        foreach (var onLoad in onloadComponents)
        {
            await onLoad.OnLoad(cancellationTokenSource.Token);
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
