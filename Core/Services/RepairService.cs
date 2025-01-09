using System.Text.Json.Serialization;
using Core.Models.Eft.Common;
using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.ItemEvent;
using Core.Models.Eft.Repair;
using Core.Models.Enums;

namespace Core.Services;

public class RepairService
{
    /// <summary>
    /// Use trader to repair an items durability
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="pmcData">Profile to find item to repair in</param>
    /// <param name="repairItemDetails">Details of the item to repair</param>
    /// <param name="traderId">Trader being used to repair item</param>
    /// <returns>RepairDetails object</returns>
    public RepairDetails RepairItemByTrader(
        string sessionID,
        PmcData pmcData,
        RepairItem repairItemDetails,
        string traderId
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="pmcData">Profile to take money from</param>
    /// <param name="repairedItemId">Repaired item id</param>
    /// <param name="repairCost">Cost to repair item in roubles</param>
    /// <param name="traderId">Id of the trader who repaired the item / who is paid</param>
    /// <param name="output"></param>
    public void PayForRepair(
        string sessionID,
        PmcData pmcData,
        string repairedItemId,
        decimal repairCost,
        string traderId,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add skill points to profile after repairing an item
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="repairDetails">Details of item repaired, cost/item</param>
    /// <param name="pmcData">Profile to add points to</param>
    public void AddRepairSkillPoints(string sessionId, RepairDetails repairDetails, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    protected decimal GetIntellectGainedFromRepair(RepairDetails repairDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Return an approximation of the amount of skill points live would return for the given repairDetails
    /// </summary>
    /// <param name="repairDetails">The repair details to calculate skill points for</param>
    /// <returns>The number of skill points to reward the user</returns>
    protected decimal GetWeaponRepairSkillPoints(RepairDetails repairDetails)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="sessionId">Session id</param>
    /// <param name="pmcData">Profile to update repaired item in</param>
    /// <param name="repairKits">List of Repair kits to use</param>
    /// <param name="itemToRepairId">Item id to repair</param>
    /// <param name="output">ItemEventRouterResponse</param>
    /// <returns>Details of repair, item/price</returns>
    public RepairDetails RepairItemByKit(
        string sessionId,
        PmcData pmcData,
        List<RepairKitsInfo> repairKits,
        string itemToRepairId,
        ItemEventRouterResponse output
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Calculate value repairkit points need to be divided by to get the durability points to be added to an item
    /// </summary>
    /// <param name="itemToRepairDetails">Item to repair details</param>
    /// <param name="isArmor">Is the item being repaired armor</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>Number to divide kit points by</returns>
    protected decimal GetKitDivisor(TemplateItem itemToRepairDetails, bool isArmor, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the bonus multiplier for a skill from a player profile
    /// </summary>
    /// <param name="skillBonus">Bonus to get multiplier of</param>
    /// <param name="pmcData">Player profile to look in for skill</param>
    /// <returns>Multiplier value</returns>
    protected decimal GetBonusMultiplierValue(BonusType skillBonus, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Should a repair kit apply total durability loss on repair
    /// </summary>
    /// <param name="pmcData">Player profile</param>
    /// <param name="applyRandomizeDurabilityLoss">Value from repair config</param>
    /// <returns>True if loss should be applied</returns>
    protected bool ShouldRepairKitApplyDurabilityLoss(PmcData pmcData, bool applyRandomizeDurabilityLoss)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update repair kits Resource object if it doesn't exist
    /// </summary>
    /// <param name="repairKitDetails">Repair kit details from db</param>
    /// <param name="repairKitInInventory">Repair kit to update</param>
    protected void AddMaxResourceToKitIfMissing(TemplateItem repairKitDetails, Item repairKitInInventory)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Chance to apply buff to an item (Armor/weapon) if repaired by armor kit
    /// </summary>
    /// <param name="repairDetails">Repair details of item</param>
    /// <param name="pmcData">Player profile</param>
    public void AddBuffToItem(RepairDetails repairDetails, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Add random buff to item
    /// </summary>
    /// <param name="itemConfig">weapon/armor config</param>
    /// <param name="repairDetails">Details for item to repair</param>
    public void AddBuff(BonusSettings itemConfig, Item item)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Check if item should be buffed by checking the item type and relevant player skill level
    /// </summary>
    /// <param name="repairDetails">Item that was repaired</param>
    /// <param name="itemTpl">tpl of item to be buffed</param>
    /// <param name="pmcData">Player profile</param>
    /// <returns>True if item should have buff applied</returns>
    protected bool ShouldBuffItem(RepairDetails repairDetails, PmcData pmcData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Based on item, what underlying skill does this item use for buff settings
    /// </summary>
    /// <param name="itemTemplate">Item to check for skill</param>
    /// <returns>Skill name</returns>
    protected SkillTypes? GetItemSkillType(TemplateItem itemTemplate)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Ensure multiplier is between 1 and 0.01
    /// </summary>
    /// <param name="receiveDurabilityMaxPercent">Max durability percent</param>
    /// <param name="receiveDurabilityPercent">current durability percent</param>
    /// <returns>durability multiplier value</returns>
    protected double GetDurabilityMultiplier(double receiveDurabilityMaxPercent, double receiveDurabilityPercent)
    {
        throw new NotImplementedException();
    }
}

public class RepairDetails
{
    [JsonPropertyName("repairCost")]
    public double? RepairCost { get; set; }
    
    [JsonPropertyName("repairPoints")]
    public double? RepairPoints { get; set; }
    
    [JsonPropertyName("repairedItem")]
    public Item? RepairedItem { get; set; }
    
    [JsonPropertyName("repairedItemIsArmor")]
    public bool? RepairedItemIsArmor { get; set; }
    
    [JsonPropertyName("repairAmount")]
    public double? RepairAmount { get; set; }
    
    [JsonPropertyName("repairedByKit")]
    public bool? RepairedByKit { get; set; }
}
