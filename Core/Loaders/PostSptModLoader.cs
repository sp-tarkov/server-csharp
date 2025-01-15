using Core.Annotations;
using Core.DI;
using Core.Models.External;
using Core.Models.Utils;


namespace Core.Loaders;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.PostSptModLoader)]
public class PostSptModLoader : OnLoad
{
    protected ISptLogger<PostSptModLoader> _logger;
    protected IEnumerable<IPostSptLoadMod> _postSptLoadMods;

    public PostSptModLoader(
        ISptLogger<PostSptModLoader> logger,
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
