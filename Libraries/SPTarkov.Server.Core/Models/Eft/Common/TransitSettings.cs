using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record TransitSettings
{
    [JsonPropertyName("BearPriceMod")]
    public double? BearPriceMod
    {
        get;
        set;
    }

    [JsonPropertyName("ClearAllPlayerEffectsOnTransit")]
    public bool? ClearAllPlayerEffectsOnTransit
    {
        get;
        set;
    }

    [JsonPropertyName("CoefficientDiscountCharisma")]
    public double? CoefficientDiscountCharisma
    {
        get;
        set;
    }

    [JsonPropertyName("DeliveryMinPrice")]
    public double? DeliveryMinPrice
    {
        get;
        set;
    }

    [JsonPropertyName("DeliveryPrice")]
    public double? DeliveryPrice
    {
        get;
        set;
    }

    [JsonPropertyName("ModDeliveryCost")]
    public double? ModDeliveryCost
    {
        get;
        set;
    }

    [JsonPropertyName("PercentageOfMissingEnergyRestore")]
    public double? PercentageOfMissingEnergyRestore
    {
        get;
        set;
    }

    [JsonPropertyName("PercentageOfMissingHealthRestore")]
    public double? PercentageOfMissingHealthRestore
    {
        get;
        set;
    }

    [JsonPropertyName("PercentageOfMissingWaterRestore")]
    public double? PercentageOfMissingWaterRestore
    {
        get;
        set;
    }

    [JsonPropertyName("RestoreHealthOnDestroyedParts")]
    public bool? RestoreHealthOnDestroyedParts
    {
        get;
        set;
    }

    [JsonPropertyName("ScavPriceMod")]
    public double? ScavPriceMod
    {
        get;
        set;
    }

    [JsonPropertyName("UsecPriceMod")]
    public double? UsecPriceMod
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
}
