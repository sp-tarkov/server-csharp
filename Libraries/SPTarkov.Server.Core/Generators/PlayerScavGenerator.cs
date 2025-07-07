using System.Globalization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Notes;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Json;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class PlayerScavGenerator(
    ISptLogger<PlayerScavGenerator> logger,
    RandomUtil randomUtil,
    DatabaseService databaseService,
    ItemHelper itemHelper,
    BotGeneratorHelper botGeneratorHelper,
    SaveServer saveServer,
    ProfileHelper profileHelper,
    BotHelper botHelper,
    FenceService fenceService,
    BotLootCacheService botLootCacheService,
    ServerLocalisationService serverLocalisationService,
    BotGenerator botGenerator,
    ConfigServer configServer,
    ICloner cloner,
    TimeUtil timeUtil
)
{
    protected readonly PlayerScavConfig _playerScavConfig =
        configServer.GetConfig<PlayerScavConfig>();

    /// <summary>
    ///     Update a player profile to include a new player scav profile
    /// </summary>
    /// <param name="sessionID">session id to specify what profile is updated</param>
    /// <returns>profile object</returns>
    public PmcData Generate(MongoId sessionID)
    {
        // get karma level from profile
        var profile = saveServer.GetProfile(sessionID);
        var profileCharactersClone = cloner.Clone(profile.CharacterData);
        var pmcDataClone = cloner.Clone(profileCharactersClone.PmcData);
        var existingScavDataClone = cloner.Clone(profileCharactersClone.ScavData);

        var scavKarmaLevel = pmcDataClone.GetScavKarmaLevel();

        // use karma level to get correct karmaSettings
        if (
            !_playerScavConfig.KarmaLevel.TryGetValue(
                scavKarmaLevel.ToString(CultureInfo.InvariantCulture),
                out var playerScavKarmaSettings
            )
        )
        {
            logger.Error(
                serverLocalisationService.GetText("scav-missing_karma_settings", scavKarmaLevel)
            );
        }

        if (logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug($"Generated player scav load out with karma level: {scavKarmaLevel}");
        }

        // Edit baseBotNode values
        var baseBotNode = ConstructBotBaseTemplate(playerScavKarmaSettings.BotTypeForLoot);

        AdjustBotTemplateWithKarmaSpecificSettings(playerScavKarmaSettings, baseBotNode);

        var scavData = botGenerator.GeneratePlayerScav(
            sessionID,
            playerScavKarmaSettings.BotTypeForLoot.ToLowerInvariant(),
            "easy",
            baseBotNode,
            pmcDataClone
        );

        // Remove cached bot data after scav was generated
        botLootCacheService.ClearCache();

        // Add scav metadata
        scavData.Savage = null;
        scavData.Aid = pmcDataClone.Aid;
        scavData.TradersInfo = pmcDataClone.TradersInfo;
        scavData.Info.Settings = new BotInfoSettings();
        scavData.Info.Bans = [];
        scavData.Info.RegistrationDate = pmcDataClone.Info.RegistrationDate;
        scavData.Info.GameVersion = pmcDataClone.Info.GameVersion;
        scavData.Info.MemberCategory = MemberCategory.UniqueId;
        scavData.Info.LockedMoveCommands = true;
        scavData.Info.MainProfileNickname = pmcDataClone.Info.Nickname;
        scavData.RagfairInfo = pmcDataClone.RagfairInfo;
        scavData.UnlockedInfo = pmcDataClone.UnlockedInfo;

        // Persist previous scav data into new scav
        scavData.Id = existingScavDataClone.Id ?? pmcDataClone.Savage;
        scavData.SessionId = existingScavDataClone.SessionId ?? pmcDataClone.SessionId;
        scavData.Skills = existingScavDataClone.GetSkillsOrDefault();
        scavData.Stats = GetScavStats(existingScavDataClone);
        scavData.Info.Level = GetScavLevel(existingScavDataClone);
        scavData.Info.Experience = GetScavExperience(existingScavDataClone);
        scavData.Quests = existingScavDataClone.Quests ?? [];
        scavData.TaskConditionCounters =
            existingScavDataClone.TaskConditionCounters
            ?? new Dictionary<MongoId, TaskConditionCounter>();
        scavData.Notes = existingScavDataClone.Notes ?? new Notes { DataNotes = new List<Note>() };
        scavData.WishList =
            existingScavDataClone.WishList
            ?? new DictionaryOrList<MongoId, int>(new Dictionary<MongoId, int>(), []);
        scavData.Encyclopedia = pmcDataClone.Encyclopedia ?? new Dictionary<MongoId, bool>();

        // Add additional items to player scav as loot
        AddAdditionalLootToPlayerScavContainers(
            playerScavKarmaSettings.LootItemsToAddChancePercent,
            scavData,
            [EquipmentSlots.TacticalVest, EquipmentSlots.Pockets, EquipmentSlots.Backpack]
        );

        // Remove secure container
        scavData = profileHelper.RemoveSecureContainer(scavData);

        // set cooldown timer
        scavData = SetScavCooldownTimer(scavData, pmcDataClone);

        // add scav to profile
        saveServer.GetProfile(sessionID).CharacterData.ScavData = scavData;

        return scavData;
    }

    /// <summary>
    ///     Add items picked from `playerscav.lootItemsToAddChancePercent`
    /// </summary>
    /// <param name="possibleItemsToAdd">dict of tpl + % chance to be added</param>
    /// <param name="scavData"></param>
    /// <param name="containersToAddTo">Possible slotIds to add loot to</param>
    protected void AddAdditionalLootToPlayerScavContainers(
        Dictionary<MongoId, double> possibleItemsToAdd,
        BotBase scavData,
        HashSet<EquipmentSlots> containersToAddTo
    )
    {
        foreach (var tpl in possibleItemsToAdd)
        {
            var shouldAdd = randomUtil.GetChance100(tpl.Value);
            if (!shouldAdd)
            {
                continue;
            }

            var itemResult = itemHelper.GetItem(tpl.Key);
            if (!itemResult.Key)
            {
                logger.Warning(
                    serverLocalisationService.GetText("scav-unable_to_add_item_to_player_scav", tpl)
                );
                continue;
            }

            var itemTemplate = itemResult.Value;
            var itemsToAdd = new List<Item>
            {
                new()
                {
                    Id = new MongoId(),
                    Template = itemTemplate.Id,
                    Upd = botGeneratorHelper.GenerateExtraPropertiesForItem(itemTemplate),
                },
            };

            var result = botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                containersToAddTo,
                itemsToAdd[0].Id,
                itemTemplate.Id,
                itemsToAdd,
                scavData.Inventory
            );

            if (result != ItemAddedResult.SUCCESS)
            {
                if (logger.IsLogEnabled(LogLevel.Debug))
                {
                    logger.Debug($"Unable to add keycard to bot. Reason: {result.ToString()}");
                }
            }
        }
    }

    /// <summary>
    ///     Get a baseBot template
    ///     If the parameter doesnt match "assault", take parts from the loot type and apply to the return bot template
    /// </summary>
    /// <param name="botTypeForLoot">bot type to use for inventory/chances</param>
    /// <returns>IBotType object</returns>
    protected BotType ConstructBotBaseTemplate(string botTypeForLoot)
    {
        const string baseScavType = "assault";
        var asssaultBase = cloner.Clone(botHelper.GetBotTemplate(baseScavType));

        // Loot bot is same as base bot, return base with no modification
        if (botTypeForLoot == baseScavType)
        {
            return asssaultBase;
        }

        var lootBase = cloner.Clone(botHelper.GetBotTemplate(botTypeForLoot));
        asssaultBase.BotInventory = lootBase.BotInventory;
        asssaultBase.BotChances = lootBase.BotChances;
        asssaultBase.BotGeneration = lootBase.BotGeneration;

        return asssaultBase;
    }

    /// <summary>
    ///     Adjust equipment/mod/item generation values based on scav karma levels
    /// </summary>
    /// <param name="karmaSettings">Values to modify the bot template with</param>
    /// <param name="baseBotNode">bot template to modify according to karma level settings</param>
    protected void AdjustBotTemplateWithKarmaSpecificSettings(
        KarmaLevel karmaSettings,
        BotType baseBotNode
    )
    {
        // Adjust equipment chance values
        AdjustEquipmentWeights(
            karmaSettings.Modifiers.Equipment,
            baseBotNode.BotChances.EquipmentChances
        );

        // Adjust mod chance values
        AdjustWeaponModWeights(
            karmaSettings.Modifiers.Mod,
            baseBotNode.BotChances.WeaponModsChances
        );

        // Adjust item spawn quantity values
        AdjustItemWeights(karmaSettings.ItemLimits, baseBotNode.BotGeneration.Items);

        // Blacklist equipment, keyed by equipment slot
        BlacklistEquipment(karmaSettings, baseBotNode);
    }

    protected static void AdjustEquipmentWeights(
        Dictionary<string, double> equipmentChangesToApply,
        Dictionary<string, double> botEquipmentChances
    )
    {
        foreach (var (equipmentSlot, chanceToAdd) in equipmentChangesToApply)
        {
            // Adjustment value zero, nothing to do
            if (chanceToAdd == 0)
            {
                continue;
            }

            // Try and add new key with value
            if (!botEquipmentChances.TryAdd(equipmentSlot, chanceToAdd))
            {
                // Unable to add new, update existing
                botEquipmentChances[equipmentSlot] += chanceToAdd;
            }
        }
    }

    /// <summary>
    /// Get a bots item type weightings based on the desired key
    /// </summary>
    /// <param name="key">e.g. "healing" / "looseLoot"</param>
    /// <param name="botItemWeights"></param>
    /// <returns>GenerationData</returns>
    protected GenerationData? GetKarmaLimitValuesByKey(
        string key,
        GenerationWeightingItems botItemWeights
    )
    {
        switch (key)
        {
            case "healing":
                return botItemWeights.Healing;
            case "drugs":
                return botItemWeights.Drugs;
            case "stims":
                return botItemWeights.Stims;
            case "looseLoot":
                return botItemWeights.LooseLoot;
            case "magazines":
                return botItemWeights.Magazines;
            case "grenades":
                return botItemWeights.Grenades;
            case "backpackLoot":
                return botItemWeights.BackpackLoot;
            case "drink":
                return botItemWeights.Drink;
            case "currency":
                return botItemWeights.Currency;
            case "pocketLoot":
                return botItemWeights.PocketLoot;
            case "vestLoot":
                return botItemWeights.VestLoot;
            case "specialItems":
                return botItemWeights.SpecialItems;
            default:
                logger.Error($"Subtype: {key} not found");
                return null;
        }
    }

    protected static void AdjustWeaponModWeights(
        Dictionary<string, double> modChangesToApply,
        Dictionary<string, double> weaponModChances
    )
    {
        foreach (var (modSlot, weight) in modChangesToApply)
        {
            // Adjustment value zero, nothing to do
            if (weight == 0)
            {
                continue;
            }

            if (modChangesToApply.TryGetValue(modSlot, out var value))
            {
                weaponModChances.TryAdd(modSlot, 0);
                weaponModChances[modSlot] += value;
            }
        }
    }

    protected void AdjustItemWeights(
        Dictionary<string, GenerationData> karmaSettingsItemLimits,
        GenerationWeightingItems? botGenerationItems
    )
    {
        foreach (var (subType, limitData) in karmaSettingsItemLimits)
        {
            var playerValues = GetKarmaLimitValuesByKey(subType, botGenerationItems);
            if (playerValues is null)
            {
                continue;
            }

            if (limitData.Weights is not null)
            {
                playerValues.Weights = limitData.Weights;
            }

            if (limitData.Whitelist is not null)
            {
                playerValues.Whitelist = limitData.Whitelist;
            }
        }
    }

    protected static void BlacklistEquipment(KarmaLevel karmaSettings, BotType baseBotNode)
    {
        foreach (var (slot, blacklist) in karmaSettings.EquipmentBlacklist)
        {
            if (!baseBotNode.BotInventory.Equipment.TryGetValue(slot, out var equipmentDict))
            {
                continue;
            }
            foreach (var itemToRemove in blacklist)
            {
                equipmentDict.Remove(itemToRemove);
            }
        }
    }

    protected Stats GetScavStats(PmcData scavProfile)
    {
        if (scavProfile?.Stats != null)
        {
            return scavProfile.Stats;
        }

        return profileHelper.GetDefaultCounters();
    }

    protected int GetScavLevel(PmcData scavProfile)
    {
        // Info can be null on initial account creation
        if (scavProfile?.Info?.Level == null)
        {
            return 1;
        }

        return scavProfile?.Info?.Level ?? 1;
    }

    protected int GetScavExperience(PmcData scavProfile)
    {
        // Info can be null on initial account creation
        if (scavProfile?.Info?.Experience == null)
        {
            return 0;
        }

        return scavProfile?.Info?.Experience ?? 0;
    }

    /// <summary>
    ///     Set cooldown till scav is playable
    ///     take into account scav cooldown bonus
    /// </summary>
    /// <param name="scavData">scav profile</param>
    /// <param name="pmcData">pmc profile</param>
    /// <returns>PmcData</returns>
    protected PmcData SetScavCooldownTimer(PmcData scavData, PmcData pmcData)
    {
        // Get sum of all scav cooldown reduction timer bonuses
        var modifier =
            1d
            + pmcData
                .Bonuses.Where(x => x.Type == BonusType.ScavCooldownTimer)
                .Sum(bonus => (bonus?.Value ?? 1) / 100);

        var fenceInfo = fenceService.GetFenceInfo(pmcData);
        modifier *= fenceInfo.SavageCooldownModifier ?? 1d;

        // Make sure to apply ScavCooldownTimer bonus from Hideout if the player has it.
        var scavLockDuration =
            databaseService.GetGlobals().Configuration.SavagePlayCooldown * modifier;

        var fullProfile = profileHelper.GetFullProfile(pmcData?.SessionId);
        if (
            fullProfile?.ProfileInfo?.Edition?.StartsWith(
                AccountTypes.SPT_DEVELOPER,
                StringComparison.OrdinalIgnoreCase
            ) ?? false
        )
        {
            // Force lock duration to 10seconds for dev profiles
            scavLockDuration = 10;
        }

        if (scavData?.Info != null)
        {
            scavData.Info.SavageLockTime = Math.Round(
                timeUtil.GetTimeStamp() + (scavLockDuration ?? 0)
            );
        }

        return scavData;
    }
}
