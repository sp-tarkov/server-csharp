using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record TemplateItem
{
    private Dictionary<string, bool>? _blocks;

    private string? _id;

    private string? _name;

    private string? _parent;

    private string _prototype;

    private string? _type;

    [JsonPropertyName("_id")]
    public string? Id
    {
        get { return _id; }
        set { _id = string.Intern(value); }
    }

    [JsonPropertyName("_name")]
    public string? Name
    {
        get { return _name; }
        set { _name = string.Intern(value); }
    }

    [JsonPropertyName("_parent")]
    public string? Parent
    {
        get { return _parent; }
        set { _parent = string.Intern(value); }
    }

    [JsonPropertyName("_type")]
    public string? Type
    {
        get { return _type; }
        set { _type = string.Intern(value); }
    }

    [JsonPropertyName("_props")]
    public Props? Properties { get; set; }

    [JsonPropertyName("_proto")]
    public string? Prototype
    {
        get { return _prototype; }
        set { _prototype = string.Intern(value); }
    }

    /// <summary>
    /// Used for easy access during bot generation to any slot/container this item is blocking.
    /// </summary>
    [JsonIgnore]
    public Dictionary<string, bool> Blocks
    {
        get
        {
            return _blocks ??= new Dictionary<string, bool>()
            {
                { Containers.LeftStance, Properties?.BlockLeftStance ?? false },
                { Containers.Collapsible, Properties?.BlocksCollapsible ?? false },
                { Containers.Earpiece, Properties?.BlocksEarpiece ?? false },
                { Containers.Eyewear, Properties?.BlocksEyewear ?? false },
                { Containers.FaceCover, Properties?.BlocksFaceCover ?? false },
                { Containers.Folding, Properties?.BlocksFolding ?? false },
                { Containers.Headwear, Properties?.BlocksHeadwear ?? false }
            };
        }
    }
}

public record Props
{
    private string _backgroundColor;

    private string _itemSound;
    private string? _metascoreGroup;

    private string? _rarityPvE;

    private string? _unlootableFromSlot;

    [JsonPropertyName("AllowSpawnOnLocations")]
    public List<string>? AllowSpawnOnLocations { get; set; }

    [JsonPropertyName("BeltMagazineRefreshCount")]
    public double? BeltMagazineRefreshCount { get; set; }

    [JsonPropertyName("ChangePriceCoef")]
    public double? ChangePriceCoef { get; set; }

    [JsonPropertyName("FixedPrice")]
    public bool? FixedPrice { get; set; }

