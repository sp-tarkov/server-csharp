using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2RegisterResponse : IRequestData
{
    public required bool Response
    {
        get;
        set;
    }

    public required List<MiniProfile> Profiles
    {
        get;
        set;
    }
}
