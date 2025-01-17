using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Profile;

public record ProfileChangeVoiceRequestData : IRequestData
{
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }
}
