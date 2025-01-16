using System.Text.Json.Serialization;
using Core.Utils.Json.Converters;

namespace Core.Models.Eft.Common.Tables;

public class TemplateItem
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("_name")]
    public string? Name { get; set; }

    [JsonPropertyName("_parent")]
    public string? Parent { get; set; }

    [JsonPropertyName("_type")]
    public string? Type { get; set; }

    [JsonPropertyName("_props")]
    public Props? Properties { get; set; }

    [JsonPropertyName("_proto")]
    public string? Prototype { get; set; }
}

public class Props
{
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

    [JsonPropertyName("BackgroundColor")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("Width")]
    public double? Width { get; set; }

    [JsonPropertyName("Height")]
    public double? Height { get; set; }

    [JsonPropertyName("StackMaxSize")]
    public double? StackMaxSize { get; set; }

    [JsonPropertyName("Rarity")]
    public string? Rarity { get; set; }

    [JsonPropertyName("SpawnChance")]
    public double? SpawnChance { get; set; }

    [JsonPropertyName("CreditsPrice")]
    public double? CreditsPrice { get; set; }

    [JsonPropertyName("ItemSound")]
    public string? ItemSound { get; set; }

    [JsonPropertyName("Prefab")] // TODO: TYPE FUCKERY: can be a Prefab object or empty string or a string
    public object? Prefab { get; set; }

    [JsonPropertyName("UsePrefab")]
    public Prefab? UsePrefab { get; set; }

    [JsonPropertyName("airDropTemplateId")]
    public string? AirDropTemplateId { get; set; }

    [JsonPropertyName("StackObjectsCount")]
    public double? StackObjectsCount { get; set; }

    [JsonPropertyName("NotShownInSlot")]
    public bool? NotShownInSlot { get; set; }

    [JsonPropertyName("ExaminedByDefault")]
    public bool? ExaminedByDefault { get; set; }

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

    [JsonPropertyName("LootExperience")]
    public double? LootExperience { get; set; }

    [JsonPropertyName("ExamineExperience")]
    public double? ExamineExperience { get; set; }

    [JsonPropertyName("HideEntrails")]
    public bool? HideEntrails { get; set; }

    [JsonPropertyName("InsuranceDisabled")]
    public bool? InsuranceDisabled { get; set; }

    [JsonPropertyName("RepairCost")]
    public double? RepairCost { get; set; }

    [JsonPropertyName("RepairSpeed")]
    public double? RepairSpeed { get; set; }

    [JsonPropertyName("ExtraSizeLeft")]
    public double? ExtraSizeLeft { get; set; }

    [JsonPropertyName("ExtraSizeRight")]
    public double? ExtraSizeRight { get; set; }

    [JsonPropertyName("ExtraSizeUp")]
    public double? ExtraSizeUp { get; set; }

    [JsonPropertyName("FlareTypes")]
    public List<string>? FlareTypes { get; set; }

    [JsonPropertyName("ExtraSizeDown")]
    public double? ExtraSizeDown { get; set; }

    [JsonPropertyName("ExtraSizeForceAdd")]
    public bool? ExtraSizeForceAdd { get; set; }

    [JsonPropertyName("MergesWithChildren")]
    public bool? MergesWithChildren { get; set; }

    [JsonPropertyName("MetascoreGroup")]
    public string? MetaScoreGroup { get; set; }

    [JsonPropertyName("CanSellOnRagfair")]
    public bool? CanSellOnRagfair { get; set; }

    [JsonPropertyName("CanRequireOnRagfair")]
    public bool? CanRequireOnRagfair { get; set; }

    [JsonPropertyName("ConflictingItems")]
    public List<string>? ConflictingItems { get; set; }

    [JsonPropertyName("Unlootable")]
    public bool? Unlootable { get; set; }

    [JsonPropertyName("UnlootableFromSlot")]
    public string? UnlootableFromSlot { get; set; }

    [JsonPropertyName("UnlootableFromSide")]
    public List<string>? UnlootableFromSide { get; set; }

    [JsonPropertyName("AnimationVariantsNumber")]
    public double? AnimationVariantsNumber { get; set; }

    [JsonPropertyName("DiscardingBlock")]
    public bool? DiscardingBlock { get; set; }

    [JsonPropertyName("DropSoundType")]
    public string? DropSoundType { get; set; }

    [JsonPropertyName("RagFairCommissionModifier")]
    public double? RagFairCommissionModifier { get; set; }

    [JsonPropertyName("RarityPvE")]
    public string? RarityPvE { get; set; }

    [JsonPropertyName("IsAlwaysAvailableForInsurance")]
    public bool? IsAlwaysAvailableForInsurance { get; set; }

