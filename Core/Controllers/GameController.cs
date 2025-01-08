using Core.Models.Eft.Common;
using Core.Models.Eft.Game;
using Core.Models.Eft.Profile;

namespace Core.Controllers;

public class GameController
{
    /// <summary>
    /// Handle client/game/start
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionId"></param>
    /// <param name="startTimeStampMs"></param>
    public void GameStart(
        string url,
        EmptyRequestData info,
        string sessionId,
        long startTimeStampMs)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles migrating profiles from older SPT versions
    /// </summary>
    /// <param name="fullProfile"></param>
    /// <remarks>Formerly migrate39xProfile in node server</remarks>
    private void MigrateProfile(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/config
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GameConfigResponse GetGameConfig(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/mode
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="requestData"></param>
    /// <returns></returns>
    public object GetGameMode( // TODO: Returns `any` in node server
        string sessionId,
        GameModeRequestData requestData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/server/list
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public List<ServerDetails> GetServer(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/match/group/current
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    /*
    public CurrentGroupResponse GetCurrentGroup(string sessionId)
    {
        throw new NotImplementedException();
    }
    */

    /// <summary>
    /// Handle client/checkVersion
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public CheckVersionResponse GetValidGameVersion(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle client/game/keepalive
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public GameKeepAliveResponse GetKeepAlive(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle singleplayer/settings/getRaidTime
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public GetRaidTimeResponse GetRaidTime(
        string sessionId,
        GetRaidTimeRequest request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    public SurveyResponseData GetSurvey(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Players set botReload to a high value and don't expect the crazy fast reload speeds, give them a warn about it
    /// </summary>
    /// <param name="pmcProfile">Player profile</param>
    private void WarnOnActiveBotReloadSkill(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// When player logs in, iterate over all active effects and reduce timer
    /// </summary>
    /// <param name="pmcProfile">Profile to adjust values for</param>
    private void UpdateProfileHealthValues(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send starting gifts to profile after x days
    /// </summary>
    /// <param name="pmcProfile">Profile to add gifts to</param>
    private void SendPraporGiftsToNewProfiles(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a list of installed mods and save their details to the profile being used
    /// </summary>
    /// <param name="fullProfile">Profile to add mod details to</param>
    private void SaveActiveModsToProfile(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add the logged in players name to PMC name pool
    /// </summary>
    /// <param name="pmcProfile">Profile of player to get name from</param>
    private void AddPlayerToPmcNames(PmcData pmcProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check for a dialog with the key 'undefined', and remove it
    /// </summary>
    /// <param name="fullProfile">Profile to check for dialog in</param>
    private void CheckForAndRemoveUndefinedDialogues(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fullProfile"></param>
    private void LogProfileDetails(SptProfile fullProfile)
    {
        throw new NotImplementedException();
    }
}