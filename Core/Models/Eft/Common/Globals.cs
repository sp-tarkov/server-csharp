using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Hideout;

namespace Core.Models.Eft.Common;

using System.Text.Json.Serialization;

public class Globals
{
    [JsonPropertyName("time")]
    public double? Time { get; set; }

    [JsonPropertyName("config")]
    public Config? Configuration { get; set; }

    [JsonPropertyName("LocationInfection")]
    public LocationInfection? LocationInfection { get; set; }

    [JsonPropertyName("bot_presets")]
    public List<BotPreset>? BotPresets { get; set; }

    [JsonPropertyName("BotWeaponScatterings")]
    public List<BotWeaponScattering>? BotWeaponScatterings { get; set; }

    [JsonPropertyName("ItemPresets")]
    public Dictionary<string, Preset>? ItemPresets { get; set; }
}

public class PlayerSettings
{
    [JsonPropertyName("BaseMaxMovementRolloff")]
    public double? BaseMaxMovementRolloff { get; set; }

    [JsonPropertyName("EnabledOcclusionDynamicRolloff")]
    public bool? IsEnabledOcclusionDynamicRolloff { get; set; }

    [JsonPropertyName("IndoorRolloffMult")]
    public double? IndoorRolloffMultiplier { get; set; }

    [JsonPropertyName("MinStepSoundRolloffMult")]
    public double? MinStepSoundRolloffMultiplier { get; set; }

    [JsonPropertyName("MinStepSoundVolumeMult")]
    public double? MinStepSoundVolumeMultiplier { get; set; }

    [JsonPropertyName("MovementRolloffMultipliers")]
    public List<MovementRolloffMultiplier>? MovementRolloffMultipliers { get; set; }

    [JsonPropertyName("OutdoorRolloffMult")]
    public double? OutdoorRolloffMultiplier { get; set; }
}

public class MovementRolloffMultiplier
{
    [JsonPropertyName("MovementState")]
    public string? MovementState { get; set; }

    [JsonPropertyName("RolloffMultiplier")]
    public double? RolloffMultiplier { get; set; }
}

public class RadioBroadcastSettings
{
    [JsonPropertyName("EnabledBroadcast")]
    public bool? EnabledBroadcast { get; set; }

    [JsonPropertyName("RadioStations")]
    public List<RadioStation>? RadioStations { get; set; }
}

public class RadioStation
{
    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("Station")]
    public string? Station { get; set; }
}

public class LocationInfection
{
    [JsonPropertyName("Interchange")]
    public double? Interchange { get; set; }

    [JsonPropertyName("Lighthouse")]
    public double? Lighthouse { get; set; }

    [JsonPropertyName("RezervBase")]
    public double? RezervBase { get; set; }

    [JsonPropertyName("Sandbox")]
    public double? Sandbox { get; set; }

    [JsonPropertyName("Shoreline")]
    public double? Shoreline { get; set; }

    [JsonPropertyName("TarkovStreets")]
    public double? TarkovStreets { get; set; }

    [JsonPropertyName("Woods")]
    public double? Woods { get; set; }

    [JsonPropertyName("bigmap")]
    public double? BigMap { get; set; }

    [JsonPropertyName("factory4")]
    public double? Factory4 { get; set; }

    [JsonPropertyName("laboratory")]
    public double? Laboratory { get; set; }
}

public class ArtilleryShelling
{
    [JsonPropertyName("ArtilleryMapsConfigs")]
    public Dictionary<string, ArtilleryMapSettings>? ArtilleryMapsConfigs { get; set; }

    [JsonPropertyName("ProjectileExplosionParams")]
    public ProjectileExplosionParams? ProjectileExplosionParams { get; set; }

    [JsonPropertyName("MaxCalledShellingCount")]
    public double? MaxCalledShellingCount { get; set; }
}

public class ArtilleryMapSettings
{
    [JsonPropertyName("PlanedShellingOn")]
    public bool? PlanedShellingOn { get; set; }

    [JsonPropertyName("InitShellingTimer")]
    public double? InitShellingTimer { get; set; }

    [JsonPropertyName("BeforeShellingSignalTime")]
    public double? BeforeShellingSignalTime { get; set; }

    [JsonPropertyName("ShellingCount")]
    public double? ShellingCount { get; set; }

    [JsonPropertyName("ZonesInShelling")]
    public double? ZonesInShelling { get; set; }

    [JsonPropertyName("NewZonesForEachShelling")]
    public bool? NewZonesForEachShelling { get; set; }

    [JsonPropertyName("InitCalledShellingTime")]
    public double? InitCalledShellingTime { get; set; }

    [JsonPropertyName("ShellingZones")]
    public List<ShellingZone>? ShellingZones { get; set; }

    [JsonPropertyName("Brigades")]
    public List<Brigade>? Brigades { get; set; }

    [JsonPropertyName("ArtilleryShellingAirDropSettings")]
    public ArtilleryShellingAirDropSettings? ArtilleryShellingAirDropSettings { get; set; }

    [JsonPropertyName("PauseBetweenShellings")]
    public XYZ? PauseBetweenShellings { get; set; }
}

public class ShellingZone
{
    [JsonPropertyName("ID")]
    public double? ID { get; set; }

    [JsonPropertyName("PointsInShellings")]
    public XYZ? PointsInShellings { get; set; }

    [JsonPropertyName("ShellingRounds")]
    public double? ShellingRounds { get; set; }

    [JsonPropertyName("ShotCount")]
    public double? ShotCount { get; set; }

    [JsonPropertyName("PauseBetweenRounds")]
    public XYZ? PauseBetweenRounds { get; set; }

    [JsonPropertyName("PauseBetweenShots")]
    public XYZ? PauseBetweenShots { get; set; }

    [JsonPropertyName("Center")]
    public XYZ? Center { get; set; }

    [JsonPropertyName("Rotate")]
    public double? Rotate { get; set; }

    [JsonPropertyName("GridStep")]
    public XYZ? GridStep { get; set; }

    [JsonPropertyName("Points")]
    public XYZ? Points { get; set; }

    [JsonPropertyName("PointRadius")]
    public double? PointRadius { get; set; }

    [JsonPropertyName("ExplosionDistanceRange")]
    public XYZ? ExplosionDistanceRange { get; set; }

    [JsonPropertyName("AlarmStages")]
    public List<AlarmStage>? AlarmStages { get; set; }

    [JsonPropertyName("BeforeShellingSignalTime")]
    public double? BeforeShellingSignalTime { get; set; }

    [JsonPropertyName("UsedInPlanedShelling")]
    public bool? UsedInPlanedShelling { get; set; }

    [JsonPropertyName("UseInCalledShelling")]
    public bool? UseInCalledShelling { get; set; }

    [JsonPropertyName("IsActive")]
    public bool? IsActive { get; set; }
}

public class AlarmStage
{
    [JsonPropertyName("Value")]
    public Position? Value { get; set; }
}

public class Brigade
{
    [JsonPropertyName("ID")]
    public double? Id { get; set; }

    [JsonPropertyName("ArtilleryGuns")]
    public List<ArtilleryGun>? ArtilleryGuns { get; set; }
}

public class ArtilleryGun
{
    [JsonPropertyName("Position")]
    public XYZ? Position { get; set; }
}

public class ArtilleryShellingAirDropSettings
{
    [JsonPropertyName("UseAirDrop")]
    public bool? UseAirDrop { get; set; }

    [JsonPropertyName("AirDropTime")]
    public double? AirDropTime { get; set; }

    [JsonPropertyName("AirDropPosition")]
    public XYZ? AirDropPosition { get; set; }

    [JsonPropertyName("LootTemplateId")]
    public string? LootTemplateId { get; set; }
}

public class ProjectileExplosionParams
{
    [JsonPropertyName("Blindness")]
    public XYZ? Blindness { get; set; }

    [JsonPropertyName("Contusion")]
    public XYZ? Contusion { get; set; }

    [JsonPropertyName("ArmorDistanceDistanceDamage")]
    public XYZ? ArmorDistanceDistanceDamage { get; set; }

    [JsonPropertyName("MinExplosionDistance")]
    public float? MinExplosionDistance { get; set; }

    [JsonPropertyName("MaxExplosionDistance")]
    public float? MaxExplosionDistance { get; set; }

    [JsonPropertyName("FragmentsCount")]
    public double? FragmentsCount { get; set; }

    [JsonPropertyName("Strength")]
    public float? Strength { get; set; }

    [JsonPropertyName("ArmorDamage")]
    public float? ArmorDamage { get; set; }

    [JsonPropertyName("StaminaBurnRate")]
    public float? StaminaBurnRate { get; set; }

    [JsonPropertyName("PenetrationPower")]
    public float? PenetrationPower { get; set; }

    [JsonPropertyName("DirectionalDamageAngle")]
    public float? DirectionalDamageAngle { get; set; }

    [JsonPropertyName("DirectionalDamageMultiplier")]
    public float? DirectionalDamageMultiplier { get; set; }

    [JsonPropertyName("FragmentType")]
    public string? FragmentType { get; set; }

    [JsonPropertyName("DeadlyDistance")]
    public float? DeadlyDistance { get; set; }
}

public class Config
{
    [JsonPropertyName("ArtilleryShelling")]
    public ArtilleryShelling? ArtilleryShelling { get; set; }

    [JsonPropertyName("content")]
    public Content? Content { get; set; }

    [JsonPropertyName("AimPunchMagnitude")]
    public double? AimPunchMagnitude { get; set; }

    [JsonPropertyName("WeaponSkillProgressRate")]
    public double? WeaponSkillProgressRate { get; set; }

    [JsonPropertyName("SkillAtrophy")]
    public bool? SkillAtrophy { get; set; }

    [JsonPropertyName("exp")]
    public Exp? Exp { get; set; }

    [JsonPropertyName("t_base_looting")]
    public double? TBaseLooting { get; set; }

    [JsonPropertyName("t_base_lockpicking")]
    public double? TBaseLockpicking { get; set; }

    [JsonPropertyName("armor")]
    public Armor? Armor { get; set; }

    [JsonPropertyName("SessionsToShowHotKeys")]
    public double? SessionsToShowHotKeys { get; set; }

    [JsonPropertyName("MaxBotsAliveOnMap")]
    public double? MaxBotsAliveOnMap { get; set; }

    [JsonPropertyName("MaxBotsAliveOnMapPvE")]
    public double? MaxBotsAliveOnMapPvE { get; set; }

    [JsonPropertyName("RunddansSettings")]
    public RunddansSettings? RunddansSettings { get; set; }

    [JsonPropertyName("SavagePlayCooldown")]
    public double? SavagePlayCooldown { get; set; }

    [JsonPropertyName("SavagePlayCooldownNdaFree")]
    public double? SavagePlayCooldownNdaFree { get; set; }

    [JsonPropertyName("SeasonActivity")]
    public SeasonActivity? SeasonActivity { get; set; }

    [JsonPropertyName("MarksmanAccuracy")]
    public double? MarksmanAccuracy { get; set; }

    [JsonPropertyName("SavagePlayCooldownDevelop")]
    public double? SavagePlayCooldownDevelop { get; set; }

    [JsonPropertyName("TODSkyDate")]
    public string? TODSkyDate { get; set; }

    [JsonPropertyName("Mastering")]
    public Mastering[] Mastering { get; set; }

    [JsonPropertyName("GlobalItemPriceModifier")]
    public double? GlobalItemPriceModifier { get; set; }

    [JsonPropertyName("TradingUnlimitedItems")]
    public bool? TradingUnlimitedItems { get; set; }

    [JsonPropertyName("TradingUnsetPersonalLimitItems")]
    public bool? TradingUnsetPersonalLimitItems { get; set; }

    [JsonPropertyName("TransitSettings")]
    public TransitSettings? TransitSettings { get; set; }

    [JsonPropertyName("TripwiresSettings")]
    public TripwiresSettings? TripwiresSettings { get; set; }

    [JsonPropertyName("MaxLoyaltyLevelForAll")]
    public bool? MaxLoyaltyLevelForAll { get; set; }

    [JsonPropertyName("MountingSettings")]
    public MountingSettings? MountingSettings { get; set; }

    [JsonPropertyName("GlobalLootChanceModifier")]
    public double? GlobalLootChanceModifier { get; set; }

    [JsonPropertyName("GlobalLootChanceModifierPvE")]
    public double? GlobalLootChanceModifierPvE { get; set; }

    [JsonPropertyName("GraphicSettings")]
    public GraphicSettings? GraphicSettings { get; set; }

    [JsonPropertyName("TimeBeforeDeploy")]
    public double? TimeBeforeDeploy { get; set; }

    [JsonPropertyName("TimeBeforeDeployLocal")]
    public double? TimeBeforeDeployLocal { get; set; }

    [JsonPropertyName("TradingSetting")]
    public double? TradingSetting { get; set; }

    [JsonPropertyName("TradingSettings")]
    public TradingSettings? TradingSettings { get; set; }

    [JsonPropertyName("ItemsCommonSettings")]
    public ItemsCommonSettings? ItemsCommonSettings { get; set; }

    [JsonPropertyName("LoadTimeSpeedProgress")]
    public double? LoadTimeSpeedProgress { get; set; }

    [JsonPropertyName("BaseLoadTime")]
    public double? BaseLoadTime { get; set; }

    [JsonPropertyName("BaseUnloadTime")]
    public double? BaseUnloadTime { get; set; }

    [JsonPropertyName("BaseCheckTime")]
    public double? BaseCheckTime { get; set; }

    [JsonPropertyName("BluntDamageReduceFromSoftArmorMod")]
    public double? BluntDamageReduceFromSoftArmorMod { get; set; }

    [JsonPropertyName("BodyPartColliderSettings")]
    public BodyPartColliderSettings? BodyPartColliderSettings { get; set; }

    [JsonPropertyName("Customization")]
    public Customization? Customization { get; set; }

    [JsonPropertyName("UncheckOnShot")]
    public bool? UncheckOnShot { get; set; }

    [JsonPropertyName("BotsEnabled")]
    public bool? BotsEnabled { get; set; }

    [JsonPropertyName("BufferZone")]
    public BufferZone? BufferZone { get; set; }

    [JsonPropertyName("Airdrop")]
    public AirdropGlobalSettings? Airdrop { get; set; }

    [JsonPropertyName("ArmorMaterials")]
    public ArmorMaterials? ArmorMaterials { get; set; }

    [JsonPropertyName("ArenaEftTransferSettings")]
    public ArenaEftTransferSettings
        ArenaEftTransferSettings { get; set; } // TODO: this needs to be looked into, there are two types further down commented out with the same name

    [JsonPropertyName("KarmaCalculationSettings")]
    public KarmaCalculationSettings? KarmaCalculationSettings { get; set; }

    [JsonPropertyName("LegsOverdamage")]
    public double? LegsOverdamage { get; set; }

    [JsonPropertyName("HandsOverdamage")]
    public double? HandsOverdamage { get; set; }

    [JsonPropertyName("StomachOverdamage")]
    public double? StomachOverdamage { get; set; }

    [JsonPropertyName("Health")]
    public Health? Health { get; set; }

    [JsonPropertyName("rating")]
    public Rating? Rating { get; set; }

    [JsonPropertyName("tournament")]
    public Tournament? Tournament { get; set; }

    [JsonPropertyName("QuestSettings")]
    public QuestSettings? QuestSettings { get; set; }

    [JsonPropertyName("RagFair")]
    public RagFair? RagFair { get; set; }

    [JsonPropertyName("handbook")]
    public Handbook? Handbook { get; set; }

    [JsonPropertyName("FractureCausedByFalling")]
    public Probability? FractureCausedByFalling { get; set; }

    [JsonPropertyName("FractureCausedByBulletHit")]
    public Probability? FractureCausedByBulletHit { get; set; }

    [JsonPropertyName("WAVE_COEF_LOW")]
    public double? WaveCoefficientLow { get; set; }

    [JsonPropertyName("WAVE_COEF_MID")]
    public double? WaveCoefficientMid { get; set; }

    [JsonPropertyName("WAVE_COEF_HIGH")]
    public double? WaveCoefficientHigh { get; set; }

    [JsonPropertyName("WAVE_COEF_HORDE")]
    public double? WaveCoefficientHorde { get; set; }

    [JsonPropertyName("Stamina")]
    public Stamina? Stamina { get; set; }

    [JsonPropertyName("StaminaRestoration")]
    public StaminaRestoration? StaminaRestoration { get; set; }

    [JsonPropertyName("StaminaDrain")]
    public StaminaDrain? StaminaDrain { get; set; }

    [JsonPropertyName("RequirementReferences")]
    public RequirementReferences? RequirementReferences { get; set; }

    [JsonPropertyName("RestrictionsInRaid")]
    public RestrictionsInRaid[] RestrictionsInRaid { get; set; }

    [JsonPropertyName("SkillMinEffectiveness")]
    public double? SkillMinEffectiveness { get; set; }

    [JsonPropertyName("SkillFatiguePerPoint")]
    public double? SkillFatiguePerPoint { get; set; }

    [JsonPropertyName("SkillFreshEffectiveness")]
    public double? SkillFreshEffectiveness { get; set; }

    [JsonPropertyName("SkillFreshPoints")]
    public double? SkillFreshPoints { get; set; }

    [JsonPropertyName("SkillPointsBeforeFatigue")]
    public double? SkillPointsBeforeFatigue { get; set; }

