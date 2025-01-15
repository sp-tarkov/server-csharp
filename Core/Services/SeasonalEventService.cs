using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class SeasonalEventService
{
    private readonly ILogger _logger;

    private readonly DatabaseService _databaseService;

    //private readonly DatabaseImporter _databaseImporter;
    private readonly GiftService _giftService;
    private readonly LocalisationService _localisationService;
    private readonly BotHelper _botHelper;
    private readonly ProfileHelper _profileHelper;
    private readonly ConfigServer _configServer;

    private bool _christmasEventActive = false;
    private bool _halloweenEventActive = false;

    private readonly SeasonalEventConfig _seasonalEventConfig;
    private readonly QuestConfig _questConfig;
    private readonly HttpConfig _httpConfig;
    private readonly WeatherConfig _weatherConfig;
    private readonly LocationConfig _locationConfig;

    private List<SeasonalEvent> _currentlyActiveEvents = [];

    private readonly IReadOnlyList<string> _christmasEventItems =
    [
        ItemTpl.FACECOVER_FAKE_WHITE_BEARD,
        ItemTpl.BARTER_CHRISTMAS_TREE_ORNAMENT_RED,
        ItemTpl.BARTER_CHRISTMAS_TREE_ORNAMENT_VIOLET,
        ItemTpl.BARTER_CHRISTMAS_TREE_ORNAMENT_SILVER,
        ItemTpl.HEADWEAR_DED_MOROZ_HAT,
        ItemTpl.HEADWEAR_SANTA_HAT,
        ItemTpl.BACKPACK_SANTAS_BAG,
        ItemTpl.RANDOMLOOTCONTAINER_NEW_YEAR_GIFT_BIG,
        ItemTpl.RANDOMLOOTCONTAINER_NEW_YEAR_GIFT_MEDIUM,
        ItemTpl.RANDOMLOOTCONTAINER_NEW_YEAR_GIFT_SMALL
    ];

    private readonly IReadOnlyList<string> _halloweenEventItems =
    [
        ItemTpl.FACECOVER_SPOOKY_SKULL_MASK,
        ItemTpl.RANDOMLOOTCONTAINER_PUMPKIN_RAND_LOOT_CONTAINER,
        ItemTpl.HEADWEAR_JACKOLANTERN_TACTICAL_PUMPKIN_HELMET,
        ItemTpl.FACECOVER_FACELESS_MASK,
        ItemTpl.FACECOVER_JASON_MASK,
        ItemTpl.FACECOVER_MISHA_MAYOROV_MASK,
        ItemTpl.FACECOVER_SLENDER_MASK,
        ItemTpl.FACECOVER_GHOUL_MASK,
        ItemTpl.FACECOVER_HOCKEY_PLAYER_MASK_CAPTAIN,
        ItemTpl.FACECOVER_HOCKEY_PLAYER_MASK_BRAWLER,
        ItemTpl.FACECOVER_HOCKEY_PLAYER_MASK_QUIET
    ];

    public SeasonalEventService
    (
        ILogger logger,
        DatabaseService databaseService,
        //DatabaseImporter databaseImporter,
        GiftService giftService,
        LocalisationService localisationService,
        BotHelper botHelper,
        ProfileHelper profileHelper,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _databaseService = databaseService;
        //_databaseImporter = databaseImporter;
        _giftService = giftService;
        _localisationService = localisationService;
        _botHelper = botHelper;
        _profileHelper = profileHelper;
        _configServer = configServer;

        _seasonalEventConfig = _configServer.GetConfig<SeasonalEventConfig>();
        _questConfig = _configServer.GetConfig<QuestConfig>();
        _httpConfig = _configServer.GetConfig<HttpConfig>();
        _weatherConfig = _configServer.GetConfig<WeatherConfig>();
        _locationConfig = _configServer.GetConfig<LocationConfig>();
    }

    /// <summary>
    /// Get an array of christmas items found in bots inventories as loot
    /// </summary>
    /// <returns>array</returns>
    public IEnumerable<string> GetChristmasEventItems()
    {
        return _christmasEventItems;
    }

    /// <summary>
    /// Get an array of halloween items found in bots inventories as loot
    /// </summary>
    /// <returns>array</returns>
    public IEnumerable<string> GetHalloweenEventItems()
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
    /// Check if item id exists in christmas or halloween event arrays
    /// </summary>
    /// <param name="itemTpl">item tpl to check for</param>
    /// <returns></returns>
    public bool ItemIsSeasonalRelated(string itemTpl)
    {
        return _christmasEventItems.Contains(itemTpl) || _halloweenEventItems.Contains(itemTpl);
    }

    /// <summary>
    /// Get active seasonal events
    /// </summary>
    /// <returns>Array of active events</returns>
    public List<SeasonalEvent> GetActiveEvents()
    {
        return _currentlyActiveEvents;
    }

    /// <summary>
    /// Get an array of seasonal items that should not appear
    /// e.g. if halloween is active, only return christmas items
    /// or, if halloween and christmas are inactive, return both sets of items
    /// </summary>
    /// <returns>array of tpl strings</returns>
    public List<string> GetInactiveSeasonalEventItems()
    {
        var items = new List<string>();
        if (!ChristmasEventEnabled())
        {
            items.AddRange(_christmasEventItems);
        }

        if (!HalloweenEventEnabled())
        {
            items.AddRange(_halloweenEventItems);
        }

        return items;
    }

    /// <summary>
    /// Is a seasonal event currently active
    /// </summary>
    /// <returns>true if event is active</returns>
    public bool SeasonalEventEnabled()
    {
        return _christmasEventActive || _halloweenEventActive;
    }

    /// <summary>
    /// Is christmas event active
    /// </summary>
    /// <returns>true if active</returns>
    public bool ChristmasEventEnabled()
    {
        return _christmasEventActive;
    }

    /// <summary>
    /// is halloween event active
    /// </summary>
    /// <returns>true if active</returns>
    public bool HalloweenEventEnabled()
    {
        return _halloweenEventActive;
    }

    /// <summary>
    /// Is detection of seasonal events enabled (halloween / christmas)
    /// </summary>
    /// <returns>true if seasonal events should be checked for</returns>
    public bool IsAutomaticEventDetectionEnabled()
    {
        return _seasonalEventConfig.EnableSeasonalEventDetection;
    }

    /// <summary>
    /// Get a dictionary of gear changes to apply to bots for a specific event e.g. Christmas/Halloween
    /// </summary>
    /// <param name="eventName">Name of event to get gear changes for</param>
    /// <returns>bots with equipment changes</returns>
    protected Dictionary<string, Dictionary<string, Dictionary<string, int>>> GetEventBotGear(SeasonalEventType eventType)
    {
        return _seasonalEventConfig.EventGear.GetValueOrDefault(eventType, null);
    }

    /// <summary>
    /// Get a dictionary of loot changes to apply to bots for a specific event e.g. Christmas/Halloween
    /// </summary>
    /// <param name="eventName">Name of event to get gear changes for</param>
    /// <returns>bots with loot changes</returns>
    protected Dictionary<string, Dictionary<string, Dictionary<string, int>>> GetEventBotLoot(SeasonalEventType eventType)
    {
        return _seasonalEventConfig.EventLoot.GetValueOrDefault(eventType, null);
    }

    /// <summary>
    /// Get the dates each seasonal event starts and ends at
    /// </summary>
    /// <returns>Record with event name + start/end date</returns>
    public List<SeasonalEvent> GetEventDetails()
    {
        return _seasonalEventConfig.Events;
    }

    /// <summary>
    /// Look up quest in configs/quest.json
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
    /// Handle activating seasonal events
    /// </summary>
    public void EnableSeasonalEvents()
    {
        if (_currentlyActiveEvents.Count > 0)
        {
            var globalConfig = _databaseService.GetGlobals().Configuration;
            foreach (var activeEvent in _currentlyActiveEvents)
            {
                UpdateGlobalEvents(globalConfig, activeEvent);
            }
        }
    }

    /// <summary>
    /// Force a seasonal event to be active
    /// </summary>
    /// <param name="eventType">Event to force active</param>
    /// <returns>True if event was successfully force enabled</returns>
    public bool ForceSeasonalEvent(SeasonalEventType eventType)
    {
        var globalConfig = _databaseService.GetGlobals().Configuration;
        var seasonEvent = _seasonalEventConfig.Events.FirstOrDefault((e) => e.Type == eventType);
        if (seasonEvent is null)
        {
            _logger.Warning($"Unable to force event: {eventType} as it cannot be found in events config");
            return false;
        }

        UpdateGlobalEvents(globalConfig, seasonEvent);

        return true;
    }

    /// <summary>
    /// Store active events inside class list property `currentlyActiveEvents` + set class properties: christmasEventActive/halloweenEventActive
    /// </summary>
    public void CacheActiveEvents()
    {
        var currentDate = DateTimeOffset.UtcNow.DateTime;
        var seasonalEvents = GetEventDetails();

        // reset existing data
        _currentlyActiveEvents = new();

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
    /// Get the currently active weather season e.g. SUMMER/AUTUMN/WINTER
    /// </summary>
    /// <returns>Season enum value</returns>
    public Season GetActiveWeatherSeason()
    {
        if (_weatherConfig.OverrideSeason.HasValue)
        {
            return _weatherConfig.OverrideSeason.Value;
        }

        var currentDate = new DateTime();
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
    /// Does the provided date fit between the two defined dates?
    /// Excludes year
    /// Inclusive of end date upto 23 hours 59 minutes
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
    /// Iterate through bots inventory and loot to find and remove christmas items (as defined in SeasonalEventService)
    /// </summary>
    /// <param name="botInventory">Bots inventory to iterate over</param>
    /// <param name="botRole">the role of the bot being processed</param>
    public void RemoveChristmasItemsFromBotInventory(BotTypeInventory botInventory, string botRole)
    {
        var christmasItems = GetChristmasEventItems();
        List<EquipmentSlots> equipmentSlotsToFilter = [EquipmentSlots.FaceCover, EquipmentSlots.Headwear, EquipmentSlots.Backpack, EquipmentSlots.TacticalVest];
        List<string> lootContainersToFilter = ["Backpack", "Pockets", "TacticalVest"];

        // Remove christmas related equipment
        foreach (var equipmentSlotKey in equipmentSlotsToFilter)
        {
            if (botInventory.Equipment[equipmentSlotKey] is null)
            {
                _logger.Warning(
                    _localisationService.GetText("seasonal-missing_equipment_slot_on_bot", new
                    {
                        EquipmentSlot = equipmentSlotKey,
                        BotRole = botRole,
                    })
                );
            }

            Dictionary<string, double> equipment = botInventory.Equipment[equipmentSlotKey];
            botInventory.Equipment[equipmentSlotKey] = equipment.Where(i => !_christmasEventItems.Contains(i.Key)).ToDictionary();
        }

        // Remove christmas related loot from loot containers
        var props = botInventory.Items.GetType().GetProperties();
        foreach (var lootContainerKey in lootContainersToFilter)
        {
            var prop = (Dictionary<string, double>?)props.FirstOrDefault(p => p.Name.ToLower() == lootContainerKey.ToLower()).GetValue(botInventory.Items);

            if (prop is null)
            {
                _logger.Warning(
                    _localisationService.GetText("seasonal-missing_loot_container_slot_on_bot", new
                    {
                        LootContainer = lootContainerKey,
                        BotRole = botRole,
                    })
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
            if (nonChristmasTpls.Count() == 0)
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
    /// Make adjusted to server code based on the name of the event passed in
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
                AddEventBossesToMaps(SeasonalEventType.Halloween.ToString());
                EnableHalloweenSummonEvent();
                AddPumpkinsToScavBackpacks();
                RenameBitcoin();
                EnableSnow();
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

        globalConfig.EventType = globalConfig.EventType.Where((x) => x != "None").ToList();
        globalConfig.EventType.Add("Halloween");
        globalConfig.EventType.Add("HalloweenIllumination");
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
            globalConfig.EventType = globalConfig.EventType.Where((x) => x != "None").ToList();
            globalConfig.EventType.Add("Christmas");
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
            globalConfig.EventType = globalConfig.EventType.Where((x) => x != "None").ToList();
            globalConfig.EventType.Add("Christmas");
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
                    var prop = props.FirstOrDefault(x => x.Name.ToLower() == appearanceKey.Key.ToLower());
                    var propValue = (Dictionary<string, int>)prop.GetValue(botDb.BotAppearance);
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
        var useDefault = hostilitySettings is null;

        var props = locations.GetType().GetProperties();

        foreach (var locationProp in props)
        {
            if (ignoreList.Contains(locationProp.Name))
            {
                continue;
            }

            Location location = (Location)locationProp.GetValue(locations);
            if (location?.Base?.BotLocationModifier?.AdditionalHostilitySettings is null)
            {
                continue;
            }

            List<AdditionalHostilitySettings> newHostilitySettings = useDefault ? new() : hostilitySettings[locationProp.Name];
            if (newHostilitySettings is null)
            {
                continue;
            }

            location.Base.BotLocationModifier.AdditionalHostilitySettings = new();
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
    /// Force zryachiy to always have a melee weapon
    /// </summary>
    protected void AdjustZryachiyMeleeChance()
    {
        var zyrach = _databaseService.GetBots().Types.FirstOrDefault(x => x.Key.ToLower() == "bosszryachiy");
        var value = new Dictionary<string, double>();

        foreach (var chance in zyrach.Value.BotChances.EquipmentChances)
        {
            if (chance.Key.ToLower() == "Scabbard")
            {
                value.Add(chance.Key, 100);
                continue;
            }

            value.Add(chance.Key, chance.Value);
        }

        zyrach.Value.BotChances.EquipmentChances = value;
    }

    /// <summary>
    /// Enable the halloween zryachiy summon event
    /// </summary>
    protected void EnableHalloweenSummonEvent()
    {
        _databaseService.GetGlobals().Configuration.EventSettings.EventActive = true;
    }

    protected void ConfigureZombies(ZombieSettings zombieSettings)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get location ids of maps with an infection above 0
    /// </summary>
    /// <param name="locationInfections">Dict of locations with their infection percentage</param>
    /// <returns>List of location ids</returns>
    protected List<string> GetLocationsWithZombies(Dictionary<string, double> locationInfections)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// BSG store the location ids differently inside `LocationInfection`, need to convert to matching location IDs
    /// </summary>
    /// <param name="infectedLocationKey">Key to convert</param>
    /// <returns>List of locations</returns>
    protected List<string> GetLocationFromInfectedLocation(string infectedLocationKey)
    {
        throw new NotImplementedException();
    }

    protected void AddEventWavesToMaps(string eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add event bosses to maps
    /// </summary>
    /// <param name="eventType">Seasonal event, e.g. HALLOWEEN/CHRISTMAS</param>
    /// <param name="mapIdWhitelist">OPTIONAL - Maps to add bosses to</param>
    protected void AddEventBossesToMaps(string eventType, List<string> mapIdWhitelist = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Change trader icons to be more event themed (Halloween only so far)
    /// </summary>
    /// <param name="eventType">What event is active</param>
    protected void AdjustTraderIcons(SeasonalEventType eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add lootble items from backpack into patrol.ITEMS_TO_DROP difficulty property
    /// </summary>
    protected void AddLootItemsToGifterDropItemsList()
    {
        var gifterBot = _databaseService.GetBots().Types["gifter"];
        var items = gifterBot.BotInventory.Items.Backpack.Keys.ToList();
        gifterBot.BotDifficulty.Easy.Patrol["ITEMS_TO_DROP"] = items;
        gifterBot.BotDifficulty.Normal.Patrol["ITEMS_TO_DROP"] = items;
        gifterBot.BotDifficulty.Hard.Patrol["ITEMS_TO_DROP"] = items;
        gifterBot.BotDifficulty.Impossible.Patrol["ITEMS_TO_DROP"] = items;
    }

    /// <summary>
    /// Read in data from seasonalEvents.json and add found equipment items to bots
    /// </summary>
    /// <param name="eventType">Name of the event to read equipment in from config</param>
    protected void AddEventGearToBots(SeasonalEventType eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Read in data from seasonalEvents.json and add found loot items to bots
    /// </summary>
    /// <param name="eventType">Name of the event to read loot in from config</param>
    protected void AddEventLootToBots(SeasonalEventType eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add pumpkin loot boxes to scavs
    /// </summary>
    protected void AddPumpkinsToScavBackpacks()
    {
        _databaseService.GetBots().Types["assault"].BotInventory.Items.Backpack[
            ItemTpl.RANDOMLOOTCONTAINER_PUMPKIN_RAND_LOOT_CONTAINER
        ] = 400;
    }

    protected void RenameBitcoin()
    {
        var enLocale = _databaseService.GetLocales().Global["en"];
        enLocale[$"{ItemTpl.BARTER_PHYSICAL_BITCOIN} Name"] = "Physical SPT Coin";
        enLocale[$"{ItemTpl.BARTER_PHYSICAL_BITCOIN} ShortName"] = "0.2SPT";
    }

    /// <summary>
    /// Set Khorovod(dancing tree) chance to 100% on all maps that support it
    /// </summary>
    protected void EnableDancingTree()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add santa to maps
    /// </summary>
    protected void AddGifterBotToMaps()
    {
        throw new NotImplementedException();
    }

    protected void HandleModEvent(SeasonalEvent seasonalEvent, Config globalConfig)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send gift to player if they have not already received it
    /// </summary>
    /// <param name="playerId">Player to send gift to</param>
    /// <param name="giftKey">Key of gift to give</param>
    protected void GiveGift(string playerId, string giftKey)
    {
        var gitftData = _giftService.GetGiftById(giftKey);
        if (!_profileHelper.PlayerHasRecievedMaxNumberOfGift(playerId, giftKey, gitftData.MaxToSendPlayer ?? 5))
        {
            _giftService.SendGiftToPlayer(playerId, giftKey);
        }
    }

    /// <summary>
    /// Get the underlying bot type for an event bot e.g. `peacefullZryachiyEvent` will return `bossZryachiy`
    /// </summary>
    /// <param name="eventBotRole">Event bot role type</param>
    /// <returns>Bot role as string</returns>
    public string GetBaseRoleForEventBot(string eventBotRole)
    {
        return _seasonalEventConfig.EventBotMapping.GetValueOrDefault(eventBotRole, null);
    }

    /// <summary>
    /// Force the weather to be snow
    /// </summary>
    public void EnableSnow()
    {
        _weatherConfig.OverrideSeason = Season.WINTER;
    }
}
