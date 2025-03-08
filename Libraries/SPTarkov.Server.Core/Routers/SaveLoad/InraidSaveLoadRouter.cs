using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers.SaveLoad;

[Injectable(InjectableTypeOverride = typeof(SaveLoadRouter))]
public class InraidSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new HandledRoute("spt-inraid", false)];
    }

    public override SptProfile HandleLoad(SptProfile profile)
    {
        if (profile.InraidData == null)
        {
            profile.InraidData = new Inraid
            {
                Location = "none",
                Character = "none"
            };
        }

        return profile;
    }
}
