using System.Text.RegularExpressions;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Dialog.Commando.SptCommands;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Dialog;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue.Commando.SptCommands.ProfileCommand;

[Injectable]
public class ProfileSptCommand(ISptLogger<ProfileSptCommand> logger, MailSendService mailSendService, ProfileHelper profileHelper)
    : ISptCommand
{
    /// <summary>
    /// Regex to account for all these cases
    /// spt profile level 20
    /// spt profile skill metabolism 10
    /// </summary>
    protected static readonly Regex _commandRegex = new(
        @"^spt profile (?<command>level|skill)((?<=.*skill) (?<skill>[\w]+))? (?<quantity>(?!0+)[0-9]+)$"
    );

    protected static readonly Regex _examineRegex = new(@"^spt profile (?<command>examine)");

    public string Command
    {
        get { return "profile"; }
    }

    public string CommandHelp
    {
        get
        {
            return "spt profile\n========\nSets the profile level or skill to the desired level through the message system.\n\n\tspt "
                + "profile level [desired level]\n\t\tEx: spt profile level 20\n\n\tspt profile skill [skill name] [quantity]\n\t\tEx: "
                + "spt profile skill metabolism 51";
        }
    }

    public ValueTask<string> PerformAction(UserDialogInfo commandHandler, MongoId sessionId, SendMessageRequest request)
    {
        var isCommand = _commandRegex.IsMatch(request.Text);
        var isExamine = _examineRegex.IsMatch(request.Text);

        if (!isCommand && !isExamine)
        {
            mailSendService.SendUserMessageToPlayer(
                sessionId,
                commandHandler,
                "Invalid use of trader command. Use 'help' for more information."
            );
            return new ValueTask<string>(request.DialogId);
        }

        var result = _commandRegex.Match(request.Text);

        var command = result.Groups["command"].Length > 0 ? result.Groups["command"].Captures[0].Value : null;
        var skill = result.Groups["skill"].Length > 0 ? result.Groups["skill"].Captures[0].Value : null;
        var quantity = int.Parse(result.Groups["quantity"].Length > 0 ? result.Groups["quantity"].Captures[0].Value : "0");

        ProfileChangeEvent profileChangeEvent;
        switch (command)
        {
            case "level":
                if (quantity < 1 || quantity > profileHelper.GetMaxLevel())
                {
                    mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        "Invalid use of profile command, the level was outside bounds: 1 to 70. Use 'help' for more information."
                    );
                    return new ValueTask<string>(request.DialogId);
                }

                profileChangeEvent = HandleLevelCommand(quantity);
                break;
            case "skill":
            {
                var enumSkill = Enum.GetValues<SkillTypes>()
                    .Cast<SkillTypes?>()
                    .FirstOrDefault(t => string.Equals(t?.ToString(), skill, StringComparison.OrdinalIgnoreCase));

                if (enumSkill == null)
                {
                    mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        "Invalid use of profile command, the skill was not found. Use 'help' for more information."
                    );
                    return new ValueTask<string>(request.DialogId);
                }

                if (quantity is < 0 or > 51)
                {
                    mailSendService.SendUserMessageToPlayer(
                        sessionId,
                        commandHandler,
                        "Invalid use of profile command, the skill level was outside bounds: 1 to 51. Use 'help' for more information."
                    );
                    return new ValueTask<string>(request.DialogId);
                }

                profileChangeEvent = HandleSkillCommand(enumSkill, quantity);
                break;
            }
            case "examine":
            {
                profileChangeEvent = HandleExamineCommand();
                break;
            }
            default:
                mailSendService.SendUserMessageToPlayer(
                    sessionId,
                    commandHandler,
                    $"If you are reading this, this is bad. Please report this to SPT staff with a screenshot. Command: {command}."
                );
                return new ValueTask<string>(request.DialogId);
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
            [profileChangeEvent]
        );

        return new ValueTask<string>(request.DialogId);
    }

    protected ProfileChangeEvent HandleSkillCommand(SkillTypes? skill, int level)
    {
        var profileChangeEvent = new ProfileChangeEvent
        {
            Id = new MongoId(),
            Type = "SkillPoints",
            Value = level * 100,
            Entity = skill.ToString(),
        };
        return profileChangeEvent;
    }

    protected ProfileChangeEvent HandleLevelCommand(int level)
    {
        var exp = profileHelper.GetExperience(level);
        var profileChangeEvent = new ProfileChangeEvent
        {
            Id = new MongoId(),
            Type = "ProfileLevel",
            Value = exp,
            Entity = null,
        };
        return profileChangeEvent;
    }

    protected ProfileChangeEvent HandleExamineCommand()
    {
        var profileChangeEvent = new ProfileChangeEvent
        {
            Id = new MongoId(),
            Type = "ExamineAllItems",
            Value = null,
            Entity = null,
        };

        return profileChangeEvent;
    }
}
