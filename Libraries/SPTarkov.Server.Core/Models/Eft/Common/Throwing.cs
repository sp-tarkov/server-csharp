using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Throwing
{
    [JsonPropertyName("ThrowAction")]
    public double? ThrowAction
    {
        get;
        set;
    }
}
