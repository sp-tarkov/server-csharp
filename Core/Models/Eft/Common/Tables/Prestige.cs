namespace Core.Models.Eft.Common.Tables;

using System.Text.Json.Serialization;

public class Prestige
{
    [JsonPropertyName("elements")]
    public PretigeElement? Elements { get; set; }
}

public class PretigeElement
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("conditions")]
    public List<QuestCondition>? Conditions { get; set; }
    
    [JsonPropertyName("rewards")]
    public List<QuestReward>? Rewards { get; set; }
    
    [JsonPropertyName("transferConfigs")]
    public TransferConfigs? TransferConfigs { get; set; }
    
    [JsonPropertyName("image")]
    public string? Image { get; set; }
    
    [JsonPropertyName("bigImage")]
    public string? BigImage { get; set; }
}

public class TransferConfigs
{
    [JsonPropertyName("stashConfig")]
    public StashPrestigeConfig? StashConfig { get; set; }
    
    [JsonPropertyName("skillConfig")]
    public PrestigeSkillConfig? SkillConfig { get; set; }
    
    [JsonPropertyName("masteringConfig")]
    public PrestigeMasteringConfig? MasteringConfig { get; set; }
}

public class StashPrestigeConfig
{
    [JsonPropertyName("xCellCount")]
    public int? XCellCount { get; set; }
    
    [JsonPropertyName("yCellCount")]
    public int? YCellCount { get; set; }
    
    [JsonPropertyName("filters")]
    public StashPrestigeFilters? Filters { get; set; }
}

public class StashPrestigeFilters
{
    [JsonPropertyName("includedItems")]
    public List<string>? IncludedItems { get; set; }
    
    [JsonPropertyName("excludedItems")]
    public List<string>? ExcludedItems { get; set; }
}

public class PrestigeSkillConfig
{
    [JsonPropertyName("transferMultiplier")]
    public int? TransferMultiplier { get; set; }
}

public class PrestigeMasteringConfig
{
    [JsonPropertyName("transferMultiplier")]
    public int? TransferMultiplier { get; set; }
}