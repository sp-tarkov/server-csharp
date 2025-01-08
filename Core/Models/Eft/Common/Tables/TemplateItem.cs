using System.Text.Json.Serialization;

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
    public ItemType? Type { get; set; }

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
    public int? BeltMagazineRefreshCount { get; set; }

    [JsonPropertyName("ChangePriceCoef")]
    public int? ChangePriceCoef { get; set; }

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
    public int? Weight { get; set; }

    [JsonPropertyName("BackgroundColor")]
    public string? BackgroundColor { get; set; }

    [JsonPropertyName("Width")]
    public int? Width { get; set; }

    [JsonPropertyName("Height")]
    public int? Height { get; set; }

    [JsonPropertyName("StackMaxSize")]
    public int? StackMaxSize { get; set; }

    [JsonPropertyName("Rarity")]
    public string? Rarity { get; set; }

    [JsonPropertyName("SpawnChance")]
    public int? SpawnChance { get; set; }

    [JsonPropertyName("CreditsPrice")]
    public int? CreditsPrice { get; set; }

    [JsonPropertyName("ItemSound")]
    public string? ItemSound { get; set; }

    [JsonPropertyName("Prefab")]
    public Prefab? Prefab { get; set; }

    [JsonPropertyName("UsePrefab")]
    public Prefab? UsePrefab { get; set; }

    [JsonPropertyName("airDropTemplateId")]
    public string? AirDropTemplateId { get; set; }

    [JsonPropertyName("StackObjectsCount")]
    public int? StackObjectsCount { get; set; }

    [JsonPropertyName("NotShownInSlot")]
    public bool? NotShownInSlot { get; set; }

    [JsonPropertyName("ExaminedByDefault")]
    public bool? ExaminedByDefault { get; set; }

    [JsonPropertyName("ExamineTime")]
    public int? ExamineTime { get; set; }

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
    public int? QuestStashMaxCount { get; set; }

    [JsonPropertyName("LootExperience")]
    public int? LootExperience { get; set; }

    [JsonPropertyName("ExamineExperience")]
    public int? ExamineExperience { get; set; }

    [JsonPropertyName("HideEntrails")]
    public bool? HideEntrails { get; set; }

    [JsonPropertyName("InsuranceDisabled")]
    public bool? InsuranceDisabled { get; set; }

    [JsonPropertyName("RepairCost")]
    public int? RepairCost { get; set; }

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
    public int? AnimationVariantsNumber { get; set; }

    [JsonPropertyName("DiscardingBlock")]
    public bool? DiscardingBlock { get; set; }

    [JsonPropertyName("DropSoundType")]
    public string? DropSoundType { get; set; }

    [JsonPropertyName("RagFairCommissionModifier")]
    public int? RagFairCommissionModifier { get; set; }

    [JsonPropertyName("RarityPvE")]
    public string? RarityPvE { get; set; }

    [JsonPropertyName("IsAlwaysAvailableForInsurance")]
    public bool? IsAlwaysAvailableForInsurance { get; set; }

    [JsonPropertyName("DiscardLimit")]
    public int? DiscardLimit { get; set; }

    [JsonPropertyName("MaxResource")]
    public int? MaxResource { get; set; }

    [JsonPropertyName("Resource")]
    public int? Resource { get; set; }

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
    public int? TagColor { get; set; }

    [JsonPropertyName("TagName")]
    public string? TagName { get; set; }

    [JsonPropertyName("Durability")]
    public int? Durability { get; set; }

    [JsonPropertyName("Accuracy")]
    public int? Accuracy { get; set; }

    [JsonPropertyName("Recoil")]
    public int? Recoil { get; set; }

    [JsonPropertyName("Loudness")]
    public int? Loudness { get; set; }

    [JsonPropertyName("EffectiveDistance")]
    public int? EffectiveDistance { get; set; }

    [JsonPropertyName("Ergonomics")]
    public int? Ergonomics { get; set; }

    [JsonPropertyName("Velocity")]
    public int? Velocity { get; set; }

    [JsonPropertyName("WeaponRecoilSettings")]
    public WeaponRecoilSettings? WeaponRecoilSettings { get; set; }

    [JsonPropertyName("WithAnimatorAiming")]
    public bool? WithAnimatorAiming { get; set; }

    [JsonPropertyName("RaidModdable")]
    public bool? RaidModdable { get; set; }

    [JsonPropertyName("ToolModdable")]
    public bool? ToolModdable { get; set; }

    [JsonPropertyName("UniqueAnimationModID")]
    public int? UniqueAnimationModID { get; set; }

    [JsonPropertyName("BlocksFolding")]
    public bool? BlocksFolding { get; set; }

    [JsonPropertyName("BlocksCollapsible")]
    public bool? BlocksCollapsible { get; set; }

    [JsonPropertyName("IsAnimated")]
    public bool? IsAnimated { get; set; }

    [JsonPropertyName("HasShoulderContact")]
    public bool? HasShoulderContact { get; set; }

    [JsonPropertyName("SightingRange")]
    public int? SightingRange { get; set; }

    [JsonPropertyName("ZoomSensitivity")]
    public int? ZoomSensitivity { get; set; }

    [JsonPropertyName("DoubleActionAccuracyPenaltyMult")]
    public int? DoubleActionAccuracyPenaltyMult { get; set; }

    [JsonPropertyName("ModesCount")]
    public object? ModesCount { get; set; } // TODO: object here

    [JsonPropertyName("DurabilityBurnModificator")]
    public int? DurabilityBurnModificator { get; set; }

    [JsonPropertyName("HeatFactor")]
    public int? HeatFactor { get; set; }

    [JsonPropertyName("CoolFactor")]
    public int? CoolFactor { get; set; }

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
    public int? AimingSensitivity { get; set; }

    [JsonPropertyName("SightModesCount")]
    public int? SightModesCount { get; set; }

    [JsonPropertyName("OpticCalibrationDistances")]
    public List<int>? OpticCalibrationDistances { get; set; }

    [JsonPropertyName("ScopesCount")]
    public int? ScopesCount { get; set; }

    [JsonPropertyName("AimSensitivity")]
    public object? AimSensitivity { get; set; } // TODO: object here

    [JsonPropertyName("Zooms")]
    public List<List<int>>? Zooms { get; set; }

    [JsonPropertyName("CalibrationDistances")]
    public List<List<int>>? CalibrationDistances { get; set; }

    [JsonPropertyName("Intensity")]
    public int? Intensity { get; set; }

    [JsonPropertyName("Mask")]
    public string? Mask { get; set; }

    [JsonPropertyName("MaskSize")]
    public int? MaskSize { get; set; }

    [JsonPropertyName("IsMagazineForStationaryWeapon")]
    public bool? IsMagazineForStationaryWeapon { get; set; }

    [JsonPropertyName("NoiseIntensity")]
    public int? NoiseIntensity { get; set; }

    [JsonPropertyName("NoiseScale")]
    public int? NoiseScale { get; set; }

    [JsonPropertyName("Color")]
    public Color? Color { get; set; }

    [JsonPropertyName("DiffuseIntensity")]
    public int? DiffuseIntensity { get; set; }

    [JsonPropertyName("MagazineWithBelt")]
    public bool? MagazineWithBelt { get; set; }

    [JsonPropertyName("HasHinge")]
    public bool? HasHinge { get; set; }

    [JsonPropertyName("RampPalette")]
    public string? RampPalette { get; set; }

    [JsonPropertyName("DepthFade")]
    public int? DepthFade { get; set; }

    [JsonPropertyName("RoughnessCoef")]
    public int? RoughnessCoef { get; set; }

    [JsonPropertyName("SpecularCoef")]
    public int? SpecularCoef { get; set; }

    [JsonPropertyName("MainTexColorCoef")]
    public int? MainTexColorCoef { get; set; }

    [JsonPropertyName("MinimumTemperatureValue")]
    public int? MinimumTemperatureValue { get; set; }

    [JsonPropertyName("RampShift")]
    public int? RampShift { get; set; }

    [JsonPropertyName("HeatMin")]
    public int? HeatMin { get; set; }

    [JsonPropertyName("ColdMax")]
    public int? ColdMax { get; set; }

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
    public int? PixelationBlockCount { get; set; }

    [JsonPropertyName("ShiftsAimCamera")]
    public int? ShiftsAimCamera { get; set; }

    [JsonPropertyName("magAnimationIndex")]
    public int? MagAnimationIndex { get; set; }

    [JsonPropertyName("Cartridges")]
    public List<Slot>? Cartridges { get; set; }

    [JsonPropertyName("CanFast")]
    public bool? CanFast { get; set; }

    [JsonPropertyName("CanHit")]
    public bool? CanHit { get; set; }

    [JsonPropertyName("canAdmin")]
    public bool? CanAdmin { get; set; }

    [JsonPropertyName("loadUnloadModifier")]
    public int? LoadUnloadModifier { get; set; }

    [JsonPropertyName("checkTimeModifier")]
    public int? CheckTimeModifier { get; set; }

    [JsonPropertyName("checkOverride")]
    public int? CheckOverride { get; set; }

    [JsonPropertyName("reloadMagType")]
    public string? ReloadMagType { get; set; }

    [JsonPropertyName("visibleAmmoRangesString")]
    public string? VisibleAmmoRangesString { get; set; }

    [JsonPropertyName("malfunctionChance")]
    public int? MalfunctionChance { get; set; }

    [JsonPropertyName("isShoulderContact")]
    public bool? IsShoulderContact { get; set; }

    [JsonPropertyName("foldable")]
    public bool? Foldable { get; set; }

    [JsonPropertyName("retractable")]
    public bool? Retractable { get; set; }

    [JsonPropertyName("sizeReduceRight")]
    public int? SizeReduceRight { get; set; }

    [JsonPropertyName("centerOfImpact")]
    public int? CenterOfImpact { get; set; }

    [JsonPropertyName("isSilencer")]
    public bool? IsSilencer { get; set; }

    [JsonPropertyName("deviationCurve")]
    public int? DeviationCurve { get; set; }

    [JsonPropertyName("deviationMax")]
    public int? DeviationMax { get; set; }

    [JsonPropertyName("searchSound")]
    public string? SearchSound { get; set; }

    [JsonPropertyName("blocksArmorVest")]
    public bool? BlocksArmorVest { get; set; }

    [JsonPropertyName("speedPenaltyPercent")]
    public int? SpeedPenaltyPercent { get; set; }

    [JsonPropertyName("gridLayoutName")]
    public string? GridLayoutName { get; set; }

    [JsonPropertyName("containerSpawnChanceModifier")]
    public int? ContainerSpawnChanceModifier { get; set; }

    [JsonPropertyName("spawnExcludedFilter")]
    public List<string>? SpawnExcludedFilter { get; set; }

    [JsonPropertyName("spawnFilter")]
    public List<object>? SpawnFilter { get; set; } // TODO: object here

    [JsonPropertyName("containType")]
    public List<object>? ContainType { get; set; } // TODO: object here

    [JsonPropertyName("sizeWidth")]
    public int? SizeWidth { get; set; }

    [JsonPropertyName("sizeHeight")]
    public int? SizeHeight { get; set; }

    [JsonPropertyName("isSecured")]
    public bool? IsSecured { get; set; }

    [JsonPropertyName("spawnTypes")]
    public string? SpawnTypes { get; set; }

    [JsonPropertyName("lootFilter")]
    public List<object>? LootFilter { get; set; } // TODO: object here

    [JsonPropertyName("spawnRarity")]
    public string? SpawnRarity { get; set; }

    [JsonPropertyName("minCountSpawn")]
    public int? MinCountSpawn { get; set; }

    [JsonPropertyName("maxCountSpawn")]
    public int? MaxCountSpawn { get; set; }

    [JsonPropertyName("openedByKeyID")]
    public List<string>? OpenedByKeyID { get; set; }

    [JsonPropertyName("rigLayoutName")]
    public string? RigLayoutName { get; set; }

    [JsonPropertyName("maxDurability")]
    public int? MaxDurability { get; set; }

    [JsonPropertyName("armorZone")]
    public List<string>? ArmorZone { get; set; }

    [JsonPropertyName("armorClass")]
    public object? ArmorClass { get; set; } // TODO: object here

    [JsonPropertyName("armorColliders")]
    public List<string>? ArmorColliders { get; set; }

    [JsonPropertyName("armorPlateColliders")]
    public List<string>? ArmorPlateColliders { get; set; }

    [JsonPropertyName("bluntDamageReduceFromSoftArmor")]
    public bool? BluntDamageReduceFromSoftArmor { get; set; }

    [JsonPropertyName("mousePenalty")]
    public int? MousePenalty { get; set; }

    [JsonPropertyName("weaponErgonomicPenalty")]
    public int? WeaponErgonomicPenalty { get; set; }

    [JsonPropertyName("bluntThroughput")]
    public int? BluntThroughput { get; set; }

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
    public int? OperatingResource { get; set; }

    [JsonPropertyName("postRecoilHorizontalRangeHandRotation")]
    public XYZ? PostRecoilHorizontalRangeHandRotation { get; set; }

    [JsonPropertyName("postRecoilVerticalRangeHandRotation")]
    public XYZ? PostRecoilVerticalRangeHandRotation { get; set; }

    [JsonPropertyName("progressRecoilAngleOnStable")]
    public XYZ? ProgressRecoilAngleOnStable { get; set; }

    [JsonPropertyName("repairComplexity")]
    public int? RepairComplexity { get; set; }

    [JsonPropertyName("durabSpawnMin")]
    public int? DurabSpawnMin { get; set; }

    [JsonPropertyName("durabSpawnMax")]
    public int? DurabSpawnMax { get; set; }

    [JsonPropertyName("isFastReload")]
    public bool? IsFastReload { get; set; }

    [JsonPropertyName("recoilForceUp")]
    public int? RecoilForceUp { get; set; }

    [JsonPropertyName("recoilForceBack")]
    public int? RecoilForceBack { get; set; }

    [JsonPropertyName("recoilAngle")]
    public int? RecoilAngle { get; set; }

    [JsonPropertyName("recoilCamera")]
    public int? RecoilCamera { get; set; }

    [JsonPropertyName("recoilCategoryMultiplierHandRotation")]
    public int? RecoilCategoryMultiplierHandRotation { get; set; }

    [JsonPropertyName("weapFireType")]
    public List<string>? WeapFireType { get; set; }

    [JsonPropertyName("recolDispersion")]
    public int? RecolDispersion { get; set; }

    [JsonPropertyName("singleFireRate")]
    public int? SingleFireRate { get; set; }

    [JsonPropertyName("canQueueSecondShot")]
    public bool? CanQueueSecondShot { get; set; }

    [JsonPropertyName("bFirerate")]
    public int? BFirerate { get; set; }

    [JsonPropertyName("bEffDist")]
    public int? BEffDist { get; set; }

    [JsonPropertyName("bHearDist")]
    public int? BHearDist { get; set; }

    [JsonPropertyName("blockLeftStance")]
    public bool? BlockLeftStance { get; set; }

    [JsonPropertyName("isChamberLoad")]
    public bool? IsChamberLoad { get; set; }

    [JsonPropertyName("chamberAmmoCount")]
    public int? ChamberAmmoCount { get; set; }

    [JsonPropertyName("isBoltCatch")]
    public bool? IsBoltCatch { get; set; }

    [JsonPropertyName("defMagType")]
    public string? DefMagType { get; set; }

    [JsonPropertyName("defAmmo")]
    public string? DefAmmo { get; set; }

    [JsonPropertyName("adjustCollimatorsToTrajectory")]
    public bool? AdjustCollimatorsToTrajectory { get; set; }

    [JsonPropertyName("shotgunDispersion")]
    public int? ShotgunDispersion { get; set; }

    [JsonPropertyName("chambers")]
    public List<Slot>? Chambers { get; set; }

    [JsonPropertyName("cameraSnap")]
    public int? CameraSnap { get; set; }

    [JsonPropertyName("cameraToWeaponAngleSpeedRange")]
    public XYZ? CameraToWeaponAngleSpeedRange { get; set; }

    [JsonPropertyName("cameraToWeaponAngleStep")]
    public int? CameraToWeaponAngleStep { get; set; }

    [JsonPropertyName("reloadMode")]
    public string? ReloadMode { get; set; }

    [JsonPropertyName("aimPlane")]
    public int? AimPlane { get; set; }

    [JsonPropertyName("tacticalReloadStiffnes")]
    public XYZ? TacticalReloadStiffnes { get; set; }

    [JsonPropertyName("tacticalReloadFixation")]
    public int? TacticalReloadFixation { get; set; }

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
    public int? MinRepairDegradation { get; set; }

    [JsonPropertyName("maxRepairDegradation")]
    public int? MaxRepairDegradation { get; set; }

    [JsonPropertyName("ironSightRange")]
    public int? IronSightRange { get; set; }

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
    public int? HipAccuracyRestorationDelay { get; set; }

    [JsonPropertyName("hipAccuracyRestorationSpeed")]
    public int? HipAccuracyRestorationSpeed { get; set; }

    [JsonPropertyName("hipInnaccuracyGain")]
    public int? HipInnaccuracyGain { get; set; }

    [JsonPropertyName("manualBoltCatch")]
    public bool? ManualBoltCatch { get; set; }

    [JsonPropertyName("burstShotsCount")]
    public int? BurstShotsCount { get; set; }

    [JsonPropertyName("baseMalfunctionChance")]
    public int? BaseMalfunctionChance { get; set; }

    [JsonPropertyName("allowJam")]
    public bool? AllowJam { get; set; }

    [JsonPropertyName("allowFeed")]
    public bool? AllowFeed { get; set; }

    [JsonPropertyName("allowMisfire")]
    public bool? AllowMisfire { get; set; }

    [JsonPropertyName("allowSlide")]
    public bool? AllowSlide { get; set; }

    [JsonPropertyName("durabilityBurnRatio")]
    public int? DurabilityBurnRatio { get; set; }

    [JsonPropertyName("heatFactorGun")]
    public int? HeatFactorGun { get; set; }

    [JsonPropertyName("coolFactorGun")]
    public int? CoolFactorGun { get; set; }

    [JsonPropertyName("coolFactorGunMods")]
    public int? CoolFactorGunMods { get; set; }

    [JsonPropertyName("heatFactorByShot")]
    public int? HeatFactorByShot { get; set; }

    [JsonPropertyName("allowOverheat")]
    public bool? AllowOverheat { get; set; }

    [JsonPropertyName("doubleActionAccuracyPenalty")]
    public int? DoubleActionAccuracyPenalty { get; set; }

    [JsonPropertyName("recoilPosZMult")]
    public int? RecoilPosZMult { get; set; }

    [JsonPropertyName("recoilReturnPathDampingHandRotation")]
    public int? RecoilReturnPathDampingHandRotation { get; set; }

    [JsonPropertyName("recoilReturnPathOffsetHandRotation")]
    public int? RecoilReturnPathOffsetHandRotation { get; set; }

    [JsonPropertyName("recoilReturnSpeedHandRotation")]
    public int? RecoilReturnSpeedHandRotation { get; set; }

    [JsonPropertyName("recoilStableAngleIncreaseStep")]
    public int? RecoilStableAngleIncreaseStep { get; set; }

    [JsonPropertyName("recoilStableIndexShot")]
    public int? RecoilStableIndexShot { get; set; }

    [JsonPropertyName("minRepairKitDegradation")]
    public int? MinRepairKitDegradation { get; set; }

    [JsonPropertyName("maxRepairKitDegradation")]
    public int? MaxRepairKitDegradation { get; set; }

    [JsonPropertyName("mountCameraSnapMultiplier")]
    public int? MountCameraSnapMultiplier { get; set; }

    [JsonPropertyName("mountHorizontalRecoilMultiplier")]
    public int? MountHorizontalRecoilMultiplier { get; set; }

    [JsonPropertyName("mountReturnSpeedHandMultiplier")]
    public int? MountReturnSpeedHandMultiplier { get; set; }

    [JsonPropertyName("mountVerticalRecoilMultiplier")]
    public int? MountVerticalRecoilMultiplier { get; set; }

    [JsonPropertyName("mountingHorizontalOutOfBreathMultiplier")]
    public int? MountingHorizontalOutOfBreathMultiplier { get; set; }

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
    public int? Indestructibility { get; set; }

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
    public int? BlindnessProtection { get; set; }

    [JsonPropertyName("distortion")]
    public int? Distortion { get; set; }

    [JsonPropertyName("compressorTreshold")]
    public int? CompressorTreshold { get; set; }

    [JsonPropertyName("compressorAttack")]
    public int? CompressorAttack { get; set; }

    [JsonPropertyName("compressorRelease")]
    public int? CompressorRelease { get; set; }

    [JsonPropertyName("compressorGain")]
    public int? CompressorGain { get; set; }

    [JsonPropertyName("cutoffFreq")]
    public int? CutoffFreq { get; set; }

    [JsonPropertyName("resonance")]
    public int? Resonance { get; set; }

    [JsonPropertyName("rolloffMultiplier")]
    public int? RolloffMultiplier { get; set; }

    [JsonPropertyName("reverbVolume")]
    public int? ReverbVolume { get; set; }

    [JsonPropertyName("compressorVolume")]
    public int? CompressorVolume { get; set; }

    [JsonPropertyName("ambientVolume")]
    public int? AmbientVolume { get; set; }

    [JsonPropertyName("dryVolume")]
    public int? DryVolume { get; set; }

    [JsonPropertyName("highFrequenciesGain")]
    public int? HighFrequenciesGain { get; set; }

    [JsonPropertyName("foodUseTime")]
    public int? FoodUseTime { get; set; }

    [JsonPropertyName("foodEffectType")]
    public string? FoodEffectType { get; set; }

    [JsonPropertyName("stimulatorBuffs")]
    public string? StimulatorBuffs { get; set; }

    [JsonPropertyName("effects_health")]
    public object? EffectsHealth { get; set; } // TODO: object here

    [JsonPropertyName("effects_damage")]
    public Dictionary<string, EffectDamageProps>? EffectsDamage { get; set; }

    [JsonPropertyName("maximumNumberOfUsage")]
    public int? MaximumNumberOfUsage { get; set; }

    [JsonPropertyName("knifeHitDelay")]
    public int? KnifeHitDelay { get; set; }

    [JsonPropertyName("knifeHitSlashRate")]
    public int? KnifeHitSlashRate { get; set; }

    [JsonPropertyName("knifeHitStabRate")]
    public int? KnifeHitStabRate { get; set; }

    [JsonPropertyName("knifeHitRadius")]
    public int? KnifeHitRadius { get; set; }

    [JsonPropertyName("knifeHitSlashDam")]
    public int? KnifeHitSlashDam { get; set; }

    [JsonPropertyName("knifeHitStabDam")]
    public int? KnifeHitStabDam { get; set; }

    [JsonPropertyName("knifeDurab")]
    public int? KnifeDurab { get; set; }

    [JsonPropertyName("primaryDistance")]
    public int? PrimaryDistance { get; set; }

    [JsonPropertyName("secondryDistance")]
    public int? SecondryDistance { get; set; }

    [JsonPropertyName("slashPenetration")]
    public int? SlashPenetration { get; set; }

    [JsonPropertyName("stabPenetration")]
    public int? StabPenetration { get; set; }

    [JsonPropertyName("primaryConsumption")]
    public int? PrimaryConsumption { get; set; }

    [JsonPropertyName("secondryConsumption")]
    public int? SecondryConsumption { get; set; }

    [JsonPropertyName("deflectionConsumption")]
    public int? DeflectionConsumption { get; set; }

    [JsonPropertyName("appliedTrunkRotation")]
    public XYZ? AppliedTrunkRotation { get; set; }

    [JsonPropertyName("appliedHeadRotation")]
    public XYZ? AppliedHeadRotation { get; set; }

    [JsonPropertyName("displayOnModel")]
    public bool? DisplayOnModel { get; set; }

    [JsonPropertyName("additionalAnimationLayer")]
    public int? AdditionalAnimationLayer { get; set; }

    [JsonPropertyName("staminaBurnRate")]
    public int? StaminaBurnRate { get; set; }

    [JsonPropertyName("colliderScaleMultiplier")]
    public XYZ? ColliderScaleMultiplier { get; set; }

    [JsonPropertyName("configPathStr")]
    public string? ConfigPathStr { get; set; }

    [JsonPropertyName("maxMarkersCount")]
    public int? MaxMarkersCount { get; set; }

    [JsonPropertyName("scaleMin")]
    public int? ScaleMin { get; set; }

    [JsonPropertyName("scaleMax")]
    public int? ScaleMax { get; set; }

    [JsonPropertyName("medUseTime")]
    public int? MedUseTime { get; set; }

    [JsonPropertyName("medEffectType")]
    public string? MedEffectType { get; set; }

    [JsonPropertyName("maxHpResource")]
    public int? MaxHpResource { get; set; }

    [JsonPropertyName("hpResourceRate")]
    public int? HpResourceRate { get; set; }

    [JsonPropertyName("apResource")]
    public int? ApResource { get; set; }

    [JsonPropertyName("krResource")]
    public int? KrResource { get; set; }

    [JsonPropertyName("maxOpticZoom")]
    public int? MaxOpticZoom { get; set; }

    [JsonPropertyName("maxRepairResource")]
    public int? MaxRepairResource { get; set; }

    [JsonPropertyName("targetItemFilter")]
    public List<string>? TargetItemFilter { get; set; }

    [JsonPropertyName("repairQuality")]
    public int? RepairQuality { get; set; }

    [JsonPropertyName("repairType")]
    public string? RepairType { get; set; }

    [JsonPropertyName("stackMinRandom")]
    public int? StackMinRandom { get; set; }

    [JsonPropertyName("stackMaxRandom")]
    public int? StackMaxRandom { get; set; }

    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    [JsonPropertyName("initialSpeed")]
    public int? InitialSpeed { get; set; }

    [JsonPropertyName("ballisticCoefficient")]
    public int? BallisticCoefficient { get; set; }

    [JsonPropertyName("bulletMassGram")]
    public int? BulletMassGram { get; set; }

    [JsonPropertyName("bulletDiameterMillimeters")]
    public int? BulletDiameterMillimeters { get; set; }

    [JsonPropertyName("damage")]
    public int? Damage { get; set; }

    [JsonPropertyName("ammoAccr")]
    public int? AmmoAccr { get; set; }

    [JsonPropertyName("ammoRec")]
    public int? AmmoRec { get; set; }

    [JsonPropertyName("ammoDist")]
    public int? AmmoDist { get; set; }

    [JsonPropertyName("buckshotBullets")]
    public int? BuckshotBullets { get; set; }

    [JsonPropertyName("penetrationPower")]
    public int? PenetrationPower { get; set; }

    [JsonPropertyName("penetrationPowerDeviation")]
    public int? PenetrationPowerDeviation { get; set; }

    [JsonPropertyName("ammoHear")]
    public int? AmmoHear { get; set; }

    [JsonPropertyName("ammoSfx")]
    public string? AmmoSfx { get; set; }

    [JsonPropertyName("misfireChance")]
    public int? MisfireChance { get; set; }

    [JsonPropertyName("minFragmentsCount")]
    public int? MinFragmentsCount { get; set; }

    [JsonPropertyName("maxFragmentsCount")]
    public int? MaxFragmentsCount { get; set; }

    [JsonPropertyName("ammoShiftChance")]
    public int? AmmoShiftChance { get; set; }

    [JsonPropertyName("casingName")]
    public string? CasingName { get; set; }

    [JsonPropertyName("casingEjectPower")]
    public int? CasingEjectPower { get; set; }

    [JsonPropertyName("casingMass")]
    public int? CasingMass { get; set; }

    [JsonPropertyName("casingSounds")]
    public string? CasingSounds { get; set; }

    [JsonPropertyName("projectileCount")]
    public int? ProjectileCount { get; set; }

    [JsonPropertyName("penetrationChanceObstacle")]
    public int? PenetrationChanceObstacle { get; set; }

    [JsonPropertyName("penetrationDamageMod")]
    public int? PenetrationDamageMod { get; set; }

    [JsonPropertyName("ricochetChance")]
    public int? RicochetChance { get; set; }

    [JsonPropertyName("fragmentationChance")]
    public int? FragmentationChance { get; set; }

    [JsonPropertyName("deterioration")]
    public int? Deterioration { get; set; }

    [JsonPropertyName("speedRetardation")]
    public int? SpeedRetardation { get; set; }

    [JsonPropertyName("tracer")]
    public bool? Tracer { get; set; }

    [JsonPropertyName("tracerColor")]
    public string? TracerColor { get; set; }

    [JsonPropertyName("tracerDistance")]
    public int? TracerDistance { get; set; }

    [JsonPropertyName("armorDamage")]
    public int? ArmorDamage { get; set; }

    [JsonPropertyName("caliber")]
    public string? Caliber { get; set; }

    [JsonPropertyName("staminaBurnPerDamage")]
    public int? StaminaBurnPerDamage { get; set; }

    [JsonPropertyName("heavyBleedingDelta")]
    public int? HeavyBleedingDelta { get; set; }

    [JsonPropertyName("lightBleedingDelta")]
    public int? LightBleedingDelta { get; set; }

    [JsonPropertyName("showBullet")]
    public bool? ShowBullet { get; set; }

    [JsonPropertyName("hasGrenaderComponent")]
    public bool? HasGrenaderComponent { get; set; }

    [JsonPropertyName("fuzeArmTimeSec")]
    public int? FuzeArmTimeSec { get; set; }

    [JsonPropertyName("explosionStrength")]
    public int? ExplosionStrength { get; set; }

    [JsonPropertyName("minExplosionDistance")]
    public int? MinExplosionDistance { get; set; }

    [JsonPropertyName("maxExplosionDistance")]
    public int? MaxExplosionDistance { get; set; }

    [JsonPropertyName("fragmentsCount")]
    public int? FragmentsCount { get; set; }

    [JsonPropertyName("fragmentType")]
    public string? FragmentType { get; set; }

    [JsonPropertyName("showHitEffectOnExplode")]
    public bool? ShowHitEffectOnExplode { get; set; }

    [JsonPropertyName("explosionType")]
    public string? ExplosionType { get; set; }

    [JsonPropertyName("ammoLifeTimeSec")]
    public int? AmmoLifeTimeSec { get; set; }

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
    public int? LightAndSoundShotAngle { get; set; }

    [JsonPropertyName("lightAndSoundShotSelfContusionTime")]
    public int? LightAndSoundShotSelfContusionTime { get; set; }

    [JsonPropertyName("lightAndSoundShotSelfContusionStrength")]
    public int? LightAndSoundShotSelfContusionStrength { get; set; }

    [JsonPropertyName("malfMisfireChance")]
    public int? MalfMisfireChance { get; set; }

    [JsonPropertyName("malfFeedChance")]
    public int? MalfFeedChance { get; set; }

    [JsonPropertyName("stackSlots")]
    public List<StackSlot>? StackSlots { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("eqMin")]
    public int? EqMin { get; set; }

    [JsonPropertyName("eqMax")]
    public int? EqMax { get; set; }

    [JsonPropertyName("rate")]
    public int? Rate { get; set; }

    [JsonPropertyName("throwType")]
    public string? ThrowType { get; set; }

    [JsonPropertyName("explDelay")]
    public int? ExplDelay { get; set; }

    [JsonPropertyName("strength")]
    public int? Strength { get; set; }

    [JsonPropertyName("contusionDistance")]
    public int? ContusionDistance { get; set; }

    [JsonPropertyName("throwDamMax")]
    public int? ThrowDamMax { get; set; }

    [JsonPropertyName("emitTime")]
    public int? EmitTime { get; set; }

    [JsonPropertyName("canBeHiddenDuringThrow")]
    public bool? CanBeHiddenDuringThrow { get; set; }

    [JsonPropertyName("canPlantOnGround")]
    public bool? CanPlantOnGround { get; set; }

    [JsonPropertyName("minTimeToContactExplode")]
    public int? MinTimeToContactExplode { get; set; }

    [JsonPropertyName("explosionEffectType")]
    public string? ExplosionEffectType { get; set; }

    [JsonPropertyName("linkedWeapon")]
    public string? LinkedWeapon { get; set; }

    [JsonPropertyName("useAmmoWithoutShell")]
    public bool? UseAmmoWithoutShell { get; set; }

    [JsonPropertyName("randomLootSettings")]
    public RandomLootSettings? RandomLootSettings { get; set; }

    [JsonPropertyName("recoilDampingHandRotation")]
    public int? RecoilDampingHandRotation { get; set; }

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
    public int? MaxUsages { get; set; }

    [JsonPropertyName("scavKillExpPenalty")]
    public int? ScavKillExpPenalty { get; set; }

    [JsonPropertyName("scavKillExpPenaltyPVE")]
    public int? ScavKillExpPenaltyPVE { get; set; }

    [JsonPropertyName("scavKillStandingPenalty")]
    public int? ScavKillStandingPenalty { get; set; }

    [JsonPropertyName("scavKillStandingPenaltyPVE")]
    public int? ScavKillStandingPenaltyPVE { get; set; }

    [JsonPropertyName("tradersDiscount")]
    public int? TradersDiscount { get; set; }

    [JsonPropertyName("tradersDiscountPVE")]
    public int? TradersDiscountPVE { get; set; }
}

