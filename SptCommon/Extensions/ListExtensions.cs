namespace SptCommon.Extensions;

public static class ListExtensions
{
    public static List<T> Splice<T>(this List<T> source, int index, int count)
    {
        var items = source.GetRange(index, count);
        source.RemoveRange(index, count);
        return items;
    }

    public static T PopFirst<T>(this IList<T> source)
    {
        var r = source.First();
        source.Remove(source.First());
        return r;
    }

    public static T PopLast<T>(this IList<T> source)
    {
        var r = source.Last();
        source.Remove(source.Last());
        return r;
    }
}
