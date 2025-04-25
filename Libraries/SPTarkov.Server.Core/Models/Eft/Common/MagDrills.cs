using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MagDrills
{
    [JsonPropertyName("RaidLoadedAmmoAction")]
    public double? RaidLoadedAmmoAction
    {
        get;
        set;
    }

    [JsonPropertyName("RaidUnloadedAmmoAction")]
    public double? RaidUnloadedAmmoAction
    {
        get;
        set;
    }

    [JsonPropertyName("MagazineCheckAction")]
    public double? MagazineCheckAction
    {
        get;
        set;
    }
}