public class WeaponRecoilSettings
{
    [JsonPropertyName("Enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("values")]
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
    public int? CellsH { get; set; }

    [JsonPropertyName("cellsV")]
    public int? CellsV { get; set; }

    [JsonPropertyName("minCount")]
    public int? MinCount { get; set; }

    [JsonPropertyName("maxCount")]
    public int? MaxCount { get; set; }

    [JsonPropertyName("maxWeight")]
    public int? MaxWeight { get; set; }

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
    public int? MaxCount { get; set; }

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
    public int? MaxStackCount { get; set; }
}

public class SlotFilter
{
    [JsonPropertyName("Shift")]
    public int? Shift { get; set; }

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
    public int? AnimationIndex { get; set; }
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
    public int? MaxCount { get; set; }

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
    public int? MaxCount { get; set; }

    [JsonPropertyName("minCount")]
    public int? MinCount { get; set; }
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
    public int? Value { get; set; }
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
    public int? Delay { get; set; }

    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    [JsonPropertyName("fadeOut")]
    public int? FadeOut { get; set; }

    [JsonPropertyName("cost")]
    public int? Cost { get; set; }

    [JsonPropertyName("healthPenaltyMin")]
    public int? HealthPenaltyMin { get; set; }

    [JsonPropertyName("healthPenaltyMax")]
    public int? HealthPenaltyMax { get; set; }
}

public class Color
{
    [JsonPropertyName("r")]
    public int? R { get; set; }

    [JsonPropertyName("g")]
    public int? G { get; set; }

    [JsonPropertyName("b")]
    public int? B { get; set; }

    [JsonPropertyName("a")]
    public int? A { get; set; }
}

public class ShotsGroupSettings
{
    [JsonPropertyName("EndShotIndex")]
    public int? EndShotIndex { get; set; }

    [JsonPropertyName("ShotRecoilPositionStrength")]
    public XYZ? ShotRecoilPositionStrength { get; set; }

    [JsonPropertyName("ShotRecoilRadianRange")]
    public XYZ? ShotRecoilRadianRange { get; set; }

    [JsonPropertyName("ShotRecoilRotationStrength")]
    public XYZ? ShotRecoilRotationStrength { get; set; }

    [JsonPropertyName("StartShotIndex")]
    public int? StartShotIndex { get; set; }
}

public enum ItemType
{
    NODE = 1,
    ITEM = 2
}