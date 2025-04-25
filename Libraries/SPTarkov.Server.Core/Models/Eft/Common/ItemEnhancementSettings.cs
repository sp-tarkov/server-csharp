using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ItemEnhancementSettings
{
    [JsonPropertyName("DamageReduction")]
    public PriceModifier? DamageReduction
    {
        get;
        set;
    }

    [JsonPropertyName("MalfunctionProtections")]
    public PriceModifier? MalfunctionProtections
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponSpread")]
    public PriceModifier? WeaponSpread
    {
        get;
        set;
    }
}
