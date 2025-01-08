using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class ProfileStatusResponse
{
    [JsonPropertyName("maxPveCountExceeded")]
    public bool? MaxPveCountExceeded { get; set; }

    [JsonPropertyName("profiles")]
    public List<SessionStatus>? Profiles { get; set; }
}