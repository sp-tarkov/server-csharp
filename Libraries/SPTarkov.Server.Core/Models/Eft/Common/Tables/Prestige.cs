using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Prestige
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("elements")]
    public List<PrestigeElement>? Elements
    {
        get;
        set;
    }
}

public record PrestigeElement
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("conditions")]
    public List<QuestCondition>? Conditions
    {
        get;
        set;
    }

    [JsonPropertyName("rewards")]
    public List<Reward>? Rewards
    {
        get;
        set;
    }

    [JsonPropertyName("transferConfigs")]
    public TransferConfigs? TransferConfigs
    {
        get;
        set;
    }

    [JsonPropertyName("image")]
    public string? Image
    {
        get;
        set;
    }

    [JsonPropertyName("bigImage")]
    public string? BigImage
    {
        get;
        set;
    }
}

public record TransferConfigs
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("stashConfig")]
    public StashPrestigeConfig? StashConfig
    {
        get;
        set;
    }

    [JsonPropertyName("skillConfig")]
    public PrestigeSkillConfig? SkillConfig
    {
        get;
        set;
    }

    [JsonPropertyName("masteringConfig")]
    public PrestigeMasteringConfig? MasteringConfig
    {
        get;
        set;
    }
}

public record StashPrestigeConfig
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("xCellCount")]
    public int? XCellCount
    {
        get;
        set;
    }

    [JsonPropertyName("yCellCount")]
    public int? YCellCount
    {
        get;
        set;
    }

    [JsonPropertyName("filters")]
    public StashPrestigeFilters? Filters
    {
        get;
        set;
    }
}

public record StashPrestigeFilters
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("includedItems")]
    public List<string>? IncludedItems
    {
        get;
        set;
    }

    [JsonPropertyName("excludedItems")]
    public List<string>? ExcludedItems
    {
        get;
        set;
    }
}

public record PrestigeSkillConfig
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("transferMultiplier")]
    public double? TransferMultiplier
    {
        get;
        set;
    }
}

public record PrestigeMasteringConfig
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("transferMultiplier")]
    public double? TransferMultiplier
    {
        get;
        set;
    }
}
