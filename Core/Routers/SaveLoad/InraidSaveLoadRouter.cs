using Core.Annotations;
using Core.DI;
using Core.Models.Eft.Profile;

namespace Core.Routers.SaveLoad;

[Injectable]
public class InraidSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>() { new HandledRoute("spt-inraid", false) };
    }

    public override SptProfile HandleLoad(SptProfile profile)
    {
        if (profile.InraidData == null)
            profile.InraidData = new Inraid { Location = "none", Character = "none" };

        return profile;
    }
}
