using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Config;

namespace Core.Generators;

public class PlayerScavGenerator
{
    private PlayerScavConfig _playerScavConfig;

    public PlayerScavGenerator()
    {
        
    }
    
    /// <summary>
    /// Update a player profile to include a new player scav profile
    /// </summary>
    /// <param name="sessionID">session id to specify what profile is updated</param>
    /// <returns>profile object</returns>
    public PmcData Generate(string sessionID) 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add items picked from `playerscav.lootItemsToAddChancePercent`
    /// </summary>
    /// <param name="possibleItemsToAdd">dict of tpl + % chance to be added</param>
    /// <param name="scavData"></param>
    /// <param name="containersToAddTo">Possible slotIds to add loot to</param>
    protected void AddAdditionalLootToPlayerScavContainers(Dictionary<string, double> possibleItemsToAdd, BotBase scavData, List<string> containersToAddTo)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the scav karama level for a profile
    /// Is also the fence trader rep level
    /// </summary>
    /// <param name="pmcData">pmc profile</param>
    /// <returns>karma level</returns>
    protected double GetScavKarmaLevel(PmcData pmcData) 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a baseBot template
    /// If the parameter doesnt match "assault", take parts from the loot type and apply to the return bot template
    /// </summary>
    /// <param name="botTypeForLoot">bot type to use for inventory/chances</param>
    /// <returns>IBotType object</returns>
    protected BotType ConstructBotBaseTemplate(string botTypeForLoot) 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Adjust equipment/mod/item generation values based on scav karma levels
    /// </summary>
    /// <param name="karmaSettings">Values to modify the bot template with</param>
    /// <param name="baseBotNode">bot template to modify according to karama level settings</param>
    protected void AdjustBotTemplateWithKarmaSpecificSettings(KarmaLevel karmaSettings, BotType baseBotNode) 
    {
        throw new NotImplementedException();
    }
    
    protected Skills GetScavSkills(PmcData scavProfile) 
    {
        throw new NotImplementedException();
    }

    protected Skills GetDefaultScavSkills() 
    {
        throw new NotImplementedException();
    }

    protected Stats GetScavStats(PmcData scavProfile) 
    {
        throw new NotImplementedException();
    }

    protected int GetScavLevel(PmcData scavProfile) 
    {
        throw new NotImplementedException();
    }

    protected int GetScavExperience(PmcData scavProfile) 
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Set cooldown till scav is playable
    /// take into account scav cooldown bonus
    /// </summary>
    /// <param name="scavData">scav profile</param>
    /// <param name="pmcData">pmc profile</param>
    /// <returns></returns>
    protected PmcData SetScavCooldownTimer(PmcData scavData, PmcData pmcData) 
    {
        throw new NotImplementedException();
    }
}