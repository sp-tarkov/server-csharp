using System.Text.Json.Serialization;

namespace Core.Models.Spt.Mod;

public record PackageJsonData
{
    [JsonPropertyName("incompatibilities")]
    public List<string>? Incompatibilities { get; set; }

    [JsonPropertyName("loadBefore")]
    public List<string>? LoadBefore { get; set; }

    [JsonPropertyName("loadAfter")]
    public List<string>? LoadAfter { get; set; }

    [JsonPropertyName("dependencies")]
    public Dictionary<string, string>? Dependencies { get; set; }

    [JsonPropertyName("modDependencies")]
    public Dictionary<string, string>? ModDependencies { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("sptVersion")]
    public string? SptVersion { get; set; }

    // We deliberately purge this data
    [JsonPropertyName("scripts")]
    public Dictionary<string, string>? Scripts { get; set; }

    [JsonPropertyName("devDependencies")]
    public Dictionary<string, string>? DevDependencies { get; set; }

    [JsonPropertyName("licence")]
    public string? Licence { get; set; }

    [JsonPropertyName("main")]
    public string? Main { get; set; }

    [JsonPropertyName("isBundleMod")]
    public bool? IsBundleMod { get; set; }

    [JsonPropertyName("contributors")]
    public List<string>? Contributors { get; set; }
}

// TODO: this will need changing to however we implement it in this project
