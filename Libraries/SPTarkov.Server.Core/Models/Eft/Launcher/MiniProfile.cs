using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Models.Eft.Launcher;

public record MiniProfile
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("side")]
    public string? Side { get; set; }

    [JsonPropertyName("currlvl")]
    public int? CurrentLevel { get; set; }

    [JsonPropertyName("currexp")]
    public int? CurrentExperience { get; set; }

    [JsonPropertyName("prevexp")]
    public int? PreviousExperience { get; set; }

    [JsonPropertyName("nextlvl")]
    public int? NextLevel { get; set; }

    [JsonPropertyName("maxlvl")]
    public int? MaxLevel { get; set; }

    [JsonPropertyName("edition")]
    public string? Edition { get; set; }

    [JsonPropertyName("profileId")]
    public string? ProfileId { get; set; }

    [JsonPropertyName("invalidOrUnloadableProfile")]
    public bool? InvalidOrUnloadableProfile { get; set; }

    [JsonPropertyName("sptData")]
    public Profile.Spt? SptData { get; set; }

    [JsonPropertyName("wipe")]
    public bool Wipe { get; set; }

    [JsonPropertyName("profileCurrency")]
    public ProfileCurrency? ProfileCurrency { get; set; }

    [JsonPropertyName("profileStats")]
    public ProfileStats? ProfileStats { get; set; }
}

public record ProfileStats
{
    [JsonPropertyName("overall")]
    public required Dictionary<string, string> Overall { get; set; }

    [JsonPropertyName("pmc")]
    public required Dictionary<string, string> Pmc { get; set; }

    [JsonPropertyName("scav")]
    public required Dictionary<string, string> Scav { get; set; }
}

public record ProfileCurrency
{
    [JsonPropertyName("roubles")]
    public required int Roubles { get; set; }

    [JsonPropertyName("euros")]
    public required int Euros { get; set; }

    [JsonPropertyName("dollars")]
    public required int Dollars { get; set; }

    [JsonPropertyName("gp")]
    public required int GP { get; set; }
}
