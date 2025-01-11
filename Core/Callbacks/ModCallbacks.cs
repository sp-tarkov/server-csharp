using Core.Annotations;
using Core.DI;

namespace Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.ModCallbacks)]
public class ModCallbacks : OnLoad
{
    public ModCallbacks()
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
