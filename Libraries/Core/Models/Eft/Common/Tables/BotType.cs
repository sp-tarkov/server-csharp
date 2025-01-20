using System.Text.Json.Serialization;
using Core.Models.Common;
using Core.Models.Enums;
using Core.Utils.Json.Converters;
using SptCommon.Extensions;

namespace Core.Models.Eft.Common.Tables;

public record BotType
{
    [JsonPropertyName("appearance")]
    public Appearance? BotAppearance { get; set; }

    [JsonPropertyName("chances")]
    public Chances? BotChances { get; set; }

    [JsonPropertyName("difficulty")]
    public Dictionary<string, DifficultyCategories>? BotDifficulty { get; set; }

    [JsonPropertyName("experience")]
    public Experience? BotExperience { get; set; }

    [JsonPropertyName("firstName")]
    public List<string>? FirstNames { get; set; }

    [JsonPropertyName("generation")]
    public Generation? BotGeneration { get; set; }

    [JsonPropertyName("health")]
    public BotTypeHealth? BotHealth { get; set; }

    [JsonPropertyName("inventory")]
    public BotTypeInventory? BotInventory { get; set; }

    [JsonPropertyName("lastName")]
    public List<string>? LastNames { get; set; }

    [JsonPropertyName("skills")]
    public BotDbSkills? BotSkills { get; set; }
}

public record Appearance
{
    [JsonPropertyName("body")]
    public Dictionary<string, double>? Body { get; set; }

    [JsonPropertyName("feet")]
    public Dictionary<string, double>? Feet { get; set; }

