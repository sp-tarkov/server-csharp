using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Types.Models.Spt.Dialog;

public class SendMessageDetails
{
    /// <summary>
    /// Player id
    /// </summary>
    [JsonPropertyName("recipientId")]
    public string RecipientId { get; set; }

    /// <summary>
    /// Who is sending this message
    /// </summary>
    [JsonPropertyName("sender")]
    public MessageType Sender { get; set; }

    /// <summary>
    /// Optional - leave blank to use sender value
    /// </summary>
    [JsonPropertyName("dialogType")]
    public MessageType? DialogType { get; set; }

    /// <summary>
    /// Optional - if sender is USER these details are used
    /// </summary>
    [JsonPropertyName("senderDetails")]
    public IUserDialogInfo? SenderDetails { get; set; }

    /// <summary>
    /// Optional - the trader sending the message
    /// </summary>
    [JsonPropertyName("trader")]
    public Traders? Trader { get; set; }

    /// <summary>
    /// Optional - used in player/system messages, otherwise templateId is used
    /// </summary>
    [JsonPropertyName("messageText")]
    public string? MessageText { get; set; }

    /// <summary>
    /// Optional - Items to send to player
    /// </summary>
    [JsonPropertyName("items")]
    public List<IItem>? Items { get; set; }

    /// <summary>
    /// Optional - How long items will be stored in mail before expiry
    /// </summary>
    [JsonPropertyName("itemsMaxStorageLifetimeSeconds")]
    public int? ItemsMaxStorageLifetimeSeconds { get; set; }

    /// <summary>
    /// Optional - Used when sending messages from traders who send text from locale json
    /// </summary>
    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    /// <summary>
    /// Optional - ragfair related
    /// </summary>
    [JsonPropertyName("systemData")]
    public ISystemData? SystemData { get; set; }

    /// <summary>
    /// Optional - Used by ragfair messages
    /// </summary>
    [JsonPropertyName("ragfairDetails")]
    public IMessageContentRagfair? RagfairDetails { get; set; }

    /// <summary>
    /// OPTIONAL - allows modification of profile settings via mail
    /// </summary>
    [JsonPropertyName("profileChangeEvents")]
    public List<IProfileChangeEvent>? ProfileChangeEvents { get; set; }
}

public class ProfileChangeEvent
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [JsonPropertyName("Type")]
    public ProfileChangeEventType Type { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("entity")]
    public string? Entity { get; set; }
}

public struct ProfileChangeEventType
{
    public const string TRADER_SALES_SUM = "TraderSalesSum";
    public const string TRADER_STANDING = "TraderStanding";
    public const string PROFILE_LEVEL = "ProfileLevel";
    public const string SKILL_POINTS = "SkillPoints";
    public const string EXAMINE_ALL_ITEMS = "ExamineAllItems";
    public const string UNLOCK_TRADER = "UnlockTrader";
    public const string ASSORT_UNLOCK_RULE = "AssortmentUnlockRule";
    public const string HIDEOUT_AREA_LEVEL = "HideoutAreaLevel";
}