using System.Text.Json.Serialization;

namespace Core.Models.Eft.Profile;

public class ProfileCreateRequestData
{
    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("headId")]
    public string? HeadId { get; set; }

    [JsonPropertyName("voiceId")]
    public string? VoiceId { get; set; }
}