    [JsonPropertyName("hands")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Hands { get; set; }

    [JsonPropertyName("head")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Head { get; set; }

    [JsonPropertyName("voice")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Voice { get; set; }

    public Dictionary<string, double> this[string propName]
    {
        get
        {
            var matchingProp = GetType()
                .GetProperties()
                .SingleOrDefault(p => p.GetJsonName() == propName)
                ?.GetValue(this);

            return (Dictionary<string, double>)matchingProp;
        }
    }
}

public record Chances
{
    [JsonPropertyName("equipment")]
    public Dictionary<string, double>? EquipmentChances { get; set; }

    [JsonPropertyName("weaponMods")]
    public Dictionary<string, double>? WeaponModsChances { get; set; }

    [JsonPropertyName("equipmentMods")]
    public Dictionary<string, double>? EquipmentModsChances { get; set; }

    [JsonPropertyName("mods")]
    public Dictionary<string, double>? Mods { get; set; }
}

/* record removed in favor of Dictionary<string, double>
 used to be used in:
 Chances.WeaponModsChances
 Chances.EquipmentModsChances
 GenerateWeaponRequest.ModSpawnChances
public record ModsChances
{
    [JsonPropertyName("mod_charge")]
public double? ModCharge { get; set; }

    [JsonPropertyName("mod_bipod")]
public double? ModBipod { get; set; }

    [JsonPropertyName("mod_barrel")]
public double? ModBarrel { get; set; }

    [JsonPropertyName("mod_catch")]
public double? ModCatch { get; set; }

    [JsonPropertyName("mod_gas_block")]
public double? ModGasBlock { get; set; }

    [JsonPropertyName("mod_hammer")]
public double? ModHammer { get; set; }

    [JsonPropertyName("mod_equipment")]
public double? ModEquipment { get; set; }

    [JsonPropertyName("mod_equipment_000")]
public double? ModEquipment000 { get; set; }

    [JsonPropertyName("mod_equipment_001")]
public double? ModEquipment001 { get; set; }

    [JsonPropertyName("mod_equipment_002")]
public double? ModEquipment002 { get; set; }

    [JsonPropertyName("mod_flashlight")]
public double? ModFlashlight { get; set; }

    [JsonPropertyName("mod_foregrip")]
public double? ModForegrip { get; set; }

    [JsonPropertyName("mod_launcher")]
public double? ModLauncher { get; set; }

    [JsonPropertyName("mod_magazine")]
public double? ModMagazine { get; set; }

    [JsonPropertyName("mod_mount")]
public double? ModMount { get; set; }

    [JsonPropertyName("mod_mount_000")]
public double? ModMount000 { get; set; }

    [JsonPropertyName("mod_mount_001")]
public double? ModMount001 { get; set; }

    [JsonPropertyName("mod_muzzle")]
public double? ModMuzzle { get; set; }

    [JsonPropertyName("mod_nvg")]
public double? ModNvg { get; set; }

    [JsonPropertyName("mod_pistol_grip")]
public double? ModPistolGrip { get; set; }

    [JsonPropertyName("mod_reciever")]
public double? ModReceiver { get; set; }

    [JsonPropertyName("mod_scope")]
public double? ModScope { get; set; }

    [JsonPropertyName("mod_scope_000")]
public double? ModScope000 { get; set; }

    [JsonPropertyName("mod_scope_001")]
public double? ModScope001 { get; set; }

    [JsonPropertyName("mod_scope_002")]
public double? ModScope002 { get; set; }

    [JsonPropertyName("mod_scope_003")]
public double? ModScope003 { get; set; }

    [JsonPropertyName("mod_sight_front")]
public double? ModSightFront { get; set; }

    [JsonPropertyName("mod_sight_rear")]
public double? ModSightRear { get; set; }

    [JsonPropertyName("mod_stock")]
public double? ModStock { get; set; }

    [JsonPropertyName("mod_stock_000")]
public double? ModStock000 { get; set; }

    [JsonPropertyName("mod_stock_002")]
public double? ModStock002 { get; set; }

    [JsonPropertyName("mod_stock_akms")]
public double? ModStockAkms { get; set; }

    [JsonPropertyName("mod_tactical")]
public double? ModTactical { get; set; }

    [JsonPropertyName("mod_tactical_000")]
public double? ModTactical000 { get; set; }

    [JsonPropertyName("mod_tactical_001")]
public double? ModTactical001 { get; set; }

    [JsonPropertyName("mod_tactical_002")]
public double? ModTactical002 { get; set; }

    [JsonPropertyName("mod_tactical_2")]
public double? ModTactical2 { get; set; }

    [JsonPropertyName("mod_tactical_003")]
public double? ModTactical003 { get; set; }

    [JsonPropertyName("mod_handguard")]
public double? ModHandguard { get; set; }

    [JsonPropertyName("back_plate")]
public double? BackPlate { get; set; }

    [JsonPropertyName("front_plate")]
public double? FrontPlate { get; set; }

    [JsonPropertyName("left_side_plate")]
public double? LeftSidePlate { get; set; }

    [JsonPropertyName("right_side_plate")]
public double? RightSidePlate { get; set; }

    [JsonPropertyName("mod_mount_002")]
public double? ModMount002 { get; set; }

    [JsonPropertyName("mod_mount_003")]
public double? ModMount003 { get; set; }

    [JsonPropertyName("mod_mount_004")]
public double? ModMount004 { get; set; }

    [JsonPropertyName("mod_mount_005")]
public double? ModMount005 { get; set; }

    [JsonPropertyName("mod_mount_006")]
public double? ModMount006 { get; set; }

    [JsonPropertyName("mod_muzzle_000")]
public double? ModMuzzle000 { get; set; }

    [JsonPropertyName("mod_muzzle_001")]
public double? ModMuzzle001 { get; set; }

    [JsonPropertyName("mod_pistol_grip_akms")]
public double? ModPistolGripAkms { get; set; }

    [JsonPropertyName("mod_pistolgrip")]
public double? ModPistol_Grip { get; set; }
}
*/

public record Difficulties
{
    [JsonPropertyName("easy")]
    public DifficultyCategories? Easy { get; set; }

    [JsonPropertyName("normal")]
    public DifficultyCategories? Normal { get; set; }

    [JsonPropertyName("hard")]
    public DifficultyCategories? Hard { get; set; }

    [JsonPropertyName("impossible")]
    public DifficultyCategories? Impossible { get; set; }
}

public record DifficultyCategories
{
    public Dictionary<string, object>? Aiming { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Boss { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Change { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Core { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Cover { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Grenade { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Hearing { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Lay { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Look { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Mind { get; set; } // TODO: string | number | boolean | string[]
    public Dictionary<string, object>? Move { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Patrol { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Scattering { get; set; } // TODO: string | number | boolean
    public Dictionary<string, object>? Shoot { get; set; } // TODO: string | number | boolean
}

public record Experience
{
    /** key = bot difficulty */
    [JsonPropertyName("aggressorBonus")]
    public Dictionary<string, double>? AggressorBonus { get; set; }

    [JsonPropertyName("level")]
    public MinMax? Level { get; set; }

    /** key = bot difficulty */
    [JsonPropertyName("reward")]
    public Dictionary<string, MinMax>? Reward { get; set; }

    /** key = bot difficulty */
    [JsonPropertyName("standingForKill")]
    public Dictionary<string, double>? StandingForKill { get; set; }

    [JsonPropertyName("useSimpleAnimator")]
    public bool? UseSimpleAnimator { get; set; }
}

public record Generation
{
    [JsonPropertyName("items")]
    public GenerationWeightingItems? Items { get; set; }
}

public record GenerationData
{
    /** key: number of items, value: weighting */
    [JsonPropertyName("weights")]
    public Dictionary<double, double>? Weights { get; set; }

    /** Array of item tpls */
    [JsonPropertyName("whitelist")]
    [JsonConverter(typeof(ArrayToObjectFactoryConverter))]
    public Dictionary<string, double>? Whitelist { get; set; }
}

public record GenerationWeightingItems
{
    [JsonPropertyName("grenades")]
    public GenerationData Grenades { get; set; }
    
    [JsonPropertyName("healing")]
    public GenerationData Healing { get; set; }
    
    [JsonPropertyName("drugs")]
    public GenerationData Drugs { get; set; }
    
    [JsonPropertyName("food")]
    public GenerationData Food { get; set; }
    
    [JsonPropertyName("drink")]
    public GenerationData Drink { get; set; }
    
    [JsonPropertyName("currency")]
    public GenerationData Currency { get; set; }
    
    [JsonPropertyName("stims")]
    public GenerationData Stims { get; set; }
    
    [JsonPropertyName("backpackLoot")]
    public GenerationData BackpackLoot { get; set; }
    
    [JsonPropertyName("pocketLoot")]
    public GenerationData PocketLoot { get; set; }
    
    [JsonPropertyName("vestLoot")]
    public GenerationData VestLoot { get; set; }
    
    [JsonPropertyName("magazines")]
    public GenerationData Magazines { get; set; }
    
    [JsonPropertyName("specialItems")]
    public GenerationData SpecialItems { get; set; }
    
    [JsonPropertyName("looseLoot")]
    public GenerationData LooseLoot { get; set; }
}

public record BotTypeHealth
{
    public List<BodyPart>? BodyParts { get; set; }
    public MinMax? Energy { get; set; }
    public MinMax? Hydration { get; set; }
    public MinMax? Temperature { get; set; }
}

public record BodyPart
{
    public MinMax? Chest { get; set; }
    public MinMax? Head { get; set; }
    public MinMax? LeftArm { get; set; }
    public MinMax? LeftLeg { get; set; }
    public MinMax? RightArm { get; set; }
    public MinMax? RightLeg { get; set; }
    public MinMax? Stomach { get; set; }
}

public record BotTypeInventory
{
    [JsonPropertyName("equipment")]
    public Dictionary<EquipmentSlots, Dictionary<string, double>>? Equipment { get; set; }

    public GlobalAmmo? Ammo { get; set; }

    [JsonPropertyName("items")]
    public ItemPools? Items { get; set; }

    [JsonPropertyName("mods")]
    public GlobalMods? Mods { get; set; }
}

public record Equipment
{
    public Dictionary<string, double>? ArmBand { get; set; }
    public Dictionary<string, double>? ArmorVest { get; set; }
    public Dictionary<string, double>? Backpack { get; set; }
    public Dictionary<string, double>? Earpiece { get; set; }
    public Dictionary<string, double>? Eyewear { get; set; }
    public Dictionary<string, double>? FaceCover { get; set; }
    public Dictionary<string, double>? FirstPrimaryWeapon { get; set; }
    public Dictionary<string, double>? Headwear { get; set; }
    public Dictionary<string, double>? Holster { get; set; }
    public Dictionary<string, double>? Pockets { get; set; }
    public Dictionary<string, double>? Scabbard { get; set; }
    public Dictionary<string, double>? SecondPrimaryWeapon { get; set; }
    public Dictionary<string, double>? SecuredContainer { get; set; }
    public Dictionary<string, double>? TacticalVest { get; set; }
}

public record ItemPools
{
    public Dictionary<string, double>? Backpack { get; set; }
    public Dictionary<string, double>? Pockets { get; set; }
    public Dictionary<string, double>? SecuredContainer { get; set; }
    public Dictionary<string, double>? SpecialLoot { get; set; }
    public Dictionary<string, double>? TacticalVest { get; set; }
}

public record BotDbSkills
{
    public Dictionary<string, MinMax>? Common { get; set; }

    public Dictionary<string, MinMax>? Mastering { get; set; }
}
