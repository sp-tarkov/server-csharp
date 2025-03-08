using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

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
