using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record CovertMovement
{
    [JsonPropertyName("MovementAction")]
    public double? MovementAction
    {
        get;
        set;
    }
}
