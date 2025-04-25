using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

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
