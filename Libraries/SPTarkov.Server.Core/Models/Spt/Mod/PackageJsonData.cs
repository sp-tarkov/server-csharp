using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public record PackageJsonData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("contributors")]
    public List<string>? Contributors { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("sptVersion")]
    public string? SptVersion { get; set; }

    [JsonPropertyName("loadBefore")]
    public List<string>? LoadBefore { get; set; }

    [JsonPropertyName("loadAfter")]
    public List<string>? LoadAfter { get; set; }

    [JsonPropertyName("incompatibilities")]
    public List<string>? Incompatibilities { get; set; }

    [JsonPropertyName("modDependencies")]
    public Dictionary<string, string>? ModDependencies { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("isBundleMod")]
    public bool? IsBundleMod { get; set; }

    [JsonPropertyName("licence")]
    public string? Licence { get; set; }
}