    [JsonPropertyName("SkillFatigueReset")]
    public double? SkillFatigueReset { get; set; }

    [JsonPropertyName("DiscardLimitsEnabled")]
    public bool? DiscardLimitsEnabled { get; set; }

    [JsonPropertyName("EnvironmentSettings")]
    public EnvironmentSetting2? EnvironmentSettings { get; set; }

    [JsonPropertyName("EventSettings")]
    public EventSettings? EventSettings { get; set; }

    [JsonPropertyName("FavoriteItemsSettings")]
    public FavoriteItemsSettings? FavoriteItemsSettings { get; set; }

    [JsonPropertyName("VaultingSettings")]
    public VaultingSettings? VaultingSettings { get; set; }

    [JsonPropertyName("BTRSettings")]
    public BTRSettings? BTRSettings { get; set; }

    [JsonPropertyName("EventType")]
    public List<string> EventType { get; set; }

    [JsonPropertyName("WalkSpeed")]
    public XYZ? WalkSpeed { get; set; }

    [JsonPropertyName("SprintSpeed")]
    public XYZ? SprintSpeed { get; set; }

    [JsonPropertyName("SquadSettings")]
    public SquadSettings? SquadSettings { get; set; }

    [JsonPropertyName("SkillEnduranceWeightThreshold")]
    public double? SkillEnduranceWeightThreshold { get; set; }

    [JsonPropertyName("TeamSearchingTimeout")]
    public double? TeamSearchingTimeout { get; set; }

    [JsonPropertyName("Insurance")]
    public Insurance? Insurance { get; set; }

    [JsonPropertyName("SkillExpPerLevel")]
    public double? SkillExpPerLevel { get; set; }

    [JsonPropertyName("GameSearchingTimeout")]
    public double? GameSearchingTimeout { get; set; }

    [JsonPropertyName("WallContusionAbsorption")]
    public XYZ? WallContusionAbsorption { get; set; }

    [JsonPropertyName("WeaponFastDrawSettings")]
    public WeaponFastDrawSettings? WeaponFastDrawSettings { get; set; }

    [JsonPropertyName("SkillsSettings")]
    public SkillsSettings? SkillsSettings { get; set; }

    [JsonPropertyName("AzimuthPanelShowsPlayerOrientation")]
    public bool? AzimuthPanelShowsPlayerOrientation { get; set; }

    [JsonPropertyName("Aiming")]
    public Aiming? Aiming { get; set; }

    [JsonPropertyName("Malfunction")]
    public Malfunction? Malfunction { get; set; }

    [JsonPropertyName("Overheat")]
    public Overheat? Overheat { get; set; }

    [JsonPropertyName("FenceSettings")]
    public FenceSettings? FenceSettings { get; set; }

    [JsonPropertyName("TestValue")]
    public double? TestValue { get; set; }

    [JsonPropertyName("Inertia")]
    public Inertia? Inertia { get; set; }

    [JsonPropertyName("Ballistic")]
    public Ballistic? Ballistic { get; set; }

    [JsonPropertyName("RepairSettings")]
    public RepairSettings? RepairSettings { get; set; }

    [JsonPropertyName("AudioSettings")]
    public AudioSettings? AudioSettings { get; set; }

    public CoopSettings? CoopSettings { get; set; }

    public PveSettings? PveSettings { get; set; }
}

public class PveSettings
{
    public List<string>? AvailableVersions { get; set; }
    public bool? ModeEnabled { get; set; }
}

public class CoopSettings
{
    public List<string>? AvailableVersions { get; set; }
}

public class RunddansSettings
{
    [JsonPropertyName("accessKeys")]
    public List<string>? AccessKeys { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("activePVE")]
    public bool? ActivePVE { get; set; }

    [JsonPropertyName("applyFrozenEverySec")]
    public double? ApplyFrozenEverySec { get; set; }

    [JsonPropertyName("consumables")]
    public List<string>? Consumables { get; set; }

    [JsonPropertyName("drunkImmunitySec")]
    public double? DrunkImmunitySec { get; set; }

    [JsonPropertyName("durability")]
    public XY? Durability { get; set; }

    [JsonPropertyName("fireDistanceToHeat")]
    public double? FireDistanceToHeat { get; set; }

    [JsonPropertyName("grenadeDistanceToBreak")]
    public double? GrenadeDistanceToBreak { get; set; }

    [JsonPropertyName("interactionDistance")]
    public double? InteractionDistance { get; set; }

    [JsonPropertyName("knifeCritChanceToBreak")]
    public double? KnifeCritChanceToBreak { get; set; }

    [JsonPropertyName("locations")]
    public List<string>? Locations { get; set; }

    [JsonPropertyName("multitoolRepairSec")]
    public double? MultitoolRepairSec { get; set; }

    [JsonPropertyName("nonExitsLocations")]
    public List<string>? NonExitsLocations { get; set; }

    [JsonPropertyName("rainForFrozen")]
    public double? RainForFrozen { get; set; }

    [JsonPropertyName("repairSec")]
    public double? RepairSec { get; set; }

    [JsonPropertyName("secToBreak")]
    public XY? SecToBreak { get; set; }

    [JsonPropertyName("sleighLocations")]
    public List<string>? SleighLocations { get; set; }
}

public class SeasonActivity
{
    [JsonPropertyName("InfectionHalloween")]
    public SeasonActivityHalloween? InfectionHalloween { get; set; }
}

public class SeasonActivityHalloween
{
    [JsonPropertyName("DisplayUIEnabled")]
    public bool? DisplayUIEnabled { get; set; }

    [JsonPropertyName("Enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("ZombieBleedMul")]
    public double? ZombieBleedMul { get; set; }
}

public class EnvironmentSetting2
{
    public EnvironmentUIData? EnvironmentUIData { get; set; }
}

public class EnvironmentUIData
{
    public string[] TheUnheardEditionEnvironmentUiType { get; set; }
}

public class BodyPartColliderSettings
{
    public BodyPartColliderPart? BackHead { get; set; }
    public BodyPartColliderPart? Ears { get; set; }
    public BodyPartColliderPart? Eyes { get; set; }
    public BodyPartColliderPart? HeadCommon { get; set; }
    public BodyPartColliderPart? Jaw { get; set; }
    public BodyPartColliderPart? LeftCalf { get; set; }
    public BodyPartColliderPart? LeftForearm { get; set; }
    public BodyPartColliderPart? LeftSideChestDown { get; set; }
    public BodyPartColliderPart? LeftSideChestUp { get; set; }
    public BodyPartColliderPart? LeftThigh { get; set; }
    public BodyPartColliderPart? LeftUpperArm { get; set; }
    public BodyPartColliderPart? NeckBack { get; set; }
    public BodyPartColliderPart? NeckFront { get; set; }
    public BodyPartColliderPart? ParietalHead { get; set; }
    public BodyPartColliderPart? Pelvis { get; set; }
    public BodyPartColliderPart? PelvisBack { get; set; }
    public BodyPartColliderPart? RibcageLow { get; set; }
    public BodyPartColliderPart? RibcageUp { get; set; }
    public BodyPartColliderPart? RightCalf { get; set; }
    public BodyPartColliderPart? RightForearm { get; set; }
    public BodyPartColliderPart? RightSideChestDown { get; set; }
    public BodyPartColliderPart? RightSideChestUp { get; set; }
    public BodyPartColliderPart? RightThigh { get; set; }
    public BodyPartColliderPart? RightUpperArm { get; set; }
    public BodyPartColliderPart? SpineDown { get; set; }
    public BodyPartColliderPart? SpineTop { get; set; }
}

public class BodyPartColliderPart
{
    [JsonPropertyName("PenetrationChance")]
    public double? PenetrationChance { get; set; }

    [JsonPropertyName("PenetrationDamageMod")]
    public double? PenetrationDamageMod { get; set; }

    [JsonPropertyName("PenetrationLevel")]
    public double? PenetrationLevel { get; set; }
}

public class WeaponFastDrawSettings
{
    [JsonPropertyName("HandShakeCurveFrequency")]
    public double? HandShakeCurveFrequency { get; set; }

    [JsonPropertyName("HandShakeCurveIntensity")]
    public double? HandShakeCurveIntensity { get; set; }

    [JsonPropertyName("HandShakeMaxDuration")]
    public double? HandShakeMaxDuration { get; set; }

    [JsonPropertyName("HandShakeTremorIntensity")]
    public double? HandShakeTremorIntensity { get; set; }

    [JsonPropertyName("WeaponFastSwitchMaxSpeedMult")]
    public double? WeaponFastSwitchMaxSpeedMult { get; set; }

    [JsonPropertyName("WeaponFastSwitchMinSpeedMult")]
    public double? WeaponFastSwitchMinSpeedMult { get; set; }

    [JsonPropertyName("WeaponPistolFastSwitchMaxSpeedMult")]
    public double? WeaponPistolFastSwitchMaxSpeedMult { get; set; }

    [JsonPropertyName("WeaponPistolFastSwitchMinSpeedMult")]
    public double? WeaponPistolFastSwitchMinSpeedMult { get; set; }
}

public class EventSettings
{
    [JsonPropertyName("EventActive")]
    public bool? EventActive { get; set; }

    [JsonPropertyName("EventTime")]
    public double? EventTime { get; set; }

    [JsonPropertyName("EventWeather")]
    public EventWeather? EventWeather { get; set; }

    [JsonPropertyName("ExitTimeMultiplier")]
    public double? ExitTimeMultiplier { get; set; }

    [JsonPropertyName("StaminaMultiplier")]
    public double? StaminaMultiplier { get; set; }

    [JsonPropertyName("SummonFailedWeather")]
    public EventWeather? SummonFailedWeather { get; set; }

    [JsonPropertyName("SummonSuccessWeather")]
    public EventWeather? SummonSuccessWeather { get; set; }

    [JsonPropertyName("WeatherChangeTime")]
    public double? WeatherChangeTime { get; set; }
}

public class EventWeather
{
    [JsonPropertyName("Cloudness")]
    public double? Cloudness { get; set; }

    [JsonPropertyName("Hour")]
    public double? Hour { get; set; }

    [JsonPropertyName("Minute")]
    public double? Minute { get; set; }

    [JsonPropertyName("Rain")]
    public double? Rain { get; set; }

    [JsonPropertyName("RainRandomness")]
    public double? RainRandomness { get; set; }

    [JsonPropertyName("ScaterringFogDensity")]
    public double? ScaterringFogDensity { get; set; }

    [JsonPropertyName("TopWindDirection")]
    public XYZ? TopWindDirection { get; set; }

    [JsonPropertyName("Wind")]
    public double? Wind { get; set; }

    [JsonPropertyName("WindDirection")]
    public double? WindDirection { get; set; }
}

public class TransitSettings
{
    [JsonPropertyName("BearPriceMod")]
    public double? BearPriceMod { get; set; }

    [JsonPropertyName("ClearAllPlayerEffectsOnTransit")]
    public bool? ClearAllPlayerEffectsOnTransit { get; set; }

    [JsonPropertyName("CoefficientDiscountCharisma")]
    public double? CoefficientDiscountCharisma { get; set; }

    [JsonPropertyName("DeliveryMinPrice")]
    public double? DeliveryMinPrice { get; set; }

    [JsonPropertyName("DeliveryPrice")]
    public double? DeliveryPrice { get; set; }

    [JsonPropertyName("ModDeliveryCost")]
    public double? ModDeliveryCost { get; set; }

    [JsonPropertyName("PercentageOfMissingEnergyRestore")]
    public double? PercentageOfMissingEnergyRestore { get; set; }

    [JsonPropertyName("PercentageOfMissingHealthRestore")]
    public double? PercentageOfMissingHealthRestore { get; set; }

    [JsonPropertyName("PercentageOfMissingWaterRestore")]
    public double? PercentageOfMissingWaterRestore { get; set; }

    [JsonPropertyName("RestoreHealthOnDestroyedParts")]
    public bool? RestoreHealthOnDestroyedParts { get; set; }

    [JsonPropertyName("ScavPriceMod")]
    public double? ScavPriceMod { get; set; }

    [JsonPropertyName("UsecPriceMod")]
    public double? UsecPriceMod { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }
}

public class TripwiresSettings
{
    [JsonPropertyName("CollisionCapsuleCheckCoef")]
    public double? CollisionCapsuleCheckCoef { get; set; }

    [JsonPropertyName("CollisionCapsuleRadius")]
    public double? CollisionCapsuleRadius { get; set; }

    [JsonPropertyName("DefuseTimeSeconds")]
    public double? DefuseTimeSeconds { get; set; }

    [JsonPropertyName("DestroyedSeconds")]
    public double? DestroyedSeconds { get; set; }

    [JsonPropertyName("GroundDotProductTolerance")]
    public double? GroundDotProductTolerance { get; set; }

    [JsonPropertyName("InertSeconds")]
    public double? InertSeconds { get; set; }

    [JsonPropertyName("InteractionSqrDistance")]
    public double? InteractionSqrDistance { get; set; }

    [JsonPropertyName("MaxHeightDifference")]
    public double? MaxHeightDifference { get; set; }

    [JsonPropertyName("MaxLength")]
    public double? MaxLength { get; set; }

    [JsonPropertyName("MaxPreviewLength")]
    public double? MaxPreviewLength { get; set; }

    [JsonPropertyName("MaxTripwireToPlayerDistance")]
    public double? MaxTripwireToPlayerDistance { get; set; }

    [JsonPropertyName("MinLength")]
    public double? MinLength { get; set; }

    [JsonPropertyName("MultitoolDefuseTimeSeconds")]
    public double? MultitoolDefuseTimeSeconds { get; set; }

    [JsonPropertyName("ShotSqrDistance")]
    public double? ShotSqrDistance { get; set; }
}

public class MountingSettings
{
    [JsonPropertyName("MovementSettings")]
    public MountingMovementSettings? MovementSettings { get; set; }

    [JsonPropertyName("PointDetectionSettings")]
    public MountingPointDetectionSettings? PointDetectionSettings { get; set; }
}

public class MountingMovementSettings
{
    [JsonPropertyName("ApproachTime")]
    public double? ApproachTime { get; set; }

    [JsonPropertyName("ApproachTimeDeltaAngleModifier")]
    public double? ApproachTimeDeltaAngleModifier { get; set; }

    [JsonPropertyName("ExitTime")]
    public double? ExitTime { get; set; }

    [JsonPropertyName("MaxApproachTime")]
    public double? MaxApproachTime { get; set; }

    [JsonPropertyName("MaxPitchLimitExcess")]
    public double? MaxPitchLimitExcess { get; set; }

    [JsonPropertyName("MaxVerticalMountAngle")]
    public double? MaxVerticalMountAngle { get; set; }

    [JsonPropertyName("MaxYawLimitExcess")]
    public double? MaxYawLimitExcess { get; set; }

    [JsonPropertyName("MinApproachTime")]
    public double? MinApproachTime { get; set; }

    [JsonPropertyName("MountingCameraSpeed")]
    public double? MountingCameraSpeed { get; set; }

    [JsonPropertyName("MountingSwayFactorModifier")]
    public double? MountingSwayFactorModifier { get; set; }

    [JsonPropertyName("PitchLimitHorizontal")]
    public XYZ? PitchLimitHorizontal { get; set; }

    [JsonPropertyName("PitchLimitHorizontalBipod")]
    public XYZ? PitchLimitHorizontalBipod { get; set; }

    [JsonPropertyName("PitchLimitVertical")]
    public XYZ? PitchLimitVertical { get; set; }

    [JsonPropertyName("RotationSpeedClamp")]
    public double? RotationSpeedClamp { get; set; }

    [JsonPropertyName("SensitivityMultiplier")]
    public double? SensitivityMultiplier { get; set; }
}

public class MountingPointDetectionSettings
{
    [JsonPropertyName("CheckHorizontalSecondaryOffset")]
    public double? CheckHorizontalSecondaryOffset { get; set; }

    [JsonPropertyName("CheckWallOffset")]
    public double? CheckWallOffset { get; set; }

    [JsonPropertyName("EdgeDetectionDistance")]
    public double? EdgeDetectionDistance { get; set; }

    [JsonPropertyName("GridMaxHeight")]
    public double? GridMaxHeight { get; set; }

    [JsonPropertyName("GridMinHeight")]
    public double? GridMinHeight { get; set; }

    [JsonPropertyName("HorizontalGridFromTopOffset")]
    public double? HorizontalGridFromTopOffset { get; set; }

    [JsonPropertyName("HorizontalGridSize")]
    public double? HorizontalGridSize { get; set; }

    [JsonPropertyName("HorizontalGridStepsAmount")]
    public double? HorizontalGridStepsAmount { get; set; }

    [JsonPropertyName("MaxFramesForRaycast")]
    public double? MaxFramesForRaycast { get; set; }

    [JsonPropertyName("MaxHorizontalMountAngleDotDelta")]
    public double? MaxHorizontalMountAngleDotDelta { get; set; }

    [JsonPropertyName("MaxProneMountAngleDotDelta")]
    public double? MaxProneMountAngleDotDelta { get; set; }

    [JsonPropertyName("MaxVerticalMountAngleDotDelta")]
    public double? MaxVerticalMountAngleDotDelta { get; set; }

    [JsonPropertyName("PointHorizontalMountOffset")]
    public double? PointHorizontalMountOffset { get; set; }

    [JsonPropertyName("PointVerticalMountOffset")]
    public double? PointVerticalMountOffset { get; set; }

    [JsonPropertyName("RaycastDistance")]
    public double? RaycastDistance { get; set; }

    [JsonPropertyName("SecondCheckVerticalDistance")]
    public double? SecondCheckVerticalDistance { get; set; }

    [JsonPropertyName("SecondCheckVerticalGridOffset")]
    public double? SecondCheckVerticalGridOffset { get; set; }

    [JsonPropertyName("SecondCheckVerticalGridSize")]
    public double? SecondCheckVerticalGridSize { get; set; }

    [JsonPropertyName("SecondCheckVerticalGridSizeStepsAmount")]
    public double? SecondCheckVerticalGridSizeStepsAmount { get; set; }

    [JsonPropertyName("VerticalGridSize")]
    public double? VerticalGridSize { get; set; }

    [JsonPropertyName("VerticalGridStepsAmount")]
    public double? VerticalGridStepsAmount { get; set; }
}

public class GraphicSettings
{
    [JsonPropertyName("ExperimentalFogInCity")]
    public bool? ExperimentalFogInCity { get; set; }
}

public class BufferZone
{
    [JsonPropertyName("CustomerAccessTime")]
    public double? CustomerAccessTime { get; set; }

    [JsonPropertyName("CustomerCriticalTimeStart")]
    public double? CustomerCriticalTimeStart { get; set; }

    [JsonPropertyName("CustomerKickNotifTime")]
    public double? CustomerKickNotifTime { get; set; }
}

public class ItemsCommonSettings
{
    [JsonPropertyName("ItemRemoveAfterInterruptionTime")]
    public double? ItemRemoveAfterInterruptionTime { get; set; }
}

public class TradingSettings
{
    [JsonPropertyName("BuyRestrictionMaxBonus")]
    public Dictionary<string, BuyRestrictionMaxBonus>? BuyRestrictionMaxBonus { get; set; }

    [JsonPropertyName("BuyoutRestrictions")]
    public BuyoutRestrictions? BuyoutRestrictions { get; set; }
}

public class BuyRestrictionMaxBonus
{
    [JsonPropertyName("multiplier")]
    public double? Multiplier { get; set; }
}

public class BuyoutRestrictions
{
    [JsonPropertyName("MinDurability")]
    public double? MinDurability { get; set; }

    [JsonPropertyName("MinFoodDrinkResource")]
    public double? MinFoodDrinkResource { get; set; }

    [JsonPropertyName("MinMedsResource")]
    public double? MinMedsResource { get; set; }
}

public class Content
{
    [JsonPropertyName("ip")]
    public string? Ip { get; set; }

    [JsonPropertyName("port")]
    public double? Port { get; set; }

    [JsonPropertyName("root")]
    public string? Root { get; set; }
}

public class Exp
{
    [JsonPropertyName("heal")]
    public Heal? Heal { get; set; }

    [JsonPropertyName("match_end")]
    public MatchEnd? MatchEnd { get; set; }

    [JsonPropertyName("kill")]
    public Kill? Kill { get; set; }

    [JsonPropertyName("level")]
    public Level? Level { get; set; }

    [JsonPropertyName("loot_attempts")]
    public List<LootAttempt>? LootAttempts { get; set; }

    [JsonPropertyName("expForLevelOneDogtag")]
    public double? ExpForLevelOneDogtag { get; set; }

    [JsonPropertyName("expForLockedDoorOpen")]
    public double? ExpForLockedDoorOpen { get; set; }

    [JsonPropertyName("expForLockedDoorBreach")]
    public double? ExpForLockedDoorBreach { get; set; }

    [JsonPropertyName("triggerMult")]
    public double? TriggerMult { get; set; }
}

public class Heal
{
    [JsonPropertyName("expForHeal")]
    public double? ExpForHeal { get; set; }

    [JsonPropertyName("expForHydration")]
    public double? ExpForHydration { get; set; }

    [JsonPropertyName("expForEnergy")]
    public double? ExpForEnergy { get; set; }
}

public class MatchEnd
{
    [JsonPropertyName("README")]
    public string? ReadMe { get; set; }

    [JsonPropertyName("survived_exp_requirement")]
    public double? SurvivedExperienceRequirement { get; set; }

    [JsonPropertyName("survived_seconds_requirement")]
    public double? SurvivedSecondsRequirement { get; set; }

    [JsonPropertyName("survived_exp_reward")]
    public double? SurvivedExperienceReward { get; set; }

    [JsonPropertyName("mia_exp_reward")]
    public double? MiaExperienceReward { get; set; }

    [JsonPropertyName("runner_exp_reward")]
    public double? RunnerExperienceReward { get; set; }

    [JsonPropertyName("leftMult")]
    public double? LeftMultiplier { get; set; }

    [JsonPropertyName("miaMult")]
    public double? MiaMultiplier { get; set; }

    [JsonPropertyName("survivedMult")]
    public double? SurvivedMultiplier { get; set; }

    [JsonPropertyName("runnerMult")]
    public double? RunnerMultiplier { get; set; }

    [JsonPropertyName("killedMult")]
    public double? KilledMultiplier { get; set; }

    [JsonPropertyName("transit_exp_reward")]
    public double? TransitExperienceReward { get; set; }

    [JsonPropertyName("transit_mult")]
    public List<Dictionary<string, double>>? TransitMultiplier { get; set; }
}

public class Kill
{
    [JsonPropertyName("combo")]
    public Combo[] Combos { get; set; }

    [JsonPropertyName("victimLevelExp")]
    public double? VictimLevelExperience { get; set; }

    [JsonPropertyName("headShotMult")]
    public double? HeadShotMultiplier { get; set; }

    [JsonPropertyName("expOnDamageAllHealth")]
    public double? ExperienceOnDamageAllHealth { get; set; }

    [JsonPropertyName("longShotDistance")]
    public double? LongShotDistance { get; set; }

    [JsonPropertyName("bloodLossToLitre")]
    public double? BloodLossToLitre { get; set; }

    [JsonPropertyName("botExpOnDamageAllHealth")]
    public double? BotExperienceOnDamageAllHealth { get; set; }

    [JsonPropertyName("botHeadShotMult")]
    public double? BotHeadShotMultiplier { get; set; }

    [JsonPropertyName("victimBotLevelExp")]
    public double? VictimBotLevelExperience { get; set; }

    [JsonPropertyName("pmcExpOnDamageAllHealth")]
    public double? PmcExperienceOnDamageAllHealth { get; set; }

    [JsonPropertyName("pmcHeadShotMult")]
    public double? PmcHeadShotMultiplier { get; set; }
}

public class Combo
{
    [JsonPropertyName("percent")]
    public double? Percentage { get; set; }
}

public class Level
{
    [JsonPropertyName("exp_table")]
    public ExpTable[] ExperienceTable { get; set; }

    [JsonPropertyName("trade_level")]
    public double? TradeLevel { get; set; }

    [JsonPropertyName("savage_level")]
    public double? SavageLevel { get; set; }

    [JsonPropertyName("clan_level")]
    public double? ClanLevel { get; set; }

    [JsonPropertyName("mastering1")]
    public double? Mastering1 { get; set; }

    [JsonPropertyName("mastering2")]
    public double? Mastering2 { get; set; }
}

public class ExpTable
{
    [JsonPropertyName("exp")]
    public int? Experience { get; set; }
}

public class LootAttempt
{
    [JsonPropertyName("k_exp")]
    public double? ExperiencePoints { get; set; }
}

public class Armor
{
    [JsonPropertyName("class")]
    public List<Class>? Classes { get; set; }
}

public class Class
{
    [JsonPropertyName("resistance")]
    public double? Resistance { get; set; }
}

public class Mastering
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Templates")]
    public List<string>? Templates { get; set; }

    [JsonPropertyName("Level2")]
    public double? Level2 { get; set; }

    [JsonPropertyName("Level3")]
    public double? Level3 { get; set; }
}

public class Customization
{
    [JsonPropertyName("SavageHead")]
    public Dictionary<string, WildHead>? Head { get; set; }

