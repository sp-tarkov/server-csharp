using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Inventory;

public record ItemSize
{
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
