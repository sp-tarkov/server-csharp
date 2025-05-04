using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public record LauncherV2VersionResponse : IRequestData
{
    public required LauncherV2CompatibleVersion Response { get; set; }
}
