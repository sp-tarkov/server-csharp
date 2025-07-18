using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(InjectionType.Singleton)]
public class OnLoadModLoader(
    ISptLogger<OnLoadModLoader> _logger,
    IEnumerable<IOnLoadModAsync> _onLoadMods
)
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            _logger.Info("Loading OnLoadMods...");
            foreach (var onLoadMod in _onLoadMods)
            {
                await onLoadMod.OnLoadAsync();
            }

            _logger.Info("Finished loading OnLoadMods...");
        }
    }
}