    [JsonPropertyName("SavageBody")]
    public Dictionary<string, WildBody>? Body { get; set; }

    [JsonPropertyName("SavageFeet")]
    public Dictionary<string, WildFeet>? Feet { get; set; }

    [JsonPropertyName("CustomizationVoice")]
    public List<CustomizationVoice>? VoiceOptions { get; set; }

    [JsonPropertyName("BodyParts")]
    public BodyParts? BodyParts { get; set; }
}

public class WildHead
{
    [JsonPropertyName("head")]
    public string? Head { get; set; }

    [JsonPropertyName("isNotRandom")]
    public bool? IsNotRandom { get; set; }

    [JsonPropertyName("NotRandom")]
    public bool? NotRandom { get; set; }
}

public class WildBody
{
    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("hands")]
    public string? Hands { get; set; }

    [JsonPropertyName("isNotRandom")]
    public bool? IsNotRandom { get; set; }
}
public class WildFeet
{
    [JsonPropertyName("feet")]
    public string? Feet { get; set; }

    [JsonPropertyName("isNotRandom")]
    public bool? IsNotRandom { get; set; }

    [JsonPropertyName("NotRandom")]
    public bool? NotRandom { get; set; }
}

public class CustomizationVoice
{
    [JsonPropertyName("voice")]
    public string? Voice { get; set; }

    [JsonPropertyName("side")]
    public List<string>? Side { get; set; }

    [JsonPropertyName("isNotRandom")]
    public bool? IsNotRandom { get; set; }
}

public class BodyParts
{
    public string? Head { get; set; }
    public string? Body { get; set; }
    public string? Feet { get; set; }
    public string? Hands { get; set; }
}

public class AirdropGlobalSettings
{
    public string? AirdropViewType { get; set; }
    public double? ParachuteEndOpenHeight { get; set; }
    public double? ParachuteStartOpenHeight { get; set; }
    public double? PlaneAdditionalDistance { get; set; }
    public double? PlaneAirdropDuration { get; set; }
    public double? PlaneAirdropFlareWait { get; set; }
    public double? PlaneAirdropSmoke { get; set; }
    public double? PlaneMaxFlightHeight { get; set; }
    public double? PlaneMinFlightHeight { get; set; }
    public double? PlaneSpeed { get; set; }
    public double? SmokeActivateHeight { get; set; }
}

public class KarmaCalculationSettings
{
    [JsonPropertyName("defaultPveKarmaValue")]
    public double? DefaultPveKarmaValue { get; set; }

    [JsonPropertyName("enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("expireDaysAfterLastRaid")]
    public double? ExpireDaysAfterLastRaid { get; set; }

    [JsonPropertyName("maxKarmaThresholdPercentile")]
    public double? MaxKarmaThresholdPercentile { get; set; }

    [JsonPropertyName("minKarmaThresholdPercentile")]
    public double? MinKarmaThresholdPercentile { get; set; }

    [JsonPropertyName("minSurvivedRaidCount")]
    public double? MinSurvivedRaidCount { get; set; }
}

public class ArenaEftTransferSettings
{
    public double? ArenaManagerReputationTaxMultiplier { get; set; }
    public double? CharismaTaxMultiplier { get; set; }
    public double? CreditPriceTaxMultiplier { get; set; }
    public double? RubTaxMultiplier { get; set; }
    public Dictionary<string, double>? TransferLimitsByGameEdition { get; set; }
    public Dictionary<string, double>? TransferLimitsSettings { get; set; }
}

public class ArmorMaterials
{
    [JsonPropertyName("UHMWPE")]
    public ArmorType? UHMWPE { get; set; }

    [JsonPropertyName("Aramid")]
    public ArmorType? Aramid { get; set; }

    [JsonPropertyName("Combined")]
    public ArmorType? Combined { get; set; }

    [JsonPropertyName("Titan")]
    public ArmorType? Titan { get; set; }

    [JsonPropertyName("Aluminium")]
    public ArmorType? Aluminium { get; set; }

    [JsonPropertyName("ArmoredSteel")]
    public ArmorType? ArmoredSteel { get; set; }

    [JsonPropertyName("Ceramic")]
    public ArmorType? Ceramic { get; set; }

    [JsonPropertyName("Glass")]
    public ArmorType? Glass { get; set; }
}

public class ArmorType
{
    [JsonPropertyName("Destructibility")]
    public double? Destructibility { get; set; }

    [JsonPropertyName("MinRepairDegradation")]
    public double? MinRepairDegradation { get; set; }

    [JsonPropertyName("MaxRepairDegradation")]
    public double? MaxRepairDegradation { get; set; }

    [JsonPropertyName("ExplosionDestructibility")]
    public double? ExplosionDestructibility { get; set; }

    [JsonPropertyName("MinRepairKitDegradation")]
    public double? MinRepairKitDegradation { get; set; }

    [JsonPropertyName("MaxRepairKitDegradation")]
    public double? MaxRepairKitDegradation { get; set; }
}

public class Health
{
    [JsonPropertyName("Falling")]
    public Falling? Falling { get; set; }

    [JsonPropertyName("Effects")]
    public Effects? Effects { get; set; }

    [JsonPropertyName("HealPrice")]
    public HealPrice? HealPrice { get; set; }

    [JsonPropertyName("ProfileHealthSettings")]
    public ProfileHealthSettings? ProfileHealthSettings { get; set; }
}

public class Falling
{
    [JsonPropertyName("DamagePerMeter")]
    public double? DamagePerMeter { get; set; }

    [JsonPropertyName("SafeHeight")]
    public double? SafeHeight { get; set; }
}

public class Effects
{
    [JsonPropertyName("Existence")]
    public Existence? Existence { get; set; }

    [JsonPropertyName("Dehydration")]
    public Dehydration? Dehydration { get; set; }

    [JsonPropertyName("BreakPart")]
    public BreakPart? BreakPart { get; set; }

    [JsonPropertyName("Contusion")]
    public Contusion? Contusion { get; set; }

    [JsonPropertyName("Disorientation")]
    public Disorientation? Disorientation { get; set; }

    [JsonPropertyName("Exhaustion")]
    public Exhaustion? Exhaustion { get; set; }

    [JsonPropertyName("LowEdgeHealth")]
    public LowEdgeHealth? LowEdgeHealth { get; set; }

    [JsonPropertyName("RadExposure")]
    public RadExposure? RadExposure { get; set; }

    [JsonPropertyName("Stun")]
    public Stun? Stun { get; set; }

    [JsonPropertyName("Intoxication")]
    public Intoxication? Intoxication { get; set; }

    [JsonPropertyName("Regeneration")]
    public Regeneration? Regeneration { get; set; }

    [JsonPropertyName("Wound")]
    public Wound? Wound { get; set; }

    [JsonPropertyName("Berserk")]
    public Berserk? Berserk { get; set; }

    [JsonPropertyName("Flash")]
    public Flash? Flash { get; set; }

    [JsonPropertyName("MedEffect")]
    public MedEffect? MedEffect { get; set; }

    [JsonPropertyName("Pain")]
    public Pain? Pain { get; set; }

    [JsonPropertyName("PainKiller")]
    public PainKiller? PainKiller { get; set; }

    [JsonPropertyName("SandingScreen")]
    public SandingScreen? SandingScreen { get; set; }

    [JsonPropertyName("MildMusclePain")]
    public MusclePainEffect? MildMusclePain { get; set; }

    [JsonPropertyName("SevereMusclePain")]
    public MusclePainEffect? SevereMusclePain { get; set; }

    [JsonPropertyName("Stimulator")]
    public Stimulator? Stimulator { get; set; }

    [JsonPropertyName("Tremor")]
    public Tremor? Tremor { get; set; }

    [JsonPropertyName("ChronicStaminaFatigue")]
    public ChronicStaminaFatigue? ChronicStaminaFatigue { get; set; }

    [JsonPropertyName("Fracture")]
    public Fracture? Fracture { get; set; }

    [JsonPropertyName("HeavyBleeding")]
    public HeavyBleeding? HeavyBleeding { get; set; }

    [JsonPropertyName("LightBleeding")]
    public LightBleeding? LightBleeding { get; set; }

    [JsonPropertyName("BodyTemperature")]
    public BodyTemperature? BodyTemperature { get; set; }

    [JsonPropertyName("ZombieInfection")]
    public ZombieInfection? ZombieInfection { get; set; }
}

public class ZombieInfection
{
    [JsonPropertyName("Dehydration")]
    public double? Dehydration { get; set; }

    [JsonPropertyName("HearingDebuffPercentage")]
    public double? HearingDebuffPercentage { get; set; }

    // The C on the Cumulatie down here is the russian C, its encoded differently, I THINK
    // Just in case, dont change it
    [JsonPropertyName("СumulativeTime")]
    public double? CumulativeTime { get; set; }
}

public class Existence
{
    [JsonPropertyName("EnergyLoopTime")]
    public double? EnergyLoopTime { get; set; }

    [JsonPropertyName("HydrationLoopTime")]
    public double? HydrationLoopTime { get; set; }

    [JsonPropertyName("EnergyDamage")]
    public double? EnergyDamage { get; set; }

    [JsonPropertyName("HydrationDamage")]
    public double? HydrationDamage { get; set; }

    [JsonPropertyName("DestroyedStomachEnergyTimeFactor")]
    public double? DestroyedStomachEnergyTimeFactor { get; set; }

    [JsonPropertyName("DestroyedStomachHydrationTimeFactor")]
    public double? DestroyedStomachHydrationTimeFactor { get; set; }
}

public class Dehydration
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("BleedingHealth")]
    public double? BleedingHealth { get; set; }

