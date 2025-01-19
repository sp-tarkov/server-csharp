using System.Text.Json.Serialization;
using Core.Models.Utils;

namespace Core.Models.Eft.Profile;

public record ProfileCreateRequestData : IRequestData
{
    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("headId")]
    public string? HeadId { get; set; }

    [JsonPropertyName("voiceId")]
    public string? VoiceId { get; set; }

    [JsonPropertyName("sptForcePrestigeLevel")]
    public double? SptForcePrestigeLevel { get; set; }
}
