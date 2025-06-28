namespace SPTarkov.Server.Core.Extensions
{
    public static class UtilityExtensions
    {
        public static List<T> IntersectWith<T>(this List<T> first, List<T> second)
        {
            //a.Intersect(x => b.Contains(x)).ToList();
            // gives error Delegate type could not be infered

            return first.Where(x => second.Contains(x)).ToList();
        }
    }
}
