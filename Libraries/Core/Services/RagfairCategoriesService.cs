using SptCommon.Annotations;
using Core.Models.Eft.Ragfair;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairCategoriesService
{
    /// <summary>
    /// Get a dictionary of each item the play can see in their flea menu, filtered by what is available for them to buy
    /// </summary>
    /// <param name="offers">All offers in flea</param>
    /// <param name="searchRequestData">Search criteria requested</param>
    /// <param name="fleaUnlocked">Can player see full flea yet (level 15 by default)</param>
    /// <returns>KVP of item tpls + count of offers</returns>
    public Dictionary<string, int> GetCategoriesFromOffers(
        List<RagfairOffer> offers,
        SearchRequestData searchRequestData,
        bool fleaUnlocked)
    {
        throw new NotImplementedException();
    }
}
