using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Loaders;

public static class ConfigLoader
{
    private static readonly string _configPath = Path.Combine("SPT_Data", "configs");
    private static readonly HashSet<string> _acceptableFileExtensions = [".json", ".jsonc"];

    public static async Task<IReadOnlyDictionary<Type, BaseConfig>> Initialize(
        ILogger? logger = null,
        CancellationToken cancellationToken = default
    )
    {
        if (logger is not null && logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Importing configs...");
        }

        var options = new JsonSerializerOptions()
        {
            // This is required for JSONC support
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#if DEBUG
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
#endif
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        var converters = new JsonConverter[]
        {
            new ListOrTConverterFactory(),
            new DictionaryOrListConverter(),
            new ProcessBaseTradeRequestDataConverter(),
            new StringToMongoIdConverter(),
            new EftEnumConverterFactory(),
            new EftListEnumConverterFactory(),
            new EnumerableConverterFactory(),
            new StringOrIntConverterFactory(),
            new FloatOrIrregularFloatArrayFactory(),
        };

        foreach (var converter in converters)
        {
            options.Converters.Add(converter);
        }

        Dictionary<Type, BaseConfig> configs = [];
        var files = new List<string>(Directory.GetFiles(_configPath, "*"));

        foreach (var file in files)
        {
            if (!_acceptableFileExtensions.Contains(Path.GetExtension(file)))
            {
                continue;
            }

            var configType = GetConfigTypeByFilename(file);

            if (configType == null)
            {
                logger?.LogError($"Config file: {file} has no associated ConfigTypes entry. Skipping.");
                continue;
            }

            object? deserializedContent = null;

            try
            {
                if (File.Exists(file))
                {
                    await using FileStream fs = new(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

                    deserializedContent = await JsonSerializer.DeserializeAsync(fs, configType, options, cancellationToken);
                }
            }
            catch (JsonException)
            {
                logger?.LogError("Config file: {fileName} failed to deserialize.", file);
                logger?.LogError("Server will not run until the: {fileName} config error mentioned above is fixed", file);
                throw;
            }

            if (deserializedContent == null)
            {
                logger?.LogError($"Config file: {file} is corrupt. Validate the file using a JSON validator.");
                throw new Exception($"Server will not run until the: {file} config error mentioned above is fixed");
            }

            configs[configType] = (BaseConfig)deserializedContent;
        }

        return configs;
    }

    private static Type? GetConfigTypeByFilename(string filename)
    {
        Func<ConfigTypes, bool> filterMethod = (entry => entry.GetValue().Contains(Path.GetFileNameWithoutExtension(filename)));

        if (!Enum.GetValues<ConfigTypes>().Any(filterMethod))
        {
            return null;
        }

        var type = Enum.GetValues<ConfigTypes>().First(filterMethod);
        return type.GetConfigType();
    }
}
