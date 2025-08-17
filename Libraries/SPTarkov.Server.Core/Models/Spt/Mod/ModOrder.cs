using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public class ModOrder
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("order")]
    public List<string> Order { get; set; }
}
