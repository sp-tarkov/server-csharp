namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SkillPointRate
{
    public double? ResourceSpent
    {
        get;
        set;
    }

    public double? PointsGained
    {
        get;
        set;
    }
}
