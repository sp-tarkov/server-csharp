using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Routers.SaveLoad;

[Injectable]
public class InsuranceSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new HandledRoute("spt-insurance", false)];
    }

    protected override SptProfile HandleLoadInternal(SptProfile profile)
    {
        profile.InsuranceList ??= [];

        return profile;
    }
}
