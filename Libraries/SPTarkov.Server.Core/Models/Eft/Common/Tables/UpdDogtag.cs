using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Common.Tables;

public record UpdDogtag
{
    [JsonPropertyName("AccountId")]
    public string? AccountId
    {
        get;
        set;
    }

    [JsonPropertyName("ProfileId")]
    public string? ProfileId
    {
        get;
        set;
    }

    [JsonPropertyName("Nickname")]
    public string? Nickname
    {
        get;
        set;
    }

    [JsonPropertyName("Side")]
    public object? Side
    {
        get;
        set;
    }

    [JsonPropertyName("Level")]
    public double? Level
    {
        get;
        set;
    }

    [JsonPropertyName("Time")]
    public string? Time
    {
        get;
        set;
    }

    [JsonPropertyName("Status")]
    public string? Status
    {
        get;
        set;
    }

    [JsonPropertyName("KillerAccountId")]
    public string? KillerAccountId
    {
        get;
        set;
    }

    [JsonPropertyName("KillerProfileId")]
    public string? KillerProfileId
    {
        get;
        set;
    }

    [JsonPropertyName("KillerName")]
    public string? KillerName
    {
        get;
        set;
    }

    [JsonPropertyName("WeaponName")]
    public string? WeaponName
    {
        get;
        set;
    }

    public bool? CarriedByGroupMember
    {
        get;
        set;
    }
}
