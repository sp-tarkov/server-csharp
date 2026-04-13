using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public struct XYZ
{
    [JsonPropertyName("x")]
    public required float X { get; set; }

    [JsonPropertyName("y")]
    public required float Y { get; set; }

    [JsonPropertyName("z")]
    public required float Z { get; set; }
}
