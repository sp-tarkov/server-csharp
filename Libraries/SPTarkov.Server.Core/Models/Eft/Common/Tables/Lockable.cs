namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Lockable
{
    public bool? Locked
    {
        get;
        set;
    }
}
