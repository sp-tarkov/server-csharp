using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record GraphicSettings
{
    [JsonPropertyName("ExperimentalFogInCity")]
    public bool? ExperimentalFogInCity
    {
        get;
        set;
    }
}
