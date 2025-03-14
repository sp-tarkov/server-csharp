using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Common.Annotations;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class DialogueHelper(
    ISptLogger<DialogueHelper> _logger,
    HashUtil _hashUtil,
    DatabaseServer _databaseServer,
    NotifierHelper _notifierHelper,
    ProfileHelper _profileHelper,
    NotificationSendHelper _notificationSendHelper,
    LocalisationService _localisationService,
    ItemHelper _itemHelper
)
{
    /// <summary>
    ///     Get the preview contents of the last message in a dialogue.
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns>MessagePreview</returns>
    public MessagePreview GetMessagePreview(Models.Eft.Profile.Dialogue? dialogue)
    {
        // The last message of the dialogue should be shown on the preview.
        var message = dialogue.Messages.Last();
        MessagePreview result = new()
        {
            DateTime = message?.DateTime,
            MessageType = message?.MessageType,
            TemplateId = message?.TemplateId,
            UserId = dialogue?.Id
        };

        if (message?.Text is not null)
        {
            result.Text = message.Text;
        }

        if (message?.SystemData is not null)
        {
            result.SystemData = message?.SystemData;
        }

        return result;
    }

    /// <summary>
    ///     Get the item contents for a particular message.
    /// </summary>
    /// <param name="messageID"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <param name="itemId">Item being moved to inventory</param>
    /// <returns></returns>
    public List<Item> GetMessageItemContents(string messageID, string sessionID, string itemId)
    {
        var fullProfile = _profileHelper.GetFullProfile(sessionID);
        var dialogueData = fullProfile.DialogueRecords;
        foreach (var dialogue in dialogueData)
        {
            var message = dialogueData[dialogue.Key].Messages.FirstOrDefault(x => x.Id == messageID);
            if (message is null)
            {
                continue;
            }

            if (message.Id == messageID)
            {
                var attachmentsNew = fullProfile.DialogueRecords[dialogue.Key].AttachmentsNew;
                if (attachmentsNew > 0)
                {
                    fullProfile.DialogueRecords[dialogue.Key].AttachmentsNew = attachmentsNew - 1;
                }

                // Check reward count when item being moved isn't in reward list
                // If count is 0, it means after this move occurs the reward array will be empty and all rewards collected
                if (message.Items.Data is null)
                {
                    message.Items.Data = [];
                }

                var rewardItems = message.Items.Data?.Where(x => x.Id != itemId);
                if (!rewardItems.Any())
                {
                    message.RewardCollected = true;
                    message.HasRewards = false;
                }

                return message.Items.Data;
            }
        }

        return [];
    }

    /// <summary>
    ///     Get the dialogs dictionary for a profile, create if doesn't exist
    /// </summary>
    /// <param name="sessionId">Session/player id</param>
    /// <returns>Dialog dictionary</returns>
    public Dictionary<string, Models.Eft.Profile.Dialogue> GetDialogsForProfile(string sessionId)
    {
        var profile = _profileHelper.GetFullProfile(sessionId);
        return profile.DialogueRecords ?? (profile.DialogueRecords = new Dictionary<string, Models.Eft.Profile.Dialogue>());
    }

    /// <summary>
    /// Find and return a profiles dialogue by id
    /// </summary>
    /// <param name="profileId">Profile to look in</param>
    /// <param name="dialogueId">Dialog to return</param>
    /// <returns>Dialogue</returns>
    public Models.Eft.Profile.Dialogue? GetDialogueFromProfile(string profileId, string dialogueId)
    {
        var dialogues = GetDialogsForProfile(profileId);
        if (dialogues.TryGetValue(dialogueId, out var dialogue))
        {
            return dialogue;
        }

        _logger.Error($"Unable to find a dialogue with id: {dialogueId} in profile: {profileId}");
        return null;
    }
}
