using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace SPTarkov.Server.Core.Models.Eft.Match;

public record GroupCharacter
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("aid")]
    public int? Aid { get; set; }

    [JsonPropertyName("Info")]
    public CharacterInfo? Info { get; set; }

    [JsonPropertyName("PlayerVisualRepresentation")]
    public PlayerVisualRepresentation? VisualRepresentation { get; set; }

    [JsonPropertyName("isLeader")]
    public bool? IsLeader { get; set; }

    [JsonPropertyName("isReady")]
    public bool? IsReady { get; set; }

    [JsonPropertyName("region")]
    public string? Region { get; set; }

    [JsonPropertyName("lookingGroup")]
    public bool? LookingGroup { get; set; }
}

public record CharacterInfo
{
    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public int? Level { get; set; }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory { get; set; }

    [JsonPropertyName("GameVersion")]
    public string? GameVersion { get; set; }

    [JsonPropertyName("SavageLockTime")]
    public double? SavageLockTime { get; set; }

    [JsonPropertyName("SavageNickname")]
    public string? SavageNickname { get; set; }

    [JsonPropertyName("hasCoopExtension")]
    public bool? HasCoopExtension { get; set; }
}

public record PlayerVisualRepresentation
{
    [JsonPropertyName("Info")]
    public VisualInfo? Info { get; set; }

    [JsonPropertyName("Customization")]
    public Customization? Customization { get; set; }

    [JsonPropertyName("Equipment")]
    public Equipment? Equipment { get; set; }
}

public record VisualInfo
{
    [JsonPropertyName("Side")]
    public string? Side { get; set; }

    [JsonPropertyName("Level")]
    public int? Level { get; set; }

    [JsonPropertyName("Nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("MemberCategory")]
    public MemberCategory? MemberCategory { get; set; }

    [JsonPropertyName("GameVersion")]
    public string? GameVersion { get; set; }
}

public record Customization
{
    [JsonPropertyName("Head")]
    public string? Head { get; set; }

    [JsonPropertyName("Body")]
    public string? Body { get; set; }

    [JsonPropertyName("Feet")]
    public string? Feet { get; set; }

    [JsonPropertyName("Hands")]
    public string? Hands { get; set; }
}

public record Equipment
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Items")]
    public List<Item>? Items { get; set; }
}
