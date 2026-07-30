using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Bundles;

public sealed record BundleManifest
{
    [JsonPropertyName("manifest")]
    public List<BundleManifestEntry>? Manifest { get; set; }
}
