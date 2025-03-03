using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Prestige;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Controllers;

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
    /// </summary>
    /// <param name="sessionId">Session/Player id</param>
    /// <param name="info"></param>
    /// <returns></returns>
    public Prestige GetPrestige(
        string sessionId,
        EmptyRequestData info)
    {
        return _databaseService.GetTemplates().Prestige;
    }

    /// <summary>
    ///     <para>Handle /client/prestige/obtain</para>
    ///     Going to Prestige 1 grants the below
    ///     <list type="bullet">
    ///         <item>5% of skills should be transfered over</item>
    ///         <item>5% of mastering should be transfered over</item>
    ///         <item>Earned achievements should be transfered over</item>
    ///         <item>Profile stats should be transfered over</item>
    ///         <item>Prestige progress should be transfered over</item>
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
    public void ObtainPrestige(
        string sessionId,
        ObtainPrestigeRequestList request)
    {
        var profile = _profileHelper.GetFullProfile(sessionId);

        if (profile is not null)
        {
            var pendingPrestige = new PendingPrestige
            {
                PrestigeLevel = profile.CharacterData.PmcData.Info.PrestigeLevel + 1,
                Items = request
            };

            profile.SptData.PendingPrestige = pendingPrestige;
            profile.ProfileInfo.IsWiped = true;

            _saveServer.SaveProfile(sessionId);
        }
    }
}
