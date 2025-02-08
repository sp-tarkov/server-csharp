using Core.DI;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.SaveCallbacks)]
[Injectable(InjectableTypeOverride = typeof(OnUpdate), TypePriority = OnUpdateOrder.SaveCallbacks)]
public class SaveCallbacks(
    SaveServer _saveServer,
    ConfigServer _configServer,
    BackupService _backupService
)
    : IOnLoad, OnUpdate
{
    private readonly CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

    public async Task OnLoad()
    {
        _backupService.StartBackupSystem();
        _saveServer.Load();
    }

    public string GetRoute()
    {
        return "spt-save";
    }

    public bool OnUpdate(long secondsSinceLastRun)
    {
        if (secondsSinceLastRun > _coreConfig.ProfileSaveIntervalInSeconds)
        {
            _saveServer.Save();
            return true;
        }

        return false;
    }
}
