using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace SPTarkov.Server.Core.Servers;

[Injectable(InjectionType.Singleton)]
[Obsolete("This class will be removed in SPT 4.2 in favor for directly injecting the configuration into classes")]
public class ConfigServer(IReadOnlyDictionary<Type, BaseConfig> configs)
{
    [Obsolete("This method will be removed in SPT 4.2 in favor for directly injecting the configuration into classes")]
    public T GetConfig<T>()
        where T : BaseConfig
    {
        return configs.TryGetValue(typeof(T), out var cfg)
            ? (T)cfg
            : throw new InvalidOperationException($"Config of type {typeof(T).Name} is missing.");
    }
}
