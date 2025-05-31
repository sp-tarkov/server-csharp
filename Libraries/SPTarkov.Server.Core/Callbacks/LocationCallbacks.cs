using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Location;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class LocationCallbacks(
    HttpResponseUtil _httpResponseUtil,
    LocationController _locationController
)
{
    /// <summary>
    ///     Handle client/locations
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetLocationData(string url, EmptyRequestData _, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_locationController.GenerateAll(sessionID)));
    }

    /// <summary>
    ///     Handle client/airdrop/loot
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> GetAirdropLoot(string url, GetAirdropLootRequest info, string sessionID)
    {
        return new ValueTask<string>(_httpResponseUtil.GetBody(_locationController.GetAirDropLoot(info)));
    }
}
