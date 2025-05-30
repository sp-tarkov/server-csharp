using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.External;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Loaders;

[Obsolete("This mod loader is obsolete and will be removed in 4.1.0. See documentation in IPostDBLoadModAsync for more information.")]
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader)]
public class PostDBModLoader(
    ISptLogger<PostDBModLoader> _logger,
    IEnumerable<IPostDBLoadModAsync> _postDbLoadMods
) : IOnLoad
{
    public async Task OnLoad()
    {
        if (ProgramStatics.MODS())
        {
            _logger.Info("Loading PostDBMods...");
            foreach (var postDbLoadMod in _postDbLoadMods)
            {
                await postDbLoadMod.PostDBLoadAsync();
            }

            _logger.Info("Finished loading PostDBMods...");
        }
    }
}
