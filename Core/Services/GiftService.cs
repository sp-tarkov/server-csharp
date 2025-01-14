using Core.Annotations;
using Core.Helpers;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Spt.Dialog;
using Core.Servers;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class GiftService
{
    private readonly ILogger _logger;

    private readonly MailSendService _mailSendService;
    private readonly LocalisationService _localisationService;
    private readonly HashUtil _hashUtil;
    private readonly TimeUtil _timeUtil;
    private readonly ProfileHelper _profileHelper;
    private readonly ConfigServer _configServer;

    private readonly GiftsConfig _giftConfig;

    public GiftService
    (
        ILogger logger,
        MailSendService mailSendService,
        LocalisationService localisationService,
        HashUtil hashUtil,
        TimeUtil timeUtil,
        ProfileHelper profileHelper,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _mailSendService = mailSendService;
        _localisationService = localisationService;
        _hashUtil = hashUtil;
        _timeUtil = timeUtil;
        _profileHelper = profileHelper;
        _configServer = configServer;

        _giftConfig = _configServer.GetConfig<GiftsConfig>(ConfigTypes.GIFTS);
    }

    /**
     * Does a gift with a specific ID exist in db
     * @param giftId Gift id to check for
     * @returns True if it exists in db
     */
    public bool GiftExists(string giftId)
    {
        return _giftConfig.Gifts[giftId] is not null;
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
        return _giftConfig.Gifts;
    }

    /**
     * Get an array of all gift ids
     * @returns string array of gift ids
     */
    public List<string> GetGiftIds()
    {
        return _giftConfig.Gifts.Keys.ToList();
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

        // Handle system messsages
        if (giftData.Sender == GiftSenderType.System)
        {
            // Has a localisable text id to send to player
            if (giftData.LocaleTextId is not null)
            {
                _mailSendService.SendLocalisedSystemMessageToPlayer(
                    playerId,
                    giftData.LocaleTextId,
                    giftData.Items,
                    giftData.ProfileChangeEvents,
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1));
            }
            else
            {
                _mailSendService.SendSystemMessageToPlayer(
                    playerId,
                    giftData.MessageText,
                    giftData.Items,
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1),
                    giftData.ProfileChangeEvents);
            }
        }
        // Handle user messages
        else if (giftData.Sender == GiftSenderType.User)
        {
            _mailSendService.SendUserMessageToPlayer(
                playerId,
                giftData.SenderDetails,
                giftData.MessageText,
                giftData.Items,
                _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1));
        }
        else if (giftData.Sender == GiftSenderType.Trader)
        {
            if (giftData.LocaleTextId is not null)
            {
                _mailSendService.SendLocalisedNpcMessageToPlayer(
                    playerId,
                    giftData.Trader,
                    MessageType.MESSAGE_WITH_ITEMS,
                    giftData.LocaleTextId,
                    giftData.Items,
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1),
                    null,
                    null
                );
            }
            else
            {
                _mailSendService.SendLocalisedNpcMessageToPlayer(
                    playerId,
                    giftData.Trader,
                    MessageType.MESSAGE_WITH_ITEMS,
                    giftData.MessageText,
                    giftData.Items,
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1),
                    null,
                    null
                );
            }
        }
        else
        {
            // TODO: further split out into different message systems like above SYSTEM method
            // Trader / ragfair
            SendMessageDetails details = new () {
                RecipientId = playerId,
                Sender = GetMessageType(giftData),
                SenderDetails = new ()
                {
                    Id = GetSenderId(giftData),
                    Aid = 1234567, // TODO - pass proper aid value
                    Info = null,
                },
                MessageText = giftData.MessageText,
                Items = giftData.Items,
                ItemsMaxStorageLifetimeSeconds = _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 0),
            };

            if (giftData.Trader is not null) {
                details.Trader = giftData.Trader;
            }

            _mailSendService.SendMessageToPlayer(details);
        }
        
        _profileHelper.FlagGiftReceivedInProfile(playerId, giftId, maxGiftsToSendCount);

        return GiftSentResult.SUCCESS;
    }

    /**
     * Get sender id based on gifts sender type enum
     * @param giftData Gift to send player
     * @returns trader/user/system id
     */
    private string? GetSenderId(Gift giftData)
    {
        if (giftData.Sender == GiftSenderType.Trader)
        {
            return Enum.GetName(typeof(GiftSenderType), giftData.Sender);
        }

        if (giftData.Sender == GiftSenderType.User)
        {
            return giftData.Sender.ToString();
        }

        return null;
    }

    /**
     * Convert GiftSenderType into a dialog MessageType
     * @param giftData Gift to send player
     * @returns MessageType enum value
     */
    protected MessageType? GetMessageType(Gift giftData)
    {
        switch (giftData.Sender)
        {
            case GiftSenderType.System:
                return MessageType.SYSTEM_MESSAGE;
            case GiftSenderType.Trader:
                return MessageType.NPC_TRADER;
            case GiftSenderType.User:
                return MessageType.USER_MESSAGE;
            default:
                _logger.Error(_localisationService.GetText("gift-unable_to_handle_message_type_command", giftData.Sender));
                return null;
        }
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
