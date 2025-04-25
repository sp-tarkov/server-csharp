using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Effects
{
    [JsonPropertyName("Existence")]
    public Existence? Existence
    {
        get;
        set;
    }

    [JsonPropertyName("Dehydration")]
    public Dehydration? Dehydration
    {
        get;
        set;
    }

    [JsonPropertyName("BreakPart")]
    public BreakPart? BreakPart
    {
        get;
        set;
    }

    [JsonPropertyName("Contusion")]
    public Contusion? Contusion
    {
        get;
        set;
    }

    [JsonPropertyName("Disorientation")]
    public Disorientation? Disorientation
    {
        get;
        set;
    }

    [JsonPropertyName("Exhaustion")]
    public Exhaustion? Exhaustion
    {
        get;
        set;
    }

    [JsonPropertyName("LowEdgeHealth")]
    public LowEdgeHealth? LowEdgeHealth
    {
        get;
        set;
    }

    [JsonPropertyName("RadExposure")]
    public RadExposure? RadExposure
    {
        get;
        set;
    }

    [JsonPropertyName("Stun")]
    public Stun? Stun
    {
        get;
        set;
    }

    [JsonPropertyName("Intoxication")]
    public Intoxication? Intoxication
    {
        get;
        set;
    }

    [JsonPropertyName("Regeneration")]
    public Regeneration? Regeneration
    {
        get;
        set;
    }

    [JsonPropertyName("Wound")]
    public Wound? Wound
    {
        get;
        set;
    }

    [JsonPropertyName("Berserk")]
    public Berserk? Berserk
    {
        get;
        set;
    }

    [JsonPropertyName("Flash")]
    public Flash? Flash
    {
        get;
        set;
    }

    [JsonPropertyName("MedEffect")]
    public MedEffect? MedEffect
    {
        get;
        set;
    }

    [JsonPropertyName("Pain")]
    public Pain? Pain
    {
        get;
        set;
    }

    [JsonPropertyName("PainKiller")]
    public PainKiller? PainKiller
    {
        get;
        set;
    }

    [JsonPropertyName("SandingScreen")]
    public SandingScreen? SandingScreen
    {
        get;
        set;
    }

    [JsonPropertyName("MildMusclePain")]
    public MusclePainEffect? MildMusclePain
    {
        get;
        set;
    }

    [JsonPropertyName("SevereMusclePain")]
    public MusclePainEffect? SevereMusclePain
    {
        get;
        set;
    }

    [JsonPropertyName("Stimulator")]
    public Stimulator? Stimulator
    {
        get;
        set;
    }

    [JsonPropertyName("Tremor")]
    public Tremor? Tremor
    {
        get;
        set;
    }

    [JsonPropertyName("ChronicStaminaFatigue")]
    public ChronicStaminaFatigue? ChronicStaminaFatigue
    {
        get;
        set;
    }

    [JsonPropertyName("Fracture")]
    public Fracture? Fracture
    {
        get;
        set;
    }

    [JsonPropertyName("HeavyBleeding")]
    public HeavyBleeding? HeavyBleeding
    {
        get;
        set;
    }

    [JsonPropertyName("LightBleeding")]
    public LightBleeding? LightBleeding
    {
        get;
        set;
    }

    [JsonPropertyName("BodyTemperature")]
    public BodyTemperature? BodyTemperature
    {
        get;
        set;
    }

    [JsonPropertyName("ZombieInfection")]
    public ZombieInfection? ZombieInfection
    {
        get;
        set;
    }
}
