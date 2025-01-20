using System.Reflection;

namespace SptCommon.Extensions
{
    public static class ObjectExtensions
    {
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _indexedProperties = new();
        private static readonly object _indexedPropertiesLockObject = new();

        private static bool TryGetCachedProperty(Type type, string key, out PropertyInfo cachedProperty)
        {
            lock (_indexedPropertiesLockObject)
            {
                if (!_indexedProperties.TryGetValue(type, out var properties))
                {
                    properties = type.GetProperties().ToDictionary(prop => prop.GetJsonName(), prop => prop);
                    _indexedProperties.Add(type, properties);
                }

                return properties.TryGetValue(key, out cachedProperty);
            }
        }

        /// <summary>
        /// CARE WHEN USING THIS, THIS IS TO GET PROP ON A TYPE
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool ContainsJsonProp<T>(this object? obj, T key)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            
            return TryGetCachedProperty(obj.GetType(), key.ToString(), out _);
        }

        public static T? GetByJsonProp<T>(this object? obj, string? toLower)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(toLower);

            if (!TryGetCachedProperty(obj.GetType(), toLower, out var cachedProperty))
                return default;
            return (T?)cachedProperty.GetValue(obj);
        }
    }
}
