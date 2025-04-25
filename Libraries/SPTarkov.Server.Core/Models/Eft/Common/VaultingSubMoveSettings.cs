using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record VaultingSubMoveSettings
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive
    {
        get;
        set;
    }

    [JsonPropertyName("MaxWithoutHandHeight")]
    public double? MaxWithoutHandHeight
    {
        get;
        set;
    }

    public double? MaxOneHandHeight
    {
        get;
        set;
    }

    [JsonPropertyName("SpeedRange")]
    public XYZ? SpeedRange
    {
        get;
        set;
    }

    [JsonPropertyName("MoveRestrictions")]
    public MoveRestrictions? MoveRestrictions
    {
        get;
        set;
    }

    [JsonPropertyName("AutoMoveRestrictions")]
    public MoveRestrictions? AutoMoveRestrictions
    {
        get;
        set;
    }
}
