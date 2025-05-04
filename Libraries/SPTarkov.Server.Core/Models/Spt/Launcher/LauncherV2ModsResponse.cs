using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2ModsResponse : IRequestData
{
    public required Dictionary<string, PackageJsonData> Response { get; set; }
}
