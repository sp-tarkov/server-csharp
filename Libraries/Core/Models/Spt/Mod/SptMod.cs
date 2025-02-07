using System.Reflection;
using System.Text.Json.Serialization;

namespace Core.Models.Spt.Mod;

public class SptMod
{
    [JsonPropertyName("PackageJson")]
    public PackageJsonData PackageJson { get; set; }

    [JsonPropertyName("Assembly")]
    public Assembly Assembly { get; set; }
}
