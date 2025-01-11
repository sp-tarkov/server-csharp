using Core.Annotations;
using Core.Models.Enums;
using Core.Models.Spt.Config;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class GiftService
{
    /**
     * Does a gift with a specific ID exist in db
     * @param giftId Gift id to check for
     * @returns True if it exists in db
     */
    public bool GiftExists(string giftId)
    {
        throw new NotImplementedException();
    }

    public Gift GetGiftById(string giftId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get dictionary of all gifts
     * @returns Dict keyed by gift id
     */
    public Dictionary<string, Gift> GetGifts()
    {
        throw new NotImplementedException();
    }

    /**
     * Get an array of all gift ids
     * @returns string array of gift ids
     */
    public List<string> GetGiftIds()
    {
        throw new NotImplementedException();
    }

    /**
     * Send player a gift from a range of sources
     * @param playerId Player to send gift to / sessionId
     * @param giftId Id of gift in configs/gifts.json to send player
     * @returns outcome of sending gift to player
     */
    public GiftSentResult SendGiftToPlayer(string playerId, string giftId)
    {
        throw new NotImplementedException();
    }

    /**
     * Get sender id based on gifts sender type enum
     * @param giftData Gift to send player
     * @returns trader/user/system id
     */
    protected string? GetSenderId(Gift giftData)
    {
        throw new NotImplementedException();
    }

    /**
     * Convert GiftSenderType into a dialog MessageType
     * @param giftData Gift to send player
     * @returns MessageType enum value
     */
    protected MessageType? GetMessageType(Gift giftData)
    {
        throw new NotImplementedException();
    }

    /**
     * Prapor sends gifts to player for first week after profile creation
     * @param sessionId Player id
     * @param day What day to give gift for
     */
    public void SendPraporStartingGift(string sessionId, int day)
    {
        throw new NotImplementedException();
    }
}
