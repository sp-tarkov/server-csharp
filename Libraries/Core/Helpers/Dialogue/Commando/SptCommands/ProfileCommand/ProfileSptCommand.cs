using System.Text.RegularExpressions;
using Core.Helpers.Dialog.Commando.SptCommands;
using Core.Helpers.Dialogue.Commando.SptCommands.GiveCommand;
using Core.Models.Eft.Common.Tables;
using SptCommon.Annotations;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Dialog;
using Core.Models.Utils;
using Core.Servers;
using Core.Services;
using Core.Utils;

namespace Core.Helpers.Dialogue.Commando.SptCommands.ProfileCommand;

[Injectable]
public class ProfileSptCommand(
    ISptLogger<ProfileSptCommand> _logger,
    ItemHelper _itemHelper,
    HashUtil _hashUtil,
    PresetHelper _presetHelper,
    MailSendService _mailSendService,
    LocaleService _localeService,
    DatabaseServer dbServer,
    ProfileHelper _profileHelper
    ) : ISptCommand
{

    /**
    * Regex to account for all these cases:
    * spt profile level 20
    * spt profile skill metabolism 10
    */
    // TODO: Fix this shit as Valens doesn't know Regex.
     protected Regex _commandRegex = new("""^spt profile (?<command>level|skill)((?<=.*skill) (?<skill>[\w]+)){0,1} (?<quantity>(?!0+)[0-9]+)$/""");
    protected Regex _examineRegex = new ("""/^spt profile (?<command>examine)/""");
    //
    protected SavedCommand _savedCommand = null;
    
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
        var isCommand = _commandRegex.IsMatch(request.Text);
        var isExamine = _examineRegex.IsMatch(request.Text);

         if (!isCommand && !isExamine) {
             _mailSendService.SendUserMessageToPlayer(
                 sessionId,
                 commandHandler,
                 "Invalid use of trader command. Use 'help' for more information."
             );
             return request.DialogId;
         }
        
         var result = _commandRegex.Match(request.Text);
        
         var command = result.Groups["command"].Captures[0].Value;
         var skill = result.Groups["skill"].Captures[0].Value;
         var quantity = int.Parse(result.Groups["quantity"].Captures[0].Value);

        ProfileChangeEvent profileChangeEvent;
         switch (command) {
             case "level":
                 if (quantity < 1 || quantity > _profileHelper.GetMaxLevel()) {
                     _mailSendService.SendUserMessageToPlayer(
                         sessionId,
                         commandHandler,
                         "Invalid use of profile command, the level was outside bounds: 1 to 70. Use 'help' for more information."
                     );
                     return request.DialogId;
                 }
                 profileChangeEvent = HandleLevelCommand(quantity);
                 break;
             case "skill": {
                 var enumSkill = Enum.GetValues(typeof(SkillTypes)).Cast<SkillTypes>().FirstOrDefault(
                     (t) => t.ToString() == skill);
        
                 if (enumSkill == null) {
                     _mailSendService.SendUserMessageToPlayer(
                         sessionId,
                         commandHandler,
                         "Invalid use of profile command, the skill was not found. Use 'help' for more information."
                     );
                     return request.DialogId;
                 }
        
                 if (quantity is < 0 or > 51) {
                     _mailSendService.SendUserMessageToPlayer(
                         sessionId,
                         commandHandler,
                         "Invalid use of profile command, the skill level was outside bounds: 1 to 51. Use 'help' for more information."
                     );
                     return request.DialogId;
                 }
        
                 profileChangeEvent = HandleSkillCommand(enumSkill, quantity);
                 break;
             }
             case "examine": {
                 profileChangeEvent = HandleExamineCommand();
                 break;
             }
             default:
                 _mailSendService.SendUserMessageToPlayer(
                     sessionId,
                     commandHandler,
                     $"If you are reading this, this is bad. Please report this to SPT staff with a screenshot. Command: {command}."
                 );
                 return request.DialogId;
         }
        
         _mailSendService.SendSystemMessageToPlayer(
             sessionId,
             "A single ruble is being attached, required by BSG logic.",
             [
                 new Item{
                     Id = _hashUtil.Generate(),
                     Template = Money.ROUBLES,
                     Upd = new Upd{ StackObjectsCount = 1 },
                     ParentId = _hashUtil.Generate(),
                     SlotId = "main",
                 },
             ],
             null,
             [profileChangeEvent]
         );
         return request.DialogId;
    }

    protected ProfileChangeEvent HandleSkillCommand(SkillTypes skill, int level)
    {
         var profileChangeEvent = new ProfileChangeEvent
         {
             Id = _hashUtil.Generate(),
             Type = ProfileChangeEventType.SkillPoints,
             Value = level * 100,
             Entity = skill.ToString(),
         };
         return profileChangeEvent;
    }

    protected ProfileChangeEvent HandleLevelCommand(int level)
    {
         var exp = _profileHelper.GetExperience(level);
         var profileChangeEvent = new ProfileChangeEvent
         {
             Id = _hashUtil.Generate(),
             Type = ProfileChangeEventType.ProfileLevel,
             Value = exp,
             Entity = null,
         };
         return profileChangeEvent;
    }
    
    protected ProfileChangeEvent HandleExamineCommand() {
         var profileChangeEvent = new ProfileChangeEvent
         {
             Id = _hashUtil.Generate(),
             Type = ProfileChangeEventType.ExamineAllItems,
             Value = null,
             Entity = null,
         };

         return profileChangeEvent;
    }
}
