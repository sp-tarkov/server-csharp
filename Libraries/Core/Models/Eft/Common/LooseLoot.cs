using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;

namespace Core.Models.Eft.Common;

public record LooseLoot
{
    [JsonPropertyName("spawnpointCount")]
    public SpawnpointCount? SpawnpointCount
    {
        get;
        set;
    }

    [JsonPropertyName("spawnpointsForced")]
    public List<Spawnpoint>? SpawnpointsForced
    {
        get;
        set;
    }

    [JsonPropertyName("spawnpoints")]
    public List<Spawnpoint>? Spawnpoints
    {
        get;
        set;
    }
}

public record SpawnpointCount
{
    [JsonPropertyName("mean")]
    public double? Mean
    {
        get;
        set;
    }

    [JsonPropertyName("std")]
    public double? Std
    {
        get;
        set;
    }
}

public record SpawnpointTemplate
{
    private string? _root;

    [JsonPropertyName("Id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("IsContainer")]
    public bool? IsContainer
    {
        get;
        set;
    }

    [JsonPropertyName("useGravity")]
    public bool? UseGravity
    {
        get;
        set;
    }

    [JsonPropertyName("randomRotation")]
    public bool? RandomRotation
    {
        get;
        set;
    }

    [JsonPropertyName("Position")]
    public XYZ? Position
    {
        get;
        set;
    }

    [JsonPropertyName("Rotation")]
    public XYZ? Rotation
    {
        get;
        set;
    }

    [JsonPropertyName("IsAlwaysSpawn")]
    public bool? IsAlwaysSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("IsGroupPosition")]
    public bool? IsGroupPosition
    {
        get;
        set;
    }

    [JsonPropertyName("GroupPositions")]
    public List<GroupPosition>? GroupPositions
    {
        get;
        set;
    }

    [JsonPropertyName("Root")]
    public string? Root
    {
        get
        {
            return _root;
        }
        set
        {
            _root = value == null ? null : string.Intern(value);
        }
    }

    [JsonPropertyName("Items")]
    public List<Item>? Items
    {
        get;
        set;
    }
}

public record GroupPosition
{
    private string? _name;

    [JsonPropertyName("Name")]
    public string? Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value == null ? null : string.Intern(value);
        }
    }

    [JsonPropertyName("Weight")]
    public double? Weight
    {
        get;
        set;
    }

    [JsonPropertyName("Position")]
    public XYZ? Position
    {
        get;
        set;
    }

    [JsonPropertyName("Rotation")]
    public XYZ? Rotation
    {
        get;
        set;
    }
}

public record Spawnpoint
{
    [JsonPropertyName("locationId")]
    public string? LocationId
    {
        get;
        set;
    }

    [JsonPropertyName("probability")]
    public double? Probability
    {
        get;
        set;
    }

    [JsonPropertyName("template")]
    public SpawnpointTemplate? Template
    {
        get;
        set;
    }

    [JsonPropertyName("itemDistribution")]
    public List<LooseLootItemDistribution>? ItemDistribution
    {
        get;
        set;
    }
}

public record LooseLootItemDistribution
{
    [JsonPropertyName("composedKey")]
    public ComposedKey? ComposedKey
    {
        get;
        set;
    }

    [JsonPropertyName("relativeProbability")]
    public double? RelativeProbability
    {
        get;
        set;
    }
}

public record ComposedKey
{
    [JsonPropertyName("key")]
    public string? Key
    {
        get;
        set;
    }
}
