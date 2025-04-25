using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record HandbookItem
{
    [JsonPropertyName("Id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("ParentId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? ParentId
    {
        get;
        set;
    }

    [JsonPropertyName("Price")]
    public double? Price
    {
        get;
        set;
    }
}
