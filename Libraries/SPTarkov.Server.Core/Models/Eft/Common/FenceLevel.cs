using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record FenceLevel
{
    [JsonPropertyName("ReachOnMarkOnUnknowns")]
    public bool? CanReachOnMarkOnUnknowns
    {
        get;
        set;
    }

    [JsonPropertyName("SavageCooldownModifier")]
    public double? SavageCooldownModifier
    {
        get;
        set;
    }

    [JsonPropertyName("ScavCaseTimeModifier")]
    public double? ScavCaseTimeModifier
    {
        get;
        set;
    }

    [JsonPropertyName("PaidExitCostModifier")]
    public double? PaidExitCostModifier
    {
        get;
        set;
    }

    [JsonPropertyName("BotFollowChance")]
    public double? BotFollowChance
    {
        get;
        set;
    }

    [JsonPropertyName("ScavEquipmentSpawnChanceModifier")]
    public double? ScavEquipmentSpawnChanceModifier
    {
        get;
        set;
    }

    [JsonPropertyName("TransitGridSize")]
    public XYZ? TransitGridSize
    {
        get;
        set;
    }

    [JsonPropertyName("PriceModifier")]
    public double? PriceModifier
    {
        get;
        set;
    }

    [JsonPropertyName("HostileBosses")]
    public bool? AreHostileBossesPresent
    {
        get;
        set;
    }

    [JsonPropertyName("HostileScavs")]
    public bool? AreHostileScavsPresent
    {
        get;
        set;
    }

    [JsonPropertyName("ScavAttackSupport")]
    public bool? IsScavAttackSupported
    {
        get;
        set;
    }

    [JsonPropertyName("ExfiltrationPriceModifier")]
    public double? ExfiltrationPriceModifier
    {
        get;
        set;
    }

    [JsonPropertyName("AvailableExits")]
    public double? AvailableExits
    {
        get;
        set;
    }

    [JsonPropertyName("BotApplySilenceChance")]
    public double? BotApplySilenceChance
    {
        get;
        set;
    }

    [JsonPropertyName("BotGetInCoverChance")]
    public double? BotGetInCoverChance
    {
        get;
        set;
    }

    [JsonPropertyName("BotHelpChance")]
    public double? BotHelpChance
    {
        get;
        set;
    }

    [JsonPropertyName("BotSpreadoutChance")]
    public double? BotSpreadoutChance
    {
        get;
        set;
    }

    [JsonPropertyName("BotStopChance")]
    public double? BotStopChance
    {
        get;
        set;
    }

    [JsonPropertyName("PriceModTaxi")]
    public double? PriceModifierTaxi
    {
        get;
        set;
    }

    [JsonPropertyName("PriceModDelivery")]
    public double? PriceModifierDelivery
    {
        get;
        set;
    }

    [JsonPropertyName("PriceModCleanUp")]
    public double? PriceModifierCleanUp
    {
        get;
        set;
    }

    [JsonPropertyName("ReactOnMarkOnUnknowns")]
    public bool? ReactOnMarkOnUnknowns
    {
        get;
        set;
    }

    [JsonPropertyName("ReactOnMarkOnUnknownsPVE")]
    public bool? ReactOnMarkOnUnknownsPVE
    {
        get;
        set;
    }

    [JsonPropertyName("DeliveryGridSize")]
    public XYZ? DeliveryGridSize
    {
        get;
        set;
    }

    [JsonPropertyName("CanInteractWithBtr")]
    public bool? CanInteractWithBtr
    {
        get;
        set;
    }

    [JsonPropertyName("CircleOfCultistsBonusPercent")]
    public double? CircleOfCultistsBonusPercentage
    {
        get;
        set;
    }
}
