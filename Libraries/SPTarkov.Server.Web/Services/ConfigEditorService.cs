using System.Globalization;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Loaders;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Web.Models.Configs;

namespace SPTarkov.Server.Web.Services;

[Injectable(InjectionType.Singleton)]
public class ConfigEditorService
{
    private static readonly string _configDirectory = Path.Combine("SPT_Data", "configs");
    private static readonly string _presetDirectory = Path.Combine("user", "config-presets");
    private static readonly JsonWriterOptions _presetJsonWriterOptions = new()
    {
        Indented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    // JSON pointer paths listed here are hidden from the structured Controls tab only.
    private static readonly string[] _ignoredStructuredEditorSectionPaths =
    [
        "/kind",
        "/projectName",
        "/compatibleTarkovVersion",
        "/serverName",
        "/serverStartTime",
        "/release",
    ];

    private static readonly IReadOnlyDictionary<ConfigTypes, string[]> _ignoredStructuredEditorSectionPathsByConfig =
        new Dictionary<ConfigTypes, string[]>();

    private readonly IServiceProvider _serviceProvider;
    private readonly JsonUtil _jsonUtil;
    private readonly IEnumerable<IConfigEditorConfigProvider> _configProviders;
    private readonly Dictionary<string, ConfigEditorPreset> _presets = [];
    private readonly Lock _presetLock = new();

    public ConfigEditorService(
        IServiceProvider serviceProvider,
        JsonUtil jsonUtil,
        IEnumerable<IConfigEditorConfigProvider> configProviders
    )
    {
        _serviceProvider = serviceProvider;
        _jsonUtil = jsonUtil;
        _configProviders = configProviders;
        LoadPresetsFromDisk();
    }

    public async Task<IReadOnlyList<ConfigEditorConfigSummary>> GetConfigSummariesAsync()
    {
        var cleanConfigs = await ConfigLoader.Initialize();
        var summaries = new List<ConfigEditorConfigSummary>();

        foreach (var descriptor in GetConfigDescriptors())
        {
            summaries.Add(await BuildSummaryAsync(descriptor, cleanConfigs));
        }

        return summaries.OrderBy(summary => summary.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<ConfigEditorSnapshot> GetSnapshotAsync(string configId)
    {
        var descriptor = GetConfigDescriptor(configId);
        var cleanConfigs = descriptor.ConfigType is null ? null : await ConfigLoader.Initialize();
        var summary = await BuildSummaryAsync(descriptor, cleanConfigs);
        var runtimeConfig = GetRuntimeConfig(descriptor);
        var cleanConfig = await GetCleanConfigAsync(descriptor, cleanConfigs);
        var runtimeJson = Serialize(runtimeConfig, summary.RuntimeType);
        var cleanJson = Serialize(cleanConfig, summary.RuntimeType);

        return new ConfigEditorSnapshot(
            summary,
            runtimeJson,
            cleanJson,
            summary.ModifiedByMods,
            BuildAddableObjectPaths(runtimeJson, cleanJson, summary.RuntimeType),
            descriptor.IgnoredSectionPaths
        );
    }

    public string FormatJson(string configId, string json)
    {
        var descriptor = GetConfigDescriptor(configId);
        var runtimeType = descriptor.RuntimeType;
        var deserialized = DeserializeConfig(json, runtimeType);
        return Serialize(deserialized, runtimeType);
    }

    public void ValidateJson(string configId, string json)
    {
        _ = FormatJson(configId, json);
    }

    public string ApplyToRuntime(string configId, string json)
    {
        return ApplyToRuntimeAsync(configId, json).GetAwaiter().GetResult();
    }

    public async Task<string> ApplyToRuntimeAsync(string configId, string json)
    {
        var descriptor = GetConfigDescriptor(configId);
        var runtimeType = descriptor.RuntimeType;
        var runtimeConfig = GetRuntimeConfig(descriptor);
        var editedConfig = DeserializeConfig(json, runtimeType);

        if (descriptor.Registration?.ApplyToRuntimeAsync is not null)
        {
            await descriptor.Registration.ApplyToRuntimeAsync(editedConfig, CancellationToken.None);
        }
        else
        {
            CopyWritableProperties(editedConfig, runtimeConfig, runtimeType);
        }

        return Serialize(GetRuntimeConfig(descriptor), runtimeType);
    }

    public async Task SaveToDiskAsync(string configId, string json)
    {
        var descriptor = GetConfigDescriptor(configId);
        var config = DeserializeConfig(json, descriptor.RuntimeType);

        await SaveConfigAsync(descriptor, config, CancellationToken.None);
    }

    public IReadOnlyList<ConfigEditorPreset> GetPresets()
    {
        lock (_presetLock)
        {
            return _presets.Values.OrderBy(preset => preset.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }

    public int GetPresetCount()
    {
        lock (_presetLock)
        {
            return _presets.Count;
        }
    }

    public ConfigEditorPreset SavePreset(string configId, string name, string json)
    {
        var configJsonById = BuildPresetConfigs(configId, json);
        var presetName = string.IsNullOrWhiteSpace(name) ? $"Preset {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}" : name.Trim();
        var now = DateTimeOffset.Now;

        lock (_presetLock)
        {
            var existingPreset = _presets.Values.FirstOrDefault(preset =>
                string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase)
            );

            if (existingPreset is not null)
            {
                var updatedPreset = existingPreset with { ConfigJsonById = configJsonById, UpdatedAt = now };
                _presets[updatedPreset.Id] = updatedPreset;
                SavePresetToDisk(updatedPreset);
                return updatedPreset;
            }

            var preset = new ConfigEditorPreset(new MongoId(), presetName, configJsonById, now, now);
            _presets[preset.Id] = preset;
            SavePresetToDisk(preset);
            return preset;
        }
    }

    public ConfigEditorPreset? GetPreset(string presetId)
    {
        lock (_presetLock)
        {
            return _presets.GetValueOrDefault(presetId);
        }
    }

    public bool DeletePreset(string presetId)
    {
        lock (_presetLock)
        {
            var removed = _presets.Remove(presetId);

            if (removed)
            {
                var filePath = GetPresetFilePath(presetId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return removed;
        }
    }

    public void ApplyPresetToRuntime(string presetId)
    {
        ApplyPresetToRuntimeAsync(presetId).GetAwaiter().GetResult();
    }

    public async Task ApplyPresetToRuntimeAsync(string presetId)
    {
        ConfigEditorPreset? preset;

        lock (_presetLock)
        {
            preset = _presets.GetValueOrDefault(presetId);
        }

        if (preset is null)
        {
            throw new InvalidOperationException("Preset was not found.");
        }

        foreach (var (configId, json) in preset.ConfigJsonById)
        {
            await ApplyToRuntimeAsync(configId, json);
        }
    }

    private void LoadPresetsFromDisk()
    {
        if (!Directory.Exists(_presetDirectory))
        {
            return;
        }

        foreach (var presetFile in Directory.GetFiles(_presetDirectory, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var preset = ReadPresetFromDisk(presetFile);
                if (preset is not null)
                {
                    _presets[preset.Id] = preset;
                }
            }
            catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException)
            {
                // Ignore invalid preset files so one bad preset does not break the editor page.
            }
        }
    }

    private static ConfigEditorPreset? ReadPresetFromDisk(string presetFile)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(presetFile));
        var root = document.RootElement;

        var id = root.GetProperty("Id").GetString();
        var name = root.GetProperty("Name").GetString();
        var configJsonById = ReadPresetConfigs(root);

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || configJsonById.Count == 0)
        {
            return null;
        }

        return new ConfigEditorPreset(
            id,
            name,
            configJsonById,
            root.GetProperty("CreatedAt").GetDateTimeOffset(),
            root.GetProperty("UpdatedAt").GetDateTimeOffset()
        );
    }

    private static IReadOnlyDictionary<string, string> ReadPresetConfigs(JsonElement root)
    {
        Dictionary<string, string> configJsonById = [];

        if (root.TryGetProperty("Configs", out var configsElement) && configsElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var savedConfig in configsElement.EnumerateObject())
            {
                configJsonById[savedConfig.Name] = savedConfig.Value.GetRawText();
            }

            return configJsonById;
        }

        var configId = root.GetProperty("ConfigId").GetString();
        var configElement = root.TryGetProperty("Config", out var configProperty) ? configProperty : root.GetProperty("Json");
        var configJson = configElement.ValueKind == JsonValueKind.String ? configElement.GetString() : configElement.GetRawText();

        if (!string.IsNullOrWhiteSpace(configId) && !string.IsNullOrWhiteSpace(configJson))
        {
            configJsonById[configId] = configJson;
        }

        return configJsonById;
    }

    private static void SavePresetToDisk(ConfigEditorPreset preset)
    {
        Directory.CreateDirectory(_presetDirectory);

        using var fileStream = File.Create(GetPresetFilePath(preset.Id));
        using var writer = new Utf8JsonWriter(fileStream, _presetJsonWriterOptions);

        writer.WriteStartObject();
        writer.WriteString(nameof(ConfigEditorPreset.Id), preset.Id);
        writer.WriteString(nameof(ConfigEditorPreset.Name), preset.Name);
        writer.WritePropertyName("Configs");
        writer.WriteStartObject();

        foreach (var (configId, json) in preset.ConfigJsonById.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase))
        {
            using var configDocument = JsonDocument.Parse(json);
            writer.WritePropertyName(configId);
            configDocument.RootElement.WriteTo(writer);
        }

        writer.WriteEndObject();
        writer.WriteString(nameof(ConfigEditorPreset.CreatedAt), preset.CreatedAt);
        writer.WriteString(nameof(ConfigEditorPreset.UpdatedAt), preset.UpdatedAt);
        writer.WriteEndObject();
    }

