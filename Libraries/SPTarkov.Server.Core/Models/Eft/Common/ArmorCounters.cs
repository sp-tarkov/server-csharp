using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArmorCounters
{
    [JsonPropertyName("armorDurability")]
    public SkillCounter? ArmorDurability
    {
        get;
        set;
    }
}
