using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2ProfilesResponse : IRequestData
{
    public required List<MiniProfile> Response
    {
        get;
        set;
    }
}
