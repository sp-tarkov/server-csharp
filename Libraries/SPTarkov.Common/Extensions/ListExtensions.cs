namespace SPTarkov.Common.Extensions;

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
        var r = source[0];
        source.Remove(source[0]);
        return r;
    }

    public static T PopLast<T>(this IList<T> source)
    {
        var r = source[^1];
        source.Remove(source[^1]);
        return r;
    }
}
