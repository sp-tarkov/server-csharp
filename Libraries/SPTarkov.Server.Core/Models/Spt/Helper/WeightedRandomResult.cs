namespace SPTarkov.Server.Core.Models.Spt.Helper;

public record WeightedRandomResult<T>
{
    public required T Item
    {
        get;
        set;
    }

    public required int Index
    {
        get;
        set;
    }
}
