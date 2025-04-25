using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdLight
{
    [JsonPropertyName("IsActive")]
    public bool? IsActive
    {
        get;
        set;
    }

    [JsonPropertyName("SelectedMode")]
    public int? SelectedMode
    {
        get;
        set;
    }
}
