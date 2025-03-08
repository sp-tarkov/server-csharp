using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Spt.Dialog;

public record SendMessageDetails
{
    /// <summary>
    ///     Player id
    /// </summary>
    [JsonPropertyName("recipientId")]
    public string? RecipientId
    {
        get;
        set;
    }

    /// <summary>
    ///     Who is sending this message
    /// </summary>
    [JsonPropertyName("sender")]
    public MessageType? Sender
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - leave blank to use sender value
    /// </summary>
    [JsonPropertyName("dialogType")]
    public MessageType? DialogType
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - if sender is USER these details are used
    /// </summary>
    [JsonPropertyName("senderDetails")]
    public UserDialogInfo? SenderDetails
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - the trader sending the message
    /// </summary>
    [JsonPropertyName("trader")]
    public string? Trader
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - used in player/system messages, otherwise templateId is used
    /// </summary>
    [JsonPropertyName("messageText")]
    public string? MessageText
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - Items to send to player
    /// </summary>
    [JsonPropertyName("items")]
    public List<Item>? Items
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - How long items will be stored in mail before expiry
    /// </summary>
    [JsonPropertyName("itemsMaxStorageLifetimeSeconds")]
    public long? ItemsMaxStorageLifetimeSeconds
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - Used when sending messages from traders who send text from locale json
    /// </summary>
    [JsonPropertyName("templateId")]
    public string? TemplateId
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - ragfair related
    /// </summary>
    [JsonPropertyName("systemData")]
    public SystemData? SystemData
    {
        get;
        set;
    }

    /// <summary>
    ///     Optional - Used by ragfair messages
    /// </summary>
    [JsonPropertyName("ragfairDetails")]
    public MessageContentRagfair? RagfairDetails
    {
        get;
        set;
    }

    /// <summary>
    ///     OPTIONAL - allows modification of profile settings via mail
    /// </summary>
    [JsonPropertyName("profileChangeEvents")]
    public List<ProfileChangeEvent>? ProfileChangeEvents
    {
        get;
        set;
    }

    /// <summary>
    /// Optional - the MongoID of the dialogue message to reply to
    /// </summary>
    [JsonPropertyName("replyTo")]
    public string? ReplyTo
    {
        get;
        set;
    }
}

public record ProfileChangeEvent
{
    [JsonPropertyName("_id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("Type")]
    public ProfileChangeEventType? Type
    {
        get;
        set;
    }

    [JsonPropertyName("value")]
    public double? Value
    {
        get;
        set;
    }

    [JsonPropertyName("entity")]
    public string? Entity
    {
        get;
        set;
    }

    [JsonPropertyName("data")]
    public string? Data
    {
        get;
        set;
    }
}

public enum ProfileChangeEventType
{
    TraderSalesSum,
    TraderStanding,
    ProfileLevel,
    SkillPoints,
    ExamineAllItems,
    UnlockTrader,
    AssortmentUnlockRule,
    HideoutAreaLevel
}
