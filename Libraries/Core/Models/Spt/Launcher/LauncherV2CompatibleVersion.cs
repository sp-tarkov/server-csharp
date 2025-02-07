using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2CompatibleVersion : IRequestData
{
    public required string SptVersion
    {
        get;
        set;
    }

    public required string EftVersion
    {
        get;
        set;
    }
}
