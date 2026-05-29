using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.SaveCallbacks)]
public class SaveCallbacks(SaveServer saveServer, BackupService backupService, CoreConfig coreConfig) : IOnLoad, IOnUpdate
{
    public async Task OnLoad(CancellationToken stoppingToken)
    {
        await saveServer.LoadAsync(stoppingToken);

        // Note: This has to happen after loading the saveServer so we don't backup corrupted profiles
        await backupService.StartBackupSystem(stoppingToken);
    }

    public async Task<bool> OnUpdate(long secondsSinceLastRun, CancellationToken cancellationToken)
    {
        if (secondsSinceLastRun < coreConfig.ProfileSaveIntervalInSeconds)
        {
            // Not enough time has passed since last run, exit early
            return false;
        }

        await saveServer.SaveAsync(cancellationToken);

        return true;
    }
}
