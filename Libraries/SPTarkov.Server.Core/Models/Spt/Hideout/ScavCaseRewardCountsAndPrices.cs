using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Hideout;

public record ScavCaseRewardCountsAndPrices
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("Common")]
    public RewardCountAndPriceDetails? Common
    {
        get;
        set;
    }

    [JsonPropertyName("Rare")]
    public RewardCountAndPriceDetails? Rare
    {
        get;
        set;
    }

    [JsonPropertyName("Superrare")]
    public RewardCountAndPriceDetails? Superrare
    {
        get;
        set;
    }
}

public record RewardCountAndPriceDetails
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("minCount")]
    public double? MinCount
    {
        get;
        set;
    }

    [JsonPropertyName("maxCount")]
    public double? MaxCount
    {
        get;
        set;
    }

    [JsonPropertyName("minPriceRub")]
    public double? MinPriceRub
    {
        get;
        set;
    }

    [JsonPropertyName("maxPriceRub")]
    public double? MaxPriceRub
    {
        get;
        set;
    }
}
