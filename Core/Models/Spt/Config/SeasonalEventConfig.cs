using System.Text.Json.Serialization;
using Core.Models.Eft.Common;

namespace Core.Models.Spt.Config;

public class SeasonalEventConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-seasonalevents";
    
    [JsonPropertyName("enableSeasonalEventDetection")]
    public bool EnableSeasonalEventDetection { get; set; }
    
    /** event / botType / equipSlot / itemid */
    [JsonPropertyName("eventGear")]
    public Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> EventGear { get; set; }
    
    /** event / bot type / equipSlot / itemid */
    [JsonPropertyName("eventLoot")]
    public Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> EventLoot { get; set; }
    
    public List<SeasonalEvent> Events { get; set; }
    
    [JsonPropertyName("eventBotMapping")]
    public Dictionary<string, string> EventBotMapping { get; set; }
    
    [JsonPropertyName("eventBossSpawns")]
    public Dictionary<string, Dictionary<string, List<BossLocationSpawn>>> EventBossSpawns { get; set; }
    
    [JsonPropertyName("eventWaves")]
    public Dictionary<string, Dictionary<string, List<Wave>>> EventWaves { get; set; }
    
    [JsonPropertyName("gifterSettings")]
    public List<GifterSetting> GifterSettings { get; set; }
    
    /** key = event, second key = map name */
    [JsonPropertyName("hostilitySettingsForEvent")]
    public Dictionary<string, Dictionary<string, List<AdditionalHostilitySettings>>> HostilitySettingsForEvent { get; set; }
    
    /** Ids of containers on locations that only have christmas loot */
    [JsonPropertyName("christmasContainerIds")]
    public List<string> ChristmasContainerIds { get; set; }
    
    /** Season - botType - location (body/feet/hands/head) */
    [JsonPropertyName("botAppearanceChanges")]
    public Dictionary<SeasonalEventType, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> BotAppearanceChanges { get; set; }
}

public class SeasonalEvent
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("type")]
    public SeasonalEventType Type { get; set; }
    
    [JsonPropertyName("startDay")]
    public int StartDay { get; set; }
    
    [JsonPropertyName("startMonth")]
    public int StartMonth { get; set; }
    
    [JsonPropertyName("endDay")]
    public int EndDay { get; set; }
    
    [JsonPropertyName("endMonth")]
    public int EndMonth { get; set; }
    
    [JsonPropertyName("settings")]
    public Dictionary<string, SeasonalEventSettings> Settings { get; set; }
}

public class SeasonalEventSettings
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
}

public class ZombieSettings : SeasonalEventSettings
{
    [JsonPropertyName("mapInfectionAmount")]
    public Dictionary<string, int> MapInfectionAmount { get; set; }
    
    [JsonPropertyName("disableBosses")]
    public List<string> DisableBosses { get; set; }
    
    [JsonPropertyName("disableWaves")]
    public List<string> DisableWaves { get; set; }
}

public class GifterSetting
{
    [JsonPropertyName("map")]
    public string Map { get; set; }
    
    [JsonPropertyName("zones")]
    public string Zones { get; set; }
    
    [JsonPropertyName("spawnChance")]
    public int SpawnChance { get; set; }
}