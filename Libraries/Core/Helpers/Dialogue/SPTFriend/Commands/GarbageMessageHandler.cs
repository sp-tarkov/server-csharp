using Core.Models.Eft.Common;
using Core.Models.Eft.Profile;
using Core.Services;
using Core.Utils;
using SptCommon.Annotations;

namespace Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class GarbageMessageHandler(
    MailSendService _mailSendService,
    RandomUtil _randomUtil) : IChatMessageHandler
{
    public int GetPriority()
    {
        return 100;
    }

    public bool CanHandle(string message)
    {
        return message.ToLower() == "garbage";
    }

    public void Process(string sessionId, UserDialogInfo sptFriendUser, PmcData sender)
    {
        var beforeCollect = GC.GetTotalMemory(false) / 1024 / 1024;

        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

        var afterCollect = GC.GetTotalMemory(false) / 1024 / 1024;

        _mailSendService.SendUserMessageToPlayer(
            sessionId,
            sptFriendUser,
            $"Before: {beforeCollect}MB, After: {afterCollect}MB",
            [],
            null
        );

        
    }
}
