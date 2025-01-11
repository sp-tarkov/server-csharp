using Core.Annotations;
using Core.DI;

namespace Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.PresetCallbacks)]
public class PresetCallbacks : OnLoad
{
    public PresetCallbacks()
    {
    }

    public async Task OnLoad()
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
}
