using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BotWeaponScattering
{
    [JsonPropertyName("Name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("PriorityScatter1meter")]
    public double? PriorityScatter1Meter
    {
        get;
        set;
    }

    [JsonPropertyName("PriorityScatter10meter")]
    public double? PriorityScatter10Meter
    {
        get;
        set;
    }

    [JsonPropertyName("PriorityScatter100meter")]
    public double? PriorityScatter100Meter
    {
        get;
        set;
    }
}
