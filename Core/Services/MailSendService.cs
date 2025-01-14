using Core.Annotations;
using Core.Helpers;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Dialog;
using Core.Servers;
using Core.Utils;
using ILogger = Core.Models.Utils.ILogger;

namespace Core.Services;

[Injectable]
public class MailSendService
{
    private readonly ILogger _logger;
    private readonly HashUtil _hashUtil;
    private readonly TimeUtil _timeUtil;
    private readonly SaveServer _saveServer;
    private readonly DatabaseService _databaseService;
    private readonly NotifierHelper _notifierHelper;
    private readonly DialogueHelper _dialogueHelper;
    private readonly NotificationSendHelper _notificationSendHelper;
    private readonly LocalisationService _localisationService;
    private readonly ItemHelper _itemHelper;
    private readonly TraderHelper _traderHelper;

    private const string _systemSenderId = "59e7125688a45068a6249071";
    private readonly List<MessageType> _messageTypes = [MessageType.NPC_TRADER, MessageType.FLEAMARKET_MESSAGE];
    private readonly List<string> _slotNames = ["hideout", "main"];

    public MailSendService
    (
        ILogger logger,
        HashUtil hashUtil,
        TimeUtil timeUtil,
        SaveServer saveServer,
        DatabaseService databaseService,
        NotifierHelper notifierHelper,
        DialogueHelper dialogueHelper,
        NotificationSendHelper notificationSendHelper,
        LocalisationService localisationService,
        ItemHelper itemHelper,
        TraderHelper traderHelper
    )
    {
        _logger = logger;
        _hashUtil = hashUtil;
        _timeUtil = timeUtil;
        _saveServer = saveServer;
        _databaseService = databaseService;
        _notifierHelper = notifierHelper;
        _dialogueHelper = dialogueHelper;
        _localisationService = localisationService;
        _itemHelper = itemHelper;
        _traderHelper = traderHelper;
    }

    /**
     * Send a message from an NPC (e.g. prapor) to the player with or without items using direct message text, do not look up any locale
     * @param sessionId The session ID to send the message to
     * @param trader The trader sending the message
     * @param messageType What type the message will assume (e.g. QUEST_SUCCESS)
     * @param message Text to send to the player
     * @param items Optional items to send to player
     * @param maxStorageTimeSeconds Optional time to collect items before they expire
     */
    public void SendDirectNpcMessageToPlayer(
        string sessionId,
        string trader,
        MessageType messageType,
        string message,
        List<Item>? items,
        long? maxStorageTimeSeconds,
        SystemData? systemData,
        MessageContentRagfair? ragfair
    )
    {
        if (trader is null)
        {
            _logger.Error(_localisationService.GetText("mailsend-missing_trader", new
            {
                MessageType = messageType,
                SessionId = sessionId,
            }));

            return;
        }

        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = messageType,
            DialogType = MessageType.NPC_TRADER,
            Trader = trader,
            MessageText = message,
        };

