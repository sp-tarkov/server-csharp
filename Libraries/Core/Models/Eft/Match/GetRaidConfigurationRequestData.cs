using System.Text.Json.Serialization;
using Core.Models.Enums;
using Core.Models.Utils;

namespace Core.Models.Eft.Match;

public record GetRaidConfigurationRequestData : RaidSettings, IRequestData
{
    [JsonPropertyName("keyId")]
    public string? KeyId { get; set; }

    [JsonPropertyName("MaxGroupCount")]
    public int? MaxGroupCount { get; set; }

    [JsonPropertyName("transitionType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransitionType TransitionType { get; set; }
}
