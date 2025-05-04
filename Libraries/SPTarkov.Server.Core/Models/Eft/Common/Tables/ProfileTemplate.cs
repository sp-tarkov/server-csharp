using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ProfileTemplates
{
    [JsonPropertyName("Standard")]
    public ProfileSides? Standard { get; set; }

    [JsonPropertyName("Left Behind")]
    public ProfileSides? LeftBehind { get; set; }

    [JsonPropertyName("Prepare To Escape")]
    public ProfileSides? PrepareToEscape { get; set; }

    [JsonPropertyName("Edge Of Darkness")]
    public ProfileSides? EdgeOfDarkness { get; set; }

    [JsonPropertyName("Unheard")]
    public ProfileSides? Unheard { get; set; }

    [JsonPropertyName("Tournament")]
    public ProfileSides? Tournament { get; set; }

    [JsonPropertyName("SPT Developer")]
    public ProfileSides? SPTDeveloper { get; set; }

    [JsonPropertyName("SPT Easy start")]
    public ProfileSides? SPTEasyStart { get; set; }

    [JsonPropertyName("SPT Zero to hero")]
    public ProfileSides? SPTZeroToHero { get; set; }
}

public record ProfileSides
{
    [JsonPropertyName("descriptionLocaleKey")]
    public string? DescriptionLocaleKey { get; set; }

    [JsonPropertyName("usec")]
    public TemplateSide? Usec { get; set; }

    [JsonPropertyName("bear")]
    public TemplateSide? Bear { get; set; }
}

public record TemplateSide
{
    [JsonPropertyName("character")]
    public PmcData? Character { get; set; }

    [JsonPropertyName("suits")]
    public List<string>? Suits { get; set; }

    [JsonPropertyName("dialogues")]
    public Dictionary<string, Dialogue>? Dialogues { get; set; }

    [JsonPropertyName("userbuilds")]
    public UserBuilds? UserBuilds { get; set; }

    [JsonPropertyName("trader")]
    public ProfileTraderTemplate? Trader { get; set; }

    [JsonPropertyName("equipmentBuilds")]
    public object? EquipmentBuilds { get; set; }

    [JsonPropertyName("weaponbuilds")]
    public object? WeaponBuilds { get; set; }
}

public record ProfileTraderTemplate
{
    [JsonPropertyName("initialLoyaltyLevel")]
    public Dictionary<string, int?>? InitialLoyaltyLevel { get; set; }

    [JsonPropertyName("initialStanding")]
    public Dictionary<string, double?>? InitialStanding { get; set; }

    [JsonPropertyName("setQuestsAvailableForStart")]
    public bool? SetQuestsAvailableForStart { get; set; }

    [JsonPropertyName("setQuestsAvailableForFinish")]
    public bool? SetQuestsAvailableForFinish { get; set; }

    [JsonPropertyName("initialSalesSum")]
    public int? InitialSalesSum { get; set; }

    [JsonPropertyName("jaegerUnlocked")]
    public bool? JaegerUnlocked { get; set; }

    /// <summary>
    ///     How many days is usage of the flea blocked for upon profile creation
    /// </summary>
    [JsonPropertyName("fleaBlockedDays")]
    public int? FleaBlockedDays { get; set; }

    /// <summary>
    ///     What traders default to being locked on profile creation
    /// </summary>
    [JsonPropertyName("lockedByDefaultOverride")]
    public List<string>? LockedByDefaultOverride { get; set; }

    /// <summary>
    ///     What traders should have their clothing unlocked/purchased on creation
    /// </summary>
    [JsonPropertyName("purchaseAllClothingByDefaultForTrader")]
    public List<string>? PurchaseAllClothingByDefaultForTrader { get; set; }
}
