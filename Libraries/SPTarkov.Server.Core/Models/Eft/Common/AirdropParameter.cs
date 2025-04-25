using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common;

public record AirdropParameter
{
    [JsonPropertyName("AirdropPointDeactivateDistance")]
    public int? AirdropPointDeactivateDistance
    {
        get;
        set;
    }

    [JsonPropertyName("MinPlayersCountToSpawnAirdrop")]
    public int? MinimumPlayersCountToSpawnAirdrop
    {
        get;
        set;
    }

    [JsonPropertyName("PlaneAirdropChance")]
    public double? PlaneAirdropChance
    {
        get;
        set;
    }

    [JsonPropertyName("PlaneAirdropCooldownMax")]
    public int? PlaneAirdropCooldownMax
    {
        get;
        set;
    }

    [JsonPropertyName("PlaneAirdropCooldownMin")]
    public int? PlaneAirdropCooldownMin
    {
        get;
        set;
    }

    [JsonPropertyName("PlaneAirdropEnd")]
    public int? PlaneAirdropEnd
    {
        get;
        set;
    }

    [JsonPropertyName("PlaneAirdropMax")]
    public int? PlaneAirdropMax
    {
        get;
        set;
    }

    [JsonPropertyName("PlaneAirdropStartMax")]
    public int? PlaneAirdropStartMax
    {
        get;
        set;
    }

    [JsonPropertyName("PlaneAirdropStartMin")]
    public int? PlaneAirdropStartMin
    {
        get;
        set;
    }

    [JsonPropertyName("UnsuccessfulTryPenalty")]
    public int? UnsuccessfulTryPenalty
    {
        get;
        set;
    }
}
