using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.PresetBuild;

public record RemoveBuildRequestData : IRequestData
{
    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }
}
