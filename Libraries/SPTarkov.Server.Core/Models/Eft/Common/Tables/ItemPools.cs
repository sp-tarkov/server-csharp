namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ItemPools
{
    public Dictionary<string, double>? Backpack
    {
        get;
        set;
    }

    public Dictionary<string, double>? Pockets
    {
        get;
        set;
    }

    public Dictionary<string, double>? SecuredContainer
    {
        get;
        set;
    }

    public Dictionary<string, double>? SpecialLoot
    {
        get;
        set;
    }

    public Dictionary<string, double>? TacticalVest
    {
        get;
        set;
    }
}
