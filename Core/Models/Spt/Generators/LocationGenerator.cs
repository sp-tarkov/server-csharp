namespace Core.Models.Spt.Generators;

public class LocationGenerator
{
    public StaticContainerProps GenerateContainerLoot(StaticContainerProps containerIn, List<StaticForcedProps> staticForced,
        Dictionary<string, StaticLootDetails> staticLootDist, Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist, string locationName)
    {
        throw new NotImplementedException();
    }

    public List<SpawnpointTemplate> GenerateDynamicLoot(LooseLoot dynamicLootDist, Dictionary<string, List<StaticAmmoDetails>> staticAmmoDist,
        string locationName)
    {
        throw new NotImplementedException();
    }
}