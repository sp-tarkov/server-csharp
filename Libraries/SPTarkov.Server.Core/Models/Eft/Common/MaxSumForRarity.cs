using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MaxSumForRarity
{
    [JsonPropertyName("Common")]
    public RarityMaxSum? Common
    {
        get;
        set;
    }

    [JsonPropertyName("Rare")]
    public RarityMaxSum? Rare
    {
        get;
        set;
    }

    [JsonPropertyName("Superrare")]
    public RarityMaxSum? Superrare
    {
        get;
        set;
    }

    [JsonPropertyName("Not_exist")]
    public RarityMaxSum? NotExist
    {
        get;
        set;
    }
}
