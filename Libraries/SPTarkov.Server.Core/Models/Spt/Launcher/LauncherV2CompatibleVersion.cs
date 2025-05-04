using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2CompatibleVersion : IRequestData
{
    public required string SptVersion { get; set; }

    public required string EftVersion { get; set; }
}
