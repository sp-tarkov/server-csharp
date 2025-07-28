namespace SPTarkov.Server.Core.Extensions
{
    public static class UtilityExtensions
    {
        public static IEnumerable<T> IntersectWith<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            //a.Intersect(x => b.Contains(x)).ToList();
            // gives error Delegate type could not be inferred

            return first.Where(second.Contains);
        }
    }
}
