namespace SPTarkov.Server.Web.Models.Profiles;

public sealed class ProfileSkillEditModel
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int Level { get; set; }

    public double ProgressInLevel { get; set; }

    public double TotalProgress
    {
        get { return Level >= 51 ? 5100d : (Level * 100d) + Math.Clamp(ProgressInLevel, 0d, 99.99d); }
    }

    public ProfileSkillEditModel Clone()
    {
        return new ProfileSkillEditModel
        {
            Id = Id,
            Label = Label,
            Level = Level,
            ProgressInLevel = ProgressInLevel,
        };
    }
}
