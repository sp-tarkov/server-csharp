using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Helpers.Dialogue.SPTFriend.Commands;

[Injectable]
public class DisplayLocationsHandler(MailSendService _mailSendService) : IChatMessageHandler
{
    public int GetPriority()
    {
        return 99;
    }

    public bool CanHandle(string message)
    {
        return string.Equals(message, "locations", StringComparison.OrdinalIgnoreCase);
    }

    public void Process(MongoId sessionId, UserDialogInfo sptFriendUser, PmcData? sender, object? extraInfo = null)
    {
        // Get all items as an array
        var locations = Enum.GetNames(typeof(ELocationName));
        var totalCount = locations.Length;

        // Keep track of how many have been processed
        int parsedCount = 0;
        while (parsedCount < totalCount)
        {
            var itemsToSend = locations.Skip(parsedCount).Take(15);

            // Convert into a comma separated list, ready to show to player
            var itemsToSendCsv = string.Join(", ", itemsToSend);

            // Wait a little to maintain send order
            Thread.Sleep(500);

            // Send to player
            _mailSendService.SendUserMessageToPlayer(sessionId, sptFriendUser, itemsToSendCsv, [], null);

            // Increment processed count
            parsedCount += itemsToSend.Count();
        }
    }
}
