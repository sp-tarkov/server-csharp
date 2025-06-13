using System.Reflection;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public class SptMod
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    [JsonPropertyName("directory")]
    public string Directory
    {
        get;
        set;
    }

    [JsonPropertyName("modMetadata")]
    public AbstractModMetadata? ModMetadata
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
