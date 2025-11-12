using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Common.Models.Logging;

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
        if (fullProfile.DialogueRecords is null)
        {
            logger.Error("DialogueData is null when trying to get message item contents");
            return [];
        }

        foreach (var (dialogId, dialog) in fullProfile.DialogueRecords)
        {
            var message = dialog.Messages?.FirstOrDefault(x => x.Id == messageId);
            if (message is null)
            {
                continue;
            }

            message.Items ??= new MessageItems();
            message.Items.Data ??= [];

            // Check reward count when item being moved (and its children) isn't in reward list
            // If count is 0, it means after this move occurs the reward array will be empty and all rewards collected
            var itemWithChildren = message.Items.Data.GetItemWithChildren(itemId);
            var remainingItems = message.Items.Data.Except(itemWithChildren);
            if (!remainingItems.Any())
            {
                message.RewardCollected = true;
                message.HasRewards = false;

                if (dialog.AttachmentsNew > 0)
                {
                    dialog.AttachmentsNew--;
                }
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
