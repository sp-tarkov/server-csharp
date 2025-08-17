using System.Reflection;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public class SptMod
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; init; } = [];

    [JsonPropertyName("directory")]
    public required string Directory { get; init; }

    [JsonPropertyName("modMetadata")]
    public required AbstractModMetadata ModMetadata { get; init; }

    [JsonPropertyName("assemblies")]
    public required IEnumerable<Assembly> Assemblies { get; init; }
}
