using Core.Helpers.Dialog.Commando.SptCommands;
using SptCommon.Annotations;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Spt.Dialog;
using Core.Models.Spt.Logging;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Helpers.Dialogue.Commando.SptCommands.ProfileCommand;

[Injectable]
public class ProfileSptCommand : ISptCommand
{
    // Constructor
    // (
    // SptLogger _logger,
    //     ItemHelper _itemHelper,
    // HashUtil _hashUtil,
    //     PresetHelper _presetHelper,
    // MailSendService _mailSendService,
    //     LocaleService _localeService,
    // DatabaseServer dbServer,
    //     ProfileHelper _profileHelper
    // ) 
    
    /**
    * Regex to account for all these cases:
    * spt profile level 20
    * spt profile skill metabolism 10
    */
    
    // TODO: Fix this shit as Valens doesn't know Regex.
    // Regex commandRegex = new Regex(^spt profile (?<command>level|skill)((?<=.*skill) (?<skill>[\w]+)){0,1} (?<quantity>(?!0+)[0-9]+)$/);
    // Regex examineRegex = new Regex(/^spt profile (?<command>examine)/);
    //
    // protected savedCommand = SavedCommand;
    
    public string GetCommand()
    {
        return "profile";
    }

    public string GetCommandHelp()
    {
        return "spt profile\n========\nSets the profile level or skill to the desired level through the message system.\n\n\tspt " +
               "profile level [desired level]\n\t\tEx: spt profile level 20\n\n\tspt profile skill [skill name] [quantity]\n\t\tEx: " +
               "spt profile skill metabolism 51";
    }

    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        // TODO: Fix the leftover errors.
        // if (ProfileSptCommand.commandRegex.test(request.text) is null && ProfileSptCommand.examineRegex.test(request.text) is null) {
        //     _mailSendService.SendUserMessageToPlayer(
        //         sessionId,
        //         commandHandler,
        //         "Invalid use of trader command. Use 'help' for more information."
        //     );
        //     return request.DialogId;
        // }
        //
        // var result =
        //     ProfileSptCommand.commandRegex.exec(request.text) ?? ProfileSptCommand.examineRegex.exec(request.text);
        //
        // var command = result.groups.command;
        // var skill = result.groups.skill;
        // var quantity = +result.groups.quantity;
        //
        // ProfileChangeEvent profileChangeEvent;
        // switch (command) {
        //     case "level":
        //         if (quantity < 1 || quantity > _profileHelper.GetMaxLevel()) {
        //             _mailSendService.SendUserMessageToPlayer(
        //                 sessionId,
        //                 commandHandler,
        //                 "Invalid use of profile command, the level was outside bounds: 1 to 70. Use 'help' for more information."
        //             );
        //             return request.DialogId;
        //         }
        //         profileChangeEvent = HandleLevelCommand(quantity);
        //         break;
        //     case "skill": {
        //         var enumSkill = SkillTypes.find(
        //             (t) => t.toLocaleLowerCase() === skill.toLocaleLowerCase(),
        //         );
        //
        //         if (enumSkill == undefined) {
        //             _mailSendService.SendUserMessageToPlayer(
        //                 sessionId,
        //                 commandHandler,
        //                 "Invalid use of profile command, the skill was not found. Use 'help' for more information."
        //             );
        //             return request.DialogId;
        //         }
        //
        //         if (quantity < 0 || quantity > 51) {
        //             _mailSendService.SendUserMessageToPlayer(
        //                 sessionId,
        //                 commandHandler,
        //                 "Invalid use of profile command, the skill level was outside bounds: 1 to 51. Use 'help' for more information."
        //             );
        //             return request.DialogId;
        //         }
        //
        //         profileChangeEvent = HandleSkillCommand(enumSkill, quantity);
        //         break;
        //     }
        //     case "examine": {
        //         profileChangeEvent = HandleExamineCommand();
        //         break;
        //     }
        //     default:
        //         _mailSendService.SendUserMessageToPlayer(
        //             sessionId,
        //             commandHandler,
        //             $"If you are reading this, this is bad. Please report this to SPT staff with a screenshot. Command ${command}."
        //         );
        //         return request.DialogId;
        // }
        //
        // _mailSendService.SendSystemMessageToPlayer(
        //     sessionId,
        //     "A single ruble is being attached, required by BSG logic.",
        //     [
        //         {
        //             _id = _hashUtil.generate(),
        //             _tpl = Money.ROUBLES,
        //             upd = { StackObjectsCount: 1 },
        //             parentId = _hashUtil.Generate(),
        //             slotId = "main",
        //         },
        //     ],
        //     undefined,
        //     [profileChangeEvent]
        // );
        // return request.DialogId;
        throw new NotImplementedException();
    }

    protected ProfileChangeEvent HandleSkillCommand(string skill, int level)
    {
        // TODO: Fix the leftover errors.
        // ProfileChangeEvent profileChangeEvent = {
        //     _id = _hashUtil.Generate(),
        //     Type = ProfileChangeEventType.SkillPoints,
        //     value = level * 100,
        //     entity = skill,
        // };
        // return profileChangeEvent;
        throw new NotImplementedException();
    }

    protected ProfileChangeEvent HandleLevelCommand(int level)
    {
        // TODO: Fix the leftover errors.
        // var exp = _profileHelper.GetExperience(level);
        // ProfileChangeEvent profileChangeEvent = {
        //     _id = _hashUtil.Generate(),
        //     Type = ProfileChangeEventType.ProfileLevel,
        //     value = exp,
        //     entity = undefined,
        // };
        // return profileChangeEvent;
        throw new NotImplementedException();
    }
    
    protected ProfileChangeEvent HandleExamineCommand() {
        // TODO: Fix the leftover errors.
        // ProfileChangeEvent profileChangeEvent = {
        //     id = _hashUtil.Generate(),
        //     Type = ProfileChangeEventType.ExamineAllItems,
        //     value = undefined,
        //     entity = undefined,
        // };
        // return profileChangeEvent;
        throw new NotImplementedException();
    }
}
