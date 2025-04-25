using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Armor
{
    [JsonPropertyName("class")]
    public List<Class>? Classes
    {
        get;
        set;
    }
}
