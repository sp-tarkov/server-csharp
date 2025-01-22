using SptCommon.Annotations;
using Core.DI;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.SaveCallbacks)]
[Injectable(InjectableTypeOverride = typeof(OnUpdate), TypePriority = OnUpdateOrder.SaveCallbacks)]
public class SaveCallbacks(
    SaveServer _saveServer,
    ConfigServer _configServer,
    BackupService _backupService
    )
    : OnLoad, OnUpdate
{
    private readonly CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    public async Task OnLoad()
    {
        await _backupService.InitAsync();
        _saveServer.Load();
    }

    public Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        if (secondsSinceLastRun > _coreConfig.ProfileSaveIntervalInSeconds)
        {
            _saveServer.Save();
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public string GetRoute()
    {
        return "spt-save";
    }
}
