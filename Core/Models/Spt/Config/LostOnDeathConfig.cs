using System.Text.Json.Serialization;

namespace Core.Models.Spt.Config;

public class LostOnDeathConfig : BaseConfig
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = "spt-lostondeath";

    /** What equipment in each slot should be lost on death */
    [JsonPropertyName("equipment")]
    public LostEquipment Equipment { get; set; }

    /** Should special slot items be removed from quest inventory on death e.g. wifi camera/markers */
    [JsonPropertyName("specialSlotItems")]
    public bool SpecialSlotItems { get; set; }

    /** Should quest items be removed from quest inventory on death */
    [JsonPropertyName("questItems")]
    public bool QuestItems { get; set; }
}

public class LostEquipment
{
    [JsonPropertyName("ArmBand")]
    public bool ArmBand { get; set; }

    [JsonPropertyName("Headwear")]
    public bool Headwear { get; set; }

    [JsonPropertyName("Earpiece")]
    public bool Earpiece { get; set; }

    [JsonPropertyName("FaceCover")]
    public bool FaceCover { get; set; }

    [JsonPropertyName("ArmorVest")]
    public bool ArmorVest { get; set; }

    [JsonPropertyName("Eyewear")]
    public bool Eyewear { get; set; }

    [JsonPropertyName("TacticalVest")]
    public bool TacticalVest { get; set; }

    [JsonPropertyName("PocketItems")]
    public bool PocketItems { get; set; }

    [JsonPropertyName("Backpack")]
    public bool Backpack { get; set; }

    [JsonPropertyName("Holster")]
    public bool Holster { get; set; }

    [JsonPropertyName("FirstPrimaryWeapon")]
    public bool FirstPrimaryWeapon { get; set; }

    [JsonPropertyName("SecondPrimaryWeapon")]
    public bool SecondPrimaryWeapon { get; set; }

    [JsonPropertyName("Scabbard")]
    public bool Scabbard { get; set; }
}