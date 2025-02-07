using Core.DI;
using Core.Models.External;
using Core.Models.Utils;
using Server;
using SptCommon.Annotations;

namespace Core.Loaders;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.PostSptModLoader)]
public class PostSptModLoader(
    ISptLogger<PostSptModLoader> _logger,
    IEnumerable<IPostSptLoadMod> _postSptLoadMods
) : OnLoad
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            // await _postSptModLoader.load();
            // TODO: Huh?
        }

        _logger.Info("Loading PostSptMods...");
        foreach (var postSptLoadMod in _postSptLoadMods)
        {
            postSptLoadMod.PostSptLoad();
        }

        _logger.Info("Finished loading PostSptMods...");
    }

    public string GetRoute()
    {
        return "spt-post-spt-mods";
    }
}
