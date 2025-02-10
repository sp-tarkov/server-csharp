using Core.Helpers.Dialogue;
using Core.Models.Eft.Dialog;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Services;
using SptCommon.Annotations;

namespace _20CustomChatBot;

[Injectable]
public class CustomChatBot : IDialogueChatBot
{
    private MailSendService _mailSendService;

    public CustomChatBot(
        MailSendService mailSendService
    )
    {
        _mailSendService = mailSendService;
    }

    public UserDialogInfo GetChatBot()
    {
        return new UserDialogInfo
        {
            Id = "modderBuddy",
            Aid = 9999999,
            Info = new UserDialogDetails
            {
                Nickname = "Buddy",
                Side = "Usec",
                Level = 69,
                MemberCategory = MemberCategory.Sherpa,
                SelectedMemberCategory = MemberCategory.Sherpa
            }
        };
    }

    public string? HandleMessage(string sessionId, SendMessageRequest request)
    {
        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            GetChatBot(),
            $"Im Buddy! I just reply back what you typed to me!\n{request.Text}");

        return request.DialogId;
    }
}
