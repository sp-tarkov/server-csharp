using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando.SptCommands;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Eft.Ws;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Dialog;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands.TraderCommand;

[Injectable]
public class TraderSptCommand(ISptLogger<TraderSptCommand> logger, TraderHelper traderHelper, MailSendService mailSendService) : ISptCommand
{
    protected readonly Regex _commandRegex = new(@"^spt trader (?<trader>[\w]+) (?<command>rep|spend) (?<quantity>(?!0+)[0-9]+)$");

    public string Command
    {
        get { return "trader"; }
    }

    public string CommandHelp
    {
        get
        {
            return "spt trader \n ======== \n Sets the reputation or money spent to the input quantity through the message system.\n\n\tspt trader [trader] rep [quantity]\n\t\tEx: spt trader prapor rep 2\n\n\tspt trader [trader] spend [quantity]\n\t\tEx: spt trader therapist spend 1000000";
        }
    }

    public ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        if (!_commandRegex.IsMatch(request.Text))
        {
            mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "Invalid use of trader command. Use 'help' for more information."
            );
            return new ValueTask<string>(request.DialogId);
        }

        var result = _commandRegex.Match(request.Text);

        var trader = result.Groups["trader"].Captures.Count > 0 ? result.Groups["trader"].Captures[0].Value : null;
        var command = result.Groups["command"].Captures.Count > 0 ? result.Groups["command"].Captures[0].Value : null;
        var quantity = double.Parse(result.Groups["command"].Captures.Count > 0 ? result.Groups["quantity"].Captures[0].Value : "0");

        var dbTrader = traderHelper.GetTraderByNickName(trader);
        if (dbTrader == null)
        {
            mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "Invalid use of trader command, the trader was not found. Use 'help' for more information."
            );

            return new ValueTask<string>(request.DialogId);
        }

        NotificationEventType profileChangeEventType;
        switch (command)
        {
            case "rep":
                quantity /= 100;
                profileChangeEventType = NotificationEventType.TraderStanding;
                break;
            case "spend":
                profileChangeEventType = NotificationEventType.TraderSalesSum;
                break;
            default:
            {
                mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    "Invalid use of trader command, ProfileChangeEventType was not found. Use 'help' for more information."
                );

                return new ValueTask<string>(request.DialogId);
            }
        }

        mailSendService.SendSystemMessageToPlayer(
            sessionId,
            "A single ruble is being attached, required by BSG logic.",
            [
                new Item
                {
                    Id = new MongoId(),
                    Template = Money.ROUBLES,
                    Upd = new Upd { StackObjectsCount = 1 },
                    ParentId = new MongoId(),
                    SlotId = "main",
                },
            ],
            999999,
            [CreateProfileChangeEvent(profileChangeEventType, quantity, dbTrader.Id)]
        );

        return new ValueTask<string>(request.DialogId);
    }

    protected ProfileChangeEvent CreateProfileChangeEvent(NotificationEventType profileChangeEventType, double quantity, string dbTraderId)
    {
        return new ProfileChangeEvent
        {
            Id = new MongoId(),
            Type = profileChangeEventType.ToString(),
            Value = quantity,
            Entity = dbTraderId,
        };
    }
}
