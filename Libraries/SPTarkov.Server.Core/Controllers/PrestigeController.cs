using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Prestige;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class PrestigeController(
    ISptLogger<PrestigeController> _logger,
    ProfileHelper _profileHelper,
    DatabaseService _databaseService,
    SaveServer _saveServer
)
{
    /// <summary>
    ///     Handle /client/prestige/list
    ///     Get a collection of all possible prestiges
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>Prestige</returns>
    public Prestige GetPrestige(string sessionId)
    {
        return _databaseService.GetTemplates().Prestige;
    }

    /// <summary>
    ///     <para>Handle /client/prestige/obtain</para>
    ///     Going to Prestige 1 grants the below
    ///     <list type="bullet">
    ///         <item>5% of skills should be transferred over</item>
    ///         <item>5% of mastering should be transferred over</item>
    ///         <item>Earned achievements should be transferred over</item>
    ///         <item>Profile stats should be transferred over</item>
    ///         <item>Prestige progress should be transferred over</item>
    ///         <item>Items and rewards for Prestige 1</item>
    ///     </list>
    ///     Going to Prestige 2 grants the below
    ///     <list type="bullet">
    ///         <item>10% of skills should be transfered over</item>
    ///         <item>10% of mastering should be transfered over</item>
    ///         <item>Earned achievements should be transfered over</item>
    ///         <item>Profile stats should be transfered over</item>
    ///         <item>Prestige progress should be transfered over</item>
    ///         <item>Items and rewards for Prestige 2</item>
    ///     </list>
    ///     Each time resetting the below
    ///     <list type="bullet">
    ///         <item>Trader standing</item>
    ///         <item>Task progress</item>
    ///         <item>Character level</item>
    ///         <item>Stash</item>
    ///         <item>Hideout progress</item>
    ///     </list>
    /// </summary>
    /// <returns></returns>
    public void ObtainPrestige(string sessionId, ObtainPrestigeRequestList request)
    {
        var profile = _profileHelper.GetFullProfile(sessionId);
        if (profile is not null)
        {
            var pendingPrestige = new PendingPrestige
            {
                PrestigeLevel = profile.CharacterData.PmcData.Info.PrestigeLevel + 1,
                Items = request,
            };

            profile.SptData.PendingPrestige = pendingPrestige;
            profile.ProfileInfo.IsWiped = true;

            _saveServer.SaveProfile(sessionId);
        }
    }
}
