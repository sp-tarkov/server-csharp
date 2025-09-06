using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(InjectionType.Singleton)]
public class OnWebAppBuildModLoader(ISptLogger<OnWebAppBuildModLoader> logger, IEnumerable<IOnWebAppBuildModAsync> onWebAppBuildMods)
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            logger.Info("Loading OnWebAppBuildMods...");
            foreach (var onWebAppBuildMod in onWebAppBuildMods)
            {
                await onWebAppBuildMod.OnWebAppBuildAsync();
            }

            logger.Info("Finished loading OnWebAppBuildMods...");
        }
    }
}
