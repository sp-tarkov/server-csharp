using System.Reflection;
using System.Text.Json.Serialization;

namespace Core.Models.Spt.Mod;

public class SptMod
{
    [JsonPropertyName("directory")]
    public string Directory
    {
        get;
        set;
    }

    [JsonPropertyName("packageJson")]
    public PackageJsonData? PackageJson
    {
        get;
        set;
    }

    [JsonPropertyName("assemblies")]
    public List<Assembly>? Assemblies
    {
        get;
        set;
    }
}
