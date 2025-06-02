using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class JsonUtil
{
    private static JsonSerializerOptions? jsonSerializerOptionsIndented;
    private static JsonSerializerOptions jsonSerializerOptionsNoIndent;

    public JsonUtil(
        IEnumerable<IJsonConverterRegistrator> registrators
        )
    {
        jsonSerializerOptionsNoIndent = new JsonSerializerOptions()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        foreach (var registrator in registrators)
        {
            foreach (var converter in registrator.GetJsonConverters())
            {
                jsonSerializerOptionsNoIndent.Converters.Add(converter);
            }
        }

        jsonSerializerOptionsIndented = new JsonSerializerOptions(jsonSerializerOptionsNoIndent)
        {
            WriteIndented = true
        };
    }

    /// <summary>
    ///     Convert JSON into an object
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>Deserialized object or null</returns>
    public T? Deserialize<T>(string? json)
    {
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, jsonSerializerOptionsNoIndent);
    }

    /// <summary>
    ///     Convert JSON into an object
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="type">The type of the object to deserialize to</param>
    /// <returns></returns>
    public object? Deserialize(string? json, Type type)
    {
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize(json, type, jsonSerializerOptionsNoIndent);
    }

    /// <summary>
    ///     Convert JSON into an object from a file
    /// </summary>
    /// <param name="file">The JSON File to read</param>
    /// <returns>T</returns>
    public T? DeserializeFromFile<T>(string file)
    {
        if (!File.Exists(file))
        {
            return default;
        }

        using (FileStream fs = new(file, FileMode.Open, FileAccess.Read))
        {
            return JsonSerializer.Deserialize<T>(fs, jsonSerializerOptionsNoIndent);
        }
    }

    /// <summary>
    ///     Convert JSON into an object from a file asynchronously
    /// </summary>
    /// <param name="file">The JSON File to read</param>
    /// <returns>T</returns>
    public async Task<T?> DeserializeFromFileAsync<T>(string file)
    {
        if (!File.Exists(file))
        {
            return default;
        }

        await using FileStream fs = new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

        return await JsonSerializer.DeserializeAsync<T>(fs, jsonSerializerOptionsNoIndent);
    }

    /// <summary>
    ///     Convert JSON into an object from a file
    /// </summary>
    /// <param name="file">The JSON File to read</param>
    /// <param name="type">The type of the object to deserialize to</param>
    /// <returns>object</returns>
    public object? DeserializeFromFile(string file, Type type)
    {
        if (!File.Exists(file))
        {
            return default;
        }

        using (FileStream fs = new(file, FileMode.Open, FileAccess.Read))
        {
            return JsonSerializer.Deserialize(fs, type, jsonSerializerOptionsNoIndent);
        }
    }

    /// <summary>
    ///     Convert JSON into an object from a file asynchronously
    /// </summary>
    /// <param name="file">The JSON File to read</param>
    /// <param name="type">The type of the object to deserialize to</param>
    /// <returns>object</returns>
    public async Task<object?> DeserializeFromFileAsync(string file, Type type)
    {
        if (!File.Exists(file))
        {
            return default;
        }

        await using FileStream fs = new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

        return await JsonSerializer.DeserializeAsync(fs, type, jsonSerializerOptionsNoIndent);
    }

    /// <summary>
    ///     Convert JSON into an object from a FileStream
    /// </summary>
    /// <param name="fs">The file stream to deserialize</param>
    /// <param name="type">The type of the object to deserialize to</param>
    /// <returns></returns>
    public object? DeserializeFromFileStream(FileStream fs, Type type)
    {
        return JsonSerializer.Deserialize(fs, type, jsonSerializerOptionsNoIndent);
    }

    /// <summary>
    ///     Convert JSON into an object from a FileStream asynchronously
    /// </summary>
    /// <param name="fs">The file stream to deserialize</param>
    /// <param name="type">The type of the object to deserialize to</param>
    /// <returns></returns>
    public async Task<object?> DeserializeFromFileStreamAsync(FileStream fs, Type type)
    {
        return await JsonSerializer.DeserializeAsync(fs, type, jsonSerializerOptionsNoIndent);
    }

    /// <summary>
    ///     Convert an object into JSON
    /// </summary>
    /// <typeparam name="T">Type of the object being serialised</typeparam>
    /// <param name="obj">Object to serialise</param>
    /// <param name="indented">Should JSON be indented</param>
    /// <returns>Serialised object as JSON, or null</returns>
    public string? Serialize<T>(T? obj, bool indented = false)
    {
        return obj == null ? null : JsonSerializer.Serialize(obj, indented ? jsonSerializerOptionsIndented : jsonSerializerOptionsNoIndent);
    }

    /// <summary>
    ///     Convert an object into JSON
    /// </summary>
    /// <param name="obj">Object to serialise</param>
    /// <param name="type">Type of object being serialized</param>
    /// <param name="indented">Should JSON be indented</param>
    /// <returns>Serialized text</returns>
    public string? Serialize(object? obj, Type type, bool indented = false)
    {
        return obj == null ? null : JsonSerializer.Serialize(obj, type, indented ? jsonSerializerOptionsIndented : jsonSerializerOptionsNoIndent);
    }
}
