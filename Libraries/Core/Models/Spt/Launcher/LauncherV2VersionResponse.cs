using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public record LauncherV2VersionResponse : IRequestData
{
    public required LauncherV2CompatibleVersion Response
    {
        get;
        set;
    }
}
