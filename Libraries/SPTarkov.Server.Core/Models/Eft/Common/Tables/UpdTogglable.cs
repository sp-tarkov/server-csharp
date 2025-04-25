using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdTogglable
{
    [JsonPropertyName("On")]
    public bool? On
    {
        get;
        set;
    }
}
