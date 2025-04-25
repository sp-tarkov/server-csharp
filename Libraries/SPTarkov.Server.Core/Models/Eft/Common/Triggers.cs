namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Triggers
{
    public Dictionary<string, List<DamageData>>? HandlerDamage
    {
        get;
        set;
    }
}
