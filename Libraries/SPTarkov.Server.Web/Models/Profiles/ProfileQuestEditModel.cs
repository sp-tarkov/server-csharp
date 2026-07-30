using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Web.Models.Profiles;

public sealed class ProfileQuestEditModel
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public QuestStatusEnum Status { get; set; }

    public bool IsNew { get; set; }

    public ProfileQuestEditAction PendingAction { get; set; }

    public ProfileQuestEditModel Clone()
    {
        return new ProfileQuestEditModel
        {
            Id = Id,
            Label = Label,
            Status = Status,
            IsNew = IsNew,
            PendingAction = PendingAction,
        };
    }
}

public enum ProfileQuestEditAction
{
    None,
    AvailableForFinish,
    Restart,
}
