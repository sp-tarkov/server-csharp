using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaticForcedProps
{
    [JsonPropertyName("containerId")]
    public string? ContainerId
    {
        get;
        set;
    }

    [JsonPropertyName("itemTpl")]
    public string? ItemTpl
    {
        get;
        set;
    }
}
