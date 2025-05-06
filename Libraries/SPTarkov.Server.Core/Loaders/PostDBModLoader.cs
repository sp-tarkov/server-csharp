using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader)]
public class PostDBModLoader(
    ISptLogger<PostDBModLoader> _logger,
    IEnumerable<IPostDBLoadMod> _postDbLoadMods
) : IOnLoad
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            _logger.Info("Loading PostDBMods...");
            foreach (var postDbLoadMod in _postDbLoadMods)
            {
                postDbLoadMod.PostDBLoad();
            }

            _logger.Info("Finished loading PostDBMods...");
        }
    }

    public string GetRoute()
    {
        return "spt-post-db-mods";
    }
}
