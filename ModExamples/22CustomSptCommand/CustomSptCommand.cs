using Core.Helpers;
using Core.Helpers.Dialog.Commando.SptCommands;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Services;
using SptCommon.Annotations;

namespace _22CustomSptCommand;

[Injectable]
public class CustomSptCommand : ISptCommand
{
    private readonly MailSendService _mailSendService;
    private readonly ItemHelper _itemHelper;

    public CustomSptCommand(
        MailSendService mailSendService,
        ItemHelper itemHelper
    )
    {
        _mailSendService = mailSendService;
        _itemHelper = itemHelper;
    }

    public string GetCommand()
    {
        return "getName";
    }

    public string GetCommandHelp()
    {
        return "Usage: spt getName tplId";
    }

    public string PerformAction(UserDialogInfo commandHandler, string sessionId, SendMessageRequest request)
    {
        var splitCommand  = request.Text.Split(" ");
        _mailSendService.SendUserMessageToPlayer(sessionId, commandHandler, $"That templateId belongs to item {_itemHelper.GetItem(splitCommand[2]).Value?.Properties?.Name ?? ""}");
        return request.DialogId;
    }
}
