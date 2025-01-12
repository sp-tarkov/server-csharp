using Core.Annotations;
using Core.DI;
using Core.Models.External;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Loaders;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.PostDBModLoader)]
public class PostDBModLoader : OnLoad
{
    private readonly ILogger _logger;
    private readonly IEnumerable<IPostDBLoadMod> _postDbLoadMods;

    public PostDBModLoader(
        ILogger logger,
        IEnumerable<IPostDBLoadMod> postDbLoadMods
    )
    {
        _logger = logger;
        _postDbLoadMods = postDbLoadMods;
    }

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
