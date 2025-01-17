using Core.Models.Spt.Repeatable;

namespace Core.Utils.Extensions
{
    public static class ObjectExtensions
    {
        public static bool Contains<T>(this object obj, T key)
        {
            return obj.GetType().GetProperties().Any(x => x.Name == key.ToString());
        }

        public static T? Get<T>(this object obj, string toLower)
        {
            return (T?)obj.GetType().GetProperties().SingleOrDefault(p => p.GetJsonName() == toLower)?.GetValue(obj);
        }


        public static void Remove<T>(this EliminationTargetPool pool, T key)
        {

        }
    }
}
