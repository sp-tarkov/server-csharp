using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Callbacks;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

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

        var handler = _chatMessageHandlers.FirstOrDefault(h =>
        {
            return h.CanHandle(request.Text);
        });

        if (handler is not null)
        {
            handler.Process(sessionId, sptFriendUser, sender, request);

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

    protected string? SendPlayerHelpMessage(string sessionId, SendMessageRequest request)
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
