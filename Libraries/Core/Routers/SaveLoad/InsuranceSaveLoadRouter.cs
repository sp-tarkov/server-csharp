using Core.DI;
using Core.Models.Eft.Profile;
using SptCommon.Annotations;

namespace Core.Routers.SaveLoad;

[Injectable(InjectableTypeOverride = typeof(SaveLoadRouter))]
public class InsuranceSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new HandledRoute("spt-insurance", false)];
    }

    public override SptProfile HandleLoad(SptProfile profile)
    {
        if (profile.InsuranceList == null)
        {
            profile.InsuranceList = new List<Insurance>();
        }

        return profile;
    }
}
