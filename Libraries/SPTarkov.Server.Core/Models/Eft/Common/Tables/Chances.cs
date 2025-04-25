using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Chances
{
    [JsonPropertyName("equipment")]
    public Dictionary<string, double>? EquipmentChances
    {
        get;
        set;
    }

    [JsonPropertyName("weaponMods")]
    public Dictionary<string, double>? WeaponModsChances
    {
        get;
        set;
    }

    [JsonPropertyName("equipmentMods")]
    public Dictionary<string, double>? EquipmentModsChances
    {
        get;
        set;
    }

    [JsonPropertyName("mods")]
    public Dictionary<string, double>? Mods
    {
        get;
        set;
    }
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
