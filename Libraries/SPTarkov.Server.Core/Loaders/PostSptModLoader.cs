using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Obsolete("This mod loader is obsolete and will be removed in 4.1.0. See documentation in IPostSptLoadModAsync for more information.")]
[Injectable(TypePriority = OnLoadOrder.PostSptModLoader)]
public class PostSptModLoader(
    ISptLogger<PostSptModLoader> _logger,
    IEnumerable<IPostSptLoadModAsync> _postSptLoadMods
) : IOnLoad
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            _logger.Info("Loading PostSptMods...");
            foreach (var postSptLoadMod in _postSptLoadMods)
            {
                await postSptLoadMod.PostSptLoadAsync();
            }

            _logger.Info("Finished loading PostSptMods...");
        }
    }
}
