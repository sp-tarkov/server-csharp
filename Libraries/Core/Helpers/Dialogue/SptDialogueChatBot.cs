using Core.Helpers.Dialog.Commando;
using Core.Helpers.Dialogue.SPTFriend.Commands;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Config;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils.Callbacks;
using SptCommon.Annotations;

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

        if (request.Text?.ToLower() == "help")
        {
            return SendPlayerHelpMessage(sessionId, request);
        }


        var handler = _chatMessageHandlers.FirstOrDefault(v => v.CanHandle(request.Text));
        if (handler is not null)
        {
            handler.Process(sessionId, sptFriendUser, sender);

            return request.DialogId;
        }


        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            GetUnrecognizedCommandMessage(),
            [],
            null
        );

        return request.DialogId;
    }

    private static List<IChatMessageHandler> ChatMessageHandlerSetup(IEnumerable<IChatMessageHandler> components)
    {
        var chatMessageHandlers = components.ToList();
        chatMessageHandlers.Sort((a, b) => a.GetPriority() - b.GetPriority());

        return chatMessageHandlers;
    }

    private string GetUnrecognizedCommandMessage()
    {
        return "Unknown command.";
    }

    private string? SendPlayerHelpMessage(string sessionId, SendMessageRequest request)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            "The available commands are:\\n GIVEMESPACE \\n HOHOHO \\n VERYSPOOKY \\n ITSONLYSNOWALAN \\n GIVEMESUNSHINE",
            [],
            null
        );
        // due to BSG being dumb with messages we need a mandatory timeout between messages so they get out on the right order
        TimeoutCallback.RunInTimespan(
            () =>
            {
                foreach (var chatCommand in _chatCommands)
                {
                    _mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        GetChatBot(),
                        $"Commands available for \"{chatCommand.GetCommandPrefix()}\" prefix:",
                        [],
                        null
                    );

                    TimeoutCallback.RunInTimespan(
                        () =>
                        {
                            foreach (var subCommand in chatCommand.GetCommands())
                            {
                                _mailSendService.SendUserMessageToPlayer(
                                    sessionId,
                                    GetChatBot(),
                                    $"Subcommand {subCommand}:\\n{chatCommand.GetCommandHelp(subCommand)}",
                                    [],
                                    null
                                );
                            }
                        },
                        TimeSpan.FromSeconds(1)
                    );
                }
            },
            TimeSpan.FromSeconds(1)
        );

        return request.DialogId;
    }
}
