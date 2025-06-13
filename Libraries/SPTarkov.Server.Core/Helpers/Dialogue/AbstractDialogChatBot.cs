using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Callbacks;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

public abstract class AbstractDialogChatBot(
    ISptLogger<AbstractDialogChatBot> _logger,
    MailSendService _mailSendService,
    LocalisationService localisationService,
    IEnumerable<IChatCommand> chatCommands
) : IDialogueChatBot
{
    protected IDictionary<string, IChatCommand> _chatCommands =
        chatCommands.ToDictionary(command => command.GetCommandPrefix());

    public abstract UserDialogInfo GetChatBot();

    public string? HandleMessage(string sessionId, SendMessageRequest request)
    {
        if ((request.Text ?? "").Length == 0)
        {
            _logger.Error(localisationService.GetText("chatbot-command_was_empty"));

            return request.DialogId;
        }

        var splitCommand = request.Text.Split(" ");

        if (splitCommand.Length > 1 &&
            _chatCommands.TryGetValue(splitCommand[0], out var commando) &&
            commando.GetCommands().Contains(splitCommand[1]))
        {
            return commando.Handle(splitCommand[1], GetChatBot(), sessionId, request);
        }

        if (string.Equals(splitCommand.FirstOrDefault(), "help", StringComparison.OrdinalIgnoreCase))
        {
            return SendPlayerHelpMessage(sessionId, request);
        }

        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            GetUnrecognizedCommandMessage(),
            [],
            null
        );

        return null;
    }

    protected string? SendPlayerHelpMessage(string sessionId, SendMessageRequest request)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            "The available commands will be listed below:",
            [],
            null
        );
        // due to BSG being dumb with messages we need a mandatory timeout between messages so they get out on the right order
        TimeoutCallback.RunInTimespan(
            () =>
            {
                foreach (var chatCommand in _chatCommands.Values)
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
                                    $"Subcommand {subCommand}:\n{chatCommand.GetCommandHelp(subCommand)}",
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

    public void RegisterChatCommand(IChatCommand chatCommand)
    {
        var prefix = chatCommand.GetCommandPrefix();
        if (!_chatCommands.TryAdd(prefix, chatCommand))
        {
            throw new Exception($"The command \"{prefix}\" attempting to be registered already exists.");
        }
    }

    protected abstract string GetUnrecognizedCommandMessage();
}
