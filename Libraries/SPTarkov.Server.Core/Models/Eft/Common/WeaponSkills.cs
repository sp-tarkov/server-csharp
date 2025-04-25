using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record WeaponSkills
{
    [JsonPropertyName("WeaponReloadAction")]
    public double? WeaponReloadAction
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponShotAction")]
    public double? WeaponShotAction
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponFixAction")]
    public double? WeaponFixAction
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponChamberAction")]
    public double? WeaponChamberAction
    {
        get;
        set;
    }
}
