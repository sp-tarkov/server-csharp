namespace SPTarkov.Server.Core.Models.Eft.Common;

public record HideoutManagement
{
    public double? SkillPointsPerAreaUpgrade
    {
        get;
        set;
    }

    public double? SkillPointsPerCraft
    {
        get;
        set;
    }

    public double? CircleOfCultistsBonusPercent
    {
        get;
        set;
    }

    public double? ConsumptionReductionPerLevel
    {
        get;
        set;
    }

    public double? SkillBoostPercent
    {
        get;
        set;
    }

    public SkillPointsRate? SkillPointsRate
    {
        get;
        set;
    }

    public EliteSlots? EliteSlots
    {
        get;
        set;
    }
}
