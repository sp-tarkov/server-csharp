using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.RaidSettings;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record BotSettings
{
    [JsonPropertyName("isScavWars")]
    public bool? IsScavWars
    {
        get;
        set;
    }

    [JsonPropertyName("botAmount")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BotAmount? BotAmount
    {
        get;
        set;
    }
}
