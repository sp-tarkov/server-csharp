namespace SPTarkov.Server.Core.Models.Eft.Common;

public record CoopSettings
{
    public List<string>? AvailableVersions
    {
        get;
        set;
    }
}