    private IReadOnlyDictionary<string, string> BuildPresetConfigs(string selectedConfigId, string selectedJson)
    {
        var formattedSelectedJson = FormatJson(selectedConfigId, selectedJson);
        Dictionary<string, string> configs = [];

        foreach (var descriptor in GetConfigDescriptors())
        {
            var configId = descriptor.Id;
            configs[configId] = string.Equals(configId, selectedConfigId, StringComparison.Ordinal)
                ? formattedSelectedJson
                : Serialize(GetRuntimeConfig(descriptor), descriptor.RuntimeType);
        }

        return configs;
    }

    private IReadOnlyList<ConfigEditorDescriptor> GetConfigDescriptors()
    {
        var descriptors = Enum.GetValues<ConfigTypes>()
            .Select(configType =>
            {
                var runtimeType = configType.GetConfigType();
                return new ConfigEditorDescriptor(
                    configType.ToString(),
                    GetDisplayName(configType),
                    Path.GetFileName(GetConfigFilePath(configType)),
                    runtimeType,
                    configType,
                    null,
                    GetIgnoredSectionPaths(configType)
                );
            })
            .ToList();

        foreach (var provider in _configProviders)
        {
            foreach (var registration in provider.GetConfigs())
            {
                descriptors.Add(BuildRegisteredDescriptor(registration));
            }
        }

        var duplicate = descriptors
            .GroupBy(descriptor => descriptor.Id, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Duplicate config editor id registered: {duplicate.Key}");
        }

        return descriptors;
    }

