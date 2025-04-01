using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;
using SPTarkov.Common.Extensions;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class SeasonalEventService(
    ISptLogger<SeasonalEventService> _logger,
    TimeUtil _timeUtil,
    DatabaseService _databaseService,
    GiftService _giftService,
    LocalisationService _localisationService,
    BotHelper _botHelper,
    ProfileHelper _profileHelper,
    //DatabaseImporter _databaseImporter,
    ConfigServer _configServer
)
{
    private bool _christmasEventActive;

    protected HashSet<string> _christmasEventItems =
    [
        ItemTpl.ARMOR_6B13_M_ASSAULT_ARMOR_CHRISTMAS_EDITION,
        ItemTpl.BACKPACK_SANTAS_BAG,
        ItemTpl.BARTER_CHRISTMAS_TREE_ORNAMENT_RED,
        ItemTpl.BARTER_CHRISTMAS_TREE_ORNAMENT_SILVER,
        ItemTpl.BARTER_CHRISTMAS_TREE_ORNAMENT_VIOLET,
        ItemTpl.BARTER_JAR_OF_PICKLES,
        ItemTpl.BARTER_OLIVIER_SALAD_BOX,
        ItemTpl.BARTER_SPECIAL_40DEGREE_FUEL,
        ItemTpl.HEADWEAR_DED_MOROZ_HAT,
        ItemTpl.HEADWEAR_ELF_HAT,
        ItemTpl.HEADWEAR_HAT_WITH_HORNS,
        ItemTpl.HEADWEAR_MASKA1SCH_BULLETPROOF_HELMET_CHRISTMAS_EDITION,
        ItemTpl.HEADWEAR_SANTA_HAT,
        ItemTpl.RANDOMLOOTCONTAINER_NEW_YEAR_GIFT_BIG,
        ItemTpl.RANDOMLOOTCONTAINER_NEW_YEAR_GIFT_MEDIUM,
        ItemTpl.RANDOMLOOTCONTAINER_NEW_YEAR_GIFT_SMALL,
        ItemTpl.FACECOVER_ASTRONOMER_MASK,
        ItemTpl.FACECOVER_AYBOLIT_MASK,
        ItemTpl.FACECOVER_CIPOLLINO_MASK,
        ItemTpl.FACECOVER_FAKE_WHITE_BEARD,
        ItemTpl.FACECOVER_FOX_MASK,
        ItemTpl.FACECOVER_GRINCH_MASK,
        ItemTpl.FACECOVER_HARE_MASK,
        ItemTpl.FACECOVER_ROOSTER_MASK
    ];

    private List<SeasonalEvent> _currentlyActiveEvents = [];
    private bool _halloweenEventActive;

    protected HashSet<string> _halloweenEventItems =
    [
        ItemTpl.HEADWEAR_JACKOLANTERN_TACTICAL_PUMPKIN_HELMET,
        ItemTpl.FACECOVER_FACELESS_MASK,
        ItemTpl.FACECOVER_GHOUL_MASK,
        ItemTpl.FACECOVER_HOCKEY_PLAYER_MASK_BRAWLER,
        ItemTpl.FACECOVER_HOCKEY_PLAYER_MASK_CAPTAIN,
        ItemTpl.FACECOVER_HOCKEY_PLAYER_MASK_QUIET,
        ItemTpl.FACECOVER_JASON_MASK,
        ItemTpl.FACECOVER_MISHA_MAYOROV_MASK,
        ItemTpl.FACECOVER_SLENDER_MASK,
        ItemTpl.FACECOVER_SPOOKY_SKULL_MASK,
        ItemTpl.RANDOMLOOTCONTAINER_PUMPKIN_RAND_LOOT_CONTAINER
    ];

    protected HttpConfig _httpConfig = _configServer.GetConfig<HttpConfig>();
    protected LocationConfig _locationConfig = _configServer.GetConfig<LocationConfig>();
    protected QuestConfig _questConfig = _configServer.GetConfig<QuestConfig>();
    protected SeasonalEventConfig _seasonalEventConfig = _configServer.GetConfig<SeasonalEventConfig>();
    protected WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();

    /// <summary>
    ///     Get an array of christmas items found in bots inventories as loot
    /// </summary>
    /// <returns>array</returns>
    public HashSet<string> GetChristmasEventItems()
    {
        return _christmasEventItems;
    }

    /// <summary>
    ///     Get an array of halloween items found in bots inventories as loot
    /// </summary>
    /// <returns>array</returns>
    public HashSet<string> GetHalloweenEventItems()
    {
        return _halloweenEventItems;
    }

    public bool ItemIsChristmasRelated(string itemTpl)
    {
        return _christmasEventItems.Contains(itemTpl);
    }

    public bool ItemIsHalloweenRelated(string itemTpl)
    {
        return _halloweenEventItems.Contains(itemTpl);
    }

    /// <summary>
    ///     Check if item id exists in christmas or halloween event arrays
    /// </summary>
    /// <param name="itemTpl">item tpl to check for</param>
    /// <returns></returns>
    public bool ItemIsSeasonalRelated(string itemTpl)
    {
        return _christmasEventItems.Contains(itemTpl) || _halloweenEventItems.Contains(itemTpl);
    }

    /// <summary>
    ///     Get active seasonal events
    /// </summary>
    /// <returns>Array of active events</returns>
    public List<SeasonalEvent> GetActiveEvents()
    {
        return _currentlyActiveEvents;
    }

    /// <summary>
    ///     Get an array of seasonal items that should not appear
    ///     e.g. if halloween is active, only return christmas items
    ///     or, if halloween and christmas are inactive, return both sets of items
    /// </summary>
    /// <returns>array of tpl strings</returns>
    public HashSet<string> GetInactiveSeasonalEventItems()
    {
        var items = new HashSet<string>();
        if (!ChristmasEventEnabled())
        {
            items.UnionWith(_christmasEventItems);
        }

        if (!HalloweenEventEnabled())
        {
            items.UnionWith(_halloweenEventItems);
        }

        return items;
    }

    /// <summary>
    ///     Is a seasonal event currently active
    /// </summary>
    /// <returns>true if event is active</returns>
    public bool SeasonalEventEnabled()
    {
        return _christmasEventActive || _halloweenEventActive;
    }

    /// <summary>
    ///     Is christmas event active
    /// </summary>
    /// <returns>true if active</returns>
    public bool ChristmasEventEnabled()
    {
        return _christmasEventActive;
    }

    /// <summary>
    ///     is halloween event active
    /// </summary>
    /// <returns>true if active</returns>
    public bool HalloweenEventEnabled()
    {
        return _halloweenEventActive;
    }

    /// <summary>
    ///     Is detection of seasonal events enabled (halloween / christmas)
    /// </summary>
    /// <returns>true if seasonal events should be checked for</returns>
    public bool IsAutomaticEventDetectionEnabled()
    {
        return _seasonalEventConfig.EnableSeasonalEventDetection;
    }

    /// <summary>
    ///     Get a dictionary of gear changes to apply to bots for a specific event e.g. Christmas/Halloween
    /// </summary>
    /// <param name="eventName">Name of event to get gear changes for</param>
    /// <returns>bots with equipment changes</returns>
    protected Dictionary<string, Dictionary<string, Dictionary<string, int>>>? GetEventBotGear(SeasonalEventType eventType)
    {
        return _seasonalEventConfig.EventGear.GetValueOrDefault(eventType, null);
    }

    /// <summary>
    ///     Get a dictionary of loot changes to apply to bots for a specific event e.g. Christmas/Halloween
    /// </summary>
    /// <param name="eventName">Name of event to get gear changes for</param>
    /// <returns>bots with loot changes</returns>
    protected Dictionary<string, Dictionary<string, Dictionary<string, int>>> GetEventBotLoot(SeasonalEventType eventType)
    {
        return _seasonalEventConfig.EventLoot.GetValueOrDefault(eventType, null);
    }

    /// <summary>
    ///     Get the dates each seasonal event starts and ends at
    /// </summary>
    /// <returns>Record with event name + start/end date</returns>
    public List<SeasonalEvent> GetEventDetails()
    {
        return _seasonalEventConfig.Events;
    }

    /// <summary>
    ///     Look up quest in configs/quest.json
    /// </summary>
    /// <param name="questId">Quest to look up</param>
    /// <param name="event">event type (Christmas/Halloween/None)</param>
    /// <returns>true if related</returns>
    public bool IsQuestRelatedToEvent(string questId, SeasonalEventType eventType)
    {
        var eventQuestData = _questConfig.EventQuests.GetValueOrDefault(questId, null);
        if (eventQuestData?.Season == eventType)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Handle activating seasonal events
    /// </summary>
    public void EnableSeasonalEvents()
    {
        if (_currentlyActiveEvents.Any())
        {
            var globalConfig = _databaseService.GetGlobals().Configuration;
            foreach (var activeEvent in _currentlyActiveEvents)
            {
                UpdateGlobalEvents(globalConfig, activeEvent);
            }
        }
    }

    /// <summary>
    ///     Force a seasonal event to be active
    /// </summary>
    /// <param name="eventType">Event to force active</param>
    /// <returns>True if event was successfully force enabled</returns>
    public bool ForceSeasonalEvent(SeasonalEventType eventType)
    {
        var globalConfig = _databaseService.GetGlobals().Configuration;
        var seasonEvent = _seasonalEventConfig.Events.FirstOrDefault(e => e.Type == eventType);
        if (seasonEvent is null)
        {
            _logger.Warning($"Unable to force event: {eventType} as it cannot be found in events config");
            return false;
        }

        UpdateGlobalEvents(globalConfig, seasonEvent);

        return true;
    }

    /// <summary>
    ///     Store active events inside class list property `currentlyActiveEvents` + set class properties: christmasEventActive/halloweenEventActive
    /// </summary>
    public void CacheActiveEvents()
    {
        var currentDate = DateTimeOffset.UtcNow.DateTime;
        var seasonalEvents = GetEventDetails();

        // reset existing data
        _currentlyActiveEvents = new List<SeasonalEvent>();

        // Add active events to array
        foreach (var events in seasonalEvents)
        {
            if (!events.Enabled)
            {
                continue;
            }

            if (DateIsBetweenTwoDates(currentDate, events.StartMonth, events.StartDay, events.EndMonth, events.EndDay))
            {
                _currentlyActiveEvents.Add(events);
            }
        }
    }

    /// <summary>
    ///     Get the currently active weather season e.g. SUMMER/AUTUMN/WINTER
    /// </summary>
    /// <returns>Season enum value</returns>
    public Season GetActiveWeatherSeason()
    {
        if (_weatherConfig.OverrideSeason.HasValue)
        {
            return _weatherConfig.OverrideSeason.Value;
        }

        var currentDate = _timeUtil.GetDateTimeNow();
        foreach (var seasonRange in _weatherConfig.SeasonDates)
        {
            if (
                DateIsBetweenTwoDates(
                    currentDate,
                    seasonRange.StartMonth ?? 0,
                    seasonRange.StartDay ?? 0,
                    seasonRange.EndMonth ?? 0,
                    seasonRange.EndDay ?? 0
                )
            )
            {
                return seasonRange.SeasonType ?? Season.SUMMER;
            }
        }

        _logger.Warning(_localisationService.GetText("season-no_matching_season_found_for_date"));

        return Season.SUMMER;
    }

    /// <summary>
    ///     Does the provided date fit between the two defined dates?
    ///     Excludes year
    ///     Inclusive of end date upto 23 hours 59 minutes
    /// </summary>
    /// <param name="dateToCheck">Date to check is between 2 dates</param>
    /// <param name="startMonth">Lower bound for month</param>
    /// <param name="startDay">Lower bound for day</param>
    /// <param name="endMonth">Upper bound for month</param>
    /// <param name="endDay">Upper bound for day</param>
    /// <returns>True when inside date range</returns>
    private bool DateIsBetweenTwoDates(DateTime dateToCheck, int startMonth, int startDay, int endMonth, int endDay)
    {
        var eventStartDate = new DateTime(dateToCheck.Year, startMonth, startDay);
        var eventEndDate = new DateTime(dateToCheck.Year, endMonth, endDay, 23, 59, 0);

        return dateToCheck >= eventStartDate && dateToCheck <= eventEndDate;
    }

    /// <summary>
    ///     Iterate through bots inventory and loot to find and remove christmas items (as defined in SeasonalEventService)
    /// </summary>
    /// <param name="botInventory">Bots inventory to iterate over</param>
    /// <param name="botRole">the role of the bot being processed</param>
    public void RemoveChristmasItemsFromBotInventory(BotTypeInventory botInventory, string botRole)
    {
        var christmasItems = GetChristmasEventItems();
        HashSet<EquipmentSlots> equipmentSlotsToFilter = [EquipmentSlots.FaceCover, EquipmentSlots.Headwear, EquipmentSlots.Backpack, EquipmentSlots.TacticalVest];
        HashSet<string> lootContainersToFilter = ["Backpack", "Pockets", "TacticalVest"];

        // Remove christmas related equipment
        foreach (var equipmentSlotKey in equipmentSlotsToFilter)
        {
            if (botInventory.Equipment[equipmentSlotKey] is null)
            {
                _logger.Warning(
                    _localisationService.GetText(
                        "seasonal-missing_equipment_slot_on_bot",
                        new
                        {
                            equipmentSlot = equipmentSlotKey,
                            botRole
                        }
                    )
                );
            }

            var equipment = botInventory.Equipment[equipmentSlotKey];
            botInventory.Equipment[equipmentSlotKey] = equipment.Where(i => !_christmasEventItems.Contains(i.Key)).ToDictionary();
        }

        // Remove christmas related loot from loot containers
        var props = botInventory.Items.GetType().GetProperties();
        foreach (var lootContainerKey in lootContainersToFilter)
        {
            var prop = (Dictionary<string, double>?) props
                .FirstOrDefault(p => string.Equals(p.Name.ToLower(), lootContainerKey.ToLower(), StringComparison.OrdinalIgnoreCase))
                .GetValue(botInventory.Items);

            if (prop is null)
            {
                _logger.Warning(
                    _localisationService.GetText(
                        "seasonal-missing_loot_container_slot_on_bot",
                        new
                        {
                            lootContainer = lootContainerKey,
                            botRole
                        }
                    )
                );
            }

            List<string> tplsToRemove = [];
            foreach (var tplKey in prop)
            {
                if (christmasItems.Contains(tplKey.Key))
                {
                    tplsToRemove.Add(tplKey.Key);
                }
            }

            foreach (var tplToRemove in tplsToRemove)
            {
                prop.Remove(tplToRemove);
            }

            // Get non-christmas items
            var nonChristmasTpls = prop.Where(tpl => !christmasItems.Contains(tpl.Key));
            if (!nonChristmasTpls.Any())
            {
                continue;
            }

            Dictionary<string, double> intermediaryDict = new();

            foreach (var tpl in nonChristmasTpls)
            {
                intermediaryDict[tpl.Key] = prop[tpl.Key];
            }

            // Replace the original containerItems with the updated one
            prop = intermediaryDict;
        }
    }

    /// <summary>
    ///     Make adjusted to server code based on the name of the event passed in
    /// </summary>
    /// <param name="globalConfig">globals.json</param>
    /// <param name="event">Name of the event to enable. e.g. Christmas</param>
    private void UpdateGlobalEvents(Config globalConfig, SeasonalEvent eventType)
    {
        _logger.Success(_localisationService.GetText("season-event_is_active", eventType.Type));
        _christmasEventActive = false;
        _halloweenEventActive = false;

        switch (eventType.Type)
        {
            case SeasonalEventType.Halloween:
                ApplyHalloweenEvent(eventType, globalConfig);
                break;
            case SeasonalEventType.Christmas:
                ApplyChristmasEvent(eventType, globalConfig);
                break;
            case SeasonalEventType.NewYears:
                ApplyNewYearsEvent(eventType, globalConfig);

                break;
            case SeasonalEventType.AprilFools:
                AddGifterBotToMaps();
                AddLootItemsToGifterDropItemsList();
                AddEventGearToBots(SeasonalEventType.Halloween);
                AddEventGearToBots(SeasonalEventType.Christmas);
                AddEventLootToBots(SeasonalEventType.Christmas);
                AddEventBossesToMaps("halloweensummon");
                EnableHalloweenSummonEvent();
                AddPumpkinsToScavBackpacks();
                RenameBitcoin();
                if (eventType.Settings is not null && eventType.Settings.ReplaceBotHostility.GetValueOrDefault(false)) {
                    if (_seasonalEventConfig.HostilitySettingsForEvent.TryGetValue("AprilFools", out var botData))
                    {
                        ReplaceBotHostility(botData);
                    }
                }

                if (eventType.Settings?.ForceSeason != null) {
                    _weatherConfig.OverrideSeason = eventType.Settings.ForceSeason;
                }

                break;
            default:
                // Likely a mod event
                HandleModEvent(eventType, globalConfig);
                break;
        }
    }

    private void ApplyHalloweenEvent(SeasonalEvent eventType, Config globalConfig)
    {
        _halloweenEventActive = true;

        globalConfig.EventType = globalConfig.EventType.Where(x => x != EventType.None).ToList();
        globalConfig.EventType.Add(EventType.Halloween);
        globalConfig.EventType.Add(EventType.HalloweenIllumination);
        globalConfig.Health.ProfileHealthSettings.DefaultStimulatorBuff = "Buffs_Halloween";
        AddEventGearToBots(eventType.Type);
        AdjustZryachiyMeleeChance();
        if (eventType.Settings?.EnableSummoning ?? false)
        {
            EnableHalloweenSummonEvent();
            AddEventBossesToMaps("halloweensummon");
        }

        if (eventType.Settings?.ZombieSettings?.Enabled ?? false)
        {
            ConfigureZombies(eventType.Settings.ZombieSettings);
        }

        if (eventType.Settings?.RemoveEntryRequirement is not null)
        {
            RemoveEntryRequirement(eventType.Settings.RemoveEntryRequirement);
        }

        if (eventType.Settings?.ReplaceBotHostility ?? false)
        {
            ReplaceBotHostility(_seasonalEventConfig.HostilitySettingsForEvent.FirstOrDefault(x => x.Key == "zombies").Value);
        }

        if (eventType.Settings?.AdjustBotAppearances ?? false)
        {
            AdjustBotAppearanceValues(eventType.Type);
        }

        AddPumpkinsToScavBackpacks();
        AdjustTraderIcons(eventType.Type);
    }

    private void ApplyChristmasEvent(SeasonalEvent eventType, Config globalConfig)
    {
        _christmasEventActive = true;

        if (eventType.Settings?.EnableChristmasHideout ?? false)
        {
            globalConfig.EventType = globalConfig.EventType.Where(x => x != EventType.None).ToList();
            globalConfig.EventType.Add(EventType.Christmas);
        }

        AddEventGearToBots(eventType.Type);
        AddEventLootToBots(eventType.Type);

        if (eventType.Settings?.EnableSanta ?? false)
        {
            AddGifterBotToMaps();
            AddLootItemsToGifterDropItemsList();
        }

        EnableDancingTree();
        if (eventType.Settings?.AdjustBotAppearances ?? false)
        {
            AdjustBotAppearanceValues(eventType.Type);
        }
    }

    private void ApplyNewYearsEvent(SeasonalEvent eventType, Config globalConfig)
    {
        _christmasEventActive = true;

        if (eventType.Settings?.EnableChristmasHideout ?? false)
        {
            globalConfig.EventType = globalConfig.EventType.Where(x => x != EventType.None).ToList();
            globalConfig.EventType.Add(EventType.Christmas);
        }

        AddEventGearToBots(SeasonalEventType.Christmas);
        AddEventLootToBots(SeasonalEventType.Christmas);

        if (eventType.Settings?.EnableSanta ?? false)
        {
            AddGifterBotToMaps();
            AddLootItemsToGifterDropItemsList();
        }

        EnableDancingTree();

        if (eventType.Settings?.AdjustBotAppearances ?? false)
        {
            AdjustBotAppearanceValues(SeasonalEventType.Christmas);
        }
    }

    private void AdjustBotAppearanceValues(SeasonalEventType season)
    {
        var adjustments = _seasonalEventConfig.BotAppearanceChanges[season];
        if (adjustments is null)
        {
            return;
        }

        foreach (var botTypeKey in adjustments)
        {
            var botDb = _databaseService.GetBots().Types[botTypeKey.Key];
            if (botDb is null)
            {
                continue;
            }

            var botAppearanceAdjustments = botTypeKey.Value;
            foreach (var appearanceKey in botAppearanceAdjustments)
            {
                var weightAdjustments = appearanceKey.Value;
                var props = botDb.BotAppearance.GetType().GetProperties();
                foreach (var itemKey in weightAdjustments)
                {
                    var prop = props.FirstOrDefault(x => string.Equals(x.Name, appearanceKey.Key, StringComparison.CurrentCultureIgnoreCase));
                    var propValue = (Dictionary<string, double>) prop.GetValue(botDb.BotAppearance);
                    propValue[itemKey.Key] = weightAdjustments[itemKey.Key];
                    prop.SetValue(botDb.BotAppearance, propValue);
                }
            }
        }
    }

    private void ReplaceBotHostility(Dictionary<string, List<AdditionalHostilitySettings>> hostilitySettings)
    {
        var locations = _databaseService.GetLocations();
        var ignoreList = _locationConfig.NonMaps;

        var props = locations.GetType().GetProperties();

        foreach (var locationProp in props)
        {
            if (ignoreList.Contains(locationProp.Name))
            {
                continue;
            }

            var location = (Location) locationProp.GetValue(locations);
            if (location?.Base?.BotLocationModifier?.AdditionalHostilitySettings is null)
            {
                continue;
            }

            // Try to get map 'default' first if it exists
            if (!hostilitySettings.TryGetValue("Default", out var newHostilitySettings))
            {
                // No 'default', try for location name
                if (!hostilitySettings.TryGetValue(locationProp.Name, out newHostilitySettings))
                {
                    // no settings for map by name, skip map
                    continue;
                }
            }

            foreach (var settings in newHostilitySettings) {
                var matchingBaseSettings = location.Base.BotLocationModifier.AdditionalHostilitySettings.FirstOrDefault(x => x.BotRole == settings.BotRole);
                if (matchingBaseSettings is null)
                {
                    continue;
                }

                if (settings.AlwaysEnemies is not null)
                {
                    matchingBaseSettings.AlwaysEnemies = settings.AlwaysEnemies;
                }

                if (settings.AlwaysFriends is not null)
                {
                    matchingBaseSettings.AlwaysFriends = settings.AlwaysFriends;
                }

                if (settings.BearEnemyChance is not null)
                {
                    matchingBaseSettings.BearEnemyChance = settings.BearEnemyChance;
                }

                if (settings.ChancedEnemies is not null)
                {
                    matchingBaseSettings.ChancedEnemies = settings.ChancedEnemies;
                }

                if (settings.Neutral is not null)
                {
                    matchingBaseSettings.Neutral = settings.Neutral;
                }

                if (settings.SavageEnemyChance is not null)
                {
                    matchingBaseSettings.SavageEnemyChance = settings.SavageEnemyChance;
                }

                if (settings.SavagePlayerBehaviour is not null)
                {
                    matchingBaseSettings.SavagePlayerBehaviour = settings.SavagePlayerBehaviour;
                }

                if (settings.UsecEnemyChance is not null)
                {
                    matchingBaseSettings.UsecEnemyChance = settings.UsecEnemyChance;
                }

                if (settings.UsecPlayerBehaviour is not null)
                {
                    matchingBaseSettings.UsecPlayerBehaviour = settings.UsecPlayerBehaviour;
                }

                if (settings.Warn is not null)
                {
                    matchingBaseSettings.Warn = settings.Warn;
                }
            }
        }
    }

    private void RemoveEntryRequirement(List<string> locationIds)
    {
        foreach (var locationId in locationIds)
        {
            var location = _databaseService.GetLocation(locationId);
            location.Base.AccessKeys = [];
            location.Base.AccessKeysPvE = [];
        }
    }

    public void GivePlayerSeasonalGifts(string sessionId)
    {
        if (_currentlyActiveEvents is null)
        {
            return;
        }

        foreach (var seasonEvent in _currentlyActiveEvents)
        {
            switch (seasonEvent.Type)
            {
                case SeasonalEventType.Christmas:
                    GiveGift(sessionId, "Christmas2022");
                    break;
                case SeasonalEventType.NewYears:
                    GiveGift(sessionId, "NewYear2023");
                    GiveGift(sessionId, "NewYear2024");
                    break;
            }
        }
    }

    /// <summary>
    ///     Force zryachiy to always have a melee weapon
    /// </summary>
    protected void AdjustZryachiyMeleeChance()
    {
        var zryachiyKvP = _databaseService.GetBots().Types.FirstOrDefault(x => x.Key.ToLower() == "bosszryachiy");
        var value = new Dictionary<string, double>();

        foreach (var chance in zryachiyKvP.Value.BotChances.EquipmentChances)
        {
            if (string.Equals(chance.Key, "Scabbard", StringComparison.OrdinalIgnoreCase))
            {
                value.Add(chance.Key, 100);
                continue;
            }

            value.Add(chance.Key, chance.Value);
        }

        zryachiyKvP.Value.BotChances.EquipmentChances = value;
    }

    /// <summary>
    ///     Enable the halloween zryachiy summon event
    /// </summary>
    protected void EnableHalloweenSummonEvent()
    {
        _databaseService.GetGlobals().Configuration.EventSettings.EventActive = true;
    }

    protected void ConfigureZombies(ZombieSettings zombieSettings)
    {
        var globals = _databaseService.GetGlobals();
        var infectionHalloween = globals.Configuration.SeasonActivity.InfectionHalloween;
        infectionHalloween.DisplayUIEnabled = true;
        infectionHalloween.Enabled = true;

        var globalInfectionDict = globals.LocationInfection.GetAllPropsAsDict();
        foreach (var infectedLocationKvP in zombieSettings.MapInfectionAmount)
        {
            var mappedLocations = GetLocationFromInfectedLocation(infectedLocationKvP.Key);

            foreach (var locationKey in mappedLocations)
            {
                _databaseService.GetLocation(
                            locationKey.ToLower()
                        )
                        .Base.Events.Halloween2024.InfectionPercentage =
                    zombieSettings.MapInfectionAmount[infectedLocationKvP.Key];
            }

            globalInfectionDict[infectedLocationKvP.Key] =
                zombieSettings.MapInfectionAmount[infectedLocationKvP.Key];
        }

        foreach (var locationId in zombieSettings.DisableBosses)
        {
            _databaseService.GetLocation(locationId).Base.BossLocationSpawn = [];
        }

        foreach (var locationId in zombieSettings.DisableWaves)
        {
            _databaseService.GetLocation(locationId).Base.Waves = [];
        }

        var locationsWithActiveInfection = GetLocationsWithZombies(zombieSettings.MapInfectionAmount);
        AddEventBossesToMaps("halloweenzombies", locationsWithActiveInfection);
    }

    /// <summary>
    ///     Get location ids of maps with an infection above 0
    /// </summary>
    /// <param name="locationInfections">Dict of locations with their infection percentage</param>
    /// <returns>List of location ids</returns>
    protected HashSet<string> GetLocationsWithZombies(Dictionary<string, double> locationInfections)
    {
        var result = new HashSet<string>();

        // Get only the locations with an infection above 0
        var infectionKeys = locationInfections.Where(
            location => locationInfections[location.Key] > 0
        );

        // Convert the infected location id into its generic location id
        foreach (var location in infectionKeys)
        {
            result.UnionWith(GetLocationFromInfectedLocation(location.Key));
        }

        return result;
    }

    /// <summary>
    ///     BSG store the location ids differently inside `LocationInfection`, need to convert to matching location IDs
    /// </summary>
    /// <param name="infectedLocationKey">Key to convert</param>
    /// <returns>List of locations</returns>
    protected List<string> GetLocationFromInfectedLocation(string infectedLocationKey)
    {
        return infectedLocationKey switch
        {
            "factory4" => ["factory4_day", "factory4_night"],
            "Sandbox" => ["sandbox", "sandbox_high"],
            _ => [infectedLocationKey]
        };
    }

    protected void AddEventWavesToMaps(string eventType)
    {
        var wavesToAddByMap = _seasonalEventConfig.EventWaves[eventType.ToLower()];

        if (wavesToAddByMap is null)
        {
            _logger.Warning($"Unable to add: {eventType} waves, eventWaves is missing");
            return;
        }

        var locations = _databaseService.GetLocations().GetAllPropsAsDict();
        foreach (var map in wavesToAddByMap)
        {
            var wavesToAdd = wavesToAddByMap[map.Key];
            if (wavesToAdd is null)
            {
                _logger.Warning($"Unable to add: {eventType} wave to: {map.Key}");
                continue;
            }

            ((Location) locations[map.Key]).Base.Waves = [];
            ((Location) locations[map.Key]).Base.Waves.AddRange(wavesToAdd);
        }
    }

    /// <summary>
    ///     Add event bosses to maps
    /// </summary>
    /// <param name="eventType">Seasonal event, e.g. HALLOWEEN/CHRISTMAS</param>
    /// <param name="mapIdWhitelist">OPTIONAL - Maps to add bosses to</param>
    protected void AddEventBossesToMaps(string eventType, HashSet<string>? mapIdWhitelist = null)
    {
        if (!_seasonalEventConfig.EventBossSpawns.TryGetValue(eventType.ToLower(), out var botsToAddPerMap))
        {
            _logger.Warning($"Unable to add: {eventType} bosses, eventBossSpawns is missing");
            return;
        }

        var mapKeys = botsToAddPerMap;
        var locations = _databaseService.GetLocations().GetAllPropsAsDict();
        foreach (var (key, _) in mapKeys)
        {
            if (!botsToAddPerMap.TryGetValue(key, out var bossesToAdd))
            {
                _logger.Warning($"Unable to add: {eventType} bosses to: {key}");

                continue;
            }

            if (mapIdWhitelist is null || !mapIdWhitelist.Contains(key))
            {
                continue;
            }

            foreach (var boss in bossesToAdd)
            {
                var mapBosses = ((Location) locations[key]).Base.BossLocationSpawn;
                // If no bosses match by name
                if (mapBosses.All(bossSpawn => bossSpawn.BossName != boss.BossName))
                {
                    ((Location) locations[key]).Base.BossLocationSpawn.AddRange(bossesToAdd);
                }
            }
        }
    }

    /// <summary>
    ///     Change trader icons to be more event themed (Halloween only so far)
    /// </summary>
    /// <param name="eventType">What event is active</param>
    protected void AdjustTraderIcons(SeasonalEventType eventType)
    {
        switch (eventType)
        {
            case SeasonalEventType.Halloween:
                _httpConfig.ServerImagePathOverride["./assets/images/traders/5a7c2ebb86f7746e324a06ab.png"] =
                    "./assets/images/traders/halloween/5a7c2ebb86f7746e324a06ab.png";
                _httpConfig.ServerImagePathOverride["./assets/images/traders/5ac3b86a86f77461491d1ad8.png"] =
                    "./assets/images/traders/halloween/5ac3b86a86f77461491d1ad8.png";
                _httpConfig.ServerImagePathOverride["./assets/images/traders/5c06531a86f7746319710e1b.png"] =
                    "./assets/images/traders/halloween/5c06531a86f7746319710e1b.png";
                _httpConfig.ServerImagePathOverride["./assets/images/traders/59b91ca086f77469a81232e4.png"] =
                    "./assets/images/traders/halloween/59b91ca086f77469a81232e4.png";
                _httpConfig.ServerImagePathOverride["./assets/images/traders/59b91cab86f77469aa5343ca.png"] =
                    "./assets/images/traders/halloween/59b91cab86f77469aa5343ca.png";
                _httpConfig.ServerImagePathOverride["./assets/images/traders/59b91cb486f77469a81232e5.png"] =
                    "./assets/images/traders/halloween/59b91cb486f77469a81232e5.png";
                _httpConfig.ServerImagePathOverride["./assets/images/traders/59b91cbd86f77469aa5343cb.png"] =
                    "./assets/images/traders/halloween/59b91cbd86f77469aa5343cb.png";
                _httpConfig.ServerImagePathOverride["./assets/images/traders/579dc571d53a0658a154fbec.png"] =
                    "./assets/images/traders/halloween/579dc571d53a0658a154fbec.png";
                break;
            case SeasonalEventType.Christmas:
                // TODO: find christmas trader icons
                break;
        }

        // TODO: implement this properly as new function
        //_databaseImporter.LoadImages($"{ _databaseImporter.GetSptDataPath()} images /"
        //    ,["traders"]
        //    ,["/files/trader/avatar/"]);
    }

    /// <summary>
    ///     Add lootable items from backpack into patrol.ITEMS_TO_DROP difficulty property
    /// </summary>
    protected void AddLootItemsToGifterDropItemsList()
    {
        var gifterBot = _databaseService.GetBots().Types["gifter"];
        var itemsCSV = string.Join(",", gifterBot.BotInventory.Items.Backpack.Keys);
        string[] difficulties = ["easy", "normal", "hard", "impossible"];

        foreach (var difficulty in difficulties)
        {
            gifterBot.BotDifficulty[difficulty].Patrol.TryAdd("ITEMS_TO_DROP", "");
            gifterBot.BotDifficulty[difficulty].Patrol["ITEMS_TO_DROP"] = itemsCSV;
        }
    }

    /// <summary>
    ///     Read in data from seasonalEvents.json and add found equipment items to bots
    /// </summary>
    /// <param name="eventType">Name of the event to read equipment in from config</param>
    protected void AddEventGearToBots(SeasonalEventType eventType)
    {
        var botGearChanges = GetEventBotGear(eventType);
        if (botGearChanges is null)
        {
            _logger.Warning(_localisationService.GetText("gameevent-no_gear_data", eventType));

            return;
        }

        // Iterate over bots with changes to apply
        foreach (var botKvP in botGearChanges)
        {
            var botToUpdate = _databaseService.GetBots().Types[botKvP.Key.ToLower()];
            if (botToUpdate is null)
            {
                _logger.Warning(_localisationService.GetText("gameevent-bot_not_found", botKvP));
                continue;
            }

            // Iterate over each equipment slot change
            var gearAmendmentsBySlot = botGearChanges[botKvP.Key];
            foreach (var equipmentKvP in gearAmendmentsBySlot)
            {
                // Adjust slots spawn chance to be at least 75%
                botToUpdate.BotChances.EquipmentChances[equipmentKvP.Key] = Math.Max(
                    botToUpdate.BotChances.EquipmentChances[equipmentKvP.Key],
                    75
                );

                // Grab gear to add and loop over it
                foreach (var itemToAddKvP in equipmentKvP.Value)
                {
                    var equipmentSlot = (EquipmentSlots) Enum.Parse(typeof(EquipmentSlots), equipmentKvP.Key);
                    var equipmentDict = botToUpdate.BotInventory.Equipment[equipmentSlot];
                    equipmentDict[itemToAddKvP.Key] = equipmentKvP.Value[itemToAddKvP.Key];
                }
            }
        }
    }

    /// <summary>
    ///     Read in data from seasonalEvents.json and add found loot items to bots
    /// </summary>
    /// <param name="eventType">Name of the event to read loot in from config</param>
    protected void AddEventLootToBots(SeasonalEventType eventType)
    {
        var botLootChanges = GetEventBotLoot(eventType);
        if (botLootChanges is null)
        {
            _logger.Warning(_localisationService.GetText("gameevent-no_gear_data", eventType));

            return;
        }

        // Iterate over bots with changes to apply
        foreach (var botKvpP in botLootChanges)
        {
            var botToUpdate = _databaseService.GetBots().Types[botKvpP.Key.ToLower()];
            if (botToUpdate is null)
            {
                _logger.Warning(_localisationService.GetText("gameevent-bot_not_found", botKvpP));
                continue;
            }

            // Iterate over each loot slot change
            var lootAmendmentsBySlot = botLootChanges[botKvpP.Key];
            foreach (var slotKvP in lootAmendmentsBySlot)
            {
                // Grab loot to add and loop over it
                var itemTplsToAdd = slotKvP.Value;
                foreach (var itemKvP in itemTplsToAdd)
                {
                    var dict = botToUpdate.BotInventory.Items.GetAllPropsAsDict();
                    dict[itemKvP.Key] = itemTplsToAdd[itemKvP.Key];
                }
            }
        }
    }

    /// <summary>
    ///     Add pumpkin loot boxes to scavs
    /// </summary>
    protected void AddPumpkinsToScavBackpacks()
    {
        _databaseService.GetBots()
            .Types["assault"]
            .BotInventory.Items.Backpack[
                ItemTpl.RANDOMLOOTCONTAINER_PUMPKIN_RAND_LOOT_CONTAINER
            ] = 400;
    }

    protected void RenameBitcoin()
    {
        var enLocale = _databaseService.GetLocales().Global["en"];
        enLocale.Value[$"{ItemTpl.BARTER_PHYSICAL_BITCOIN} Name"] = "Physical SPT Coin";
        enLocale.Value[$"{ItemTpl.BARTER_PHYSICAL_BITCOIN} ShortName"] = "0.2SPT";
    }

    /// <summary>
    ///     Set Khorovod(dancing tree) chance to 100% on all maps that support it
    /// </summary>
    protected void EnableDancingTree()
    {
        var maps = _databaseService.GetLocations();
        HashSet<string> mapsToCheck = ["hideout", "base", "privatearea"];
        foreach (var mapKvP in maps.GetDictionary())
        {
            // Skip maps that have no tree
            if (mapsToCheck.Contains(mapKvP.Key))
            {
                continue;
            }

            var mapData = mapKvP.Value;
            if (mapData.Base?.Events?.Khorovod?.Chance is not null)
            {
                mapData.Base.Events.Khorovod.Chance = 100;
                mapData.Base.BotLocationModifier.KhorovodChance = 100;
            }
        }
    }

    /// <summary>
    ///     Add santa to maps
    /// </summary>
    protected void AddGifterBotToMaps()
    {
        var gifterSettings = _seasonalEventConfig.GifterSettings;
        var maps = _databaseService.GetLocations().GetDictionary();
        foreach (var gifterMapSettings in gifterSettings)
        {
            if (!maps.TryGetValue(_databaseService.GetLocations().GetMappedKey(gifterMapSettings.Map), out var mapData))
            {
                _logger.Warning($"AddGifterBotToMaps() Map not found {gifterMapSettings.Map}");

                continue;
            }

            // Don't add gifter to map twice
            var existingGifter = mapData.Base.BossLocationSpawn.FirstOrDefault(boss => boss.BossName == "gifter");
            if (existingGifter is not null)
            {
                existingGifter.BossChance = gifterMapSettings.SpawnChance;

                continue;
            }

            mapData.Base.BossLocationSpawn.Add(
                new BossLocationSpawn
                {
                    BossName = "gifter",
                    BossChance = gifterMapSettings.SpawnChance,
                    BossZone = gifterMapSettings.Zones,
                    IsBossPlayer = false,
                    BossDifficulty = "normal",
                    BossEscortType = "gifter",
                    BossEscortDifficulty = "normal",
                    BossEscortAmount = "0",
                    ForceSpawn = true,
                    SpawnMode = ["regular", "pve"],
                    Time = -1,
                    TriggerId = "",
                    TriggerName = "",
                    Delay = 0,
                    IsRandomTimeSpawn = false,
                    IgnoreMaxBots = true
                }
            );
        }
    }

    protected void HandleModEvent(SeasonalEvent seasonalEvent, Config globalConfig)
    {
        if (seasonalEvent.Settings?.EnableChristmasHideout ?? false)
        {
            globalConfig.EventType = globalConfig.EventType.Where(x => x != EventType.None).ToList();
            globalConfig.EventType.Add(EventType.Christmas);
        }

        if (seasonalEvent.Settings?.EnableHalloweenHideout ?? false)
        {
            globalConfig.EventType = globalConfig.EventType.Where(x => x != EventType.None).ToList();
            globalConfig.EventType.Add(EventType.Halloween);
            globalConfig.EventType.Add(EventType.HalloweenIllumination);
        }

        if (seasonalEvent.Settings?.AddEventGearToBots ?? false)
        {
            AddEventGearToBots(seasonalEvent.Type);
        }

        if (seasonalEvent.Settings?.AddEventLootToBots ?? false)
        {
            AddEventLootToBots(seasonalEvent.Type);
        }

        if (seasonalEvent.Settings?.EnableSummoning ?? false)
        {
            EnableHalloweenSummonEvent();
            AddEventBossesToMaps("halloweensummon");
        }

        if (seasonalEvent.Settings?.ZombieSettings?.Enabled ?? false)
        {
            ConfigureZombies(seasonalEvent.Settings.ZombieSettings);
        }

        if (seasonalEvent.Settings?.ForceSeason != null)
        {
            _weatherConfig.OverrideSeason = seasonalEvent.Settings.ForceSeason;
        }

        if (seasonalEvent.Settings?.AdjustBotAppearances ?? false)
        {
            AdjustBotAppearanceValues(seasonalEvent.Type);
        }
    }

    /// <summary>
    ///     Send gift to player if they have not already received it
    /// </summary>
    /// <param name="playerId">Player to send gift to</param>
    /// <param name="giftKey">Key of gift to give</param>
    protected void GiveGift(string playerId, string giftKey)
    {
        var giftData = _giftService.GetGiftById(giftKey);
        if (!_profileHelper.PlayerHasRecievedMaxNumberOfGift(playerId, giftKey, giftData.MaxToSendPlayer ?? 5))
        {
            _giftService.SendGiftToPlayer(playerId, giftKey);
        }
    }

    /// <summary>
    ///     Get the underlying bot type for an event bot e.g. `peacefullZryachiyEvent` will return `bossZryachiy`
    /// </summary>
    /// <param name="eventBotRole">Event bot role type</param>
    /// <returns>Bot role as string</returns>
    public string GetBaseRoleForEventBot(string? eventBotRole)
    {
        return _seasonalEventConfig.EventBotMapping.GetValueOrDefault(eventBotRole, null);
    }

    /// <summary>
    ///     Force the weather to be snow
    /// </summary>
    public void EnableSnow()
    {
        _weatherConfig.OverrideSeason = Season.WINTER;
    }
}
