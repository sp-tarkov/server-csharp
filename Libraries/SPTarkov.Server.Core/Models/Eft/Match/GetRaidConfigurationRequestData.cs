using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record GetRaidConfigurationRequestData : RaidSettings, IRequestData
{
    [JsonPropertyName("keyId")]
    public string? KeyId
    {
        get;
        set;
    }

    [JsonPropertyName("onlinePveRaidStates")]
    public Dictionary<string, bool>? OnlinePveRaidStates
    {
        get;
        set;
    }

    [JsonPropertyName("MaxGroupCount")]
    public int? MaxGroupCount
    {
        get;
        set;
    }

    [JsonPropertyName("transitionType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransitionType TransitionType
    {
        get;
        set;
    }

    /// <summary>
    /// Custom property that is not received from or sent to the client.
    /// We calculate this once based on the time slot selected for the raid to use it during inventory generation.
    /// </summary>
    [JsonIgnore]
    public bool IsNightRaid
    {
        get;
        set;
    }
}
