using System.Reflection;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public class SptMod
{
    [JsonPropertyName("directory")]
    public required string Directory { get; init; }

    [JsonPropertyName("modMetadata")]
    public AbstractModMetadata ModMetadata { get; set; } = null!;

    [JsonPropertyName("assemblies")]
    public required IEnumerable<Assembly> Assemblies { get; init; }

    [JsonPropertyName("patcherAssembly")]
    public Assembly? PatcherAssembly { get; set; }
}
