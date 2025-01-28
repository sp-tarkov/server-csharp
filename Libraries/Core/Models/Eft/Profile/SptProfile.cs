using Core.Models.Eft.Common;
using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;
using Core.Models.Spt.Dialog;

namespace Core.Models.Eft.Profile;

public record SptProfile
{
    [JsonPropertyName("info")]
    public Info? ProfileInfo { get; set; }

    [JsonPropertyName("characters")]
    public Characters? CharacterData { get; set; }

    /** Clothing purchases */
    [JsonPropertyName("suits")]
    public List<string>? Suits { get; set; }

    [JsonPropertyName("userbuilds")]
    public UserBuilds? UserBuildData { get; set; }

    [JsonPropertyName("dialogues")]
    public Dictionary<string, Dialogue>? DialogueRecords { get; set; }

    [JsonPropertyName("spt")]
    public Spt? SptData { get; set; }

    [JsonPropertyName("vitality")]
    public Vitality? VitalityData { get; set; }

    [JsonPropertyName("inraid")]
    public Inraid? InraidData { get; set; }

    [JsonPropertyName("insurance")]
    public List<Insurance>? InsuranceList { get; set; }

    /** Assort purchases made by player since last trader refresh */
    [JsonPropertyName("traderPurchases")]
    public Dictionary<string, Dictionary<string, TraderPurchaseData>?>? TraderPurchases { get; set; }

    /** Achievements earned by player */
    [JsonPropertyName("achievements")]
    public Dictionary<string, long>? PlayerAchievements { get; set; }

    /** List of friend profile IDs */
    [JsonPropertyName("friends")]
    public List<string>? FriendProfileIds { get; set; }

    [JsonPropertyName("customisationUnlocks")]
    public List<CustomisationStorage>? CustomisationUnlocks { get; set; }
}

public record TraderPurchaseData
{
    [JsonPropertyName("count")]
    public double? PurchaseCount { get; set; }

    [JsonPropertyName("purchaseTimestamp")]
    public long? PurchaseTimestamp { get; set; }
}

public record Info
{
    /** main profile id */
    [JsonPropertyName("id")]
    public string? ProfileId { get; set; }

    [JsonPropertyName("scavId")]
    public string? ScavengerId { get; set; }

    [JsonPropertyName("aid")]
    public int? Aid { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }

    [JsonPropertyName("wipe")]
    public bool? IsWiped { get; set; }

    [JsonPropertyName("edition")]
    public string? Edition { get; set; }
}

public record Characters
{
    [JsonPropertyName("pmc")]
    public PmcData? PmcData { get; set; }

    [JsonPropertyName("scav")]
    public PmcData? ScavData { get; set; }
}

/** used by profile.userbuilds */
public record UserBuilds
{
    [JsonPropertyName("weaponBuilds")]
    public List<WeaponBuild>? WeaponBuilds { get; set; }

    [JsonPropertyName("equipmentBuilds")]
    public List<EquipmentBuild>? EquipmentBuilds { get; set; }

    [JsonPropertyName("magazineBuilds")]
    public List<MagazineBuild>? MagazineBuilds { get; set; }
}

public record UserBuild
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}

public record WeaponBuild : UserBuild
{
    [JsonPropertyName("Root")]
    public string? Root { get; set; }

    [JsonPropertyName("Items")]
    public List<Item>? Items { get; set; } // Same as PMC inventory items
}

public record EquipmentBuild : UserBuild
{
    [JsonPropertyName("Root")]
    public string? Root { get; set; }

    [JsonPropertyName("Items")]
    public List<Item>? Items { get; set; } // Same as PMC inventory items

    [JsonPropertyName("BuildType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EquipmentBuildType? BuildType { get; set; }
}

public record MagazineBuild : UserBuild
{
    [JsonPropertyName("Caliber")]
    public string? Caliber { get; set; }

    [JsonPropertyName("TopCount")]
    public int? TopCount { get; set; }

    [JsonPropertyName("BottomCount")]
    public int? BottomCount { get; set; }

    [JsonPropertyName("Items")]
    public List<MagazineTemplateAmmoItem>? Items { get; set; }
}

public record MagazineTemplateAmmoItem
{
    [JsonPropertyName("TemplateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("Count")]
    public int? Count { get; set; }
}

/** Used by defaultEquipmentPresets.json */
public record DefaultEquipmentPreset : EquipmentBuild
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public record Dialogue
{
    [JsonPropertyName("attachmentsNew")]
    public int? AttachmentsNew { get; set; }

    [JsonPropertyName("new")]
    public int? New { get; set; }

    [JsonPropertyName("type")]
    public MessageType? Type { get; set; }

    [JsonPropertyName("Users")]
    public List<UserDialogInfo>? Users { get; set; }

    [JsonPropertyName("pinned")]
    public bool? Pinned { get; set; }

    [JsonPropertyName("messages")]
    public List<Message>? Messages { get; set; }

    [JsonPropertyName("_id")]
    public string? Id { get; set; }
}

// @Cleanup: Maybe the same as Dialogue?
public record DialogueInfo
{
    [JsonPropertyName("attachmentsNew")]
    public int? AttachmentsNew { get; set; }

    [JsonPropertyName("new")]
    public int? New { get; set; }

    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public MessageType? Type { get; set; }

    [JsonPropertyName("pinned")]
    public bool? Pinned { get; set; }

    [JsonPropertyName("Users")]
    public List<UserDialogInfo>? Users { get; set; }

    [JsonPropertyName("message")]
    public MessagePreview? Message { get; set; }
}

public record Message
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("uid")]
    public string? UserId { get; set; }

    [JsonPropertyName("type")]
    public MessageType? MessageType { get; set; }

    [JsonPropertyName("dt")]
    public long? DateTime { get; set; }

    [JsonPropertyName("UtcDateTime")]
    public long? UtcDateTime { get; set; }

    [JsonPropertyName("Member")]
    public UpdatableChatMember? Member { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("replyTo")]
    public ReplyTo? ReplyTo { get; set; }

    [JsonPropertyName("hasRewards")]
    public bool? HasRewards { get; set; }

    [JsonPropertyName("rewardCollected")]
    public bool? RewardCollected { get; set; }

    [JsonPropertyName("items")]
    public MessageItems? Items { get; set; }

    [JsonPropertyName("maxStorageTime")]
    public long? MaxStorageTime { get; set; }

    [JsonPropertyName("systemData")]
    public SystemData? SystemData { get; set; }

    [JsonPropertyName("profileChangeEvents")]
    public List<ProfileChangeEvent>? ProfileChangeEvents { get; set; }
}

public record ReplyTo
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("uid")]
    public string? UserId { get; set; }

