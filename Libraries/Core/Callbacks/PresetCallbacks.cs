using SptCommon.Annotations;
using Core.Controllers;
using Core.DI;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.PresetCallbacks)]
public class PresetCallbacks(PresetController _presetController) : OnLoad
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