    [JsonPropertyName("BleedingLoopTime")]
    public double? BleedingLoopTime { get; set; }

    [JsonPropertyName("BleedingLifeTime")]
    public double? BleedingLifeTime { get; set; }

    [JsonPropertyName("DamageOnStrongDehydration")]
    public double? DamageOnStrongDehydration { get; set; }

    [JsonPropertyName("StrongDehydrationLoopTime")]
    public double? StrongDehydrationLoopTime { get; set; }
}

public class BreakPart
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience { get; set; }

    [JsonPropertyName("OfflineDurationMin")]
    public double? OfflineDurationMin { get; set; }

    [JsonPropertyName("OfflineDurationMax")]
    public double? OfflineDurationMax { get; set; }

    [JsonPropertyName("RemovePrice")]
    public double? RemovePrice { get; set; }

    [JsonPropertyName("RemovedAfterDeath")]
    public bool? RemovedAfterDeath { get; set; }

    [JsonPropertyName("BulletHitProbability")]
    public Probability? BulletHitProbability { get; set; }

    [JsonPropertyName("FallingProbability")]
    public Probability? FallingProbability { get; set; }
}

public class Contusion
{
    [JsonPropertyName("Dummy")]
    public double? Dummy { get; set; }
}

public class Disorientation
{
    [JsonPropertyName("Dummy")]
    public double? Dummy { get; set; }
}

public class Exhaustion
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("Damage")]
    public double? Damage { get; set; }

    [JsonPropertyName("DamageLoopTime")]
    public double? DamageLoopTime { get; set; }
}

public class LowEdgeHealth
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("StartCommonHealth")]
    public double? StartCommonHealth { get; set; }
}

public class RadExposure
{
    [JsonPropertyName("Damage")]
    public double? Damage { get; set; }

    [JsonPropertyName("DamageLoopTime")]
    public double? DamageLoopTime { get; set; }
}

public class Stun
{
    [JsonPropertyName("Dummy")]
    public double? Dummy { get; set; }
}

public class Intoxication
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("DamageHealth")]
    public double? DamageHealth { get; set; }

    [JsonPropertyName("HealthLoopTime")]
    public double? HealthLoopTime { get; set; }

    [JsonPropertyName("OfflineDurationMin")]
    public double? OfflineDurationMin { get; set; }

    [JsonPropertyName("OfflineDurationMax")]
    public double? OfflineDurationMax { get; set; }

    [JsonPropertyName("RemovedAfterDeath")]
    public bool? RemovedAfterDeath { get; set; }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience { get; set; }

    [JsonPropertyName("RemovePrice")]
    public double? RemovePrice { get; set; }
}

public class Regeneration
{
    [JsonPropertyName("LoopTime")]
    public double? LoopTime { get; set; }

    [JsonPropertyName("MinimumHealthPercentage")]
    public double? MinimumHealthPercentage { get; set; }

    [JsonPropertyName("Energy")]
    public double? Energy { get; set; }

    [JsonPropertyName("Hydration")]
    public double? Hydration { get; set; }

    [JsonPropertyName("BodyHealth")]
    public BodyHealth? BodyHealth { get; set; }

    [JsonPropertyName("Influences")]
    public Influences? Influences { get; set; }
}

public class BodyHealth
{
    [JsonPropertyName("Head")]
    public BodyHealthValue? Head { get; set; }

    [JsonPropertyName("Chest")]
    public BodyHealthValue? Chest { get; set; }

    [JsonPropertyName("Stomach")]
    public BodyHealthValue? Stomach { get; set; }

    [JsonPropertyName("LeftArm")]
    public BodyHealthValue? LeftArm { get; set; }

    [JsonPropertyName("RightArm")]
    public BodyHealthValue? RightArm { get; set; }

    [JsonPropertyName("LeftLeg")]
    public BodyHealthValue? LeftLeg { get; set; }

    [JsonPropertyName("RightLeg")]
    public BodyHealthValue? RightLeg { get; set; }
}

public class BodyHealthValue
{
    [JsonPropertyName("Value")]
    public double? Value { get; set; }
}

public class Influences
{
    [JsonPropertyName("LightBleeding")]
    public Influence? LightBleeding { get; set; }

    [JsonPropertyName("HeavyBleeding")]
    public Influence? HeavyBleeding { get; set; }

    [JsonPropertyName("Fracture")]
    public Influence? Fracture { get; set; }

    [JsonPropertyName("RadExposure")]
    public Influence? RadExposure { get; set; }

    [JsonPropertyName("Intoxication")]
    public Influence? Intoxication { get; set; }
}

public class Influence
{
    [JsonPropertyName("HealthSlowDownPercentage")]
    public double? HealthSlowDownPercentage { get; set; }

    [JsonPropertyName("EnergySlowDownPercentage")]
    public double? EnergySlowDownPercentage { get; set; }

    [JsonPropertyName("HydrationSlowDownPercentage")]
    public double? HydrationSlowDownPercentage { get; set; }
}

public class Wound
{
    [JsonPropertyName("WorkingTime")]
    public double? WorkingTime { get; set; }

    [JsonPropertyName("ThresholdMin")]
    public double? ThresholdMin { get; set; }

    [JsonPropertyName("ThresholdMax")]
    public double? ThresholdMax { get; set; }
}

public class Berserk
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("WorkingTime")]
    public double? WorkingTime { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }
}

public class Flash
{
    [JsonPropertyName("Dummy")]
    public double? Dummy { get; set; }
}

public class MedEffect
{
    [JsonPropertyName("LoopTime")]
    public double? LoopTime { get; set; }

    [JsonPropertyName("StartDelay")]
    public double? StartDelay { get; set; }

    [JsonPropertyName("DrinkStartDelay")]
    public double? DrinkStartDelay { get; set; }

    [JsonPropertyName("FoodStartDelay")]
    public double? FoodStartDelay { get; set; }

    [JsonPropertyName("DrugsStartDelay")]
    public double? DrugsStartDelay { get; set; }

    [JsonPropertyName("MedKitStartDelay")]
    public double? MedKitStartDelay { get; set; }

    [JsonPropertyName("MedicalStartDelay")]
    public double? MedicalStartDelay { get; set; }

    [JsonPropertyName("StimulatorStartDelay")]
    public double? StimulatorStartDelay { get; set; }
}

public class Pain
{
    [JsonPropertyName("TremorDelay")]
    public double? TremorDelay { get; set; }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience { get; set; }
}

public class PainKiller
{
    public double? Dummy { get; set; }
}

public class SandingScreen
{
    public double? Dummy { get; set; }
}

public class MusclePainEffect
{
    public double? GymEffectivity { get; set; }
    public double? OfflineDurationMax { get; set; }
    public double? OfflineDurationMin { get; set; }
    public double? TraumaChance { get; set; }
}

public class Stimulator
{
    public double? BuffLoopTime { get; set; }
    public Dictionary<string, List<Buff>>? Buffs { get; set; }
}

public class Buff
{
    [JsonPropertyName("BuffType")]
    public string? BuffType { get; set; }

    [JsonPropertyName("Chance")]
    public double? Chance { get; set; }

    [JsonPropertyName("Delay")]
    public double? Delay { get; set; }

    [JsonPropertyName("Duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("Value")]
    public double? Value { get; set; }

    [JsonPropertyName("AbsoluteValue")]
    public bool? AbsoluteValue { get; set; }

    [JsonPropertyName("SkillName")]
    public string? SkillName { get; set; }

    public List<string>? AppliesTo { get; set; }
}

public class Tremor
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }
}

public class ChronicStaminaFatigue
{
    [JsonPropertyName("EnergyRate")]
    public double? EnergyRate { get; set; }

    [JsonPropertyName("WorkingTime")]
    public double? WorkingTime { get; set; }

    [JsonPropertyName("TicksEvery")]
    public double? TicksEvery { get; set; }

    [JsonPropertyName("EnergyRatePerStack")]
    public double? EnergyRatePerStack { get; set; }
}

public class Fracture
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience { get; set; }

    [JsonPropertyName("OfflineDurationMin")]
    public double? OfflineDurationMin { get; set; }

    [JsonPropertyName("OfflineDurationMax")]
    public double? OfflineDurationMax { get; set; }

    [JsonPropertyName("RemovePrice")]
    public double? RemovePrice { get; set; }

    [JsonPropertyName("RemovedAfterDeath")]
    public bool? RemovedAfterDeath { get; set; }

    [JsonPropertyName("BulletHitProbability")]
    public Probability? BulletHitProbability { get; set; }

    [JsonPropertyName("FallingProbability")]
    public Probability? FallingProbability { get; set; }
}

public class HeavyBleeding
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("DamageEnergy")]
    public double? DamageEnergy { get; set; }

    [JsonPropertyName("DamageHealth")]
    public double? DamageHealth { get; set; }

    [JsonPropertyName("EnergyLoopTime")]
    public double? EnergyLoopTime { get; set; }

    [JsonPropertyName("HealthLoopTime")]
    public double? HealthLoopTime { get; set; }

    [JsonPropertyName("DamageHealthDehydrated")]
    public double? DamageHealthDehydrated { get; set; }

    [JsonPropertyName("HealthLoopTimeDehydrated")]
    public double? HealthLoopTimeDehydrated { get; set; }

    [JsonPropertyName("LifeTimeDehydrated")]
    public double? LifeTimeDehydrated { get; set; }

    [JsonPropertyName("EliteVitalityDuration")]
    public double? EliteVitalityDuration { get; set; }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience { get; set; }

    [JsonPropertyName("OfflineDurationMin")]
    public double? OfflineDurationMin { get; set; }

    [JsonPropertyName("OfflineDurationMax")]
    public double? OfflineDurationMax { get; set; }

    [JsonPropertyName("RemovePrice")]
    public double? RemovePrice { get; set; }

    [JsonPropertyName("RemovedAfterDeath")]
    public bool? RemovedAfterDeath { get; set; }

    [JsonPropertyName("Probability")]
    public Probability? Probability { get; set; }
}

public class Probability
{
    [JsonPropertyName("FunctionType")]
    public string? FunctionType { get; set; }

    [JsonPropertyName("K")]
    public double? K { get; set; }

    [JsonPropertyName("B")]
    public double? B { get; set; }

    [JsonPropertyName("Threshold")]
    public double? Threshold { get; set; }
}

public class LightBleeding
{
    [JsonPropertyName("DefaultDelay")]
    public double? DefaultDelay { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("DamageEnergy")]
    public double? DamageEnergy { get; set; }

    [JsonPropertyName("DamageHealth")]
    public double? DamageHealth { get; set; }

    [JsonPropertyName("EnergyLoopTime")]
    public double? EnergyLoopTime { get; set; }

    [JsonPropertyName("HealthLoopTime")]
    public double? HealthLoopTime { get; set; }

    [JsonPropertyName("DamageHealthDehydrated")]
    public double? DamageHealthDehydrated { get; set; }

    [JsonPropertyName("HealthLoopTimeDehydrated")]
    public double? HealthLoopTimeDehydrated { get; set; }

    [JsonPropertyName("LifeTimeDehydrated")]
    public double? LifeTimeDehydrated { get; set; }

    [JsonPropertyName("EliteVitalityDuration")]
    public double? EliteVitalityDuration { get; set; }

    [JsonPropertyName("HealExperience")]
    public double? HealExperience { get; set; }

    [JsonPropertyName("OfflineDurationMin")]
    public double? OfflineDurationMin { get; set; }

    [JsonPropertyName("OfflineDurationMax")]
    public double? OfflineDurationMax { get; set; }

    [JsonPropertyName("RemovePrice")]
    public double? RemovePrice { get; set; }

    [JsonPropertyName("RemovedAfterDeath")]
    public bool? RemovedAfterDeath { get; set; }

    [JsonPropertyName("Probability")]
    public Probability? Probability { get; set; }
}

public class BodyTemperature
{
    [JsonPropertyName("DefaultBuildUpTime")]
    public double? DefaultBuildUpTime { get; set; }

    [JsonPropertyName("DefaultResidueTime")]
    public double? DefaultResidueTime { get; set; }

    [JsonPropertyName("LoopTime")]
    public double? LoopTime { get; set; }
}

public class HealPrice
{
    [JsonPropertyName("HealthPointPrice")]
    public double? HealthPointPrice { get; set; }

    [JsonPropertyName("HydrationPointPrice")]
    public double? HydrationPointPrice { get; set; }

    [JsonPropertyName("EnergyPointPrice")]
    public double? EnergyPointPrice { get; set; }

    [JsonPropertyName("TrialLevels")]
    public double? TrialLevels { get; set; }

    [JsonPropertyName("TrialRaids")]
    public double? TrialRaids { get; set; }
}

public class ProfileHealthSettings
{
    [JsonPropertyName("BodyPartsSettings")]
    public BodyPartsSettings? BodyPartsSettings { get; set; }

    [JsonPropertyName("HealthFactorsSettings")]
    public HealthFactorsSettings? HealthFactorsSettings { get; set; }

    [JsonPropertyName("DefaultStimulatorBuff")]
    public string? DefaultStimulatorBuff { get; set; }
}

public class BodyPartsSettings
{
    [JsonPropertyName("Head")]
    public BodyPartsSetting? Head { get; set; }

    [JsonPropertyName("Chest")]
    public BodyPartsSetting? Chest { get; set; }

    [JsonPropertyName("Stomach")]
    public BodyPartsSetting? Stomach { get; set; }

    [JsonPropertyName("LeftArm")]
    public BodyPartsSetting? LeftArm { get; set; }

    [JsonPropertyName("RightArm")]
    public BodyPartsSetting? RightArm { get; set; }

    [JsonPropertyName("LeftLeg")]
    public BodyPartsSetting? LeftLeg { get; set; }

    [JsonPropertyName("RightLeg")]
    public BodyPartsSetting? RightLeg { get; set; }
}

public class BodyPartsSetting
{
    [JsonPropertyName("Minimum")]
    public double? Minimum { get; set; }

    [JsonPropertyName("Maximum")]
    public double? Maximum { get; set; }

    [JsonPropertyName("Default")]
    public double? Default { get; set; }

    [JsonPropertyName("EnvironmentDamageMultiplier")]
    public float? EnvironmentDamageMultiplier { get; set; }

    [JsonPropertyName("OverDamageReceivedMultiplier")]
    public float? OverDamageReceivedMultiplier { get; set; }
}

public class HealthFactorsSettings
{
    [JsonPropertyName("Energy")]
    public HealthFactorSetting? Energy { get; set; }

    [JsonPropertyName("Hydration")]
    public HealthFactorSetting? Hydration { get; set; }

    [JsonPropertyName("Temperature")]
    public HealthFactorSetting? Temperature { get; set; }

    [JsonPropertyName("Poisoning")]
    public HealthFactorSetting? Poisoning { get; set; }

    [JsonPropertyName("Radiation")]
    public HealthFactorSetting? Radiation { get; set; }
}

public class HealthFactorSetting
{
    [JsonPropertyName("Minimum")]
    public double? Minimum { get; set; }

    [JsonPropertyName("Maximum")]
    public double? Maximum { get; set; }

    [JsonPropertyName("Default")]
    public double? Default { get; set; }
}

public class Rating
{
    [JsonPropertyName("levelRequired")]
    public double? LevelRequired { get; set; }

    [JsonPropertyName("limit")]
    public double? Limit { get; set; }

    [JsonPropertyName("categories")]
    public Categories? Categories { get; set; }
}

public class Categories
{
    [JsonPropertyName("experience")]
    public bool? Experience { get; set; }

    [JsonPropertyName("kd")]
    public bool? Kd { get; set; }

    [JsonPropertyName("surviveRatio")]
    public bool? SurviveRatio { get; set; }

    [JsonPropertyName("avgEarnings")]
    public bool? AvgEarnings { get; set; }

    [JsonPropertyName("pmcKills")]
    public bool? PmcKills { get; set; }

    [JsonPropertyName("raidCount")]
    public bool? RaidCount { get; set; }

    [JsonPropertyName("longestShot")]
    public bool? LongestShot { get; set; }

    [JsonPropertyName("timeOnline")]
    public bool? TimeOnline { get; set; }

    [JsonPropertyName("inventoryFullCost")]
    public bool? InventoryFullCost { get; set; }

    [JsonPropertyName("ragFairStanding")]
    public bool? RagFairStanding { get; set; }
}

public class Tournament
{
    [JsonPropertyName("categories")]
    public TournamentCategories? Categories { get; set; }

    [JsonPropertyName("limit")]
    public double? Limit { get; set; }

    [JsonPropertyName("levelRequired")]
    public double? LevelRequired { get; set; }
}

public class TournamentCategories
{
    [JsonPropertyName("dogtags")]
    public bool? Dogtags { get; set; }
}

