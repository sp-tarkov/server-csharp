using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;
using System.Collections.Frozen;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class HelloMessageHandler(
    MailSendService _mailSendService,
    RandomUtil _randomUtil) : IChatMessageHandler
{
    protected static readonly FrozenSet<string> _listOfGreetings = ["hello", "hi", "sup", "yo", "hey", "bonjour"];


    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return _listOfGreetings.Contains(message, StringComparer.OrdinalIgnoreCase);
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
}
