namespace Core.Services;

public class RagfairTaxService
{
    public void StoreClientOfferTaxValue(string sessionId, Dictionary<string, object> offer)
    {
        throw new NotImplementedException();
    }

    public void ClearStoredOfferTaxById(string offerIdToRemove)
    {
        throw new NotImplementedException();
    }

    public Dictionary<string, object> GetStoredClientOfferTaxValueById(string offerIdToGet)
    {
        throw new NotImplementedException();
    }

    /**
    // This method, along with CalculateItemWorth, is trying to mirror the client-side code found in the method "CalculateTaxPrice".
    // It's structured to resemble the client-side code as closely as possible - avoid making any big structure changes if it's not necessary.
     * @param item Item being sold on flea
     * @param pmcData player profile
     * @param requirementsValue
     * @param offerItemCount Number of offers being created
     * @param sellInOnePiece
     * @returns Tax in roubles
     */
    public double CalculateTax(
        Dictionary<string, object> item,
        Dictionary<string, object> pmcData,
        double requirementsValue,
        int offerItemCount,
        bool sellInOnePiece)
    {
        throw new NotImplementedException();
    }

    // This method is trying to replicate the item worth calculation method found in the client code.
    // Any inefficiencies or style issues are intentional and should not be fixed, to preserve the client-side code mirroring.
    protected double CalculateItemWorth(
        Dictionary<string, object> item,
        Dictionary<string, object> itemTemplate,
        int itemCount,
        Dictionary<string, object> pmcData,
        bool isRootItem = true)
    {
        throw new NotImplementedException();
    }
}
