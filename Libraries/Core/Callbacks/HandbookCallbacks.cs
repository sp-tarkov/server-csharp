using Core.Controllers;
using Core.DI;
using SptCommon.Annotations;

namespace Core.Callbacks;

[Injectable(InjectableTypeOverride = typeof(IOnLoad), TypePriority = OnLoadOrder.HandbookCallbacks)]
public class HandbookCallbacks(HandBookController _handBookController) : IOnLoad
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
