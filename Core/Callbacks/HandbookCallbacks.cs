using Core.Annotations;
using Core.Controllers;
using Core.DI;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(OnLoad), TypePriority = OnLoadOrder.HandbookCallbacks)]
public class HandbookCallbacks(HandBookController _handBookController) : OnLoad
{
    public Task OnLoad()
    {
        _handBookController.Load();
        return Task.CompletedTask;
    }

    public string GetRoute()
    {
        return "spt-handbook";
    }
}
