using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class ConfigServer
{
    private ILogger _logger;
    protected Dictionary<string, object> configs;
    protected readonly string[] acceptableFileExtensions = ["json", "jsonc"];

    public ConfigServer(
        ILogger logger
        // TODO: We need JsonUtil here => JsonUtil jsonUtil,
    )
    {
        _logger = logger;
        Initialize();
    }
    
    public T GetConfig<T>(ConfigTypes configType) where T : BaseConfig
    {
        if (!configs.ContainsKey(configType.GetValue()))
        {
            throw new Exception($"Config: {configType} is undefined. Ensure you have not broken it via editing");
        }

        return configs[configType.GetValue()] as T;
    }

    public T GetConfigByString<T>(string configType) where T : BaseConfig
    {
        return configs[configType] as T;
    }

    public void Initialize()
    {
        _logger.Debug("Importing configs...");

        // Get all filepaths
        var filepath = "./assets/configs/";
        var files = Directory.GetFiles(filepath);

        // Add file content to result
        foreach (var file in files)
        {
            if (acceptableFileExtensions.Contains(Path.GetExtension(file)))
            {
                var fileContent = File.ReadAllText(file);
                var type = GetConfigTypeByFilename(file);
                var deserializedContent = JsonSerializer.Deserialize(fileContent, type);

                if (deserializedContent == null)
                {
                    _logger.Error($"Config file: {file} is corrupt. Use a site like: https://jsonlint.com to find the issue.");
                    throw new Exception($"Server will not run until the: {file} config error mentioned above is  fixed");
                }

                this.configs[$"spt-{Path.GetFileNameWithoutExtension(file)}"] = deserializedContent;
            }
        }

        /** TODO: deal with this:
        this.logger.info(`Commit hash: ${
            globalThis.G_COMMIT || "DEBUG"
        }`);
        this.logger.info(`Build date: ${
            globalThis.G_BUILDTIME || "DEBUG"
        }`);
        **/
    }

    private Type GetConfigTypeByFilename(string filename)
    {
        var type = Enum.GetValues<ConfigTypes>()
            .First(en => en.GetValue().Contains(Path.GetFileNameWithoutExtension(filename)));
        return type.GetConfigType();
    }
}