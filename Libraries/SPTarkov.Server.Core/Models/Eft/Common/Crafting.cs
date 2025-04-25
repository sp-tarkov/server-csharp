using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Crafting
{
    [JsonPropertyName("DependentSkillRatios")]
    public List<DependentSkillRatio>? DependentSkillRatios
    {
        get;
        set;
    }

    [JsonPropertyName("PointsPerCraftingCycle")]
    public double? PointsPerCraftingCycle
    {
        get;
        set;
    }

    [JsonPropertyName("CraftingCycleHours")]
    public double? CraftingCycleHours
    {
        get;
        set;
    }

    [JsonPropertyName("PointsPerUniqueCraftCycle")]
    public double? PointsPerUniqueCraftCycle
    {
        get;
        set;
    }

    [JsonPropertyName("UniqueCraftsPerCycle")]
    public double? UniqueCraftsPerCycle
    {
        get;
        set;
    }

    [JsonPropertyName("CraftTimeReductionPerLevel")]
    public double? CraftTimeReductionPerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("ProductionTimeReductionPerLevel")]
    public double? ProductionTimeReductionPerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("EliteExtraProductions")]
    public double? EliteExtraProductions
    {
        get;
        set;
    }

    // Yes, there is a typo
    [JsonPropertyName("CraftingPointsToInteligence")]
    public double? CraftingPointsToIntelligence
    {
        get;
        set;
    }
}
