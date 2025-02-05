using SptCommon.Annotations;
using Core.DI;
using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;

namespace Core.Routers.SaveLoad;

[Injectable(InjectableTypeOverride = typeof(SaveLoadRouter))]
public class ProfileSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new HandledRoute("spt-profile", false)];
    }

    public override SptProfile HandleLoad(SptProfile profile)
    {
        if (profile.CharacterData == null)
            profile.CharacterData = new Characters { PmcData = new PmcData(), ScavData = new PmcData() };

        return profile;
    }
}
