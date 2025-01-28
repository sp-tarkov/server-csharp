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
using Core.Helpers.Dialogue.SptMessageHandlers;

namespace Core.Helpers.Dialogue;

[Injectable]
public class SptDialogueChatBot(
    ISptLogger<AbstractDialogChatBot> _logger,
    MailSendService _mailSendService,
    IEnumerable<IChatCommand> _chatCommands,
    ConfigServer _configServer,
    ProfileHelper _profileHelper,
    IEnumerable<IChatMessageHandler> chatMessageHandlers
) : IDialogueChatBot
{
    protected IEnumerable<IChatMessageHandler> _chatMessageHandlers = ChatMessageHandlerSetup(chatMessageHandlers);

    private static List<IChatMessageHandler> ChatMessageHandlerSetup(IEnumerable<IChatMessageHandler> components)
    {
        var chatMessageHandlers = components.ToList();
        chatMessageHandlers.Sort((a, b) => a.GetPriority() - b.GetPriority());

        return chatMessageHandlers;
    }

    protected CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    

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

        _chatMessageHandlers.FirstOrDefault((v) => v.CanHandle(request.Text))
            .Process(sessionId, sptFriendUser, sender);

        return request.DialogId;
    }
}
