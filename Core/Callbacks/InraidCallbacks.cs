using Core.Models.Eft.Common;
using Core.Models.Eft.HttpResponse;
using Core.Models.Eft.InRaid;

namespace Core.Callbacks;

public class InraidCallbacks
{
    public InraidCallbacks()
    {
    }

    /// <summary>
    /// Handle client/location/getLocalloot
    /// Store active map in profile + applicationContext
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">register player request</param>
    /// <param name="sessionID">Session id</param>
    /// <returns>Null http response</returns>
    public NullResponseData RegisterPlayer(string url, RegisterPlayerRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle raid/profile/scavsave
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info">Save progress request</param>
    /// <param name="sessionID">Session id</param>
    /// <returns>Null http response</returns>
    public NullResponseData SaveProgress(string url, ScavSaveRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/raid/menu
    /// </summary>
    /// <returns>JSON as string</returns>
    public string GetRaidMenuSettings()
    {
        throw new NotImplementedException();
    }

    public string GetTraitorScavHostileChance(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }

    public string GetBossConvertSettings(string url, EmptyRequestData info, string sessionID)
    {
        throw new NotImplementedException();
    }
}