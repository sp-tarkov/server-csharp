using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Bundles;

public sealed record BundleManifestEntry
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("dependencyKeys")]
    public List<string>? DependencyKeys { get; set; }
}
