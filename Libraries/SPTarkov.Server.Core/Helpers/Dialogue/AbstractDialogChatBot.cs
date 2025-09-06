using SPTarkov.Server.Core.Helpers.Dialog.Commando;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue;

public abstract class AbstractDialogChatBot(
    ISptLogger<AbstractDialogChatBot> logger,
    MailSendService mailSendService,
    ServerLocalisationService localisationService,
    IEnumerable<IChatCommand> chatCommands
) : IDialogueChatBot
{
    protected readonly IDictionary<string, IChatCommand> _chatCommands = chatCommands.ToDictionary(command => command.CommandPrefix);

    public abstract UserDialogInfo GetChatBot();

    public async ValueTask<string> HandleMessage(MongoId sessionId, SendMessageRequest request)
    {
        if (request.Text.Length == 0)
        {
            logger.Error(localisationService.GetText("chatbot-command_was_empty"));

            return request.DialogId;
        }

        var splitCommand = request.Text.Split(" ");

        if (
            splitCommand.Length > 1
            && _chatCommands.TryGetValue(splitCommand[0], out var commando)
            && commando.Commands.Contains(splitCommand[1])
        )
        {
            return await commando.Handle(splitCommand[1], GetChatBot(), sessionId, request);
        }

        if (string.Equals(splitCommand.FirstOrDefault(), "help", StringComparison.OrdinalIgnoreCase))
        {
            return await SendPlayerHelpMessage(sessionId, request);
        }

        mailSendService.SendUserMessageToPlayer(sessionId, GetChatBot(), GetUnrecognizedCommandMessage(), [], null);

        return string.Empty;
    }

    protected async ValueTask<string> SendPlayerHelpMessage(MongoId sessionId, SendMessageRequest request)
    {
        mailSendService.SendUserMessageToPlayer(sessionId, GetChatBot(), "The available commands will be listed below:", [], null);
        foreach (var chatCommand in _chatCommands.Values)
        {
            // due to BSG being dumb with messages we need a mandatory timeout between messages so they get out on the right order
            await Task.Delay(TimeSpan.FromSeconds(1));

            mailSendService.SendUserMessageToPlayer(
                sessionId,
                GetChatBot(),
                $"Commands available for \"{chatCommand.CommandPrefix}\" prefix:",
                [],
                null
            );

            await Task.Delay(TimeSpan.FromSeconds(1));

            foreach (var subCommand in chatCommand.Commands)
            {
                mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    GetChatBot(),
                    $"Subcommand {subCommand}:\n{chatCommand.GetCommandHelp(subCommand)}",
                    [],
                    null
                );

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        return request.DialogId;
    }

    public void RegisterChatCommand(IChatCommand chatCommand)
    {
        var prefix = chatCommand.CommandPrefix;
        if (!_chatCommands.TryAdd(prefix, chatCommand))
        {
            throw new Exception($"The command \"{prefix}\" attempting to be registered already exists.");
        }
    }

    protected abstract string GetUnrecognizedCommandMessage();
}
