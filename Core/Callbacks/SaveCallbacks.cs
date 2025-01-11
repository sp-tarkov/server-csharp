using Core.Annotations;
using Core.DI;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using Core.Services;

namespace Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.SaveCallbacks)]
public class SaveCallbacks : OnLoad, OnUpdate
{
    protected SaveServer _saveServer;
    protected CoreConfig _coreConfig;
    protected BackupService _backupService;

    public SaveCallbacks(
        SaveServer saveServer,
        ConfigServer configServer,
        BackupService backupService
    )
    {
        _saveServer = saveServer;
        _coreConfig = configServer.GetConfig<CoreConfig>(ConfigTypes.CORE);
        _backupService = backupService;
    }

    public Task OnLoad()
    {
        _backupService.InitAsync();
        _saveServer.Load();
        
        return Task.CompletedTask;
    }

    public async Task<bool> OnUpdate(long SecondsSinceLastRun)
    {
        if (SecondsSinceLastRun > _coreConfig.ProfileSaveIntervalInSeconds)
        {
            _saveServer.Save();
            return true;
        }

        return false;
    }

    public string GetRoute()
    {
        return "spt-save";
    }
}
