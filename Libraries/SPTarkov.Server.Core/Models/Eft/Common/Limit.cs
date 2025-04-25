using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record Limit : MinMax<int>
{
    [JsonPropertyName("items")]
    public object[] Items
    {
        get;
        set;
    } // TODO: was on TS any[] hmmm..

    [JsonPropertyName("min")]
    public int? Min
    {
        get;
        set;
    }

    [JsonPropertyName("max")]
    public int? Max
    {
        get;
        set;
    }
}
