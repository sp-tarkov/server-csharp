using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Dialog;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils.Json.Converters;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton)]
public class JsonUtil(ISptLogger<JsonUtil> logger)
{
    private static JsonSerializerOptions jsonSerializerOptionsNoIndent = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new BaseSptLoggerReferenceConverter(),
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
            new EftEnumConverter<ExitStatus>(),
            new EftEnumConverter<MemberCategory>(),
            new EftEnumConverter<PinLockState>(),
            new EftEnumConverter<PlayerSideMask>(),
            new EftEnumConverter<DamageEffectType>(),
            new EftEnumConverter<RepairStrategyType>(),
            new EftEnumConverter<ThrowWeapType>(),
            new EftEnumConverter<EventType>(),
            new EftEnumConverter<TraderServiceType>(),
            new EftEnumConverter<CurrencyType>(),
            new EftEnumConverter<RadioStationType>(),
            new EftEnumConverter<ArmorMaterial>(),
            new EftEnumConverter<RequirementState>(),
            new EftEnumConverter<ExfiltrationType>(),
            new EftEnumConverter<EquipmentSlots>(),
            new EftEnumConverter<BuffType>(),
            new EftEnumConverter<BodyPartColliderType>(),

            new EftEnumConverter<LogLevel>(),
            new EftEnumConverter<LogTextColor>(),
            new EftEnumConverter<LogBackgroundColor>(),

            new EftListEnumConverter<EquipmentSlots>(),
            new EftListEnumConverter<PlayerSide>(),
            new EftListEnumConverter<DamageType>(),
            new BaseInteractionRequestDataConverter()
        }
    };

    protected static JsonSerializerOptions jsonSerializerOptionsIndented = new(jsonSerializerOptionsNoIndent)
    {
        WriteIndented = true
    };

    /// <summary>
    ///     Convert JSON into an object
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to</typeparam>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>Deserialized object or null</returns>
    public T? Deserialize<T>(string? json)
    {
        try
        {
            return string.IsNullOrEmpty(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, jsonSerializerOptionsNoIndent);
        }
        catch (Exception e)
        {
            if (logger.IsLogEnabled(LogLevel.Debug)) logger.Debug("failed to parse json" + json);
            throw e;
        }
    }

    /// <summary>
    ///     Convert JSON into an object
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="type">The type of the object to deserialize to</param>
    /// <returns></returns>
    public object? Deserialize(string? json, Type type)
    {
        try
        {
            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize(json, type, jsonSerializerOptionsNoIndent);
        }
        catch (Exception e)
        {
            if (logger.IsLogEnabled(LogLevel.Debug)) logger.Debug("failed to parse json" + json);
            throw e;
        }
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

    protected static void AddConverter(JsonSerializerOptions options, JsonConverter newConverter)
    {
        if (options.Converters.All(c => c.GetType() != newConverter.GetType()))
        {
            // Doesn't exist, add
            options.Converters.Add(newConverter);
        }
    }

    /// <summary>
    ///     Register a Json converter to serializer options
    /// </summary>
    /// <param name="converter">The converter to add</param>
    public void RegisterJsonConverter(JsonConverter converter)
    {
        // This might actually be a terrible thing to do, but it is what it is for now

        if (!jsonSerializerOptionsNoIndent.IsReadOnly)
        {
            AddConverter(jsonSerializerOptionsNoIndent, converter);
        }
        else
        {
            var noIndentConverter = new JsonSerializerOptions(jsonSerializerOptionsNoIndent);
            AddConverter(noIndentConverter, converter);
            jsonSerializerOptionsNoIndent = noIndentConverter;
        }

        if (!jsonSerializerOptionsIndented.IsReadOnly)
        {
            AddConverter(jsonSerializerOptionsIndented, converter);
        }
        else
        {
            var indentedConverter = new JsonSerializerOptions(jsonSerializerOptionsIndented);
            AddConverter(indentedConverter, converter);
            jsonSerializerOptionsIndented = indentedConverter;
        }
    }
}
