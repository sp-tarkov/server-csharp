using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdFireMode
{
    [JsonPropertyName("FireMode")]
    public string? FireMode
    {
        get;
        set;
    }
}
