using Core.DI;
using Core.Models.External;
using Core.Models.Utils;
using SptCommon.Annotations;

namespace Core.Loaders;

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
