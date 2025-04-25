using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record EffectsDamage
{
    [JsonPropertyName("Pain")]
    public EffectDamageProps? Pain
    {
        get;
        set;
    }

    [JsonPropertyName("LightBleeding")]
    public EffectDamageProps? LightBleeding
    {
        get;
        set;
    }

    [JsonPropertyName("HeavyBleeding")]
    public EffectDamageProps? HeavyBleeding
    {
        get;
        set;
    }

    [JsonPropertyName("Contusion")]
    public EffectDamageProps? Contusion
    {
        get;
        set;
    }

    [JsonPropertyName("RadExposure")]
    public EffectDamageProps? RadExposure
    {
        get;
        set;
    }

    [JsonPropertyName("Fracture")]
    public EffectDamageProps? Fracture
    {
        get;
        set;
    }

    [JsonPropertyName("DestroyedPart")]
    public EffectDamageProps? DestroyedPart
    {
        get;
        set;
    }
}
