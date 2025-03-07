using System.Text.Json.Serialization;
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
    public string? ItemTplToClone
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
    public string? ParentId
    {
        get;
        set;
    }

    /// <summary>
    /// the id the new item should have, leave blank to have one generated for you.
    /// This is often known as the TplId, or TemplateId
    /// </summary>
    [JsonPropertyName("newId")]
    public string? NewId
    {
        get;
        set;
    } = "";
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
    public string? HandbookParentId
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
    public string? ItemId
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
