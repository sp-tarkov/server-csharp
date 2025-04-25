using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BonusSettings
{
    [JsonPropertyName("EliteBonusSettings")]
    public EliteBonusSettings? EliteBonusSettings
    {
        get;
        set;
    }

    [JsonPropertyName("LevelBonusSettings")]
    public LevelBonusSettings? LevelBonusSettings
    {
        get;
        set;
    }
}
