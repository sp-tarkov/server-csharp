using Core.Annotations;
using Core.DI;
using Core.Models.Eft.Profile;

namespace Core.Routers.SaveLoad;

[Injectable]
public class HealthSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return new List<HandledRoute>() { new HandledRoute("spt-health", false) };
    }

    public override SptProfile HandleLoad(SptProfile profile)
    {
        if (profile?.VitalityData == null)
            profile.VitalityData = new() { Health = null, Effects = null };

        profile.VitalityData.Health = new()
        {
            Hydration = 0,
            Energy = 0,
            Temperature = 0,
            Head = 0,
            Chest = 0,
            Stomach = 0,
            LeftArm = 0,
            RightArm = 0,
            LeftLeg = 0,
            RightLeg = 0
        };

        profile.VitalityData.Effects = new()
        {
            Head = new(),
            Chest = new(),
            Stomach = new(),
            LeftArm = new(),
            RightArm = new(),
            LeftLeg = new(),
            RightLeg = new()
        };

        return profile;
    }
}
