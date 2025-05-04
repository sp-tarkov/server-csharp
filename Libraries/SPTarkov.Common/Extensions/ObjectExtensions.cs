using System.Reflection;
using System.Text.Json;

namespace SPTarkov.Common.Extensions;

public static class ObjectExtensions
{
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _indexedProperties =
        new();
    private static readonly Lock _indexedPropertiesLockObject = new();

    private static bool TryGetCachedProperty(Type type, string key, out PropertyInfo cachedProperty)
    {
        lock (_indexedPropertiesLockObject)
        {
            if (!_indexedProperties.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties()
                    .ToDictionary(
                        prop =>
                        {
                            return prop.GetJsonName();
                        },
                        prop =>
                        {
                            return prop;
                        }
                    );
                _indexedProperties.Add(type, properties);
            }

            return properties.TryGetValue(key, out cachedProperty);
        }
    }

    /// <summary>
    ///     CARE WHEN USING THIS, THIS IS TO GET PROP ON A TYPE
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool ContainsJsonProp<T>(this object? obj, T key)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(key);

        return TryGetCachedProperty(obj.GetType(), key.ToString(), out _);
    }

    public static T? GetByJsonProp<T>(this object? obj, string? toLower)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(toLower);

        if (!TryGetCachedProperty(obj.GetType(), toLower, out var cachedProperty))
        {
            return default;
        }

        return (T?)cachedProperty.GetValue(obj);
    }

    public static List<object> GetAllPropValuesAsList(this object? obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var list = obj.GetType().GetProperties();
        var result = new List<object>();

        foreach (var prop in list)
        {
            result.Add(prop.GetValue(obj));
        }

        return result;
    }

    public static Dictionary<string, object?> GetAllPropsAsDict(this object? obj)
    {
        var props = obj.GetType().GetProperties();

        return props.ToDictionary(
            prop =>
            {
                return prop.Name;
            },
            prop =>
            {
                return prop.GetValue(obj);
            }
        );
    }

    public static T ToObject<T>(this JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<T>(json);
    }
}
