using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Obsolete(
    "This mod loader is obsolete and will be removed in 4.1.0. See documentation in IPreSptLoadModAsync for more information.")]
[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.PreSptModLoader)]
public class PreSptModLoader(
    ISptLogger<PreSptModLoader> _logger,
    IEnumerable<IPreSptLoadModAsync> _preSptLoadMods
) : IOnLoad
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            _logger.Info("Loading PreSptMods...");
            foreach (var postSptLoadMod in _preSptLoadMods)
            {
                await postSptLoadMod.PreSptLoadAsync();
            }

            _logger.Info("Finished loading PreSptMods...");
        }
    }
}
