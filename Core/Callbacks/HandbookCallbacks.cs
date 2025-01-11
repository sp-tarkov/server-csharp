using Core.Annotations;
using Core.Controllers;
using Core.DI;

namespace Core.Callbacks;

[Injectable]
public class HandbookCallbacks : OnLoad
{
    protected HandBookController _handBookController;

    public HandbookCallbacks
    (
        HandBookController handBookController
    )
    {
        _handBookController = handBookController;
    }

    public async Task OnLoad()
    {
        _handBookController.Load();
    }

    public string GetRoute()
    {
        return "spt-handbook";
    }
}