    [JsonPropertyName("DiscardLimit")]
    public double? DiscardLimit { get; set; }

    [JsonPropertyName("MaxResource")]
    public double? MaxResource { get; set; }

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
    public List<string>? CantRemoveFromSlotsDuringRaid { get; set; }

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

    [JsonPropertyName("aimingSensitivity")]
    public double? AimingSensitivity { get; set; }

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

    [JsonPropertyName("canAdmin")]
    public bool? CanAdmin { get; set; }

    [JsonPropertyName("loadUnloadModifier")]
    public double? LoadUnloadModifier { get; set; }

    [JsonPropertyName("checkTimeModifier")]
    public double? CheckTimeModifier { get; set; }

    [JsonPropertyName("checkOverride")]
    public double? CheckOverride { get; set; }

    [JsonPropertyName("reloadMagType")]
    public string? ReloadMagType { get; set; }

    [JsonPropertyName("visibleAmmoRangesString")]
    public string? VisibleAmmoRangesString { get; set; }

    [JsonPropertyName("malfunctionChance")]
    public double? MalfunctionChance { get; set; }

    [JsonPropertyName("isShoulderContact")]
    public bool? IsShoulderContact { get; set; }

    [JsonPropertyName("foldable")]
    public bool? Foldable { get; set; }

    [JsonPropertyName("retractable")]
    public bool? Retractable { get; set; }

    [JsonPropertyName("sizeReduceRight")]
    public double? SizeReduceRight { get; set; }

    [JsonPropertyName("centerOfImpact")]
    public double? CenterOfImpact { get; set; }

    [JsonPropertyName("isSilencer")]
    public bool? IsSilencer { get; set; }

    [JsonPropertyName("deviationCurve")]
    public double? DeviationCurve { get; set; }

    [JsonPropertyName("deviationMax")]
    public double? DeviationMax { get; set; }

    [JsonPropertyName("searchSound")]
    public string? SearchSound { get; set; }

    [JsonPropertyName("blocksArmorVest")]
    public bool? BlocksArmorVest { get; set; }

    [JsonPropertyName("speedPenaltyPercent")]
    public double? SpeedPenaltyPercent { get; set; }

    [JsonPropertyName("gridLayoutName")]
    public string? GridLayoutName { get; set; }

    [JsonPropertyName("containerSpawnChanceModifier")]
    public double? ContainerSpawnChanceModifier { get; set; }

    [JsonPropertyName("spawnExcludedFilter")]
    public List<string>? SpawnExcludedFilter { get; set; }

    [JsonPropertyName("spawnFilter")]
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

    [JsonPropertyName("rigLayoutName")]
    public string? RigLayoutName { get; set; }

    [JsonPropertyName("maxDurability")]
    public double? MaxDurability { get; set; }

    [JsonPropertyName("armorZone")]
    public List<string>? ArmorZone { get; set; }

    [JsonPropertyName("armorClass")]
    [JsonConverter(typeof(StringToNumberFactoryConverter))]
    public double? ArmorClass { get; set; } // TODO: object here

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

    [JsonPropertyName("bluntThroughput")]
    public double? BluntThroughput { get; set; }

    [JsonPropertyName("armorMaterial")]
    public string? ArmorMaterial { get; set; }

    [JsonPropertyName("armorType")]
    public string? ArmorType { get; set; }

    [JsonPropertyName("weapClass")]
    public string? WeapClass { get; set; }

    [JsonPropertyName("weapUseType")]
    public string? WeapUseType { get; set; }

    [JsonPropertyName("ammoCaliber")]
    public string? AmmoCaliber { get; set; }

    [JsonPropertyName("operatingResource")]
    public double? OperatingResource { get; set; }

    [JsonPropertyName("postRecoilHorizontalRangeHandRotation")]
    public XYZ? PostRecoilHorizontalRangeHandRotation { get; set; }

    [JsonPropertyName("postRecoilVerticalRangeHandRotation")]
    public XYZ? PostRecoilVerticalRangeHandRotation { get; set; }

    [JsonPropertyName("progressRecoilAngleOnStable")]
    public XYZ? ProgressRecoilAngleOnStable { get; set; }

    [JsonPropertyName("repairComplexity")]
    public double? RepairComplexity { get; set; }

    [JsonPropertyName("durabSpawnMin")]
    public double? DurabSpawnMin { get; set; }

    [JsonPropertyName("durabSpawnMax")]
    public double? DurabSpawnMax { get; set; }

    [JsonPropertyName("isFastReload")]
    public bool? IsFastReload { get; set; }

    [JsonPropertyName("recoilForceUp")]
    public double? RecoilForceUp { get; set; }

    [JsonPropertyName("recoilForceBack")]
    public double? RecoilForceBack { get; set; }

