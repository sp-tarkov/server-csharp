using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Inventory;

public record ItemSize
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("width")]
    public required int Width
    {
        get;
        set;
    }

    [JsonPropertyName("height")]
    public required int Height
    {
        get;
        set;
    }
}
