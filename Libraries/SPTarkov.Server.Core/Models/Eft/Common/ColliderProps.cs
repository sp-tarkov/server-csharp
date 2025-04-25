using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ColliderProps
{
    [JsonPropertyName("Center")]
    public XYZ? Center
    {
        get;
        set;
    }

    [JsonPropertyName("Size")]
    public XYZ? Size
    {
        get;
        set;
    }

    [JsonPropertyName("Radius")]
    public double? Radius
    {
        get;
        set;
    }
}
