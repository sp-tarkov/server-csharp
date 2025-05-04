using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class ConfigServer
{
    protected readonly string[] acceptableFileExtensions = ["json", "jsonc"];
    protected FileUtil _fileUtil;
    protected JsonUtil _jsonUtil;
    protected ISptLogger<ConfigServer> _logger;
    private static Dictionary<string, object> _configs = new();

    public ConfigServer(ISptLogger<ConfigServer> logger, JsonUtil jsonUtil, FileUtil fileUtil)
    {
        _logger = logger;
        _jsonUtil = jsonUtil;
        _fileUtil = fileUtil;

        if (_configs.Count == 0)
        {
            Initialize();
        }
    }

    public T GetConfig<T>()
        where T : BaseConfig
    {
        var configKey = GetConfigKey(typeof(T));
        if (!_configs.ContainsKey(configKey.GetValue()))
        {
            throw new Exception(
                $"Config: {configKey} is undefined. Ensure you have not broken it via editing"
            );
        }

        return _configs[configKey.GetValue()] as T;
    }

    private ConfigTypes GetConfigKey(Type type)
    {
        var configEnumerable = Enum.GetValues<ConfigTypes>()
            .Where(e =>
            {
                return e.GetConfigType() == type;
            });
        if (!configEnumerable.Any())
        {
            throw new Exception($"Config of type {type.Name} is not mapped to any ConfigTypes");
        }

        return configEnumerable.First();
    }

    public T GetConfigByString<T>(string configType)
        where T : BaseConfig
    {
        return _configs[configType] as T;
    }

    public void Initialize()
    {
        if (_logger.IsLogEnabled(LogLevel.Debug))
        {
            _logger.Debug("Importing configs...");
        }

        // Get all filepaths
        const string filepath = "./Assets/configs/";
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
                    _logger.Error(
                        $"Config file: {file} is corrupt. Use a site like: https://jsonlint.com to find the issue."
                    );
                    throw new Exception(
                        $"Server will not run until the: {file} config error mentioned above is  fixed"
                    );
                }

                _configs[$"spt-{_fileUtil.StripExtension(file)}"] = deserializedContent;
            }
        }
    }

    private Type GetConfigTypeByFilename(string filename)
    {
        var type = Enum.GetValues<ConfigTypes>()
            .First(en =>
            {
                return en.GetValue().Contains(_fileUtil.StripExtension(filename));
            });
        return type.GetConfigType();
    }
}