    private ConfigEditorDescriptor GetConfigDescriptor(string configId)
    {
        return GetConfigDescriptors()
                .FirstOrDefault(descriptor => string.Equals(descriptor.Id, configId, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Unknown config id: {configId}");
    }

    private static ConfigEditorDescriptor BuildRegisteredDescriptor(ConfigEditorConfigRegistration registration)
    {
        if (string.IsNullOrWhiteSpace(registration.Id))
        {
            throw new InvalidOperationException("Registered config editor config must have an id.");
        }

        if (string.IsNullOrWhiteSpace(registration.DisplayName))
        {
            throw new InvalidOperationException($"Registered config editor config {registration.Id} must have a display name.");
        }

        var runtimeType = registration.RuntimeType ?? registration.RuntimeConfig.GetType();

        if (!runtimeType.IsAssignableFrom(registration.RuntimeConfig.GetType()))
        {
            throw new InvalidOperationException(
                $"Registered config editor config {registration.Id} has runtime type {runtimeType.Name} "
                    + $"but runtime config is {registration.RuntimeConfig.GetType().Name}."
            );
        }

        return new ConfigEditorDescriptor(
            registration.Id.Trim(),
            registration.DisplayName.Trim(),
            GetRegisteredFileName(registration),
            runtimeType,
            null,
            registration,
            NormalizeJsonPaths(registration.IgnoredSectionPaths)
        );
    }

    private static string GetRegisteredFileName(ConfigEditorConfigRegistration registration)
    {
        if (!string.IsNullOrWhiteSpace(registration.FileName))
        {
            return registration.FileName;
        }

        if (!string.IsNullOrWhiteSpace(registration.FilePath))
        {
            return Path.GetFileName(registration.FilePath);
        }

        return "Runtime service";
    }

    private async Task<ConfigEditorConfigSummary> BuildSummaryAsync(
        ConfigEditorDescriptor descriptor,
        IReadOnlyDictionary<Type, BaseConfig>? cleanConfigs
    )
    {
        var runtimeType = descriptor.RuntimeType;
        var runtimeConfig = GetRuntimeConfig(descriptor);
        var cleanConfig = await GetCleanConfigAsync(descriptor, cleanConfigs);
        var runtimeJson = Serialize(runtimeConfig, runtimeType);
        var cleanJson = Serialize(cleanConfig, runtimeType);

        return new ConfigEditorConfigSummary(
            descriptor.Id,
            descriptor.DisplayName,
            descriptor.FileName,
            descriptor.ConfigType,
            runtimeType,
            !string.Equals(runtimeJson, cleanJson, StringComparison.Ordinal),
            runtimeJson.Length,
            cleanJson.Length,
            runtimeType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Count(property => property.GetIndexParameters().Length == 0),
            descriptor.Registration is not null
        );
    }

    private object GetRuntimeConfig(ConfigEditorDescriptor descriptor)
    {
        if (descriptor.Registration is not null)
        {
            return descriptor.Registration.RuntimeConfig;
        }

        return _serviceProvider.GetRequiredService(descriptor.RuntimeType);
    }

    private async Task<object> GetCleanConfigAsync(
        ConfigEditorDescriptor descriptor,
        IReadOnlyDictionary<Type, BaseConfig>? cleanConfigs,
        CancellationToken cancellationToken = default
    )
    {
        if (descriptor.ConfigType is not null)
        {
            if (cleanConfigs is null)
            {
                cleanConfigs = await ConfigLoader.Initialize();
            }

            if (!cleanConfigs.TryGetValue(descriptor.RuntimeType, out var cleanConfig))
            {
                throw new InvalidOperationException($"No clean disk config was loaded for {descriptor.RuntimeType.Name}.");
            }

            return cleanConfig;
        }

        if (descriptor.Registration is null)
        {
            throw new InvalidOperationException($"Config {descriptor.Id} does not have a registered loader.");
        }

        if (descriptor.Registration.LoadFromDiskAsync is not null)
        {
            var cleanConfig = await descriptor.Registration.LoadFromDiskAsync(cancellationToken);

            if (cleanConfig is null)
            {
                throw new InvalidOperationException($"Registered config {descriptor.DisplayName} did not load from disk.");
            }

            return cleanConfig;
        }

        if (!string.IsNullOrWhiteSpace(descriptor.Registration.FilePath) && File.Exists(descriptor.Registration.FilePath))
        {
            var cleanConfig = await _jsonUtil.DeserializeFromFileAsync(descriptor.Registration.FilePath, descriptor.RuntimeType);

            if (cleanConfig is null)
            {
                throw new InvalidOperationException($"Registered config {descriptor.DisplayName} did not deserialize from disk.");
            }

            return cleanConfig;
        }

        return CloneConfig(GetRuntimeConfig(descriptor), descriptor.RuntimeType);
    }

    private string Serialize(object config, Type runtimeType)
    {
        return _jsonUtil.Serialize(config, runtimeType, true) ?? string.Empty;
    }

    private object DeserializeConfig(string json, Type runtimeType)
    {
        var deserialized = _jsonUtil.Deserialize(json, runtimeType);

        if (deserialized is null)
        {
            throw new InvalidOperationException($"Config JSON did not deserialize into {runtimeType.Name}.");
        }

        return deserialized;
    }

    private object CloneConfig(object config, Type runtimeType)
    {
        return DeserializeConfig(Serialize(config, runtimeType), runtimeType);
    }

    private async Task SaveConfigAsync(ConfigEditorDescriptor descriptor, object config, CancellationToken cancellationToken)
    {
        if (descriptor.Registration?.SaveToDiskAsync is not null)
        {
            await descriptor.Registration.SaveToDiskAsync(config, cancellationToken);
            return;
        }

        var filePath = descriptor.Registration?.FilePath ?? GetConfigFilePath(descriptor.ConfigType!.Value);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidOperationException($"Config {descriptor.DisplayName} does not have a disk save target.");
        }

        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, Serialize(config, descriptor.RuntimeType), cancellationToken);
    }

