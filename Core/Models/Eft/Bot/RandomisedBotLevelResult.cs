using System.Text.Json.Serialization;

namespace Core.Models.Eft.Bot;

public class RandomisedBotLevelResult
{
    [JsonPropertyName("level")]
    public double? Level { get; set; }

    [JsonPropertyName("exp")]
    public double? Exp { get; set; }
}
