using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando.SptCommands;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Dialog;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands.TraderCommand;

[Injectable]
public class TraderSptCommand(
    ISptLogger<TraderSptCommand> _logger,
    HashUtil _hashUtil,
    TraderHelper _traderHelper,
    MailSendService _mailSendService) : ISptCommand
{
    protected Regex _commandRegex = new(
        @"^spt trader (?<trader>[\w]+) (?<command>rep|spend) (?<quantity>(?!0+)[0-9]+)$"
    );

    public string GetCommand()
    {
        return "trader";
    }

    public string GetCommandHelp()
    {
        return
            "spt trader\n========\nSets the reputation or money spent to the input quantity through the message system.\n\n\tspt trader [trader] rep [quantity]\n\t\tEx: spt trader prapor rep 2\n\n\tspt trader [trader] spend [quantity]\n\t\tEx: spt trader therapist spend 1000000";
    }

    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        if (!_commandRegex.IsMatch(request.Text))
        {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "Invalid use of trader command. Use 'help' for more information."
            );
            return request.DialogId;
        }

        var result = _commandRegex.Match(request.Text);

        var trader = result.Groups["trader"].Captures.Count > 0 ? result.Groups["trader"].Captures[0].Value : null;
        var command = result.Groups["command"].Captures.Count > 0 ? result.Groups["command"].Captures[0].Value : null;
        var quantity = int.Parse(result.Groups["command"].Captures.Count > 0 ? result.Groups["quantity"].Captures[0].Value : "0");

        var dbTrader = _traderHelper.GetTraderByNickName(trader);
        if (dbTrader == null)
        {
            _mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "Invalid use of trader command, the trader was not found. Use 'help' for more information."
            );

            return request.DialogId;
        }

        ProfileChangeEventType profileChangeEventType;
        switch (command)
        {
            case "rep":
                quantity /= 100;
                profileChangeEventType = ProfileChangeEventType.TraderStanding;
                break;
            case "spend":
                profileChangeEventType = ProfileChangeEventType.TraderSalesSum;
                break;
            default:
                {
                    _mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        "Invalid use of trader command, ProfileChangeEventType was not found. Use 'help' for more information."
                    );

                    return request.DialogId;
                }
        }

        _mailSendService.SendSystemMessageToPlayer(
            sessionId,
            "A single ruble is being attached, required by BSG logic.",
            [
                new Item
                {
                    Id = _hashUtil.Generate(),
                    Template = Money.ROUBLES,
                    Upd = new Upd
                    {
                        StackObjectsCount = 1
                    },
                    ParentId = _hashUtil.Generate(),
                    SlotId = "main"
                }
            ],
            999999,
            [CreateProfileChangeEvent(profileChangeEventType, quantity, dbTrader.Id)]
        );

        return request.DialogId;
    }

    protected ProfileChangeEvent CreateProfileChangeEvent(ProfileChangeEventType profileChangeEventType, int quantity, string dbTraderId)
    {
        return new ProfileChangeEvent
        {
            Id = _hashUtil.Generate(),
            Type = profileChangeEventType,
            Value = quantity,
            Entity = dbTraderId
        };
    }
}
