using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Utils;
using SptCommon.Annotations;
using LogLevel = Core.Models.Spt.Logging.LogLevel;

namespace Core.Servers;

[Injectable(InjectionType.Singleton)]
public class ConfigServer
{
    protected static readonly string[] acceptableFileExtensions = ["json", "jsonc"];
    protected FileUtil _fileUtil;
    protected JsonUtil _jsonUtil;
    protected ISptLogger<ConfigServer> _logger;
    protected Dictionary<string, object> configs = new();

    public ConfigServer(
        ISptLogger<ConfigServer> logger,
        JsonUtil jsonUtil,
        FileUtil fileUtil
    )
    {
        _logger = logger;
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;
        Initialize();
    }

    public T GetConfig<T>() where T : BaseConfig
    {
        var configKey = GetConfigKey(typeof(T));
        if (!configs.ContainsKey(configKey.GetValue()))
        {
            throw new Exception($"Config: {configKey} is undefined. Ensure you have not broken it via editing");
        }

        return configs[configKey.GetValue()] as T;
    }

    private ConfigTypes GetConfigKey(Type type)
    {
        var configEnumerable = Enum.GetValues<ConfigTypes>().Where(e => e.GetConfigType() == type);
        if (!configEnumerable.Any())
        {
            throw new Exception($"Config of type {type.Name} is not mapped to any ConfigTypes");
        }

        return configEnumerable.First();
    }

    public T GetConfigByString<T>(string configType) where T : BaseConfig
    {
        return configs[configType] as T;
    }

    public void Initialize()
    {
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug("Importing configs...");
        }

        // Get all filepaths
        var filepath = "./assets/configs/";
        var files = _fileUtil.GetFiles(filepath);

        // Add file content to result
        foreach (var file in files)
        {
            if (acceptableFileExtensions.Contains(_fileUtil.GetFileExtension(file)))
            {
                var type = GetConfigTypeByFilename(file);
                var deserializedContent = _jsonUtil.DeserializeFromFile(file, type);

                if (deserializedContent == null)
                {
                    _logger.Error($"Config file: {file} is corrupt. Use a site like: https://jsonlint.com to find the issue.");
                    throw new Exception($"Server will not run until the: {file} config error mentioned above is  fixed");
                }

                configs[$"spt-{_fileUtil.StripExtension(file)}"] = deserializedContent;
            }
        }

        /** TODO: deal with this:
        this.logger.info(`Commit hash: {
            globalThis.G_COMMIT || "DEBUG"
        }`);
        this.logger.info(`Build date: {
            globalThis.G_BUILDTIME || "DEBUG"
        }`);
        **/
    }

    private Type GetConfigTypeByFilename(string filename)
    {
        var type = Enum.GetValues<ConfigTypes>()
            .First(en => en.GetValue().Contains(_fileUtil.StripExtension(filename)));
        return type.GetConfigType();
    }
}
