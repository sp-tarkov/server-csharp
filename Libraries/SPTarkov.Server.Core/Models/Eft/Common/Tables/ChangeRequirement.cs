using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record ChangeRequirement
{
    [JsonPropertyName("changeCost")]
    public List<ChangeCost?>? ChangeCost
    {
        get;
        set;
    }

    [JsonPropertyName("changeStandingCost")]
    public double? ChangeStandingCost
    {
        get;
        set;
    }
}
