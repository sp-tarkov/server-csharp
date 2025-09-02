using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
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
    ISptLogger<GiftService> logger,
    MailSendService mailSendService,
    ServerLocalisationService serverLocalisationService,
    TimeUtil timeUtil,
    ProfileHelper profileHelper,
    ConfigServer configServer
)
{
    protected readonly GiftsConfig GiftConfig = configServer.GetConfig<GiftsConfig>();

    /// <summary>
    ///     Does a gift with a specific ID exist in db
    /// </summary>
    /// <param name="giftId"> Gift id to check for </param>
    /// <returns> True if it exists in db </returns>
    public bool GiftExists(string giftId)
    {
        return GiftConfig.Gifts.ContainsKey(giftId);
    }

    public Gift? GetGiftById(string giftId)
    {
        GiftConfig.Gifts.TryGetValue(giftId, out var gift);

        return gift;
    }

    /// <summary>
    ///     Get dictionary of all gifts
    /// </summary>
    /// <returns> Dict keyed by gift id </returns>
    public Dictionary<string, Gift> GetGifts()
    {
        return GiftConfig.Gifts;
    }

    /// <summary>
    ///     Get an array of all gift ids
    /// </summary>
    /// <returns> String list of gift ids </returns>
    public IEnumerable<string> GetGiftIds()
    {
        return GiftConfig.Gifts.Keys;
    }

    /// <summary>
    ///     Send player a gift from a range of sources
    /// </summary>
    /// <param name="playerId"> Player to send gift to / sessionID </param>
    /// <param name="giftId"> ID of gift in configs/gifts.json to send player </param>
    /// <returns> Outcome of sending gift to player </returns>
    public GiftSentResult SendGiftToPlayer(MongoId playerId, string giftId)
    {
        var giftData = GetGiftById(giftId);
        if (giftData is null)
        {
            return GiftSentResult.FAILED_GIFT_DOESNT_EXIST;
        }

        var maxGiftsToSendCount = giftData.MaxToSendPlayer ?? 1;

        if (profileHelper.PlayerHasReceivedMaxNumberOfGift(playerId, giftId, maxGiftsToSendCount))
        {
            if (logger.IsLogEnabled(LogLevel.Debug))
            {
                logger.Debug($"Player already received gift: {giftId}");
            }

            return GiftSentResult.FAILED_GIFT_ALREADY_RECEIVED;
        }

        if (giftData.Items?.Count > 0 && giftData.CollectionTimeHours is null)
        {
            logger.Warning($"Gift {giftId} has items but no collection time limit, defaulting to 48 hours");
        }

        // Handle system messages
        if (giftData.Sender == GiftSenderType.System)
        {
            // Has a localisable text id to send to player
            if (giftData.LocaleTextId is not null)
            {
                mailSendService.SendLocalisedSystemMessageToPlayer(
                    playerId,
                    giftData.LocaleTextId,
                    giftData.Items,
                    giftData.ProfileChangeEvents,
                    timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1)
                );
            }
            else
            {
                mailSendService.SendSystemMessageToPlayer(
                    playerId,
                    giftData.MessageText,
                    giftData.Items,
                    timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1),
                    giftData.ProfileChangeEvents
                );
            }
        }
        // Handle user messages
        else if (giftData.Sender == GiftSenderType.User)
        {
            mailSendService.SendUserMessageToPlayer(
                playerId,
                giftData.SenderDetails,
                giftData.MessageText,
                giftData.Items,
                timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1)
            );
        }
        else if (giftData.Sender == GiftSenderType.Trader)
        {
            mailSendService.SendLocalisedNpcMessageToPlayer(
                playerId,
                giftData.Trader,
                MessageType.MessageWithItems,
                giftData.LocaleTextId ?? giftData.LocaleTextId,
                giftData.Items,
                timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 1)
            );
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
                    Info = null,
                },
                MessageText = giftData.MessageText,
                Items = giftData.Items,
                ItemsMaxStorageLifetimeSeconds = timeUtil.GetHoursAsSeconds(giftData.CollectionTimeHours ?? 0),
            };

            if (giftData.Trader is not null)
            {
                details.Trader = giftData.Trader;
            }

            mailSendService.SendMessageToPlayer(details);
        }

        profileHelper.FlagGiftReceivedInProfile(playerId, giftId, maxGiftsToSendCount);

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
                logger.Error(serverLocalisationService.GetText("gift-unable_to_handle_message_type_command", giftData.Sender));
                return null;
        }
    }

    /// <summary>
    ///     Prapor sends gifts to player for first week after profile creation
    /// </summary>
    /// <param name="sessionId"> Player ID </param>
    /// <param name="day"> What day to give gift for </param>
    public void SendPraporStartingGift(MongoId sessionId, int day)
    {
        var giftId = day switch
        {
            1 => "PraporGiftDay1",
            2 => "PraporGiftDay2",
            _ => null,
        };

        if (giftId is not null)
        {
            if (!profileHelper.PlayerHasReceivedMaxNumberOfGift(sessionId, giftId, 1))
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
    public void SendGiftWithSilentReceivedCheck(string giftId, MongoId sessionId, int giftCount)
    {
        if (!profileHelper.PlayerHasReceivedMaxNumberOfGift(sessionId, giftId, giftCount))
        {
            SendGiftToPlayer(sessionId, giftId);
        }
    }
}
