using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
public class ConfigServer
{
    protected readonly FrozenSet<string> acceptableFileExtensions = ["json", "jsonc"];
    protected readonly FileUtil FileUtil;
    protected readonly JsonUtil JsonUtil;
    protected readonly ISptLogger<ConfigServer> Logger;
    private static readonly Dictionary<string, object> _configs = new();

    public ConfigServer(ISptLogger<ConfigServer> logger, JsonUtil jsonUtil, FileUtil fileUtil)
    {
        Logger = logger;
        JsonUtil = jsonUtil;
        FileUtil = fileUtil;

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
            throw new Exception($"Config: {configKey} is undefined. Ensure you have not broken it via editing");
        }

        return _configs[configKey.GetValue()] as T;
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

    public T GetConfigByString<T>(string configType)
        where T : BaseConfig
    {
        return _configs[configType] as T;
    }

    public void Initialize()
    {
        if (Logger.IsLogEnabled(LogLevel.Debug))
        {
            Logger.Debug("Importing configs...");
        }

        // Get all filepaths
        const string filepath = "./SPT_Data/configs/";
        var files = FileUtil.GetFiles(filepath);

        // Add file content to result
        foreach (var file in files)
        {
            if (acceptableFileExtensions.Contains(FileUtil.GetFileExtension(file)))
            {
                var type = GetConfigTypeByFilename(file);
                var deserializedContent = JsonUtil.DeserializeFromFile(file, type);

                if (deserializedContent == null)
                {
                    Logger.Error($"Config file: {file} is corrupt. Use a site like: https://jsonlint.com to find the issue.");
                    throw new Exception($"Server will not run until the: {file} config error mentioned above is  fixed");
                }

                _configs[$"spt-{FileUtil.StripExtension(file)}"] = deserializedContent;
            }
        }
    }

    private Type GetConfigTypeByFilename(string filename)
    {
        var type = Enum.GetValues<ConfigTypes>().First(en => en.GetValue().Contains(FileUtil.StripExtension(filename)));
        return type.GetConfigType();
    }
}
