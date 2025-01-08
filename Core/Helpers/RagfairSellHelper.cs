using Core.Models.Eft.Ragfair;

namespace Core.Helpers;

public class RagfairSellHelper
{
    /// <summary>
    /// Get the percent chance to sell an item based on its average listed price vs player chosen listing price
    /// </summary>
    /// <param name="averageOfferPriceRub">Price of average offer in roubles</param>
    /// <param name="playerListedPriceRub">Price player listed item for in roubles</param>
    /// <param name="qualityMultiplier">Quality multipler of item being sold</param>
    /// <returns>percent value</returns>
    public double CalculateSellChance(
        double averageOfferPriceRub,
        double playerListedPriceRub,
        double qualityMultiplier)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get list of item count and sell time (empty list = no sell)
    /// </summary>
    /// <param name="sellChancePercent">chance item will sell</param>
    /// <param name="itemSellCount">count of items to sell</param>
    /// <param name="sellInOneGo">All items listed get sold at once</param>
    /// <returns>List of purchases of item(s) listed</returns>
    public List<SellResult> RollForSale(double sellChancePercent, int itemSellCount, bool sellInOneGo = false)
    {
        throw new NotImplementedException();
    }
}
