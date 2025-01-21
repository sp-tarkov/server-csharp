using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record HideoutUpgradeCompleteRequestData : BaseInteractionRequestData
{

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
