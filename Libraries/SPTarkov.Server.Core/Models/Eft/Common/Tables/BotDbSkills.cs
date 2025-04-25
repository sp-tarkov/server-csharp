using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record BotDbSkills
{
    public Dictionary<string, MinMax<double>>? Common
    {
        get;
        set;
    }

    public Dictionary<string, MinMax<double>>? Mastering
    {
        get;
        set;
    }
}