    [JsonPropertyName("type")]
    public MessageType? MessageType { get; set; }

    [JsonPropertyName("dt")]
    public long? DateTime { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public record MessagePreview
{
    [JsonPropertyName("uid")]
    public string? UserId { get; set; }

    [JsonPropertyName("type")]
    public MessageType? MessageType { get; set; }

    [JsonPropertyName("dt")]
    public long? DateTime { get; set; }

    [JsonPropertyName("templateId")]
    public string? TemplateId { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("systemData")]
    public SystemData? SystemData { get; set; }
}

public record MessageItems
{
    [JsonPropertyName("stash")]
    public string? Stash { get; set; }

    [JsonPropertyName("data")]
    public List<Item>? Data { get; set; }
}

public record UpdatableChatMember
{
    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public int? Level { get; set; }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory { get; set; }

    [JsonPropertyName("Ignored")]
    public bool? IsIgnored { get; set; }

    [JsonPropertyName("Banned")]
    public bool? IsBanned { get; set; }
}

public record Spt
{
    /** What version of SPT was this profile made with */
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /** What mods has this profile loaded at any point in time */
    [JsonPropertyName("mods")]
    public List<ModDetails>? Mods { get; set; }

    /** What gifts has this profile received and how many */
    [JsonPropertyName("receivedGifts")]
    public List<ReceivedGift>? ReceivedGifts { get; set; }

    /** item TPLs blacklisted from being sold on flea for this profile */
    [JsonPropertyName("blacklistedItemTpls")]
    public List<string>? BlacklistedItemTemplates { get; set; }

    /** key: daily type */
    [JsonPropertyName("freeRepeatableRefreshUsedCount")]
    public Dictionary<string, int>? FreeRepeatableRefreshUsedCount { get; set; }

    /** When was a profile migrated, value is timestamp */
    [JsonPropertyName("migrations")]
    public Dictionary<string, long>? Migrations { get; set; }

    /** Cultist circle rewards received that are one time use, key (md5) is a combination of sacrificed + reward items */
    [JsonPropertyName("cultistRewards")]
    public Dictionary<string, AcceptedCultistReward>? CultistRewards { get; set; }
}

public record AcceptedCultistReward
{
    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonPropertyName("sacrificeItems")]
    public List<string>? SacrificeItems { get; set; }

    [JsonPropertyName("rewardItems")]
    public List<string>? RewardItems { get; set; }
}

public record ModDetails
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("dateAdded")]
    public long? DateAdded { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public record ReceivedGift
{
    [JsonPropertyName("giftId")]
    public string? GiftId { get; set; }

    [JsonPropertyName("timestampLastAccepted")]
    public long? TimestampLastAccepted { get; set; }

    [JsonPropertyName("current")]
    public int? Current { get; set; }
}

public record Vitality
{
    [JsonPropertyName("Hydration")]
    public double? Hydration { get; set; }
    
    [JsonPropertyName("Energy")]
    public double? Energy { get; set; }
    
    [JsonPropertyName("Temperature")]
    public double? Temperature { get; set; }
    
    [JsonPropertyName("Health")] // This now does health and effects to each bodypart
    public Dictionary<string, BodyPartHealth>? Health { get; set; }
}

public record Inraid
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("character")]
    public string? Character { get; set; }
}

public record Insurance
{
    [JsonPropertyName("scheduledTime")]
    public int? ScheduledTime { get; set; }

    [JsonPropertyName("traderId")]
    public string? TraderId { get; set; }

    [JsonPropertyName("maxStorageTime")]
    public int? MaxStorageTime { get; set; }

    [JsonPropertyName("systemData")]
    public SystemData? SystemData { get; set; }

    [JsonPropertyName("messageType")]
    public MessageType? MessageType { get; set; }

    [JsonPropertyName("messageTemplateId")]
    public string? MessageTemplateId { get; set; }

    [JsonPropertyName("items")]
    public List<Item>? Items { get; set; }
}
