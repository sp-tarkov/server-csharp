using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record MoveRestrictions
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive
    {
        get;
        set;
    }

    [JsonPropertyName("MinDistantToInteract")]
    public double? MinDistantToInteract
    {
        get;
        set;
    }

    [JsonPropertyName("MinHeight")]
    public double? MinHeight
    {
        get;
        set;
    }

    [JsonPropertyName("MaxHeight")]
    public double? MaxHeight
    {
        get;
        set;
    }

    [JsonPropertyName("MinLength")]
    public double? MinLength
    {
        get;
        set;
    }

    [JsonPropertyName("MaxLength")]
    public double? MaxLength
    {
        get;
        set;
    }
}