    [JsonPropertyName("recoilAngle")]
    public double? RecoilAngle { get; set; }

    [JsonPropertyName("recoilCamera")]
    public double? RecoilCamera { get; set; }

    [JsonPropertyName("recoilCategoryMultiplierHandRotation")]
    public double? RecoilCategoryMultiplierHandRotation { get; set; }

    [JsonPropertyName("weapFireType")]
    public List<string>? WeapFireType { get; set; }

    [JsonPropertyName("recolDispersion")]
    public double? RecolDispersion { get; set; }

    [JsonPropertyName("singleFireRate")]
    public double? SingleFireRate { get; set; }

    [JsonPropertyName("canQueueSecondShot")]
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

    [JsonPropertyName("adjustCollimatorsToTrajectory")]
    public bool? AdjustCollimatorsToTrajectory { get; set; }

    [JsonPropertyName("shotgunDispersion")]
    public double? ShotgunDispersion { get; set; }

    [JsonPropertyName("chambers")]
    public List<Slot>? Chambers { get; set; }

    [JsonPropertyName("cameraSnap")]
    public double? CameraSnap { get; set; }

    [JsonPropertyName("cameraToWeaponAngleSpeedRange")]
    public XYZ? CameraToWeaponAngleSpeedRange { get; set; }

    [JsonPropertyName("cameraToWeaponAngleStep")]
    public double? CameraToWeaponAngleStep { get; set; }

    [JsonPropertyName("reloadMode")]
    public string? ReloadMode { get; set; }

    [JsonPropertyName("aimPlane")]
    public double? AimPlane { get; set; }

    [JsonPropertyName("tacticalReloadStiffnes")]
    public XYZ? TacticalReloadStiffnes { get; set; }

    [JsonPropertyName("tacticalReloadFixation")]
    public double? TacticalReloadFixation { get; set; }

    [JsonPropertyName("recoilCenter")]
    public XYZ? RecoilCenter { get; set; }

    [JsonPropertyName("rotationCenter")]
    public XYZ? RotationCenter { get; set; }

    [JsonPropertyName("rotationCenterNoStock")]
    public XYZ? RotationCenterNoStock { get; set; }

    [JsonPropertyName("shotsGroupSettings")]
    public List<ShotsGroupSettings>? ShotsGroupSettings { get; set; }

    [JsonPropertyName("foldedSlot")]
    public string? FoldedSlot { get; set; }

    [JsonPropertyName("forbidMissingVitalParts")]
    public bool? ForbidMissingVitalParts { get; set; }

    [JsonPropertyName("forbidNonEmptyContainers")]
    public bool? ForbidNonEmptyContainers { get; set; }

    [JsonPropertyName("compactHandling")]
    public bool? CompactHandling { get; set; }

    [JsonPropertyName("minRepairDegradation")]
    public double? MinRepairDegradation { get; set; }

    [JsonPropertyName("maxRepairDegradation")]
    public double? MaxRepairDegradation { get; set; }

    [JsonPropertyName("ironSightRange")]
    public double? IronSightRange { get; set; }

    [JsonPropertyName("isBeltMachineGun")]
    public bool? IsBeltMachineGun { get; set; }

    [JsonPropertyName("isFlareGun")]
    public bool? IsFlareGun { get; set; }

    [JsonPropertyName("isGrenadeLauncher")]
    public bool? IsGrenadeLauncher { get; set; }

    [JsonPropertyName("isOneoff")]
    public bool? IsOneoff { get; set; }

    [JsonPropertyName("mustBoltBeOpennedForExternalReload")]
    public bool? MustBoltBeOpennedForExternalReload { get; set; }

    [JsonPropertyName("mustBoltBeOpennedForInternalReload")]
    public bool? MustBoltBeOpennedForInternalReload { get; set; }

    [JsonPropertyName("noFiremodeOnBoltcatch")]
    public bool? NoFiremodeOnBoltcatch { get; set; }

    [JsonPropertyName("boltAction")]
    public bool? BoltAction { get; set; }

    [JsonPropertyName("hipAccuracyRestorationDelay")]
    public double? HipAccuracyRestorationDelay { get; set; }

    [JsonPropertyName("hipAccuracyRestorationSpeed")]
    public double? HipAccuracyRestorationSpeed { get; set; }

    [JsonPropertyName("hipInnaccuracyGain")]
    public double? HipInnaccuracyGain { get; set; }

    [JsonPropertyName("manualBoltCatch")]
    public bool? ManualBoltCatch { get; set; }

    [JsonPropertyName("burstShotsCount")]
    public double? BurstShotsCount { get; set; }

    [JsonPropertyName("baseMalfunctionChance")]
    public double? BaseMalfunctionChance { get; set; }

