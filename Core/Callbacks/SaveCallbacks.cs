using Core.DI;
using Core.Models.Spt.Config;

namespace Core.Callbacks;

public class SaveCallbacks : OnLoad, OnUpdate
{
    private CoreConfig _coreConfig;

    public SaveCallbacks()
    {
    }

    public async Task OnLoad()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> OnUpdate(long SecondsSinceLastRun)
    {
        throw new NotImplementedException();
    }

    public string GetRoute()
    {
        throw new NotImplementedException();
    }
}