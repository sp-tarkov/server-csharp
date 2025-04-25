using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record StartLocalRaidResponseData
{
    [JsonPropertyName("serverId")]
    public string? ServerId
    {
        get;
        set;
    }

    [JsonPropertyName("serverSettings")]
    public LocationServices? ServerSettings
    {
        get;
        set;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    [JsonPropertyName("profile")]
    public ProfileInsuredItems? Profile
    {
        get;
        set;
    }

    [JsonPropertyName("locationLoot")]
    public LocationBase? LocationLoot
    {
        get;
        set;
    }

    [JsonPropertyName("transitionType")]
    public TransitionType? TransitionType
    {
        get;
        set;
    }

    [JsonPropertyName("transition")]
    public Transition? Transition
    {
        get;
        set;
    }
}
