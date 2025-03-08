using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Callbacks;

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
