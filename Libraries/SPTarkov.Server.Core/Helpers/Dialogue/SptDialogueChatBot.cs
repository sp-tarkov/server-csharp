using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

[Injectable]
public class SptDialogueChatBot(
    MailSendService _mailSendService,
    ConfigServer _configServer,
    ProfileHelper _profileHelper,
    IEnumerable<IChatMessageHandler> chatMessageHandlers
) : IDialogueChatBot
{
    protected readonly IEnumerable<IChatMessageHandler> _chatMessageHandlers =
        ChatMessageHandlerSetup(chatMessageHandlers);
    protected readonly CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();

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
                Side = "Usec",
            },
        };
    }

    public ValueTask<string> HandleMessage(string sessionId, SendMessageRequest request)
    {
        var sender = _profileHelper.GetPmcProfile(sessionId);
        var sptFriendUser = GetChatBot();

        if (string.Equals(request.Text, "help", StringComparison.OrdinalIgnoreCase))
        {
            return SendPlayerHelpMessage(sessionId, request);
        }

        var handler = _chatMessageHandlers.FirstOrDefault(h => h.CanHandle(request.Text));
        if (handler is not null)
        {
            handler.Process(sessionId, sptFriendUser, sender, request);

            return new ValueTask<string>(request.DialogId);
        }

        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            GetUnrecognizedCommandMessage(),
            [],
            null
        );

        return new ValueTask<string>(request.DialogId);
    }

    protected static List<IChatMessageHandler> ChatMessageHandlerSetup(
        IEnumerable<IChatMessageHandler> components
    )
    {
        var chatMessageHandlers = components.ToList();
        chatMessageHandlers.Sort((a, b) => a.GetPriority() - b.GetPriority());

        return chatMessageHandlers;
    }

    protected string GetUnrecognizedCommandMessage()
    {
        return "Unknown command.";
    }

    protected ValueTask<string> SendPlayerHelpMessage(string sessionId, SendMessageRequest request)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            "The available commands are:\n GIVEMESPACE \n HOHOHO \n VERYSPOOKY \n ITSONLYSNOWALAN \n GIVEMESUNSHINE \n GARBAGE",
            [],
            null
        );

        return new ValueTask<string>(request.DialogId);
    }
}
