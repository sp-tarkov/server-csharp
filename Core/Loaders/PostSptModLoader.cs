using Core.Annotations;
using Core.DI;
using Core.Models.External;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Loaders;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.PostSptModLoader)]
public class PostSptModLoader : OnLoad
{
    private readonly ILogger _logger;
    private readonly IEnumerable<IPostSptLoadMod> _postSptLoadMods;

    public PostSptModLoader(
        ILogger logger,
        IEnumerable<IPostSptLoadMod> postSptLoadMods
    )
    {
        _logger = logger;
        _postSptLoadMods = postSptLoadMods;
    }

    public async Task OnLoad()
    {
        // if (ProgramStatics.MODS) {
        //     await this.postSptModLoader.load();
        // } TODO: needs to be implemented
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