    [JsonPropertyName("SendToClient")]
    public bool? SendToClient { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("ShortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("Weight")]
    public double? Weight { get; set; }

    [JsonPropertyName("WeightMultipliers")]
    public object? WeightMultipliers { get; set; }

    [JsonPropertyName("BackgroundColor")]
    public string? BackgroundColor
    {
        get { return _backgroundColor; }
        set { _backgroundColor = string.Intern(value); }
    }

    // Type confirmed via client
    [JsonPropertyName("Width")]
    public int? Width { get; set; }

    // Type confirmed via client
    [JsonPropertyName("Height")]
    public int? Height { get; set; }

    // Type confirmed via client
    [JsonPropertyName("StackMaxSize")]
    public int? StackMaxSize { get; set; }

    // Type confirmed via client
    [JsonPropertyName("Rarity")]
    public LootRarity? Rarity { get; set; }

    [JsonPropertyName("SpawnChance")]
    public double? SpawnChance { get; set; }

    [JsonPropertyName("CreditsPrice")]
    public double? CreditsPrice { get; set; }

    [JsonPropertyName("ItemSound")]
    public string? ItemSound
    {
        get { return _itemSound; }
        set { _itemSound = string.Intern(value); }
    }

    [JsonPropertyName("Prefab")] // TODO: TYPE FUCKERY: can be a Prefab object or empty string or a string
    public Prefab? Prefab { get; set; }

    [JsonPropertyName("UsePrefab")]
    public Prefab? UsePrefab { get; set; }

    [JsonPropertyName("airDropTemplateId")]
    public string? AirDropTemplateId { get; set; }

    [JsonPropertyName("StackObjectsCount")]
    public double? StackObjectsCount { get; set; }

    [JsonPropertyName("NotShownInSlot")]
    public bool? NotShownInSlot { get; set; }

    [JsonPropertyName("ParticleCapacity")]
    public double? ParticleCapacity { get; set; }

    [JsonPropertyName("ParticleDuration")]
    public double? ParticleDuration { get; set; }

    [JsonPropertyName("ParticleSize")]
    public double? ParticleSize { get; set; }

    [JsonPropertyName("ExaminedByDefault")]
    public bool? ExaminedByDefault { get; set; }

    [JsonPropertyName("ExplosionRadius")]
    public double? ExplosionRadius { get; set; }

    [JsonPropertyName("ExplosionStrength")]
    public double? ExplosionStrength { get; set; }

    [JsonPropertyName("ExamineTime")]
    public double? ExamineTime { get; set; }

    [JsonPropertyName("IsUndiscardable")]
    public bool? IsUndiscardable { get; set; }

    [JsonPropertyName("IsUnsaleable")]
    public bool? IsUnsaleable { get; set; }

    [JsonPropertyName("IsUnbuyable")]
    public bool? IsUnbuyable { get; set; }

    [JsonPropertyName("IsUngivable")]
    public bool? IsUngivable { get; set; }

    [JsonPropertyName("IsUnremovable")]
    public bool? IsUnRemovable { get; set; }

    [JsonPropertyName("IsLockedafterEquip")]
    public bool? IsLockedAfterEquip { get; set; }

    [JsonPropertyName("IsSecretExitRequirement")]
    public bool? IsSecretExitRequirement { get; set; }

    [JsonPropertyName("IsRagfairCurrency")]
    public bool? IsRagfairCurrency { get; set; }

    [JsonPropertyName("IsSpecialSlotOnly")]
    public bool? IsSpecialSlotOnly { get; set; }

    [JsonPropertyName("IsStationaryWeapon")]
    public bool? IsStationaryWeapon { get; set; }

    [JsonPropertyName("QuestItem")]
    public bool? QuestItem { get; set; }

    [JsonPropertyName("QuestStashMaxCount")]
    public double? QuestStashMaxCount { get; set; }

    // Type confirmed via client
    [JsonPropertyName("LootExperience")]
    public int? LootExperience { get; set; }

    // Type confirmed via client
    [JsonPropertyName("ExamineExperience")]
    public int? ExamineExperience { get; set; }

    [JsonPropertyName("HideEntrails")]
    public bool? HideEntrails { get; set; }

    [JsonPropertyName("InsuranceDisabled")]
    public bool? InsuranceDisabled { get; set; }

    // Type confirmed via client
    [JsonPropertyName("RepairCost")]
    public int? RepairCost { get; set; }

    // Type confirmed via client
    [JsonPropertyName("RepairSpeed")]
    public int? RepairSpeed { get; set; }

    [JsonPropertyName("ExtraSizeLeft")]
    public int? ExtraSizeLeft { get; set; }

    [JsonPropertyName("ExtraSizeRight")]
    public int? ExtraSizeRight { get; set; }

    [JsonPropertyName("ExtraSizeUp")]
    public int? ExtraSizeUp { get; set; }

    [JsonPropertyName("FlareTypes")]
    public List<string>? FlareTypes { get; set; }

    [JsonPropertyName("ExtraSizeDown")]
    public int? ExtraSizeDown { get; set; }

    [JsonPropertyName("ExtraSizeForceAdd")]
    public bool? ExtraSizeForceAdd { get; set; }

    [JsonPropertyName("MergesWithChildren")]
    public bool? MergesWithChildren { get; set; }

    [JsonPropertyName("MetascoreGroup")]
    public string? MetascoreGroup
    {
        get { return _metascoreGroup; }
        set { _metascoreGroup = value == null ? null : string.Intern(value); }
    }

    [JsonPropertyName("NpcCompressorSendLevel")]
    public double? NpcCompressorSendLevel { get; set; }

    [JsonPropertyName("ObservedPlayerCompressorSendLevel")]
    public double? ObservedPlayerCompressorSendLevel { get; set; }

    [JsonPropertyName("CanSellOnRagfair")]
    public bool? CanSellOnRagfair { get; set; }

    [JsonPropertyName("ComputableUnitDamage")]
    public XY? ComputableUnitDamage { get; set; }

    [JsonPropertyName("ComputableUnitSize")]
    public double? ComputableUnitSize { get; set; }

    [JsonPropertyName("ComputableUnitType")]
    public string? ComputableUnitType { get; set; }

    [JsonPropertyName("CanUnloadAmmoByPlayer")]
    public bool? CanUnloadAmmoByPlayer { get; set; }

    [JsonPropertyName("CanRequireOnRagfair")]
    public bool? CanRequireOnRagfair { get; set; }

    [JsonPropertyName("ConflictingItems")]
    public HashSet<string>? ConflictingItems { get; set; }

    [JsonPropertyName("Unlootable")]
    public bool? Unlootable { get; set; }

    [JsonPropertyName("UnlootableFromSlot")]
    public string? UnlootableFromSlot
    {
        get { return _unlootableFromSlot; }
        set { _unlootableFromSlot = value == null ? null : string.Intern(value); }
    }

    [JsonPropertyName("UnlootableFromSide")]
    public List<PlayerSideMask>? UnlootableFromSide { get; set; }

    // Type confirmed via client
    [JsonPropertyName("AnimationVariantsNumber")]
    public int? AnimationVariantsNumber { get; set; }

    [JsonPropertyName("DiscardingBlock")]
    public bool? DiscardingBlock { get; set; }

    [JsonPropertyName("DropSoundType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemDropSoundType? DropSoundType { get; set; }

    [JsonPropertyName("RagFairCommissionModifier")]
    public double? RagFairCommissionModifier { get; set; }

    [JsonPropertyName("RarityPvE")]
    public string? RarityPvE
    {
        get { return _rarityPvE; }
        set { _rarityPvE = value == null ? null : string.Intern(value); }
    }

    [JsonPropertyName("IsAlwaysAvailableForInsurance")]
    public bool? IsAlwaysAvailableForInsurance { get; set; }

    [JsonPropertyName("DiscardLimit")]
    public double? DiscardLimit { get; set; }

    // Type confirmed via client
    [JsonPropertyName("MaxResource")]
    public int? MaxResource { get; set; }

    [JsonPropertyName("Resource")]
    public double? Resource { get; set; }

    [JsonPropertyName("DogTagQualities")]
    public bool? DogTagQualities { get; set; }

    [JsonPropertyName("Grids")]
    public List<Grid>? Grids { get; set; }

    [JsonPropertyName("Slots")]
    public List<Slot>? Slots { get; set; }

    [JsonPropertyName("CanPutIntoDuringTheRaid")]
    public bool? CanPutIntoDuringTheRaid { get; set; }

    [JsonPropertyName("CantRemoveFromSlotsDuringRaid")]
    public List<EquipmentSlots>? CantRemoveFromSlotsDuringRaid { get; set; }

    [JsonPropertyName("KeyIds")]
    public List<string>? KeyIds { get; set; }

    [JsonPropertyName("TagColor")]
    public double? TagColor { get; set; }

    [JsonPropertyName("TagName")]
    public string? TagName { get; set; }

    [JsonPropertyName("Durability")]
    public double? Durability { get; set; }

    [JsonPropertyName("Accuracy")]
    public double? Accuracy { get; set; }

    [JsonPropertyName("Recoil")]
    public double? Recoil { get; set; }

    [JsonPropertyName("Loudness")]
    public double? Loudness { get; set; }

    [JsonPropertyName("EffectiveDistance")]
    public double? EffectiveDistance { get; set; }

    [JsonPropertyName("Ergonomics")]
    public double? Ergonomics { get; set; }

    [JsonPropertyName("UseAltMountBone")]
    public bool? UseAltMountBone { get; set; }

    [JsonPropertyName("Velocity")]
    public double? Velocity { get; set; }

    [JsonPropertyName("WeaponRecoilSettings")]
    public WeaponRecoilSettings? WeaponRecoilSettings { get; set; }

    [JsonPropertyName("WithAnimatorAiming")]
    public bool? WithAnimatorAiming { get; set; }

    [JsonPropertyName("RaidModdable")]
    public bool? RaidModdable { get; set; }

    [JsonPropertyName("ToolModdable")]
    public bool? ToolModdable { get; set; }

    [JsonPropertyName("UniqueAnimationModID")]
    public double? UniqueAnimationModID { get; set; }

    [JsonPropertyName("BlocksFolding")]
    public bool? BlocksFolding { get; set; }

    [JsonPropertyName("BlocksCollapsible")]
    public bool? BlocksCollapsible { get; set; }

    [JsonPropertyName("IsAnimated")]
    public bool? IsAnimated { get; set; }

    [JsonPropertyName("HasShoulderContact")]
    public bool? HasShoulderContact { get; set; }

    [JsonPropertyName("SightingRange")]
    public double? SightingRange { get; set; }

    [JsonPropertyName("ZoomSensitivity")]
    public double? ZoomSensitivity { get; set; }

    [JsonPropertyName("DoubleActionAccuracyPenaltyMult")]
    public double? DoubleActionAccuracyPenaltyMult { get; set; }

    [JsonPropertyName("ModesCount")]
    public object? ModesCount { get; set; } // TODO: object here

    [JsonPropertyName("DurabilityBurnModificator")]
    public double? DurabilityBurnModificator { get; set; }

    [JsonPropertyName("HeatFactor")]
    public double? HeatFactor { get; set; }

    [JsonPropertyName("CoolFactor")]
    public double? CoolFactor { get; set; }

    [JsonPropertyName("muzzleModType")]
    public string? MuzzleModType { get; set; }

    [JsonPropertyName("CustomAimPlane")]
    public string? CustomAimPlane { get; set; }

    [JsonPropertyName("IsAdjustableOptic")]
    public bool? IsAdjustableOptic { get; set; }

    [JsonPropertyName("MinMaxFov")]
    public XYZ? MinMaxFov { get; set; }

    [JsonPropertyName("sightModType")]
    public string? SightModType { get; set; }

    [JsonPropertyName("SightModesCount")]
    public double? SightModesCount { get; set; }

    [JsonPropertyName("OpticCalibrationDistances")]
    public List<double>? OpticCalibrationDistances { get; set; }

    [JsonPropertyName("ScopesCount")]
    public double? ScopesCount { get; set; }

    [JsonPropertyName("AimSensitivity")]
    public object? AimSensitivity { get; set; } // TODO: object here

    [JsonPropertyName("Zooms")]
    public List<List<double>>? Zooms { get; set; }

    [JsonPropertyName("CalibrationDistances")]
    public List<List<double>>? CalibrationDistances { get; set; }

    [JsonPropertyName("Intensity")]
    public double? Intensity { get; set; }

    [JsonPropertyName("Mask")]
    public string? Mask { get; set; }

    [JsonPropertyName("MaskSize")]
    public double? MaskSize { get; set; }

    [JsonPropertyName("IsMagazineForStationaryWeapon")]
    public bool? IsMagazineForStationaryWeapon { get; set; }

    [JsonPropertyName("NoiseIntensity")]
    public double? NoiseIntensity { get; set; }

    [JsonPropertyName("NoiseScale")]
    public double? NoiseScale { get; set; }

    [JsonPropertyName("Color")]
    public Color? Color { get; set; }

    [JsonPropertyName("DiffuseIntensity")]
    public double? DiffuseIntensity { get; set; }

    [JsonPropertyName("MagazineWithBelt")]
    public bool? MagazineWithBelt { get; set; }

    [JsonPropertyName("HasHinge")]
    public bool? HasHinge { get; set; }

    [JsonPropertyName("RampPalette")]
    public string? RampPalette { get; set; }

    [JsonPropertyName("DepthFade")]
    public double? DepthFade { get; set; }

    [JsonPropertyName("RoughnessCoef")]
    public double? RoughnessCoef { get; set; }

    [JsonPropertyName("SpecularCoef")]
    public double? SpecularCoef { get; set; }

    [JsonPropertyName("MainTexColorCoef")]
    public double? MainTexColorCoef { get; set; }

    [JsonPropertyName("MinimumTemperatureValue")]
    public double? MinimumTemperatureValue { get; set; }

    [JsonPropertyName("RampShift")]
    public double? RampShift { get; set; }

    [JsonPropertyName("HeatMin")]
    public double? HeatMin { get; set; }

    [JsonPropertyName("ColdMax")]
    public double? ColdMax { get; set; }

    [JsonPropertyName("IsNoisy")]
    public bool? IsNoisy { get; set; }

    [JsonPropertyName("IsFpsStuck")]
    public bool? IsFpsStuck { get; set; }

    [JsonPropertyName("IsGlitch")]
    public bool? IsGlitch { get; set; }

    [JsonPropertyName("IsMotionBlurred")]
    public bool? IsMotionBlurred { get; set; }

    [JsonPropertyName("IsPixelated")]
    public bool? IsPixelated { get; set; }

    [JsonPropertyName("PixelationBlockCount")]
    public double? PixelationBlockCount { get; set; }

    [JsonPropertyName("ShiftsAimCamera")]
    public double? ShiftsAimCamera { get; set; }

    [JsonPropertyName("magAnimationIndex")]
    public double? MagAnimationIndex { get; set; }

    [JsonPropertyName("Cartridges")]
    public List<Slot>? Cartridges { get; set; }

    [JsonPropertyName("CanFast")]
    public bool? CanFast { get; set; }

    [JsonPropertyName("CanHit")]
    public bool? CanHit { get; set; }

    [JsonPropertyName("CanAdmin")]
    public bool? CanAdmin { get; set; }

    [JsonPropertyName("LoadUnloadModifier")]
    public double? LoadUnloadModifier { get; set; }

    [JsonPropertyName("CheckTimeModifier")]
    public double? CheckTimeModifier { get; set; }

    [JsonPropertyName("CheckOverride")]
    public double? CheckOverride { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("ReloadMagType")]
    public ReloadMode? ReloadMagType { get; set; }

    [JsonPropertyName("VisibleAmmoRangesString")]
    public string? VisibleAmmoRangesString { get; set; }

    [JsonPropertyName("MalfunctionChance")]
    public double? MalfunctionChance { get; set; }

    [JsonPropertyName("IsShoulderContact")]
    public bool? IsShoulderContact { get; set; }

    [JsonPropertyName("Foldable")]
    public bool? Foldable { get; set; }

    [JsonPropertyName("Retractable")]
    public bool? Retractable { get; set; }

    [JsonPropertyName("SizeReduceRight")]
    public int? SizeReduceRight { get; set; }

    [JsonPropertyName("CenterOfImpact")]
    public double? CenterOfImpact { get; set; }

    [JsonPropertyName("IsSilencer")]
    public bool? IsSilencer { get; set; }

    [JsonPropertyName("DeviationCurve")]
    public double? DeviationCurve { get; set; }

    [JsonPropertyName("DeviationMax")]
    public double? DeviationMax { get; set; }

    [JsonPropertyName("SearchSound")]
    public string? SearchSound { get; set; }

    [JsonPropertyName("BlocksArmorVest")]
    public bool? BlocksArmorVest { get; set; }

    [JsonPropertyName("speedPenaltyPercent")]
    public double? SpeedPenaltyPercent { get; set; }

    [JsonPropertyName("GridLayoutName")]
    public string? GridLayoutName { get; set; }

    [JsonPropertyName("ContainerSpawnChanceModifier")]
    public double? ContainerSpawnChanceModifier { get; set; }

    [JsonPropertyName("SpawnFilter")]
    public List<object>? SpawnFilter { get; set; } // TODO: object here

    [JsonPropertyName("containType")]
    public List<object>? ContainType { get; set; } // TODO: object here

    [JsonPropertyName("sizeWidth")]
    public double? SizeWidth { get; set; }

    [JsonPropertyName("sizeHeight")]
    public double? SizeHeight { get; set; }

    [JsonPropertyName("isSecured")]
    public bool? IsSecured { get; set; }

    [JsonPropertyName("spawnTypes")]
    public string? SpawnTypes { get; set; }

    [JsonPropertyName("lootFilter")]
    public List<object>? LootFilter { get; set; } // TODO: object here

    [JsonPropertyName("spawnRarity")]
    public string? SpawnRarity { get; set; }

    [JsonPropertyName("minCountSpawn")]
    public double? MinCountSpawn { get; set; }

    [JsonPropertyName("maxCountSpawn")]
    public double? MaxCountSpawn { get; set; }

    [JsonPropertyName("openedByKeyID")]
    public List<string>? OpenedByKeyID { get; set; }

    [JsonPropertyName("RigLayoutName")]
    public string? RigLayoutName { get; set; }

    [JsonPropertyName("MaxDurability")]
    public double? MaxDurability { get; set; }

    [JsonPropertyName("armorZone")]
    public List<string>? ArmorZone { get; set; }

    // Type confirmed via client
    [JsonPropertyName("armorClass")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public int? ArmorClass { get; set; }

    [JsonPropertyName("armorColliders")]
    public List<string>? ArmorColliders { get; set; }

    [JsonPropertyName("armorPlateColliders")]
    public List<string>? ArmorPlateColliders { get; set; }

    [JsonPropertyName("bluntDamageReduceFromSoftArmor")]
    public bool? BluntDamageReduceFromSoftArmor { get; set; }

    [JsonPropertyName("mousePenalty")]
    public double? MousePenalty { get; set; }

    [JsonPropertyName("weaponErgonomicPenalty")]
    public double? WeaponErgonomicPenalty { get; set; }

    [JsonPropertyName("BluntThroughput")]
    public double? BluntThroughput { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("ArmorMaterial")]
    public ArmorMaterial? ArmorMaterial { get; set; }

    [JsonPropertyName("ArmorType")]
    public string? ArmorType { get; set; }

    [JsonPropertyName("weapClass")]
    public string? WeapClass { get; set; }

    [JsonPropertyName("weapUseType")]
    public string? WeapUseType { get; set; }

    [JsonPropertyName("ammoCaliber")]
    public string? AmmoCaliber { get; set; }

    [JsonPropertyName("OperatingResource")]
    public double? OperatingResource { get; set; }

    [JsonPropertyName("PostRecoilHorizontalRangeHandRotation")]
    public XYZ? PostRecoilHorizontalRangeHandRotation { get; set; }

    [JsonPropertyName("PostRecoilVerticalRangeHandRotation")]
    public XYZ? PostRecoilVerticalRangeHandRotation { get; set; }

    [JsonPropertyName("ProgressRecoilAngleOnStable")]
    public XYZ? ProgressRecoilAngleOnStable { get; set; }

    [JsonPropertyName("RepairComplexity")]
    public double? RepairComplexity { get; set; }

    [JsonPropertyName("ResetAfterShot")]
    public bool? ResetAfterShot { get; set; }

    [JsonPropertyName("durabSpawnMin")]
    public double? DurabSpawnMin { get; set; }

    [JsonPropertyName("durabSpawnMax")]
    public double? DurabSpawnMax { get; set; }

    [JsonPropertyName("isFastReload")]
    public bool? IsFastReload { get; set; }

    [JsonPropertyName("RecoilForceUp")]
    public double? RecoilForceUp { get; set; }

    [JsonPropertyName("RecoilForceBack")]
    public double? RecoilForceBack { get; set; }

    [JsonPropertyName("RecoilAngle")]
    public double? RecoilAngle { get; set; }

    [JsonPropertyName("RecoilCamera")]
    public double? RecoilCamera { get; set; }

    [JsonPropertyName("RecoilCategoryMultiplierHandRotation")]
    public double? RecoilCategoryMultiplierHandRotation { get; set; }

    [JsonPropertyName("weapFireType")]
    public HashSet<string>? WeapFireType { get; set; }

    [JsonPropertyName("RecolDispersion")]
    public double? RecolDispersion { get; set; }

    [JsonPropertyName("SingleFireRate")]
    public double? SingleFireRate { get; set; }

    [JsonPropertyName("CanQueueSecondShot")]
    public bool? CanQueueSecondShot { get; set; }

    [JsonPropertyName("bFirerate")]
    public double? BFirerate { get; set; }

    [JsonPropertyName("bEffDist")]
    public double? BEffDist { get; set; }

    [JsonPropertyName("bHearDist")]
    public double? BHearDist { get; set; }

    [JsonPropertyName("blockLeftStance")]
    public bool? BlockLeftStance { get; set; }

    [JsonPropertyName("isChamberLoad")]
    public bool? IsChamberLoad { get; set; }

    [JsonPropertyName("chamberAmmoCount")]
    public double? ChamberAmmoCount { get; set; }

    [JsonPropertyName("isBoltCatch")]
    public bool? IsBoltCatch { get; set; }

    [JsonPropertyName("defMagType")]
    public string? DefMagType { get; set; }

    [JsonPropertyName("defAmmo")]
    public string? DefAmmo { get; set; }

    [JsonPropertyName("AdjustCollimatorsToTrajectory")]
    public bool? AdjustCollimatorsToTrajectory { get; set; }

    [JsonPropertyName("ShotgunDispersion")]
    public double? ShotgunDispersion { get; set; }

    [JsonPropertyName("shotgunDispersion")]
    public double? shotgunDispersion { get; set; }

    [JsonPropertyName("Chambers")]
    public List<Slot>? Chambers { get; set; }

    [JsonPropertyName("CameraSnap")]
    public double? CameraSnap { get; set; }

    [JsonPropertyName("CameraToWeaponAngleSpeedRange")]
    public XYZ? CameraToWeaponAngleSpeedRange { get; set; }

    [JsonPropertyName("CameraToWeaponAngleStep")]
    public double? CameraToWeaponAngleStep { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("ReloadMode")]
    public ReloadMode? ReloadMode { get; set; }

    [JsonPropertyName("AimPlane")]
    public double? AimPlane { get; set; }

    [JsonPropertyName("TacticalReloadStiffnes")]
    public XYZ? TacticalReloadStiffnes { get; set; }

    [JsonPropertyName("TacticalReloadFixation")]
    public double? TacticalReloadFixation { get; set; }

    [JsonPropertyName("RecoilCenter")]
    public XYZ? RecoilCenter { get; set; }

    [JsonPropertyName("RotationCenter")]
    public XYZ? RotationCenter { get; set; }

    [JsonPropertyName("RotationCenterNoStock")]
    public XYZ? RotationCenterNoStock { get; set; }

    [JsonPropertyName("ShotsGroupSettings")]
    public List<ShotsGroupSettings>? ShotsGroupSettings { get; set; }

    [JsonPropertyName("FoldedSlot")]
    public string? FoldedSlot { get; set; }

    [JsonPropertyName("ForbidMissingVitalParts")]
    public bool? ForbidMissingVitalParts { get; set; }

    [JsonPropertyName("ForbidNonEmptyContainers")]
    public bool? ForbidNonEmptyContainers { get; set; }

    [JsonPropertyName("CompactHandling")]
    public bool? CompactHandling { get; set; }

    [JsonPropertyName("MinRepairDegradation")]
    public double? MinRepairDegradation { get; set; }

    [JsonPropertyName("MaxRepairDegradation")]
    public double? MaxRepairDegradation { get; set; }

    [JsonPropertyName("IronSightRange")]
    public double? IronSightRange { get; set; }

    [JsonPropertyName("IsBeltMachineGun")]
    public bool? IsBeltMachineGun { get; set; }

    [JsonPropertyName("IsFlareGun")]
    public bool? IsFlareGun { get; set; }

    [JsonPropertyName("IsGrenadeLauncher")]
    public bool? IsGrenadeLauncher { get; set; }

    [JsonPropertyName("IsOneoff")]
    public bool? IsOneoff { get; set; }

    [JsonPropertyName("MustBoltBeOpennedForExternalReload")]
    public bool? MustBoltBeOpennedForExternalReload { get; set; }

    [JsonPropertyName("MustBoltBeOpennedForInternalReload")]
    public bool? MustBoltBeOpennedForInternalReload { get; set; }

    [JsonPropertyName("NoFiremodeOnBoltcatch")]
    public bool? NoFiremodeOnBoltcatch { get; set; }

    [JsonPropertyName("BoltAction")]
    public bool? BoltAction { get; set; }

    [JsonPropertyName("HipAccuracyRestorationDelay")]
    public double? HipAccuracyRestorationDelay { get; set; }

    [JsonPropertyName("HipAccuracyRestorationSpeed")]
    public double? HipAccuracyRestorationSpeed { get; set; }

    [JsonPropertyName("HipInnaccuracyGain")]
    public double? HipInnaccuracyGain { get; set; }

    [JsonPropertyName("ManualBoltCatch")]
    public bool? ManualBoltCatch { get; set; }

    [JsonPropertyName("BurstShotsCount")]
    public double? BurstShotsCount { get; set; }

    [JsonPropertyName("BaseMalfunctionChance")]
    public double? BaseMalfunctionChance { get; set; }

    [JsonPropertyName("AllowJam")]
    public bool? AllowJam { get; set; }

    [JsonPropertyName("AllowFeed")]
    public bool? AllowFeed { get; set; }

    [JsonPropertyName("AllowMisfire")]
    public bool? AllowMisfire { get; set; }

    [JsonPropertyName("AllowSlide")]
    public bool? AllowSlide { get; set; }

    [JsonPropertyName("DurabilityBurnRatio")]
    public double? DurabilityBurnRatio { get; set; }

    [JsonPropertyName("HeatFactorGun")]
    public double? HeatFactorGun { get; set; }

    [JsonPropertyName("CoolFactorGun")]
    public double? CoolFactorGun { get; set; }

    [JsonPropertyName("CoolFactorGunMods")]
    public double? CoolFactorGunMods { get; set; }

    [JsonPropertyName("HeatFactorByShot")]
    public double? HeatFactorByShot { get; set; }

    [JsonPropertyName("AllowOverheat")]
    public bool? AllowOverheat { get; set; }

    [JsonPropertyName("DoubleActionAccuracyPenalty")]
    public double? DoubleActionAccuracyPenalty { get; set; }

    [JsonPropertyName("RecoilPosZMult")]
    public double? RecoilPosZMult { get; set; }

    [JsonPropertyName("RecoilReturnPathDampingHandRotation")]
    public double? RecoilReturnPathDampingHandRotation { get; set; }

    [JsonPropertyName("RecoilReturnPathOffsetHandRotation")]
    public double? RecoilReturnPathOffsetHandRotation { get; set; }

    [JsonPropertyName("RecoilReturnSpeedHandRotation")]
    public double? RecoilReturnSpeedHandRotation { get; set; }

    [JsonPropertyName("RecoilStableAngleIncreaseStep")]
    public double? RecoilStableAngleIncreaseStep { get; set; }

    [JsonPropertyName("RecoilStableIndexShot")]
    public double? RecoilStableIndexShot { get; set; }

    [JsonPropertyName("MinRepairKitDegradation")]
    public double? MinRepairKitDegradation { get; set; }

    [JsonPropertyName("MaxRepairKitDegradation")]
    public double? MaxRepairKitDegradation { get; set; }

    [JsonPropertyName("MountCameraSnapMultiplier")]
    public double? MountCameraSnapMultiplier { get; set; }

    [JsonPropertyName("MountHorizontalRecoilMultiplier")]
    public double? MountHorizontalRecoilMultiplier { get; set; }

    [JsonPropertyName("MountReturnSpeedHandMultiplier")]
    public double? MountReturnSpeedHandMultiplier { get; set; }

    [JsonPropertyName("MountVerticalRecoilMultiplier")]
    public double? MountVerticalRecoilMultiplier { get; set; }

    [JsonPropertyName("MountingHorizontalOutOfBreathMultiplier")]
    public double? MountingHorizontalOutOfBreathMultiplier { get; set; }

    [JsonPropertyName("MountingPosition")]
    public XYZ? MountingPosition { get; set; }

    [JsonPropertyName("MountingVerticalOutOfBreathMultiplier")]
    public double? MountingVerticalOutOfBreathMultiplier { get; set; }

    [JsonPropertyName("BlocksEarpiece")]
    public bool? BlocksEarpiece { get; set; }

    [JsonPropertyName("BlocksEyewear")]
    public bool? BlocksEyewear { get; set; }

    [JsonPropertyName("BlocksHeadwear")]
    public bool? BlocksHeadwear { get; set; }

    [JsonPropertyName("BlocksFaceCover")]
    public bool? BlocksFaceCover { get; set; }

    [JsonPropertyName("Indestructibility")]
    public double? Indestructibility { get; set; }

    [JsonPropertyName("FaceShieldComponent")]
    public bool? FaceShieldComponent { get; set; }

    [JsonPropertyName("FaceShieldMask")]
    public string? FaceShieldMask { get; set; }

    [JsonPropertyName("MaterialType")]
    public string? MaterialType { get; set; }

    [JsonPropertyName("RicochetParams")]
    public XYZ? RicochetParams { get; set; }

    [JsonPropertyName("DeafStrength")]
    public string? DeafStrength { get; set; }

    [JsonPropertyName("BlindnessProtection")]
    public double? BlindnessProtection { get; set; }

    [JsonPropertyName("Distortion")]
    public double? Distortion { get; set; }

    [JsonPropertyName("CompressorAttack")]
    public double? CompressorAttack { get; set; }

    [JsonPropertyName("CompressorRelease")]
    public double? CompressorRelease { get; set; }

    [JsonPropertyName("CompressorGain")]
    public double? CompressorGain { get; set; }

    [JsonPropertyName("EQBand1Frequency")]
    public double? EQBand1Frequency { get; set; }

    [JsonPropertyName("EQBand1Gain")]
    public double? EQBand1Gain { get; set; }

    [JsonPropertyName("EQBand1Q")]
    public double? EQBand1Q { get; set; }

    [JsonPropertyName("EQBand2Frequency")]
    public double? EQBand2Frequency { get; set; }

    [JsonPropertyName("EQBand2Gain")]
    public double? EQBand2Gain { get; set; }

    [JsonPropertyName("EQBand2Q")]
    public double? EQBand2Q { get; set; }

    [JsonPropertyName("EQBand3Frequency")]
    public double? EQBand3Frequency { get; set; }

    [JsonPropertyName("EQBand3Gain")]
    public double? EQBand3Gain { get; set; }

    [JsonPropertyName("EQBand3Q")]
    public double? EQBand3Q { get; set; }

    [JsonPropertyName("EffectsReturnsCompressorSendLevel")]
    public double? EffectsReturnsCompressorSendLevel { get; set; }

    [JsonPropertyName("EffectsReturnsGroupVolume")]
    public double? EffectsReturnsGroupVolume { get; set; }

    [JsonPropertyName("EnvCommonCompressorSendLevel")]
    public double? EffectsReturnsGrEnvCommonCompressorSendLeveloupVolume { get; set; }

    [JsonPropertyName("EnvNatureCompressorSendLevel")]
    public double? EnvNatureCompressorSendLevel { get; set; }

    [JsonPropertyName("EnvTechnicalCompressorSendLevel")]
    public double? EnvTechnicalCompressorSendLevel { get; set; }

    [JsonPropertyName("GunsCompressorSendLevel")]
    public double? GunsCompressorSendLevel { get; set; }

    [JsonPropertyName("HeadphonesMixerVolume")]
    public double? HeadphonesMixerVolume { get; set; }

    [JsonPropertyName("HighpassFreq")]
    public double? HighpassFreq { get; set; }

    [JsonPropertyName("HighpassResonance")]
    public double? HighpassResonance { get; set; }

    [JsonPropertyName("LowpassFreq")]
    public double? LowpassFreq { get; set; }

    [JsonPropertyName("RolloffMultiplier")]
    public double? RolloffMultiplier { get; set; }

    [JsonPropertyName("AmbientVolume")]
    public double? AmbientVolume { get; set; }

    [JsonPropertyName("AmbientCompressorSendLevel")]
    public double? AmbientCompressorSendLevel { get; set; }

    [JsonPropertyName("ClientPlayerCompressorSendLevel")]
    public double? ClientPlayerCompressorSendLevel { get; set; }

    [JsonPropertyName("CompressorThreshold")]
    public double? CompressorThreshold { get; set; }

    [JsonPropertyName("DryVolume")]
    public double? DryVolume { get; set; }

    [JsonPropertyName("foodUseTime")]
    public double? FoodUseTime { get; set; }

    [JsonPropertyName("foodEffectType")]
    public string? FoodEffectType { get; set; }

    [JsonPropertyName("StimulatorBuffs")]
    public string? StimulatorBuffs { get; set; }

    [JsonPropertyName("effects_health")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<HealthFactor, EffectsHealthProps>? EffectsHealth { get; set; }

    [JsonPropertyName("effects_damage")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<DamageEffectType, EffectDamageProps>? EffectsDamage { get; set; }

    // Confirmed in client
    [JsonPropertyName("MaximumNumberOfUsage")]
    public int? MaximumNumberOfUsage { get; set; }

    [JsonPropertyName("knifeHitDelay")]
    public double? KnifeHitDelay { get; set; }

    [JsonPropertyName("knifeHitSlashRate")]
    public double? KnifeHitSlashRate { get; set; }

    [JsonPropertyName("knifeHitStabRate")]
    public double? KnifeHitStabRate { get; set; }

    [JsonPropertyName("knifeHitRadius")]
    public double? KnifeHitRadius { get; set; }

    // Confirmed on client
    [JsonPropertyName("knifeHitSlashDam")]
    public int? KnifeHitSlashDam { get; set; }

    // Confirmed on client
    [JsonPropertyName("knifeHitStabDam")]
    public int? KnifeHitStabDam { get; set; }

    [JsonPropertyName("knifeDurab")]
    public double? KnifeDurab { get; set; }

    [JsonPropertyName("PrimaryDistance")]
    public double? PrimaryDistance { get; set; }

    [JsonPropertyName("SecondryDistance")]
    public double? SecondryDistance { get; set; }

    // Confirmed on client
    [JsonPropertyName("SlashPenetration")]
    public int? SlashPenetration { get; set; }

    // Confirmed on client
    [JsonPropertyName("StabPenetration")]
    public int? StabPenetration { get; set; }

    [JsonPropertyName("PrimaryConsumption")]
    public double? PrimaryConsumption { get; set; }

    [JsonPropertyName("SecondryConsumption")]
    public double? SecondryConsumption { get; set; }

    [JsonPropertyName("DeflectionConsumption")]
    public double? DeflectionConsumption { get; set; }

    [JsonPropertyName("AppliedTrunkRotation")]
    public XYZ? AppliedTrunkRotation { get; set; }

    [JsonPropertyName("AppliedHeadRotation")]
    public XYZ? AppliedHeadRotation { get; set; }

    [JsonPropertyName("DisplayOnModel")]
    public bool? DisplayOnModel { get; set; }

    [JsonPropertyName("AdditionalAnimationLayer")]
    public int? AdditionalAnimationLayer { get; set; }

    [JsonPropertyName("StaminaBurnRate")]
    public double? StaminaBurnRate { get; set; }

    [JsonPropertyName("ColliderScaleMultiplier")]
    public XYZ? ColliderScaleMultiplier { get; set; }

    [JsonPropertyName("ConfigPathStr")]
    public string? ConfigPathStr { get; set; }

    // Confirmed on client
    [JsonPropertyName("MaxMarkersCount")]
    public int? MaxMarkersCount { get; set; }

    [JsonPropertyName("scaleMin")]
    public double? ScaleMin { get; set; }

    [JsonPropertyName("scaleMax")]
    public double? ScaleMax { get; set; }

    [JsonPropertyName("medUseTime")]
    public double? MedUseTime { get; set; }

    [JsonPropertyName("medEffectType")]
    public string? MedEffectType { get; set; }

    // Confirmed in client
    [JsonPropertyName("MaxHpResource")]
    public int? MaxHpResource { get; set; }

    [JsonPropertyName("hpResourceRate")]
    public double? HpResourceRate { get; set; }

    [JsonPropertyName("apResource")]
    public double? ApResource { get; set; }

    [JsonPropertyName("krResource")]
    public double? KrResource { get; set; }

    [JsonPropertyName("MaxOpticZoom")]
    public double? MaxOpticZoom { get; set; }

    // Confirmed in client
    [JsonPropertyName("MaxRepairResource")]
    public int? MaxRepairResource { get; set; }

    // Confirmed on client - MongoId
    [JsonPropertyName("TargetItemFilter")]
    public List<string>? TargetItemFilter { get; set; }

    [JsonPropertyName("RepairQuality")]
    public double? RepairQuality { get; set; }

    [JsonPropertyName("RepairType")]
    public string? RepairType { get; set; }

    [JsonPropertyName("StackMinRandom")]
    public int? StackMinRandom { get; set; }

    [JsonPropertyName("StackMaxRandom")]
    public int? StackMaxRandom { get; set; }

    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    [JsonPropertyName("InitialSpeed")]
    public double? InitialSpeed { get; set; }

    [JsonPropertyName("BulletMassGram")]
    public double? BulletMassGram { get; set; }

    [JsonPropertyName("BulletDiameterMillimeters")]
    public double? BulletDiameterMillimeters { get; set; }

    [JsonPropertyName("Damage")]
    public double? Damage { get; set; }

    [JsonPropertyName("ammoAccr")]
    public double? AmmoAccr { get; set; }

    [JsonPropertyName("ammoRec")]
    public double? AmmoRec { get; set; }

    [JsonPropertyName("ammoDist")]
    public double? AmmoDist { get; set; }

    [JsonPropertyName("buckshotBullets")]
    public double? BuckshotBullets { get; set; }

    // Confirmed in client
    [JsonPropertyName("PenetrationPower")]
    public int? PenetrationPower { get; set; }

    [JsonPropertyName("PenetrationPowerDeviation")]
    public double? PenetrationPowerDeviation { get; set; }

    [JsonPropertyName("ammoHear")]
    public double? AmmoHear { get; set; }

    [JsonPropertyName("ammoSfx")]
    public string? AmmoSfx { get; set; }

    [JsonPropertyName("MisfireChance")]
    public double? MisfireChance { get; set; }

    // Confirmed in client
    [JsonPropertyName("MinFragmentsCount")]
    public int? MinFragmentsCount { get; set; }

    // Confirmed in client
    [JsonPropertyName("MaxFragmentsCount")]
    public int? MaxFragmentsCount { get; set; }

    [JsonPropertyName("ammoShiftChance")]
    public double? AmmoShiftChance { get; set; }

    [JsonPropertyName("casingName")]
    public string? CasingName { get; set; }

    [JsonPropertyName("casingEjectPower")]
    public double? CasingEjectPower { get; set; }

    [JsonPropertyName("casingMass")]
    public double? CasingMass { get; set; }

    [JsonPropertyName("casingSounds")]
    public string? CasingSounds { get; set; }

    [JsonPropertyName("ArmingTime")]
    public double? ArmingTime { get; set; }

    [JsonPropertyName("ProjectileCount")]
    public double? ProjectileCount { get; set; }

    [JsonPropertyName("PropagationSpeed")]
    public double? PropagationSpeed { get; set; }

    [JsonPropertyName("PenetrationChanceObstacle")]
    public double? PenetrationChanceObstacle { get; set; }

    [JsonPropertyName("PenetrationDamageMod")]
    public double? PenetrationDamageMod { get; set; }

    [JsonPropertyName("RicochetChance")]
    public double? RicochetChance { get; set; }

    [JsonPropertyName("FragmentationChance")]
    public double? FragmentationChance { get; set; }

    [JsonPropertyName("Deterioration")]
    public double? Deterioration { get; set; }

    [JsonPropertyName("SpeedRetardation")]
    public double? SpeedRetardation { get; set; }

    [JsonPropertyName("Tracer")]
    public bool? Tracer { get; set; }

    [JsonPropertyName("TracerColor")]
    public string? TracerColor { get; set; }

    [JsonPropertyName("TracerDistance")]
    public double? TracerDistance { get; set; }

    [JsonPropertyName("ArmorDamage")]
    public double? ArmorDamage { get; set; }

    [JsonPropertyName("Caliber")]
    public string? Caliber { get; set; }

    [JsonPropertyName("StaminaBurnPerDamage")]
    public double? StaminaBurnPerDamage { get; set; }

    [JsonPropertyName("HeavyBleedingDelta")]
    public double? HeavyBleedingDelta { get; set; }

    [JsonPropertyName("LightBleedingDelta")]
    public double? LightBleedingDelta { get; set; }

    [JsonPropertyName("ShowBullet")]
    public bool? ShowBullet { get; set; }

    [JsonPropertyName("HasGrenaderComponent")]
    public bool? HasGrenaderComponent { get; set; }

    [JsonPropertyName("FuzeArmTimeSec")]
    public double? FuzeArmTimeSec { get; set; }

    [JsonPropertyName("MinExplosionDistance")]
    public double? MinExplosionDistance { get; set; }

    [JsonPropertyName("PenetrationPowerDiviation")]
    public double? PenetrationPowerDiviation { get; set; }

    [JsonPropertyName("MaxExplosionDistance")]
    public double? MaxExplosionDistance { get; set; }

    // Confirmed in client
    [JsonPropertyName("FragmentsCount")]
    public int? FragmentsCount { get; set; }

    [JsonPropertyName("FragmentType")]
    public string? FragmentType { get; set; }

    [JsonPropertyName("ShowHitEffectOnExplode")]
    public bool? ShowHitEffectOnExplode { get; set; }

    [JsonPropertyName("ExplosionType")]
    public string? ExplosionType { get; set; }

    [JsonPropertyName("AmmoLifeTimeSec")]
    public double? AmmoLifeTimeSec { get; set; }

    [JsonPropertyName("AmmoTooltipClass")]
    public string? AmmoTooltipClass { get; set; }

    [JsonPropertyName("Contusion")]
    public XYZ? Contusion { get; set; }

    [JsonPropertyName("ArmorDistanceDistanceDamage")]
    public XYZ? ArmorDistanceDistanceDamage { get; set; }

    [JsonPropertyName("BackBlastConeAngle")]
    public double? BackBlastConeAngle { get; set; }

    [JsonPropertyName("BackblastDamage")]
    public XY? BackblastDamage { get; set; }

    [JsonPropertyName("BackblastDistance")]
    public double? BackblastDistance { get; set; }

    [JsonPropertyName("Blindness")]
    public XYZ? Blindness { get; set; }

    [JsonPropertyName("IsLightAndSoundShot")]
    public bool? IsLightAndSoundShot { get; set; }

    [JsonPropertyName("IsMountable")]
    public bool? IsMountable { get; set; }

    [JsonPropertyName("LightAndSoundShotAngle")]
    public double? LightAndSoundShotAngle { get; set; }

    [JsonPropertyName("LightAndSoundShotSelfContusionTime")]
    public double? LightAndSoundShotSelfContusionTime { get; set; }

    [JsonPropertyName("LightAndSoundShotSelfContusionStrength")]
    public double? LightAndSoundShotSelfContusionStrength { get; set; }

    [JsonPropertyName("MalfMisfireChance")]
    public double? MalfMisfireChance { get; set; }

    [JsonPropertyName("MalfFeedChance")]
    public double? MalfFeedChance { get; set; }

    [JsonPropertyName("StackSlots")]
    public List<StackSlot>? StackSlots { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("eqMin")]
    public double? EqMin { get; set; }

    [JsonPropertyName("eqMax")]
    public double? EqMax { get; set; }

    [JsonPropertyName("rate")]
    public double? Rate { get; set; }

    [JsonPropertyName("ThrowType")]
    public ThrowWeapType? ThrowType { get; set; }

    [JsonPropertyName("ExplDelay")]
    public double? ExplDelay { get; set; }

    [JsonPropertyName("explDelay")]
    public double? explDelay { get; set; }

    [JsonPropertyName("Strength")]
    public double? Strength { get; set; }

    [JsonPropertyName("ContusionDistance")]
    public double? ContusionDistance { get; set; }

    [JsonPropertyName("throwDamMax")]
    public double? ThrowDamMax { get; set; }

    [JsonPropertyName("EmitTime")]
    public double? EmitTime { get; set; }

    [JsonPropertyName("CanBeHiddenDuringThrow")]
    public bool? CanBeHiddenDuringThrow { get; set; }

    [JsonPropertyName("CanPlantOnGround")]
    public bool? CanPlantOnGround { get; set; }

    [JsonPropertyName("MinTimeToContactExplode")]
    public double? MinTimeToContactExplode { get; set; }

    [JsonPropertyName("ExplosionEffectType")]
    public string? ExplosionEffectType { get; set; }

    [JsonPropertyName("LinkedWeapon")]
    public string? LinkedWeapon { get; set; }

    [JsonPropertyName("UseAmmoWithoutShell")]
    public bool? UseAmmoWithoutShell { get; set; }

    [JsonPropertyName("RecoilDampingHandRotation")]
    public double? RecoilDampingHandRotation { get; set; }

    [JsonPropertyName("LeanWeaponAgainstBody")]
    public bool? LeanWeaponAgainstBody { get; set; }

    [JsonPropertyName("RemoveShellAfterFire")]
    public bool? RemoveShellAfterFire { get; set; }

    [JsonPropertyName("RepairStrategyTypes")]
    public List<RepairStrategyType>? RepairStrategyTypes { get; set; }

    [JsonPropertyName("IsEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("LayoutName")]
    public string? LayoutName { get; set; }

    [JsonPropertyName("Lower75Prefab")]
    public Prefab? Lower75Prefab { get; set; }

    [JsonPropertyName("MaxUsages")]
    public double? MaxUsages { get; set; }

    [JsonPropertyName("BallisticCoeficient")]
    public double? BallisticCoeficient { get; set; }

    [JsonPropertyName("BulletDiameterMilimeters")]
    public double? BulletDiameterMilimeters { get; set; }

    [JsonPropertyName("ScavKillExpPenalty")]
    public double? ScavKillExpPenalty { get; set; }

    [JsonPropertyName("ScavKillExpPenaltyPVE")]
    public double? ScavKillExpPenaltyPVE { get; set; }

    [JsonPropertyName("ScavKillStandingPenalty")]
    public double? ScavKillStandingPenalty { get; set; }

    [JsonPropertyName("ScavKillStandingPenaltyPVE")]
    public double? ScavKillStandingPenaltyPVE { get; set; }

    [JsonPropertyName("TradersDiscount")]
    public double? TradersDiscount { get; set; }

    [JsonPropertyName("TradersDiscountPVE")]
    public double? TradersDiscountPVE { get; set; }

    [JsonPropertyName("AvailableAsDefault")]
    public bool? AvailableAsDefault { get; set; }

    [JsonPropertyName("ProfileVersions")]
    public List<string>? ProfileVersions { get; set; }

    [JsonPropertyName("Side")]
    public List<string>? Side { get; set; }

    [JsonPropertyName("BipodCameraSnapMultiplier")]
    public double? BipodCameraSnapMultiplier { get; set; }

    [JsonPropertyName("BipodOutOfStaminaBreathMultiplier")]
    public double? BipodOutOfStaminaBreathMultiplier { get; set; }

    [JsonPropertyName("BipodRecoilMultiplier")]
    public double? BipodRecoilMultiplier { get; set; }

    [JsonPropertyName("BipodReturnHandSpeedMultiplier")]
    public double? BipodReturnHandSpeedMultiplier { get; set; }

    [JsonPropertyName("PitchLimitProneBipod")]
    public XYZ? PitchLimitProneBipod { get; set; }

    [JsonPropertyName("YawLimitProneBipod")]
    public XYZ? YawLimitProneBipod { get; set; }

    [JsonPropertyName("AdjustableOpticSensitivity")]
    public double? AdjustableOpticSensitivity { get; set; }

    [JsonPropertyName("AdjustableOpticSensitivityMax")]
    public double? AdjustableOpticSensitivityMax { get; set; }

    //[JsonExtensionData]
    //public Dictionary<string, object> OtherProperties { get; set; }
}

public record WeaponRecoilSettings
{
    [JsonPropertyName("Enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("Values")]
    public List<WeaponRecoilSettingValues>? Values { get; set; }
}

public record WeaponRecoilSettingValues
{
    [JsonPropertyName("Enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("Process")]
    public WeaponRecoilProcess? Process { get; set; }

    [JsonPropertyName("Target")]
    public string? Target { get; set; }
}

public record WeaponRecoilProcess
{
    [JsonPropertyName("ComponentType")]
    public string? ComponentType { get; set; }

    [JsonPropertyName("CurveAimingValueMultiply")]
    public double? CurveAimingValueMultiply { get; set; }

    [JsonPropertyName("CurveTimeMultiply")]
    public double? CurveTimeMultiply { get; set; }

    [JsonPropertyName("CurveValueMultiply")]
    public double? CurveValueMultiply { get; set; }

    [JsonPropertyName("TransformationCurve")]
    public WeaponRecoilTransformationCurve? TransformationCurve { get; set; }
}

public record WeaponRecoilTransformationCurve
{
    [JsonPropertyName("Keys")]
    public List<WeaponRecoilTransformationCurveKey>? Keys { get; set; }
}

public record WeaponRecoilTransformationCurveKey
{
    [JsonPropertyName("inTangent")]
    public double? InTangent { get; set; }

    [JsonPropertyName("outTangent")]
    public double? OutTangent { get; set; }

    [JsonPropertyName("time")]
    public double? Time { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public record HealthEffect
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public record Prefab
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("rcid")]
    public string? Rcid { get; set; }
}

public record Grid
{
    [JsonPropertyName("_name")]
    public string? Name { get; set; }

    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("_props")]
    public GridProps? Props { get; set; }

    [JsonPropertyName("_proto")]
    public string? Proto { get; set; }
}

public record GridProps
{
    [JsonPropertyName("filters")]
    public List<GridFilter>? Filters { get; set; }

    [JsonPropertyName("cellsH")]
    public int? CellsH { get; set; }

    [JsonPropertyName("cellsV")]
    public int? CellsV { get; set; }

    [JsonPropertyName("minCount")]
    public double? MinCount { get; set; }

    [JsonPropertyName("maxCount")]
    public double? MaxCount { get; set; }

    [JsonPropertyName("maxWeight")]
    public double? MaxWeight { get; set; }

    [JsonPropertyName("isSortingTable")]
    public bool? IsSortingTable { get; set; }
}

public record GridFilter
{
    [JsonPropertyName("Filter")]
    public HashSet<string>? Filter { get; set; }

    [JsonPropertyName("ExcludedFilter")]
    public List<string>? ExcludedFilter { get; set; }

    [JsonPropertyName("locked")]
    public bool? Locked { get; set; }
}

public record Slot
{
    private string? _name;

    private string? _proto;

    [JsonPropertyName("_name")]
    public string? Name
    {
        get { return _name; }
        set { _name = value == null ? null : string.Intern(value); }
    }

    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("_props")]
    public SlotProps? Props { get; set; }

    [JsonPropertyName("_max_count")]
    public double? MaxCount { get; set; }

    [JsonPropertyName("_required")]
    public bool? Required { get; set; }

    [JsonPropertyName("_mergeSlotWithChildren")]
    public bool? MergeSlotWithChildren { get; set; }

    [JsonPropertyName("_proto")]
    public string? Proto
    {
        get { return _proto; }
        set { _proto = value == null ? null : string.Intern(value); }
    }
}

public record SlotProps
{
    [JsonPropertyName("filters")]
    public List<SlotFilter>? Filters { get; set; }

    [JsonPropertyName("MaxStackCount")]
    public double? MaxStackCount { get; set; }
}

public record SlotFilter
{
    [JsonPropertyName("Shift")]
    public double? Shift { get; set; }

    [JsonPropertyName("locked")]
    public bool? Locked { get; set; }

    [JsonPropertyName("Plate")]
    public string? Plate { get; set; }

    [JsonPropertyName("armorColliders")]
    public List<string>? ArmorColliders { get; set; }

    [JsonPropertyName("armorPlateColliders")]
    public List<string>? ArmorPlateColliders { get; set; }

    [JsonPropertyName("Filter")]
    public HashSet<string>? Filter { get; set; }

    [JsonPropertyName("AnimationIndex")]
    public double? AnimationIndex { get; set; }

    [JsonPropertyName("MaxStackCount")]
    public double? MaxStackCount { get; set; }

    [JsonPropertyName("bluntDamageReduceFromSoftArmor")]
    public bool? BluntDamageReduceFromSoftArmor { get; set; }
}

public record StackSlot
{
    [JsonPropertyName("_name")]
    public string? Name { get; set; }

    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("_max_count")]
    public double? MaxCount { get; set; }

    [JsonPropertyName("_props")]
    public StackSlotProps? Props { get; set; }

    [JsonPropertyName("_proto")]
    public string? Proto { get; set; }

    [JsonPropertyName("upd")]
    public object? Upd { get; set; } // TODO: object here
}

public record StackSlotProps
{
    [JsonPropertyName("filters")]
    public List<SlotFilter>? Filters { get; set; }
}

public record RandomLootSettings
{
    [JsonPropertyName("allowToSpawnIdenticalItems")]
    public bool? AllowToSpawnIdenticalItems { get; set; }

    [JsonPropertyName("allowToSpawnQuestItems")]
    public bool? AllowToSpawnQuestItems { get; set; }

    [JsonPropertyName("countByRarity")]
    public List<object>? CountByRarity { get; set; } // TODO: object here

    [JsonPropertyName("excluded")]
    public RandomLootExcluded? Excluded { get; set; }

    [JsonPropertyName("filters")]
    public List<object>? Filters { get; set; } // TODO: object here

    [JsonPropertyName("findInRaid")]
    public bool? FindInRaid { get; set; }

    [JsonPropertyName("maxCount")]
    public double? MaxCount { get; set; }

    [JsonPropertyName("minCount")]
    public double? MinCount { get; set; }
}

public record RandomLootExcluded
{
    [JsonPropertyName("categoryTemplates")]
    public List<object>? CategoryTemplates { get; set; } // TODO: object here

    [JsonPropertyName("rarity")]
    public List<string>? Rarity { get; set; }

    [JsonPropertyName("templates")]
    public List<object>? Templates { get; set; } // TODO: object here
}

public record EffectsHealth
{
    [JsonPropertyName("Energy")]
    public EffectsHealthProps? Energy { get; set; }

    [JsonPropertyName("Hydration")]
    public EffectsHealthProps? Hydration { get; set; }
}

public record EffectsHealthProps
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("delay")]
    public double? Delay { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }
}

public record EffectsDamage
{
    [JsonPropertyName("Pain")]
    public EffectDamageProps? Pain { get; set; }

    [JsonPropertyName("LightBleeding")]
    public EffectDamageProps? LightBleeding { get; set; }

    [JsonPropertyName("HeavyBleeding")]
    public EffectDamageProps? HeavyBleeding { get; set; }

    [JsonPropertyName("Contusion")]
    public EffectDamageProps? Contusion { get; set; }

    [JsonPropertyName("RadExposure")]
    public EffectDamageProps? RadExposure { get; set; }

    [JsonPropertyName("Fracture")]
    public EffectDamageProps? Fracture { get; set; }

    [JsonPropertyName("DestroyedPart")]
    public EffectDamageProps? DestroyedPart { get; set; }
}

public record EffectDamageProps
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }

    [JsonPropertyName("delay")]
    public double? Delay { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }

    [JsonPropertyName("fadeOut")]
    public double? FadeOut { get; set; }

    [JsonPropertyName("cost")]
    public double? Cost { get; set; }

    [JsonPropertyName("healthPenaltyMin")]
    public double? HealthPenaltyMin { get; set; }

    [JsonPropertyName("healthPenaltyMax")]
    public double? HealthPenaltyMax { get; set; }
}

public record Color
{
    [JsonPropertyName("r")]
    public double? R { get; set; }

    [JsonPropertyName("g")]
    public double? G { get; set; }

    [JsonPropertyName("b")]
    public double? B { get; set; }

    [JsonPropertyName("a")]
    public double? A { get; set; }
}

public record ShotsGroupSettings
{
    [JsonPropertyName("EndShotIndex")]
    public double? EndShotIndex { get; set; }

    [JsonPropertyName("ShotRecoilPositionStrength")]
    public XYZ? ShotRecoilPositionStrength { get; set; }

    [JsonPropertyName("ShotRecoilRadianRange")]
    public XYZ? ShotRecoilRadianRange { get; set; }

    [JsonPropertyName("ShotRecoilRotationStrength")]
    public XYZ? ShotRecoilRotationStrength { get; set; }

    [JsonPropertyName("StartShotIndex")]
    public double? StartShotIndex { get; set; }
}

public enum ItemType
{
    NODE = 1,
    ITEM = 2,
}
