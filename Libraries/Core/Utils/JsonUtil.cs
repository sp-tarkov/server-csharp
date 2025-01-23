using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SptCommon.Annotations;
using Core.Models.Eft.Ws;
using Core.Models.Enums;
using Core.Models.Spt.Dialog;
using Core.Utils.Json.Converters;

namespace Core.Utils;

[Injectable(InjectionType.Singleton)]
public class JsonUtil
{
    private static readonly JsonSerializerOptions jsonSerializerOptionsNoIndent = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new ListOrTConverterFactory(),
            new DictionaryOrListConverter(),
            new EftEnumConverter<SptAirdropTypeEnum>(),
            new EftEnumConverter<GiftSenderType>(),
            new EftEnumConverter<SeasonalEventType>(),
            new EftEnumConverter<ProfileChangeEventType>(),
            new EftEnumConverter<QuestStatusEnum>(),
            new EftEnumConverter<RewardType>(),
            new EftEnumConverter<SideType>(),
            new EftEnumConverter<BonusSkillType>(),
            new EftEnumConverter<NotificationEventType>(),
            new EftEnumConverter<QuestTypeEnum>(),
            new EftEnumConverter<RewardType>(),
            new BaseInteractionRequestDataConverter()
        }
    };
    private static readonly JsonSerializerOptions jsonSerializerOptionsIndented = new(jsonSerializerOptionsNoIndent)
    {
        WriteIndented = true
    };
    

    public T? Deserialize<T>(string? json)
    {
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, jsonSerializerOptionsNoIndent);
    }

    public object? Deserialize(string? json, Type type)
    {
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize(json, type, jsonSerializerOptionsNoIndent);
    }

    public string? Serialize<T>(T? obj, bool indented = false)
    {
        return obj == null ? null : JsonSerializer.Serialize(obj, indented ? jsonSerializerOptionsIndented : jsonSerializerOptionsNoIndent);
    }

    public string? Serialize(object? obj, Type type, bool indented = false)
    {
        return obj == null ? null : JsonSerializer.Serialize(obj, type, indented ? jsonSerializerOptionsIndented : jsonSerializerOptionsNoIndent);
    }
}
