using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Inventory;

public record ItemSize
{
    [JsonPropertyName("width")]
    public int Width
    {
        get;
        set;
    }

    [JsonPropertyName("height")]
    public int Height
    {
        get;
        set;
    }
}
