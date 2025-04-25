using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Trade;

public record SoldItem
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("count")]
    public int? Count
    {
        get;
        set;
    }

    [JsonPropertyName("scheme_id")]
    public int? SchemeId
    {
        get;
        set;
    }
}
