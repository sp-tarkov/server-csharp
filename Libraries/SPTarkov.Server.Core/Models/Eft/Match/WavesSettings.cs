using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.RaidSettings;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record WavesSettings
{
    [JsonPropertyName("botAmount")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BotAmount? BotAmount
    {
        get;
        set;
    }

    [JsonPropertyName("botDifficulty")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BotDifficulty? BotDifficulty
    {
        get;
        set;
    }

    [JsonPropertyName("isBosses")]
    public bool? IsBosses
    {
        get;
        set;
    }

    [JsonPropertyName("isTaggedAndCursed")]
    public bool? IsTaggedAndCursed
    {
        get;
        set;
    }
}
