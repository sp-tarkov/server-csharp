using Core.Annotations;
using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.Location;
using Core.Utils;

namespace Core.Callbacks;

[Injectable]
public class LocationCallbacks
{
    protected HttpResponseUtil _httpResponseUtil;
    protected LocationController _locationController;

    public LocationCallbacks
    (
        HttpResponseUtil httpResponseUtil,
        LocationController locationController
    )
    {
        _httpResponseUtil = httpResponseUtil;
        _locationController = locationController;
    }

    /// <summary>
    /// Handle client/locations
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetLocationData(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_locationController.GenerateAll(sessionID));
    }

    /// <summary>
    /// Handle client/airdrop/loot
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public string GetAirdropLoot(string url, GetAirdropLootRequest info, string sessionID)
    {
        return _httpResponseUtil.GetBody(_locationController.GetAirDropLoot(info));
    }
}
