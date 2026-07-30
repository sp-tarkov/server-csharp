using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class ModLoaderCallbacks(
    ClientEnumDefinitions clientEnumDefinitions,
    ModdedTraderCustomizationController moddedTraderCustomizationController,
    HttpResponseUtil httpResponseUtil
)
{
    /// <summary>
    ///     Handle /singleplayer/customEnumEntries
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetCustomEnumEntries(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(clientEnumDefinitions.Entries.Values.SelectMany(e => e)));
    }

    /// <summary>
    ///     Handle /singleplayer/moddedTraders
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetCustomizationTraders(string url, EmptyRequestData _, MongoId sessionID)
    {
        return new ValueTask<string>(httpResponseUtil.NoBody(moddedTraderCustomizationController.GetCustomizationSellerIds()));
    }
}
