using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Prestige;

public record GetPrestigeResponse
{
    [JsonPropertyName("elements")]
    public List<Common.Tables.Prestige>? Elements
    {
        get;
        set;
    }
}
