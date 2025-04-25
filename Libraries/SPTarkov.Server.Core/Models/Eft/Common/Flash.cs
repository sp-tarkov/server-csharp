using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Flash
{
    [JsonPropertyName("Dummy")]
    public double? Dummy
    {
        get;
        set;
    }
}
