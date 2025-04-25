using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record AimDrills
{
    [JsonPropertyName("WeaponShotAction")]
    public double? WeaponShotAction
    {
        get;
        set;
    }
}
