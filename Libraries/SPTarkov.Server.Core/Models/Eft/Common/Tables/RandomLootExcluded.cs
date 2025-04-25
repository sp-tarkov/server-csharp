using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record RandomLootExcluded
{
    [JsonPropertyName("categoryTemplates")]
    public List<object>? CategoryTemplates
    {
        get;
        set;
    } // TODO: object here

    [JsonPropertyName("rarity")]
    public List<string>? Rarity
    {
        get;
        set;
    }

    [JsonPropertyName("templates")]
    public List<object>? Templates
    {
        get;
        set;
    } // TODO: object here
}