public class RagFair
{
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("priceStabilizerEnabled")]
    public bool? PriceStabilizerEnabled { get; set; }

    [JsonPropertyName("includePveTraderSales")]
    public bool? IncludePveTraderSales { get; set; }

    [JsonPropertyName("priceStabilizerStartIntervalInHours")]
    public double? PriceStabilizerStartIntervalInHours { get; set; }

    [JsonPropertyName("minUserLevel")]
    public double? MinUserLevel { get; set; }

    [JsonPropertyName("communityTax")]
    public float? CommunityTax { get; set; }

    [JsonPropertyName("communityItemTax")]
    public float? CommunityItemTax { get; set; }

    [JsonPropertyName("communityRequirementTax")]
    public float? CommunityRequirementTax { get; set; }

    [JsonPropertyName("offerPriorityCost")]
    public float? OfferPriorityCost { get; set; }

    [JsonPropertyName("offerDurationTimeInHour")]
    public double? OfferDurationTimeInHour { get; set; }

    [JsonPropertyName("offerDurationTimeInHourAfterRemove")]
    public double? OfferDurationTimeInHourAfterRemove { get; set; }

    [JsonPropertyName("priorityTimeModifier")]
    public float? PriorityTimeModifier { get; set; }

    [JsonPropertyName("maxRenewOfferTimeInHour")]
    public double? MaxRenewOfferTimeInHour { get; set; }

    [JsonPropertyName("renewPricePerHour")]
    public float? RenewPricePerHour { get; set; }

    [JsonPropertyName("maxActiveOfferCount")]
    public List<MaxActiveOfferCount>? MaxActiveOfferCount { get; set; }

    [JsonPropertyName("balancerRemovePriceCoefficient")]
    public float? BalancerRemovePriceCoefficient { get; set; }

    [JsonPropertyName("balancerMinPriceCount")]
    public float? BalancerMinPriceCount { get; set; }

    [JsonPropertyName("balancerAveragePriceCoefficient")]
    public float? BalancerAveragePriceCoefficient { get; set; }

    [JsonPropertyName("delaySinceOfferAdd")]
    public float? DelaySinceOfferAdd { get; set; }

    [JsonPropertyName("uniqueBuyerTimeoutInDays")]
    public double? UniqueBuyerTimeoutInDays { get; set; }

    [JsonPropertyName("userRatingChangeFrequencyMultiplayer")]
    public float? UserRatingChangeFrequencyMultiplayer { get; set; }

    [JsonPropertyName("RagfairTurnOnTimestamp")]
    public long? RagfairTurnOnTimestamp { get; set; }

    [JsonPropertyName("ratingSumForIncrease")]
    public float? RatingSumForIncrease { get; set; }

    [JsonPropertyName("ratingIncreaseCount")]
    public double? RatingIncreaseCount { get; set; }

    [JsonPropertyName("ratingSumForDecrease")]
    public float? RatingSumForDecrease { get; set; }

    [JsonPropertyName("ratingDecreaseCount")]
    public double? RatingDecreaseCount { get; set; }

    [JsonPropertyName("maxSumForIncreaseRatingPerOneSale")]
    public float? MaxSumForIncreaseRatingPerOneSale { get; set; }

    [JsonPropertyName("maxSumForDecreaseRatingPerOneSale")]
    public float? MaxSumForDecreaseRatingPerOneSale { get; set; }

    [JsonPropertyName("maxSumForRarity")]
    public MaxSumForRarity? MaxSumForRarity { get; set; }

    [JsonPropertyName("ChangePriceCoef")]
    public float? ChangePriceCoef { get; set; }

    [JsonPropertyName("ItemRestrictions")]
    public List<ItemGlobalRestrictions>? ItemRestrictions { get; set; }

    [JsonPropertyName("balancerUserItemSaleCooldownEnabled")]
    public bool? BalancerUserItemSaleCooldownEnabled { get; set; }

    [JsonPropertyName("balancerUserItemSaleCooldown")]
    public float? BalancerUserItemSaleCooldown { get; set; }

    [JsonPropertyName("youSellOfferMaxStorageTimeInHour")]
    public double? YouSellOfferMaxStorageTimeInHour { get; set; }

    [JsonPropertyName("yourOfferDidNotSellMaxStorageTimeInHour")]
    public double? YourOfferDidNotSellMaxStorageTimeInHour { get; set; }

    [JsonPropertyName("isOnlyFoundInRaidAllowed")]
    public bool? IsOnlyFoundInRaidAllowed { get; set; }

    [JsonPropertyName("sellInOnePiece")]
    public double? SellInOnePiece { get; set; }
}

public class ItemGlobalRestrictions
{
    [JsonPropertyName("MaxFlea")]
    public double? MaxFlea { get; set; }

    [JsonPropertyName("MaxFleaStacked")]
    public double? MaxFleaStacked { get; set; }

    [JsonPropertyName("TemplateId")]
    public string? TemplateId { get; set; }
}

public class MaxActiveOfferCount
{
    [JsonPropertyName("from")]
    public double? From { get; set; }

    [JsonPropertyName("to")]
    public double? To { get; set; }

    [JsonPropertyName("count")]
    public double? Count { get; set; }

    [JsonPropertyName("countForSpecialEditions")]
    public double? CountForSpecialEditions { get; set; }
}

public class MaxSumForRarity
{
    [JsonPropertyName("Common")]
    public RarityMaxSum? Common { get; set; }

    [JsonPropertyName("Rare")]
    public RarityMaxSum? Rare { get; set; }

    [JsonPropertyName("Superrare")]
    public RarityMaxSum? Superrare { get; set; }

    [JsonPropertyName("Not_exist")]
    public RarityMaxSum? NotExist { get; set; }
}

public class RarityMaxSum
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public class Handbook
{
    [JsonPropertyName("defaultCategory")]
    public string? DefaultCategory { get; set; }
}

public class Stamina
{
    [JsonPropertyName("Capacity")]
    public double? Capacity { get; set; }

    [JsonPropertyName("SprintDrainRate")]
    public double? SprintDrainRate { get; set; }

    [JsonPropertyName("BaseRestorationRate")]
    public double? BaseRestorationRate { get; set; }

    [JsonPropertyName("BipodAimDrainRateMultiplier")]
    public double? BipodAimDrainRateMultiplier { get; set; }

    [JsonPropertyName("JumpConsumption")]
    public double? JumpConsumption { get; set; }

    [JsonPropertyName("MountingHorizontalAimDrainRateMultiplier")]
    public double? MountingHorizontalAimDrainRateMultiplier { get; set; }

    [JsonPropertyName("MountingVerticalAimDrainRateMultiplier")]
    public double? MountingVerticalAimDrainRateMultiplier { get; set; }

    [JsonPropertyName("GrenadeHighThrow")]
    public double? GrenadeHighThrow { get; set; }

    [JsonPropertyName("GrenadeLowThrow")]
    public double? GrenadeLowThrow { get; set; }

    [JsonPropertyName("AimDrainRate")]
    public double? AimDrainRate { get; set; }

    [JsonPropertyName("AimRangeFinderDrainRate")]
    public double? AimRangeFinderDrainRate { get; set; }

    [JsonPropertyName("OxygenCapacity")]
    public double? OxygenCapacity { get; set; }

    [JsonPropertyName("OxygenRestoration")]
    public double? OxygenRestoration { get; set; }

    [JsonPropertyName("WalkOverweightLimits")]
    public XYZ? WalkOverweightLimits { get; set; }

    [JsonPropertyName("BaseOverweightLimits")]
    public XYZ? BaseOverweightLimits { get; set; }

    [JsonPropertyName("SprintOverweightLimits")]
    public XYZ? SprintOverweightLimits { get; set; }

    [JsonPropertyName("WalkSpeedOverweightLimits")]
    public XYZ? WalkSpeedOverweightLimits { get; set; }

    [JsonPropertyName("CrouchConsumption")]
    public XYZ? CrouchConsumption { get; set; }

    [JsonPropertyName("WalkConsumption")]
    public XYZ? WalkConsumption { get; set; }

    [JsonPropertyName("StandupConsumption")]
    public XYZ? StandupConsumption { get; set; }

    [JsonPropertyName("TransitionSpeed")]
    public XYZ? TransitionSpeed { get; set; }

    [JsonPropertyName("SprintAccelerationLowerLimit")]
    public double? SprintAccelerationLowerLimit { get; set; }

    [JsonPropertyName("SprintSpeedLowerLimit")]
    public double? SprintSpeedLowerLimit { get; set; }

    [JsonPropertyName("SprintSensitivityLowerLimit")]
    public double? SprintSensitivityLowerLimit { get; set; }

    [JsonPropertyName("AimConsumptionByPose")]
    public XYZ? AimConsumptionByPose { get; set; }

    [JsonPropertyName("RestorationMultiplierByPose")]
    public XYZ? RestorationMultiplierByPose { get; set; }

    [JsonPropertyName("OverweightConsumptionByPose")]
    public XYZ? OverweightConsumptionByPose { get; set; }

    [JsonPropertyName("AimingSpeedMultiplier")]
    public double? AimingSpeedMultiplier { get; set; }

    [JsonPropertyName("WalkVisualEffectMultiplier")]
    public double? WalkVisualEffectMultiplier { get; set; }

    [JsonPropertyName("WeaponFastSwitchConsumption")]
    public double? WeaponFastSwitchConsumption { get; set; }

    [JsonPropertyName("HandsCapacity")]
    public double? HandsCapacity { get; set; }

    [JsonPropertyName("HandsRestoration")]
    public double? HandsRestoration { get; set; }

    [JsonPropertyName("ProneConsumption")]
    public double? ProneConsumption { get; set; }

    [JsonPropertyName("BaseHoldBreathConsumption")]
    public double? BaseHoldBreathConsumption { get; set; }

    [JsonPropertyName("SoundRadius")]
    public XYZ? SoundRadius { get; set; }

    [JsonPropertyName("ExhaustedMeleeSpeed")]
    public double? ExhaustedMeleeSpeed { get; set; }

    [JsonPropertyName("FatigueRestorationRate")]
    public double? FatigueRestorationRate { get; set; }

    [JsonPropertyName("FatigueAmountToCreateEffect")]
    public double? FatigueAmountToCreateEffect { get; set; }

    [JsonPropertyName("ExhaustedMeleeDamageMultiplier")]
    public double? ExhaustedMeleeDamageMultiplier { get; set; }

    [JsonPropertyName("FallDamageMultiplier")]
    public double? FallDamageMultiplier { get; set; }

    [JsonPropertyName("SafeHeightOverweight")]
    public double? SafeHeightOverweight { get; set; }

    [JsonPropertyName("SitToStandConsumption")]
    public double? SitToStandConsumption { get; set; }

    [JsonPropertyName("StaminaExhaustionCausesJiggle")]
    public bool? StaminaExhaustionCausesJiggle { get; set; }

    [JsonPropertyName("StaminaExhaustionStartsBreathSound")]
    public bool? StaminaExhaustionStartsBreathSound { get; set; }

    [JsonPropertyName("StaminaExhaustionRocksCamera")]
    public bool? StaminaExhaustionRocksCamera { get; set; }

    [JsonPropertyName("HoldBreathStaminaMultiplier")]
    public XYZ? HoldBreathStaminaMultiplier { get; set; }

    [JsonPropertyName("PoseLevelIncreaseSpeed")]
    public XYZ? PoseLevelIncreaseSpeed { get; set; }

    [JsonPropertyName("PoseLevelDecreaseSpeed")]
    public XYZ? PoseLevelDecreaseSpeed { get; set; }

    [JsonPropertyName("PoseLevelConsumptionPerNotch")]
    public XYZ? PoseLevelConsumptionPerNotch { get; set; }

    public XYZ? ClimbLegsConsumption { get; set; }
    public XYZ? ClimbOneHandConsumption { get; set; }
    public XYZ? ClimbTwoHandsConsumption { get; set; }
    public XYZ? VaultLegsConsumption { get; set; }
    public XYZ? VaultOneHandConsumption { get; set; }
}

public class StaminaRestoration
{
    [JsonPropertyName("LowerLeftPoint")]
    public double? LowerLeftPoint { get; set; }

    [JsonPropertyName("LowerRightPoint")]
    public double? LowerRightPoint { get; set; }

    [JsonPropertyName("LeftPlatoPoint")]
    public double? LeftPlatoPoint { get; set; }

    [JsonPropertyName("RightPlatoPoint")]
    public double? RightPlatoPoint { get; set; }

    [JsonPropertyName("RightLimit")]
    public double? RightLimit { get; set; }

    [JsonPropertyName("ZeroValue")]
    public double? ZeroValue { get; set; }
}

public class StaminaDrain
{
    [JsonPropertyName("LowerLeftPoint")]
    public double? LowerLeftPoint { get; set; }

    [JsonPropertyName("LowerRightPoint")]
    public double? LowerRightPoint { get; set; }

    [JsonPropertyName("LeftPlatoPoint")]
    public double? LeftPlatoPoint { get; set; }

    [JsonPropertyName("RightPlatoPoint")]
    public double? RightPlatoPoint { get; set; }

    [JsonPropertyName("RightLimit")]
    public double? RightLimit { get; set; }

    [JsonPropertyName("ZeroValue")]
    public double? ZeroValue { get; set; }
}

public class RequirementReferences
{
    [JsonPropertyName("Alpinist")]
    public List<Alpinist>? Alpinists { get; set; }
}

public class Alpinist
{
    [JsonPropertyName("Requirement")]
    public string? Requirement { get; set; }

    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Count")]
    public double? Count { get; set; }

    [JsonPropertyName("RequiredSlot")]
    public string? RequiredSlot { get; set; }

    [JsonPropertyName("RequirementTip")]
    public string? RequirementTip { get; set; }
}

public class RestrictionsInRaid
{
    [JsonPropertyName("MaxInLobby")]
    public double? MaxInLobby { get; set; }

    [JsonPropertyName("MaxInRaid")]
    public double? MaxInRaid { get; set; }

    [JsonPropertyName("TemplateId")]
    public string? TemplateId { get; set; }
}

public class FavoriteItemsSettings
{
    [JsonPropertyName("WeaponStandMaxItemsCount")]
    public double? WeaponStandMaxItemsCount { get; set; }

    [JsonPropertyName("PlaceOfFameMaxItemsCount")]
    public double? PlaceOfFameMaxItemsCount { get; set; }
}

public class VaultingSettings
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("VaultingInputTime")]
    public double? VaultingInputTime { get; set; }

    [JsonPropertyName("GridSettings")]
    public VaultingGridSettings? GridSettings { get; set; }

    [JsonPropertyName("MovesSettings")]
    public VaultingMovesSettings? MovesSettings { get; set; }
}

public class VaultingGridSettings
{
    [JsonPropertyName("GridSizeX")]
    public double? GridSizeX { get; set; }

    [JsonPropertyName("GridSizeY")]
    public double? GridSizeY { get; set; }

    [JsonPropertyName("GridSizeZ")]
    public double? GridSizeZ { get; set; }

    [JsonPropertyName("SteppingLengthX")]
    public double? SteppingLengthX { get; set; }

    [JsonPropertyName("SteppingLengthY")]
    public double? SteppingLengthY { get; set; }

    [JsonPropertyName("SteppingLengthZ")]
    public double? SteppingLengthZ { get; set; }

    [JsonPropertyName("GridOffsetX")]
    public double? GridOffsetX { get; set; }

    [JsonPropertyName("GridOffsetY")]
    public double? GridOffsetY { get; set; }

    [JsonPropertyName("GridOffsetZ")]
    public double? GridOffsetZ { get; set; }

    [JsonPropertyName("OffsetFactor")]
    public double? OffsetFactor { get; set; }
}

public class VaultingMovesSettings
{
    [JsonPropertyName("VaultSettings")]
    public VaultingSubMoveSettings? VaultSettings { get; set; }

    [JsonPropertyName("ClimbSettings")]
    public VaultingSubMoveSettings? ClimbSettings { get; set; }
}

public class VaultingSubMoveSettings
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("MaxWithoutHandHeight")]
    public double? MaxWithoutHandHeight { get; set; }

    public double? MaxOneHandHeight { get; set; }

    [JsonPropertyName("SpeedRange")]
    public XYZ? SpeedRange { get; set; }

    [JsonPropertyName("MoveRestrictions")]
    public MoveRestrictions? MoveRestrictions { get; set; }

    [JsonPropertyName("AutoMoveRestrictions")]
    public MoveRestrictions? AutoMoveRestrictions { get; set; }
}

public class MoveRestrictions
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive { get; set; }

    [JsonPropertyName("MinDistantToInteract")]
    public double? MinDistantToInteract { get; set; }

    [JsonPropertyName("MinHeight")]
    public double? MinHeight { get; set; }

    [JsonPropertyName("MaxHeight")]
    public double? MaxHeight { get; set; }

    [JsonPropertyName("MinLength")]
    public double? MinLength { get; set; }

    [JsonPropertyName("MaxLength")]
    public double? MaxLength { get; set; }
}

public class BTRSettings
{
    [JsonPropertyName("LocationsWithBTR")]
    public List<string>? LocationsWithBTR { get; set; }

    [JsonPropertyName("BasePriceTaxi")]
    public double? BasePriceTaxi { get; set; }

    [JsonPropertyName("AddPriceTaxi")]
    public double? AddPriceTaxi { get; set; }

    [JsonPropertyName("CleanUpPrice")]
    public double? CleanUpPrice { get; set; }

    [JsonPropertyName("DeliveryPrice")]
    public double? DeliveryPrice { get; set; }

    [JsonPropertyName("ModDeliveryCost")]
    public double? ModDeliveryCost { get; set; }

    [JsonPropertyName("BearPriceMod")]
    public double? BearPriceMod { get; set; }

    [JsonPropertyName("UsecPriceMod")]
    public double? UsecPriceMod { get; set; }

