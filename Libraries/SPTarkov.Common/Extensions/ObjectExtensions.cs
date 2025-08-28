using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SPTarkov.Common.Extensions;

public static class ObjectExtensions
{
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _indexedProperties = new();
    private static readonly Lock _indexedPropertiesLockObject = new();

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
    ///     CARE WHEN USING THIS, THIS IS TO GET PROP ON A TYPE
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool ContainsJsonProperty<T>(this object? obj, T key)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(key);

        return TryGetCachedProperty(obj.GetType(), key.ToString(), out _);
    }

    public static T? GetByJsonProperty<T>(this object? obj, string? toLower)
    {
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(toLower);

        if (!TryGetCachedProperty(obj.GetType(), toLower, out var cachedProperty))
        {
            return default;
        }

        return (T?)cachedProperty.GetValue(obj);
    }

    public static List<object> GetAllPropertyValuesAsList(this object? obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        var list = obj.GetType().GetProperties();
        var result = new List<object>();

        foreach (var prop in list)
        {
            // Edge case
            if (Attribute.IsDefined(prop, typeof(JsonExtensionDataAttribute)))
            {
                if (prop.GetValue(obj) is not IDictionary<string, object> kvp)
                {
                    // Not a dictionary, skip iterating over its keys/values
                    continue;
                }

                result.AddRange(kvp.Select(jsonExtensionKvP => jsonExtensionKvP.Value));

                continue;
            }

            result.Add(prop.GetValue(obj));
        }

        return result;
    }

    public static Dictionary<string, object?> GetAllPropertiesAsDictionary(this object? obj)
    {
        if (obj is null)
        {
            return [];
        }

        var resultDict = new Dictionary<string, object?>();
        foreach (var prop in obj.GetType().GetProperties())
        {
            // Edge case
            if (Attribute.IsDefined(prop, typeof(JsonExtensionDataAttribute)))
            {
                if (prop.GetValue(obj) is not IDictionary<string, object> kvp)
                {
                    // Not a dictionary, skip iterating over its keys/values
                    continue;
                }

                foreach (var jsonExtensionKvP in kvp)
                {
                    // Add contents of prop into dictionary we return
                    resultDict.TryAdd(jsonExtensionKvP.Key, jsonExtensionKvP.Value);
                }

                continue;
            }

            // Normal prop
            resultDict.Add(prop.Name, prop.GetValue(obj));
        }

        return resultDict;
    }

    public static T? ToObject<T>(this JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<T>(json);
    }

    public static object AddAllToExtensionData(this object obj, Dictionary<string, object> extensionData)
    {
        foreach (var keyValuePair in extensionData)
        {
            obj.AddToExtensionData(keyValuePair.Key, keyValuePair.Value);
        }

        return obj;
    }

    public static object AddToExtensionData(this object obj, string key, object value)
    {
        obj.GetExtensionData().Add(key, value);
        return obj;
    }

    public static object RemoveFromExtensionData(this object obj, string key)
    {
        obj.GetExtensionData().Remove(key);
        return obj;
    }

    public static object RemoveAllFromExtensionData(this object obj)
    {
        obj.GetExtensionData().Clear();
        return obj;
    }

    public static Dictionary<string, object> GetExtensionData(this object obj)
    {
        if (!obj.TryGetExtensionData(out var extensionData))
        {
            throw new Exception(
                $"Attempted to get from extension data for type {obj.GetType().FullName}, but the type doesnt contain ExtensionData or it is null"
            );
        }
        return extensionData!;
    }

    public static bool TryGetExtensionData(this object obj, out Dictionary<string, object>? extensionData)
    {
        extensionData =
            obj.GetType().GetProperty("ExtensionData", BindingFlags.Instance | BindingFlags.Public)?.GetValue(obj)
            as Dictionary<string, object>;
        return extensionData is not null;
    }
}
