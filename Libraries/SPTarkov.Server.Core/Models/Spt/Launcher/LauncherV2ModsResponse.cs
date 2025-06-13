using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2ModsResponse : IRequestData
{
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }

    public required Dictionary<string, AbstractModMetadata> Response
    {
        get;
        set;
    }
}
