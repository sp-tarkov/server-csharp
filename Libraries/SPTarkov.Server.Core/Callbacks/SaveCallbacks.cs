using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.SaveCallbacks)]
[Injectable(InjectableTypeOverride = typeof(IOnUpdate), TypePriority = OnUpdateOrder.SaveCallbacks)]
public class SaveCallbacks(
    SaveServer _saveServer,
    ConfigServer _configServer,
    BackupService _backupService
) : IOnLoad, IOnUpdate
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

    public bool OnUpdate(long timeSinceLastRun)
    {
        if (timeSinceLastRun > _coreConfig.ProfileSaveIntervalInSeconds)
        {
            _saveServer.Save();
            return true;
        }

        return false;
    }
}