    [JsonPropertyName("allowJam")]
    public bool? AllowJam { get; set; }

    [JsonPropertyName("allowFeed")]
    public bool? AllowFeed { get; set; }

    [JsonPropertyName("allowMisfire")]
    public bool? AllowMisfire { get; set; }

    [JsonPropertyName("allowSlide")]
    public bool? AllowSlide { get; set; }

    [JsonPropertyName("durabilityBurnRatio")]
    public double? DurabilityBurnRatio { get; set; }

    [JsonPropertyName("heatFactorGun")]
    public double? HeatFactorGun { get; set; }

    [JsonPropertyName("coolFactorGun")]
    public double? CoolFactorGun { get; set; }

    [JsonPropertyName("coolFactorGunMods")]
    public double? CoolFactorGunMods { get; set; }

    [JsonPropertyName("heatFactorByShot")]
    public double? HeatFactorByShot { get; set; }

    [JsonPropertyName("allowOverheat")]
    public bool? AllowOverheat { get; set; }

    [JsonPropertyName("doubleActionAccuracyPenalty")]
    public double? DoubleActionAccuracyPenalty { get; set; }

    [JsonPropertyName("recoilPosZMult")]
    public double? RecoilPosZMult { get; set; }

    [JsonPropertyName("recoilReturnPathDampingHandRotation")]
    public double? RecoilReturnPathDampingHandRotation { get; set; }

    [JsonPropertyName("recoilReturnPathOffsetHandRotation")]
    public double? RecoilReturnPathOffsetHandRotation { get; set; }

    [JsonPropertyName("recoilReturnSpeedHandRotation")]
    public double? RecoilReturnSpeedHandRotation { get; set; }

    [JsonPropertyName("recoilStableAngleIncreaseStep")]
    public double? RecoilStableAngleIncreaseStep { get; set; }

    [JsonPropertyName("recoilStableIndexShot")]
    public double? RecoilStableIndexShot { get; set; }

    [JsonPropertyName("minRepairKitDegradation")]
    public double? MinRepairKitDegradation { get; set; }

    [JsonPropertyName("maxRepairKitDegradation")]
    public double? MaxRepairKitDegradation { get; set; }

    [JsonPropertyName("mountCameraSnapMultiplier")]
    public double? MountCameraSnapMultiplier { get; set; }

    [JsonPropertyName("mountHorizontalRecoilMultiplier")]
    public double? MountHorizontalRecoilMultiplier { get; set; }

    [JsonPropertyName("mountReturnSpeedHandMultiplier")]
    public double? MountReturnSpeedHandMultiplier { get; set; }

    [JsonPropertyName("mountVerticalRecoilMultiplier")]
    public double? MountVerticalRecoilMultiplier { get; set; }

    [JsonPropertyName("mountingHorizontalOutOfBreathMultiplier")]
    public double? MountingHorizontalOutOfBreathMultiplier { get; set; }

    [JsonPropertyName("mountingPosition")]
    public XYZ? MountingPosition { get; set; }

    [JsonPropertyName("mountingVerticalOutOfBreathMultiplier")]
    public XYZ? MountingVerticalOutOfBreathMultiplier { get; set; }

    [JsonPropertyName("blocksEarpiece")]
    public bool? BlocksEarpiece { get; set; }

    [JsonPropertyName("blocksEyewear")]
    public bool? BlocksEyewear { get; set; }

    [JsonPropertyName("blocksHeadwear")]
    public bool? BlocksHeadwear { get; set; }

    [JsonPropertyName("blocksFaceCover")]
    public bool? BlocksFaceCover { get; set; }

    [JsonPropertyName("indestructibility")]
    public double? Indestructibility { get; set; }

    [JsonPropertyName("headSegments")]
    public List<string>? HeadSegments { get; set; }

    [JsonPropertyName("faceShieldComponent")]
    public bool? FaceShieldComponent { get; set; }

    [JsonPropertyName("faceShieldMask")]
    public string? FaceShieldMask { get; set; }

    [JsonPropertyName("materialType")]
    public string? MaterialType { get; set; }

    [JsonPropertyName("ricochetParams")]
    public XYZ? RicochetParams { get; set; }

    [JsonPropertyName("deafStrength")]
    public string? DeafStrength { get; set; }

    [JsonPropertyName("blindnessProtection")]
    public double? BlindnessProtection { get; set; }

    [JsonPropertyName("distortion")]
    public double? Distortion { get; set; }

    [JsonPropertyName("compressorTreshold")]
    public double? CompressorTreshold { get; set; }

    [JsonPropertyName("compressorAttack")]
    public double? CompressorAttack { get; set; }

