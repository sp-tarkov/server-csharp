using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LocationEvents
{
    [JsonPropertyName("Halloween2024")]
    public Halloween2024? Halloween2024
    {
        get;
        set;
    }

    public Khorovod? Khorovod
    {
        get;
        set;
    }
}
