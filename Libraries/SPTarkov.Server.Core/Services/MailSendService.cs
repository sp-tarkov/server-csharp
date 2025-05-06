using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Dialog;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Services;

[Injectable]
public class MailSendService(
    ISptLogger<MailSendService> _logger,
    HashUtil _hashUtil,
    TimeUtil _timeUtil,
    SaveServer _saveServer,
    DatabaseService _databaseService,
    NotifierHelper _notifierHelper,
    DialogueHelper _dialogueHelper,
    NotificationSendHelper _notificationSendHelper,
    LocalisationService _localisationService,
    ItemHelper _itemHelper,
    TraderHelper _traderHelper,
    ICloner _cloner
)
{
    private const string _systemSenderId = "59e7125688a45068a6249071";
    protected HashSet<MessageType> _messageTypes = [MessageType.NPC_TRADER, MessageType.FLEAMARKET_MESSAGE];
    protected HashSet<string> _slotNames = ["hideout", "main"];

    /// <summary>
    ///     Send a message from an NPC (e.g. prapor) to the player with or without items using direct message text, do not look up any locale
    /// </summary>
    /// <param name="sessionId"> The session ID to send the message to </param>
    /// <param name="trader"> The trader sending the message </param>
    /// <param name="messageType"> What type the message will assume (e.g. QUEST_SUCCESS) </param>
    /// <param name="message"> Text to send to the player </param>
    /// <param name="items"> Optional items to send to player </param>
    /// <param name="maxStorageTimeSeconds"> Optional time to collect items before they expire </param>
    /// <param name="systemData"> </param>
    /// <param name="ragfair"> </param>
    public void SendDirectNpcMessageToPlayer(
        string sessionId,
        string? trader,
        MessageType messageType,
        string message,
        List<Item>? items,
        long? maxStorageTimeSeconds = 172800,
        SystemData? systemData = null,
        MessageContentRagfair? ragfair = null
    )
    {
        if (trader is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "mailsend-missing_trader",
                    new
                    {
                        messageType,
                        sessionId
                    }
                )
            );

            return;
        }

        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = messageType,
            DialogType = MessageType.NPC_TRADER,
            Trader = trader,
            MessageText = message,
            Items = []
        };

        // Add items to message
        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds;
        }

        if (systemData is not null)
        {
            details.SystemData = systemData;
        }

        if (ragfair is not null)
        {
            details.RagfairDetails = ragfair;
        }

        SendMessageToPlayer(details);
    }

    /// <summary>
    ///     Send a message from an NPC (e.g. prapor) to the player with or without items
    /// </summary>
    /// <param name="sessionId"> The session ID to send the message to </param>
    /// <param name="trader"> The trader sending the message </param>
    /// <param name="messageType"> What type the message will assume (e.g. QUEST_SUCCESS) </param>
    /// <param name="messageLocaleId"> The localised text to send to player </param>
    /// <param name="items"> Optional items to send to player </param>
    /// <param name="maxStorageTimeSeconds"> Optional time to collect items before they expire </param>
    /// <param name="systemData"></param>
    /// <param name="ragfair"></param>
    public void SendLocalisedNpcMessageToPlayer(
        string sessionId,
        string? trader,
        MessageType messageType,
        string messageLocaleId,
        List<Item>? items,
        long? maxStorageTimeSeconds = 172800,
        SystemData? systemData = null,
        MessageContentRagfair? ragfair = null
    )
    {
        if (trader is null)
        {
            _logger.Error(
                _localisationService.GetText(
                    "mailsend-missing_trader",
                    new
                    {
                        messageType,
                        sessionId
                    }
                )
            );

            return;
        }

        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = messageType,
            DialogType = MessageType.NPC_TRADER,
            Trader = trader,
            TemplateId = messageLocaleId,
            Items = new List<Item>()
        };

        // add items to message

        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            if (maxStorageTimeSeconds is not null && maxStorageTimeSeconds > 0)
            {
                details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds;
            }
            else
            {
                details.ItemsMaxStorageLifetimeSeconds = 172800;
            }
        }

        if (systemData is not null)
        {
            details.SystemData = systemData;
        }

        if (ragfair is not null)
        {
            details.RagfairDetails = ragfair;
        }

        SendMessageToPlayer(details);
    }

    /// <summary>
    ///     Send a message from SYSTEM to the player with or without items
    /// </summary>
    /// <param name="sessionId"> The session ID to send the message to </param>
    /// <param name="message"> The text to send to player </param>
    /// <param name="items"> Optional items to send to player </param>
    /// <param name="maxStorageTimeSeconds"> Optional time to collect items before they expire </param>
    /// <param name="profileChangeEvents"></param>
    public void SendSystemMessageToPlayer(
        string sessionId,
        string message,
        List<Item>? items,
        long? maxStorageTimeSeconds = 172800,
        List<ProfileChangeEvent>? profileChangeEvents = null)
    {
        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = MessageType.SYSTEM_MESSAGE,
            MessageText = message,
            Items = new List<Item>()
        };

        // add items to message
        if (items?.Count > 0)
        {
            var rootItemParentId = _hashUtil.Generate();

            details.Items.AddRange(_itemHelper.AdoptOrphanedItems(rootItemParentId, items));
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds;
        }

        if ((profileChangeEvents?.Count ?? 0) > 0)
        {
            details.ProfileChangeEvents = profileChangeEvents;
        }

        SendMessageToPlayer(details);
    }

    /// <summary>
    ///     Send a message from SYSTEM to the player with or without items with localised text
    /// </summary>
    /// <param name="sessionId"> The session ID to send the message to </param>
    /// <param name="messageLocaleId"> Id of key from locale file to send to player </param>
    /// <param name="items"> Optional items to send to player </param>
    /// <param name="profileChangeEvents"></param>
    /// <param name="maxStorageTimeSeconds"> Optional time to collect items before they expire </param>
    public void SendLocalisedSystemMessageToPlayer(
        string sessionId,
        string messageLocaleId,
        List<Item>? items,
        List<ProfileChangeEvent>? profileChangeEvents,
        long? maxStorageTimeSeconds = 172800
    )
    {
        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = MessageType.SYSTEM_MESSAGE,
            TemplateId = messageLocaleId,
            Items = new List<Item>()
        };

        // add items to message
        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds;
        }

        if ((profileChangeEvents?.Count ?? 0) > 0)
        {
            details.ProfileChangeEvents = profileChangeEvents;
        }

        SendMessageToPlayer(details);
    }

    /// <summary>
    ///     Send a USER message to a player with or without items
    /// </summary>
    /// <param name="sessionId"> The session ID to send the message to </param>
    /// <param name="senderDetails"> Who is sending the message </param>
    /// <param name="message"> The text to send to player </param>
    /// <param name="items"> Optional items to send to player </param>
    /// <param name="maxStorageTimeSeconds"> Optional time to collect items before they expire </param>
    public void SendUserMessageToPlayer(
        string sessionId,
        UserDialogInfo senderDetails,
        string message,
        List<Item>? items = null,
        long? maxStorageTimeSeconds = 172800
    )
    {
        SendMessageDetails details = new()
        {
            RecipientId = sessionId,
            Sender = MessageType.USER_MESSAGE,
            SenderDetails = senderDetails,
            MessageText = message,
            Items = new List<Item>()
        };

        // add items to message
        if (items?.Count > 0)
        {
            details.Items.AddRange(items);
            details.ItemsMaxStorageLifetimeSeconds = maxStorageTimeSeconds;
        }

        SendMessageToPlayer(details);
    }

    /// <summary>
    ///     Large function to send messages to players from a variety of sources (SYSTEM/NPC/USER).
    ///     Helper functions in this class are available to simplify common actions
    /// </summary>
    /// <param name="messageDetails"> Details needed to send a message to the player </param>
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
        {
            message.ProfileChangeEvents = messageDetails.ProfileChangeEvents;
        }

        // Add message to dialog
        senderDialog.Messages.Add(message);

        // TODO: clean up old code here
        // Offer Sold notifications are now separate from the main notification
        if (_messageTypes.Contains(senderDialog.Type ?? MessageType.SYSTEM_MESSAGE) &&
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

    /// <summary>
    ///     Send a message from the player to an NPC
    /// </summary>
    /// <param name="sessionId"> Session ID </param>
    /// <param name="targetNpcId"> NPC message is sent to </param>
    /// <param name="message"> Text to send to NPC </param>
    public void SendPlayerMessageToNpc(string sessionId, string targetNpcId, string message)
    {
        var playerProfile = _saveServer.GetProfile(sessionId);
        if (playerProfile.DialogueRecords is null ||
            !playerProfile.DialogueRecords.TryGetValue(targetNpcId, out var dialogWithNpc))
        {
            _logger.Error(_localisationService.GetText("mailsend-missing_npc_dialog", targetNpcId));
            return;
        }

        dialogWithNpc.Messages.Add(
            new Message
            {
                Id = _hashUtil.Generate(),
                DateTime = _timeUtil.GetTimeStamp(),
                HasRewards = false,
                UserId = playerProfile.CharacterData.PmcData.Id,
                MessageType = MessageType.USER_MESSAGE,
                RewardCollected = false,
                Text = message
            }
        );
    }

    /// <summary>
    ///     Create a message for storage inside a dialog in the player profile
    /// </summary>
    /// <param name="dialogId"> ID of dialog that will hold the message </param>
    /// <param name="messageDetails"> Various details on what the message must contain/do </param>
    /// <returns> Message </returns>
    private Message CreateDialogMessage(string dialogId, SendMessageDetails messageDetails)
    {
        Message message = new()
        {
            Id = _hashUtil.Generate(),
            UserId = dialogId,
            MessageType = messageDetails.DialogType,
            DateTime = _timeUtil.GetTimeStamp(),
            Text = messageDetails.TemplateId is not null ? "" : messageDetails.MessageText,
            TemplateId = messageDetails.TemplateId,
            HasRewards = false,
            RewardCollected = false,
            SystemData = messageDetails.SystemData is not null ? messageDetails.SystemData : null,
            ProfileChangeEvents = messageDetails.ProfileChangeEvents?.Count == 0 ? messageDetails.ProfileChangeEvents : null
        };

        // Handle replyTo
        if (messageDetails.ReplyTo is not null)
        {
            var replyMessage = GetMessageToReplyTo(messageDetails.RecipientId, messageDetails.ReplyTo, dialogId);
            if (replyMessage is not null)
            {
                message.ReplyTo = replyMessage;
            }
        }

        return message;
    }

    /// <summary>
    ///     Finds the Message to reply to using the ID of the recipient, message and the dialogue.
    /// </summary>
    /// <param name="recipientId"> The ID of the recipient </param>
    /// <param name="replyToId"> The ID of the message to reply to </param>
    /// <param name="dialogueId"> The ID of the dialogue (traderId or profileId) </param>
    /// <returns> A new instance with data from the found message, otherwise undefined </returns>
    protected ReplyTo? GetMessageToReplyTo(string recipientId, string replyToId, string dialogueId)
    {
        var currentDialogue = _dialogueHelper.GetDialogueFromProfile(recipientId, dialogueId);

        if (currentDialogue is null)
        {
            _logger.Warning($"Unable to find dialogue: {dialogueId} from sender");
            return null;
        }

        var messageToReplyTo = currentDialogue.Messages?.FirstOrDefault(message => message.Id == replyToId);
        if (messageToReplyTo is null)
        {
            return null;
        }

        return new ReplyTo
        {
            Id = messageToReplyTo.Id,
            DateTime = messageToReplyTo.DateTime,
            MessageType = messageToReplyTo.MessageType,
            UserId = messageToReplyTo.UserId,
            Text = messageToReplyTo.Text
        };
    }

    /// <summary>
    ///     Add items to message and adjust various properties to reflect the items being added
    /// </summary>
    /// <param name="message"> Message to add items to </param>
    /// <param name="itemsToSendToPlayer"> Items to add to message </param>
    /// <param name="maxStorageTimeSeconds"> Total time the items are stored in mail before being deleted </param>
    private void AddRewardItemsToMessage(Message message, MessageItems? itemsToSendToPlayer, long? maxStorageTimeSeconds)
    {
        if ((itemsToSendToPlayer?.Data?.Count ?? 0) > 0)
        {
            message.Items = itemsToSendToPlayer;
            message.HasRewards = true;
            message.MaxStorageTime = maxStorageTimeSeconds ?? 172800;
            message.RewardCollected = false;
        }
    }

    /// <summary>
    ///     Perform various sanitising actions on the items before they're considered ready for insertion into message
    /// </summary>
    /// <param name="dialogType"> The type of the dialog that will hold the reward items being processed </param>
    /// <param name="messageDetails"> Details fo the message e.g. Text, items it has etc. </param>
    /// <returns> Sanitised items </returns>
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
                _localisationService.GetText(
                    "mailsend-missing_parent",
                    new
                    {
                        traderId = messageDetails.Trader,
                        sender = messageDetails.Sender
                    }
                );

                return itemsToSendToPlayer;
            }

            // No parent id, generate random id and add (doesn't need to be actual parentId from db, only unique)
            if (parentItem?.ParentId is null)
            {
                parentItem.ParentId = _hashUtil.Generate();
            }

            // Prep return object
            itemsToSendToPlayer = new MessageItems
            {
                Stash = parentItem.ParentId,
                Data = new List<Item>()
            };

            // Ensure Ids are unique and cont collide with items in player inventory later
            messageDetails.Items = _itemHelper.ReplaceIDs(_cloner.Clone(messageDetails.Items));

            // Ensure item exits in items db
            foreach (var reward in messageDetails.Items)
            {
                var itemTemplate = items[reward.Template];
                if (itemTemplate is null)
                {
                    _logger.Error(
                        _localisationService.GetText(
                            "dialog-missing_item_template",
                            new
                            {
                                tpl = reward.Template,
                                type = dialogType
                            }
                        )
                    );

                    continue;
                }

                // Ensure every 'base/root' item has the same parentId + has a slotId of 'main'
                if (reward.SlotId is null || reward.SlotId == "hideout" || reward.ParentId == parentItem.ParentId)
                {
                    // Reward items NEED a parent id + slotId
                    reward.ParentId = parentItem.ParentId;
                    reward.SlotId = "main";
                }

                // Boxes can contain sub-items
                if (_itemHelper.IsOfBaseclass(itemTemplate.Id, BaseClasses.AMMO_BOX))
                {
                    // look for child cartridge objects
                    var childItems = messageDetails.Items?.Where(x => x.ParentId == reward.Id);
                    if (childItems is null || !childItems.Any())
                    {
                        // No cartridges found, generate and add to rewards
                        var boxAndCartridges = new List<Item>
                        {
                            reward
                        };
                        _itemHelper.AddCartridgesToAmmoBox(boxAndCartridges, itemTemplate);

                        // Push box + cartridge children into array
                        itemsToSendToPlayer.Data.AddRange(boxAndCartridges);

                        continue;
                    }

                    // Ammo box reward already has ammo, don't do anything extra
                    itemsToSendToPlayer.Data.Add(reward);
                }
                else
                {
                    if (itemTemplate.Properties.StackSlots is not null)
                    {
                        _logger.Error(_localisationService.GetText("mail-unable_to_give_gift_not_handled", itemTemplate.Id));
                    }

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

    /// <summary>
    ///     Try to find the most correct item to be the 'primary' item in a reward mail
    /// </summary>
    /// <param name="items"> Possible items to choose from </param>
    /// <returns> Chosen 'primary' item </returns>
    private Item GetBaseItemFromRewards(List<Item>? items)
    {
        // Only one item in reward, return it
        if (items?.Count == 1)
        {
            return items[0];
        }

        // Find first item with slotId that indicates its a 'base' item
        var item = items.FirstOrDefault(x => _slotNames.Contains(x.SlotId ?? ""));
        if (item is not null)
        {
            return item;
        }

        // Not a singlular item + no items have a hideout/main slotid
        // Look for first item without parent id
        item = items.FirstOrDefault(x => x.ParentId is null);
        if (item is not null)
        {
            return item;
        }

        // Just return first item in array
        return items[0];
    }

    /// <summary>
    ///     Get a dialog with a specified entity (user/trader).
    ///     Create and store empty dialog if none exists in profile.
    /// </summary>
    /// <param name="messageDetails"> Data on what message should do </param>
    /// <returns> Relevant Dialogue object </returns>
    /// <exception cref="Exception"> Thrown when message not found </exception>
    private Dialogue GetDialog(SendMessageDetails messageDetails)
    {
        var senderId = GetMessageSenderIdByType(messageDetails);
        if (senderId is null)
        {
            throw new Exception(_localisationService.GetText("mail-unable_to_find_message_sender_by_id", messageDetails.Sender));
        }

        var dialogsInProfile = _dialogueHelper.GetDialogsForProfile(messageDetails.RecipientId);

        // Does dialog exist
        if (!dialogsInProfile.ContainsKey(senderId))
            // Doesn't exist, create
        {
            dialogsInProfile[senderId] = new Dialogue
            {
                Id = senderId,
                Type = messageDetails.DialogType ?? messageDetails.Sender,
                Messages = [],
                Pinned = false,
                New = 0,
                AttachmentsNew = 0
            };
        }

        return dialogsInProfile[senderId];
    }

    /// <summary>
    ///     Get the appropriate sender id by the sender enum type
    /// </summary>
    /// <param name="messageDetails"> Data of the message </param>
    /// <returns> Gets an id of the individual sending it </returns>
    private string? GetMessageSenderIdByType(SendMessageDetails messageDetails)
    {
        if (messageDetails.Sender == MessageType.SYSTEM_MESSAGE)
        {
            return _systemSenderId;
        }

        if (messageDetails.Sender == MessageType.NPC_TRADER || messageDetails.DialogType == MessageType.NPC_TRADER)
        {
            if (messageDetails.Trader == null && _logger.IsLogEnabled(LogLevel.Debug))
            {
                _logger.Debug($"Trader was null for {messageDetails.TemplateId}");
            }

            return messageDetails.Trader;
        }

        if (messageDetails.Sender == MessageType.USER_MESSAGE)
        {
            return messageDetails.SenderDetails?.Id;
        }

        if (messageDetails.SenderDetails?.Id is not null)
        {
            return messageDetails.SenderDetails.Id;
        }

        if (messageDetails.Trader is not null)
        {
            return messageDetails.Trader;
        }

        _logger.Warning($"Unable to handle message of type: {messageDetails.Sender}");
        return null;
    }
}
