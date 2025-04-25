using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record ClientEvents
{
    [JsonPropertyName("MatchingCompleted")]
    public double? MatchingCompleted
    {
        get;
        set;
    }

    [JsonPropertyName("MatchingCompletedReal")]
    public double? MatchingCompletedReal
    {
        get;
        set;
    }

    [JsonPropertyName("LocationLoaded")]
    public double? LocationLoaded
    {
        get;
        set;
    }

    [JsonPropertyName("LocationLoadedReal")]
    public double? LocationLoadedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GamePrepared")]
    public double? GamePrepared
    {
        get;
        set;
    }

    [JsonPropertyName("GamePreparedReal")]
    public double? GamePreparedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameCreated")]
    public double? GameCreated
    {
        get;
        set;
    }

    [JsonPropertyName("GameCreatedReal")]
    public double? GameCreatedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GamePooled")]
    public double? GamePooled
    {
        get;
        set;
    }

    [JsonPropertyName("GamePooledReal")]
    public double? GamePooledReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameRunned")]
    public double? GameRunned
    {
        get;
        set;
    }

    [JsonPropertyName("GameRunnedReal")]
    public double? GameRunnedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawn")]
    public double? GameSpawn
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawnReal")]
    public double? GameSpawnReal
    {
        get;
        set;
    }

    [JsonPropertyName("PlayerSpawnEvent")]
    public double? PlayerSpawnEvent
    {
        get;
        set;
    }

    [JsonPropertyName("PlayerSpawnEventReal")]
    public double? PlayerSpawnEventReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawned")]
    public double? GameSpawned
    {
        get;
        set;
    }

    [JsonPropertyName("GameSpawnedReal")]
    public double? GameSpawnedReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameStarting")]
    public double? GameStarting
    {
        get;
        set;
    }

    [JsonPropertyName("GameStartingReal")]
    public double? GameStartingReal
    {
        get;
        set;
    }

    [JsonPropertyName("GameStarted")]
    public double? GameStarted
    {
        get;
        set;
    }

    [JsonPropertyName("GameStartedReal")]
    public double? GameStartedReal
    {
        get;
        set;
    }
}
