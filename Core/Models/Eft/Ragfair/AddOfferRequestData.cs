using System.Text.Json.Serialization;

namespace Core.Models.Eft.Ragfair;

public record AddOfferRequestData
{
    [JsonPropertyName("Action")]
    public string? Action { get; set; }

    [JsonPropertyName("sellInOnePiece")]
    public bool? SellInOnePiece { get; set; }

    [JsonPropertyName("items")]
    public List<string>? Items { get; set; }

    [JsonPropertyName("requirements")]
    public List<Requirement>? Requirements { get; set; }
}

public record Requirement
{
    [JsonPropertyName("_tpl")]
    public string? Template { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("side")]
    public int? Side { get; set; }

    [JsonPropertyName("onlyFunctional")]
    public bool? OnlyFunctional { get; set; }
}
