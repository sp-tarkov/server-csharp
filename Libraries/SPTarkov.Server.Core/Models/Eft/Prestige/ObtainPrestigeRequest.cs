using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Eft.Prestige;

public class ObtainPrestigeRequestList : List<ObtainPrestigeRequest>, IRequestData
{
}

public record ObtainPrestigeRequest : IRequestData
{
    [JsonPropertyName("id")]
    public string Id
    {
        get;
        set;
    }

    [JsonPropertyName("location")]
    public Location Location
    {
        get;
        set;
    }
}

public record Location
{
    [JsonPropertyName("x")]
    public int X
    {
        get;
        set;
    }

    [JsonPropertyName("y")]
    public int Y
    {
        get;
        set;
    }

    [JsonPropertyName("z")]
    public int Z
    {
        get;
        set;
    }

    [JsonPropertyName("r")]
    public string R
    {
        get;
        set;
    }
}
