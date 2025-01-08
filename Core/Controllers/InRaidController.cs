using Core.Models.Eft.InRaid;

namespace Core.Controllers;

public class InRaidController
{
    /// <summary>
    /// Save locationId to active profiles in-raid object AND app context
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="info">Register player request</param>
    public void AddPlayer(
        string sessionId,
        RegisterPlayerRequestData info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle raid/profile/scavsave
    /// Save profile state to disk
    /// Handles pmc/pscav
    /// </summary>
    /// <param name="offRaidProfileData"></param>
    /// <param name="sessionId"></param>
    public void SavePostRaidProfileForScav(
        ScavSaveRequestData offRaidProfileData,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the inraid config from configs/inraid.json
    /// </summary>
    public void GetInRaidConfig()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public float GetTraitorScavHostileChance(
        string url,
        string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public List<string> GetBossConvertSettings(
        string url,
        string sessionId)
    {
        throw new NotImplementedException();
    }
}