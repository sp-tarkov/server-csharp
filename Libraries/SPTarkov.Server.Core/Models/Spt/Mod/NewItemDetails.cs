using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public record NewItemDetails : NewItemDetailsBase
{
    [JsonPropertyName("newItem")]
    public required TemplateItem NewItem { get; set; }
}

public record NewItemFromCloneDetails : NewItemDetailsBase
{
    /// <summary>
    ///     Id of the item to copy and use as a base
    /// </summary>
    [JsonPropertyName("itemTplToClone")]
    public required MongoId ItemTplToClone { get; set; }

    /// <summary>
    ///     Item properties that should be applied over the top of the cloned base
    /// </summary>
    [JsonPropertyName("overrideProperties")]
    public TemplateItemProperties? OverrideProperties { get; set; }

    /// <summary>
    ///     ParentId for the new item (item type)
    /// </summary>
    [JsonPropertyName("parentId")]
    public required MongoId ParentId { get; set; }

    /// <summary>
    ///     The id the new item should have. This is often known as the TplId, or TemplateId
    /// </summary>
    [JsonPropertyName("newId")]
    public required MongoId NewId { get; set; }

    /// <summary>
    /// The new name to assign the item, this is typically something like weapon_colt_m4a1_556x45
    /// </summary>
    [JsonPropertyName("newItemName")]
    public required string NewItemName { get; set; }
}

public record NewItemDetailsBase
{
    [JsonPropertyName("fleaPriceRoubles")]
    public double? FleaPriceRoubles { get; set; }

    [JsonPropertyName("handbookPriceRoubles")]
    public double? HandbookPriceRoubles { get; set; }

    [JsonPropertyName("handbookParentId")]
    public string? HandbookParentId { get; set; }

    /// <summary>
    /// Whether the item should be added to Tarkov's handbook
    /// </summary>
    [JsonPropertyName("addToHandbook")]
    public bool AddToHandbook { get; set; } = true;

    /// <summary>
    /// Whether the item should be added to the flea price database
    /// </summary>
    [JsonPropertyName("addToFleaPriceDb")]
    public bool AddToFleaPriceDb { get; set; } = true;

    /// <summary>
    /// Whether to add the new item to the hideout's weapon shelf whitelist
    /// </summary>
    [JsonPropertyName("addToWeaponShelf")]
    public bool AddToWeaponShelf { get; set; } = true;

    [JsonPropertyName("locales")]
    public required Dictionary<string, LocaleDetails> Locales { get; set; } = [];
}

public record LocaleDetails
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("shortName")]
    public string? ShortName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

public record CreateItemResult
{
    public CreateItemResult()
    {
        Success = false;
        Errors = [];
    }

    [JsonPropertyName("success")]
    public required bool Success { get; set; }

    [JsonPropertyName("itemId")]
    public required MongoId ItemId { get; set; }

    [JsonPropertyName("errors")]
    public required List<string> Errors { get; set; } = [];
}
