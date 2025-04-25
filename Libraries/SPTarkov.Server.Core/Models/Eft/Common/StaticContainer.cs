using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record StaticContainer
{
    [JsonPropertyName("containersGroups")]
    public Dictionary<string, ContainerMinMax>? ContainersGroups
    {
        get;
        set;
    }

    [JsonPropertyName("containers")]
    public Dictionary<string, ContainerData>? Containers
    {
        get;
        set;
    }
}
