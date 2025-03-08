using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Inventory;

namespace SPTarkov.Server.Core.Models.Eft.Ragfair;

public record AddOfferRequestData : InventoryBaseActionRequestData
{
    [JsonPropertyName("sellInOnePiece")]
    public bool? SellInOnePiece
    {
        get;
        set;
    }

    [JsonPropertyName("items")]
    public List<string>? Items
    {
        get;
        set;
    }

    [JsonPropertyName("requirements")]
    public List<Requirement>? Requirements
    {
        get;
        set;
    }
}

public record Requirement
{
    [JsonPropertyName("_tpl")]
    public string? Template
    {
        get;
        set;
    }

    // Can be decimal value
    [JsonPropertyName("count")]
    public double? Count
    {
        get;
        set;
    }

    [JsonPropertyName("level")]
    public int? Level
    {
        get;
        set;
    }

    [JsonPropertyName("side")]
    public int? Side
    {
        get;
        set;
    }

    [JsonPropertyName("onlyFunctional")]
    public bool? OnlyFunctional
    {
        get;
        set;
    }
}
