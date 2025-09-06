using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class DialogueHelper(ISptLogger<DialogueHelper> logger, ProfileHelper profileHelper)
{
    /// <summary>
    ///     Get the preview contents of the last message in a dialogue.
    /// </summary>
    /// <param name="dialogue"></param>
    /// <returns>MessagePreview</returns>
    public MessagePreview GetMessagePreview(Models.Eft.Profile.Dialogue? dialogue)
    {
        // The last message of the dialogue should be shown on the preview.
        var message = dialogue?.Messages?.LastOrDefault();

        MessagePreview result = new()
        {
            DateTime = message?.DateTime,
            MessageType = message?.MessageType,
            TemplateId = message?.TemplateId,
            UserId = dialogue?.Id,
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
    /// <param name="messageId"></param>
    /// <param name="sessionId">Session/player id</param>
    /// <param name="itemId">Item being moved to inventory</param>
    /// <returns>Collection of items from message</returns>
    public List<Item>? GetMessageItemContents(MongoId messageId, MongoId sessionId, MongoId itemId)
    {
        var fullProfile = profileHelper.GetFullProfile(sessionId);
        var dialogueData = fullProfile.DialogueRecords;
        if (dialogueData is null)
        {
            logger.Error("DialogueData is null when trying to get message item contents");
            return [];
        }

        foreach (var (dialogId, dialog) in dialogueData)
        {
            var message = dialog.Messages?.FirstOrDefault(x => x.Id == messageId);
            if (message is null)
            {
                continue;
            }

            if (message.Id != messageId)
            {
                continue;
            }

            if (fullProfile.DialogueRecords is null)
            {
                continue;
            }

            var attachmentsNew = fullProfile.DialogueRecords[dialogId].AttachmentsNew;
            if (attachmentsNew > 0)
            {
                fullProfile.DialogueRecords[dialogId].AttachmentsNew = attachmentsNew - 1;
            }

            // Check reward count when item being moved isn't in reward list
            // If count is 0, it means after this move occurs the reward array will be empty and all rewards collected
            message.Items ??= new MessageItems();
            message.Items.Data ??= [];

            var messageItems = message.Items.Data?.Where(x => x.Id != itemId);
            if (messageItems is null || !messageItems.Any())
            {
                message.RewardCollected = true;
                message.HasRewards = false;
            }

            return message.Items.Data;
        }

        return [];
    }

    /// <summary>
    ///     Get the dialogs dictionary for a profile, create if it doesn't exist
    /// </summary>
    /// <param name="sessionId">Session/player id</param>
    /// <returns>Dialog dictionary</returns>
    public Dictionary<MongoId, Models.Eft.Profile.Dialogue> GetDialogsForProfile(MongoId sessionId)
    {
        var profile = profileHelper.GetFullProfile(sessionId);
        return profile.DialogueRecords ?? (profile.DialogueRecords = new Dictionary<MongoId, Models.Eft.Profile.Dialogue>());
    }

    /// <summary>
    ///     Find and return a profiles dialogue by id
    /// </summary>
    /// <param name="profileId">Profile to look in</param>
    /// <param name="dialogueId">Dialog to return</param>
    /// <returns>Dialogue</returns>
    public Models.Eft.Profile.Dialogue? GetDialogueFromProfile(MongoId profileId, MongoId dialogueId)
    {
        var dialogues = GetDialogsForProfile(profileId);
        if (dialogues.TryGetValue(dialogueId, out var dialogue))
        {
            return dialogue;
        }

        logger.Error($"Unable to find a dialogue with id: {dialogueId} in profile: {profileId}");
        return null;
    }
}
