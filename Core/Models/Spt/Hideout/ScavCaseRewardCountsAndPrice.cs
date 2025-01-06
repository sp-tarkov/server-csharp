using System.Text.Json.Serialization;

namespace Types.Models.Spt.Hideout;

public class ScavCaseRewardCountsAndPrice
{
    [JsonPropertyName("Common")]
    public RewardCountAndPriceDetails Common { get; set; }
    
    [JsonPropertyName("Rare")]
    public RewardCountAndPriceDetails Rare { get; set; }
    
    [JsonPropertyName("Superrare")]
    public RewardCountAndPriceDetails Superrare { get; set; }
}

public class RewardCountAndPriceDetails
{
    [JsonPropertyName("minCount")]
    public int MinCount { get; set; }
    
    [JsonPropertyName("maxCount")]
    public int MaxCount { get; set; }
    
    [JsonPropertyName("minPriceRub")]
    public int MinPriceRub { get; set; }
    
    [JsonPropertyName("maxPriceRub")]
    public int MaxPriceRub { get; set; }
}