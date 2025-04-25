using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MaxActiveOfferCount
{
    [JsonPropertyName("from")]
    public double? From
    {
        get;
        set;
    }

    [JsonPropertyName("to")]
    public double? To
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public double? Count
    {
        get;
        set;
    }

    [JsonPropertyName("countForSpecialEditions")]
    public double? CountForSpecialEditions
    {
        get;
        set;
    }
}
