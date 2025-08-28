using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Routers.SaveLoad;

[Injectable]
public class HealthSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new HandledRoute("spt-health", false)];
    }

    protected override SptProfile HandleLoadInternal(SptProfile profile)
    {
        return profile;
    }
}
