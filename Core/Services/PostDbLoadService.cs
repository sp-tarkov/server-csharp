namespace Core.Services;

public class PostDbLoadService
{
    public void PerformPostDbLoadActions()
    {
        throw new NotImplementedException();
    }

    protected void AdjustMinReserveRaiderSpawnChance()
    {
        throw new NotImplementedException();
    }

    protected void AddCustomLooseLootPositions()
    {
        throw new NotImplementedException();
    }

// BSG have two values for shotgun dispersion, we make sure both have the same value
    protected void FixShotgunDispersions()
    {
        throw new NotImplementedException();
    }

// Apply custom limits on bot types as defined in configs/location.json/botTypeLimits
    protected void AdjustMapBotLimits()
    {
        throw new NotImplementedException();
    }

    protected void AdjustLooseLootSpawnProbabilities()
    {
        throw new NotImplementedException();
    }

    protected void AdjustLocationBotValues()
    {
        throw new NotImplementedException();
    }

// Make Rogues spawn later to allow for scavs to spawn first instead of rogues filling up all spawn positions
    protected void FixRoguesSpawningInstantlyOnLighthouse()
    {
        throw new NotImplementedException();
    }

// Find and split waves with large numbers of bots into smaller waves - BSG appears to reduce the size of these
// waves to one bot when they're waiting to spawn for too long
    protected void SplitBotWavesIntoSingleWaves()
    {
        throw new NotImplementedException();
    }

// Make non-trigger-spawned raiders spawn earlier + always
    protected void AdjustLabsRaiderSpawnRate()
    {
        throw new NotImplementedException();
    }

    protected void AdjustHideoutCraftTimes(int overrideSeconds)
    {
        throw new NotImplementedException();
    }

// Adjust all hideout craft times to be no higher than the override
    protected void AdjustHideoutBuildTimes(int overrideSeconds)
    {
        throw new NotImplementedException();
    }

// Blank out the "test" mail message from prapor
    protected void RemovePraporTestMessage()
    {
        throw new NotImplementedException();
    }

// Check for any missing assorts inside each traders assort.json data, checking against traders questassort.json
    protected void ValidateQuestAssortUnlocksExist()
    {
        throw new NotImplementedException();
    }

    protected void SetAllDbItemsAsSellableOnFlea()
    {
        throw new NotImplementedException();
    }

    protected void AddMissingTraderBuyRestrictionMaxValue()
    {
        throw new NotImplementedException();
    }

    protected void ApplyFleaPriceOverrides()
    {
        throw new NotImplementedException();
    }

    protected void AddCustomItemPresetsToGlobals()
    {
        throw new NotImplementedException();
    }
}