        // Add items to message
        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds ?? 172800;
        }

        if (systemData is not null)
            details.SystemData = systemData;

        if (ragfair is not null)
            details.RagfairDetails = ragfair;

        SendMessageToPlayer(details);
    }

    /**
     * Send a message from an NPC (e.g. prapor) to the player with or without items
     * @param sessionId The session ID to send the message to
     * @param trader The trader sending the message
     * @param messageType What type the message will assume (e.g. QUEST_SUCCESS)
     * @param messageLocaleId The localised text to send to player
     * @param items Optional items to send to player
     * @param maxStorageTimeSeconds Optional time to collect items before they expire
     */
    public void SendLocalisedNpcMessageToPlayer(
        string sessionId,
        string trader,
        MessageType messageType,
        string messageLocaleId,
        List<Item>? items,
        long? maxStorageTimeSeconds,
        SystemData? systemData,
        MessageContentRagfair? ragfair
    )
    {
        if (trader is null)
        {
            _logger.Error(_localisationService.GetText("mailsend-missing_trader", new
            {
                MessageType = messageType,
                SessionId = sessionId,
            }));

            return;
        }

        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = messageType,
            DialogType = MessageType.NPC_TRADER,
            Trader = trader,
            TemplateId = messageLocaleId,
        };

        // add items to message

        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds ?? 172800;
        }

        if (systemData is not null)
            details.SystemData = systemData;

        if (ragfair is not null)
            details.RagfairDetails = ragfair;

        SendMessageToPlayer(details);
    }

    /**
     * Send a message from SYSTEM to the player with or without items
     * @param sessionId The session ID to send the message to
     * @param message The text to send to player
     * @param items Optional items to send to player
     * @param maxStorageTimeSeconds Optional time to collect items before they expire
     */
    public void SendSystemMessageToPlayer(
        string sessionId,
        string message,
        List<Item>? items,
        long? maxStorageTimeSeconds,
        List<ProfileChangeEvent>? profileChangeEvents)
    {
        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = MessageType.SYSTEM_MESSAGE,
            MessageText = message,
        };

        // add items to message
        if (items?.Count > 0)
        {
            var rootItemParentId = _hashUtil.Generate();

            details.Items.AddRange(_itemHelper.AdoptOrphanedItems(rootItemParentId, items));
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds ?? 172800;
        }

        if ((profileChangeEvents?.Count ?? 0) > 0)
            details.ProfileChangeEvents = profileChangeEvents;

        SendMessageToPlayer(details);
    }

    public void SendLocalisedSystemMessageToPlayer(
        string sessionId,
        string messageLocaleId,
        List<Item>? items,
        List<ProfileChangeEvent>? profileChangeEvents,
        long? maxStorageTimeSeconds
    )
    {
        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = MessageType.SYSTEM_MESSAGE,
            TemplateId = messageLocaleId
        };

        // add items to message
        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds ?? 172800;
        }

        if ((profileChangeEvents?.Count ?? 0) > 0)
            details.ProfileChangeEvents = profileChangeEvents;

        SendMessageToPlayer(details);
    }

    /**
     * Send a USER message to a player with or without items
     * @param sessionId The session ID to send the message to
     * @param senderId Who is sending the message
     * @param message The text to send to player
     * @param items Optional items to send to player
     * @param maxStorageTimeSeconds Optional time to collect items before they expire
     */
    public void SendUserMessageToPlayer(
        string sessionId,
        UserDialogInfo senderDetails,
        string message,
        List<Item>? items,
        long? maxStorageTimeSeconds
    )
    {
        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = MessageType.USER_MESSAGE,
            SenderDetails = senderDetails,
            MessageText = message
        };

        // add items to message
        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds ?? 172800;
        }

        SendMessageToPlayer(details);
    }

    /**
     * Large function to send messages to players from a variety of sources (SYSTEM/NPC/USER)
     * Helper functions in this class are available to simplify common actions
     * @param messageDetails Details needed to send a message to the player
     */
    public void SendMessageToPlayer(SendMessageDetails messageDetails)
    {
        // Get dialog, create if doesn't exist
        var senderDialog = GetDialog(messageDetails);

        // Flag dialog as containing a new message to player
        senderDialog.New++;

        // Craft message
        var message = CreateDialogMessage(senderDialog.Id, messageDetails);

        // Create items array
        // Generate item stash if we have rewards.
        var itemsToSendToPlayer = ProcessItemsBeforeAddingToMail(senderDialog.Type, messageDetails);

        // If there's items to send to player, flag dialog as containing attachments
        if ((itemsToSendToPlayer.Data?.Count ?? 0) > 0)
        {
            senderDialog.AttachmentsNew += 1;
        }

        // Store reward items inside message and set appropriate flags inside message
        AddRewardItemsToMessage(message, itemsToSendToPlayer, messageDetails.ItemsMaxStorageLifetimeSeconds);

        if (messageDetails.ProfileChangeEvents is not null)
            message.ProfileChangeEvents = messageDetails.ProfileChangeEvents;

        // Add message to dialog
        senderDialog.Messages.Add(message);

        // TODO: clean up old code here
        // Offer Sold notifications are now separate from the main notification
        if (
            _messageTypes.Contains(senderDialog.Type ?? MessageType.SYSTEM_MESSAGE) &&
            messageDetails?.RagfairDetails is not null
        )
        {
            var offerSoldMessage = _notifierHelper.CreateRagfairOfferSoldNotification(
                message,
                messageDetails.RagfairDetails
            );
            _notificationSendHelper.SendMessage(messageDetails.RecipientId, offerSoldMessage);
            message.MessageType = MessageType.MESSAGE_WITH_ITEMS; // Should prevent getting the same notification popup twice
        }

        // Send message off to player so they get it in client
        var notificationMessage = _notifierHelper.CreateNewMessageNotification(message);
        _notificationSendHelper.SendMessage(messageDetails.RecipientId, notificationMessage);
    }

    public void SendPlayerMessageToNpc(string sessionId, string targetNpcId, string message)
    {
        var playerProfile = _saveServer.GetProfile(sessionId);
        var dialogWithNpc = playerProfile.DialogueRecords[targetNpcId];
        if (dialogWithNpc is null)
        {
            _logger.Error(_localisationService.GetText("mailsend-missing_npc_dialog", targetNpcId));
            return;
        }

        dialogWithNpc.Messages.Add(new()
        {
            Id = _hashUtil.Generate(),
            DateTime = _timeUtil.GetTimeStamp(),
            HasRewards = false,
            UserId = playerProfile.CharacterData.PmcData.Id,
            MessageType = MessageType.USER_MESSAGE,
            RewardCollected = false,
            Text = message
        });
    }

    private Message CreateDialogMessage(string dialogId, SendMessageDetails messageDetails)
    {
        Message message = new()
        {
            Id = _hashUtil.Generate(),
            UserId = dialogId,
            MessageType = messageDetails.DialogType,
            DateTime = _timeUtil.GetTimeStamp(),
            Text = (messageDetails.TemplateId is not null) ? "" : messageDetails.MessageText,
            TemplateId = messageDetails.TemplateId,
            HasRewards = false,
            RewardCollected = false,
            SystemData = messageDetails.SystemData is not null ? messageDetails.SystemData : null,
            ProfileChangeEvents = (messageDetails.ProfileChangeEvents?.Count == 0) ? messageDetails.ProfileChangeEvents : null
        };

        // Clean up empty system data
        // if (message.SystemData is null) {
        //     delete message.SystemData;
        // }

        // Clean up empty template id
        // if (message.TemplateId is null)
        //     delete message.templateId;

        return message;
    }

    /**
     * Add items to message and adjust various properties to reflect the items being added
     * @param message Message to add items to
     * @param itemsToSendToPlayer Items to add to message
     * @param maxStorageTimeSeconds total time items are stored in mail before being deleted
     */
    private void AddRewardItemsToMessage(Message message, MessageItems? itemsToSendToPlayer, long? maxStorageTimeSeconds)
    {
        if ((itemsToSendToPlayer?.Data?.Count ?? 0) > 0)
        {
            message.Items = itemsToSendToPlayer;
            message.HasRewards = true;
            message.MaxStorageTime = maxStorageTimeSeconds;
            message.RewardCollected = false;
        }
    }

    private MessageItems ProcessItemsBeforeAddingToMail(MessageType? dialogType, SendMessageDetails messageDetails)
    {
        var items = _databaseService.GetItems();

        MessageItems itemsToSendToPlayer = new();
        if ((messageDetails.Items?.Count ?? 0) > 0)
        {
            // Find base item that should be the 'primary' + have its parent id be used as the dialogs 'stash' value
            var parentItem = GetBaseItemFromRewards(messageDetails.Items);
            if (parentItem is null)
            {
                _localisationService.GetText("mailsend-missing_parent", new
                {
                    TraderId = messageDetails.Trader,
                    Sender = messageDetails.Sender,
                });

                return itemsToSendToPlayer;
            }

            // No parent id, generate random id and add (doesn't need to be actual parentId from db, only unique)
            if (parentItem?.ParentId is null)
                parentItem.ParentId = _hashUtil.Generate();

            itemsToSendToPlayer = new()
            {
                Stash = parentItem.ParentId, Data = new()
            };

            // Ensure Ids are unique and cont collide with items in player inventory later
            messageDetails.Items = _itemHelper.ReplaceIDs(messageDetails.Items);

            foreach (var reward in messageDetails.Items)
            {
                // Ensure item exists in items db
                var itemTemplate = items[reward.Template];
                if (itemTemplate is null)
                {
                    _logger.Error(_localisationService.GetText("dialog-missing_item_template", new
                    {
                        Tpl = reward.Template,
                        Type = dialogType,
                    }));

                    continue;
                }

                // Ensure every 'base/root' item has the same parentId + has a slotid of 'main'
                if (!(reward.SlotId is not null) || reward.SlotId == "hideout" || reward.ParentId == parentItem.ParentId)
                {
                    // Reward items NEED a parent id + slotid
                    reward.ParentId = parentItem.ParentId;
                    reward.SlotId = "main";
                }

                // Boxes can contain sub-items
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.AMMO_BOX))
                {
                    var boxAndCartridges = new List<Item>();
                    boxAndCartridges.Add(reward);
                    _itemHelper.AddCartridgesToAmmoBox(boxAndCartridges, itemTemplate);

                    // Push box + cartridge children into array
                    itemsToSendToPlayer.Data.AddRange(boxAndCartridges);
                }
                else
                {
                    if (itemTemplate.Properties.StackSlots is not null)
                        _logger.Error(_localisationService.GetText("mail-unable_to_give_gift_not_handled", itemTemplate.Id));

                    // Item is sanitised and ready to be pushed into holding array
                    itemsToSendToPlayer.Data.Add(reward);
                }
            }

            // Remove empty data property if no rewards
            // if (itemsToSendToPlayer.Data.Count == 0)
            //     delete itemsToSendToPlayer.data;
        }

        return itemsToSendToPlayer;
    }

    /**
     * Try to find the most correct item to be the 'primary' item in a reward mail
     * @param items Possible items to choose from
     * @returns Chosen 'primary' item
     */
    private Item GetBaseItemFromRewards(List<Item>? items)
    {
        // Only one item in reward, return it
        if (items?.Count == 1)
            return items[0];

        // Find first item with slotId that indicates its a 'base' item
        var item = items.FirstOrDefault(x => _slotNames.Contains(x.SlotId ?? ""));
        if (item is not null)
            return item;

        // Not a singlular item + no items have a hideout/main slotid
        // Look for first item without parent id
        item = items.FirstOrDefault(x => x.ParentId is null);
        if (item is not null)
            return item;

        // Just return first item in array
        return items[0];
    }

    /**
     * Get a dialog with a specified entity (user/trader)
     * Create and store empty dialog if none exists in profile
     * @param messageDetails Data on what message should do
     * @returns Relevant Dialogue
     */
    private Dialogue GetDialog(SendMessageDetails messageDetails)
    {
        var dialogsInProfile = _dialogueHelper.GetDialogsForProfile(messageDetails.RecipientId);
        var senderId = GetMessageSenderIdByType(messageDetails);
        if (senderId is null)
            throw new Exception(_localisationService.GetText("mail-unable_to_find_message_sender_by_id", messageDetails.Sender));
        
        // Does dialog exist
        var senderDialog = dialogsInProfile[senderId];
        if (senderDialog is null)
        {
            // create if doesnt
            dialogsInProfile[senderId] = senderDialog = new Dialogue()
            {
                Id = senderId,
                Type = (messageDetails.DialogType is not null) ? messageDetails.DialogType : messageDetails.Sender,
                Messages = new(),
                Pinned = false,
                New = 0,
                AttachmentsNew = 0
            };
            
            senderDialog = dialogsInProfile[senderId];
        }
        
        return senderDialog;
    }

    /**
     * Get the appropriate sender id by the sender enum type
     * @param messageDetails
     * @returns gets an id of the individual sending it
     */
    private string? GetMessageSenderIdByType(SendMessageDetails messageDetails)
    {
        if (messageDetails.Sender == MessageType.SYSTEM_MESSAGE)
            return _systemSenderId;

        if (messageDetails.Sender == MessageType.NPC_TRADER || messageDetails.DialogType == MessageType.NPC_TRADER)
            return (messageDetails.Trader is not null) ? _traderHelper.GetValidTraderIdByEnumValue(messageDetails.Trader) : null;

        if (messageDetails.Sender == MessageType.USER_MESSAGE)
            return messageDetails.SenderDetails?.Id;
        
        if (messageDetails.SenderDetails?.Id is not null)
            return messageDetails.SenderDetails.Id;
        
        if (messageDetails.Trader is not null)
            return _traderHelper.GetValidTraderIdByEnumValue(messageDetails.Trader);
        
        _logger.Warning($"Unable to handle message of type: {messageDetails.Sender}");
        return null;
    }
}
