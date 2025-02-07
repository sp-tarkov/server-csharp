using Core.Models.Eft.Launcher;
using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2ProfilesResponse : IRequestData
{
    public required List<MiniProfile> Response
    {
        get;
        set;
    }
}
