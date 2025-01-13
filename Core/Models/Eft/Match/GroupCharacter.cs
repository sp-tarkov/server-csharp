using System.Text.Json.Serialization;
using Core.Models.Eft.Common.Tables;
using Core.Models.Enums;

namespace Core.Models.Eft.Match;

public class GroupCharacter
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

public class CharacterInfo
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

public class PlayerVisualRepresentation
{
    [JsonPropertyName("Info")]
    public VisualInfo? Info { get; set; }

    [JsonPropertyName("Customization")]
    public Customization? Customization { get; set; }

    [JsonPropertyName("Equipment")]
    public Equipment? Equipment { get; set; }
}

public class VisualInfo
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

public class Customization
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

public class Equipment
{
    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Items")]
    public List<Item>? Items { get; set; }
}
