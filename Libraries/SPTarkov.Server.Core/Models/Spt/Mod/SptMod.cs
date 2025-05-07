using System.Reflection;
using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public class SptMod
{
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
