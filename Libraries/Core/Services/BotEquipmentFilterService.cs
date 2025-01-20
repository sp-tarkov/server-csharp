using SptCommon.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Bots;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using SptCommon.Extensions;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotEquipmentFilterService
{
    protected ISptLogger<BotEquipmentFilterService> _logger;
    protected ProfileHelper _profileHelper;
    protected BotHelper _botHelper;

    protected BotConfig _botConfig;
    protected Dictionary<string, EquipmentFilters?> _botEquipmentConfig;

    public BotEquipmentFilterService(
        ISptLogger<BotEquipmentFilterService> logger,
        BotHelper botHelper,
        ProfileHelper profileHelper,
        ConfigServer configServer)
    {
        _logger = logger;
        _profileHelper = profileHelper;
        _botHelper = botHelper;

        _botConfig = configServer.GetConfig<BotConfig>();
        _botEquipmentConfig = _botConfig.Equipment!;
    }

    /// <summary>
    /// Filter a bots data to exclude equipment and cartridges defines in the botConfig
    /// </summary>
    /// <param name="sessionId">Players id</param>
    /// <param name="baseBotNode">bots json data to filter</param>
    /// <param name="botLevel">Level of the bot</param>
    /// <param name="botGenerationDetails">details on how to generate a bot</param>
    public void FilterBotEquipment(string sessionId, BotType baseBotNode, int botLevel, BotGenerationDetails botGenerationDetails)
    {
        var pmcProfile = _profileHelper.GetPmcProfile(sessionId);

        var botRole = botGenerationDetails.IsPmc ?? false ? "pmc" : botGenerationDetails.Role;
        var botEquipmentBlacklist = GetBotEquipmentBlacklist(botRole, botLevel);
        var botEquipmentWhitelist = GetBotEquipmentWhitelist(botRole, botLevel);
        var botWeightingAdjustments = GetBotWeightingAdjustments(botRole, botLevel);
        var botWeightingAdjustmentsByPlayerLevel = GetBotWeightingAdjustmentsByPlayerLevel(
            botRole,
            pmcProfile.Info.Level ?? 0
        );

        var botEquipConfig = _botEquipmentConfig[botRole];
        var randomisationDetails = _botHelper.GetBotRandomizationDetails(botLevel, botEquipConfig);

        if (botEquipmentBlacklist is not null || botEquipmentWhitelist is not null)
        {
            FilterEquipment(baseBotNode, botEquipmentBlacklist, botEquipmentWhitelist);
            FilterCartridges(baseBotNode, botEquipmentBlacklist, botEquipmentWhitelist);
        }

        if (botWeightingAdjustments is not null)
        {
            AdjustWeighting(botWeightingAdjustments.Equipment, baseBotNode.BotInventory.Equipment);
            AdjustWeighting(botWeightingAdjustments.Ammo, baseBotNode.BotInventory.Ammo);
            // Dont warn when edited item not found, we're editing usec/bear clothing and they dont have each others clothing
            AdjustWeighting(botWeightingAdjustments?.Clothing, baseBotNode.BotAppearance, false);
        }

        if (botWeightingAdjustmentsByPlayerLevel is not null)
        {
            AdjustWeighting(botWeightingAdjustmentsByPlayerLevel.Equipment, baseBotNode.BotInventory.Equipment);
            AdjustWeighting(botWeightingAdjustmentsByPlayerLevel.Ammo, baseBotNode.BotInventory.Ammo);
        }

        if (randomisationDetails is not null)
        {
            AdjustChances(randomisationDetails.Equipment, baseBotNode.BotChances.EquipmentChances);
            AdjustChances(randomisationDetails.WeaponMods, baseBotNode.BotChances.WeaponModsChances);
            AdjustChances(randomisationDetails.EquipmentMods, baseBotNode.BotChances.EquipmentModsChances);
            AdjustGenerationChances(randomisationDetails.Generation, baseBotNode.BotGeneration);
        }
    }

    /// <summary>
    /// Iterate over the changes passed in and apply them to baseValues parameter
    /// </summary>
    /// <param name="equipmentChanges">Changes to apply</param>
    /// <param name="baseValues">data to update</param>
    protected void AdjustChances(
        Dictionary<string, double> equipmentChanges,
        Dictionary<string, double> baseValues)
    {
        if (equipmentChanges is null)
        {
            return;
        }

        foreach (var itemKey in equipmentChanges)
        {
            baseValues[itemKey.Key] = equipmentChanges[itemKey.Key];
        }
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
        if (generationChanges is null)
        {
            return;
        }

        foreach (var itemKey in generationChanges)
        {
            baseBotGeneration.Items.Get<GenerationData>(itemKey.Key).Weights = generationChanges.Get<GenerationData>(itemKey.Key).Weights;
            baseBotGeneration.Items.Get<GenerationData>(itemKey.Key).Whitelist = generationChanges.Get<GenerationData>(itemKey.Key).Whitelist;
        }
    }

    /// <summary>
    /// Get equipment settings for bot
    /// </summary>
    /// <param name="botEquipmentRole">equipment role to return</param>
    /// <returns>EquipmentFilters object</returns>
    public EquipmentFilters GetBotEquipmentSettings(string botEquipmentRole)
    {
        return _botEquipmentConfig[botEquipmentRole];
    }

    /// <summary>
    /// Get weapon sight whitelist for a specific bot type
    /// </summary>
    /// <param name="botEquipmentRole">equipment role of bot to look up</param>
    /// <returns>Dictionary of weapon type and their whitelisted scope types</returns>
    public Dictionary<string, List<string>> GetBotWeaponSightWhitelist(string botEquipmentRole)
    {
        var botEquipmentSettings = _botConfig.Equipment[botEquipmentRole];

        if (botEquipmentSettings is null)
        {
            return null;
        }

        return botEquipmentSettings.WeaponSightWhitelist;
    }

    /// <summary>
    /// Get an object that contains equipment and cartridge blacklists for a specified bot type
    /// </summary>
    /// <param name="botRole">Role of the bot we want the blacklist for</param>
    /// <param name="playerLevel">Level of the player</param>
    /// <returns>EquipmentBlacklistDetails object</returns>
    public EquipmentFilterDetails? GetBotEquipmentBlacklist(string botRole, double playerLevel)
    {
        var blacklistDetailsForBot = _botEquipmentConfig.GetValueOrDefault(botRole, null);

        return (blacklistDetailsForBot?.Blacklist ?? []).FirstOrDefault(
            equipmentFilter => playerLevel >= equipmentFilter.LevelRange.Min && playerLevel <= equipmentFilter.LevelRange.Max
        );
    }

    /// <summary>
    /// Get the whitelist for a specific bot type that's within the players level
    /// </summary>
    /// <param name="botRole">Bot type</param>
    /// <param name="playerLevel">Players level</param>
    /// <returns>EquipmentFilterDetails object</returns>
    protected EquipmentFilterDetails? GetBotEquipmentWhitelist(string botRole, int playerLevel)
    {
        var whitelistDetailsForBot = _botEquipmentConfig.GetValueOrDefault(botRole, null);

        return (whitelistDetailsForBot?.Whitelist ?? []).FirstOrDefault(
            equipmentFilter => playerLevel >= equipmentFilter.LevelRange.Min && playerLevel <= equipmentFilter.LevelRange.Max
        );
    }

    /// <summary>
    /// Retrieve item weighting adjustments from bot.json config based on bot level
    /// </summary>
    /// <param name="botRole">Bot type to get adjustments for</param>
    /// <param name="botLevel">Level of bot</param>
    /// <returns>Weighting adjustments for bot items</returns>
    protected WeightingAdjustmentDetails? GetBotWeightingAdjustments(string botRole, int botLevel)
    {
        var weightingDetailsForBot = _botEquipmentConfig.GetValueOrDefault(botRole, null);

        return (weightingDetailsForBot?.WeightingAdjustmentsByBotLevel ?? [] ).FirstOrDefault(
            x => botLevel >= x.LevelRange.Min && botLevel <= x.LevelRange.Max
        );
    }

    /// <summary>
    /// Retrieve item weighting adjustments from bot.json config based on player level
    /// </summary>
    /// <param name="botRole">Bot type to get adjustments for</param>
    /// <param name="playerlevel">Level of bot</param>
    /// <returns>Weighting adjustments for bot items</returns>
    protected WeightingAdjustmentDetails? GetBotWeightingAdjustmentsByPlayerLevel(string botRole, int playerlevel)
    {
        var weightingDetailsForBot = _botEquipmentConfig.GetValueOrDefault(botRole, null);

        return (weightingDetailsForBot?.WeightingAdjustmentsByBotLevel ?? []).FirstOrDefault(
            x => playerlevel >= x.LevelRange.Min && playerlevel <= x.LevelRange.Max
        );
    }

    /// <summary>
    /// Filter bot equipment based on blacklist and whitelist from config/bot.json
    /// Prioritizes whitelist first, if one is found blacklist is ignored
    /// </summary>
    /// <param name="baseBotNode">bot .json file to update</param>
    /// <param name="blacklist">equipment blacklist</param>
    /// <returns>Filtered bot file</returns>
    protected void FilterEquipment(BotType baseBotNode, EquipmentFilterDetails? blacklist, EquipmentFilterDetails? whitelist)
    {
        if (whitelist is not null)
        {
            foreach (var equipmentSlotKey in baseBotNode.BotInventory.Equipment)
            {
                var botEquipment = baseBotNode.BotInventory.Equipment[equipmentSlotKey.Key];

                // Skip equipment slot if whitelist doesn't exist / is empty
                var whitelistEquipmentForSlot = whitelist.Equipment[equipmentSlotKey.Key.ToString()];
                if (whitelistEquipmentForSlot is null || whitelistEquipmentForSlot.Count == 0)
                {
                    continue;
                }

                // Filter equipment slot items to just items in whitelist
                baseBotNode.BotInventory.Equipment[equipmentSlotKey.Key] = new Dictionary<string, double>();
                foreach (var dict in botEquipment)
                {
                    if (whitelistEquipmentForSlot.Contains(dict.Key))
                    {
                        baseBotNode.BotInventory.Equipment[equipmentSlotKey.Key][dict.Key] = botEquipment[dict.Key];
                    }
                }
            }

            return;
        }

        if (blacklist is not null)
        {
            foreach (var equipmentSlotKvP in baseBotNode.BotInventory.Equipment)
            {
                var botEquipment = baseBotNode.BotInventory.Equipment[equipmentSlotKvP.Key];

                // Skip equipment slot if blacklist doesn't exist / is empty
                if (!blacklist.Equipment.TryGetValue(equipmentSlotKvP.Key.ToString(), out var equipmentSlotBlacklist))
                {
                    continue;
                }

                // Filter equipment slot items to just items not in blacklist
                equipmentSlotKvP.Value.Clear();
                foreach (var dict in botEquipment)
                {
                    if (!equipmentSlotBlacklist.Contains(dict.Key))
                    {
                        equipmentSlotKvP.Value[dict.Key] = botEquipment[dict.Key];
                    }
                }
            }
        }
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
        EquipmentFilterDetails? blacklist,
        EquipmentFilterDetails? whitelist)
    {
        if (whitelist is not null)
        {
            // Loop over each caliber + cartridges of that type
            foreach ( var (caliber, cartridges) in baseBotNode.BotInventory.Ammo) {

                if(!whitelist.Cartridge.TryGetValue(caliber, out var matchingWhitelist))
                {
                    // No cartridge whitelist, move to next cartridge
                    continue;
                }

                // Loop over each cartridge + weight
                // Clear all cartridges ready for whitelist to be added
                foreach (var ammoKvP in cartridges)
                {
                    // Cartridge not on whitelist
                    if (!matchingWhitelist.Contains(ammoKvP.Key))
                    {
                        // Remove
                        cartridges.Remove(ammoKvP.Key);
                    }
                }
            }

            return;
        }

        if (blacklist is not null)
        {
            foreach ( var ammoCaliberKvP  in baseBotNode.BotInventory.Ammo) {
                var botAmmo  = baseBotNode.BotInventory.Ammo[ammoCaliberKvP.Key];

                // Skip cartridge slot if blacklist doesn't exist / is empty
                blacklist.Cartridge.TryGetValue(ammoCaliberKvP.Key, out List<string> cartridgeCaliberBlacklist);
                if (cartridgeCaliberBlacklist is null || cartridgeCaliberBlacklist.Count == 0)
                {
                    continue;
                }

                // Filter cartridge slot items to just items not in blacklist
                foreach (var blacklistedTpl in cartridgeCaliberBlacklist
                             .Where(blacklistedTpl => ammoCaliberKvP.Value.ContainsKey(blacklistedTpl)))
                {
                    ammoCaliberKvP.Value.Remove(blacklistedTpl);
                }
            }
        }
    }

    /// <summary>
    /// Add/Edit weighting changes to bot items using values from config/bot.json/equipment
    /// </summary>
    /// <param name="weightingAdjustments">Weighting change to apply to bot</param>
    /// <param name="botItemPool">Bot item dictionary to adjust</param>
    protected void AdjustWeighting(
        AdjustmentDetails weightingAdjustments,
        Dictionary<EquipmentSlots, Dictionary<string, double>> botItemPool,
        bool showEditWarnings = true)
    {
        //TODO, bad typing by key with method below due to, EquipmentSlots
        if (weightingAdjustments is null)
        {
            return;
        }

        if (weightingAdjustments.Add?.Count > 0)
        {
            foreach (var poolAdjustmentKvP in weightingAdjustments.Add)
            {
                var locationToUpdate = botItemPool[Enum.Parse<EquipmentSlots>(poolAdjustmentKvP.Key)];
                foreach (var itemToAddKvP in poolAdjustmentKvP.Value)
                {
                    locationToUpdate[itemToAddKvP.Key] = itemToAddKvP.Value;
                }
            }
        }

        if (weightingAdjustments.Edit?.Count > 0)
        {
            foreach (var poolAdjustmentKvP in weightingAdjustments.Edit)
            {
                var locationToUpdate = botItemPool[Enum.Parse<EquipmentSlots>(poolAdjustmentKvP.Key)];
                foreach (var itemToEditKvP in poolAdjustmentKvP.Value)
                {
                    // Only make change if item exists as we're editing, not adding
                    if (locationToUpdate[itemToEditKvP.Key] != null || locationToUpdate[itemToEditKvP.Key] == 0)
                    {
                        locationToUpdate[itemToEditKvP.Key] = itemToEditKvP.Value;
                    }
                    else
                    {
                        if (showEditWarnings)
                        {
                            _logger.Debug($"Tried to edit a non - existent item for slot: {poolAdjustmentKvP} {itemToEditKvP}");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Add/Edit weighting changes to bot items using values from config/bot.json/equipment
    /// </summary>
    /// <param name="weightingAdjustments">Weighting change to apply to bot</param>
    /// <param name="botItemPool">Bot item dictionary to adjust</param>
    /// <param name="showEditWarnings"></param>
    protected void AdjustWeighting(
        AdjustmentDetails? weightingAdjustments,
        Dictionary<string, Dictionary<string, double>> botItemPool,
        bool showEditWarnings = true)
    {
        if (weightingAdjustments is null)
        {
            return;
        }

        if (weightingAdjustments.Add?.Count > 0)
        {
            foreach (var poolAdjustmentKvP in weightingAdjustments.Add)
            {
                var locationToUpdate = botItemPool[poolAdjustmentKvP.Key];
                foreach (var itemToAddKvP in poolAdjustmentKvP.Value)
                {
                    locationToUpdate[itemToAddKvP.Key] = itemToAddKvP.Value;
                }
            }
        }

        if (weightingAdjustments.Edit?.Count > 0)
        {
            foreach (var poolAdjustmentKvP in weightingAdjustments.Edit)
            {
                var locationToUpdate = botItemPool[poolAdjustmentKvP.Key];
                foreach (var itemToEditKvP in poolAdjustmentKvP.Value)
                {
                    // Only make change if item exists as we're editing, not adding
                    if (locationToUpdate[itemToEditKvP.Key] != null || locationToUpdate[itemToEditKvP.Key] == 0)
                    {
                        locationToUpdate[itemToEditKvP.Key] = itemToEditKvP.Value;
                    }
                    else
                    {
                        if (showEditWarnings)
                        {
                            _logger.Debug($"Tried to edit a non - existent item for slot: {poolAdjustmentKvP} {itemToEditKvP}");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Add/Edit weighting changes to bot items using values from config/bot.json/equipment
    /// </summary>
    /// <param name="weightingAdjustments">Weighting change to apply to bot</param>
    /// <param name="botItemPool">Bot item dictionary to adjust</param>
    protected void AdjustWeighting(
        AdjustmentDetails weightingAdjustments,
        Appearance botItemPool,
        bool showEditWarnings = true)
    {
        if (weightingAdjustments is null)
        {
            return;
        }

        if (weightingAdjustments.Add?.Count > 0)
        {
            foreach (var poolAdjustmentKvP in weightingAdjustments.Add)
            {
                var locationToUpdate = botItemPool[poolAdjustmentKvP.Key];
                foreach (var itemToAddKvP in poolAdjustmentKvP.Value)
                {
                    locationToUpdate[itemToAddKvP.Key] = itemToAddKvP.Value;
                }
            }
        }

        if (weightingAdjustments.Edit?.Count > 0)
        {
            foreach (var poolAdjustmentKvP in weightingAdjustments.Edit)
            {
                var locationToUpdate = botItemPool[poolAdjustmentKvP.Key];
                foreach (var itemToEditKvP in poolAdjustmentKvP.Value)
                {
                    // Only make change if item exists as we're editing, not adding
                    if (locationToUpdate[itemToEditKvP.Key] != null || locationToUpdate[itemToEditKvP.Key] == 0)
                    {
                        locationToUpdate[itemToEditKvP.Key] = itemToEditKvP.Value;
                    }
                    else
                    {
                        if (showEditWarnings)
                        {
                            _logger.Debug($"Tried to edit a non - existent item for slot: {poolAdjustmentKvP} {itemToEditKvP}");
                        }
                    }
                }
            }
        }
    }
}
