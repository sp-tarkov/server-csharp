using Core.Controllers;
using Core.Models.Eft.Common;
using Core.Models.Eft.InRaid;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Callbacks;

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

    public string GetTraitorScavHostileChance(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_inRaidController.GetTraitorScavHostileChance(url, sessionID));
    }

    public string GetBossConvertSettings(string url, EmptyRequestData info, string sessionID)
    {
        return _httpResponseUtil.NoBody(_inRaidController.GetBossConvertSettings(url, sessionID));
    }
}
