using Core.Models.Eft.Profile;
using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2ProfileResponse : IRequestData
{
    public SptProfile Response
    {
        get;
        set;
    }
}
