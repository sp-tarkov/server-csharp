using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaticPropsBase
{
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

    [JsonPropertyName("IsGroupPosition")]
    public bool? IsGroupPosition
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

    [JsonPropertyName("GroupPositions")]
    public GroupPosition[] GroupPositions
    {
        get;
        set;
    }

    [JsonPropertyName("Root")]
    public string? Root
    {
        get;
        set;
    }

    [JsonPropertyName("Items")]
    public Item[] Items
    {
        get;
        set;
    }
}
