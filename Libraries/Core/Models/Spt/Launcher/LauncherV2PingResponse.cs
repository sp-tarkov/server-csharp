using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2PingResponse : IRequestData
{
    public required string Response
    {
        get;
        set;
    }
}
