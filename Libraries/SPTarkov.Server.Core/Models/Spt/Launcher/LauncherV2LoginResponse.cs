using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2LoginResponse : IRequestData
{
    public required bool Response { get; set; }
}
