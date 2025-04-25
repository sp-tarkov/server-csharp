using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record EventWeather
{
    [JsonPropertyName("Cloudness")]
    public double? Cloudness
    {
        get;
        set;
    }

    [JsonPropertyName("Hour")]
    public double? Hour
    {
        get;
        set;
    }

    [JsonPropertyName("Minute")]
    public double? Minute
    {
        get;
        set;
    }

    [JsonPropertyName("Rain")]
    public double? Rain
    {
        get;
        set;
    }

    [JsonPropertyName("RainRandomness")]
    public double? RainRandomness
    {
        get;
        set;
    }

    [JsonPropertyName("ScaterringFogDensity")]
    public double? ScaterringFogDensity
    {
        get;
        set;
    }

    [JsonPropertyName("TopWindDirection")]
    public XYZ? TopWindDirection
    {
        get;
        set;
    }

    [JsonPropertyName("Wind")]
    public double? Wind
    {
        get;
        set;
    }

    [JsonPropertyName("WindDirection")]
    public double? WindDirection
    {
        get;
        set;
    }
}
