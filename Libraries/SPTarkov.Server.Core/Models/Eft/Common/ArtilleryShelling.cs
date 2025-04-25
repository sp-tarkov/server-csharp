using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ArtilleryShelling
{
    [JsonPropertyName("ArtilleryMapsConfigs")]
    public Dictionary<string, ArtilleryMapSettings>? ArtilleryMapsConfigs
    {
        get;
        set;
    }

    [JsonPropertyName("ProjectileExplosionParams")]
    public ProjectileExplosionParams? ProjectileExplosionParams
    {
        get;
        set;
    }

    [JsonPropertyName("MaxCalledShellingCount")]
    public double? MaxCalledShellingCount
    {
        get;
        set;
    }
}
