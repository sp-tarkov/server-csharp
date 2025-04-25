using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record WeaponTreatmentCounters
{
    [JsonPropertyName("firearmsDurability")]
    public SkillCounter? FirearmsDurability
    {
        get;
        set;
    }
}