    private static IReadOnlyDictionary<string, bool> BuildAddableObjectPaths(string runtimeJson, string cleanJson, Type runtimeType)
    {
        Dictionary<string, bool> addableObjectPaths = [];

        AddObjectPaths(addableObjectPaths, JsonNode.Parse(runtimeJson), runtimeType, string.Empty);
        AddObjectPaths(addableObjectPaths, JsonNode.Parse(cleanJson), runtimeType, string.Empty);

        return addableObjectPaths;
    }

    private static IReadOnlySet<string> GetIgnoredSectionPaths(ConfigTypes configType)
    {
        HashSet<string> ignoredPaths = [];

        foreach (var ignoredPath in _ignoredStructuredEditorSectionPaths)
        {
            ignoredPaths.Add(NormalizeJsonPath(ignoredPath));
        }

        if (_ignoredStructuredEditorSectionPathsByConfig.TryGetValue(configType, out var configIgnoredPaths))
        {
            foreach (var ignoredPath in configIgnoredPaths)
            {
                ignoredPaths.Add(NormalizeJsonPath(ignoredPath));
            }
        }

        return ignoredPaths;
    }

    private static void AddObjectPaths(IDictionary<string, bool> addableObjectPaths, JsonNode? node, Type? type, string path)
    {
        if (node is JsonObject jsonObject)
        {
            var objectType = UnwrapNullableType(type);

            if (TryGetListOrItemType(objectType, out var listOrItemType))
            {
                objectType = UnwrapNullableType(listOrItemType);
            }

            var allowAddProperties = !IsConcreteObjectType(objectType);

            if (addableObjectPaths.TryGetValue(path, out var existingAllowAddProperties))
            {
                addableObjectPaths[path] = existingAllowAddProperties && allowAddProperties;
            }
            else
            {
                addableObjectPaths[path] = allowAddProperties;
            }

            if (TryGetDictionaryValueType(objectType, out var dictionaryValueType))
            {
                foreach (var property in jsonObject)
                {
                    AddObjectPaths(addableObjectPaths, property.Value, dictionaryValueType, AppendJsonPath(path, property.Key));
                }

                return;
            }

            if (TryGetDictionaryOrListValueType(objectType, out dictionaryValueType))
            {
                foreach (var property in jsonObject)
                {
                    AddObjectPaths(addableObjectPaths, property.Value, dictionaryValueType, AppendJsonPath(path, property.Key));
                }

                return;
            }

            foreach (var property in GetSerializableProperties(objectType))
            {
                var propertyName = GetJsonPropertyName(property);

                if (jsonObject.TryGetPropertyValue(propertyName, out var propertyValue))
                {
                    AddObjectPaths(addableObjectPaths, propertyValue, property.PropertyType, AppendJsonPath(path, propertyName));
                }
            }

            return;
        }

        if (node is JsonArray jsonArray && TryGetElementType(type, out var elementType))
        {
            for (var index = 0; index < jsonArray.Count; index++)
            {
                AddObjectPaths(
                    addableObjectPaths,
                    jsonArray[index],
                    elementType,
                    AppendJsonPath(path, index.ToString(CultureInfo.InvariantCulture))
                );
            }
        }
    }

