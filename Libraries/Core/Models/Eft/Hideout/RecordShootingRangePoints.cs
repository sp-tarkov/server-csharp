using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Request;

namespace Core.Models.Eft.Hideout;

public record RecordShootingRangePoints : BaseInteractionRequestData
{
    [JsonPropertyName("points")]
    public int? Points { get; set; }
}
