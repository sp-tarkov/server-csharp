namespace SPTarkov.Server.Web.Models.Profiles;

public sealed class ProfileEditModel
{
    public string Id { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string PmcNickname { get; set; } = string.Empty;

    public int? PmcLevel { get; set; }

    public int? PmcExperience { get; set; }

    public int? ScavLevel { get; set; }

    public int? ScavExperience { get; set; }

    public bool HasCharacter { get; set; }

    public List<ProfileSkillEditModel> Skills { get; set; } = [];

    public List<ProfileHideoutAreaEditModel> HideoutAreas { get; set; } = [];

    public List<ProfileQuestEditModel> Quests { get; set; } = [];

    public List<ProfileQuestOptionModel> MissingQuests { get; set; } = [];

    public List<ProfileTraderEditModel> Traders { get; set; } = [];

    public ProfileEditModel Clone()
    {
        return new ProfileEditModel
        {
            Id = Id,
            Username = Username,
            PmcNickname = PmcNickname,
            PmcLevel = PmcLevel,
            PmcExperience = PmcExperience,
            ScavLevel = ScavLevel,
            ScavExperience = ScavExperience,
            HasCharacter = HasCharacter,
            Skills = Skills.Select(skill => skill.Clone()).ToList(),
            HideoutAreas = HideoutAreas.Select(area => area.Clone()).ToList(),
            Quests = Quests.Select(quest => quest.Clone()).ToList(),
            MissingQuests = MissingQuests.Select(quest => quest.Clone()).ToList(),
            Traders = Traders.Select(trader => trader.Clone()).ToList(),
        };
    }
}
