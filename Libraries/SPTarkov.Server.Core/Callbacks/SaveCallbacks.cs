using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.SaveCallbacks)]
public class SaveCallbacks(SaveServer saveServer, ConfigServer configServer, BackupService backupService) : IOnLoad, IOnUpdate
{
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();

    public async Task OnLoad()
    {
        await saveServer.LoadAsync();

        // Note: This has to happen after loading the saveServer so we don't backup corrupted profiles
        await backupService.StartBackupSystem();
    }

    public async Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        if (secondsSinceLastRun < CoreConfig.ProfileSaveIntervalInSeconds)
        {
            // Not enough time has passed since last run, exit early
            return false;
        }

        await saveServer.SaveAsync();

        return true;
    }
}
