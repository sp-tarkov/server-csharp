using Core.Controllers;
using Core.DI;
using SptCommon.Annotations;

namespace Core.Callbacks;

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
