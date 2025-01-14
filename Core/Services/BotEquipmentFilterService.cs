using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotEquipmentFilterService
{
    /// <summary>
    /// Filter a bots data to exclude equipment and cartridges defines in the botConfig
    /// </summary>
    /// <param name="sessionId">Players id</param>
    /// <param name="baseBotNode">bots json data to filter</param>
    /// <param name="botLevel">Level of the bot</param>
    /// <param name="botGenerationDetails">details on how to generate a bot</param>
    public void FilterBotEquipment(
        string sessionId,
        BotType baseBotNode,
        int botLevel,
        BotGenerationDetails botGenerationDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over the changes passed in and apply them to baseValues parameter
    /// </summary>
    /// <param name="equipmentChanges">Changes to apply</param>
    /// <param name="baseValues">data to update</param>
    protected void AdjustChances(
        Dictionary<string, int> equipmentChanges,
        object baseValues)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate over the Generation changes and alter data in baseValues.Generation
    /// </summary>
    /// <param name="generationChanges">Changes to apply</param>
    /// <param name="baseBotGeneration">dictionary to update</param>
    protected void AdjustGenerationChances(
        Dictionary<string, GenerationData> generationChanges,
        Generation baseBotGeneration)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get equipment settings for bot
    /// </summary>
    /// <param name="botEquipmentRole">equipment role to return</param>
    /// <returns>EquipmentFilters object</returns>
    public EquipmentFilters GetBotEquipmentSettings(string botEquipmentRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get weapon sight whitelist for a specific bot type
    /// </summary>
    /// <param name="botEquipmentRole">equipment role of bot to look up</param>
    /// <returns>Dictionary of weapon type and their whitelisted scope types</returns>
    public Dictionary<string, List<string>> GetBotWeaponSightWhitelist(string botEquipmentRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get an object that contains equipment and cartridge blacklists for a specified bot type
    /// </summary>
    /// <param name="botRole">Role of the bot we want the blacklist for</param>
    /// <param name="playerLevel">Level of the player</param>
    /// <returns>EquipmentBlacklistDetails object</returns>
    public EquipmentFilterDetails GetBotEquipmentBlacklist(string botRole, double playerLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the whitelist for a specific bot type that's within the players level
    /// </summary>
    /// <param name="botRole">Bot type</param>
    /// <param name="playerLevel">Players level</param>
    /// <returns>EquipmentFilterDetails object</returns>
    protected EquipmentFilterDetails GetBotEquipmentWhitelist(string botRole, int playerLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve item weighting adjustments from bot.json config based on bot level
    /// </summary>
    /// <param name="botRole">Bot type to get adjustments for</param>
    /// <param name="botLevel">Level of bot</param>
    /// <returns>Weighting adjustments for bot items</returns>
    protected WeightingAdjustmentDetails GetBotWeightingAdjustments(string botRole, int botLevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieve item weighting adjustments from bot.json config based on player level
    /// </summary>
    /// <param name="botRole">Bot type to get adjustments for</param>
    /// <param name="playerlevel">Level of bot</param>
    /// <returns>Weighting adjustments for bot items</returns>
    protected WeightingAdjustmentDetails GetBotWeightingAdjustmentsByPlayerLevel(string botRole, int playerlevel)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter bot equipment based on blacklist and whitelist from config/bot.json
    /// Prioritizes whitelist first, if one is found blacklist is ignored
    /// </summary>
    /// <param name="baseBotNode">bot .json file to update</param>
    /// <param name="blacklist">equipment blacklist</param>
    /// <returns>Filtered bot file</returns>
    protected void FilterEquipment(
        BotType baseBotNode,
        EquipmentFilterDetails blacklist,
        EquipmentFilterDetails whitelist)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Filter bot cartridges based on blacklist and whitelist from config/bot.json
    /// Prioritizes whitelist first, if one is found blacklist is ignored
    /// </summary>
    /// <param name="baseBotNode">bot .json file to update</param>
    /// <param name="blacklist">equipment on this list should be excluded from the bot</param>
    /// <param name="whitelist">equipment on this list should be used exclusively</param>
    /// <returns>Filtered bot file</returns>
    protected void FilterCartridges(
        BotType baseBotNode,
        EquipmentFilterDetails blacklist,
        EquipmentFilterDetails whitelist)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add/Edit weighting changes to bot items using values from config/bot.json/equipment
    /// </summary>
    /// <param name="weightingAdjustments">Weighting change to apply to bot</param>
    /// <param name="botItemPool">Bot item dictionary to adjust</param>
    protected void AdjustWeighting(
        AdjustmentDetails weightingAdjustments,
        Dictionary<string, object> botItemPool,
        bool showEditWarnings = true)
    {
        throw new NotImplementedException();
    }
}
