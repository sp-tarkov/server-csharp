using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdFoldable
{
    [JsonPropertyName("Folded")]
    public bool? Folded
    {
        get;
        set;
    }
}
