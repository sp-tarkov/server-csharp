using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BodyPartColliderPart
{
    [JsonPropertyName("PenetrationChance")]
    public double? PenetrationChance
    {
        get;
        set;
    }

    [JsonPropertyName("PenetrationDamageMod")]
    public double? PenetrationDamageMod
    {
        get;
        set;
    }

    [JsonPropertyName("PenetrationLevel")]
    public double? PenetrationLevel
    {
        get;
        set;
    }
}
