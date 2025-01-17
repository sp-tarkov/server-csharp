namespace Core.Models.Spt.Helper;

public record WeightedRandomResult<T>
{
    public T Item { get; set; }
    public int Index { get; set; }
}
