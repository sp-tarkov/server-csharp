using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.PresetCallbacks)]
public class PresetCallbacks(PresetController _presetController) : IOnLoad
{
    public Task OnLoad()
    {
        _presetController.Initialize();
        return Task.CompletedTask;
    }

    public string GetRoute()
    {
        return "spt-presets";
    }
}
