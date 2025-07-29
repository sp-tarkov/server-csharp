using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.HandbookCallbacks)]
public class HandbookCallbacks(HandBookController handBookController) : IOnLoad
{
    public Task OnLoad()
    {
        handBookController.Load();
        return Task.CompletedTask;
    }
}
