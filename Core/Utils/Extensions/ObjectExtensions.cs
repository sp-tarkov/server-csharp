namespace Core.Utils.Extensions
{
    public static class ObjectExtensions
    {
        public static bool Contains<T>(this object obj, T key)
        {
            return obj.GetType().GetProperties().Any(x => x.Name == key.ToString());
        }
    }
}
