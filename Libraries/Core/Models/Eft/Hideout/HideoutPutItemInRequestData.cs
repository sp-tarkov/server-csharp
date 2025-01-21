using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutPutItemInRequestData : BaseInteractionRequestData
{

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, ItemDetails>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

public record ItemDetails
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