    [JsonPropertyName("ScavPriceMod")]
    public double? ScavPriceMod { get; set; }

    [JsonPropertyName("CoefficientDiscountCharisma")]
    public double? CoefficientDiscountCharisma { get; set; }

    [JsonPropertyName("DeliveryMinPrice")]
    public double? DeliveryMinPrice { get; set; }

    [JsonPropertyName("TaxiMinPrice")]
    public double? TaxiMinPrice { get; set; }

    [JsonPropertyName("BotCoverMinPrice")]
    public double? BotCoverMinPrice { get; set; }

    [JsonPropertyName("MapsConfigs")]
    public Dictionary<string, BtrMapConfig>? MapsConfigs { get; set; }

    [JsonPropertyName("DiameterWheel")]
    public double? DiameterWheel { get; set; }

    [JsonPropertyName("HeightWheel")]
    public double? HeightWheel { get; set; }

    [JsonPropertyName("HeightWheelMaxPosLimit")]
    public double? HeightWheelMaxPosLimit { get; set; }

    [JsonPropertyName("HeightWheelMinPosLimit")]
    public double? HeightWheelMinPosLimit { get; set; }

    [JsonPropertyName("SnapToSurfaceWheelsSpeed")]
    public double? SnapToSurfaceWheelsSpeed { get; set; }

    [JsonPropertyName("CheckSurfaceForWheelsTimer")]
    public double? CheckSurfaceForWheelsTimer { get; set; }

    [JsonPropertyName("HeightWheelOffset")]
    public double? HeightWheelOffset { get; set; }
}

public class BtrMapConfig
{
    [JsonPropertyName("BtrSkin")]
    public string? BtrSkin { get; set; }

    [JsonPropertyName("CheckSurfaceForWheelsTimer")]
    public double? CheckSurfaceForWheelsTimer { get; set; }

    [JsonPropertyName("DiameterWheel")]
    public double? DiameterWheel { get; set; }

    [JsonPropertyName("HeightWheel")]
    public double? HeightWheel { get; set; }

    [JsonPropertyName("HeightWheelMaxPosLimit")]
    public double? HeightWheelMaxPosLimit { get; set; }

    [JsonPropertyName("HeightWheelMinPosLimit")]
    public double? HeightWheelMinPosLimit { get; set; }

    [JsonPropertyName("HeightWheelOffset")]
    public double? HeightWheelOffset { get; set; }

    [JsonPropertyName("SnapToSurfaceWheelsSpeed")]
    public double? SnapToSurfaceWheelsSpeed { get; set; }

    [JsonPropertyName("SuspensionDamperStiffness")]
    public double? SuspensionDamperStiffness { get; set; }

    [JsonPropertyName("SuspensionRestLength")]
    public double? SuspensionRestLength { get; set; }

    [JsonPropertyName("SuspensionSpringStiffness")]
    public double? SuspensionSpringStiffness { get; set; }

    [JsonPropertyName("SuspensionTravel")]
    public double? SuspensionTravel { get; set; }

    [JsonPropertyName("SuspensionWheelRadius")]
    public double? SuspensionWheelRadius { get; set; }

    [JsonPropertyName("mapID")]
    public string? MapID { get; set; }

    [JsonPropertyName("pathsConfigurations")]
    public List<PathConfig>? PathsConfigurations { get; set; }
}

public class PathConfig
{
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("enterPoint")]
    public string? EnterPoint { get; set; }

    [JsonPropertyName("exitPoint")]
    public string? ExitPoint { get; set; }

    [JsonPropertyName("pathPoints")]
    public List<string>? PathPoints { get; set; }

    [JsonPropertyName("once")]
    public bool? Once { get; set; }

    [JsonPropertyName("circle")]
    public bool? Circle { get; set; }

    [JsonPropertyName("circleCount")]
    public double? CircleCount { get; set; }

    [JsonPropertyName("skinType")]
    public List<string>? SkinType { get; set; }
}

public class SquadSettings
{
    [JsonPropertyName("CountOfRequestsToOnePlayer")]
    public double? CountOfRequestsToOnePlayer { get; set; }

    [JsonPropertyName("SecondsForExpiredRequest")]
    public double? SecondsForExpiredRequest { get; set; }

    [JsonPropertyName("SendRequestDelaySeconds")]
    public double? SendRequestDelaySeconds { get; set; }
}

public class Insurance
{
    [JsonPropertyName("ChangeForReturnItemsInOfflineRaid")]
    public double? ChangeForReturnItemsInOfflineRaid { get; set; }

    [JsonPropertyName("MaxStorageTimeInHour")]
    public double? MaxStorageTimeInHour { get; set; }

    [JsonPropertyName("CoefOfSendingMessageTime")]
    public double? CoefOfSendingMessageTime { get; set; }

    [JsonPropertyName("CoefOfHavingMarkOfUnknown")]
    public double? CoefOfHavingMarkOfUnknown { get; set; }

    [JsonPropertyName("EditionSendingMessageTime")]
    public Dictionary<string, MessageSendTimeMultiplier>? EditionSendingMessageTime { get; set; }

    [JsonPropertyName("OnlyInDeathCase")]
    public bool? OnlyInDeathCase { get; set; }
}

public class MessageSendTimeMultiplier
{
    [JsonPropertyName("multiplier")]
    public double? Multiplier { get; set; }
}

public class SkillsSettings
{
    [JsonPropertyName("SkillProgressRate")]
    public double? SkillProgressRate { get; set; }

    [JsonPropertyName("WeaponSkillProgressRate")]
    public double? WeaponSkillProgressRate { get; set; }

    [JsonPropertyName("WeaponSkillRecoilBonusPerLevel")]
    public double? WeaponSkillRecoilBonusPerLevel { get; set; }

    [JsonPropertyName("HideoutManagement")]
    public HideoutManagement? HideoutManagement { get; set; }

    [JsonPropertyName("Crafting")]
    public Crafting? Crafting { get; set; }

    [JsonPropertyName("Metabolism")]
    public Metabolism? Metabolism { get; set; }

    [JsonPropertyName("MountingErgonomicsBonusPerLevel")]
    public double? MountingErgonomicsBonusPerLevel { get; set; }

    [JsonPropertyName("Immunity")]
    public Immunity? Immunity { get; set; }

    [JsonPropertyName("Endurance")]
    public Endurance? Endurance { get; set; }

    [JsonPropertyName("Strength")]
    public Strength? Strength { get; set; }

    [JsonPropertyName("Vitality")]
    public Vitality? Vitality { get; set; }

    [JsonPropertyName("Health")]
    public HealthSkillProgress? Health { get; set; }

    [JsonPropertyName("StressResistance")]
    public StressResistance? StressResistance { get; set; }

    [JsonPropertyName("Throwing")]
    public Throwing? Throwing { get; set; }

    [JsonPropertyName("RecoilControl")]
    public RecoilControl? RecoilControl { get; set; }

    [JsonPropertyName("Pistol")]
    public WeaponSkills? Pistol { get; set; }

    [JsonPropertyName("Revolver")]
    public WeaponSkills? Revolver { get; set; }

    [JsonPropertyName("SMG")]
    public List<object>? SMG { get; set; }

    [JsonPropertyName("Assault")]
    public WeaponSkills? Assault { get; set; }

    [JsonPropertyName("Shotgun")]
    public WeaponSkills? Shotgun { get; set; }

    [JsonPropertyName("Sniper")]
    public WeaponSkills? Sniper { get; set; }

    [JsonPropertyName("LMG")]
    public List<object>? LMG { get; set; }

    [JsonPropertyName("HMG")]
    public List<object>? HMG { get; set; }

    [JsonPropertyName("Launcher")]
    public List<object>? Launcher { get; set; }

    [JsonPropertyName("AttachedLauncher")]
    public List<object>? AttachedLauncher { get; set; }

    [JsonPropertyName("Melee")]
    public MeleeSkill? Melee { get; set; }

    [JsonPropertyName("DMR")]
    public WeaponSkills? DMR { get; set; }

    [JsonPropertyName("BearAssaultoperations")]
    public List<object>? BearAssaultoperations { get; set; }

    [JsonPropertyName("BearAuthority")]
    public List<object>? BearAuthority { get; set; }

    [JsonPropertyName("BearAksystems")]
    public List<object>? BearAksystems { get; set; }

    [JsonPropertyName("BearHeavycaliber")]
    public List<object>? BearHeavycaliber { get; set; }

    [JsonPropertyName("BearRawpower")]
    public List<object>? BearRawpower { get; set; }

    [JsonPropertyName("BipodErgonomicsBonusPerLevel")]
    public double? BipodErgonomicsBonusPerLevel { get; set; }

    [JsonPropertyName("UsecArsystems")]
    public List<object>? UsecArsystems { get; set; }

    [JsonPropertyName("UsecDeepweaponmodding_Settings")]
    public List<object>? UsecDeepweaponmodding_Settings { get; set; }

    [JsonPropertyName("UsecLongrangeoptics_Settings")]
    public List<object>? UsecLongrangeoptics_Settings { get; set; }

    [JsonPropertyName("UsecNegotiations")]
    public List<object>? UsecNegotiations { get; set; }

    [JsonPropertyName("UsecTactics")]
    public List<object>? UsecTactics { get; set; }

    [JsonPropertyName("BotReload")]
    public List<object>? BotReload { get; set; }

    [JsonPropertyName("CovertMovement")]
    public CovertMovement? CovertMovement { get; set; }

    [JsonPropertyName("FieldMedicine")]
    public List<object>? FieldMedicine { get; set; }

    [JsonPropertyName("Search")]
    public Search? Search { get; set; }

    [JsonPropertyName("Sniping")]
    public List<object>? Sniping { get; set; }

    [JsonPropertyName("ProneMovement")]
    public List<object>? ProneMovement { get; set; }

    [JsonPropertyName("FirstAid")]
    public List<object>? FirstAid { get; set; }

    [JsonPropertyName("LightVests")]
    public ArmorSkills? LightVests { get; set; }

    [JsonPropertyName("HeavyVests")]
    public ArmorSkills? HeavyVests { get; set; }

    [JsonPropertyName("WeaponModding")]
    public List<object>? WeaponModding { get; set; }

    [JsonPropertyName("AdvancedModding")]
    public List<object>? AdvancedModding { get; set; }

    [JsonPropertyName("NightOps")]
    public List<object>? NightOps { get; set; }

    [JsonPropertyName("SilentOps")]
    public List<object>? SilentOps { get; set; }

    [JsonPropertyName("Lockpicking")]
    public List<object>? Lockpicking { get; set; }

    [JsonPropertyName("WeaponTreatment")]
    public WeaponTreatment? WeaponTreatment { get; set; }

    [JsonPropertyName("MagDrills")]
    public MagDrills? MagDrills { get; set; }

    [JsonPropertyName("Freetrading")]
    public List<object>? Freetrading { get; set; }

    [JsonPropertyName("Auctions")]
    public List<object>? Auctions { get; set; }

    [JsonPropertyName("Cleanoperations")]
    public List<object>? Cleanoperations { get; set; }

    [JsonPropertyName("Barter")]
    public List<object>? Barter { get; set; }

    [JsonPropertyName("Shadowconnections")]
    public List<object>? Shadowconnections { get; set; }

    [JsonPropertyName("Taskperformance")]
    public List<object>? Taskperformance { get; set; }

    [JsonPropertyName("Perception")]
    public Perception? Perception { get; set; }

    [JsonPropertyName("Intellect")]
    public Intellect? Intellect { get; set; }

    [JsonPropertyName("Attention")]
    public Attention? Attention { get; set; }

    [JsonPropertyName("Charisma")]
    public Charisma? Charisma { get; set; }

    [JsonPropertyName("Memory")]
    public Memory? Memory { get; set; }

    [JsonPropertyName("Surgery")]
    public Surgery? Surgery { get; set; }

    [JsonPropertyName("AimDrills")]
    public AimDrills? AimDrills { get; set; }

    [JsonPropertyName("BotSound")]
    public List<object>? BotSound { get; set; }

    [JsonPropertyName("TroubleShooting")]
    public TroubleShooting? TroubleShooting { get; set; }
}

public class MeleeSkill
{
    public BuffSettings? BuffSettings { get; set; }
}

public class ArmorSkills
{
    public double? BluntThroughputDamageHVestsReducePerLevel { get; set; }
    public double? WearAmountRepairHVestsReducePerLevel { get; set; }
    public double? WearChanceRepairHVestsReduceEliteLevel { get; set; }
    public double? BuffMaxCount { get; set; }
    public BuffSettings? BuffSettings { get; set; }
    public ArmorCounters? Counters { get; set; }
    public double? MoveSpeedPenaltyReductionHVestsReducePerLevel { get; set; }
    public double? RicochetChanceHVestsCurrentDurabilityThreshold { get; set; }
    public double? RicochetChanceHVestsEliteLevel { get; set; }
    public double? RicochetChanceHVestsMaxDurabilityThreshold { get; set; }
    public double? MeleeDamageLVestsReducePerLevel { get; set; }
    public double? MoveSpeedPenaltyReductionLVestsReducePerLevel { get; set; }
    public double? WearAmountRepairLVestsReducePerLevel { get; set; }
    public double? WearChanceRepairLVestsReduceEliteLevel { get; set; }
}

public class ArmorCounters
{
    [JsonPropertyName("armorDurability")]
    public SkillCounter? ArmorDurability { get; set; }
}

public class HideoutManagement
{
    public double? SkillPointsPerAreaUpgrade { get; set; }
    public double? SkillPointsPerCraft { get; set; }
    public double? CircleOfCultistsBonusPercent { get; set; }
    public double? ConsumptionReductionPerLevel { get; set; }
    public double? SkillBoostPercent { get; set; }
    public SkillPointsRate? SkillPointsRate { get; set; }
    public EliteSlots? EliteSlots { get; set; }
}

public class SkillPointsRate
{
    public SkillPointRate? Generator { get; set; }
    public SkillPointRate? AirFilteringUnit { get; set; }
    public SkillPointRate? WaterCollector { get; set; }
    public SkillPointRate? SolarPower { get; set; }
}

public class SkillPointRate
{
    public double? ResourceSpent { get; set; }
    public double? PointsGained { get; set; }
}

public class EliteSlots
{
    public EliteSlot? Generator { get; set; }
    public EliteSlot? AirFilteringUnit { get; set; }
    public EliteSlot? WaterCollector { get; set; }
    public EliteSlot? BitcoinFarm { get; set; }
}

public class EliteSlot
{
    public double? Slots { get; set; }
    public double? Container { get; set; }
}

public class Crafting
{
    [JsonPropertyName("DependentSkillRatios")]
    public List<DependentSkillRatio>? DependentSkillRatios { get; set; }

    [JsonPropertyName("PointsPerCraftingCycle")]
    public double? PointsPerCraftingCycle { get; set; }

    [JsonPropertyName("CraftingCycleHours")]
    public double? CraftingCycleHours { get; set; }

    [JsonPropertyName("PointsPerUniqueCraftCycle")]
    public double? PointsPerUniqueCraftCycle { get; set; }

    [JsonPropertyName("UniqueCraftsPerCycle")]
    public double? UniqueCraftsPerCycle { get; set; }

    [JsonPropertyName("CraftTimeReductionPerLevel")]
    public double? CraftTimeReductionPerLevel { get; set; }

    [JsonPropertyName("ProductionTimeReductionPerLevel")]
    public double? ProductionTimeReductionPerLevel { get; set; }

    [JsonPropertyName("EliteExtraProductions")]
    public double? EliteExtraProductions { get; set; }

    // Yes, there is a typo
    [JsonPropertyName("CraftingPointsToInteligence")]
    public double? CraftingPointsToIntelligence { get; set; }
}

public class Metabolism
{
    [JsonPropertyName("HydrationRecoveryRate")]
    public double? HydrationRecoveryRate { get; set; }

    [JsonPropertyName("EnergyRecoveryRate")]
    public double? EnergyRecoveryRate { get; set; }

    [JsonPropertyName("IncreasePositiveEffectDurationRate")]
    public double? IncreasePositiveEffectDurationRate { get; set; }

    [JsonPropertyName("DecreaseNegativeEffectDurationRate")]
    public double? DecreaseNegativeEffectDurationRate { get; set; }

    [JsonPropertyName("DecreasePoisonDurationRate")]
    public double? DecreasePoisonDurationRate { get; set; }
}

public class Immunity
{
    [JsonPropertyName("ImmunityMiscEffects")]
    public double? ImmunityMiscEffects { get; set; }

    [JsonPropertyName("ImmunityPoisonBuff")]
    public double? ImmunityPoisonBuff { get; set; }

    [JsonPropertyName("ImmunityPainKiller")]
    public double? ImmunityPainKiller { get; set; }

    [JsonPropertyName("HealthNegativeEffect")]
    public double? HealthNegativeEffect { get; set; }

    [JsonPropertyName("StimulatorNegativeBuff")]
    public double? StimulatorNegativeBuff { get; set; }
}

public class Endurance
{
    [JsonPropertyName("MovementAction")]
    public double? MovementAction { get; set; }

    [JsonPropertyName("SprintAction")]
    public double? SprintAction { get; set; }

    [JsonPropertyName("GainPerFatigueStack")]
    public double? GainPerFatigueStack { get; set; }

    [JsonPropertyName("DependentSkillRatios")]
    public List<DependentSkillRatio>? DependentSkillRatios { get; set; }

