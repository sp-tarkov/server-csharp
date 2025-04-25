using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ContainerData
{
    [JsonPropertyName("groupId")]
    public string? GroupId
    {
        get;
        set;
    }
}
