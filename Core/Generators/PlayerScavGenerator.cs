using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;
using Core.Utils.Cloners;


namespace Core.Generators;

[Injectable]
public class PlayerScavGenerator
{
    protected ISptLogger<PlayerScavGenerator> _logger;
    protected RandomUtil _randomUtil;
    protected DatabaseService _databaseService;
    protected HashUtil _hashUtil;
    protected ItemHelper _itemHelper;
    protected BotGeneratorHelper _botGeneratorHelper;
    protected SaveServer _saveServer;
    protected ProfileHelper _profileHelper;
    protected BotHelper _botHelper;
    protected FenceService _fenceService;
    protected BotLootCacheService _botLootCacheService;
    protected LocalisationService _localisationService;
    protected BotGenerator _botGenerator;
    protected ConfigServer _configServer;
    protected ICloner _cloner;
    protected TimeUtil _timeUtil;

    private PlayerScavConfig _playerScavConfig;

    public PlayerScavGenerator
    (
        ISptLogger<PlayerScavGenerator> logger,
        RandomUtil randomUtil,
        DatabaseService databaseService,
        HashUtil hashUtil,
        ItemHelper itemHelper,
        BotGeneratorHelper botGeneratorHelper,
        SaveServer saveServer,
        ProfileHelper profileHelper,
        BotHelper botHelper,
        FenceService fenceService,
        BotLootCacheService botLootCacheService,
        LocalisationService localisationService,
        BotGenerator botGenerator,
        ConfigServer configServer,
        ICloner cloner,
        TimeUtil timeUtil
    )
    {
        _logger = logger;
        _randomUtil = randomUtil;
        _databaseService = databaseService;
        _hashUtil = hashUtil;
        _itemHelper = itemHelper;
        _botGeneratorHelper = botGeneratorHelper;
        _saveServer = saveServer;
        _profileHelper = profileHelper;
        _botHelper = botHelper;
        _fenceService = fenceService;
        _botLootCacheService = botLootCacheService;
        _localisationService = localisationService;
        _botGenerator = botGenerator;
        _configServer = configServer;
        _cloner = cloner;
        _timeUtil = timeUtil;

        _playerScavConfig = configServer.GetConfig<PlayerScavConfig>();
    }

    /// <summary>
    /// Update a player profile to include a new player scav profile
    /// </summary>
    /// <param name="sessionID">session id to specify what profile is updated</param>
    /// <returns>profile object</returns>
    public PmcData Generate(string sessionID)
    {
        // get karma level from profile
        var profile = _saveServer.GetProfile(sessionID);
        var profileCharactersClone = _cloner.Clone(profile.CharacterData);
        var pmcDataClone = _cloner.Clone(profileCharactersClone.PmcData);
        var existingScavDataClone = _cloner.Clone(profileCharactersClone.ScavData);

        var scavKarmaLevel = GetScavKarmaLevel(pmcDataClone);

        // use karma level to get correct karmaSettings
        var playerScavKarmaSettings = _playerScavConfig.KarmaLevel[scavKarmaLevel.ToString()];
        if (playerScavKarmaSettings == null)
            _logger.Error(_localisationService.GetText("scav-missing_karma_settings", scavKarmaLevel));

        _logger.Debug($"generated player scav loadout with karma level {scavKarmaLevel}");

        // Edit baseBotNode values
        var baseBotNode = ConstructBotBaseTemplate(playerScavKarmaSettings.BotTypeForLoot);
        AdjustBotTemplateWithKarmaSpecificSettings(playerScavKarmaSettings, baseBotNode);

        var scavData = _botGenerator.GeneratePlayerScav(
            sessionID,
            playerScavKarmaSettings.BotTypeForLoot.ToLower(),
            "easy",
            baseBotNode,
            pmcDataClone);

        // Remove cached bot data after scav was generated
        _botLootCacheService.ClearCache();

        // Add scav metadata
        scavData.Savage = null;
        scavData.Aid = pmcDataClone.Aid;
        scavData.TradersInfo = pmcDataClone.TradersInfo;
        scavData.Info.Settings = new();
        scavData.Info.Bans = [];
        scavData.Info.RegistrationDate = pmcDataClone.Info.RegistrationDate;
        scavData.Info.GameVersion = pmcDataClone.Info.GameVersion;
        scavData.Info.MemberCategory = MemberCategory.UNIQUE_ID;
        scavData.Info.LockedMoveCommands = true;
        scavData.RagfairInfo = pmcDataClone.RagfairInfo;
        scavData.UnlockedInfo = pmcDataClone.UnlockedInfo;

        // Persist previous scav data into new scav
        scavData.Id = existingScavDataClone.Id ?? pmcDataClone.Savage;
        scavData.SessionId = existingScavDataClone.SessionId ?? pmcDataClone.SessionId;
        scavData.Skills = GetScavSkills(existingScavDataClone);
        scavData.Stats = GetScavStats(existingScavDataClone);
        scavData.Info.Level = GetScavLevel(existingScavDataClone);
        scavData.Info.Experience = GetScavExperience(existingScavDataClone);
        scavData.Quests = existingScavDataClone.Quests ?? [];
        scavData.TaskConditionCounters = existingScavDataClone.TaskConditionCounters ?? new();
        scavData.Notes = existingScavDataClone.Notes ?? new() { DataNotes = new() };
        scavData.WishList = existingScavDataClone.WishList ?? new(new(), new());
        scavData.Encyclopedia = pmcDataClone.Encyclopedia ?? new();

        // Add additional items to player scav as loot
        AddAdditionalLootToPlayerScavContainers(playerScavKarmaSettings.LootItemsToAddChancePercent, scavData, [
            EquipmentSlots.TacticalVest,
            EquipmentSlots.Pockets,
            EquipmentSlots.Backpack
        ]);

        // Remove secure container
        scavData = _profileHelper.RemoveSecureContainer(scavData);

        // set cooldown timer
        scavData = SetScavCooldownTimer(scavData, pmcDataClone);

        // add scav to profile
        _saveServer.GetProfile(sessionID).CharacterData.ScavData = scavData;

        return scavData;
    }

