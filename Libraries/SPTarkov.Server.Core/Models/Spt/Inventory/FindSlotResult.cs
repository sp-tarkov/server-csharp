using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Inventory;

public class FindSlotResult
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    public FindSlotResult(bool success)
    {
        Success = success;
    }

    public FindSlotResult(bool success, int x, int y, bool rotation)
    {
        Success = success;
        X = x;
        Y = y;
        Rotation = rotation;
    }

    public FindSlotResult() { }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    [JsonPropertyName("x")]
    public int? X { get; set; }

    [JsonPropertyName("y")]
    public int? Y { get; set; }

    [JsonPropertyName("rotation")]
    public bool? Rotation { get; set; }
}
