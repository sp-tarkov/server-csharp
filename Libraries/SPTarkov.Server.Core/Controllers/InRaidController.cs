using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.InRaid;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;

namespace SPTarkov.Server.Core.Controllers;

[Injectable]
public class InRaidController(ProfileHelper profileHelper, ConfigServer configServer)
{
    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly InRaidConfig InRaidConfig = configServer.GetConfig<InRaidConfig>();

    /// <summary>
    ///     Save locationId to active profiles in-raid object AND app context
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="info">Register player request</param>
    public void AddPlayer(MongoId sessionId, RegisterPlayerRequestData info)
    {
        // _applicationContext.AddValue(ContextVariableType.REGISTER_PLAYER_REQUEST, info);
    }

    /// <summary>
    ///     Handle raid/profile/scavsave
    ///     Save profile state to disk
    ///     Handles pmc/pscav
    /// </summary>
    /// <param name="offRaidProfileData"></param>
    /// <param name="sessionId">Session/Player id</param>
    public void SavePostRaidProfileForScav(ScavSaveRequestData offRaidProfileData, MongoId sessionId)
    {
        var serverScavProfile = profileHelper.GetScavProfile(sessionId);

        // If equipment match overwrite existing data from update to date raid data for scavenger screen to work correctly.
        // otherwise Scav inventory will be overwritten and break scav regeneration, breaking profile.
        if (serverScavProfile.Inventory.Equipment == offRaidProfileData.Inventory.Equipment)
        {
            serverScavProfile.Inventory.Items = offRaidProfileData.Inventory.Items;
        }
    }

    /// <summary>
    ///     Get the inraid config from configs/inraid.json
    /// </summary>
    public InRaidConfig GetInRaidConfig()
    {
        return InRaidConfig;
    }

    /// <summary>
    ///     Handle singleplayer/scav/traitorscavhostile
    ///     Get a % chance a scav will be hostile to the player when they're also a scav
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>% chance scav is hostile to player</returns>
    public double GetTraitorScavHostileChance(string url, MongoId sessionId)
    {
        return InRaidConfig.PlayerScavHostileChancePercent;
    }

    /// <summary>
    ///     Get all boss role types e.g. bossTagilla
    /// </summary>
    /// <param name="url"></param>
    /// <param name="sessionId">Session/Player id</param>
    /// <returns>string array of boss types</returns>
    public List<string> GetBossTypes(string url, MongoId sessionId)
    {
        return BotConfig.Bosses;
    }
}
