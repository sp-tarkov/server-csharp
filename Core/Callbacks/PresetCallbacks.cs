using Core.Annotations;
using Core.Controllers;
using Core.DI;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.PresetCallbacks)]
public class PresetCallbacks : OnLoad
{
    protected PresetController _presetController;

    public PresetCallbacks
    (
        PresetController presetController
    )
    {
        _presetController = presetController;
    }

    public async Task OnLoad()
    {
        _presetController.Initialize();
    }

    public string GetRoute()
    {
        return "spt-presets";
    }
}
