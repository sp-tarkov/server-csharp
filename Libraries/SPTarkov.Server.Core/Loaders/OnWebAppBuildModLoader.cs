using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(InjectionType.Singleton)]
public class OnWebAppBuildModLoader(ISptLogger<OnWebAppBuildModLoader> _logger, IEnumerable<IOnWebAppBuildModAsync> _onWebAppBuildMods)
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            _logger.Info("Loading OnWebAppBuildMods...");
            foreach (var onWebAppBuildMod in _onWebAppBuildMods)
            {
                await onWebAppBuildMod.OnWebAppBuildAsync();
            }

            _logger.Info("Finished loading OnWebAppBuildMods...");
        }
    }
}
