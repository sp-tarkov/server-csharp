using Core.Annotations;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Inventory;
using Core.Models.Eft.Profile;
using Core.Models.Enums;
using Core.Models.Spt.Dialog;

namespace Core.Services;

[Injectable(InjectionType.Singleton)]
public class MapMarkerService
{
    /// <summary>
    /// Add note to a map item in player inventory
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Add marker request</param>
    /// <returns>Item</returns>
    public Item CreateMarkerOnMap(PmcData pmcData, InventoryCreateMarkerRequestData request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Delete a map marker
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Delete marker request</param>
    /// <returns>Item</returns>
    public Item DeleteMarkerFromMap(PmcData pmcData, InventoryDeleteMarkerRequestData request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Edit an existing map marker
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="request">Edit marker request</param>
    /// <returns>Item</returns>
    public Item EditMarkerOnMap(PmcData pmcData, InventoryEditMarkerRequestData request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Strip out characters from note string that are not: letter/numbers/unicode/spaces
    /// </summary>
    /// <param name="mapNoteText">Marker text to sanitise</param>
    /// <returns>Sanitised map marker text</returns>
    protected string SanitiseMapMarkerText(string mapNoteText)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send a message from an NPC (e.g. prapor) to the player with or without items using direct message text, do not look up any locale
    /// </summary>
    /// <param name="sessionId">The session ID to send the message to</param>
    /// <param name="trader">The trader sending the message</param>
    /// <param name="messageType">What type the message will assume (e.g. QUEST_SUCCESS)</param>
    /// <param name="message">Text to send to the player</param>
    /// <param name="items">Optional items to send to player</param>
    /// <param name="maxStorageTimeSeconds">Optional time to collect items before they expire</param>
    public void SendDirectNpcMessageToPlayer(
        string sessionId,
        object trader,
        MessageType messageType,
        string message,
        List<Item> items = null,
        int? maxStorageTimeSeconds = null,
        SystemData systemData = null,
        MessageContentRagfair ragfair = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send a message from an NPC (e.g. prapor) to the player with or without items
    /// </summary>
    /// <param name="sessionId">The session ID to send the message to</param>
    /// <param name="trader">The trader sending the message</param>
    /// <param name="messageType">What type the message will assume (e.g. QUEST_SUCCESS)</param>
    /// <param name="messageLocaleId">The localised text to send to player</param>
    /// <param name="items">Optional items to send to player</param>
    /// <param name="maxStorageTimeSeconds">Optional time to collect items before they expire</param>
    public void SendLocalisedNpcMessageToPlayer(
        string sessionId,
        object trader,
        MessageType messageType,
        string messageLocaleId,
        List<Item> items = null,
        int? maxStorageTimeSeconds = null,
        SystemData systemData = null,
        MessageContentRagfair ragfair = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send a message from SYSTEM to the player with or without items
    /// </summary>
    /// <param name="sessionId">The session ID to send the message to</param>
    /// <param name="message">The text to send to player</param>
    /// <param name="items">Optional items to send to player</param>
    /// <param name="maxStorageTimeSeconds">Optional time to collect items before they expire</param>
    public void SendSystemMessageToPlayer(
        string sessionId,
        string message,
        List<Item> items = null,
        int? maxStorageTimeSeconds = null,
        List<ProfileChangeEvent> profileChangeEvents = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send a message from SYSTEM to the player with or without items with localised text
    /// </summary>
    /// <param name="sessionId">The session ID to send the message to</param>
    /// <param name="messageLocaleId">Id of key from locale file to send to player</param>
    /// <param name="items">Optional items to send to player</param>
    /// <param name="maxStorageTimeSeconds">Optional time to collect items before they expire</param>
    public void SendLocalisedSystemMessageToPlayer(
        string sessionId,
        string messageLocaleId,
        List<Item> items = null,
        List<ProfileChangeEvent> profileChangeEvents = null,
        int? maxStorageTimeSeconds = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send a USER message to a player with or without items
    /// </summary>
    /// <param name="sessionId">The session ID to send the message to</param>
    /// <param name="senderDetails">Who is sending the message</param>
    /// <param name="message">The text to send to player</param>
    /// <param name="items">Optional items to send to player</param>
    /// <param name="maxStorageTimeSeconds">Optional time to collect items before they expire</param>
    public void SendUserMessageToPlayer(
        string sessionId,
        UserDialogInfo senderDetails,
        string message,
        List<Item> items = null,
        int? maxStorageTimeSeconds = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Large function to send messages to players from a variety of sources (SYSTEM/NPC/USER)
    /// Helper functions in this class are available to simplify common actions
    /// </summary>
    /// <param name="messageDetails">Details needed to send a message to the player</param>
    public void SendMessageToPlayer(SendMessageDetails messageDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Send a message from the player to an NPC
    /// </summary>
    /// <param name="sessionId">Player id</param>
    /// <param name="targetNpcId">NPC message is sent to</param>
    /// <param name="message">Text to send to NPC</param>
    public void SendPlayerMessageToNpc(string sessionId, string targetNpcId, string message)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a message for storage inside a dialog in the player profile
    /// </summary>
    /// <param name="senderDialog">Id of dialog that will hold the message</param>
    /// <param name="messageDetails">Various details on what the message must contain/do</param>
    /// <returns>Message</returns>
    protected Message CreateDialogMessage(string dialogId, SendMessageDetails messageDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add items to message and adjust various properties to reflect the items being added
    /// </summary>
    /// <param name="message">Message to add items to</param>
    /// <param name="itemsToSendToPlayer">Items to add to message</param>
    /// <param name="maxStorageTimeSeconds">total time items are stored in mail before being deleted</param>
    protected void AddRewardItemsToMessage(
        Message message,
        List<MessageItems> itemsToSendToPlayer,
        int? maxStorageTimeSeconds)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// perform various sanitising actions on the items before they're considered ready for insertion into message
    /// </summary>
    /// <param name="dialogType">The type of the dialog that will hold the reward items being processed</param>
    /// <param name="messageDetails"></param>
    /// <returns>Sanitised items</returns>
    protected List<MessageItems> ProcessItemsBeforeAddingToMail(
        MessageType dialogType,
        SendMessageDetails messageDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Try to find the most correct item to be the 'primary' item in a reward mail
    /// </summary>
    /// <param name="items">Possible items to choose from</param>
    /// <returns>Chosen 'primary' item</returns>
    protected Item GetBaseItemFromRewards(List<Item> items)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get a dialog with a specified entity (user/trader)
    /// Create and store empty dialog if none exists in profile
    /// </summary>
    /// <param name="messageDetails">Data on what message should do</param>
    /// <returns>Relevant Dialogue</returns>
    protected Dialogue GetDialog(SendMessageDetails messageDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the appropriate sender id by the sender enum type
    /// </summary>
    /// <param name="messageDetails"></param>
    /// <returns>gets an id of the individual sending it</returns>
    protected string GetMessageSenderIdByType(SendMessageDetails messageDetails)
    {
        throw new NotImplementedException();
    }
}
