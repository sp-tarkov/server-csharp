using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Reference;

// This class is used by the JsonExtensionDataPatch, dont remove!
public class StaticReferences
{
    [JsonIgnore]
    private Dictionary<string, object> _reference = new();

    [JsonExtensionData]
    public Dictionary<string, object> Reference
    {
        get { return _reference; }
    }
}
