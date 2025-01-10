using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Dialog;
using Core.Utils.Json.Converters;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class JsonUtil
{
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters =
        {
            new ListOrTConverterFactory(),
            new DictionaryOrListConverter(),
            new EftEnumConverter<SptAirdropTypeEnum>(),
            new EftEnumConverter<GiftSenderType>(),
            new EftEnumConverter<SeasonalEventType>(),
            new EftEnumConverter<ProfileChangeEventType>()
        }
    };

    public T? Deserialize<T>(string? json)
    {
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
    }

    public object? Deserialize(string? json, Type type)
    {
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize(json, type, jsonSerializerOptions);
    }


    public string? Serialize<T>(T? obj, bool indented = false)
    {
        jsonSerializerOptions.WriteIndented = indented;
        return obj == null ? null : JsonSerializer.Serialize(obj, jsonSerializerOptions);
    }

    public string? Serialize(object? obj, Type type, bool indented = false)
    {
        jsonSerializerOptions.WriteIndented = indented;
        return obj == null ? null : JsonSerializer.Serialize(obj, type, jsonSerializerOptions);
    }
}
