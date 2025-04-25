namespace SPTarkov.Server.Core.Models.Eft.Common;

public record PveSettings
{
    public List<string>? AvailableVersions
    {
        get;
        set;
    }

    public bool? ModeEnabled
    {
        get;
        set;
    }
}
