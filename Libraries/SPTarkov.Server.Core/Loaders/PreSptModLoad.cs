using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PreSptModLoader)]
public sealed class PreSptModLoader(ISptLogger<PreSptModLoader> logger, IEnumerable<IPreSptLoadModAsync> preSptLoadMods) : IOnLoad
{
    public async Task OnLoad(CancellationToken cancellationToken)
    {
        if (ProgramStatics.MODS())
        {
            logger.Info("Loading PreSptMods...");
            foreach (var postSptLoadMod in preSptLoadMods)
            {
                await postSptLoadMod.PreSptLoadAsync(cancellationToken);
            }

            logger.Info("Finished loading PreSptMods...");
        }
    }
}
