namespace SPTarkov.Server.Web.Models.Profiles;

public sealed record ProfilePrestigeEditModel
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int Level { get; set; }

    public bool Achieved { get; set; }

    public long AchievedAt { get; set; }
}
