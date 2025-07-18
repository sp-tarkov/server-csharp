using SPTarkov.Server.Core.Models.Eft.Ragfair;

namespace SPTarkov.Server.Core.Extensions
{
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
    }
}