    /// <summary>
    /// Add items picked from `playerscav.lootItemsToAddChancePercent`
    /// </summary>
    /// <param name="possibleItemsToAdd">dict of tpl + % chance to be added</param>
    /// <param name="scavData"></param>
    /// <param name="containersToAddTo">Possible slotIds to add loot to</param>
    protected void AddAdditionalLootToPlayerScavContainers(Dictionary<string, double> possibleItemsToAdd, BotBase scavData, List<EquipmentSlots> containersToAddTo)
    {
        foreach (var tpl in possibleItemsToAdd)
        {
            var shouldAdd = _randomUtil.GetChance100(tpl.Value);
            if (!shouldAdd)
                continue;

            var itemResult = _itemHelper.GetItem(tpl.Key);
            if (!itemResult.Key)
            {
                _logger.Warning(_localisationService.GetText("scav-unable_to_add_item_to_player_scav", tpl));
                continue;
            }

            var itemTemplate = itemResult.Value;
            var itemsToAdd = new List<Item>()
            {
                new Item()
                {
                    Id = _hashUtil.Generate(),
                    Template = itemTemplate.Id,
                    Upd = _botGeneratorHelper.GenerateExtraPropertiesForItem(itemTemplate)
                }
            };

            var result = _botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
                containersToAddTo,
                itemsToAdd[0].Id,
                itemTemplate.Id,
                itemsToAdd,
                scavData.Inventory);

            if (result != ItemAddedResult.SUCCESS)
                _logger.Debug($"Unable to add keycard to bot. Reason: {result.ToString()}");
        }
    }

    /// <summary>
    /// Get the scav karama level for a profile
    /// Is also the fence trader rep level
    /// </summary>
    /// <param name="pmcData">pmc profile</param>
    /// <returns>karma level</returns>
    protected double GetScavKarmaLevel(PmcData pmcData)
    {
        // can be empty during profile creation
        if (!pmcData.TradersInfo.TryGetValue(Traders.FENCE, out var fenceInfo))
        {
            _logger.Warning(_localisationService.GetText("scav-missing_karma_level_getting_default"));
            return 0;
        }

        if (fenceInfo.Standing > 6)
            return 6;

        return Math.Floor(fenceInfo.Standing ?? 0);
    }

    /// <summary>
    /// Get a baseBot template
    /// If the parameter doesnt match "assault", take parts from the loot type and apply to the return bot template
    /// </summary>
    /// <param name="botTypeForLoot">bot type to use for inventory/chances</param>
    /// <returns>IBotType object</returns>
    protected BotType ConstructBotBaseTemplate(string botTypeForLoot)
    {
        var baseScavType = "assault";
        var asssaultBase = _cloner.Clone(_botHelper.GetBotTemplate(baseScavType));

        // Loot bot is same as base bot, return base with no modification
        if (botTypeForLoot == baseScavType)
            return asssaultBase;

        var lootBase = _cloner.Clone(_botHelper.GetBotTemplate(botTypeForLoot));
        asssaultBase.BotInventory = lootBase.BotInventory;
        asssaultBase.BotChances = lootBase.BotChances;
        asssaultBase.BotGeneration = lootBase.BotGeneration;

        return asssaultBase;
    }

    /// <summary>
    /// Adjust equipment/mod/item generation values based on scav karma levels
    /// </summary>
    /// <param name="karmaSettings">Values to modify the bot template with</param>
    /// <param name="baseBotNode">bot template to modify according to karama level settings</param>
    protected void AdjustBotTemplateWithKarmaSpecificSettings(KarmaLevel karmaSettings, BotType baseBotNode)
    {
        // Adjust equipment chance values
        foreach (var equipmentKvP in karmaSettings.Modifiers.Equipment) {

            // Adjustment value zero, nothing to do
            if (equipmentKvP.Value == 0)
            {
                continue;
            }

            // Try add new key with value
            if (!baseBotNode.BotChances.EquipmentChances.TryAdd(equipmentKvP.Key, equipmentKvP.Value))
            {
                // Unable to add new, update existing
                baseBotNode.BotChances.EquipmentChances[equipmentKvP.Key] += equipmentKvP.Value;
            }
        }

        // Adjust mod chance values
        foreach (var modKvP in karmaSettings.Modifiers.Mod)
        {
            // Adjustment value zero, nothing to do
            if (modKvP.Value == 0)
                continue;

            baseBotNode.BotChances.WeaponModsChances[modKvP.Key] += karmaSettings.Modifiers.Mod[modKvP.Key];
        }

        // Adjust item spawn quantity values
        var props = baseBotNode.BotGeneration.Items.GetType().GetProperties();
        foreach (var itemLimitKvP in karmaSettings.ItemLimits)
        {
            var prop = props.FirstOrDefault(x => x.Name.ToLower() == itemLimitKvP.Key.ToLower());
            prop.SetValue(baseBotNode.BotGeneration.Items, itemLimitKvP.Value);
        }

        // Blacklist equipment, keyed by equipment slot
        foreach (var equipmentBlacklistKvP in karmaSettings.EquipmentBlacklist) {
            baseBotNode.BotInventory.Equipment.TryGetValue(equipmentBlacklistKvP.Key, out var equipmentDict);
            foreach (var itemToRemove in equipmentBlacklistKvP.Value)
            {
                equipmentDict.Remove(itemToRemove);
            }
        }
    }

    protected Skills GetScavSkills(PmcData scavProfile)
    {
        if (scavProfile?.Skills != null)
            return scavProfile.Skills;

        return GetDefaultScavSkills();
    }

    protected Skills GetDefaultScavSkills()
    {
        return new()
        {
            Common = new(),
            Mastering = new (),
            Points = 0
        };
    }

    protected Stats GetScavStats(PmcData scavProfile)
    {
        if (scavProfile?.Stats != null)
            return scavProfile.Stats;

        return _profileHelper.GetDefaultCounters();
    }

    protected double GetScavLevel(PmcData scavProfile)
    {
        // Info can be null on initial account creation
        if (scavProfile?.Info?.Level == null)
            return 1;

        return scavProfile?.Info?.Level ?? 1;
    }

    protected double GetScavExperience(PmcData scavProfile)
    {
        // Info can be null on initial account creation
        if (scavProfile?.Info?.Experience == null)
            return 0;

        return scavProfile?.Info?.Experience ?? 0;
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
        // Set cooldown time.
        // Make sure to apply ScavCooldownTimer bonus from Hideout if the player has it.
        var scavLockDuration = _databaseService.GetGlobals().Configuration.SavagePlayCooldown;
        var modifier = 1D;

        foreach (var bonus in pmcData.Bonuses)
        {
            if (bonus.Type == BonusType.ScavCooldownTimer)
            {
                // Value is negative, so add.
                // Also note that for scav cooldown, multiple bonuses stack additively.
                modifier += (bonus?.Value ?? 1) / 100;
            }
        }

        var fenceInfo = _fenceService.GetFenceInfo(pmcData);
        modifier *= fenceInfo.SavageCooldownModifier ?? 1;
        scavLockDuration *= modifier;

        var fullProfile = _profileHelper.GetFullProfile(pmcData?.SessionId);
        if (fullProfile?.ProfileInfo?.Edition.ToLower().StartsWith(AccountTypes.SPT_DEVELOPER) ?? false)
            scavLockDuration = 10;

        if (scavData?.Info != null)
            scavData.Info.SavageLockTime = Math.Round(_timeUtil.GetTimeStamp() / 1000 + scavLockDuration ?? 0);

        return scavData;
    }
}
