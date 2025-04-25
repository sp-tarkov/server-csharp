namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Stimulator
{
    public double? BuffLoopTime
    {
        get;
        set;
    }

    public Dictionary<string, List<Buff>>? Buffs
    {
        get;
        set;
    }
}
