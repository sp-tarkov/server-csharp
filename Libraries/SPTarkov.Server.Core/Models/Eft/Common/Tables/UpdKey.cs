using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdKey
{
    // Checked in client
    [JsonPropertyName("NumberOfUsages")]
    public int? NumberOfUsages
    {
        get;
        set;
    }
}
