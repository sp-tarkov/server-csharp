using System.Collections.Frozen;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class HelloMessageHandler(MailSendService mailSendService, RandomUtil randomUtil) : IChatMessageHandler
{
    protected static readonly FrozenSet<string> _greetings = ["hello", "hi", "sup", "yo", "hey", "bonjour"];

    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return _greetings.Contains(message, StringComparer.OrdinalIgnoreCase);
    }

    public void Process(MongoId sessionId, UserDialogInfo sptFriendUser, PmcData? sender, object? extraInfo = null)
    {
        mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            randomUtil.GetArrayValue(
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
                    $"Hello {sender?.Info?.Nickname}",
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

    public string PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        mailSendService.SendUserMessageToPlayer(
            sessionId,
            commandHandler,
            randomUtil.GetArrayValue(["Howdy", "Hi", "Greetings", "Hello", "Bonjor", "Yo", "Sup", "Heyyyyy", "Hey there", "OH its you"]),
            [],
            null
        );

        return request.DialogId;
    }
}
