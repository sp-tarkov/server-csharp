using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record LocationInfection
{
    [JsonPropertyName("Interchange")]
    public double? Interchange
    {
        get;
        set;
    }

    [JsonPropertyName("Lighthouse")]
    public double? Lighthouse
    {
        get;
        set;
    }

    [JsonPropertyName("RezervBase")]
    public double? RezervBase
    {
        get;
        set;
    }

    [JsonPropertyName("Sandbox")]
    public double? Sandbox
    {
        get;
        set;
    }

    [JsonPropertyName("Shoreline")]
    public double? Shoreline
    {
        get;
        set;
    }

    [JsonPropertyName("TarkovStreets")]
    public double? TarkovStreets
    {
        get;
        set;
    }

    [JsonPropertyName("Woods")]
    public double? Woods
    {
        get;
        set;
    }

    [JsonPropertyName("bigmap")]
    public double? BigMap
    {
        get;
        set;
    }

    [JsonPropertyName("factory4")]
    public double? Factory4
    {
        get;
        set;
    }

    [JsonPropertyName("laboratory")]
    public double? Laboratory
    {
        get;
        set;
    }
}
