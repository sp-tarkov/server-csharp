using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ProjectileExplosionParams
{
    [JsonPropertyName("Blindness")]
    public XYZ? Blindness
    {
        get;
        set;
    }

    [JsonPropertyName("Contusion")]
    public XYZ? Contusion
    {
        get;
        set;
    }

    [JsonPropertyName("ArmorDistanceDistanceDamage")]
    public XYZ? ArmorDistanceDistanceDamage
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("MinExplosionDistance")]
    public double? MinExplosionDistance
    {
        get;
        set;
    }

    [JsonPropertyName("MaxExplosionDistance")]
    public float? MaxExplosionDistance
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("FragmentsCount")]
    public int? FragmentsCount
    {
        get;
        set;
    }

    [JsonPropertyName("Strength")]
    public double? Strength
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("ArmorDamage")]
    public double? ArmorDamage
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("StaminaBurnRate")]
    public double? StaminaBurnRate
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("PenetrationPower")]
    public double? PenetrationPower
    {
        get;
        set;
    }

    [JsonPropertyName("DirectionalDamageAngle")]
    public double? DirectionalDamageAngle
    {
        get;
        set;
    }

    [JsonPropertyName("DirectionalDamageMultiplier")]
    public double? DirectionalDamageMultiplier
    {
        get;
        set;
    }

    [JsonPropertyName("FragmentType")]
    public string? FragmentType
    {
        get;
        set;
    }

    [JsonPropertyName("DeadlyDistance")]
    public double? DeadlyDistance
    {
        get;
        set;
    }
}
