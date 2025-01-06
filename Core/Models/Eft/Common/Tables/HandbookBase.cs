using System.Text.Json.Serialization;

namespace Core.Models.Eft.Common.Tables;

public class HandbookBase
{
    [JsonPropertyName("Categories")]
    public List<HandbookCategory> Categories { get; set; }
    
    [JsonPropertyName("Items")]
    public List<HandbookItem> Items { get; set; }
}

public class HandbookCategory
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }
    
    [JsonPropertyName("ParentId")]
    public string? ParentId { get; set; }
    
    [JsonPropertyName("Icon")]
    public string Icon { get; set; }
    
    [JsonPropertyName("Color")]
    public string Color { get; set; }
    
    [JsonPropertyName("Order")]
    public string Order { get; set; }
}

public class HandbookItem
{
    [JsonPropertyName("Id")]
    public string Id { get; set; }
    
    [JsonPropertyName("ParentId")]
    public string ParentId { get; set; }
    
    [JsonPropertyName("Price")]
    public decimal Price { get; set; }
}