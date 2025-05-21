using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Dialog;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class GiftService(
    ISptLogger<GiftService> _logger,
    MailSendService _mailSendService,
    LocalisationService _localisationService,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    ProfileHelper _profileHelper,
    ConfigServer _configServer)
{
    protected GiftsConfig _giftConfig = _configServer.GetConfig<GiftsConfig>();

    /// <summary>
    ///     Does a gift with a specific ID exist in db
    /// </summary>
    /// <param name="giftId"> Gift id to check for </param>
    /// <returns> True if it exists in db </returns>
    public bool GiftExists(string giftId)
    {
        return _giftConfig.Gifts.ContainsKey(giftId);
    }

    public Gift? GetGiftById(string giftId)
    {
        _giftConfig.Gifts.TryGetValue(giftId, out var gift);

        return gift;
    }

    /// <summary>
    ///     Get dictionary of all gifts
    /// </summary>
    /// <returns> Dict keyed by gift id </returns>
    public Dictionary<string, Gift> GetGifts()
    {
        return _giftConfig.Gifts;
    }

    /// <summary>
    ///     Get an array of all gift ids
    /// </summary>
    /// <returns> String list of gift ids </returns>
    public List<string> GetGiftIds()
    {
        return _giftConfig.Gifts.Keys.ToList();
    }

    /// <summary>
    ///     Send player a gift from a range of sources
    /// </summary>
    /// <param name="playerId"> Player to send gift to / sessionID </param>
    /// <param name="giftId"> ID of gift in configs/gifts.json to send player </param>
    /// <returns> Outcome of sending gift to player </returns>
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
            if (_logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Player already received gift: {giftId}");
            }

            return GiftSentResult.FAILED_GIFT_ALREADY_RECEIVED;
        }

        if (giftData.Items?.Count > 0 && giftData.CollectionTimeHours is not null)
        {
            _logger.Warning($"Gift {giftId} has items but no collection time limit, defaulting to 48 hours");
        }

        // Handle system messages
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
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1)
                );
            }
            else
            {
                _mailSendService.SendSystemMessageToPlayer(
                    playerId,
                    giftData.MessageText,
                    giftData.Items,
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1),
                    giftData.ProfileChangeEvents
                );
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
                _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1)
            );
        }
        else if (giftData.Sender == GiftSenderType.Trader)
        {
            if (giftData.LocaleTextId is not null)
            {
                _mailSendService.SendLocalisedNpcMessageToPlayer(
                    playerId,
                    giftData.Trader,
                    MessageType.MessageWithItems,
                    giftData.LocaleTextId,
                    giftData.Items,
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1)
                );
            }
            else
            {
                _mailSendService.SendLocalisedNpcMessageToPlayer(
                    playerId,
                    giftData.Trader,
                    MessageType.MessageWithItems,
                    giftData.MessageText,
                    giftData.Items,
                    _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1)
                );
            }
        }
        else
        {
            // TODO: further split out into different message systems like above SYSTEM method
            // Trader / ragfair
            SendMessageDetails details = new()
            {
                RecipientId = playerId,
                Sender = GetMessageType(giftData),
                SenderDetails = new UserDialogInfo
                {
                    Id = GetSenderId(giftData),
                    Aid = 1234567, // TODO - pass proper aid value
                    Info = null
                },
                MessageText = giftData.MessageText,
                Items = giftData.Items,
                ItemsMaxStorageLifetimeSeconds = _timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 0)
            };

            if (giftData.Trader is not null)
            {
                details.Trader = giftData.Trader;
            }

            _mailSendService.SendMessageToPlayer(details);
        }

        _profileHelper.FlagGiftReceivedInProfile(playerId, giftId, maxGiftsToSendCount);

        return GiftSentResult.SUCCESS;
    }

    /// <summary>
    ///     Get sender id based on gifts sender type enum
    /// </summary>
    /// <param name="giftData"> Gift to send player </param>
    /// <returns> trader/user/system id </returns>
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

    /// <summary>
    ///     Convert GiftSenderType into a dialog MessageType
    /// </summary>
    /// <param name="giftData"> Gift to send player </param>
    /// <returns> MessageType enum value </returns>
    protected MessageType? GetMessageType(Gift giftData)
    {
        switch (giftData.Sender)
        {
            case GiftSenderType.System:
                return MessageType.SystemMessage;
            case GiftSenderType.Trader:
                return MessageType.NpcTraderMessage;
            case GiftSenderType.User:
                return MessageType.UserMessage;
            default:
                _logger.Error(_localisationService.GetText("gift-unable_to_handle_message_type_command", giftData.Sender));
                return null;
        }
    }

    /// <summary>
    ///     Prapor sends gifts to player for first week after profile creation
    /// </summary>
    /// <param name="sessionId"> Player ID </param>
    /// <param name="day"> What day to give gift for </param>
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
            if (!_profileHelper.PlayerHasRecievedMaxNumberOfGift(sessionId, giftId, 1))
            {
                SendGiftToPlayer(sessionId, giftId);
            }
        }
    }

    /// <summary>
    ///     Send player a gift with silent received check
    /// </summary>
    /// <param name="giftId"> ID of gift to send </param>
    /// <param name="sessionId"> Session ID of player to send to </param>
    /// <param name="giftCount"> Optional, how many to send </param>
    public void SendGiftWithSilentReceivedCheck(string giftId, string? sessionId, int giftCount)
    {
        if (!_profileHelper.PlayerHasRecievedMaxNumberOfGift(sessionId, giftId, giftCount))
        {
            SendGiftToPlayer(sessionId, giftId);
        }
    }
}
