using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Models.Eft.Hideout;

public record HideoutUpgradeRequestData : BaseInteractionRequestData
{

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("items")]
    public List<HideoutItem>? Items { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}
