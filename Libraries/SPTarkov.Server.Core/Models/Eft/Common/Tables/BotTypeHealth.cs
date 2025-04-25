using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record BotTypeHealth
{
    public List<BodyPart>? BodyParts
    {
        get;
        set;
    }

    public MinMax<double>? Energy
    {
        get;
        set;
    }

    public MinMax<double>? Hydration
    {
        get;
        set;
    }

    public MinMax<double>? Temperature
    {
        get;
        set;
    }
}
