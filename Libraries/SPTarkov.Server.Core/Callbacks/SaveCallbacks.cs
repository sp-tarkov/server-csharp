using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.SaveCallbacks)]
public class SaveCallbacks(SaveServer saveServer, ConfigServer configServer, BackupService backupService) : IOnLoad, IOnUpdate
{
    private readonly CoreConfig _coreConfig = configServer.GetConfig<CoreConfig>();

    public async Task OnLoad()
    {
        await backupService.StartBackupSystem();
        await saveServer.LoadAsync();
    }

    public async Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        if (secondsSinceLastRun < _coreConfig.ProfileSaveIntervalInSeconds)
        {
            // Not enough time has passed since last run, exit early
            return false;
        }

        await saveServer.SaveAsync();

        return true;
    }
}
