using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common;

public class XYZ
{
    [JsonPropertyName("x")]
    public double? X { get; set; }

    [JsonPropertyName("y")]
    public double? Y { get; set; }

    [JsonPropertyName("z")]
    public double? Z { get; set; }
}