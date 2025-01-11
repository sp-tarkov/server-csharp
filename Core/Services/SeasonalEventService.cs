using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Config;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class SeasonalEventService
{
    /// <summary>
    /// Get an array of christmas items found in bots inventories as loot
    /// </summary>
    /// <returns>array</returns>
    public string[] GetChristmasEventItems()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get an array of halloween items found in bots inventories as loot
    /// </summary>
    /// <returns>array</returns>
    public string[] GetHalloweenEventItems()
    {
        throw new NotImplementedException();
    }

    public bool ItemIsChristmasRelated(string itemTpl)
    {
        throw new NotImplementedException();
    }

    public bool ItemIsHalloweenRelated(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if item id exists in christmas or halloween event arrays
    /// </summary>
    /// <param name="itemTpl">item tpl to check for</param>
    /// <returns></returns>
    public bool ItemIsSeasonalRelated(string itemTpl)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get active seasonal events
    /// </summary>
    /// <returns>Array of active events</returns>
    public List<SeasonalEvent> GetActiveEvents()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get an array of seasonal items that should not appear
    /// e.g. if halloween is active, only return christmas items
    /// or, if halloween and christmas are inactive, return both sets of items
    /// </summary>
    /// <returns>array of tpl strings</returns>
    public string[] GetInactiveSeasonalEventItems()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is a seasonal event currently active
    /// </summary>
    /// <returns>true if event is active</returns>
    public bool SeasonalEventEnabled()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is christmas event active
    /// </summary>
    /// <returns>true if active</returns>
    public bool ChristmasEventEnabled()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// is halloween event active
    /// </summary>
    /// <returns>true if active</returns>
    public bool HalloweenEventEnabled()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Is detection of seasonal events enabled (halloween / christmas)
    /// </summary>
    /// <returns>true if seasonal events should be checked for</returns>
    public bool IsAutomaticEventDetectionEnabled()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a dictionary of gear changes to apply to bots for a specific event e.g. Christmas/Halloween
    /// </summary>
    /// <param name="eventName">Name of event to get gear changes for</param>
    /// <returns>bots with equipment changes</returns>
    protected Dictionary<string, Dictionary<string, Dictionary<string, int>>> GetEventBotGear(SeasonalEventType eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a dictionary of loot changes to apply to bots for a specific event e.g. Christmas/Halloween
    /// </summary>
    /// <param name="eventName">Name of event to get gear changes for</param>
    /// <returns>bots with loot changes</returns>
    protected Dictionary<string, Dictionary<string, Dictionary<string, int>>> GetEventBotLoot(SeasonalEventType eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the dates each seasonal event starts and ends at
    /// </summary>
    /// <returns>Record with event name + start/end date</returns>
    public List<SeasonalEvent> GetEventDetails()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Look up quest in configs/quest.json
    /// </summary>
    /// <param name="questId">Quest to look up</param>
    /// <param name="event">event type (Christmas/Halloween/None)</param>
    /// <returns>true if related</returns>
    public bool IsQuestRelatedToEvent(string questId, SeasonalEventType eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handle activating seasonal events
    /// </summary>
    public void EnableSeasonalEvents()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Force a seasonal event to be active
    /// </summary>
    /// <param name="eventType">Event to force active</param>
    /// <returns>True if event was successfully force enabled</returns>
    public bool ForceSeasonalEvent(SeasonalEventType eventType)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Store active events inside class list property `currentlyActiveEvents` + set class properties: christmasEventActive/halloweenEventActive
    /// </summary>
    public void CacheActiveEvents()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the currently active weather season e.g. SUMMER/AUTUMN/WINTER
    /// </summary>
    /// <returns>Season enum value</returns>
    public Season GetActiveWeatherSeason()
    {
        throw new NotImplementedException();
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
    protected bool DateIsBetweenTwoDates(DateTime dateToCheck, int startMonth, int startDay, int endMonth, int endDay)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Iterate through bots inventory and loot to find and remove christmas items (as defined in SeasonalEventService)
    /// </summary>
    /// <param name="botInventory">Bots inventory to iterate over</param>
    /// <param name="botRole">the role of the bot being processed</param>
    public void RemoveChristmasItemsFromBotInventory(BotBaseInventory botInventory, string botRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Make adjusted to server code based on the name of the event passed in
    /// </summary>
    /// <param name="globalConfig">globals.json</param>
    /// <param name="event">Name of the event to enable. e.g. Christmas</param>
    protected void UpdateGlobalEvents(Config globalConfig, SeasonalEvent eventType)
    {
        throw new NotImplementedException();
    }

    protected void ApplyHalloweenEvent(SeasonalEvent eventType, Config globalConfig)
    {
        throw new NotImplementedException();
    }

    protected void ApplyChristmasEvent(SeasonalEvent eventType, Config globalConfig)
    {
        throw new NotImplementedException();
    }

    protected void ApplyNewYearsEvent(SeasonalEvent eventType, Config globalConfig)
    {
        throw new NotImplementedException();
    }

    protected void AdjustBotAppearanceValues(SeasonalEventType season)
    {
        throw new NotImplementedException();
    }

    protected void ReplaceBotHostility(Dictionary<string, AdditionalHostilitySettings[]> hostilitySettings)
    {
        throw new NotImplementedException();
    }

    protected void RemoveEntryRequirement(List<string> locationIds)
    {
        throw new NotImplementedException();
    }

    public void GivePlayerSeasonalGifts(string sessionId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Force zryachiy to always have a melee weapon
    /// </summary>
    protected void AdjustZryachiyMeleeChance()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Enable the halloween zryachiy summon event
    /// </summary>
    protected void EnableHalloweenSummonEvent()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    protected void RenameBitcoin()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the underlying bot type for an event bot e.g. `peacefullZryachiyEvent` will return `bossZryachiy`
    /// </summary>
    /// <param name="eventBotRole">Event bot role type</param>
    /// <returns>Bot role as string</returns>
    public string GetBaseRoleForEventBot(string eventBotRole)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Force the weather to be snow
    /// </summary>
    public void EnableSnow()
    {
        throw new NotImplementedException();
    }
}
