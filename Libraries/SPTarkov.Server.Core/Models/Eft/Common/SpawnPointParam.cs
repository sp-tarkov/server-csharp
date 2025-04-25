using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record SpawnPointParam
{
    [JsonPropertyName("BotZoneName")]
    public string? BotZoneName
    {
        get;
        set;
    }

    [JsonPropertyName("Categories")]
    public List<string>? Categories
    {
        get;
        set;
    }

    [JsonPropertyName("ColliderParams")]
    public ColliderParams? ColliderParams
    {
        get;
        set;
    }

    [JsonPropertyName("CorePointId")]
    public int? CorePointId
    {
        get;
        set;
    }

    [JsonPropertyName("DelayToCanSpawnSec")]
    public double? DelayToCanSpawnSec
    {
        get;
        set;
    }

    [JsonPropertyName("Id")]
    public string? Id
    {
        get;
        set;
    }

    [JsonPropertyName("Infiltration")]
    public string? Infiltration
    {
        get;
        set;
    }

    [JsonPropertyName("Position")]
    public XYZ? Position
    {
        get;
        set;
    }

    [JsonPropertyName("Rotation")]
    public double? Rotation
    {
        get;
        set;
    }

    [JsonPropertyName("Sides")]
    public List<string>? Sides
    {
        get;
        set;
    }
}
