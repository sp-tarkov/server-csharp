using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record HandbookCategory
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

    [JsonPropertyName("Icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Icon
    {
        get;
        set;
    }

    [JsonPropertyName("Color")]
    public string? Color
    {
        get;
        set;
    }

    [JsonPropertyName("Order")]
    public string? Order
    {
        get;
        set;
    }
}
