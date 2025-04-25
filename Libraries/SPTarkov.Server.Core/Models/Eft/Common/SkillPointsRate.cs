namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SkillPointsRate
{
    public SkillPointRate? Generator
    {
        get;
        set;
    }

    public SkillPointRate? AirFilteringUnit
    {
        get;
        set;
    }

    public SkillPointRate? WaterCollector
    {
        get;
        set;
    }

    public SkillPointRate? SolarPower
    {
        get;
        set;
    }
}
