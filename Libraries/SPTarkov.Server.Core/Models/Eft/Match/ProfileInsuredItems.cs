using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record ProfileInsuredItems
{
    [JsonPropertyName("insuredItems")]
    public List<InsuredItem>? InsuredItems
    {
        get;
        set;
    }
}
