using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(TypePriority = OnLoadOrder.SPTPatchLoad)]
public class PatchLoader(
    ISptLogger<PatchLoader> _logger,
    IEnumerable<IPatchLoadMod> _modPatches)
    : IOnLoad
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            _logger.Info("Loading mod patches...");
            foreach (var patch in _modPatches)
            {
                patch.LoadPatches();
            }

            _logger.Info("Finished loading mod patches...");
        }
    }

    public string GetRoute()
    {
        return "spt-patch-mods";
    }
}
