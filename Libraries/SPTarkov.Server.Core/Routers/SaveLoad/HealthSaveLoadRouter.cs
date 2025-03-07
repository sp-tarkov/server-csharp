using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Routers.SaveLoad;

[Injectable(InjectableTypeOverride = typeof(SaveLoadRouter))]
public class HealthSaveLoadRouter : SaveLoadRouter
{
    protected override List<HandledRoute> GetHandledRoutes()
    {
        return [new HandledRoute("spt-health", false)];
    }

    public override SptProfile HandleLoad(SptProfile profile)
    {
        DefaultVitality(profile.VitalityData);

        return profile;
    }

    public void DefaultVitality(Vitality? vitality)
    {
        vitality ??= new Vitality
        {
            Health = null,
            Energy = 0,
            Temperature = 0,
            Hydration = 0
        };

        vitality.Health = new Dictionary<string, BodyPartHealth>
        {
            {
                "Head", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "Chest", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "Stomach", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "LeftArm", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "RightArm", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "LeftLeg", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            },
            {
                "RightLeg", new BodyPartHealth
                {
                    Health = new CurrentMinMax
                    {
                        Current = 0
                    },
                    Effects = new Dictionary<string, BodyPartEffectProperties>()
                }
            }
        };
    }
}
