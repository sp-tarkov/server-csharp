using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common;

public class XY
{
    [JsonPropertyName("x")]
    public double? X { get; set; }

    [JsonPropertyName("y")]
    public double? Y { get; set; }
}