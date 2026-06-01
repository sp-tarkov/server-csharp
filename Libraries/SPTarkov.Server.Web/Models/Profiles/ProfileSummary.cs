namespace SPTarkov.Server.Web.Models.Profiles;

public sealed record ProfileSummary(
    string Id,
    string Username,
    string Nickname,
    string Side,
    string Edition,
    int? Level,
    double? Experience,
    int ItemCount,
    int? ScavLevel,
    bool HasCharacter,
    bool IsInvalid,
    bool IsWiped,
    string Status
)
{
    public string LevelLabel
    {
        get { return Level is null ? "n/a" : Level.Value.ToString("N0"); }
    }
}
