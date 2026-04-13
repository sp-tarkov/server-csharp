using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LooseLoot
{
    [JsonPropertyName("spawnpointCount")]
    public SpawnpointCount? SpawnpointCount { get; set; }

    [JsonPropertyName("spawnpointsForced")]
    public IEnumerable<Spawnpoint>? SpawnpointsForced { get; set; }

    [JsonPropertyName("spawnpoints")]
    public IEnumerable<Spawnpoint>? Spawnpoints { get; set; }
}

public record SpawnpointCount
{
    [JsonPropertyName("mean")]
    public required double Mean { get; set; }

    [JsonPropertyName("std")]
    public required double Std { get; set; }
}

public record SpawnpointTemplate
{
    /// <summary>
    /// Not a mongoId
    /// </summary>
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

    [JsonPropertyName("IsAlwaysSpawn")]
    public bool? IsAlwaysSpawn { get; set; }

    [JsonPropertyName("IsGroupPosition")]
    public bool? IsGroupPosition { get; set; }

    [JsonPropertyName("GroupPositions")]
    public IEnumerable<GroupPosition>? GroupPositions { get; set; }

    [JsonPropertyName("Root")]
    public string? Root
    {
        get { return field; }
        set { field = value == null ? null : string.Intern(value); }
    }

    [JsonPropertyName("Items")]
    public IEnumerable<SptLootItem>? Items { get; set; }
}

public record SptLootItem : Item
{
    [JsonPropertyName("composedKey")]
    public string? ComposedKey { get; set; }
}

public record GroupPosition
{
    [JsonPropertyName("Name")]
    public string? Name
    {
        get { return field; }
        set { field = value == null ? null : string.Intern(value); }
    }

    [JsonPropertyName("Weight")]
    public double? Weight { get; set; }

    [JsonPropertyName("Position")]
    public XYZ? Position { get; set; }

    [JsonPropertyName("Rotation")]
    public XYZ? Rotation { get; set; }
}

public record Spawnpoint
{
    [JsonPropertyName("locationId")]
    public string? LocationId { get; set; }

    [JsonPropertyName("probability")]
    public double? Probability { get; set; }

    [JsonPropertyName("template")]
    public SpawnpointTemplate? Template { get; set; }

    [JsonPropertyName("itemDistribution")]
    public IEnumerable<LooseLootItemDistribution>? ItemDistribution { get; set; }
}

public record LooseLootItemDistribution
{
    [JsonPropertyName("composedKey")]
    public ComposedKey? ComposedKey { get; set; }

    [JsonPropertyName("relativeProbability")]
    public double? RelativeProbability { get; set; }
}

public record ComposedKey
{
    [JsonPropertyName("key")]
    public string? Key
    {
        get { return field; }
        set { field = string.Intern(value); }
    }
}
