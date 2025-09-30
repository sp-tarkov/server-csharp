using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace SPTarkov.Server.Core.Extensions;

public static class ProductionExtensions
{
    /// <summary>
    ///     Has the craft completed
    ///     Ignores bitcoin farm/cultist circle as they're continuous crafts
    /// </summary>
    /// <param name="craft">Craft to check</param>
    /// <returns>True when craft is complete</returns>
    public static bool IsCraftComplete(this Production craft)
    {
        return craft.Progress >= craft.ProductionTime
            && !craft.IsCraftOfType(HideoutAreas.BitcoinFarm)
            && !craft.IsCraftOfType(HideoutAreas.CircleOfCultists);
    }

    /// <summary>
    ///     Is a craft from a particular hideout area
    /// </summary>
    /// <param name="craft">Craft to check</param>
    /// <param name="hideoutType">Type to check craft against</param>
    /// <returns>True if it is from that area</returns>
    public static bool IsCraftOfType(this Production craft, HideoutAreas hideoutType)
    {
        switch (hideoutType)
        {
            case HideoutAreas.WaterCollector:
                return craft.RecipeId == HideoutHelper.WaterCollectorId;
            case HideoutAreas.BitcoinFarm:
                return craft.RecipeId == HideoutHelper.BitcoinProductionId;
            case HideoutAreas.ScavCase:
                return craft.SptIsScavCase ?? false;
            case HideoutAreas.CircleOfCultists:
                return craft.SptIsCultistCircle ?? false;
            default:
                return false;
        }
    }

    /// <summary>
    /// Craft has completed, update its properties to reflect its new state
    /// </summary>
    /// <param name="production">Craft to update</param>
    public static void FlagCultistCircleCraftAsComplete(this Production production)
    {
        production.AvailableForFinish = true;

        // The client expects `Progress` to be 0, and `inProgress` to be false when a circle is complete
        production.Progress = 0;
        production.InProgress = false;
    }
}
