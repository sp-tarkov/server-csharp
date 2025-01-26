using SptCommon.Annotations;
using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Helpers.Dialogue;

[Injectable]
public class SptDialogueChatBot(
    ISptLogger<AbstractDialogChatBot> _logger,
    MailSendService _mailSendService,
    IEnumerable<IChatCommand> _chatCommands,
    ConfigServer _configServer,
    ProfileHelper _profileHelper,
    RandomUtil _randomUtil,
    SeasonalEventService _seasonalEventService,
    GiftService _giftService,
    LocalisationService _localisationService
) : IDialogueChatBot
{
    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    protected WeatherConfig _weatherConfig = _configServer.GetConfig<WeatherConfig>();
    protected List<string> _listOfMessages = ["hello", "hi", "sup", "yo", "hey"];

    public UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = _coreConfig.Features.ChatbotFeatures.Ids["spt"],
            Aid = 1234566,
            Info = new UserDialogDetails
            {
                Level = 1,
                MemberCategory = MemberCategory.Developer,
                SelectedMemberCategory = MemberCategory.Developer,
                Nickname = _coreConfig.SptFriendNickname,
                Side = "Usec"
            }
        };
    }

    public string? HandleMessage(string sessionId, SendMessageRequest request)
    {
        var sender = _profileHelper.GetPmcProfile(sessionId);

        var sptFriendUser = GetChatBot();
        var requestInput = request.Text.ToLower();

        // only check if entered text is gift code when feature enabled
        if (_coreConfig.Features.ChatbotFeatures.SptFriendGiftsEnabled) {
            var giftSent = _giftService.SendGiftToPlayer(sessionId, request.Text);
            if (giftSent == GiftSentResult.SUCCESS) {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _randomUtil.GetArrayValue([
                        "Hey! you got the right code!",
                        "A secret code, how exciting!",
                        "You found a gift code!",
                        "A gift code! incredible",
                        "A gift! what could it be!",
                    ]),
                    [],
                    null
                );

                return null;
            }

            if (giftSent == GiftSentResult.FAILED_GIFT_ALREADY_RECEIVED) {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _randomUtil.GetArrayValue(["Looks like you already used that code", "You already have that!!"]),
                    [],
                    null
                );

                return null;
            }
        }

        if (requestInput.Contains("love you")) {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue([
                    "That's quite forward but i love you too in a purely chatbot-human way",
                    "I love you too buddy :3!",
                    "uwu",
                    $"love you too {sender?.Info?.Nickname}",
                ]),
                [],
                null
            );
        }

        if (requestInput == "spt") {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue(["Its me!!", "spt? i've heard of that project"]),
                [],
                null
            );
        }

        if (requestInput == "fish") {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue(["blub"]),
                [],
                null
            );
        }

        if (_listOfMessages.Contains(requestInput)) {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue([
                    "Howdy",
                    "Hi",
                    "Greetings",
                    "Hello",
                    "bonjor",
                    "Yo",
                    "Sup",
                    "Heyyyyy",
                    "Hey there",
                    $"Hello {sender?.Info?.Nickname}",
                ]),
            [], null
            );
        }

        if (requestInput == "nikita") {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue([
                    "I know that guy!",
                    "Cool guy, he made EFT!",
                    "Legend",
                    "Remember when he said webel-webel-webel-webel, classic Nikita moment",
                ]), [], null
            );
        }

        if (requestInput == "are you a bot") {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue(["beep boop", "**sad boop**", "probably", "sometimes", "yeah lol"]),
                [], null
            );
        }

        if (requestInput == "itsonlysnowalan") {
            _weatherConfig.OverrideSeason = Season.WINTER;

            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue([_localisationService.GetText("chatbot-snow_enabled")]), [], null
            );
        }

        if (requestInput == "givemesunshine") {
            _weatherConfig.OverrideSeason = Season.SUMMER;

            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                sptFriendUser,
                _randomUtil.GetArrayValue([_localisationService.GetText("chatbot-summer_enabled")]), [], null
            );
        }

        if (requestInput == "veryspooky") {
            var enableEventResult = _seasonalEventService.ForceSeasonalEvent(SeasonalEventType.Halloween);
            if (enableEventResult) {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _randomUtil.GetArrayValue([
                        _localisationService.GetText("chatbot-forced_event_enabled", SeasonalEventType.Halloween)
                    ]), [], null
                );
            }
        }

        if (requestInput == "hohoho") {
            var enableEventResult = _seasonalEventService.ForceSeasonalEvent(SeasonalEventType.Christmas);
            if (enableEventResult) {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _randomUtil.GetArrayValue([
                        _localisationService.GetText("chatbot-forced_event_enabled", SeasonalEventType.Christmas)
                    ]), [], null
                );
            }
        }

        if (requestInput == "givemespace") {
            var stashRowGiftId = "StashRows";
            var maxGiftsToSendCount = _coreConfig.Features.ChatbotFeatures.CommandUseLimits[stashRowGiftId] ?? 5;
            if (_profileHelper.PlayerHasRecievedMaxNumberOfGift(sessionId, stashRowGiftId, maxGiftsToSendCount)) {
                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _localisationService.GetText("chatbot-cannot_accept_any_more_of_gift"), [], null
                );
            } else {
                _profileHelper.AddStashRowsBonusToProfile(sessionId, 2);

                _mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    sptFriendUser,
                    _randomUtil.GetArrayValue([
                        _localisationService.GetText("chatbot-added_stash_rows_please_restart"),
                    ]), [], null
                );

                _profileHelper.FlagGiftReceivedInProfile(sessionId, stashRowGiftId, maxGiftsToSendCount);
            }
        }

        return request.DialogId;
    }
}
