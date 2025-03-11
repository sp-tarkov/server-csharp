using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Spt.Mod;

public record NewItemDetails : NewItemDetailsBase
{
    [JsonPropertyName("newItem")]
    public TemplateItem? NewItem
    {
        get;
        set;
    }
}

public record NewItemFromCloneDetails : NewItemDetailsBase
{
    /// <summary>
    /// Id of the item to copy and use as a base
    /// </summary>
    [JsonPropertyName("itemTplToClone")]
    public MongoId ItemTplToClone
    {
        get;
        set;
    }

    /// <summary>
    /// Item properties that should be applied over the top of the cloned base
    /// </summary>
    [JsonPropertyName("overrideProperties")]
    public Props? OverrideProperties
    {
        get;
        set;
    }

    /// <summary>
    /// ParentId for the new item (item type)
    /// </summary>
    [JsonPropertyName("parentId")]
    public MongoId ParentId
    {
        get;
        set;
    }

    /// <summary>
    /// The new items template id or _tpl
    /// NOTE: This should never be auto generated. That's how you corrupt profiles.
    /// </summary>
    [JsonPropertyName("newId")]
    public MongoId NewId
    {
        get;
        set;
    }
}

public record NewItemDetailsBase
{
    [JsonPropertyName("fleaPriceRoubles")]
    public double? FleaPriceRoubles
    {
        get;
        set;
    }

    [JsonPropertyName("handbookPriceRoubles")]
    public double? HandbookPriceRoubles
    {
        get;
        set;
    }

    [JsonPropertyName("handbookParentId")]
    public MongoId? HandbookParentId
    {
        get;
        set;
    }

    [JsonPropertyName("locales")]
    public Dictionary<string, LocaleDetails>? Locales
    {
        get;
        set;
    }
}

public record LocaleDetails
{
    [JsonPropertyName("name")]
    public string? Name
    {
        get;
        set;
    }

    [JsonPropertyName("shortName")]
    public string? ShortName
    {
        get;
        set;
    }

    [JsonPropertyName("description")]
    public string? Description
    {
        get;
        set;
    }
}

public record CreateItemResult
{
    public CreateItemResult()
    {
        Success = false;
        Errors = new List<string>();
    }

    [JsonPropertyName("success")]
    public bool? Success
    {
        get;
        set;
    }

    [JsonPropertyName("itemId")]
    public MongoId? ItemId
    {
        get;
        set;
    }

    [JsonPropertyName("errors")]
    public List<string>? Errors
    {
        get;
        set;
    }
}

// TODO: This needs to be reworked with however we do it for this project
