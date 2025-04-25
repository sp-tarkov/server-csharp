using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record PrestigeElement
{
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
