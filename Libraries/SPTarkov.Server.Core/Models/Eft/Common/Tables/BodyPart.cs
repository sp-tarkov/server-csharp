using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record BodyPart
{
    public MinMax<double>? Chest
    {
        get;
        set;
    }

    public MinMax<double>? Head
    {
        get;
        set;
    }

    public MinMax<double>? LeftArm
    {
        get;
        set;
    }

    public MinMax<double>? LeftLeg
    {
        get;
        set;
    }

    public MinMax<double>? RightArm
    {
        get;
        set;
    }

    public MinMax<double>? RightLeg
    {
        get;
        set;
    }

    public MinMax<double>? Stomach
    {
        get;
        set;
    }
}
