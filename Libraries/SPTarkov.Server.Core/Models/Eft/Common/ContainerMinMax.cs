using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record ContainerMinMax
{
    [JsonPropertyName("minContainers")]
    public int? MinContainers
    {
        get;
        set;
    }

    [JsonPropertyName("maxContainers")]
    public int? MaxContainers
    {
        get;
        set;
    }

    [JsonPropertyName("current")]
    public int? Current
    {
        get;
        set;
    }

    [JsonPropertyName("chosenCount")]
    public int? ChosenCount
    {
        get;
        set;
    }
}
