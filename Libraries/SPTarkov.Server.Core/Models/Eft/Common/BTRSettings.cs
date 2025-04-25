using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record BTRSettings
{
    [JsonPropertyName("LocationsWithBTR")]
    public List<string>? LocationsWithBTR
    {
        get;
        set;
    }

    [JsonPropertyName("BasePriceTaxi")]
    public double? BasePriceTaxi
    {
        get;
        set;
    }

    [JsonPropertyName("AddPriceTaxi")]
    public double? AddPriceTaxi
    {
        get;
        set;
    }

    [JsonPropertyName("CleanUpPrice")]
    public double? CleanUpPrice
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

    [JsonPropertyName("BearPriceMod")]
    public double? BearPriceMod
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

    [JsonPropertyName("ScavPriceMod")]
    public double? ScavPriceMod
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

    [JsonPropertyName("TaxiMinPrice")]
    public double? TaxiMinPrice
    {
        get;
        set;
    }

    [JsonPropertyName("BotCoverMinPrice")]
    public double? BotCoverMinPrice
    {
        get;
        set;
    }

    [JsonPropertyName("MapsConfigs")]
    public Dictionary<string, BtrMapConfig>? MapsConfigs
    {
        get;
        set;
    }

    [JsonPropertyName("DiameterWheel")]
    public double? DiameterWheel
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheel")]
    public double? HeightWheel
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheelMaxPosLimit")]
    public double? HeightWheelMaxPosLimit
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheelMinPosLimit")]
    public double? HeightWheelMinPosLimit
    {
        get;
        set;
    }

    [JsonPropertyName("SnapToSurfaceWheelsSpeed")]
    public double? SnapToSurfaceWheelsSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("CheckSurfaceForWheelsTimer")]
    public double? CheckSurfaceForWheelsTimer
    {
        get;
        set;
    }

    [JsonPropertyName("HeightWheelOffset")]
    public double? HeightWheelOffset
    {
        get;
        set;
    }
}
