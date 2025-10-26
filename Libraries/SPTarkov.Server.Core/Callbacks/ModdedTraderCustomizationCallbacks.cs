using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class ModdedTraderCustomizationCallbacks(
    ModdedTraderCustomizationController moddedTraderCustomizationController,
    HttpResponseUtil httpResponseUtil
)
{
    /// <summary>
    ///     Handle /singleplayer/moddedTraders
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetCustomizationTraders(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(moddedTraderCustomizationController.GetCustomizationSellerIds()));
    }
}
