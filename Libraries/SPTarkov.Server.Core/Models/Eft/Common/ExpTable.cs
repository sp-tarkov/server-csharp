using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ExpTable
{
    [JsonPropertyName("exp")]
    public int? Experience
    {
        get;
        set;
    }
}
