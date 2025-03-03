using Core.Helpers.Dialog.Commando;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Utils;
using Core.Services;
using Core.Utils.Callbacks;

namespace Core.Helpers.Dialogue;

public abstract class AbstractDialogChatBot(
    ISptLogger<AbstractDialogChatBot> _logger,
    MailSendService _mailSendService,
    IEnumerable<IChatCommand> chatCommands
) : IDialogueChatBot
{
    protected List<IChatCommand> _chatCommands = chatCommands.ToList();

    public abstract UserDialogInfo GetChatBot();

    public string? HandleMessage(string sessionId, SendMessageRequest request)
    {
        if ((request.Text ?? "").Length == 0)
        {
            _logger.Error("Command came in as empty text! Invalid data!");

            return request.DialogId;
        }

        var splitCommand = request.Text.Split(" ");

        var commandos = _chatCommands.Where(c => c.GetCommandPrefix() == splitCommand.FirstOrDefault());
        if (commandos.FirstOrDefault()?.GetCommands().Contains(splitCommand[1]) ?? false)
        {
            return commandos.FirstOrDefault().Handle(splitCommand[1], GetChatBot(), sessionId, request);
        }

        if (splitCommand.FirstOrDefault()?.ToLower() == "help")
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
        if (_chatCommands.Any(cc => cc.GetCommandPrefix() == chatCommand.GetCommandPrefix()))
        {
            throw new Exception(
                $"The command \"{chatCommand.GetCommandPrefix()}\" attempting to be registered already exists."
            );
        }

        _chatCommands.Add(chatCommand);
    }

    protected abstract string GetUnrecognizedCommandMessage();
}
