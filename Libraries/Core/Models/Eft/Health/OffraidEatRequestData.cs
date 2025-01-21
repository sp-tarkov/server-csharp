using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Health;

public record OffraidEatRequestData : BaseInteractionRequestData
{
    [JsonPropertyName("item")]
    public string? Item { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("time")]
    public int? Time { get; set; }
}
