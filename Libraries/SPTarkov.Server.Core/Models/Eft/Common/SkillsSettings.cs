using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SkillsSettings
{
    [JsonPropertyName("SkillProgressRate")]
    public double? SkillProgressRate
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

    [JsonPropertyName("WeaponSkillRecoilBonusPerLevel")]
    public double? WeaponSkillRecoilBonusPerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("HideoutManagement")]
    public HideoutManagement? HideoutManagement
    {
        get;
        set;
    }

    [JsonPropertyName("Crafting")]
    public Crafting? Crafting
    {
        get;
        set;
    }

    [JsonPropertyName("Metabolism")]
    public Metabolism? Metabolism
    {
        get;
        set;
    }

    [JsonPropertyName("MountingErgonomicsBonusPerLevel")]
    public double? MountingErgonomicsBonusPerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("Immunity")]
    public Immunity? Immunity
    {
        get;
        set;
    }

    [JsonPropertyName("Endurance")]
    public Endurance? Endurance
    {
        get;
        set;
    }

    [JsonPropertyName("Strength")]
    public Strength? Strength
    {
        get;
        set;
    }

    [JsonPropertyName("Vitality")]
    public Vitality? Vitality
    {
        get;
        set;
    }

    [JsonPropertyName("Health")]
    public HealthSkillProgress? Health
    {
        get;
        set;
    }

    [JsonPropertyName("StressResistance")]
    public StressResistance? StressResistance
    {
        get;
        set;
    }

    [JsonPropertyName("Throwing")]
    public Throwing? Throwing
    {
        get;
        set;
    }

    [JsonPropertyName("RecoilControl")]
    public RecoilControl? RecoilControl
    {
        get;
        set;
    }

    [JsonPropertyName("Pistol")]
    public WeaponSkills? Pistol
    {
        get;
        set;
    }

    [JsonPropertyName("Revolver")]
    public WeaponSkills? Revolver
    {
        get;
        set;
    }

    [JsonPropertyName("SMG")]
    public List<object>? SMG
    {
        get;
        set;
    }

    [JsonPropertyName("Assault")]
    public WeaponSkills? Assault
    {
        get;
        set;
    }

    [JsonPropertyName("Shotgun")]
    public WeaponSkills? Shotgun
    {
        get;
        set;
    }

    [JsonPropertyName("Sniper")]
    public WeaponSkills? Sniper
    {
        get;
        set;
    }

    [JsonPropertyName("LMG")]
    public List<object>? LMG
    {
        get;
        set;
    }

    [JsonPropertyName("HMG")]
    public List<object>? HMG
    {
        get;
        set;
    }

    [JsonPropertyName("Launcher")]
    public List<object>? Launcher
    {
        get;
        set;
    }

    [JsonPropertyName("AttachedLauncher")]
    public List<object>? AttachedLauncher
    {
        get;
        set;
    }

    [JsonPropertyName("Melee")]
    public MeleeSkill? Melee
    {
        get;
        set;
    }

    [JsonPropertyName("DMR")]
    public WeaponSkills? DMR
    {
        get;
        set;
    }

    [JsonPropertyName("BearAssaultoperations")]
    public List<object>? BearAssaultoperations
    {
        get;
        set;
    }

    [JsonPropertyName("BearAuthority")]
    public List<object>? BearAuthority
    {
        get;
        set;
    }

    [JsonPropertyName("BearAksystems")]
    public List<object>? BearAksystems
    {
        get;
        set;
    }

    [JsonPropertyName("BearHeavycaliber")]
    public List<object>? BearHeavycaliber
    {
        get;
        set;
    }

    [JsonPropertyName("BearRawpower")]
    public List<object>? BearRawpower
    {
        get;
        set;
    }

    [JsonPropertyName("BipodErgonomicsBonusPerLevel")]
    public double? BipodErgonomicsBonusPerLevel
    {
        get;
        set;
    }

    [JsonPropertyName("UsecArsystems")]
    public List<object>? UsecArsystems
    {
        get;
        set;
    }

    [JsonPropertyName("UsecDeepweaponmodding_Settings")]
    public List<object>? UsecDeepweaponmodding_Settings
    {
        get;
        set;
    }

    [JsonPropertyName("UsecLongrangeoptics_Settings")]
    public List<object>? UsecLongrangeoptics_Settings
    {
        get;
        set;
    }

    [JsonPropertyName("UsecNegotiations")]
    public List<object>? UsecNegotiations
    {
        get;
        set;
    }

    [JsonPropertyName("UsecTactics")]
    public List<object>? UsecTactics
    {
        get;
        set;
    }

    [JsonPropertyName("BotReload")]
    public List<object>? BotReload
    {
        get;
        set;
    }

    [JsonPropertyName("CovertMovement")]
    public CovertMovement? CovertMovement
    {
        get;
        set;
    }

    [JsonPropertyName("FieldMedicine")]
    public List<object>? FieldMedicine
    {
        get;
        set;
    }

    [JsonPropertyName("Search")]
    public Search? Search
    {
        get;
        set;
    }

    [JsonPropertyName("Sniping")]
    public List<object>? Sniping
    {
        get;
        set;
    }

    [JsonPropertyName("ProneMovement")]
    public List<object>? ProneMovement
    {
        get;
        set;
    }

    [JsonPropertyName("FirstAid")]
    public List<object>? FirstAid
    {
        get;
        set;
    }

    [JsonPropertyName("LightVests")]
    public ArmorSkills? LightVests
    {
        get;
        set;
    }

    [JsonPropertyName("HeavyVests")]
    public ArmorSkills? HeavyVests
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponModding")]
    public List<object>? WeaponModding
    {
        get;
        set;
    }

    [JsonPropertyName("AdvancedModding")]
    public List<object>? AdvancedModding
    {
        get;
        set;
    }

    [JsonPropertyName("NightOps")]
    public List<object>? NightOps
    {
        get;
        set;
    }

    [JsonPropertyName("SilentOps")]
    public List<object>? SilentOps
    {
        get;
        set;
    }

    [JsonPropertyName("Lockpicking")]
    public List<object>? Lockpicking
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponTreatment")]
    public WeaponTreatment? WeaponTreatment
    {
        get;
        set;
    }

    [JsonPropertyName("MagDrills")]
    public MagDrills? MagDrills
    {
        get;
        set;
    }

    [JsonPropertyName("Freetrading")]
    public List<object>? Freetrading
    {
        get;
        set;
    }

    [JsonPropertyName("Auctions")]
    public List<object>? Auctions
    {
        get;
        set;
    }

    [JsonPropertyName("Cleanoperations")]
    public List<object>? Cleanoperations
    {
        get;
        set;
    }

    [JsonPropertyName("Barter")]
    public List<object>? Barter
    {
        get;
        set;
    }

    [JsonPropertyName("Shadowconnections")]
    public List<object>? Shadowconnections
    {
        get;
        set;
    }

    [JsonPropertyName("Taskperformance")]
    public List<object>? Taskperformance
    {
        get;
        set;
    }

    [JsonPropertyName("Perception")]
    public Perception? Perception
    {
        get;
        set;
    }

    [JsonPropertyName("Intellect")]
    public Intellect? Intellect
    {
        get;
        set;
    }

    [JsonPropertyName("Attention")]
    public Attention? Attention
    {
        get;
        set;
    }

    [JsonPropertyName("Charisma")]
    public Charisma? Charisma
    {
        get;
        set;
    }

    [JsonPropertyName("Memory")]
    public Memory? Memory
    {
        get;
        set;
    }

    [JsonPropertyName("Surgery")]
    public Surgery? Surgery
    {
        get;
        set;
    }

    [JsonPropertyName("AimDrills")]
    public AimDrills? AimDrills
    {
        get;
        set;
    }

    [JsonPropertyName("BotSound")]
    public List<object>? BotSound
    {
        get;
        set;
    }

    [JsonPropertyName("TroubleShooting")]
    public TroubleShooting? TroubleShooting
    {
        get;
        set;
    }
}
