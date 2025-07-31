using SPTarkov.Server.Core.Models.Eft.Ragfair;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Extensions;

public static class RagfairOfferExtensions
{
    /// <summary>
    ///     Is the passed in offer stale - end time > passed in time
    /// </summary>
    /// <param name="offer">Offer to check</param>
    /// <param name="time">Time to check offer against</param>
    /// <returns>True - offer is stale</returns>
    public static bool IsStale(this RagfairOffer offer, long time)
    {
        return offer.EndTime < time || (offer.Quantity) < 1;
    }

    /// <summary>
    ///     Does this offer come from a trader
    /// </summary>
    /// <param name="offer">Offer to check</param>
    /// <returns>True = from trader</returns>
    public static bool IsTraderOffer(this RagfairOffer offer)
    {
        if (offer.CreatedBy is not null)
        {
            return offer.CreatedBy == OfferCreator.Trader;
        }

        return offer.User.MemberType == MemberCategory.Trader;
    }

    /// <summary>
    /// Was this offer created by a human player
    /// </summary>
    /// <param name="offer"></param>
    /// <returns></returns>
    public static bool IsPlayerOffer(this RagfairOffer offer)
    {
        if (offer.CreatedBy is not null)
        {
            return offer.CreatedBy == OfferCreator.Player;
        }

        return false;
    }
}
