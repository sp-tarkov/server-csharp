using Core.Annotations;
using Core.Models.Eft.Common.Tables;
using Props = Core.Models.Eft.Common.Props;

namespace Core.Helpers;

[Injectable]
public class RepairHelper
{
    /// <summary>
    /// Alter an items durability after a repair by trader/repair kit
    /// </summary>
    /// <param name="itemToRepair">item to update durability details</param>
    /// <param name="itemToRepairDetails">db details of item to repair</param>
    /// <param name="isArmor">Is item being repaired a piece of armor</param>
    /// <param name="amountToRepair">how many unit of durability to repair</param>
    /// <param name="useRepairKit">Is item being repaired with a repair kit</param>
    /// <param name="traderQualityMultipler">Trader quality value from traders base json</param>
    /// <param name="applyMaxDurabilityDegradation">should item have max durability reduced</param>
    public void UpdateItemDurability(
        Item itemToRepair,
        TemplateItem itemToRepairDetails,
        bool isArmor,
        int amountToRepair,
        bool useRepairKit,
        double traderQualityMultipler,
        bool applyMaxDurabilityDegradation = true
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Repairing armor reduces the total durability value slightly, get a randomised (to 2dp) amount based on armor material
    /// </summary>
    /// <param name="armorMaterial">What material is the armor being repaired made of</param>
    /// <param name="isRepairKit">Was a repair kit used</param>
    /// <param name="armorMax">Max amount of durability item can have</param>
    /// <param name="traderQualityMultipler">Different traders produce different loss values</param>
    /// <returns>Amount to reduce max durability by</returns>
    protected double GetRandomisedArmorRepairDegradationValue(
        string armorMaterial,
        bool isRepairKit,
        int armorMax,
        double traderQualityMultipler
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Repairing weapons reduces the total durability value slightly, get a randomised (to 2dp) amount
    /// </summary>
    /// <param name="itemProps">Weapon properties</param>
    /// <param name="isRepairKit">Was a repair kit used</param>
    /// <param name="weaponMax">Max amount of durability item can have</param>
    /// <param name="traderQualityMultipler">Different traders produce different loss values</param>
    /// <returns>Amount to reduce max durability by</returns>
    protected double GetRandomisedWeaponRepairDegradationValue(
        Props itemProps,
        bool isRepairKit,
        int weaponMax,
        double traderQualityMultipler
    )
    {
        throw new NotImplementedException();
    }
}
