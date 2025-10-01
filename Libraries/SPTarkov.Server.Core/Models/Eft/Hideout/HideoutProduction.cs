using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace SPTarkov.Server.Core.Models.Eft.Hideout;

public record HideoutProductionData
{
    [JsonPropertyName("recipes")]
    public List<HideoutProduction>? Recipes { get; set; }

    [JsonPropertyName("scavRecipes")]
    public List<ScavRecipe>? ScavRecipes { get; set; }

    [JsonPropertyName("cultistRecipes")]
    public List<CultistRecipe>? CultistRecipes { get; set; }
}

public record HideoutProduction
{
    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("areaType")]
    public HideoutAreas? AreaType { get; set; }

    [JsonPropertyName("requirements")]
    public List<Requirement>? Requirements { get; set; }

    [JsonPropertyName("productionTime")]
    public double? ProductionTime { get; set; }

    /// <summary>
    ///     Tpl of item being crafted
    /// </summary>
    [JsonPropertyName("endProduct")]
    public MongoId EndProduct { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("locked")]
    public bool? Locked { get; set; }

    [JsonPropertyName("needFuelForAllProductionTime")]
    public bool? NeedFuelForAllProductionTime { get; set; }

    [JsonPropertyName("continuous")]
    public bool? Continuous { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("productionLimitCount")]
    public int? ProductionLimitCount { get; set; }

    [JsonPropertyName("isCodeProduction")]
    public bool? IsCodeProduction { get; set; }
}

public record Requirement
{
    [JsonPropertyName("templateId")]
    public MongoId? TemplateId { get; set; }

    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("isFunctional")]
    public bool? IsFunctional { get; set; }

    [JsonPropertyName("areaType")]
    public int? AreaType { get; set; }

    [JsonPropertyName("requiredLevel")]
    public int? RequiredLevel { get; set; }

    [JsonPropertyName("resource")]
    public int? Resource { get; set; }

    [JsonPropertyName("questId")]
    public MongoId? QuestId { get; set; }

    [JsonPropertyName("isSpawnedInSession")]
    public bool? IsSpawnedInSession { get; set; }

    [JsonPropertyName("gameVersions")]
    public List<string>? GameVersions { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public record ScavRecipe
{
    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("requirements")]
    public List<Requirement>? Requirements { get; set; }

    [JsonPropertyName("productionTime")]
    public double? ProductionTime { get; set; }

    [JsonPropertyName("endProducts")]
    public EndProducts? EndProducts { get; set; }
}

public record EndProducts
{
    [JsonPropertyName("Common")]
    public MinMax<int>? Common { get; set; }

    [JsonPropertyName("Rare")]
    public MinMax<int>? Rare { get; set; }

    [JsonPropertyName("Superrare")]
    public MinMax<int>? Superrare { get; set; }
}

public record CultistRecipe
{
    [JsonPropertyName("_id")]
    public MongoId Id { get; set; }
}
