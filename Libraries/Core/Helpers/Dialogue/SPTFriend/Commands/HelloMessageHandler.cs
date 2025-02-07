using Core.Models.Eft.Common;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class HelloMessageHandler(
    MailSendService _mailSendService,
    RandomUtil _randomUtil) : IChatMessageHandler
{
    protected List<string> _listOfMessages = ["hello", "hi", "sup", "yo", "hey", "bonjour"];


    public string GetCommand()
    {
        return "hello";
    }

    public string GetAssociatedBotId()
    {
        return "6723fd51c5924c57ce0ca01f";
    }

    public string GetCommandHelp()
    {
        return "'hello' replies to the player with a random greeting";
    }

    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            commandHandler,
            _randomUtil.GetArrayValue(
                [
                    "Howdy",
                    "Hi",
                    "Greetings",
                    "Hello",
                    "Bonjor",
                    "Yo",
                    "Sup",
                    "Heyyyyy",
                    "Hey there",
                    "OH its you"
                ]
            ),
            [],
            null
        );

        return request.DialogId;
    }


    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return _listOfMessages.Contains(message, StringComparer.OrdinalIgnoreCase);
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            _randomUtil.GetArrayValue(
                [
                    "Howdy",
                    "Hi",
                    "Greetings",
                    "Hello",
                    "Bonjor",
                    "Yo",
                    "Sup",
                    "Heyyyyy",
                    "Hey there",
                    "OH its you",
                    $"Hello {sender?.Info?.Nickname}"
                ]
            ),
            [],
            null
        );
    }
}
