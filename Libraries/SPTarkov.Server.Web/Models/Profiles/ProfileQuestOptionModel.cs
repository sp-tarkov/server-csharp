namespace SPTarkov.Server.Web.Models.Profiles;

public sealed class ProfileQuestOptionModel
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public ProfileQuestOptionModel Clone()
    {
        return new ProfileQuestOptionModel
        {
            Id = Id,
            Label = Label,
        };
    }
}

