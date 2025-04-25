namespace SPTarkov.Server.Core.Models.Eft.Common;

public record HandlerDamageObject
{
    public int? Amount
    {
        get;
        set;
    }

    public string? BodyPartColliderType
    {
        get;
        set;
    }
}
