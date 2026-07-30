using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Services.Commerce;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

[Injectable]
public class SptDialogueChatBot(
    MailSendService mailSendService,
    CoreConfig coreConfig,
    ProfileHelper profileHelper,
    IEnumerable<IChatMessageHandler> chatMessageHandlers
) : IDialogueChatBot
{
    protected readonly IEnumerable<IChatMessageHandler> ChatMessageHandlers = ChatMessageHandlerSetup(chatMessageHandlers);

    public UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = coreConfig.Features.ChatbotFeatures.Ids["spt"],
            Aid = coreConfig.Features.ChatbotFeatures.Aids["spt"],
            Info = new UserDialogDetails
            {
                Level = 1,
                MemberCategory = MemberCategory.Developer,
                SelectedMemberCategory = MemberCategory.Developer,
                Nickname = coreConfig.SptFriendNickname,
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
