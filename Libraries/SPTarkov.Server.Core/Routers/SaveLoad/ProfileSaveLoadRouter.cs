using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Routers.SaveLoad;

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
        {
            profile.CharacterData = new Characters
            {
                PmcData = new PmcData(),
                ScavData = new PmcData()
            };
        }

        return profile;
    }
}
