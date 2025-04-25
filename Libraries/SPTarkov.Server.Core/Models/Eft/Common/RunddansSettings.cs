using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record RunddansSettings
{
    [JsonPropertyName("accessKeys")]
    public List<string>? AccessKeys
    {
        get;
        set;
    }

    [JsonPropertyName("active")]
    public bool? Active
    {
        get;
        set;
    }

    [JsonPropertyName("activePVE")]
    public bool? ActivePVE
    {
        get;
        set;
    }

    [JsonPropertyName("applyFrozenEverySec")]
    public double? ApplyFrozenEverySec
    {
        get;
        set;
    }

    [JsonPropertyName("consumables")]
    public List<string>? Consumables
    {
        get;
        set;
    }

    [JsonPropertyName("drunkImmunitySec")]
    public double? DrunkImmunitySec
    {
        get;
        set;
    }

    [JsonPropertyName("durability")]
    public XY? Durability
    {
        get;
        set;
    }

    [JsonPropertyName("fireDistanceToHeat")]
    public double? FireDistanceToHeat
    {
        get;
        set;
    }

    [JsonPropertyName("grenadeDistanceToBreak")]
    public double? GrenadeDistanceToBreak
    {
        get;
        set;
    }

    [JsonPropertyName("interactionDistance")]
    public double? InteractionDistance
    {
        get;
        set;
    }

    [JsonPropertyName("knifeCritChanceToBreak")]
    public double? KnifeCritChanceToBreak
    {
        get;
        set;
    }

    [JsonPropertyName("locations")]
    public List<string>? Locations
    {
        get;
        set;
    }

    [JsonPropertyName("multitoolRepairSec")]
    public double? MultitoolRepairSec
    {
        get;
        set;
    }

    [JsonPropertyName("nonExitsLocations")]
    public List<string>? NonExitsLocations
    {
        get;
        set;
    }

    [JsonPropertyName("rainForFrozen")]
    public double? RainForFrozen
    {
        get;
        set;
    }

    [JsonPropertyName("repairSec")]
    public double? RepairSec
    {
        get;
        set;
    }

    [JsonPropertyName("secToBreak")]
    public XY? SecToBreak
    {
        get;
        set;
    }

    [JsonPropertyName("sleighLocations")]
    public List<string>? SleighLocations
    {
        get;
        set;
    }
}
