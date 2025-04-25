using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record HandbookBase
{
    [JsonPropertyName("Categories")]
    public List<HandbookCategory>? Categories
    {
        get;
        set;
    }

    [JsonPropertyName("Items")]
    public List<HandbookItem>? Items
    {
        get;
        set;
    }
}
