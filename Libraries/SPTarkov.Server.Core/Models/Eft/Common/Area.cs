using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Area
{
    [JsonPropertyName("center")]
    public XYZ? Center
    {
        get;
        set;
    }

    [JsonPropertyName("infiltrationZone")]
    public string? InfiltrationZone
    {
        get;
        set;
    }

    [JsonPropertyName("orientation")]
    public double? Orientation
    {
        get;
        set;
    }

    [JsonPropertyName("position")]
    public XYZ? Position
    {
        get;
        set;
    }

    [JsonPropertyName("sides")]
    public List<string>? Sides
    {
        get;
        set;
    }

    [JsonPropertyName("size")]
    public XYZ? Size
    {
        get;
        set;
    }
}
