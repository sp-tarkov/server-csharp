using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2TypesResponse : IRequestData
{
    public required Dictionary<string, string> Response
    {
        get;
        set;
    }
}