    [JsonPropertyName("QTELevelMultipliers")]
    public Dictionary<string, Dictionary<string, double>>? QTELevelMultipliers { get; set; }
}

public class Strength
{
    [JsonPropertyName("DependentSkillRatios")]
    public List<DependentSkillRatio>? DependentSkillRatios { get; set; }

    [JsonPropertyName("SprintActionMin")]
    public double? SprintActionMin { get; set; }

    [JsonPropertyName("SprintActionMax")]
    public double? SprintActionMax { get; set; }

    [JsonPropertyName("MovementActionMin")]
    public double? MovementActionMin { get; set; }

    [JsonPropertyName("MovementActionMax")]
    public double? MovementActionMax { get; set; }

    [JsonPropertyName("PushUpMin")]
    public double? PushUpMin { get; set; }

    [JsonPropertyName("PushUpMax")]
    public double? PushUpMax { get; set; }

    [JsonPropertyName("QTELevelMultipliers")]
    public List<QTELevelMultiplier>? QTELevelMultipliers { get; set; }

    [JsonPropertyName("FistfightAction")]
    public double? FistfightAction { get; set; }

    [JsonPropertyName("ThrowAction")]
    public double? ThrowAction { get; set; }
}

public class DependentSkillRatio
{
    [JsonPropertyName("Ratio")]
    public double? Ratio { get; set; }

    [JsonPropertyName("SkillId")]
    public string? SkillId { get; set; }
}

public class QTELevelMultiplier
{
    [JsonPropertyName("Level")]
    public double? Level { get; set; }

    [JsonPropertyName("Multiplier")]
    public double? Multiplier { get; set; }
}

public class Vitality
{
    [JsonPropertyName("DamageTakenAction")]
    public double? DamageTakenAction { get; set; }

    [JsonPropertyName("HealthNegativeEffect")]
    public double? HealthNegativeEffect { get; set; }
}

public class HealthSkillProgress
{
    [JsonPropertyName("SkillProgress")]
    public double? SkillProgress { get; set; }
}

public class StressResistance
{
    [JsonPropertyName("HealthNegativeEffect")]
    public double? HealthNegativeEffect { get; set; }

    [JsonPropertyName("LowHPDuration")]
    public double? LowHPDuration { get; set; }
}

public class Throwing
{
    [JsonPropertyName("ThrowAction")]
    public double? ThrowAction { get; set; }
}

public class RecoilControl
{
    [JsonPropertyName("RecoilAction")]
    public double? RecoilAction { get; set; }

    [JsonPropertyName("RecoilBonusPerLevel")]
    public double? RecoilBonusPerLevel { get; set; }
}

public class WeaponSkills
{
    [JsonPropertyName("WeaponReloadAction")]
    public double? WeaponReloadAction { get; set; }

    [JsonPropertyName("WeaponShotAction")]
    public double? WeaponShotAction { get; set; }

    [JsonPropertyName("WeaponFixAction")]
    public double? WeaponFixAction { get; set; }

    [JsonPropertyName("WeaponChamberAction")]
    public double? WeaponChamberAction { get; set; }
}

public class CovertMovement
{
    [JsonPropertyName("MovementAction")]
    public double? MovementAction { get; set; }
}

public class Search
{
    [JsonPropertyName("SearchAction")]
    public double? SearchAction { get; set; }

    [JsonPropertyName("FindAction")]
    public double? FindAction { get; set; }
}

public class WeaponTreatment
{
    [JsonPropertyName("BuffMaxCount")]
    public double? BuffMaxCount { get; set; }

    [JsonPropertyName("BuffSettings")]
    public BuffSettings? BuffSettings { get; set; }

    [JsonPropertyName("Counters")]
    public WeaponTreatmentCounters? Counters { get; set; }

    [JsonPropertyName("DurLossReducePerLevel")]
    public double? DurLossReducePerLevel { get; set; }

    [JsonPropertyName("SkillPointsPerRepair")]
    public double? SkillPointsPerRepair { get; set; }

    [JsonPropertyName("Filter")]
    public List<object>? Filter { get; set; }

    [JsonPropertyName("WearAmountRepairGunsReducePerLevel")]
    public double? WearAmountRepairGunsReducePerLevel { get; set; }

    [JsonPropertyName("WearChanceRepairGunsReduceEliteLevel")]
    public double? WearChanceRepairGunsReduceEliteLevel { get; set; }
}

public class WeaponTreatmentCounters
{
    [JsonPropertyName("firearmsDurability")]
    public SkillCounter? FirearmsDurability { get; set; }
}

public class BuffSettings
{
    [JsonPropertyName("CommonBuffChanceLevelBonus")]
    public double? CommonBuffChanceLevelBonus { get; set; }

    [JsonPropertyName("CommonBuffMinChanceValue")]
    public double? CommonBuffMinChanceValue { get; set; }

    [JsonPropertyName("CurrentDurabilityLossToRemoveBuff")]
    public double? CurrentDurabilityLossToRemoveBuff { get; set; }

    [JsonPropertyName("MaxDurabilityLossToRemoveBuff")]
    public double? MaxDurabilityLossToRemoveBuff { get; set; }

    [JsonPropertyName("RareBuffChanceCoff")]
    public double? RareBuffChanceCoff { get; set; }

    [JsonPropertyName("ReceivedDurabilityMaxPercent")]
    public double? ReceivedDurabilityMaxPercent { get; set; }
}

public class MagDrills
{
    [JsonPropertyName("RaidLoadedAmmoAction")]
    public double? RaidLoadedAmmoAction { get; set; }

    [JsonPropertyName("RaidUnloadedAmmoAction")]
    public double? RaidUnloadedAmmoAction { get; set; }

    [JsonPropertyName("MagazineCheckAction")]
    public double? MagazineCheckAction { get; set; }
}

public class Perception
{
    [JsonPropertyName("DependentSkillRatios")]
    public List<SkillRatio>? DependentSkillRatios { get; set; }

    [JsonPropertyName("OnlineAction")]
    public double? OnlineAction { get; set; }

    [JsonPropertyName("UniqueLoot")]
    public double? UniqueLoot { get; set; }
}

public class SkillRatio
{
    [JsonPropertyName("Ratio")]
    public double? Ratio { get; set; }

    [JsonPropertyName("SkillId")]
    public string? SkillId { get; set; }
}

public class Intellect
{
    public SkillRatio[] DependentSkillRatios { get; set; }

    [JsonPropertyName("Counters")]
    public IntellectCounters? Counters { get; set; }

    [JsonPropertyName("ExamineAction")]
    public double? ExamineAction { get; set; }

    [JsonPropertyName("SkillProgress")]
    public double? SkillProgress { get; set; }

    [JsonPropertyName("RepairAction")]
    public double? RepairAction { get; set; }

    [JsonPropertyName("WearAmountReducePerLevel")]
    public double? WearAmountReducePerLevel { get; set; }

    [JsonPropertyName("WearChanceReduceEliteLevel")]
    public double? WearChanceReduceEliteLevel { get; set; }

    [JsonPropertyName("RepairPointsCostReduction")]
    public double? RepairPointsCostReduction { get; set; }
}

public class IntellectCounters
{
    [JsonPropertyName("armorDurability")]
    public SkillCounter? ArmorDurability { get; set; }

    [JsonPropertyName("firearmsDurability")]
    public SkillCounter? FirearmsDurability { get; set; }

    [JsonPropertyName("meleeWeaponDurability")]
    public SkillCounter? MeleeWeaponDurability { get; set; }
}

public class SkillCounter
{
    [JsonPropertyName("divisor")]
    public double? Divisor { get; set; }

    [JsonPropertyName("points")]
    public double? Points { get; set; }
}

public class Attention
{
    [JsonPropertyName("DependentSkillRatios")]
    public SkillRatio[] DependentSkillRatios { get; set; }

    [JsonPropertyName("ExamineWithInstruction")]
    public double? ExamineWithInstruction { get; set; }

    [JsonPropertyName("FindActionFalse")]
    public double? FindActionFalse { get; set; }

    [JsonPropertyName("FindActionTrue")]
    public double? FindActionTrue { get; set; }
}

public class Charisma
{
    [JsonPropertyName("BonusSettings")]
    public BonusSettings? BonusSettings { get; set; }

    [JsonPropertyName("Counters")]
    public CharismaSkillCounters? Counters { get; set; }

    [JsonPropertyName("SkillProgressInt")]
    public double? SkillProgressInt { get; set; }

    [JsonPropertyName("SkillProgressAtn")]
    public double? SkillProgressAtn { get; set; }

    [JsonPropertyName("SkillProgressPer")]
    public double? SkillProgressPer { get; set; }
}

public class CharismaSkillCounters
{
    [JsonPropertyName("insuranceCost")]
    public SkillCounter? InsuranceCost { get; set; }

    [JsonPropertyName("repairCost")]
    public SkillCounter? RepairCost { get; set; }

    [JsonPropertyName("repeatableQuestCompleteCount")]
    public SkillCounter? RepeatableQuestCompleteCount { get; set; }

    [JsonPropertyName("restoredHealthCost")]
    public SkillCounter? RestoredHealthCost { get; set; }

    [JsonPropertyName("scavCaseCost")]
    public SkillCounter? ScavCaseCost { get; set; }
}

public class BonusSettings
{
    [JsonPropertyName("EliteBonusSettings")]
    public EliteBonusSettings? EliteBonusSettings { get; set; }

    [JsonPropertyName("LevelBonusSettings")]
    public LevelBonusSettings? LevelBonusSettings { get; set; }
}

public class EliteBonusSettings
{
    [JsonPropertyName("FenceStandingLossDiscount")]
    public double? FenceStandingLossDiscount { get; set; }

    [JsonPropertyName("RepeatableQuestExtraCount")]
    public double? RepeatableQuestExtraCount { get; set; }

    [JsonPropertyName("ScavCaseDiscount")]
    public double? ScavCaseDiscount { get; set; }
}

public class LevelBonusSettings
{
    [JsonPropertyName("HealthRestoreDiscount")]
    public double? HealthRestoreDiscount { get; set; }

    [JsonPropertyName("HealthRestoreTraderDiscount")]
    public double? HealthRestoreTraderDiscount { get; set; }

    [JsonPropertyName("InsuranceDiscount")]
    public double? InsuranceDiscount { get; set; }

    [JsonPropertyName("InsuranceTraderDiscount")]
    public double? InsuranceTraderDiscount { get; set; }

    [JsonPropertyName("PaidExitDiscount")]
    public double? PaidExitDiscount { get; set; }

    [JsonPropertyName("RepeatableQuestChangeDiscount")]
    public double? RepeatableQuestChangeDiscount { get; set; }
}

public class Memory
{
    [JsonPropertyName("AnySkillUp")]
    public double? AnySkillUp { get; set; }

    [JsonPropertyName("SkillProgress")]
    public double? SkillProgress { get; set; }
}

public class Surgery
{
    [JsonPropertyName("SurgeryAction")]
    public double? SurgeryAction { get; set; }

    [JsonPropertyName("SkillProgress")]
    public double? SkillProgress { get; set; }
}

public class AimDrills
{
    [JsonPropertyName("WeaponShotAction")]
    public double? WeaponShotAction { get; set; }
}

public class TroubleShooting
{
    [JsonPropertyName("MalfRepairSpeedBonusPerLevel")]
    public double? MalfRepairSpeedBonusPerLevel { get; set; }

    [JsonPropertyName("SkillPointsPerMalfFix")]
    public double? SkillPointsPerMalfFix { get; set; }

    [JsonPropertyName("EliteDurabilityChanceReduceMult")]
    public double? EliteDurabilityChanceReduceMult { get; set; }

    [JsonPropertyName("EliteAmmoChanceReduceMult")]
    public double? EliteAmmoChanceReduceMult { get; set; }

    [JsonPropertyName("EliteMagChanceReduceMult")]
    public double? EliteMagChanceReduceMult { get; set; }
}

public class Aiming
{
    [JsonPropertyName("ProceduralIntensityByPose")]
    public XYZ? ProceduralIntensityByPose { get; set; }

    [JsonPropertyName("AimProceduralIntensity")]
    public double? AimProceduralIntensity { get; set; }

    [JsonPropertyName("HeavyWeight")]
    public double? HeavyWeight { get; set; }

    [JsonPropertyName("LightWeight")]
    public double? LightWeight { get; set; }

    [JsonPropertyName("MaxTimeHeavy")]
    public double? MaxTimeHeavy { get; set; }

    [JsonPropertyName("MinTimeHeavy")]
    public double? MinTimeHeavy { get; set; }

    [JsonPropertyName("MaxTimeLight")]
    public double? MaxTimeLight { get; set; }

    [JsonPropertyName("MinTimeLight")]
    public double? MinTimeLight { get; set; }

    [JsonPropertyName("RecoilScaling")]
    public double? RecoilScaling { get; set; }

    [JsonPropertyName("RecoilDamping")]
    public double? RecoilDamping { get; set; }

    [JsonPropertyName("CameraSnapGlobalMult")]
    public double? CameraSnapGlobalMult { get; set; }

    [JsonPropertyName("RecoilXIntensityByPose")]
    public XYZ? RecoilXIntensityByPose { get; set; }

    [JsonPropertyName("RecoilYIntensityByPose")]
    public XYZ? RecoilYIntensityByPose { get; set; }

    [JsonPropertyName("RecoilZIntensityByPose")]
    public XYZ? RecoilZIntensityByPose { get; set; }

    [JsonPropertyName("RecoilCrank")]
    public bool? RecoilCrank { get; set; }

    [JsonPropertyName("RecoilHandDamping")]
    public double? RecoilHandDamping { get; set; }

    [JsonPropertyName("RecoilConvergenceMult")]
    public double? RecoilConvergenceMult { get; set; }

    [JsonPropertyName("RecoilVertBonus")]
    public double? RecoilVertBonus { get; set; }

    [JsonPropertyName("RecoilBackBonus")]
    public double? RecoilBackBonus { get; set; }
}

public class Malfunction
{
    [JsonPropertyName("AmmoMalfChanceMult")]
    public double? AmmoMalfChanceMult { get; set; }

    [JsonPropertyName("MagazineMalfChanceMult")]
    public double? MagazineMalfChanceMult { get; set; }

    [JsonPropertyName("MalfRepairHardSlideMult")]
    public double? MalfRepairHardSlideMult { get; set; }

    [JsonPropertyName("MalfRepairOneHandBrokenMult")]
    public double? MalfRepairOneHandBrokenMult { get; set; }

    [JsonPropertyName("MalfRepairTwoHandsBrokenMult")]
    public double? MalfRepairTwoHandsBrokenMult { get; set; }

    [JsonPropertyName("AllowMalfForBots")]
    public bool? AllowMalfForBots { get; set; }

    [JsonPropertyName("ShowGlowAttemptsCount")]
    public double? ShowGlowAttemptsCount { get; set; }

    [JsonPropertyName("OutToIdleSpeedMultForPistol")]
    public double? OutToIdleSpeedMultForPistol { get; set; }

    [JsonPropertyName("IdleToOutSpeedMultOnMalf")]
    public double? IdleToOutSpeedMultOnMalf { get; set; }

    [JsonPropertyName("TimeToQuickdrawPistol")]
    public double? TimeToQuickdrawPistol { get; set; }

    [JsonPropertyName("DurRangeToIgnoreMalfs")]
    public XYZ? DurRangeToIgnoreMalfs { get; set; }

    [JsonPropertyName("DurFeedWt")]
    public double? DurFeedWt { get; set; }

    [JsonPropertyName("DurMisfireWt")]
    public double? DurMisfireWt { get; set; }

    [JsonPropertyName("DurJamWt")]
    public double? DurJamWt { get; set; }

    [JsonPropertyName("DurSoftSlideWt")]
    public double? DurSoftSlideWt { get; set; }

    [JsonPropertyName("DurHardSlideMinWt")]
    public double? DurHardSlideMinWt { get; set; }

    [JsonPropertyName("DurHardSlideMaxWt")]
    public double? DurHardSlideMaxWt { get; set; }

    [JsonPropertyName("AmmoMisfireWt")]
    public double? AmmoMisfireWt { get; set; }

    [JsonPropertyName("AmmoFeedWt")]
    public double? AmmoFeedWt { get; set; }

    [JsonPropertyName("AmmoJamWt")]
    public double? AmmoJamWt { get; set; }

    [JsonPropertyName("OverheatFeedWt")]
    public double? OverheatFeedWt { get; set; }

    [JsonPropertyName("OverheatJamWt")]
    public double? OverheatJamWt { get; set; }

    [JsonPropertyName("OverheatSoftSlideWt")]
    public double? OverheatSoftSlideWt { get; set; }

    [JsonPropertyName("OverheatHardSlideMinWt")]
    public double? OverheatHardSlideMinWt { get; set; }

    [JsonPropertyName("OverheatHardSlideMaxWt")]
    public double? OverheatHardSlideMaxWt { get; set; }
}

public class Overheat
{
    [JsonPropertyName("MinOverheat")]
    public double? MinimumOverheat { get; set; }

    [JsonPropertyName("MaxOverheat")]
    public double? MaximumOverheat { get; set; }

    [JsonPropertyName("OverheatProblemsStart")]
    public double? OverheatProblemsStart { get; set; }

