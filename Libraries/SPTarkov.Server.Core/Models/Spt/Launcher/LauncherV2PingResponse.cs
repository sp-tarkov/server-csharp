using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2PingResponse : IRequestData
{
    public required string Response
    {
        get;
        set;
    }
}
