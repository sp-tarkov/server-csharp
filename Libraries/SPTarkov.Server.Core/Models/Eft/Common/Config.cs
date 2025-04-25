using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Config
{
    [JsonPropertyName("ArtilleryShelling")]
    public ArtilleryShelling? ArtilleryShelling
    {
        get;
        set;
    }

    [JsonPropertyName("AudioSettings")]
    public GlobalAudioSettings? AudioSettings
    {
        get;
        set;
    }

    [JsonPropertyName("content")]
    public Content? Content
    {
        get;
        set;
    }

    [JsonPropertyName("AimPunchMagnitude")]
    public double? AimPunchMagnitude
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponSkillProgressRate")]
    public double? WeaponSkillProgressRate
    {
        get;
        set;
    }

    [JsonPropertyName("SkillAtrophy")]
    public bool? SkillAtrophy
    {
        get;
        set;
    }

    [JsonPropertyName("exp")]
    public Exp? Exp
    {
        get;
        set;
    }

    [JsonPropertyName("t_base_looting")]
    public double? TBaseLooting
    {
        get;
        set;
    }

    [JsonPropertyName("t_base_lockpicking")]
    public double? TBaseLockpicking
    {
        get;
        set;
    }

    [JsonPropertyName("armor")]
    public Armor? Armor
    {
        get;
        set;
    }

    [JsonPropertyName("SessionsToShowHotKeys")]
    public double? SessionsToShowHotKeys
    {
        get;
        set;
    }

    [JsonPropertyName("MaxBotsAliveOnMap")]
    public double? MaxBotsAliveOnMap
    {
        get;
        set;
    }

    [JsonPropertyName("MaxBotsAliveOnMapPvE")]
    public double? MaxBotsAliveOnMapPvE
    {
        get;
        set;
    }

    [JsonPropertyName("RunddansSettings")]
    public RunddansSettings? RunddansSettings
    {
        get;
        set;
    }

    // Checked in client
    [JsonPropertyName("SavagePlayCooldown")]
    public int? SavagePlayCooldown
    {
        get;
        set;
    }

    [JsonPropertyName("SavagePlayCooldownNdaFree")]
    public double? SavagePlayCooldownNdaFree
    {
        get;
        set;
    }

    [JsonPropertyName("SeasonActivity")]
    public SeasonActivity? SeasonActivity
    {
        get;
        set;
    }

    [JsonPropertyName("MarksmanAccuracy")]
    public double? MarksmanAccuracy
    {
        get;
        set;
    }

    [JsonPropertyName("SavagePlayCooldownDevelop")]
    public double? SavagePlayCooldownDevelop
    {
        get;
        set;
    }

    [JsonPropertyName("TODSkyDate")]
    public string? TODSkyDate
    {
        get;
        set;
    }

    [JsonPropertyName("Mastering")]
    public Mastering[] Mastering
    {
        get;
        set;
    }

    [JsonPropertyName("GlobalItemPriceModifier")]
    public double? GlobalItemPriceModifier
    {
        get;
        set;
    }

    [JsonPropertyName("TradingUnlimitedItems")]
    public bool? TradingUnlimitedItems
    {
        get;
        set;
    }

    [JsonPropertyName("TradingUnsetPersonalLimitItems")]
    public bool? TradingUnsetPersonalLimitItems
    {
        get;
        set;
    }

    [JsonPropertyName("TransitSettings")]
    public TransitSettings? TransitSettings
    {
        get;
        set;
    }

    [JsonPropertyName("Triggers")]
    public Triggers? Triggers
    {
        get;
        set;
    }

    [JsonPropertyName("TripwiresSettings")]
    public TripwiresSettings? TripwiresSettings
    {
        get;
        set;
    }

    [JsonPropertyName("MaxLoyaltyLevelForAll")]
    public bool? MaxLoyaltyLevelForAll
    {
        get;
        set;
    }

    [JsonPropertyName("MountingSettings")]
    public MountingSettings? MountingSettings
    {
        get;
        set;
    }

    [JsonPropertyName("GlobalLootChanceModifier")]
    public double? GlobalLootChanceModifier
    {
        get;
        set;
    }

    [JsonPropertyName("GlobalLootChanceModifierPvE")]
    public double? GlobalLootChanceModifierPvE
    {
        get;
        set;
    }

    [JsonPropertyName("GraphicSettings")]
    public GraphicSettings? GraphicSettings
    {
        get;
        set;
    }

    [JsonPropertyName("TimeBeforeDeploy")]
    public double? TimeBeforeDeploy
    {
        get;
        set;
    }

    [JsonPropertyName("TimeBeforeDeployLocal")]
    public double? TimeBeforeDeployLocal
    {
        get;
        set;
    }

    [JsonPropertyName("TradingSetting")]
    public double? TradingSetting
    {
        get;
        set;
    }

    [JsonPropertyName("TradingSettings")]
    public TradingSettings? TradingSettings
    {
        get;
        set;
    }

    [JsonPropertyName("ItemsCommonSettings")]
    public ItemsCommonSettings? ItemsCommonSettings
    {
        get;
        set;
    }

    [JsonPropertyName("LoadTimeSpeedProgress")]
    public double? LoadTimeSpeedProgress
    {
        get;
        set;
    }

    [JsonPropertyName("BaseLoadTime")]
    public double? BaseLoadTime
    {
        get;
        set;
    }

    [JsonPropertyName("BaseUnloadTime")]
    public double? BaseUnloadTime
    {
        get;
        set;
    }

    [JsonPropertyName("BaseCheckTime")]
    public double? BaseCheckTime
    {
        get;
        set;
    }

    [JsonPropertyName("BluntDamageReduceFromSoftArmorMod")]
    public double? BluntDamageReduceFromSoftArmorMod
    {
        get;
        set;
    }

    [JsonPropertyName("BodyPartColliderSettings")]
    public BodyPartColliderSettings? BodyPartColliderSettings
    {
        get;
        set;
    }

    [JsonPropertyName("Customization")]
    public Customization? Customization
    {
        get;
        set;
    }

    [JsonPropertyName("UncheckOnShot")]
    public bool? UncheckOnShot
    {
        get;
        set;
    }

    [JsonPropertyName("BotsEnabled")]
    public bool? BotsEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("BufferZone")]
    public BufferZone? BufferZone
    {
        get;
        set;
    }

    [JsonPropertyName("Airdrop")]
    public AirdropGlobalSettings? Airdrop
    {
        get;
        set;
    }

    [JsonPropertyName("ArmorMaterials")]
    public Dictionary<ArmorMaterial, ArmorType>? ArmorMaterials
    {
        get;
        set;
    }

    [JsonPropertyName("ArenaEftTransferSettings")]
    public ArenaEftTransferSettings
        ArenaEftTransferSettings
    {
        get;
        set;
    } // TODO: this needs to be looked into, there are two types further down commented out with the same name

    [JsonPropertyName("KarmaCalculationSettings")]
    public KarmaCalculationSettings? KarmaCalculationSettings
    {
        get;
        set;
    }

    [JsonPropertyName("LegsOverdamage")]
    public double? LegsOverdamage
    {
        get;
        set;
    }

    [JsonPropertyName("HandsOverdamage")]
    public double? HandsOverdamage
    {
        get;
        set;
    }

    [JsonPropertyName("StomachOverdamage")]
    public double? StomachOverdamage
    {
        get;
        set;
    }

    [JsonPropertyName("Health")]
    public Health? Health
    {
        get;
        set;
    }

    [JsonPropertyName("rating")]
    public Rating? Rating
    {
        get;
        set;
    }

    [JsonPropertyName("tournament")]
    public Tournament? Tournament
    {
        get;
        set;
    }

    [JsonPropertyName("QuestSettings")]
    public QuestSettings? QuestSettings
    {
        get;
        set;
    }

    [JsonPropertyName("RagFair")]
    public RagFair? RagFair
    {
        get;
        set;
    }

    [JsonPropertyName("handbook")]
    public Handbook? Handbook
    {
        get;
        set;
    }

    [JsonPropertyName("FractureCausedByFalling")]
    public Probability? FractureCausedByFalling
    {
        get;
        set;
    }

    [JsonPropertyName("FractureCausedByBulletHit")]
    public Probability? FractureCausedByBulletHit
    {
        get;
        set;
    }

    [JsonPropertyName("WAVE_COEF_LOW")]
    public double? WaveCoefficientLow
    {
        get;
        set;
    }

    [JsonPropertyName("WAVE_COEF_MID")]
    public double? WaveCoefficientMid
    {
        get;
        set;
    }

    [JsonPropertyName("WAVE_COEF_HIGH")]
    public double? WaveCoefficientHigh
    {
        get;
        set;
    }

    [JsonPropertyName("WAVE_COEF_HORDE")]
    public double? WaveCoefficientHorde
    {
        get;
        set;
    }

    [JsonPropertyName("Stamina")]
    public Stamina? Stamina
    {
        get;
        set;
    }

    [JsonPropertyName("StaminaRestoration")]
    public StaminaRestoration? StaminaRestoration
    {
        get;
        set;
    }

    [JsonPropertyName("StaminaDrain")]
    public StaminaDrain? StaminaDrain
    {
        get;
        set;
    }

    [JsonPropertyName("RequirementReferences")]
    public RequirementReferences? RequirementReferences
    {
        get;
        set;
    }

    [JsonPropertyName("RestrictionsInRaid")]
    public RestrictionsInRaid[] RestrictionsInRaid
    {
        get;
        set;
    }

    [JsonPropertyName("SkillMinEffectiveness")]
    public double? SkillMinEffectiveness
    {
        get;
        set;
    }

    [JsonPropertyName("SkillFatiguePerPoint")]
    public double? SkillFatiguePerPoint
    {
        get;
        set;
    }

    [JsonPropertyName("SkillFreshEffectiveness")]
    public double? SkillFreshEffectiveness
    {
        get;
        set;
    }

    [JsonPropertyName("SkillFreshPoints")]
    public double? SkillFreshPoints
    {
        get;
        set;
    }

    [JsonPropertyName("SkillPointsBeforeFatigue")]
    public double? SkillPointsBeforeFatigue
    {
        get;
        set;
    }

    [JsonPropertyName("SkillFatigueReset")]
    public double? SkillFatigueReset
    {
        get;
        set;
    }

    [JsonPropertyName("DiscardLimitsEnabled")]
    public bool? DiscardLimitsEnabled
    {
        get;
        set;
    }

    [JsonPropertyName("EnvironmentSettings")]
    public EnvironmentUISettings? EnvironmentSettings
    {
        get;
        set;
    }

    [JsonPropertyName("EventSettings")]
    public EventSettings? EventSettings
    {
        get;
        set;
    }

    [JsonPropertyName("FavoriteItemsSettings")]
    public FavoriteItemsSettings? FavoriteItemsSettings
    {
        get;
        set;
    }

    [JsonPropertyName("VaultingSettings")]
    public VaultingSettings? VaultingSettings
    {
        get;
        set;
    }

    [JsonPropertyName("BTRSettings")]
    public BTRSettings? BTRSettings
    {
        get;
        set;
    }

    [JsonPropertyName("EventType")]
    public List<EventType> EventType
    {
        get;
        set;
    }

    [JsonPropertyName("WalkSpeed")]
    public XYZ? WalkSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("SprintSpeed")]
    public XYZ? SprintSpeed
    {
        get;
        set;
    }

    [JsonPropertyName("SquadSettings")]
    public SquadSettings? SquadSettings
    {
        get;
        set;
    }

    [JsonPropertyName("SkillEnduranceWeightThreshold")]
    public double? SkillEnduranceWeightThreshold
    {
        get;
        set;
    }

    [JsonPropertyName("TeamSearchingTimeout")]
    public double? TeamSearchingTimeout
    {
        get;
        set;
    }

    [JsonPropertyName("Insurance")]
    public Insurance? Insurance
    {
        get;
        set;
    }

    [JsonPropertyName("SkillExpPerLevel")]
    public double? SkillExpPerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("GameSearchingTimeout")]
    public double? GameSearchingTimeout
    {
        get;
        set;
    }

    [JsonPropertyName("WallContusionAbsorption")]
    public XYZ? WallContusionAbsorption
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponFastDrawSettings")]
    public WeaponFastDrawSettings? WeaponFastDrawSettings
    {
        get;
        set;
    }

    [JsonPropertyName("SkillsSettings")]
    public SkillsSettings? SkillsSettings
    {
        get;
        set;
    }

    [JsonPropertyName("AzimuthPanelShowsPlayerOrientation")]
    public bool? AzimuthPanelShowsPlayerOrientation
    {
        get;
        set;
    }

    [JsonPropertyName("Aiming")]
    public Aiming? Aiming
    {
        get;
        set;
    }

    [JsonPropertyName("Malfunction")]
    public Malfunction? Malfunction
    {
        get;
        set;
    }

    [JsonPropertyName("Overheat")]
    public Overheat? Overheat
    {
        get;
        set;
    }

    [JsonPropertyName("FenceSettings")]
    public FenceSettings? FenceSettings
    {
        get;
        set;
    }

    [JsonPropertyName("TestValue")]
    public double? TestValue
    {
        get;
        set;
    }

    [JsonPropertyName("Inertia")]
    public Inertia? Inertia
    {
        get;
        set;
    }

    [JsonPropertyName("Ballistic")]
    public Ballistic? Ballistic
    {
        get;
        set;
    }

    [JsonPropertyName("RepairSettings")]
    public RepairSettings? RepairSettings
    {
        get;
        set;
    }

    public CoopSettings? CoopSettings
    {
        get;
        set;
    }

    public PveSettings? PveSettings
    {
        get;
        set;
    }
}
