using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Contusion
{
    [JsonPropertyName("Dummy")]
    public double? Dummy
    {
        get;
        set;
    }
}
