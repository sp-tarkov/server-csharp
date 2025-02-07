using Core.Models.Eft.Launcher;
using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2PasswordChangeResponse : IRequestData
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
