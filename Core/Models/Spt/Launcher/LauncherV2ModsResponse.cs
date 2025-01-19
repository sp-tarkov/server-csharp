using Core.Models.Spt.Mod;
using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2ModsResponse : IRequestData
{
    public required Dictionary<string, PackageJsonData> Response { get; set; }
}
