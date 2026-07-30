using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2ModPagesResponse : IRequestData
{
    public required List<ModPage> Response { get; set; }
}
