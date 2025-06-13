using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Bot;

public record RandomisedBotLevelResult
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("level")]
    public int? Level
    {
        get;
        set;
    }

    [JsonPropertyName("exp")]
    public int? Exp
    {
        get;
        set;
    }
}
