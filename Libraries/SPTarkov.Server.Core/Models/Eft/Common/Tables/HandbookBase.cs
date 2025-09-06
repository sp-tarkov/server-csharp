using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record HandbookBase
{
    [JsonPropertyName("Categories")]
    public required List<HandbookCategory> Categories { get; init; }

    [JsonPropertyName("Items")]
    public required List<HandbookItem> Items { get; init; }
}

public record HandbookCategory
{
    [JsonPropertyName("Id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("ParentId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public MongoId? ParentId { get; set; }

    [JsonPropertyName("Icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string Icon { get; set; }

    [JsonPropertyName("Color")]
    public string Color { get; set; }

    [JsonPropertyName("Order")]
    public string Order { get; set; }
}

public record HandbookItem
{
    [JsonPropertyName("Id")]
    public MongoId Id { get; set; }

    [JsonPropertyName("ParentId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public MongoId ParentId { get; set; }

    [JsonPropertyName("Price")]
    public double? Price { get; set; }
}
