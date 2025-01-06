using Core.Models.Eft.Common.Tables;
using Core.Models.Eft.Ragfair;
using Core.Models.Enums;

namespace Core.Models.Eft.ItemEvent;

using System.Text.Json.Serialization;

public class ItemEventRouterBase
{
    [JsonPropertyName("warnings")]
    public List<Warning> Warnings { get; set; }
    
    [JsonPropertyName("profileChanges")]
    public object ProfileChanges { get; set; } // Note: using object to accommodate string or TProfileChanges
}

public class TProfileChanges : Dictionary<string, ProfileChange> { }

public class Warning
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    
    [JsonPropertyName("errmsg")]
    public string ErrorMessage { get; set; }
    
    [JsonPropertyName("code")]
    public string Code { get; set; }
    
    [JsonPropertyName("data")]
    public object Data { get; set; }
}

public class ProfileChange
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    
    [JsonPropertyName("experience")]
    public int Experience { get; set; }
    
    [JsonPropertyName("quests")]
    public List<Quest> Quests { get; set; }
    
    [JsonPropertyName("ragFairOffers")]
    public List<RagfairOffer> RagFairOffers { get; set; }
    
    [JsonPropertyName("weaponBuilds")]
    public List<WeaponBuildChange> WeaponBuilds { get; set; }
    
    [JsonPropertyName("equipmentBuilds")]
    public List<EquipmentBuildChange> EquipmentBuilds { get; set; }
    
    [JsonPropertyName("items")]
    public ItemChanges Items { get; set; }
    
    [JsonPropertyName("production")]
    public Dictionary<string, Productive> Production { get; set; }
    
    /** Hideout area improvement id */
    [JsonPropertyName("improvements")]
    public Dictionary<string, HideoutImprovement> Improvements { get; set; }
    
    [JsonPropertyName("skills")]
    public Skills Skills { get; set; }
    
    [JsonPropertyName("health")]
    public Common.Health Health { get; set; }
    
    [JsonPropertyName("traderRelations")]
    public Dictionary<string, TraderData> TraderRelations { get; set; }
    
    [JsonPropertyName("moneyTransferLimitData")]
    public MoneyTransferLimits MoneyTransferLimitData { get; set; }
    
    [JsonPropertyName("repeatableQuests")]
    public List<PmcDataRepeatableQuest> RepeatableQuests { get; set; }
    
    [JsonPropertyName("recipeUnlocked")]
    public Dictionary<string, bool> RecipeUnlocked { get; set; }
    
    [JsonPropertyName("changedHideoutStashes")]
    public Dictionary<string, HideoutStashItem> ChangedHideoutStashes { get; set; }
    
    [JsonPropertyName("questsStatus")]
    public List<QuestStatus> QuestsStatus { get; set; }
}

public class HideoutStashItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("tpl")]
    public string Template { get; set; }
}

public class WeaponBuildChange
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("root")]
    public string Root { get; set; }
    
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }
}

public class EquipmentBuildChange
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("root")]
    public string Root { get; set; }
    
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("fastpanel")]
    public List<object> FastPanel { get; set; }
    
    [JsonPropertyName("buildType")]
    public EquipmentBuildType BuildType { get; set; }
}

public class ItemChanges
{
    [JsonPropertyName("new")]
    public List<Product> NewItems { get; set; }
    
    [JsonPropertyName("change")]
    public List<Product> ChangedItems { get; set; }
    
    [JsonPropertyName("del")]
    public List<Product> DeletedItems { get; set; } // Only needs _id property
}

/** Related to TraderInfo */
public class TraderData
{
    [JsonPropertyName("salesSum")]
    public double SalesSum { get; set; }
    
    [JsonPropertyName("standing")]
    public double Standing { get; set; }
    
    [JsonPropertyName("loyalty")]
    public double Loyalty { get; set; }
    
    [JsonPropertyName("unlocked")]
    public bool Unlocked { get; set; }
    
    [JsonPropertyName("disabled")]
    public bool Disabled { get; set; }
}

public class Product
{
    [JsonPropertyName("_id")]
    public string Id { get; set; }
    
    [JsonPropertyName("_tpl")]
    public string Template { get; set; }
    
    [JsonPropertyName("parentId")]
    public string ParentId { get; set; }
    
    [JsonPropertyName("slotId")]
    public string SlotId { get; set; }
    
    [JsonPropertyName("location")]
    public ItemLocation Location { get; set; }
    
    [JsonPropertyName("upd")]
    public Upd Upd { get; set; }
}