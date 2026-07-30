namespace SPTarkov.Server.Web.Models.Profiles;

public sealed class ProfileHideoutAreaEditModel
{
    public string Type { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int Level { get; set; }

    public int MaxLevel { get; set; }

    public bool Active { get; set; }

    public bool PassiveBonusesEnabled { get; set; }

    public bool Constructing { get; set; }

    public bool CompleteConstruction { get; set; }

    public int? CompleteTime { get; set; }

    public ProfileHideoutAreaEditModel Clone()
    {
        return new ProfileHideoutAreaEditModel
        {
            Type = Type,
            Label = Label,
            Level = Level,
            MaxLevel = MaxLevel,
            Active = Active,
            PassiveBonusesEnabled = PassiveBonusesEnabled,
            Constructing = Constructing,
            CompleteConstruction = CompleteConstruction,
            CompleteTime = CompleteTime,
        };
    }
}