    [JsonPropertyName("compressorRelease")]
    public double? CompressorRelease { get; set; }

    [JsonPropertyName("compressorGain")]
    public double? CompressorGain { get; set; }

    [JsonPropertyName("cutoffFreq")]
    public double? CutoffFreq { get; set; }

    [JsonPropertyName("resonance")]
    public double? Resonance { get; set; }

    [JsonPropertyName("rolloffMultiplier")]
    public double? RolloffMultiplier { get; set; }

    [JsonPropertyName("reverbVolume")]
    public double? ReverbVolume { get; set; }

    [JsonPropertyName("compressorVolume")]
    public double? CompressorVolume { get; set; }

    [JsonPropertyName("ambientVolume")]
    public double? AmbientVolume { get; set; }

    [JsonPropertyName("dryVolume")]
    public double? DryVolume { get; set; }

    [JsonPropertyName("highFrequenciesGain")]
    public double? HighFrequenciesGain { get; set; }

    [JsonPropertyName("foodUseTime")]
    public double? FoodUseTime { get; set; }

    [JsonPropertyName("foodEffectType")]
    public string? FoodEffectType { get; set; }

    [JsonPropertyName("stimulatorBuffs")]
    public string? StimulatorBuffs { get; set; }

    [JsonPropertyName("effects_health")]
    public object? EffectsHealth { get; set; } // TODO: object here

