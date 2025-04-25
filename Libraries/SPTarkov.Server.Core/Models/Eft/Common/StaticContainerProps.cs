using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

[Obsolete("use SpawnpointTemplate")]
public record StaticContainerProps : StaticPropsBase
{
    [JsonPropertyName("Items")]
    public StaticItem[] Items
    {
        get;
        set;
    }
}
