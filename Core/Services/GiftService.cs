using Core.Annotations;
using Core.Helpers;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Servers;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class GiftService
{
    private readonly ILogger _logger;
    private readonly ConfigServer _configServer;
    private readonly ProfileHelper _profileHelper;
    private readonly GiftsConfig _giftConfig;

    public GiftService(
        ILogger logger,
        ConfigServer configServer,
        ProfileHelper profileHelper)
    {
        _logger = logger;
        _configServer = configServer;
        _profileHelper = profileHelper;

        _giftConfig = _configServer.GetConfig<GiftsConfig>(ConfigTypes.GIFTS);
    }

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
        _giftConfig.Gifts.TryGetValue(giftId, out var gift);

        return gift;
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
        var giftData = GetGiftById(giftId);
        if (giftData is null)
        {
            return GiftSentResult.FAILED_GIFT_DOESNT_EXIST;
        }

        var maxGiftsToSendCount = giftData.MaxToSendPlayer ?? 1;

        if (_profileHelper.PlayerHasRecievedMaxNumberOfGift(playerId, giftId, maxGiftsToSendCount))
        {
            _logger.Debug($"Player already received gift: {giftId}");

            return GiftSentResult.FAILED_GIFT_ALREADY_RECEIVED;
        }

        if (giftData.Items?.Count > 0 && giftData.CollectionTimeHours is not null)
        {
            _logger.Warning($"Gift {giftId} has items but no collection time limit, defaulting to 48 hours");
        }

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
        var giftId = day switch
        {
            1 => "PraporGiftDay1",
            2 => "PraporGiftDay2",
            _ => null
        };

        if (giftId is not null)
        {
            //var giftData = GetGiftById(giftId);
            if (!_profileHelper.PlayerHasRecievedMaxNumberOfGift(sessionId, giftId, 1))
            {
                SendGiftToPlayer(sessionId, giftId);
            }
        }
    }
}
