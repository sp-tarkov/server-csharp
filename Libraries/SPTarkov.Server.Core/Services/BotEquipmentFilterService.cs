using SPTarkov.Common.Extensions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class BotEquipmentFilterService(
    ISptLogger<BotEquipmentFilterService> logger,
    BotHelper botHelper,
    ProfileHelper profileHelper,
    ConfigServer configServer
)
{
    protected readonly BotConfig BotConfig = configServer.GetConfig<BotConfig>();
    protected readonly Dictionary<string, EquipmentFilters?> BotEquipmentConfig = configServer.GetConfig<BotConfig>().Equipment;

    /// <summary>
    ///     Filter a bots data to exclude equipment and cartridges defines in the botConfig
    /// </summary>
    /// <param name="sessionId">Players id</param>
    /// <param name="baseBotNode">bots json data to filter</param>
    /// <param name="botGenerationDetails">details on how to generate a bot</param>
    public void FilterBotEquipment(MongoId sessionId, BotType baseBotNode, BotGenerationDetails botGenerationDetails)
    {
        var pmcProfile = profileHelper.GetPmcProfile(sessionId);

        var botRole = botGenerationDetails.IsPmc ? "pmc" : botGenerationDetails.Role;
        var botEquipmentBlacklist = GetBotEquipmentBlacklist(botRole, botGenerationDetails.BotLevel);
        var botEquipmentWhitelist = GetBotEquipmentWhitelist(botRole, botGenerationDetails.BotLevel);
        var botWeightingAdjustments = GetBotWeightingAdjustments(botRole, botGenerationDetails.BotLevel);
        var botWeightingAdjustmentsByPlayerLevel = GetBotWeightingAdjustmentsByPlayerLevel(botRole, pmcProfile?.Info?.Level ?? 1);

        RandomisationDetails? randomisationDetails = null;
        if (BotEquipmentConfig.TryGetValue(botRole.ToLowerInvariant(), out var botEquipmentConfig))
        {
            randomisationDetails = botHelper.GetBotRandomizationDetails(botGenerationDetails.BotLevel, botEquipmentConfig);
        }

        if (botEquipmentBlacklist is not null || botEquipmentWhitelist is not null)
        {
            FilterEquipment(baseBotNode, botEquipmentBlacklist, botEquipmentWhitelist);
            FilterCartridges(baseBotNode, botEquipmentBlacklist, botEquipmentWhitelist);
        }

        if (botWeightingAdjustments is not null)
        {
            AdjustWeighting(botWeightingAdjustments.Equipment, baseBotNode.BotInventory.Equipment);
            AdjustWeighting(botWeightingAdjustments.Ammo, baseBotNode.BotInventory.Ammo);

            // Don't warn when edited item not found, we're editing usec/bear clothing and they don't have each others clothing
            AdjustWeighting(botWeightingAdjustments.Clothing, baseBotNode.BotAppearance, false);
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
    ///     Iterate over the changes passed in and apply them to baseValues parameter
    /// </summary>
    /// <param name="equipmentChanges">Changes to apply</param>
    /// <param name="baseValues">data to update</param>
    protected void AdjustChances(Dictionary<string, double> equipmentChanges, Dictionary<string, double> baseValues)
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
    ///     Iterate over the Generation changes and alter data in baseValues.Generation
    /// </summary>
    /// <param name="generationChanges">Changes to apply</param>
    /// <param name="baseBotGeneration">dictionary to update</param>
    protected void AdjustGenerationChances(Dictionary<string, GenerationData> generationChanges, Generation baseBotGeneration)
    {
        if (generationChanges is null)
        {
            return;
        }

        foreach (var itemKey in generationChanges)
        {
            baseBotGeneration.Items.GetByJsonProperty<GenerationData>(itemKey.Key).Weights = generationChanges
                .GetValueOrDefault(itemKey.Key)
                .Weights;
            baseBotGeneration.Items.GetByJsonProperty<GenerationData>(itemKey.Key).Whitelist = generationChanges
                .GetValueOrDefault(itemKey.Key)
                .Whitelist;
        }
    }

    /// <summary>
    ///     Get equipment settings for bot
    /// </summary>
    /// <param name="botEquipmentRole">equipment role to return</param>
    /// <returns>EquipmentFilters object</returns>
    public EquipmentFilters? GetBotEquipmentSettings(string botEquipmentRole)
    {
        return BotEquipmentConfig.GetValueOrDefault(botEquipmentRole);
    }

    /// <summary>
    ///     Get weapon sight whitelist for a specific bot type
    /// </summary>
    /// <param name="botEquipmentRole">equipment role of bot to look up</param>
    /// <returns>Dictionary of weapon type and their whitelisted scope types</returns>
    public Dictionary<MongoId, HashSet<MongoId>>? GetBotWeaponSightWhitelist(string botEquipmentRole)
    {
        return BotConfig.Equipment.TryGetValue(botEquipmentRole, out var botEquipmentSettings)
            ? botEquipmentSettings.WeaponSightWhitelist
            : null;
    }

    /// <summary>
    ///     Get an object that contains equipment and cartridge blacklists for a specified bot type
    /// </summary>
    /// <param name="botRole">Role of the bot we want the blacklist for</param>
    /// <param name="playerLevel">Level of the player</param>
    /// <returns>EquipmentBlacklistDetails object</returns>
    public EquipmentFilterDetails? GetBotEquipmentBlacklist(string botRole, double playerLevel)
    {
        var blacklistDetailsForBot = BotEquipmentConfig.GetValueOrDefault(botRole, null);

        return (blacklistDetailsForBot?.Blacklist ?? []).FirstOrDefault(equipmentFilter =>
            playerLevel >= equipmentFilter.LevelRange.Min && playerLevel <= equipmentFilter.LevelRange.Max
        );
    }

    /// <summary>
    ///     Get the whitelist for a specific bot type that's within the players level
    /// </summary>
    /// <param name="botRole">Bot type</param>
    /// <param name="playerLevel">Players level</param>
    /// <returns>EquipmentFilterDetails object</returns>
    protected EquipmentFilterDetails? GetBotEquipmentWhitelist(string botRole, int playerLevel)
    {
        var whitelistDetailsForBot = BotEquipmentConfig.GetValueOrDefault(botRole, null);

        return (whitelistDetailsForBot?.Whitelist ?? []).FirstOrDefault(equipmentFilter =>
            playerLevel >= equipmentFilter.LevelRange.Min && playerLevel <= equipmentFilter.LevelRange.Max
        );
    }

    /// <summary>
    ///     Retrieve item weighting adjustments from bot.json config based on bot level
    /// </summary>
    /// <param name="botRole">Bot type to get adjustments for</param>
    /// <param name="botLevel">Level of bot</param>
    /// <returns>Weighting adjustments for bot items</returns>
    protected WeightingAdjustmentDetails? GetBotWeightingAdjustments(string botRole, int botLevel)
    {
        var weightingDetailsForBot = BotEquipmentConfig.GetValueOrDefault(botRole, null);

        return (weightingDetailsForBot?.WeightingAdjustmentsByBotLevel ?? []).FirstOrDefault(x =>
            botLevel >= x.LevelRange.Min && botLevel <= x.LevelRange.Max
        );
    }

    /// <summary>
    ///     Retrieve item weighting adjustments from bot.json config based on player level
    /// </summary>
    /// <param name="botRole">Bot type to get adjustments for</param>
    /// <param name="playerLevel">Level of bot</param>
    /// <returns>Weighting adjustments for bot items</returns>
    protected WeightingAdjustmentDetails? GetBotWeightingAdjustmentsByPlayerLevel(string botRole, int playerLevel)
    {
        var weightingDetailsForBot = BotEquipmentConfig.GetValueOrDefault(botRole, null);

        return (weightingDetailsForBot?.WeightingAdjustmentsByBotLevel ?? []).FirstOrDefault(x =>
            playerLevel >= x.LevelRange.Min && playerLevel <= x.LevelRange.Max
        );
    }

    /// <summary>
    ///     Filter bot equipment based on blacklist and whitelist from config/bot.json
    ///     Prioritizes whitelist first, if one is found blacklist is ignored
    /// </summary>
    /// <param name="baseBotNode">bot .json file to update</param>
    /// <param name="blacklist">Equipment blacklist</param>
    /// <param name="whitelist">Equipment whitelist</param>
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
                baseBotNode.BotInventory.Equipment[equipmentSlotKey.Key] = [];
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
    ///     Filter bot cartridges based on blacklist and whitelist from config/bot.json
    ///     Prioritizes whitelist first, if one is found blacklist is ignored
    /// </summary>
    /// <param name="baseBotNode">bot .json file to update</param>
    /// <param name="blacklist">equipment on this list should be excluded from the bot</param>
    /// <param name="whitelist">equipment on this list should be used exclusively</param>
    /// <returns>Filtered bot file</returns>
    protected void FilterCartridges(BotType baseBotNode, EquipmentFilterDetails? blacklist, EquipmentFilterDetails? whitelist)
    {
        if (whitelist is not null)
        {
            // Loop over each caliber + cartridges of that type
            foreach (var (caliber, cartridges) in baseBotNode.BotInventory.Ammo)
            {
                if (!whitelist.Cartridge.TryGetValue(caliber, out var matchingWhitelist))
                // No cartridge whitelist, move to next cartridge
                {
                    continue;
                }

                // Loop over each cartridge + weight
                // Clear all cartridges ready for whitelist to be added
                foreach (var ammoKvP in cartridges)
                // Cartridge not on whitelist
                {
                    if (!matchingWhitelist.Contains(ammoKvP.Key))
                    // Remove
                    {
                        cartridges.Remove(ammoKvP.Key);
                    }
                }
            }

            return;
        }

        if (blacklist is not null)
        {
            foreach (var (caliber, cartridgesAndWeights) in baseBotNode.BotInventory.Ammo)
            {
                // Skip cartridge slot if blacklist doesn't exist / is empty
                blacklist.Cartridge.TryGetValue(caliber, out var cartridgeCaliberBlacklist);
                if (cartridgeCaliberBlacklist is null || cartridgeCaliberBlacklist.Count == 0)
                {
                    continue;
                }

                // Filter cartridge slot items to just items not in blacklist
                foreach (
                    var blacklistedTpl in cartridgeCaliberBlacklist.Where(blacklistedTpl =>
                        cartridgesAndWeights.ContainsKey(blacklistedTpl)
                    )
                )
                {
                    cartridgesAndWeights.Remove(blacklistedTpl);
                }
            }
        }
    }

    /// <summary>
    ///     Add/Edit weighting changes to bot items using values from config/bot.json/equipment
    /// </summary>
    /// <param name="weightingAdjustments">Weighting change to apply to bot</param>
    /// <param name="botItemPool">Bot item dictionary to adjust</param>
    /// <param name="showEditWarnings">OPTIONAL - show warnings when editing existing value</param>
    protected void AdjustWeighting(
        AdjustmentDetails? weightingAdjustments,
        Dictionary<EquipmentSlots, Dictionary<MongoId, double>> botItemPool,
        bool showEditWarnings = true
    )
    {
        // TODO: bad typing by key with method below due to, EquipmentSlots
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
                // Only make change if item exists as we're editing, not adding
                {
                    if (locationToUpdate[itemToEditKvP.Key] != null || locationToUpdate[itemToEditKvP.Key] == 0)
                    {
                        locationToUpdate[itemToEditKvP.Key] = itemToEditKvP.Value;
                    }
                    else
                    {
                        if (showEditWarnings)
                        {
                            if (logger.IsLogEnabled(LogLevel.Debug))
                            {
                                logger.Debug($"Tried to edit a non - existent item for slot: {poolAdjustmentKvP} {itemToEditKvP}");
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Add/Edit weighting changes to bot items using values from config/bot.json/equipment
    /// </summary>
    /// <param name="weightingAdjustments">Weighting change to apply to bot</param>
    /// <param name="botItemPool">Bot item dictionary to adjust</param>
    /// <param name="showEditWarnings"></param>
    protected void AdjustWeighting(
        AdjustmentDetails? weightingAdjustments,
        Dictionary<string, Dictionary<MongoId, double>> botItemPool,
        bool showEditWarnings = true
    )
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
                // Only make change if item exists as we're editing, not adding
                {
                    if (locationToUpdate.ContainsKey(itemToEditKvP.Key) || locationToUpdate[itemToEditKvP.Key] == 0)
                    {
                        locationToUpdate[itemToEditKvP.Key] = itemToEditKvP.Value;
                    }
                    else
                    {
                        if (showEditWarnings)
                        {
                            if (logger.IsLogEnabled(LogLevel.Debug))
                            {
                                logger.Debug($"Tried to edit a non - existent item for slot: {poolAdjustmentKvP} {itemToEditKvP}");
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Add/Edit weighting changes to bot items using values from config/bot.json/equipment
    /// </summary>
    /// <param name="weightingAdjustments">Weighting change to apply to bot</param>
    /// <param name="botItemPool">Bot item dictionary to adjust</param>
    /// <param name="showEditWarnings">When item being adjusted cannot be found at source, show warning message</param>
    protected void AdjustWeighting(AdjustmentDetails? weightingAdjustments, Appearance botItemPool, bool showEditWarnings = true)
    {
        if (weightingAdjustments is null)
        {
            return;
        }

        if (weightingAdjustments.Add?.Count > 0)
        {
            foreach (var poolAdjustmentKvP in weightingAdjustments.Add)
            {
                var locationToUpdate = botItemPool.GetByJsonProperty<Dictionary<MongoId, double>>(poolAdjustmentKvP.Key);
                if (locationToUpdate is null)
                {
                    continue;
                }

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
                var locationToUpdate = botItemPool.GetByJsonProperty<Dictionary<MongoId, double>>(poolAdjustmentKvP.Key);
                if (locationToUpdate is null)
                {
                    continue;
                }

                foreach (var itemToEditKvP in poolAdjustmentKvP.Value)
                // Only make change if item exists as we're editing, not adding
                {
                    if (locationToUpdate.ContainsKey(itemToEditKvP.Key))
                    {
                        locationToUpdate[itemToEditKvP.Key] = itemToEditKvP.Value;

                        continue;
                    }

                    // We tried to add an item flagged as edit only
                    if (showEditWarnings)
                    {
                        if (logger.IsLogEnabled(LogLevel.Debug))
                        {
                            logger.Debug($"Tried to edit a non - existent item for slot: {poolAdjustmentKvP} {itemToEditKvP}");
                        }
                    }
                }
            }
        }
    }
}
