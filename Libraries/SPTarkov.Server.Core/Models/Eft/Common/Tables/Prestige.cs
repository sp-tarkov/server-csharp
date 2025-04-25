using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record Prestige
{
    [JsonPropertyName("elements")]
    public List<PrestigeElement>? Elements
    {
        get;
        set;
    }
}
