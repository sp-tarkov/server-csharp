using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record EnemyHealthEffect
{
    [JsonPropertyName("bodyParts")]
    public List<string>? BodyParts
    {
        get;
        set;
    }

    [JsonPropertyName("effects")]
    public List<string>? Effects
    {
        get;
        set;
    }
}
