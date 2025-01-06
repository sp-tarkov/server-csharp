using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class Gift
{
    /// <summary>
    /// Items to send to player
    /// </summary>
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }

    /// <summary>
    /// Who is sending the gift to player
    /// </summary>
    [JsonPropertyName("sender")]
    public GiftSenderType Sender { get; set; }

    /// <summary>
    /// Optional - supply a users id to send from, not necessary when sending from SYSTEM or TRADER
    /// </summary>
    [JsonPropertyName("senderId")]
    public string? SenderId { get; set; }

    [JsonPropertyName("senderDetails")]
    public UserDialogInfo SenderDetails { get; set; }

    /// <summary>
    /// Optional - supply a trader type to send from, not necessary when sending from SYSTEM or USER
    /// </summary>
    [JsonPropertyName("trader")]
    public Traders? Trader { get; set; }

    [JsonPropertyName("messageText")]
    public string MessageText { get; set; }

    /// <summary>
    /// Optional - if sending text from the client locale file
    /// </summary>
    [JsonPropertyName("localeTextId")]
    public string? LocaleTextId { get; set; }

    /// <summary>
    /// Optional - Used by Seasonal events to send on specific day
    /// </summary>
    [JsonPropertyName("timestampToSend")]
    public long? TimestampToSend { get; set; }

    [JsonPropertyName("associatedEvent")]
    public SeasonalEventType AssociatedEvent { get; set; }

    [JsonPropertyName("collectionTimeHours")]
    public int CollectionTimeHours { get; set; }

    /// <summary>
    /// Optional, can be used to change profile settings like level/skills
    /// </summary>
    [JsonPropertyName("profileChangeEvents")]
    public List<ProfileChangeEvent>? ProfileChangeEvents { get; set; }

    [JsonPropertyName("maxToSendPlayer")]
    public int? MaxToSendPlayer { get; set; }
}