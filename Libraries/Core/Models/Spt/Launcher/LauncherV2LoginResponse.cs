using Core.Models.Utils;

namespace Core.Models.Spt.Launcher;

public class LauncherV2LoginResponse : IRequestData
{
    public required bool Response { get; set; }
}
