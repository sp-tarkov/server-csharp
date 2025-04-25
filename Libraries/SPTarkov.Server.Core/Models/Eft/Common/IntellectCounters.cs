using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record IntellectCounters
{
    [JsonPropertyName("armorDurability")]
    public SkillCounter? ArmorDurability
    {
        get;
        set;
    }

    [JsonPropertyName("firearmsDurability")]
    public SkillCounter? FirearmsDurability
    {
        get;
        set;
    }

    [JsonPropertyName("meleeWeaponDurability")]
    public SkillCounter? MeleeWeaponDurability
    {
        get;
        set;
    }
}