    private static IEnumerable<PropertyInfo> GetSerializableProperties(Type? type)
    {
        if (type is null)
        {
            return [];
        }

        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetIndexParameters().Length == 0 && property.GetCustomAttribute<JsonIgnoreAttribute>() is null);
    }

    private static string GetJsonPropertyName(PropertyInfo property)
    {
        return property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
    }

    private static bool IsConcreteObjectType(Type? type)
    {
        if (type is null || type == typeof(object) || type == typeof(JsonNode) || type == typeof(JsonObject))
        {
            return false;
        }

        return !TryGetDictionaryValueType(type, out _) && !TryGetDictionaryOrListValueType(type, out _) && !TryGetElementType(type, out _);
    }

    private static bool TryGetDictionaryValueType(Type? type, out Type valueType)
    {
        var dictionaryType = GetGenericInterfaceOrSelf(type, typeof(IDictionary<,>));

        if (dictionaryType is not null)
        {
            valueType = dictionaryType.GetGenericArguments()[1];
            return true;
        }

        valueType = typeof(object);
        return false;
    }

    private static bool TryGetDictionaryOrListValueType(Type? type, out Type valueType)
    {
        var dictionaryOrListType = GetGenericTypeOrSelf(type, "SPTarkov.Server.Core.Utils.Json.DictionaryOrList`2");

        if (dictionaryOrListType is not null)
        {
            valueType = dictionaryOrListType.GetGenericArguments()[1];
            return true;
        }

        valueType = typeof(object);
        return false;
    }

    private static bool TryGetListOrItemType(Type? type, out Type itemType)
    {
        var listOrItemType = GetGenericTypeOrSelf(type, "SPTarkov.Server.Core.Utils.Json.ListOrT`1");

        if (listOrItemType is not null)
        {
            itemType = listOrItemType.GetGenericArguments()[0];
            return true;
        }

        itemType = typeof(object);
        return false;
    }

    private static bool TryGetElementType(Type? type, out Type elementType)
    {
        type = UnwrapNullableType(type);

        if (type == typeof(string) || type is null)
        {
            elementType = typeof(object);
            return false;
        }

        if (type.IsArray)
        {
            elementType = type.GetElementType() ?? typeof(object);
            return true;
        }

        var enumerableType = GetGenericInterfaceOrSelf(type, typeof(IEnumerable<>));

        if (enumerableType is not null)
        {
            elementType = enumerableType.GetGenericArguments()[0];
            return true;
        }

        if (TryGetListOrItemType(type, out elementType))
        {
            return true;
        }

        if (TryGetDictionaryOrListValueType(type, out elementType))
        {
            return true;
        }

        elementType = typeof(object);
        return false;
    }

    private static Type? GetGenericInterfaceOrSelf(Type? type, Type genericTypeDefinition)
    {
        type = UnwrapNullableType(type);

        if (type is null)
        {
            return null;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition)
        {
            return type;
        }

        return type.GetInterfaces()
            .FirstOrDefault(candidate => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == genericTypeDefinition);
    }

    private static Type? GetGenericTypeOrSelf(Type? type, string genericTypeFullName)
    {
        type = UnwrapNullableType(type);

        if (type is null)
        {
            return null;
        }

        return type.IsGenericType && type.GetGenericTypeDefinition().FullName == genericTypeFullName ? type : null;
    }

    private static Type? UnwrapNullableType(Type? type)
    {
        return type is null ? null : Nullable.GetUnderlyingType(type) ?? type;
    }

    private static string AppendJsonPath(string path, string segment)
    {
        var escapedSegment = segment.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
        return string.IsNullOrEmpty(path) ? $"/{escapedSegment}" : $"{path}/{escapedSegment}";
    }

    private static string NormalizeJsonPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || path == "/")
        {
            return string.Empty;
        }

        return path.StartsWith("/", StringComparison.Ordinal) ? path : $"/{path}";
    }

    private static IReadOnlySet<string> NormalizeJsonPaths(IEnumerable<string> paths)
    {
        return paths.Select(NormalizeJsonPath).ToHashSet(StringComparer.Ordinal);
    }

    private static void CopyWritableProperties(object source, object destination, Type runtimeType)
    {
        foreach (var property in runtimeType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || !property.CanWrite || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            property.SetValue(destination, property.GetValue(source));
        }
    }

    private static string GetConfigFilePath(ConfigTypes configType)
    {
        var fileName = configType.GetValue().Replace("spt-", string.Empty, StringComparison.Ordinal);
        return Path.Combine(_configDirectory, $"{fileName}.json");
    }

    private static string GetPresetFilePath(string presetId)
    {
        return Path.Combine(_presetDirectory, $"{presetId}.json");
    }

    private static string GetDisplayName(ConfigTypes configType)
    {
        return string.Join(
            " ",
            configType
                .ToString()
                .Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => string.Concat(word[..1], word[1..].ToLowerInvariant()))
        );
    }

    private sealed record ConfigEditorDescriptor(
        string Id,
        string DisplayName,
        string FileName,
        Type RuntimeType,
        ConfigTypes? ConfigType,
        ConfigEditorConfigRegistration? Registration,
        IReadOnlySet<string> IgnoredSectionPaths
    );
}
