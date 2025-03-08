using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Loaders;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.PostDBModLoader)]
public class PostDBModLoader(
    ISptLogger<PostDBModLoader> _logger,
    IEnumerable<IPostDBLoadMod> _postDbLoadMods
) : IOnLoad
{
    public async Task OnLoad()
    {
        _logger.Info("Loading PostDBLoadMod...");
        foreach (var postDbLoadMod in _postDbLoadMods)
        {
            postDbLoadMod.PostDBLoad();
        }

        _logger.Info("Finished loading PostDBLoadMod...");
    }

    public string GetRoute()
    {
        return "spt-post-db-mods";
    }
}
