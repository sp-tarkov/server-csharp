using System.Text.Json.Serialization;

namespace Core.Models.Spt.Mod;

public class NewItemDetails : NewItemDetailsBase
{
    [JsonPropertyName("newItem")]
    public TemplateItem NewItem { get; set; }
}

public class NewItemFromCloneDetails : NewItemDetailsBase
{
    [JsonPropertyName("itemTplToClone")]
    public string ItemTplToClone { get; set; }
    
    [JsonPropertyName("overrideProperties")]
    public Props OverrideProperties { get; set; }
    
    [JsonPropertyName("parentId")]
    public string ParentId { get; set; }

    [JsonPropertyName("newId")] 
    public string NewId { get; set; } = "";
}

public class NewItemDetailsBase
{
    [JsonPropertyName("fleaPriceRoubles")]
    public float FleaPriceRoubles { get; set; }
    
    [JsonPropertyName("handbookPriceRoubles")]
    public float HandbookPriceRoubles { get; set; }
    
    [JsonPropertyName("handbookParentId")]
    public string HandbookParentId { get; set; }
    
    [JsonPropertyName("locales")]
    public Dictionary<string, LocaleDetails> Locales { get; set; }
}

public class LocaleDetails
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("shortName")]
    public string ShortName { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
}

public class CreateItemResult
{
    [JsonPropertyName("success")] 
    public bool Success { get; set; }
    
    [JsonPropertyName("itemId")]
    public string ItemId { get; set; }
    
    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; }

    public CreateItemResult()
    {
        Success = false;
        Errors = new List<string>();
    }
}

// TODO: This needs to be reworked with however we do it for this project