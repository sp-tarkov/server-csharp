using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaminaRestoration
{
    [JsonPropertyName("LowerLeftPoint")]
    public double? LowerLeftPoint
    {
        get;
        set;
    }

    [JsonPropertyName("LowerRightPoint")]
    public double? LowerRightPoint
    {
        get;
        set;
    }

    [JsonPropertyName("LeftPlatoPoint")]
    public double? LeftPlatoPoint
    {
        get;
        set;
    }

    [JsonPropertyName("RightPlatoPoint")]
    public double? RightPlatoPoint
    {
        get;
        set;
    }

    [JsonPropertyName("RightLimit")]
    public double? RightLimit
    {
        get;
        set;
    }

    [JsonPropertyName("ZeroValue")]
    public double? ZeroValue
    {
        get;
        set;
    }
}