    [JsonPropertyName("ModHeatFactor")]
    public double? ModificationHeatFactor { get; set; }

    [JsonPropertyName("ModCoolFactor")]
    public double? ModificationCoolFactor { get; set; }

    [JsonPropertyName("MinWearOnOverheat")]
    public double? MinimumWearOnOverheat { get; set; }

    [JsonPropertyName("MaxWearOnOverheat")]
    public double? MaximumWearOnOverheat { get; set; }

    [JsonPropertyName("MinWearOnMaxOverheat")]
    public double? MinimumWearOnMaximumOverheat { get; set; }

    [JsonPropertyName("MaxWearOnMaxOverheat")]
    public double? MaximumWearOnMaximumOverheat { get; set; }

    [JsonPropertyName("OverheatWearLimit")]
    public double? OverheatWearLimit { get; set; }

    [JsonPropertyName("MaxCOIIncreaseMult")]
    public double? MaximumCOIIncreaseMultiplier { get; set; }

    [JsonPropertyName("MinMalfChance")]
    public double? MinimumMalfunctionChance { get; set; }

    [JsonPropertyName("MaxMalfChance")]
    public double? MaximumMalfunctionChance { get; set; }

    [JsonPropertyName("DurReduceMinMult")]
    public double? DurabilityReductionMinimumMultiplier { get; set; }

    [JsonPropertyName("DurReduceMaxMult")]
    public double? DurabilityReductionMaximumMultiplier { get; set; }

    [JsonPropertyName("BarrelMoveRndDuration")]
    public double? BarrelMovementRandomDuration { get; set; }

    [JsonPropertyName("BarrelMoveMaxMult")]
    public double? BarrelMovementMaximumMultiplier { get; set; }

    [JsonPropertyName("FireratePitchMult")]
    public double? FireRatePitchMultiplier { get; set; }

    [JsonPropertyName("FirerateReduceMinMult")]
    public double? FireRateReductionMinimumMultiplier { get; set; }

    [JsonPropertyName("FirerateReduceMaxMult")]
    public double? FireRateReductionMaximumMultiplier { get; set; }

    [JsonPropertyName("FirerateOverheatBorder")]
    public double? FireRateOverheatBorder { get; set; }

    [JsonPropertyName("EnableSlideOnMaxOverheat")]
    public bool? IsSlideEnabledOnMaximumOverheat { get; set; }

    [JsonPropertyName("StartSlideOverheat")]
    public double? StartSlideOverheat { get; set; }

    [JsonPropertyName("FixSlideOverheat")]
    public double? FixSlideOverheat { get; set; }

    [JsonPropertyName("AutoshotMinOverheat")]
    public double? AutoshotMinimumOverheat { get; set; }

    [JsonPropertyName("AutoshotChance")]
    public double? AutoshotChance { get; set; }

    [JsonPropertyName("AutoshotPossibilityDuration")]
    public double? AutoshotPossibilityDuration { get; set; }

    [JsonPropertyName("MaxOverheatCoolCoef")]
    public double? MaximumOverheatCoolCoefficient { get; set; }
}

public class FenceSettings
{
    [JsonPropertyName("FenceId")]
    public string? FenceIdentifier { get; set; }

    [JsonPropertyName("Levels")]
    public Dictionary<string, FenceLevel>? Levels { get; set; }

    [JsonPropertyName("paidExitStandingNumerator")]
    public double? PaidExitStandingNumerator { get; set; }

    public double? PmcBotKillStandingMultiplier { get; set; }
}

public class FenceLevel
{
    [JsonPropertyName("ReachOnMarkOnUnknowns")]
    public bool? CanReachOnMarkOnUnknowns { get; set; }

    [JsonPropertyName("SavageCooldownModifier")]
    public double? SavageCooldownModifier { get; set; }

    [JsonPropertyName("ScavCaseTimeModifier")]
    public double? ScavCaseTimeModifier { get; set; }

    [JsonPropertyName("PaidExitCostModifier")]
    public double? PaidExitCostModifier { get; set; }

    [JsonPropertyName("BotFollowChance")]
    public double? BotFollowChance { get; set; }

    [JsonPropertyName("ScavEquipmentSpawnChanceModifier")]
    public double? ScavEquipmentSpawnChanceModifier { get; set; }

    [JsonPropertyName("TransitGridSize")]
    public XYZ? TransitGridSize { get; set; }

    [JsonPropertyName("PriceModifier")]
    public double? PriceModifier { get; set; }

    [JsonPropertyName("HostileBosses")]
    public bool? AreHostileBossesPresent { get; set; }

    [JsonPropertyName("HostileScavs")]
    public bool? AreHostileScavsPresent { get; set; }

    [JsonPropertyName("ScavAttackSupport")]
    public bool? IsScavAttackSupported { get; set; }

    [JsonPropertyName("ExfiltrationPriceModifier")]
    public double? ExfiltrationPriceModifier { get; set; }

    [JsonPropertyName("AvailableExits")]
    public double? AvailableExits { get; set; }

    [JsonPropertyName("BotApplySilenceChance")]
    public double? BotApplySilenceChance { get; set; }

    [JsonPropertyName("BotGetInCoverChance")]
    public double? BotGetInCoverChance { get; set; }

    [JsonPropertyName("BotHelpChance")]
    public double? BotHelpChance { get; set; }

    [JsonPropertyName("BotSpreadoutChance")]
    public double? BotSpreadoutChance { get; set; }

    [JsonPropertyName("BotStopChance")]
    public double? BotStopChance { get; set; }

    [JsonPropertyName("PriceModTaxi")]
    public double? PriceModifierTaxi { get; set; }

    [JsonPropertyName("PriceModDelivery")]
    public double? PriceModifierDelivery { get; set; }

    [JsonPropertyName("PriceModCleanUp")]
    public double? PriceModifierCleanUp { get; set; }

    [JsonPropertyName("ReactOnMarkOnUnknowns")]
    public bool? ReactOnMarkOnUnknowns { get; set; }

    [JsonPropertyName("ReactOnMarkOnUnknownsPVE")]
    public bool? ReactOnMarkOnUnknownsPVE { get; set; }

    [JsonPropertyName("DeliveryGridSize")]
    public XYZ? DeliveryGridSize { get; set; }

    [JsonPropertyName("CanInteractWithBtr")]
    public bool? CanInteractWithBtr { get; set; }

    [JsonPropertyName("CircleOfCultistsBonusPercent")]
    public double? CircleOfCultistsBonusPercentage { get; set; }
}

public class Inertia
{
    [JsonPropertyName("InertiaLimits")]
    public XYZ? InertiaLimits { get; set; }

    [JsonPropertyName("InertiaLimitsStep")]
    public double? InertiaLimitsStep { get; set; }

    [JsonPropertyName("ExitMovementStateSpeedThreshold")]
    public XYZ? ExitMovementStateSpeedThreshold { get; set; }

    [JsonPropertyName("WalkInertia")]
    public XYZ? WalkInertia { get; set; }

    [JsonPropertyName("FallThreshold")]
    public double? FallThreshold { get; set; }

    [JsonPropertyName("SpeedLimitAfterFallMin")]
    public XYZ? SpeedLimitAfterFallMin { get; set; }

    [JsonPropertyName("SpeedLimitAfterFallMax")]
    public XYZ? SpeedLimitAfterFallMax { get; set; }

    [JsonPropertyName("SpeedLimitDurationMin")]
    public XYZ? SpeedLimitDurationMin { get; set; }

    [JsonPropertyName("SpeedLimitDurationMax")]
    public XYZ? SpeedLimitDurationMax { get; set; }

    [JsonPropertyName("SpeedInertiaAfterJump")]
    public XYZ? SpeedInertiaAfterJump { get; set; }

    [JsonPropertyName("BaseJumpPenaltyDuration")]
    public double? BaseJumpPenaltyDuration { get; set; }

    [JsonPropertyName("DurationPower")]
    public double? DurationPower { get; set; }

    [JsonPropertyName("BaseJumpPenalty")]
    public double? BaseJumpPenalty { get; set; }

    [JsonPropertyName("PenaltyPower")]
    public double? PenaltyPower { get; set; }

    [JsonPropertyName("InertiaTiltCurveMin")]
    public XYZ? InertiaTiltCurveMin { get; set; }

    [JsonPropertyName("InertiaTiltCurveMax")]
    public XYZ? InertiaTiltCurveMax { get; set; }

    [JsonPropertyName("InertiaBackwardCoef")]
    public XYZ? InertiaBackwardCoef { get; set; }

    [JsonPropertyName("TiltInertiaMaxSpeed")]
    public XYZ? TiltInertiaMaxSpeed { get; set; }

    [JsonPropertyName("TiltStartSideBackSpeed")]
    public XYZ? TiltStartSideBackSpeed { get; set; }

    [JsonPropertyName("TiltMaxSideBackSpeed")]
    public XYZ? TiltMaxSideBackSpeed { get; set; }

    [JsonPropertyName("TiltAcceleration")]
    public XYZ? TiltAcceleration { get; set; }

    [JsonPropertyName("AverageRotationFrameSpan")]
    public double? AverageRotationFrameSpan { get; set; }

    [JsonPropertyName("SprintSpeedInertiaCurveMin")]
    public XYZ? SprintSpeedInertiaCurveMin { get; set; }

    [JsonPropertyName("SprintSpeedInertiaCurveMax")]
    public XYZ? SprintSpeedInertiaCurveMax { get; set; }

    [JsonPropertyName("SprintBrakeInertia")]
    public XYZ? SprintBrakeInertia { get; set; }

    [JsonPropertyName("SprintTransitionMotionPreservation")]
    public XYZ? SprintTransitionMotionPreservation { get; set; }

    [JsonPropertyName("WeaponFlipSpeed")]
    public XYZ? WeaponFlipSpeed { get; set; }

    [JsonPropertyName("PreSprintAccelerationLimits")]
    public XYZ? PreSprintAccelerationLimits { get; set; }

    [JsonPropertyName("SprintAccelerationLimits")]
    public XYZ? SprintAccelerationLimits { get; set; }

    [JsonPropertyName("SideTime")]
    public XYZ? SideTime { get; set; }

    [JsonPropertyName("DiagonalTime")]
    public XYZ? DiagonalTime { get; set; }

    [JsonPropertyName("MaxTimeWithoutInput")]
    public XYZ? MaxTimeWithoutInput { get; set; }

    [JsonPropertyName("MinDirectionBlendTime")]
    public double? MinDirectionBlendTime { get; set; }

    [JsonPropertyName("MoveTimeRange")]
    public XYZ? MoveTimeRange { get; set; }

    [JsonPropertyName("ProneDirectionAccelerationRange")]
    public XYZ? ProneDirectionAccelerationRange { get; set; }

    [JsonPropertyName("ProneSpeedAccelerationRange")]
    public XYZ? ProneSpeedAccelerationRange { get; set; }

    [JsonPropertyName("MinMovementAccelerationRangeRight")]
    public XYZ? MinMovementAccelerationRangeRight { get; set; }

    [JsonPropertyName("MaxMovementAccelerationRangeRight")]
    public XYZ? MaxMovementAccelerationRangeRight { get; set; }

    public XYZ? CrouchSpeedAccelerationRange { get; set; }
}

public class Ballistic
{
    [JsonPropertyName("GlobalDamageDegradationCoefficient")]
    public double? GlobalDamageDegradationCoefficient { get; set; }
}

public class RepairSettings
{
    [JsonPropertyName("ItemEnhancementSettings")]
    public ItemEnhancementSettings? ItemEnhancementSettings { get; set; }

    [JsonPropertyName("MinimumLevelToApplyBuff")]
    public double? MinimumLevelToApplyBuff { get; set; }

    [JsonPropertyName("RepairStrategies")]
    public RepairStrategies? RepairStrategies { get; set; }

    [JsonPropertyName("armorClassDivisor")]
    public double? ArmorClassDivisor { get; set; }

    [JsonPropertyName("durabilityPointCostArmor")]
    public double? DurabilityPointCostArmor { get; set; }

    [JsonPropertyName("durabilityPointCostGuns")]
    public double? DurabilityPointCostGuns { get; set; }
}

public class ItemEnhancementSettings
{
    [JsonPropertyName("DamageReduction")]
    public PriceModifier? DamageReduction { get; set; }

    [JsonPropertyName("MalfunctionProtections")]
    public PriceModifier? MalfunctionProtections { get; set; }

    [JsonPropertyName("WeaponSpread")]
    public PriceModifier? WeaponSpread { get; set; }
}

public class PriceModifier
{
    [JsonPropertyName("PriceModifier")]
    public double? PriceModifierValue { get; set; }
}

public class RepairStrategies
{
    [JsonPropertyName("Armor")]
    public RepairStrategy? Armor { get; set; }

    [JsonPropertyName("Firearms")]
    public RepairStrategy? Firearms { get; set; }
}

public class RepairStrategy
{
    [JsonPropertyName("BuffTypes")]
    public List<string>? BuffTypes { get; set; }

    [JsonPropertyName("Filter")]
    public List<string>? Filter { get; set; }
}

public class BotPreset
{
    [JsonPropertyName("UseThis")]
    public bool? UseThis { get; set; }

    [JsonPropertyName("Role")]
    public string? Role { get; set; }

    [JsonPropertyName("BotDifficulty")]
    public string? BotDifficulty { get; set; }

    [JsonPropertyName("VisibleAngle")]
    public double? VisibleAngle { get; set; }

    [JsonPropertyName("VisibleDistance")]
    public double? VisibleDistance { get; set; }

    [JsonPropertyName("ScatteringPerMeter")]
    public double? ScatteringPerMeter { get; set; }

    [JsonPropertyName("HearingSense")]
    public double? HearingSense { get; set; }

    [JsonPropertyName("SCATTERING_DIST_MODIF")]
    public double? ScatteringDistModif { get; set; }

    [JsonPropertyName("MAX_AIMING_UPGRADE_BY_TIME")]
    public double? MaxAimingUpgradeByTime { get; set; }

    [JsonPropertyName("FIRST_CONTACT_ADD_SEC")]
    public double? FirstContactAddSec { get; set; }

    [JsonPropertyName("COEF_IF_MOVE")]
    public double? CoefIfMove { get; set; }
}

public class AudioSettings
{
    [JsonPropertyName("AudioGroupPresets")]
    public List<AudioGroupPreset>? AudioGroupPresets { get; set; }

    [JsonPropertyName("EnvironmentSettings")]
    public EnvironmentSettings? EnvironmentSettings { get; set; }

    [JsonPropertyName("PlayerSettings")]
    public PlayerSettings? PlayerSettings { get; set; }

    [JsonPropertyName("RadioBroadcastSettings")]
    public RadioBroadcastSettings? RadioBroadcastSettings { get; set; }
}

public class AudioGroupPreset
{
    [JsonPropertyName("AngleToAllowBinaural")]
    public double? AngleToAllowBinaural { get; set; }

    [JsonPropertyName("DisabledBinauralByDistance")]
    public bool? DisabledBinauralByDistance { get; set; }

    [JsonPropertyName("DistanceToAllowBinaural")]
    public double? DistanceToAllowBinaural { get; set; }

    [JsonPropertyName("GroupType")]
    public double? GroupType { get; set; }

    [JsonPropertyName("HeightToAllowBinaural")]
    public double? HeightToAllowBinaural { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("OcclusionEnabled")]
    public bool? OcclusionEnabled { get; set; }

    [JsonPropertyName("OcclusionIntensity")]
    public double? OcclusionIntensity { get; set; }

    [JsonPropertyName("OcclusionRolloffScale")]
    public double? OcclusionRolloffScale { get; set; }

    [JsonPropertyName("OverallVolume")]
    public double? OverallVolume { get; set; }
}

public class EnvironmentSettings
{
    [JsonPropertyName("SnowStepsVolumeMultiplier")]
    public double? SnowStepsVolumeMultiplier { get; set; }

    [JsonPropertyName("SurfaceMultipliers")]
    public List<SurfaceMultiplier>? SurfaceMultipliers { get; set; }
}

public class SurfaceMultiplier
{
    [JsonPropertyName("SurfaceType")]
    public string? SurfaceType { get; set; }

    [JsonPropertyName("VolumeMult")]
    public double? VolumeMultiplier { get; set; }
}

public class BotWeaponScattering
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("PriorityScatter1meter")]
    public double? PriorityScatter1Meter { get; set; }

    [JsonPropertyName("PriorityScatter10meter")]
    public double? PriorityScatter10Meter { get; set; }

    [JsonPropertyName("PriorityScatter100meter")]
    public double? PriorityScatter100Meter { get; set; }
}

public class Preset
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_type")]
    public string? Type { get; set; }

    [JsonPropertyName("_changeWeaponName")]
    public bool? ChangeWeaponName { get; set; }

    [JsonPropertyName("_name")]
    public string? Name { get; set; }

    [JsonPropertyName("_parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("_items")]
    public List<Item>? Items { get; set; }

    /** Default presets have this property */
    [JsonPropertyName("_encyclopedia")]
    public string? Encyclopedia { get; set; }
}

public class QuestSettings
{
    [JsonPropertyName("GlobalRewardRepModifierDailyQuestPvE")]
    public double? GlobalRewardRepModifierDailyQuestPvE { get; set; }

    [JsonPropertyName("GlobalRewardRepModifierQuestPvE")]
    public double? GlobalRewardRepModifierQuestPvE { get; set; }
}
