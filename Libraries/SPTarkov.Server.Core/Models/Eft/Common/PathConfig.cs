using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record PathConfig
{
    [JsonPropertyName("active")]
    public bool? Active
    {
        get;
        set;
    }

    [JsonPropertyName("id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("enterPoint")]
    public string? EnterPoint
    {
        get;
        set;
    }

    [JsonPropertyName("exitPoint")]
    public string? ExitPoint
    {
        get;
        set;
    }

    [JsonPropertyName("pathPoints")]
    public List<string>? PathPoints
    {
        get;
        set;
    }

    [JsonPropertyName("once")]
    public bool? Once
    {
        get;
        set;
    }

    [JsonPropertyName("circle")]
    public bool? Circle
    {
        get;
        set;
    }

    [JsonPropertyName("circleCount")]
    public double? CircleCount
    {
        get;
        set;
    }

    [JsonPropertyName("skinType")]
    public List<string>? SkinType
    {
        get;
        set;
    }
}
