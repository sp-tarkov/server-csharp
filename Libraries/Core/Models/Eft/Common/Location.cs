using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Utils.Json;

namespace Core.Models.Eft.Common;

public record Location
{
    /** Map meta-data */
    [JsonPropertyName("base")]
    public LocationBase? Base { get; set; }

    /** Loose loot positions and item weights */
    [JsonPropertyName("looseLoot")]
    public LazyLoad<LooseLoot>? LooseLoot { get; set; }

    /** Static loot item weights */
    [JsonPropertyName("staticLoot")]
    public LazyLoad<Dictionary<string, StaticLootDetails>>? StaticLoot { get; set; }

    /** Static container positions and item weights */
    [JsonPropertyName("staticContainers")]
    public LazyLoad<StaticContainerDetails>? StaticContainers { get; set; }

    [JsonPropertyName("staticAmmo")]
    public Dictionary<string, List<StaticAmmoDetails>> StaticAmmo { get; set; }

    /** All possible static containers on map + their assign groupings */
    [JsonPropertyName("statics")]
    public StaticContainer? Statics { get; set; }

    /** All possible map extracts */
    [JsonPropertyName("allExtracts")]
    public Exit[] AllExtracts { get; set; }
}

public record StaticContainer
{
    [JsonPropertyName("containersGroups")]
    public Dictionary<string, ContainerMinMax>? ContainersGroups { get; set; }

    [JsonPropertyName("containers")]
    public Dictionary<string, ContainerData>? Containers { get; set; }
}

public record ContainerMinMax
{
    [JsonPropertyName("minContainers")]
    public int? MinContainers { get; set; }

    [JsonPropertyName("maxContainers")]
    public int? MaxContainers { get; set; }

    [JsonPropertyName("current")]
    public int? Current { get; set; }

    [JsonPropertyName("chosenCount")]
    public int? ChosenCount { get; set; }
}

public record ContainerData
{
    [JsonPropertyName("groupId")]
    public string? GroupId { get; set; }
}

public record StaticLootDetails
{
    [JsonPropertyName("itemcountDistribution")]
    public ItemCountDistribution[] ItemCountDistribution { get; set; }

    [JsonPropertyName("itemDistribution")]
    public ItemDistribution[] ItemDistribution { get; set; }
}

public record ItemCountDistribution
{
    [JsonPropertyName("count")]
    public int? Count { get; set; }

    [JsonPropertyName("relativeProbability")]
    public float? RelativeProbability { get; set; }
}

public record ItemDistribution
{
    [JsonPropertyName("tpl")]
    public string? Tpl { get; set; }

    [JsonPropertyName("relativeProbability")]
    public float? RelativeProbability { get; set; }
}

public record StaticPropsBase
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("IsContainer")]
    public bool? IsContainer { get; set; }

    [JsonPropertyName("useGravity")]
    public bool? UseGravity { get; set; }

    [JsonPropertyName("randomRotation")]
    public bool? RandomRotation { get; set; }

    [JsonPropertyName("Position")]
    public XYZ? Position { get; set; }

    [JsonPropertyName("Rotation")]
    public XYZ? Rotation { get; set; }

    [JsonPropertyName("IsGroupPosition")]
    public bool? IsGroupPosition { get; set; }

    [JsonPropertyName("IsAlwaysSpawn")]
    public bool? IsAlwaysSpawn { get; set; }

    [JsonPropertyName("GroupPositions")]
    public GroupPosition[] GroupPositions { get; set; }

    [JsonPropertyName("Root")]
    public string? Root { get; set; }

    [JsonPropertyName("Items")]
    public Item[] Items { get; set; }
}

[Obsolete("use SpawnpointTemplate")]
public record StaticWeaponProps : StaticPropsBase
{
    [JsonPropertyName("Items")]
    public Item[] Items { get; set; }
}

public record StaticContainerDetails
{
    [JsonPropertyName("staticWeapons")]
    public List<SpawnpointTemplate> StaticWeapons { get; set; }

    [JsonPropertyName("staticContainers")]
    public List<StaticContainerData> StaticContainers { get; set; }

    [JsonPropertyName("staticForced")]
    public List<StaticForced> StaticForced { get; set; }
}

public record StaticForced
{
    [JsonPropertyName("containerId")]
    public string ContainerId { get; set; }

    [JsonPropertyName("itemTpl")]
    public string ItemTpl { get; set; }
}

public record StaticContainerData
{
    [JsonPropertyName("probability")]
    public float? Probability { get; set; }

    [JsonPropertyName("template")]
    public SpawnpointTemplate? Template { get; set; }
}

public record StaticAmmoDetails
{
    [JsonPropertyName("tpl")]
    public string? Tpl { get; set; }

    [JsonPropertyName("relativeProbability")]
    public float? RelativeProbability { get; set; }
}

public record StaticForcedProps
{
    [JsonPropertyName("containerId")]
    public string? ContainerId { get; set; }

    [JsonPropertyName("itemTpl")]
    public string? ItemTpl { get; set; }
}

[Obsolete("use SpawnpointTemplate")]
public record StaticContainerProps : StaticPropsBase
{
    [JsonPropertyName("Items")]
    public StaticItem[] Items { get; set; }
}

public record StaticItem
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_tpl")]
    public string? Tpl { get; set; }

    [JsonPropertyName("upd")]
    public Upd? Upd { get; set; }
}
