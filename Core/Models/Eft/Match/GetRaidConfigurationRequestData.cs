using System.Text.Json.Serialization;

namespace Core.Models.Eft.Match;

public class GetRaidConfigurationRequestData : RaidSettings
{
    [JsonPropertyName("keyId")]
    public string? KeyId { get; set; }

    [JsonPropertyName("MaxGroupCount")]
    public int? MaxGroupCount { get; set; }
}