    [JsonPropertyName("effects_damage")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Dictionary<string, EffectDamageProps>? EffectsDamage { get; set; }

    [JsonPropertyName("maximumNumberOfUsage")]
    public double? MaximumNumberOfUsage { get; set; }

    [JsonPropertyName("knifeHitDelay")]
    public double? KnifeHitDelay { get; set; }

    [JsonPropertyName("knifeHitSlashRate")]
    public double? KnifeHitSlashRate { get; set; }

    [JsonPropertyName("knifeHitStabRate")]
    public double? KnifeHitStabRate { get; set; }

    [JsonPropertyName("knifeHitRadius")]
    public double? KnifeHitRadius { get; set; }

    [JsonPropertyName("knifeHitSlashDam")]
    public double? KnifeHitSlashDam { get; set; }

    [JsonPropertyName("knifeHitStabDam")]
    public double? KnifeHitStabDam { get; set; }

    [JsonPropertyName("knifeDurab")]
    public double? KnifeDurab { get; set; }

    [JsonPropertyName("primaryDistance")]
    public double? PrimaryDistance { get; set; }

    [JsonPropertyName("secondryDistance")]
    public double? SecondryDistance { get; set; }

    [JsonPropertyName("slashPenetration")]
    public double? SlashPenetration { get; set; }

    [JsonPropertyName("stabPenetration")]
    public double? StabPenetration { get; set; }

    [JsonPropertyName("primaryConsumption")]
    public double? PrimaryConsumption { get; set; }

    [JsonPropertyName("secondryConsumption")]
    public double? SecondryConsumption { get; set; }

    [JsonPropertyName("deflectionConsumption")]
    public double? DeflectionConsumption { get; set; }

    [JsonPropertyName("appliedTrunkRotation")]
    public XYZ? AppliedTrunkRotation { get; set; }

    [JsonPropertyName("appliedHeadRotation")]
    public XYZ? AppliedHeadRotation { get; set; }

    [JsonPropertyName("displayOnModel")]
    public bool? DisplayOnModel { get; set; }

    [JsonPropertyName("additionalAnimationLayer")]
    public double? AdditionalAnimationLayer { get; set; }

    [JsonPropertyName("staminaBurnRate")]
    public double? StaminaBurnRate { get; set; }

    [JsonPropertyName("colliderScaleMultiplier")]
    public XYZ? ColliderScaleMultiplier { get; set; }

    [JsonPropertyName("configPathStr")]
    public string? ConfigPathStr { get; set; }

    [JsonPropertyName("maxMarkersCount")]
    public double? MaxMarkersCount { get; set; }

    [JsonPropertyName("scaleMin")]
    public double? ScaleMin { get; set; }

    [JsonPropertyName("scaleMax")]
    public double? ScaleMax { get; set; }

    [JsonPropertyName("medUseTime")]
    public double? MedUseTime { get; set; }

    [JsonPropertyName("medEffectType")]
    public string? MedEffectType { get; set; }

    [JsonPropertyName("maxHpResource")]
    public double? MaxHpResource { get; set; }

    [JsonPropertyName("hpResourceRate")]
    public double? HpResourceRate { get; set; }

    [JsonPropertyName("apResource")]
    public double? ApResource { get; set; }

    [JsonPropertyName("krResource")]
    public double? KrResource { get; set; }

    [JsonPropertyName("maxOpticZoom")]
    public double? MaxOpticZoom { get; set; }

    [JsonPropertyName("maxRepairResource")]
    public double? MaxRepairResource { get; set; }

    [JsonPropertyName("targetItemFilter")]
    public List<string>? TargetItemFilter { get; set; }

    [JsonPropertyName("repairQuality")]
    public double? RepairQuality { get; set; }

    [JsonPropertyName("repairType")]
    public string? RepairType { get; set; }

    [JsonPropertyName("stackMinRandom")]
    public double? StackMinRandom { get; set; }

    [JsonPropertyName("stackMaxRandom")]
    public double? StackMaxRandom { get; set; }

    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    [JsonPropertyName("initialSpeed")]
    public double? InitialSpeed { get; set; }

    [JsonPropertyName("ballisticCoefficient")]
    public double? BallisticCoefficient { get; set; }

    [JsonPropertyName("bulletMassGram")]
    public double? BulletMassGram { get; set; }

    [JsonPropertyName("bulletDiameterMillimeters")]
    public double? BulletDiameterMillimeters { get; set; }

    [JsonPropertyName("damage")]
    public double? Damage { get; set; }

    [JsonPropertyName("ammoAccr")]
    public double? AmmoAccr { get; set; }

    [JsonPropertyName("ammoRec")]
    public double? AmmoRec { get; set; }

    [JsonPropertyName("ammoDist")]
    public double? AmmoDist { get; set; }

    [JsonPropertyName("buckshotBullets")]
    public double? BuckshotBullets { get; set; }

    [JsonPropertyName("penetrationPower")]
    public double? PenetrationPower { get; set; }

    [JsonPropertyName("penetrationPowerDeviation")]
    public double? PenetrationPowerDeviation { get; set; }

    [JsonPropertyName("ammoHear")]
    public double? AmmoHear { get; set; }

    [JsonPropertyName("ammoSfx")]
    public string? AmmoSfx { get; set; }

    [JsonPropertyName("misfireChance")]
    public double? MisfireChance { get; set; }

    [JsonPropertyName("minFragmentsCount")]
    public double? MinFragmentsCount { get; set; }

    [JsonPropertyName("maxFragmentsCount")]
    public double? MaxFragmentsCount { get; set; }

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

    [JsonPropertyName("projectileCount")]
    public double? ProjectileCount { get; set; }

    [JsonPropertyName("penetrationChanceObstacle")]
    public double? PenetrationChanceObstacle { get; set; }

    [JsonPropertyName("penetrationDamageMod")]
    public double? PenetrationDamageMod { get; set; }

    [JsonPropertyName("ricochetChance")]
    public double? RicochetChance { get; set; }

    [JsonPropertyName("fragmentationChance")]
    public double? FragmentationChance { get; set; }

    [JsonPropertyName("deterioration")]
    public double? Deterioration { get; set; }

    [JsonPropertyName("speedRetardation")]
    public double? SpeedRetardation { get; set; }

    [JsonPropertyName("tracer")]
    public bool? Tracer { get; set; }

    [JsonPropertyName("tracerColor")]
    public string? TracerColor { get; set; }

    [JsonPropertyName("tracerDistance")]
    public double? TracerDistance { get; set; }

    [JsonPropertyName("armorDamage")]
    public double? ArmorDamage { get; set; }

    [JsonPropertyName("caliber")]
    public string? Caliber { get; set; }

    [JsonPropertyName("staminaBurnPerDamage")]
    public double? StaminaBurnPerDamage { get; set; }

    [JsonPropertyName("heavyBleedingDelta")]
    public double? HeavyBleedingDelta { get; set; }

    [JsonPropertyName("lightBleedingDelta")]
    public double? LightBleedingDelta { get; set; }

    [JsonPropertyName("showBullet")]
    public bool? ShowBullet { get; set; }

    [JsonPropertyName("hasGrenaderComponent")]
    public bool? HasGrenaderComponent { get; set; }

    [JsonPropertyName("fuzeArmTimeSec")]
    public double? FuzeArmTimeSec { get; set; }

    [JsonPropertyName("explosionStrength")]
    public double? ExplosionStrength { get; set; }

    [JsonPropertyName("minExplosionDistance")]
    public double? MinExplosionDistance { get; set; }

    [JsonPropertyName("maxExplosionDistance")]
    public double? MaxExplosionDistance { get; set; }

    [JsonPropertyName("fragmentsCount")]
    public double? FragmentsCount { get; set; }

    [JsonPropertyName("fragmentType")]
    public string? FragmentType { get; set; }

    [JsonPropertyName("showHitEffectOnExplode")]
    public bool? ShowHitEffectOnExplode { get; set; }

    [JsonPropertyName("explosionType")]
    public string? ExplosionType { get; set; }

    [JsonPropertyName("ammoLifeTimeSec")]
    public double? AmmoLifeTimeSec { get; set; }

    [JsonPropertyName("ammoTooltipClass")]
    public string? AmmoTooltipClass { get; set; }

    [JsonPropertyName("contusion")]
    public XYZ? Contusion { get; set; }

    [JsonPropertyName("armorDistanceDistanceDamage")]
    public XYZ? ArmorDistanceDistanceDamage { get; set; }

    [JsonPropertyName("blindness")]
    public XYZ? Blindness { get; set; }

    [JsonPropertyName("isLightAndSoundShot")]
    public bool? IsLightAndSoundShot { get; set; }

    [JsonPropertyName("lightAndSoundShotAngle")]
    public double? LightAndSoundShotAngle { get; set; }

    [JsonPropertyName("lightAndSoundShotSelfContusionTime")]
    public double? LightAndSoundShotSelfContusionTime { get; set; }

    [JsonPropertyName("lightAndSoundShotSelfContusionStrength")]
    public double? LightAndSoundShotSelfContusionStrength { get; set; }

    [JsonPropertyName("malfMisfireChance")]
    public double? MalfMisfireChance { get; set; }

    [JsonPropertyName("malfFeedChance")]
    public double? MalfFeedChance { get; set; }

    [JsonPropertyName("stackSlots")]
    public List<StackSlot>? StackSlots { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("eqMin")]
    public double? EqMin { get; set; }

    [JsonPropertyName("eqMax")]
    public double? EqMax { get; set; }

    [JsonPropertyName("rate")]
    public double? Rate { get; set; }

    [JsonPropertyName("throwType")]
    public string? ThrowType { get; set; }

    [JsonPropertyName("explDelay")]
    public double? ExplDelay { get; set; }

    [JsonPropertyName("strength")]
    public double? Strength { get; set; }

    [JsonPropertyName("contusionDistance")]
    public double? ContusionDistance { get; set; }

    [JsonPropertyName("throwDamMax")]
    public double? ThrowDamMax { get; set; }

    [JsonPropertyName("emitTime")]
    public double? EmitTime { get; set; }

    [JsonPropertyName("canBeHiddenDuringThrow")]
    public bool? CanBeHiddenDuringThrow { get; set; }

    [JsonPropertyName("canPlantOnGround")]
    public bool? CanPlantOnGround { get; set; }

    [JsonPropertyName("minTimeToContactExplode")]
    public double? MinTimeToContactExplode { get; set; }

    [JsonPropertyName("explosionEffectType")]
    public string? ExplosionEffectType { get; set; }

    [JsonPropertyName("linkedWeapon")]
    public string? LinkedWeapon { get; set; }

    [JsonPropertyName("useAmmoWithoutShell")]
    public bool? UseAmmoWithoutShell { get; set; }

    [JsonPropertyName("randomLootSettings")]
    public RandomLootSettings? RandomLootSettings { get; set; }

    [JsonPropertyName("recoilDampingHandRotation")]
    public double? RecoilDampingHandRotation { get; set; }

    [JsonPropertyName("leanWeaponAgainstBody")]
    public bool? LeanWeaponAgainstBody { get; set; }

    [JsonPropertyName("removeShellAfterFire")]
    public bool? RemoveShellAfterFire { get; set; }

    [JsonPropertyName("repairStrategyTypes")]
    public List<string>? RepairStrategyTypes { get; set; }

    [JsonPropertyName("isEncoded")]
    public bool? IsEncoded { get; set; }

    [JsonPropertyName("layoutName")]
    public string? LayoutName { get; set; }

    [JsonPropertyName("lower75Prefab")]
    public Prefab? Lower75Prefab { get; set; }

    [JsonPropertyName("maxUsages")]
    public double? MaxUsages { get; set; }

    [JsonPropertyName("scavKillExpPenalty")]
    public double? ScavKillExpPenalty { get; set; }

    [JsonPropertyName("scavKillExpPenaltyPVE")]
    public double? ScavKillExpPenaltyPVE { get; set; }

    [JsonPropertyName("scavKillStandingPenalty")]
    public double? ScavKillStandingPenalty { get; set; }

    [JsonPropertyName("scavKillStandingPenaltyPVE")]
    public double? ScavKillStandingPenaltyPVE { get; set; }

    [JsonPropertyName("tradersDiscount")]
    public double? TradersDiscount { get; set; }

    [JsonPropertyName("tradersDiscountPVE")]
    public double? TradersDiscountPVE { get; set; }
    
    [JsonPropertyName("AvailableAsDefault")]
    public bool? AvailableAsDefault { get; set; }
    
    [JsonPropertyName("ProfileVersions")]
    public List<string>? ProfileVersions { get; set; }
    
    [JsonPropertyName("Side")]
    public List<string>? Side { get; set; }
    
    [JsonPropertyName("BodyPart")]
    public string? BodyPart { get; set; }
    
    [JsonPropertyName("IntegratedArmorVest")]
    public bool? IntegratedArmorVest { get; set; }
    
    [JsonPropertyName("WatchPosition")]
    public XYZ? WatchPosition { get; set; }
    
    [JsonPropertyName("WatchPrefab")]
    public Prefab? WatchPrefab { get; set; }
    
    [JsonPropertyName("WatchRotation")]
    public XYZ? WatchRotation { get; set; }
    
    [JsonPropertyName("Game")]
    public List<string>? Game { get; set; }
    
    [JsonPropertyName("Body")]
    public string? Body { get; set; }
    
    [JsonPropertyName("Hands")]
    public string? Hands { get; set; }
    
    [JsonPropertyName("Feet")]
    public string? Feet { get; set; }
    
    [JsonExtensionData] 
    public Dictionary<string, object> OtherProperties { get; set; } 
}

public class WeaponRecoilSettings
{
    [JsonPropertyName("Enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("Values")]
    public List<WeaponRecoilSettingValues>? Values { get; set; }
}

public class WeaponRecoilSettingValues
{
    [JsonPropertyName("Enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("Process")]
    public WeaponRecoilProcess? Process { get; set; }

    [JsonPropertyName("Target")]
    public string? Target { get; set; }
}

public class WeaponRecoilProcess
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

public class WeaponRecoilTransformationCurve
{
    [JsonPropertyName("Keys")]
    public List<WeaponRecoilTransformationCurveKey>? Keys { get; set; }
}

public class WeaponRecoilTransformationCurveKey
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

public class HealthEffect
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public class Prefab
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("rcid")]
    public string? Rcid { get; set; }
}

public class Grid
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

public class GridProps
{
    [JsonPropertyName("filters")]
    public List<GridFilter>? Filters { get; set; }

    [JsonPropertyName("cellsH")]
    public double? CellsH { get; set; }

    [JsonPropertyName("cellsV")]
    public double? CellsV { get; set; }

    [JsonPropertyName("minCount")]
    public double? MinCount { get; set; }

    [JsonPropertyName("maxCount")]
    public double? MaxCount { get; set; }

    [JsonPropertyName("maxWeight")]
    public double? MaxWeight { get; set; }

    [JsonPropertyName("isSortingTable")]
    public bool? IsSortingTable { get; set; }
}

public class GridFilter
{
    [JsonPropertyName("Filter")]
    public List<string>? Filter { get; set; }

    [JsonPropertyName("ExcludedFilter")]
    public List<string>? ExcludedFilter { get; set; }

    [JsonPropertyName("locked")]
    public bool? Locked { get; set; }
}

public class Slot
{
    [JsonPropertyName("_name")]
    public string? Name { get; set; }

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
    public string? Proto { get; set; }
}

public class SlotProps
{
    [JsonPropertyName("filters")]
    public List<SlotFilter>? Filters { get; set; }

    [JsonPropertyName("MaxStackCount")]
    public double? MaxStackCount { get; set; }
}

public class SlotFilter
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
    public List<string>? Filter { get; set; }

    [JsonPropertyName("AnimationIndex")]
    public double? AnimationIndex { get; set; }
    
    [JsonPropertyName("MaxStackCount")]
    public double? MaxStackCount { get; set; }
    
    [JsonPropertyName("bluntDamageReduceFromSoftArmor")]
    public bool? BluntDamageReduceFromSoftArmor { get; set; }
}

public class StackSlot
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

public class StackSlotProps
{
    [JsonPropertyName("filters")]
    public List<SlotFilter>? Filters { get; set; }
}

public class RandomLootSettings
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

public class RandomLootExcluded
{
    [JsonPropertyName("categoryTemplates")]
    public List<object>? CategoryTemplates { get; set; } // TODO: object here

    [JsonPropertyName("rarity")]
    public List<string>? Rarity { get; set; }

    [JsonPropertyName("templates")]
    public List<object>? Templates { get; set; } // TODO: object here
}

public class EffectsHealth
{
    [JsonPropertyName("Energy")]
    public EffectsHealthProps? Energy { get; set; }

    [JsonPropertyName("Hydration")]
    public EffectsHealthProps? Hydration { get; set; }
}

public class EffectsHealthProps
{
    [JsonPropertyName("value")]
    public double? Value { get; set; }
}

public class EffectsDamage
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

public class EffectDamageProps
{
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

public class Color
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

public class ShotsGroupSettings
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
    ITEM = 2
}
