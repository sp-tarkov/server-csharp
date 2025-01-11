using Core.Annotations;
using Core.DI;

namespace Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.HandbookCallbacks)]
public class HandbookCallbacks : OnLoad
{
    public HandbookCallbacks()
    {
    }

    public Task OnLoad()
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
}
