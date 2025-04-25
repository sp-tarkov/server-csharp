using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Handbook
{
    [JsonPropertyName("defaultCategory")]
    public string? DefaultCategory
    {
        get;
        set;
    }
}
