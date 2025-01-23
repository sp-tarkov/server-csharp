using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutPutItemInRequestData : BaseInteractionRequestData
{

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, IdWithCount>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
