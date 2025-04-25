using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record QTELevelMultiplier
{
    [JsonPropertyName("Level")]
    public double? Level
    {
        get;
        set;
    }

    [JsonPropertyName("Multiplier")]
    public double? Multiplier
    {
        get;
        set;
    }
}
