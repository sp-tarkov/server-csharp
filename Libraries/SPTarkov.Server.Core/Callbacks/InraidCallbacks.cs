using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class InraidCallbacks(
    InRaidController _inRaidController,
    HttpResponseUtil _httpResponseUtil
)
{
    /// <summary>
    ///     Handle client/location/getLocalloot
    ///     Store active map in profile + applicationContext
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">register player request</param>
    /// <param name="sessionID">Session id</param>
    /// <returns>Null http response</returns>
    public string RegisterPlayer(string url, RegisterPlayerRequestData info, string sessionID)
    {
        _inRaidController.AddPlayer(sessionID, info);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle raid/profile/scavsave
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">Save progress request</param>
    /// <param name="sessionID">Session id</param>
    /// <returns>Null http response</returns>
    public string SaveProgress(string url, ScavSaveRequestData info, string sessionID)
    {
        _inRaidController.SavePostRaidProfileForScav(info, sessionID);
        return _httpResponseUtil.NullResponse();
    }

    /// <summary>
    ///     Handle singleplayer/settings/raid/menu
    /// </summary>
    /// <returns>JSON as string</returns>
    public string GetRaidMenuSettings()
    {
        return _httpResponseUtil.NoBody(_inRaidController.GetInRaidConfig().RaidMenuSettings);
    }

    /// <summary>
    ///     Handle singleplayer/scav/traitorscavhostile
    /// </summary>
    /// <returns></returns>
    public string GetTraitorScavHostileChance(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NoBody(_inRaidController.GetTraitorScavHostileChance(url, sessionID));
    }

    /// <summary>
    ///     Handle singleplayer/bosstypes
    /// </summary>
    /// <returns></returns>
    public string GetBossTypes(string url, EmptyRequestData _, string sessionID)
    {
        return _httpResponseUtil.NoBody(_inRaidController.GetBossTypes(url, sessionID));
    }
}
