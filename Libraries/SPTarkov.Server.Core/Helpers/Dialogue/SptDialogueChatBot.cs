using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

[Injectable]
public class SptDialogueChatBot(
    MailSendService mailSendService,
    ConfigServer configServer,
    ProfileHelper profileHelper,
    IEnumerable<IChatMessageHandler> chatMessageHandlers
) : IDialogueChatBot
{
    protected readonly IEnumerable<IChatMessageHandler> ChatMessageHandlers = ChatMessageHandlerSetup(chatMessageHandlers);
    protected readonly CoreConfig CoreConfig = configServer.GetConfig<CoreConfig>();

    public UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = CoreConfig.Features.ChatbotFeatures.Ids["spt"],
            Aid = 1234566,
            Info = new UserDialogDetails
            {
                Level = 1,
                MemberCategory = MemberCategory.Developer,
                SelectedMemberCategory = MemberCategory.Developer,
                Nickname = CoreConfig.SptFriendNickname,
                Side = "Usec",
            },
        };
    }

    public ValueTask<string> HandleMessage(MongoId sessionId, SendMessageRequest request)
    {
        var sender = profileHelper.GetPmcProfile(sessionId);
        var sptFriendUser = GetChatBot();

        if (string.Equals(request.Text, "help", StringComparison.OrdinalIgnoreCase))
        {
            return SendPlayerHelpMessage(sessionId, request);
        }

        var handler = ChatMessageHandlers.FirstOrDefault(h => h.CanHandle(request.Text));
        if (handler is not null)
        {
            handler.Process(sessionId, sptFriendUser, sender, request);

            return new ValueTask<string>(request.DialogId);
        }

        mailSendService.SendUserMessageToPlayer(sessionId, GetChatBot(), GetUnrecognizedCommandMessage(), [], null);

        return new ValueTask<string>(request.DialogId);
    }

    protected static List<IChatMessageHandler> ChatMessageHandlerSetup(IEnumerable<IChatMessageHandler> components)
    {
        var chatMessageHandlers = components.ToList();
        chatMessageHandlers.Sort((a, b) => a.GetPriority() - b.GetPriority());

        return chatMessageHandlers;
    }

    protected string GetUnrecognizedCommandMessage()
    {
        return "Unknown command.";
    }

    protected ValueTask<string> SendPlayerHelpMessage(MongoId sessionId, SendMessageRequest request)
    {
        mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            "The available commands are:\n GIVEMESPACE \n HOHOHO \n VERYSPOOKY \n ITSONLYSNOWALAN \n GIVEMESUNSHINE \n GARBAGE",
            [],
            null
        );

        return new ValueTask<string>(request.DialogId);
    }
}
