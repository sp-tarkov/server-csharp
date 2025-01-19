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

        public static bool Contains<T>(this object obj, T key)
        {
            return TryGetCachedProperty(obj.GetType(), key.ToString(), out _);
        }

        public static T? Get<T>(this object? obj, string? toLower)
        {
            ArgumentNullException.ThrowIfNull(obj);
            ArgumentNullException.ThrowIfNull(toLower);

            if (!TryGetCachedProperty(obj.GetType(), toLower, out var cachedProperty))
                return default;
            return (T?)cachedProperty.GetValue(obj);
        }
    }
}
